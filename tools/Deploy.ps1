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

function Test-StoryModeInstall {
    param(
        [Parameter(Mandatory = $true)][string]$Path
    )

    if (-not (Test-Path $Path)) {
        return $false
    }

    $gtaExe = Join-Path $Path "GTA5.exe"
    if (-not (Test-Path $gtaExe)) {
        return $false
    }

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

    $candidates += "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Grand Theft Auto V"
    $candidates += "C:\\Program Files\\Rockstar Games\\Grand Theft Auto V"

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
