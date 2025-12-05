param(
    [string]$GamePath,
    [string]$BuildConfiguration = "Release"
)

$ErrorActionPreference = "Stop"

function Get-SteamInstallPath {
    $steamKeys = @(
        "HKLM:\\SOFTWARE\\WOW6432Node\\Valve\\Steam",
        "HKCU:\\SOFTWARE\\Valve\\Steam"
    )

    foreach ($key in $steamKeys) {
        try {
            $install = (Get-ItemProperty -Path $key -Name InstallPath -ErrorAction Stop).InstallPath
            if ($install) {
                return $install
            }
        }
        catch {
            continue
        }
    }

    return $null
}

function Get-RockstarInstallPath {
    $rockstarKeys = @(
        "HKLM:\\SOFTWARE\\WOW6432Node\\Rockstar Games\\Grand Theft Auto V",
        "HKCU:\\SOFTWARE\\Rockstar Games\\Grand Theft Auto V"
    )

    foreach ($key in $rockstarKeys) {
        try {
            $install = (Get-ItemProperty -Path $key -Name InstallFolder -ErrorAction Stop).InstallFolder
            if ($install) {
                return $install
            }
        }
        catch {
            continue
        }
    }

    return $null
}

function Get-EpicInstallPaths {
    $paths = @()
    $manifestDir = Join-Path $env:ProgramData "Epic\\EpicGamesLauncher\\Data\\Manifests"

    if (Test-Path $manifestDir) {
        foreach ($manifest in Get-ChildItem -Path $manifestDir -Filter "*.item" -ErrorAction SilentlyContinue) {
            try {
                $content = Get-Content $manifest.FullName -Raw | ConvertFrom-Json
                if ($content.DisplayName -like "Grand Theft Auto V*" -or $content.AppName -like "GTAV*") {
                    if ($content.InstallLocation) {
                        $paths += $content.InstallLocation
                    }
                }
            }
            catch {
                continue
            }
        }
    }

    $paths += "C:\\Program Files\\Epic Games\\GTAV"
    return $paths | Where-Object { $_ }
}

function Test-StoryModeInstall {
    param(
        [Parameter(Mandatory = $true)][string]$Path
    )

    if (-not (Test-Path $Path)) {
        return $false
    }

    $executables = @("GTA5.exe", "PlayGTAV.exe", "GTA5_en.exe")
    $hasStoryModeExe = $false

    foreach ($exe in $executables) {
        if (Test-Path (Join-Path $Path $exe)) {
            $hasStoryModeExe = $true
            break
        }
    }

    if (-not $hasStoryModeExe) { return $false }

    # Avoid dropping files into FiveM or other multiplayer-focused installs
    $fivemExe = Join-Path $Path "FiveM.exe"
    if (Test-Path $fivemExe) {
        return $false
    }

    return $true
}

function Resolve-StoryModePath {
    param([string]$ProvidedPath)

    $steamRoot = Get-SteamInstallPath
    $rockstarRoot = Get-RockstarInstallPath
    $epicRoots = Get-EpicInstallPaths

    $candidates = @()

    if ($ProvidedPath) {
        $candidates += $ProvidedPath
    }

    if ($steamRoot) {
        $candidates += (Join-Path $steamRoot "steamapps\\common\\Grand Theft Auto V")
    }

    if ($rockstarRoot) {
        $candidates += $rockstarRoot
    }

    if ($epicRoots) {
        $candidates += $epicRoots
    }

    $candidates += "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Grand Theft Auto V"
    $candidates += "C:\\Program Files\\Rockstar Games\\Grand Theft Auto V"
    $candidates += "C:\\Program Files\\Rockstar Games\\Grand Theft Auto V (Legacy)"
    $candidates += "C:\\Program Files\\Rockstar Games\\Grand Theft Auto V (Enhanced)"
    $candidates += "D:\\Games\\Grand Theft Auto V"
    $candidates += "D:\\Games\\Grand Theft Auto V (Legacy)"
    $candidates += "D:\\Games\\Grand Theft Auto V (Enhanced)"
    $candidates += "C:\\Program Files\\Epic Games\\GTAV"

    foreach ($candidate in $candidates) {
        if (Test-StoryModeInstall -Path $candidate) {
            return (Resolve-Path $candidate).Path
        }
    }

    return $null
}

$projectRoot = Split-Path -Parent $PSScriptRoot
$buildFolder = Join-Path $projectRoot "bin/$BuildConfiguration"

$resolvedGamePath = Resolve-StoryModePath -ProvidedPath $GamePath
if (-not $resolvedGamePath) {
    throw "Unable to locate a GTA V Story Mode installation. Provide -GamePath pointing at the folder that contains GTA5.exe. FiveM/online installs are not supported."
}

$scriptFolder = Join-Path $resolvedGamePath "scripts"
$trainerFolder = Join-Path $scriptFolder "NinnyTrainer"
$pluginsFolder = Join-Path $trainerFolder "Plugins"

if (-not (Test-Path $buildFolder)) {
    throw "Build output not found at $buildFolder. Build the project first (Release configuration)."
}

New-Item -ItemType Directory -Force -Path $scriptFolder | Out-Null
New-Item -ItemType Directory -Force -Path $trainerFolder | Out-Null
New-Item -ItemType Directory -Force -Path $pluginsFolder | Out-Null

Get-ChildItem -Path $buildFolder -Filter "*.dll" | ForEach-Object {
    Copy-Item $_.FullName -Destination $trainerFolder -Force
}

$rootConfig = Join-Path $projectRoot "TrainerConfig.ini"
if (Test-Path $rootConfig) {
    Copy-Item $rootConfig -Destination $trainerFolder -Force
}

$localPlugins = Join-Path $projectRoot "Plugins"
if (Test-Path $localPlugins) {
    Get-ChildItem -Path $localPlugins -Filter "*.dll" | ForEach-Object {
        Copy-Item $_.FullName -Destination $pluginsFolder -Force
    }
}

Write-Host "Deployed Ninny Trainer to $trainerFolder (Story Mode only)" -ForegroundColor Green
