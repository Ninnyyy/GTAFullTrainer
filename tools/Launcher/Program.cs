using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Microsoft.Win32;

namespace NinnyTrainer.Launcher;

internal sealed class Program
{
    private static int Main(string[] args)
    {
        var options = DeploymentOptions.Parse(args);
        var loggerBundle = LoggerFactory.Create(options);
        var logger = loggerBundle.Logger;

        LauncherPresentation.ShowBanner();

        try
        {
            var workflow = new LauncherWorkflow(options, logger, loggerBundle.LogPath);
            var result = workflow.Run();
            LauncherPresentation.ShowSummary(result, options.DryRun, logger);
            return result.Success ? 0 : 1;
        }
        catch (Exception ex)
        {
            logger.Error($"Unexpected launcher failure: {ex.Message}");
            return 1;
        }
    }
}

internal sealed record DeploymentOptions(
    string? GamePath,
    string? BuildConfiguration,
    bool DryRun,
    bool Verbose,
    string? ProjectRoot,
    bool LaunchGame,
    string? LogFile,
    string? SummaryFile)
{
    public static DeploymentOptions Parse(string[] args)
    {
        string? gamePath = null;
        string? buildConfiguration = null;
        string? projectRoot = null;
        var dryRun = false;
        var verbose = false;
        var launchGame = false;
        string? logFile = null;
        string? summaryFile = null;

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
                case "--launch-game":
                    launchGame = true;
                    break;
                case "--log-file":
                    logFile = NextArg(args, ref i, "log file path");
                    break;
                case "--summary-file":
                    summaryFile = NextArg(args, ref i, "summary file path");
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

        return new DeploymentOptions(gamePath, buildConfiguration, dryRun, verbose, projectRoot, launchGame, logFile, summaryFile);
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
        Console.WriteLine("      --launch-game            Launch GTA V Story Mode immediately after deployment");
        Console.WriteLine("      --log-file <path>        Save a copy of the launcher output to a custom path");
        Console.WriteLine("      --summary-file <path>    Save a JSON summary of the deployment to a path (default: logs folder)");
        Console.WriteLine("      --dry-run                Simulate copy operations without writing files");
        Console.WriteLine("      --verbose                Print detailed detection and copy steps");
        Console.WriteLine("  -h, --help                   Show this help");
    }
}

internal sealed class LauncherWorkflow
{
    private readonly DeploymentOptions _options;
    private readonly ILogger _logger;
    private readonly string _logPath;

    public LauncherWorkflow(DeploymentOptions options, ILogger logger, string logPath)
    {
        _options = options;
        _logger = logger;
        _logPath = logPath;
    }

    public LauncherResult Run()
    {
        _logger.Info("Looking for project root...");
        var projectRoot = _options.ProjectRoot ?? PathFinder.FindProjectRoot();
        if (!string.IsNullOrWhiteSpace(projectRoot))
        {
            _logger.Info($"Project root: {projectRoot}");
        }
        else
        {
            _logger.Warn("Project root not found; expecting trainer payload next to the launcher.");
        }

        var buildConfiguration = _options.BuildConfiguration ?? "Release";
        var buildOutput = BuildOutputLocator.Resolve(projectRoot, buildConfiguration, _logger);
        if (string.IsNullOrWhiteSpace(buildOutput))
        {
            _logger.Error("Unable to locate trainer binaries. Build the project or run the launcher next to a payload folder containing NinnyTrainer.dll.");
            return LauncherResult.Failure(_logPath, projectRoot, null, null);
        }

        _logger.Info($"Using trainer binaries from: {buildOutput}");

        _logger.Info("Searching for GTA V Story Mode...");
        var storyModePath = _options.GamePath ?? StoryModeLocator.ResolveStoryModePath(_logger);
        if (string.IsNullOrWhiteSpace(storyModePath))
        {
            _logger.Error("Unable to locate a GTA V Story Mode installation. Use --game-path to point at the folder that contains GTA5.exe.");
            return LauncherResult.Failure(_logPath, projectRoot, buildOutput, null);
        }

        var storyModeFullPath = Path.GetFullPath(storyModePath);
        _logger.Info($"Story Mode folder: {storyModeFullPath}");

        var healthReport = InstallHealthCheck.Run(storyModeFullPath, _logger);
        BattleyeAdvisor.Report(_logger);

        var copier = new DeploymentCopier(projectRoot, buildOutput, storyModeFullPath, _logger, _options.DryRun);
        var copyResult = copier.Copy();

        var summaryPath = SummaryExporter.TryWrite(_options.SummaryFile, storyModeFullPath, buildOutput, copyResult, healthReport, _logPath, _logger, _options.DryRun);

        var launchResult = LaunchGameIfRequested(storyModeFullPath, _options.LaunchGame, _options.DryRun);

        return LauncherResult.Successful(copyResult, healthReport, storyModeFullPath, buildOutput, _logPath, summaryPath, launchResult);
    }

    private LaunchResult LaunchGameIfRequested(string storyModePath, bool launchGame, bool dryRun)
    {
        if (!launchGame)
        {
            return LaunchResult.Skipped("Launch not requested.");
        }

        var executable = StoryModeLocator.ResolveExecutable(storyModePath);
        if (executable is null)
        {
            _logger.Warn("Launch requested, but GTA V executable was not found in the detected Story Mode path.");
            return LaunchResult.Failed("GTA5.exe missing");
        }

        _logger.Info($"Launching GTA V Story Mode from {executable}...");
        if (dryRun)
        {
            _logger.Info("(dry run) Would start GTA5.exe after deployment");
            return LaunchResult.Skipped("Dry run");
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = executable,
                WorkingDirectory = Path.GetDirectoryName(executable) ?? storyModePath
            });
            _logger.Info("GTA V launch triggered. Story Mode will open with the trainer files in place.");
            return LaunchResult.Started(executable);
        }
        catch (Exception ex)
        {
            _logger.Warn($"Failed to start GTA V automatically: {ex.Message}");
            return LaunchResult.Failed(ex.Message);
        }
    }
}

internal sealed class DeploymentCopier
{
    private readonly string _projectRoot;
    private readonly string _buildOutput;
    private readonly string _storyModePath;
    private readonly ILogger _logger;
    private readonly bool _dryRun;

    public DeploymentCopier(string? projectRoot, string buildOutput, string storyModePath, ILogger logger, bool dryRun)
    {
        _projectRoot = string.IsNullOrWhiteSpace(projectRoot) ? buildOutput : projectRoot;
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

internal static class BuildOutputLocator
{
    public static string? Resolve(string? projectRoot, string buildConfiguration, ILogger logger)
    {
        var candidates = new List<string>();
        var assemblyDir = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        if (!string.IsNullOrWhiteSpace(projectRoot))
        {
            candidates.Add(Path.Combine(projectRoot, "bin", buildConfiguration));
        }

        var repoRoot = TryFindRepoRoot(assemblyDir);
        if (repoRoot is not null)
        {
            logger.Verbose($"Detected repository root near launcher: {repoRoot}");
            candidates.Add(Path.Combine(repoRoot, "bin", buildConfiguration));
        }

        candidates.Add(Path.Combine(assemblyDir, "payload", "bin", buildConfiguration));
        candidates.Add(assemblyDir);

        foreach (var candidate in candidates)
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            var trainerDll = Path.Combine(candidate, "NinnyTrainer.dll");
            if (File.Exists(trainerDll))
            {
                logger.Verbose($"Found trainer payload at {trainerDll}");
                return candidate;
            }

            logger.Verbose($"No trainer DLL found in {candidate}");
        }

        return null;
    }

    private static string? TryFindRepoRoot(string assemblyDir)
    {
        var dir = new DirectoryInfo(assemblyDir);
        for (int i = 0; i < 6 && dir != null; i++)
        {
            var csproj = Path.Combine(dir.FullName, "NinnyTrainer.csproj");
            if (File.Exists(csproj))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
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

    public static string? ResolveExecutable(string storyModePath)
    {
        var executables = new[] { "GTA5.exe", "PlayGTAV.exe", "GTA5_en.exe" };
        return executables
            .Select(exe => Path.Combine(storyModePath, exe))
            .FirstOrDefault(File.Exists);
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

internal static class InstallHealthCheck
{
    private static readonly (string FileName, string Message)[] RequiredFiles =
    {
        ("ScriptHookV.dll", "ScriptHookV not found. Install Alexander Blade's ScriptHookV into the GTA V root."),
        ("ScriptHookVDotNet.asi", "ScriptHookVDotNet.asi missing. Install ScriptHookVDotNet so the trainer can load."),
        ("dinput8.dll", "dinput8.dll missing. Ensure the ScriptHookV package is fully extracted.")
    };

    public static InstallHealthReport Run(string storyModePath, ILogger logger)
    {
        var warnings = new List<string>();

        foreach (var (fileName, message) in RequiredFiles)
        {
            var candidate = Path.Combine(storyModePath, fileName);
            if (!File.Exists(candidate))
            {
                warnings.Add(message);
                logger.Warn(message);
            }
        }

        var scriptsFolder = Path.Combine(storyModePath, "scripts");
        if (!Directory.Exists(scriptsFolder))
        {
            var warning = "scripts folder not found; it will be created during deployment.";
            warnings.Add(warning);
            logger.Warn(warning);
        }
        else if (!IsWritable(scriptsFolder))
        {
            var warning = "scripts folder is not writable. Run the launcher elevated or adjust permissions.";
            warnings.Add(warning);
            logger.Warn(warning);
        }

        return new InstallHealthReport(warnings);
    }

    private static bool IsWritable(string path)
    {
        try
        {
            var testFile = Path.Combine(path, $".nt_writetest_{Guid.NewGuid():N}.tmp");
            File.WriteAllText(testFile, "ok");
            File.Delete(testFile);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

internal interface ILogger
{
    void Info(string message);
    void Warn(string message);
    void Error(string message);
    void Verbose(string message);
}

internal static class LoggerFactory
{
    public static LoggerBundle Create(DeploymentOptions options)
    {
        var logPath = ResolveLogPath(options.LogFile);
        var loggers = new List<ILogger> { new ConsoleLogger(options.Verbose), new FileLogger(options.Verbose, logPath) };
        return new LoggerBundle(new CompositeLogger(loggers), logPath);
    }

    private static string ResolveLogPath(string? userPath)
    {
        if (!string.IsNullOrWhiteSpace(userPath))
        {
            var targetDir = Path.GetDirectoryName(userPath);
            if (string.IsNullOrWhiteSpace(targetDir))
            {
                targetDir = Directory.GetCurrentDirectory();
            }

            Directory.CreateDirectory(targetDir);
            return Path.GetFullPath(Path.Combine(targetDir, Path.GetFileName(userPath)));
        }

        var defaultDir = Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(defaultDir);
        return Path.Combine(defaultDir, $"NinnyLauncher-{DateTime.Now:yyyyMMdd-HHmmss}.log");
    }
}

internal sealed record LoggerBundle(ILogger Logger, string LogPath);

internal sealed class ConsoleLogger(bool verbose) : ILogger
{
    private readonly bool _verbose = verbose;

    public void Info(string message) => Write("INFO", message, ConsoleTheme.AccentBright);

    public void Warn(string message) => Write("WARN", message, ConsoleTheme.Highlight);

    public void Error(string message) => Write("ERROR", message, ConsoleTheme.Alert);

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
        var previousBackground = Console.BackgroundColor;
        Console.BackgroundColor = ConsoleTheme.Background;
        Console.ForegroundColor = color;
        var symbol = prefix switch
        {
            "INFO" => "◆",
            "WARN" => "▲",
            "ERROR" => "✖",
            "VERBOSE" => "··",
            _ => "•"
        };
        Console.WriteLine($"{DateTime.Now:HH:mm:ss} {symbol} {message}");
        Console.ForegroundColor = previous;
        Console.BackgroundColor = previousBackground;
    }
}

internal sealed class FileLogger(bool verbose, string logPath) : ILogger
{
    private readonly bool _verbose = verbose;
    private readonly string _logPath = logPath;

    public void Info(string message) => Write("INFO", message);

    public void Warn(string message) => Write("WARN", message);

    public void Error(string message) => Write("ERROR", message);

    public void Verbose(string message)
    {
        if (_verbose)
        {
            Write("VERBOSE", message);
        }
    }

    private void Write(string prefix, string message)
    {
        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{prefix}] {message}{Environment.NewLine}";
        File.AppendAllText(_logPath, line);
    }
}

internal sealed class CompositeLogger : ILogger
{
    private readonly IReadOnlyList<ILogger> _loggers;

    public CompositeLogger(IReadOnlyList<ILogger> loggers)
    {
        _loggers = loggers;
    }

    public void Info(string message)
    {
        foreach (var logger in _loggers)
        {
            logger.Info(message);
        }
    }

    public void Warn(string message)
    {
        foreach (var logger in _loggers)
        {
            logger.Warn(message);
        }
    }

    public void Error(string message)
    {
        foreach (var logger in _loggers)
        {
            logger.Error(message);
        }
    }

    public void Verbose(string message)
    {
        foreach (var logger in _loggers)
        {
            logger.Verbose(message);
        }
    }
}

internal static class LauncherPresentation
{
    public static void ShowBanner()
    {
        ConsoleAnimator.DrawPulseBanner();
        Console.WriteLine();
    }

    public static void ShowSummary(LauncherResult result, bool dryRun, ILogger logger)
    {
        logger.Info(string.Empty);
        var status = result.Success ? (dryRun ? "Simulated" : "Completed") : "Failed";
        logger.Info($"Deployment {status}:");
        logger.Info($"   ✓ Trainer DLLs: {result.CopyResult.TrainerDllsCopied}");
        logger.Info($"   ✓ Plugins:     {result.CopyResult.PluginDllsCopied}");
        logger.Info($"   ✓ Configs:     {result.CopyResult.ConfigsCopied}");

        if (!result.Success)
        {
            logger.Warn("Launcher exited early. Check the log for details and rerun when ready.");
            logger.Info($"Logs:    {result.LogPath}");
            if (!string.IsNullOrWhiteSpace(result.SummaryPath))
            {
                logger.Info($"Summary: {result.SummaryPath}");
            }
            return;
        }

        if (result.HealthReport.Warnings.Count > 0)
        {
            logger.Warn("Health checks: review the warnings below (Story Mode can still run):");
            foreach (var warning in result.HealthReport.Warnings)
            {
                logger.Warn($"   - {warning}");
            }
        }
        else
        {
            logger.Info("Health checks: all essentials detected.");
        }

        if (result.LaunchResult.Status == LaunchStatus.Started)
        {
            logger.Info("Launch: GTA V Story Mode start triggered.");
        }
        else if (result.LaunchResult.Status == LaunchStatus.Skipped)
        {
            logger.Info($"Launch: skipped ({result.LaunchResult.Reason})");
        }
        else
        {
            logger.Warn($"Launch: could not start GTA V automatically ({result.LaunchResult.Reason})");
        }

        logger.Info("----------------------------------------------");
        logger.Info($"Logs:    {result.LogPath}");
        if (!string.IsNullOrWhiteSpace(result.SummaryPath))
        {
            logger.Info($"Summary: {result.SummaryPath}");
        }
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
    public const ConsoleColor AccentBright = ConsoleColor.Magenta;
    public const ConsoleColor Accent = ConsoleColor.DarkMagenta;
    public const ConsoleColor Highlight = ConsoleColor.Gray;
    public const ConsoleColor Muted = ConsoleColor.DarkGray;
    public const ConsoleColor Background = ConsoleColor.Black;
    public const ConsoleColor Alert = ConsoleColor.DarkRed;
}

internal static class ConsoleAnimator
{
    public static void DrawPulseBanner()
    {
        var frames = new List<(string line, ConsoleColor color)>
        {
            ("╔════════════════════════════════════════════╗", ConsoleTheme.Accent),
            ("║     Ninny Trainer Launcher – Story Luxe     ║", ConsoleTheme.AccentBright),
            ("║  Premium deploy • purple / charcoal glow   ║", ConsoleTheme.Highlight),
            ("╚════════════════════════════════════════════╝", ConsoleTheme.Accent)
        };

        foreach (var frame in frames)
        {
            WriteAnimatedLine(frame.line, frame.color);
        }
    }

    private static void WriteAnimatedLine(string text, ConsoleColor color)
    {
        var previousForeground = Console.ForegroundColor;
        var previousBackground = Console.BackgroundColor;
        Console.BackgroundColor = ConsoleTheme.Background;
        Console.ForegroundColor = color;

        foreach (var ch in text)
        {
            Console.Write(ch);
            Thread.Sleep(4);
        }

        Console.WriteLine();
        Console.ForegroundColor = previousForeground;
        Console.BackgroundColor = previousBackground;
    }
}

internal sealed record InstallHealthReport(IReadOnlyList<string> Warnings);

internal sealed record LaunchResult(LaunchStatus Status, string? Reason, string? Executable)
{
    public static LaunchResult Skipped(string? reason) => new(LaunchStatus.Skipped, reason, null);
    public static LaunchResult Failed(string? reason) => new(LaunchStatus.Failed, reason, null);
    public static LaunchResult Started(string executable) => new(LaunchStatus.Started, null, executable);
}

internal enum LaunchStatus
{
    Skipped,
    Started,
    Failed
}

internal sealed record LauncherResult(
    bool Success,
    DeploymentResult CopyResult,
    InstallHealthReport HealthReport,
    string? StoryModePath,
    string? BuildOutput,
    string LogPath,
    string? SummaryPath,
    LaunchResult LaunchResult)
{
    public static LauncherResult Successful(DeploymentResult copy, InstallHealthReport health, string storyModePath, string buildOutput, string logPath, string? summaryPath, LaunchResult launch) =>
        new(true, copy, health, storyModePath, buildOutput, logPath, summaryPath, launch);

    public static LauncherResult Failure(string logPath, string? projectRoot, string? buildOutput, string? storyModePath) =>
        new(false, new DeploymentResult(0, 0, 0), new InstallHealthReport(Array.Empty<string>()), storyModePath ?? projectRoot, buildOutput, logPath, null, LaunchResult.Skipped("failed early"));
}

internal static class SummaryExporter
{
    public static string? TryWrite(string? userPath, string storyModePath, string buildOutput, DeploymentResult copy, InstallHealthReport health, string logPath, ILogger logger, bool dryRun)
    {
        var summaryPath = ResolveSummaryPath(userPath);
        var payload = new
        {
            GeneratedAt = DateTime.Now,
            StoryModePath = storyModePath,
            BuildOutput = buildOutput,
            Copy = new
            {
                copy.TrainerDllsCopied,
                copy.PluginDllsCopied,
                copy.ConfigsCopied,
                DryRun = dryRun
            },
            HealthWarnings = health.Warnings,
            LogPath = logPath
        };

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(summaryPath)!);
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(summaryPath, json);
            logger.Info($"Saved deployment summary to {summaryPath}");
            return summaryPath;
        }
        catch (Exception ex)
        {
            logger.Warn($"Unable to write summary file: {ex.Message}");
            return null;
        }
    }

    private static string ResolveSummaryPath(string? userPath)
    {
        if (!string.IsNullOrWhiteSpace(userPath))
        {
            var targetDir = Path.GetDirectoryName(userPath);
            if (string.IsNullOrWhiteSpace(targetDir))
            {
                targetDir = Directory.GetCurrentDirectory();
            }

            Directory.CreateDirectory(targetDir);
            return Path.GetFullPath(Path.Combine(targetDir, Path.GetFileName(userPath)));
        }

        var defaultDir = Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(defaultDir);
        return Path.Combine(defaultDir, $"NinnyLauncher-summary-{DateTime.Now:yyyyMMdd-HHmmss}.json");
    }
}
