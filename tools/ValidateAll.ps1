param(
    [string]$Configuration = "Release",
    [switch]$SkipTrainer
)

$ErrorActionPreference = "Stop"

function Write-Section($text) {
    Write-Host "`n=== $text ===" -ForegroundColor Cyan
}

function Require-Dotnet {
    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    if (-not $dotnet) {
        Write-Error "The .NET SDK is required to run validation. Install .NET 8 SDK and re-run." -ErrorAction Stop
    }

    Write-Host "dotnet: $($dotnet.Source)" -ForegroundColor Green
    dotnet --info | Select-Object -First 5
}

function Test-TrainerDependencies {
    param([string]$Root)

    $referenceFolder = Join-Path $Root "ScriptHookVDotNet"
    $required = @(
        (Join-Path $referenceFolder "ScriptHookVDotNet.dll"),
        (Join-Path $referenceFolder "ScriptHookVDotNet2.dll"),
        (Join-Path $referenceFolder "ScriptHookVDotNet3.dll")
    )

    $missing = $required | Where-Object { -not (Test-Path $_) }
    if ($missing) {
        Write-Warning "Skipping trainer build because required ScriptHookVDotNet DLLs are missing:`n$($missing -join "`n")"
        return $false
    }

    return $true
}

function Build-Trainer {
    param([string]$Root, [string]$Configuration)

    Write-Section "Building trainer ($Configuration)"
    dotnet build (Join-Path $Root "NinnyTrainer.csproj") -c $Configuration
}

function Build-Launcher {
    param([string]$Root, [string]$Configuration)

    Write-Section "Building launcher ($Configuration)"
    dotnet build (Join-Path $Root "tools/Launcher/TrainerLauncher.csproj") -c $Configuration
}

$root = Resolve-Path "$PSScriptRoot/.." | Select-Object -ExpandProperty Path

Write-Section "Environment"
Require-Dotnet
Build-Launcher -Root $root -Configuration $Configuration

if (-not $SkipTrainer) {
    if (Test-TrainerDependencies -Root $root) {
        Build-Trainer -Root $root -Configuration $Configuration
    }
}

Write-Section "Validation complete"
