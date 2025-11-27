using System.Collections.Generic;
using GTA;
using GTA.Native;
using GTAFullTrainer.CoreUI;
using GTAFullTrainer.Rendering;
using GTAFullTrainer.UI;
using GTAFullTrainer.Pages;

public class MainScript : Script
{
    public MainScript()
    {
        Tick += OnTick;
        KeyUp += OnKeyUp;

        Interval = 0;

        SetupUI();
    }

    private void SetupUI()
    {
        // Reset existing categories
        UICore.Initialize();

        // ====================================================
        // REGISTER CATEGORIES + THEIR ITEMS
        // ====================================================

        // 1. PLAYER PAGE
        UICore.RegisterCategory("Player", new List<UIControl> {
            new UIButton("Godmode", () => PlayerPage.ToggleGod()),
            new UIButton("Never Wanted", () => PlayerPage.ToggleWanted()),
            new UIButton("Heal Player", () => PlayerPage.HealPlayer()),
            new UISlider("Run Speed", 0.5f, 5.0f, 1.0f),
            new UISlider("Swim Speed", 0.5f, 5.0f, 1.0f),
            new UIList("Walk Style", new [] {"Default", "Gangster", "Swagger", "Injured"})
        });

        // 2. VEHICLE PAGE
        UICore.RegisterCategory("Vehicle", new List<UIControl> {
            new UIButton("Spawn Supercar", () => VehiclePage.SpawnSuper()),
            new UIButton("Repair Vehicle", () => VehiclePage.Repair()),
            new UIButton("Flip Vehicle", () => VehiclePage.Flip()),
            new UIToggle("Invincible Vehicle", false),
            new UISlider("Torque", 0.1f, 10f, 1f),
            new UISlider("Brake Force", 0.1f, 10f, 1f)
        });

        // 3. WEAPON MODDER PAGE
        UICore.RegisterCategory("Weapon Modder", new List<UIControl> {
            new UIList("Bullet Type", new [] {"Normal", "Fire", "Freeze", "Shockwave"}),
            new UISlider("Damage Boost", 1f, 10f, 1f),
            new UIToggle("Explosive Ammo", false),
            new UIButton("Save Weapon Preset", () => WeaponModderPage.SavePreset()),
            new UIButton("Load Weapon Preset", () => WeaponModderPage.LoadPreset())
        });

        // 4. WORLD BUILDER PAGE
        UICore.RegisterCategory("World Builder", new List<UIControl> {
            new UIButton("Select Target", () => WorldBuilderPage.SelectTarget()),
            new UIToggle("Placement Mode", false),
            new UIToggle("Freecam Mode", false),
            new UIButton("Save Map", () => WorldBuilderPage.SaveMap()),
            new UIButton("Load Map", () => WorldBuilderPage.LoadMap()),
            new UIButton("Clear Nearby", () => WorldBuilderPage.ClearMap())
        });

        // 5. HUD PAGE
        UICore.RegisterCategory("HUD", new List<UIControl> {
            new UIToggle("Speedometer", false),
            new UIToggle("RPM Gauge", false),
            new UIToggle("G-Force Meter", false),
            new UIToggle("Compass", false),
            new UIToggle("Health/Armor", false),
            new UIToggle("Damage Indicator", false)
        });

        // 6. THEME PAGE
        UICore.RegisterCategory("Theme", new List<UIControl> {
            new UIList("Theme", new [] {"Purple", "Blue", "Red", "Gold"}),
            new UIToggle("Glow Effects", true),
            new UIToggle("Sound Effects", true)
        });

        // 7. SUPERPOWERS
        UICore.RegisterCategory("Superpowers", new List<UIControl> {
            new UIToggle("Flash Run", false),
            new UIToggle("Super Jump", false),
            new UIToggle("Time Slow", false),
            new UISlider("Strength Multiplier", 1f, 20f, 1f)
        });

        // 8. DEV DEBUG TOOLS (optional)
        UICore.RegisterCategory("Dev Tools", new List<UIControl> {
            new UIToggle("Show Coordinates", false),
            new UIButton("Reload Trainer", () => ReloadTrainer()),
        });
    }

    private void OnTick(object sender, System.EventArgs e)
    {
        // Only process menu if open
        UICore.Process();

        // Apply logic of pages that run in background:
        WeaponModderPage.ApplyLogic();
        WorldBuilderPage.ApplyLogic();
        HUDWidgetsPage.ApplyLogic();
    }

    private void OnKeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
    {
        // Toggle menu (INSERT key)
        if (e.KeyCode == System.Windows.Forms.Keys.Insert)
        {
            UICore.ToggleMenu();
            return;
        }

        // If menu is open → UI navigation
        if (UICore.MenuOpen)
            UICore.HandleInput();
    }

    private void ReloadTrainer()
    {
        // Simple reload logic
        SetupUI();
        UICore.ActiveCategory = 0;
        UICore.ActiveItem = 0;

        UI.SoundManager.PlayOpen();
    }
}
