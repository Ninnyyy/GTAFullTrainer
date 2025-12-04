param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [switch]$SelfContained = $true
)

$ErrorActionPreference = "Stop"

$root = Resolve-Path "$PSScriptRoot/../.."
$trainerProject = Join-Path $root "NinnyTrainer.csproj"
$launcherProject = Join-Path $PSScriptRoot "TrainerLauncher.csproj"
$dist = Join-Path $PSScriptRoot "dist/$Runtime"
$payload = Join-Path $dist "payload/bin/$Configuration"

Write-Host "Building trainer ($Configuration)..." -ForegroundColor Cyan
dotnet build $trainerProject -c $Configuration

Write-Host "Publishing launcher ($Runtime)..." -ForegroundColor Cyan
$publishArgs = @(
    "publish", $launcherProject,
    "-c", "Release",
    "-r", $Runtime,
    "-o", $dist,
    "/p:PublishSingleFile=true",
    "/p:IncludeNativeLibrariesForSelfExtract=true"
)

if ($SelfContained) {
    $publishArgs += @("--self-contained", "true")
} else {
    $publishArgs += @("--self-contained", "false")
}

dotnet @publishArgs

Write-Host "Staging payload..." -ForegroundColor Cyan
New-Item -ItemType Directory -Force -Path $payload | Out-Null
Copy-Item (Join-Path $root "bin/$Configuration/*.dll") $payload -Force
Copy-Item (Join-Path $root "TrainerConfig.ini") $payload -Force
if (Test-Path (Join-Path $root "Plugins")) {
    $pluginsDest = Join-Path $payload "Plugins"
    New-Item -ItemType Directory -Force -Path $pluginsDest | Out-Null
    Copy-Item (Join-Path $root "Plugins/*.dll") $pluginsDest -Force -ErrorAction SilentlyContinue
}

Write-Host "Bundle ready in $dist" -ForegroundColor Green
Write-Host "Upload NinnyTrainer.Launcher.exe with the payload folder to your GitHub release assets." -ForegroundColor Yellow
