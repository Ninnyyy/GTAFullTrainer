# 💜 Ninny Trainer — A Modern GTA V Single-Player Framework

A fully-featured, extensible, animated **GTA V trainer** designed for clean visuals, powerful tools, and a premium user experience.
Built on a custom **Ninny Purple UI Framework**, smooth animations, plugin support, and dozens of built-in systems.

For forward-looking enhancements and ambitious experiments, see [Advanced Feature Ideas](ADVANCED_IDEAS.md).

> ⚠️ **IMPORTANT:** This trainer is strictly for **single-player** use.
> Using mods in GTA Online will result in a ban.
> This project does *not* support, encourage, or condone online cheating.

---

## ✨ Features

### 💜 Modern Animated UI
- Purple neon theme
- Smooth slide-in transitions
- Blur effect behind menu
- Animated category switching
- Glow highlights & pulse effects
- Clean slider, toggle, list, and button components
- Sound effects (navigate, select, open, close)

### ⚔ Player Tools
- Godmode
- Heal player
- Never wanted
- Walkstyle selector
- Speed modifiers

### 🚗 Vehicle Tools
- Spawn vehicles
- Repair / flip
- Invincibility
- Torque & brake tuning

### 🔫 Weapon Modder
- Laser bullets
- Freeze bullets
- Fire rounds
- Shockwave blast
- Damage multipliers
- Attachment toggles
- Preset save/load system

### 🧱 World Builder / Map Editor
- Select entities
- Move / rotate
- Snap to ground
- Placement mode
- Freecam build mode
- Duplicate entities
- Save map
- Load map
- Clear world

### 🧭 HUD Overlays
- Speedometer
- RPM gauge
- G-force meter
- Compass
- Damage indicator
- Health / armor bars

### 🧩 Add-On Plugin API
- Add pages dynamically
- Add buttons, toggles, sliders, lists
- Hot-load plugins from `/Plugins/`
- Plugin sandbox interface

---

## 📁 Project Structure

Everything is modular and built to be extended.

---

## ▶️ How to Install (Player)

1. Install **ScriptHookV** (Alexander Blade) and **ScriptHookVDotNet** (build 3.x recommended; 2.x is also bundled in the project references).
2. Build this project in **Release** mode with `.NET Framework 4.8`.
3. Choose your preferred deployment helper to copy everything into your GTA V folder (both only target **Story Mode**, intentionally skip FiveM/online installs, and will note that BattlEye isn’t used for Story Mode):
   - **One-click EXE launcher** (auto-finds Steam/Rockstar/Epic installs, legacy/enhanced paths, walks up the repo to find `NinnyTrainer.csproj`, saves a timestamped log + JSON summary, and can optionally auto-launch Story Mode):
     ```powershell
     dotnet publish tools/Launcher/TrainerLauncher.csproj -c Release -r win-x64 -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
     # run after publishing; useful flags:
     #   --launch-game       start GTA5.exe after copying
     #   --log-file          override where the launcher writes its premium log file
     #   --summary-file      override where the launcher writes its JSON deployment summary
     #   --verbose           show detailed detection/copy output
     #   --dry-run           simulate without copying
     #   --game-path         override the Story Mode install folder
     .\tools\Launcher\bin\Release\net8.0\win-x64\publish\NinnyTrainer.Launcher.exe
     ```
   - **PowerShell script** (same detection logic, handy for quick terminal use):
     ```powershell
     pwsh -File tools/Deploy.ps1
     # or explicitly: pwsh -File tools/Deploy.ps1 -GamePath "C\Program Files\Rockstar Games\Grand Theft Auto V"
     ```
   Both helpers place `NinnyTrainer.dll` under `scripts/NinnyTrainer` and also copy any `.dll` files you have in the repository `Plugins/` folder into `scripts/NinnyTrainer/Plugins`.
   The launcher saves a colorized console log and JSON summary in `tools/Launcher/bin/<config>/<rid>/publish/logs` by default (override with `--log-file` / `--summary-file`).

4. Launch GTA V (story mode only). The trainer will open its menu automatically on first load, and also logs menu events to `NinnyTrainer.log` alongside your game scripts folder.

Press **INSERT** to toggle the trainer (or set your own hotkey in the trainer Settings page).

---

## 🧩 Create Your Own Add-On Plugin

Drop a compiled `.dll` into: `Grand Theft Auto V/scripts/NinnyTrainer/Plugins`.

**Quick-start example** (see `Examples/SamplePlugin.cs` for a full file):
```csharp
using NinnyTrainer.Plugins;

public class HelloWorldPlugin : ITrainerPlugin
{
    public void Initialize()
    {
        GTA.UI.Notify("~p~Hello from my plugin!");
    }
}
```

Build the plugin as a **Class Library targeting .NET Framework 4.8**, copy the output `.dll` into the `Plugins` folder, and restart/reload the trainer.

---

🛡 License

This project is released under the MIT License.
See LICENSE for details.

⚠️ Legal Disclaimer

This trainer is designed only for:

Single-player

Offline use

Modding, testing, and creative experimentation

Using mods in GTA Online violates Rockstar’s TOS and will result in a ban.
The author is not responsible for misuse.
