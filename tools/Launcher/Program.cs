using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.Win32;

namespace NinnyTrainer.Launcher;

internal sealed class Program
{
    private static int Main(string[] args)
    {
        var options = DeploymentOptions.Parse(args);
        var logger = new ConsoleLogger(options.Verbose);

        LauncherPresentation.ShowBanner();

        logger.Info("Looking for project root...");

        var projectRoot = options.ProjectRoot ?? PathFinder.FindProjectRoot();
        if (string.IsNullOrWhiteSpace(projectRoot))
        {
            logger.Error("Unable to locate the project root (tried to find NinnyTrainer.csproj upwards from the launcher).");
            return 1;
        }

        logger.Info($"Project root: {projectRoot}");

        var buildConfiguration = options.BuildConfiguration ?? "Release";
        var buildOutput = Path.Combine(projectRoot, "bin", buildConfiguration);
        if (!Directory.Exists(buildOutput))
        {
            logger.Error($"Build output not found at {buildOutput}. Build the trainer first (dotnet build -c {buildConfiguration}).");
            return 1;
        }

        logger.Info($"Searching for GTA V Story Mode...");
        var storyModePath = options.GamePath ?? StoryModeLocator.ResolveStoryModePath(logger);
        if (string.IsNullOrWhiteSpace(storyModePath))
        {
            logger.Error("Unable to locate a GTA V Story Mode installation. Use --game-path to point at the folder that contains GTA5.exe.");
            return 1;
        }

        logger.Info($"Story Mode folder: {storyModePath}");

        BattleyeAdvisor.Report(logger);

        var copier = new DeploymentCopier(projectRoot, buildOutput, storyModePath, logger, options.DryRun);
        var result = copier.Copy();

        LauncherPresentation.ShowSummary(result, options.DryRun, logger);
        return 0;
    }
}

internal sealed record DeploymentOptions(string? GamePath, string? BuildConfiguration, bool DryRun, bool Verbose, string? ProjectRoot)
{
    public static DeploymentOptions Parse(string[] args)
    {
        string? gamePath = null;
        string? buildConfiguration = null;
        string? projectRoot = null;
        var dryRun = false;
        var verbose = false;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--game-path":
                case "-g":
                    gamePath = NextArg(args, ref i, "game path");
                    break;
                case "--build-config":
                case "-c":
                    buildConfiguration = NextArg(args, ref i, "build configuration");
                    break;
                case "--project-root":
                case "-p":
                    projectRoot = NextArg(args, ref i, "project root");
                    break;
                case "--dry-run":
                    dryRun = true;
                    break;
                case "--verbose":
                    verbose = true;
                    break;
                case "--help":
                case "-h":
                    PrintHelp();
                    Environment.Exit(0);
                    break;
                default:
                    throw new ArgumentException($"Unknown argument: {args[i]}");
            }
        }

        return new DeploymentOptions(gamePath, buildConfiguration, dryRun, verbose, projectRoot);
    }

    private static string NextArg(IReadOnlyList<string> args, ref int index, string name)
    {
        if (index + 1 >= args.Count)
        {
            throw new ArgumentException($"Missing value for {name} argument.");
        }

        index++;
        return args[index];
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Ninny Trainer Launcher");
        Console.WriteLine("Copies trainer DLLs and plugins into a detected GTA V Story Mode install.");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -g, --game-path <path>       Override Story Mode directory (folder containing GTA5.exe)");
        Console.WriteLine("  -c, --build-config <name>    Build configuration to pull from bin/<name> (default Release)");
        Console.WriteLine("  -p, --project-root <path>    Override project root (auto-detected by default)");
        Console.WriteLine("      --dry-run                Simulate copy operations without writing files");
        Console.WriteLine("      --verbose                Print detailed detection and copy steps");
        Console.WriteLine("  -h, --help                   Show this help");
    }
}

internal sealed class DeploymentCopier
{
    private readonly string _projectRoot;
    private readonly string _buildOutput;
    private readonly string _storyModePath;
    private readonly ILogger _logger;
    private readonly bool _dryRun;

    public DeploymentCopier(string projectRoot, string buildOutput, string storyModePath, ILogger logger, bool dryRun)
    {
        _projectRoot = projectRoot;
        _buildOutput = buildOutput;
        _storyModePath = storyModePath;
        _logger = logger;
        _dryRun = dryRun;
    }

    public DeploymentResult Copy()
    {
        var scriptsFolder = Path.Combine(_storyModePath, "scripts");
        var trainerFolder = Path.Combine(scriptsFolder, "NinnyTrainer");
        var pluginsFolder = Path.Combine(trainerFolder, "Plugins");

        if (!_dryRun)
        {
            Directory.CreateDirectory(scriptsFolder);
            Directory.CreateDirectory(trainerFolder);
            Directory.CreateDirectory(pluginsFolder);
        }

        var trainerDllsCopied = CopyDlls(_buildOutput, trainerFolder, "trainer dlls");
        var pluginDllsCopied = CopyPlugins(pluginsFolder);
        var configsCopied = CopyConfigs(trainerFolder);

        return new DeploymentResult(trainerDllsCopied, pluginDllsCopied, configsCopied);
    }

    private int CopyDlls(string sourceFolder, string destinationFolder, string label)
    {
        if (!Directory.Exists(sourceFolder))
        {
            _logger.Warn($"Skipped copying {label} because source folder is missing: {sourceFolder}");
            return 0;
        }

        var dlls = Directory.EnumerateFiles(sourceFolder, "*.dll", SearchOption.TopDirectoryOnly).ToImmutableArray();
        foreach (var dll in dlls)
        {
            var dest = Path.Combine(destinationFolder, Path.GetFileName(dll));
            LogCopy(dll, dest);
            if (!_dryRun)
            {
                File.Copy(dll, dest, overwrite: true);
            }
        }

        return dlls.Length;
    }

    private int CopyPlugins(string pluginsFolder)
    {
        var localPlugins = Path.Combine(_projectRoot, "Plugins");
        if (!Directory.Exists(localPlugins))
        {
            _logger.Info("No local plugins folder found. Skipping plugin copy.");
            return 0;
        }

        return CopyDlls(localPlugins, pluginsFolder, "plugins");
    }

    private int CopyConfigs(string trainerFolder)
    {
        var copied = 0;
        var configPath = Path.Combine(_projectRoot, "TrainerConfig.ini");
        if (File.Exists(configPath))
        {
            var dest = Path.Combine(trainerFolder, Path.GetFileName(configPath));
            LogCopy(configPath, dest);
            if (!_dryRun)
            {
                File.Copy(configPath, dest, overwrite: true);
            }
            copied++;
        }

        return copied;
    }

    private void LogCopy(string source, string destination)
    {
        _logger.Info($"Copying {Path.GetFileName(source)} -> {destination}");
        if (_dryRun)
        {
            _logger.Info("(dry run)");
        }
    }
}

internal sealed record DeploymentResult(int TrainerDllsCopied, int PluginDllsCopied, int ConfigsCopied);

internal static class PathFinder
{
    public static string? FindProjectRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null)
        {
            var csproj = Path.Combine(current.FullName, "NinnyTrainer.csproj");
            if (File.Exists(csproj))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return null;
    }
}

internal static class StoryModeLocator
{
    public static string? ResolveStoryModePath(ILogger logger)
    {
        foreach (var path in EnumerateCandidates(logger))
        {
            if (!Directory.Exists(path))
            {
                continue;
            }

            if (IsStoryModeInstall(path, logger))
            {
                return Path.GetFullPath(path);
            }
        }

        return null;
    }

    private static IEnumerable<string> EnumerateCandidates(ILogger logger)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void Add(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            var normalized = Path.GetFullPath(path);
            if (seen.Add(normalized))
            {
                logger.Verbose($"Candidate path: {normalized}");
            }
        }

        Add(RegistryLocator.GetSteamPath());
        Add(RegistryLocator.GetRockstarPath());
        foreach (var epicPath in EpicLocator.GetEpicInstallPaths())
        {
            Add(epicPath);
        }

        var defaults = new[]
        {
            @"C:\\Program Files (x86)\\Steam\\steamapps\\common\\Grand Theft Auto V",
            @"C:\\Program Files\\Rockstar Games\\Grand Theft Auto V",
            @"C:\\Program Files\\Rockstar Games\\Grand Theft Auto V (Legacy)",
            @"C:\\Program Files\\Rockstar Games\\Grand Theft Auto V (Enhanced)",
            @"D:\\Games\\Grand Theft Auto V",
            @"D:\\Games\\Grand Theft Auto V (Legacy)",
            @"D:\\Games\\Grand Theft Auto V (Enhanced)",
            @"C:\\Program Files\\Epic Games\\GTAV"
        };

        foreach (var path in defaults)
        {
            Add(path);
        }

        return seen;
    }

    private static bool IsStoryModeInstall(string root, ILogger logger)
    {
        var executables = new[] { "GTA5.exe", "PlayGTAV.exe", "GTA5_en.exe" };
        var hasExecutable = executables.Any(exe => File.Exists(Path.Combine(root, exe)));
        if (!hasExecutable)
        {
            logger.Verbose($"Missing Story Mode executables in {root}");
            return false;
        }

        var disallowed = Path.Combine(root, "FiveM.exe");
        if (File.Exists(disallowed))
        {
            logger.Verbose($"Skipping FiveM/online-heavy path {root}");
            return false;
        }

        return true;
    }
}

internal static class RegistryLocator
{
    public static string? GetSteamPath()
    {
        return GetString(new[]
        {
            @"HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Valve\\Steam",
            @"HKEY_CURRENT_USER\\SOFTWARE\\Valve\\Steam"
        }, "InstallPath");
    }

    public static string? GetRockstarPath()
    {
        return GetString(new[]
        {
            @"HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Rockstar Games\\Grand Theft Auto V",
            @"HKEY_CURRENT_USER\\SOFTWARE\\Rockstar Games\\Grand Theft Auto V"
        }, "InstallFolder");
    }

    private static string? GetString(IEnumerable<string> registryKeys, string valueName)
    {
        foreach (var key in registryKeys)
        {
            try
            {
                var value = Registry.GetValue(key, valueName, null)?.ToString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        return null;
    }
}

internal static class EpicLocator
{
    public static IEnumerable<string> GetEpicInstallPaths()
    {
        var results = new List<string>();
        var manifestDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Epic", "EpicGamesLauncher", "Data", "Manifests");
        if (Directory.Exists(manifestDir))
        {
            foreach (var manifest in Directory.EnumerateFiles(manifestDir, "*.item"))
            {
                try
                {
                    var json = File.ReadAllText(manifest);
                    var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("InstallLocation", out var install) && install.ValueKind == JsonValueKind.String)
                    {
                        var path = install.GetString();
                        if (!string.IsNullOrWhiteSpace(path))
                        {
                            results.Add(path);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        return results;
    }
}

internal interface ILogger
{
    void Info(string message);
    void Warn(string message);
    void Error(string message);
    void Verbose(string message);
}

internal sealed class ConsoleLogger(bool verbose) : ILogger
{
    private readonly bool _verbose = verbose;

    public void Info(string message) => Write("INFO", message, ConsoleTheme.Accent);

    public void Warn(string message) => Write("WARN", message, ConsoleColor.Yellow);

    public void Error(string message) => Write("ERROR", message, ConsoleColor.Red);

    public void Verbose(string message)
    {
        if (_verbose)
        {
            Write("VERBOSE", message, ConsoleTheme.Muted);
        }
    }

    private static void Write(string prefix, string message, ConsoleColor color)
    {
        var previous = Console.ForegroundColor;
        Console.ForegroundColor = color;
        var symbol = prefix switch
        {
            "INFO" => "●",
            "WARN" => "▲",
            "ERROR" => "✖",
            "VERBOSE" => "··",
            _ => "•"
        };
        Console.WriteLine($"{DateTime.Now:HH:mm:ss} {symbol} {message}");
        Console.ForegroundColor = previous;
    }
}

internal static class LauncherPresentation
{
    public static void ShowBanner()
    {
        var previous = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleTheme.Accent;
        Console.WriteLine("╔════════════════════════════════════════════╗");
        Console.WriteLine("║         Ninny Trainer Launcher v1          ║");
        Console.WriteLine("║    Story Mode deployer • polished build    ║");
        Console.WriteLine("╚════════════════════════════════════════════╝");
        Console.ForegroundColor = previous;
        Console.WriteLine();
    }

    public static void ShowSummary(DeploymentResult result, bool dryRun, ILogger logger)
    {
        logger.Info(string.Empty);
        var status = dryRun ? "Simulated" : "Completed";
        logger.Info($"Deployment {status}:");
        logger.Info($"   ✓ Trainer DLLs: {result.TrainerDllsCopied}");
        logger.Info($"   ✓ Plugins:     {result.PluginDllsCopied}");
        logger.Info($"   ✓ Configs:     {result.ConfigsCopied}");
        logger.Info("----------------------------------------------");
        logger.Info("You can now launch GTA V Story Mode and open the trainer menu.");
    }
}

internal static class BattleyeAdvisor
{
    public static void Report(ILogger logger)
    {
        var settingsPaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Rockstar Games", "Launcher", "settings.json"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Rockstar Games", "Launcher", "settings.json")
        };

        var foundSetting = settingsPaths.FirstOrDefault(File.Exists);
        if (foundSetting is not null)
        {
            logger.Verbose($"Found Rockstar Launcher settings at {foundSetting}.");
            var contents = File.ReadAllText(foundSetting);
            if (contents.Contains("battl", StringComparison.OrdinalIgnoreCase))
            {
                logger.Info("Rockstar Launcher settings mention BattlEye. GTA V Story Mode does not start BattlEye, so no toggle is needed. Leaving settings untouched.");
                return;
            }
        }

        logger.Info("No BattlEye toggles detected. GTA V Story Mode runs offline without BattlEye, so nothing extra is required.");
    }
}

internal static class ConsoleTheme
{
    public const ConsoleColor Accent = ConsoleColor.Cyan;
    public const ConsoleColor Muted = ConsoleColor.DarkGray;
}
