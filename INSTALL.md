# Installation Guide

This guide walks you through setting up Ninny Trainer for **GTA V Story Mode** using the premium launcher or the lightweight deploy script.

## Prerequisites
- Windows PC with **GTA V Story Mode** installed (Steam/Rockstar/Epic, legacy or enhanced).
- .NET 8 SDK (for building) or the published launcher executable.
- ScriptHookV and ScriptHookVDotNet files ready. If you place them under a local `Dependencies/` folder, the launcher will copy
  them into your Story Mode install automatically.
- A terminal/PowerShell window with permission to write to your GTA V directory.

## Quick install with the launcher (recommended)
1. Build the trainer and launcher bundle:
   ```powershell
   ./tools/Launcher/PublishLauncher.ps1
   ```
   - Produces `publish/TrainerLauncher.exe` plus a `payload/` folder containing the trainer DLLs and plugins.
   - If you place ScriptHookV/ScriptHookVDotNet or other files inside `Dependencies/` at the repo root, they are bundled too a
     nd will be copied into the game automatically.
2. Run the launcher and let it auto-discover Story Mode:
   ```powershell
   ./publish/TrainerLauncher.exe
   ```
   - Uses the purple/grey/black themed console with animated banner and colored logs.
   - Auto-detects Steam/Rockstar/Epic installs (legacy/enhanced), skips FiveM/online-only folders, and copies the payload into `scripts/NinnyTrainer`.
   - Log file and JSON summary are written next to the executable; pass `--log` or `--summary` to override locations.
3. Optionally auto-launch Story Mode after deployment:
   ```powershell
   ./publish/TrainerLauncher.exe --auto-launch
   ```

## Deploy with the PowerShell helper
If you already have a built trainer DLL, you can copy it directly without rebuilding:
```powershell
./tools/Deploy.ps1
```
- Auto-detects Story Mode paths across Steam/Rockstar/Epic and legacy/enhanced installs.
- Copies trainer binaries and plugins into `scripts/NinnyTrainer` while skipping FiveM/online installs.
- Use `-GamePath "C:\Games\Grand Theft Auto V"` to override detection.

## Configuration and first run
- The trainer menu opens by default in Story Mode (auto-open), with logging enabled and animated purple UI.
- Use the in-game Settings page to change the open-menu hotkey, animation speed, UI scale, and logging.
- Logs are written to `scripts/NinnyTrainer/Logs/Trainer.log` inside your GTA V directory.

## Troubleshooting
- If the launcher cannot find the game, supply `--game-path "C:\Games\Grand Theft Auto V"`.
- Ensure ScriptHookV and ScriptHookVDotNet are present; the game must be launched in single-player mode.
- If you see build errors, install the .NET 8 SDK and rerun the publish script.
- For full validation, run `./tools/ValidateAll.ps1` (Windows) or `./tools/ValidateAll.sh` (macOS/Linux with wine/proton build tools installed).
