# 💜 Ninny Trainer — A Modern GTA V Single-Player Framework

A fully-featured, extensible, animated **GTA V trainer** designed for clean visuals, powerful tools, and a premium user experience.
Built on a custom **Ninny Purple UI Framework**, smooth animations, plugin support, and dozens of built-in systems.

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
3. Run the deployment helper to copy everything into your GTA V folder:
   ```powershell
   pwsh -File tools/Deploy.ps1 -GamePath "C:\Program Files\Rockstar Games\Grand Theft Auto V"
   ```
   This places `NinnyTrainer.dll` under `scripts/NinnyTrainer` and will also copy any `.dll` files you have in the repository `Plugins/` folder into `scripts/NinnyTrainer/Plugins`.
4. Launch GTA V (story mode).

Press **INSERT** to open the trainer (or set your own hotkey in the trainer Settings page).

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
