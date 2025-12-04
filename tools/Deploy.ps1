param(
    [Parameter(Mandatory = $true)]
    [string]$GamePath,

    [string]$BuildConfiguration = "Release"
)

$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $PSScriptRoot
$buildFolder = Join-Path $projectRoot "bin/$BuildConfiguration"
$scriptFolder = Join-Path $GamePath "scripts"
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

Write-Host "Deployed Ninny Trainer to $trainerFolder" -ForegroundColor Green
