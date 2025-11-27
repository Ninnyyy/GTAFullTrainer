using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GTA;
using NinnyTrainer.CoreUI;
using NinnyTrainer.UI;
using NinnyTrainer.Rendering;
using NinnyTrainer.Pages;
using NinnyTrainer.Plugins;

public class MainScript : Script
{
    public MainScript()
    {
        Tick += OnTick;
        KeyUp += OnKeyUp;
        Interval = 0;

        SetupUI();
        PluginLoader.LoadPlugins();
    }

    private void SetupUI()
    {
        UICore.Initialize();

        // PLAYER
        UICore.RegisterCategory("Player", new List<UIControl> {
            new UIButton("Godmode", () => PlayerPage.ToggleGod()),
            new UIButton("Never Wanted", () => PlayerPage.ToggleNeverWanted()),
            new UIButton("Heal Player", () => PlayerPage.Heal()),
            new UIList("Walk Style", new [] { "Default","Gangster","Swagger","Injured" },
                i => PlayerPage.SetWalkStyle(i)),
            new UISlider("Run Speed", 0.5f, 5f, 1f, v => PlayerPage.SetRunSpeed(v)),
            new UISlider("Swim Speed", 0.5f, 5f, 1f, v => PlayerPage.SetSwimSpeed(v))
        });

        // VEHICLE
        UICore.RegisterCategory("Vehicle", new List<UIControl> {
            new UIButton("Spawn Supercar", () => VehiclePage.SpawnSupercar()),
            new UIButton("Repair Vehicle", () => VehiclePage.Repair()),
            new UIButton("Flip Vehicle", () => VehiclePage.Flip()),
            new UIToggle("Invincible Vehicle", false, s => VehiclePage.SetInvincible(s)),
            new UISlider("Torque", 0.1f, 10f, 1f, v => VehiclePage.SetTorque(v)),
            new UISlider("Brake Force", 0.1f, 10f, 1f, v => VehiclePage.SetBrakes(v))
        });

        // WEAPON MODDER
        UICore.RegisterCategory("Weapon Modder", new List<UIControl> {
            new UIList("Bullet Type", new[] { "Normal","Fire","Freeze","Laser","Shockwave"},
                i => WeaponModderPage.SetBulletType(i)),
            new UISlider("Damage Multiplier", 1f, 10f, 1f,
                v => WeaponModderPage.SetDamageMultiplier(v)),
            new UIToggle("Explosive Ammo", false,
                s => WeaponModderPage.SetExplosiveAmmo(s)),
            new UIButton("Save Preset", () => WeaponModderPage.SavePreset()),
            new UIButton("Load Preset", () => WeaponModderPage.LoadPreset())
        });

        // WORLD BUILDER
        UICore.RegisterCategory("World Builder", new List<UIControl> {
            new UIButton("Select Target", () => WorldBuilderPage.SelectTarget()),
            new UIToggle("Placement Mode", false, s => WorldBuilderPage.SetPlacementMode(s)),
            new UIToggle("Freecam Mode", false, s => WorldBuilderPage.SetFreecamMode(s)),
            new UIButton("Save Map", () => WorldBuilderPage.SaveMap()),
            new UIButton("Load Map", () => WorldBuilderPage.LoadMap()),
            new UIButton("Clear Map", () => WorldBuilderPage.ClearMap())
        });

        // HUD
        UICore.RegisterCategory("HUD", new List<UIControl> {
            new UIToggle("Speedometer", false, s => HUDWidgetsPage.ToggleSpeed(s)),
            new UIToggle("RPM Gauge", false, s => HUDWidgetsPage.ToggleRPM(s)),
            new UIToggle("G-Force Meter", false, s => HUDWidgetsPage.ToggleGForce(s)),
            new UIToggle("Compass", false, s => HUDWidgetsPage.ToggleCompass(s)),
            new UIToggle("Health/Armor", false, s => HUDWidgetsPage.ToggleHealthArmor(s)),
            new UIToggle("Damage Indicator", false, s => HUDWidgetsPage.ToggleDamage(s))
        });

        // THEME
        UICore.RegisterCategory("Theme", new List<UIControl> {
            new UIList("Theme", new [] {"Purple","Blue","Red","Gold"},
                i => ThemeManager.SetTheme(i)),
            new UIToggle("Glow Effects", true, s => ThemeManager.ToggleGlow(s)),
            new UIToggle("Sound Effects", true, s => ThemeManager.ToggleSound(s))
        });

        // SUPERPOWERS
        UICore.RegisterCategory("Superpowers", new List<UIControl> {
            new UIToggle("Flash Running", false, s => SuperpowerPage.ToggleFlash(s)),
            new UIToggle("Super Jump", false, s => SuperpowerPage.ToggleSuperJump(s)),
            new UIToggle("Time Slow", false, s => SuperpowerPage.ToggleTimeSlow(s)),
            new UISlider("Strength", 1f, 20f, 1f, v => SuperpowerPage.SetStrength(v))
        });

        // DEVTOOLS
        UICore.RegisterCategory("Dev Tools", new List<UIControl> {
            new UIToggle("Show Coordinates", false, s => DevPage.ToggleCoords(s)),
            new UIButton("Reload Trainer", () => ReloadTrainer())
        });
    }

    private void ReloadTrainer()
    {
        SetupUI();
        UICore.ActiveCategory = 0;
        UICore.ActiveItem = 0;
        NinnyTrainer.UI.SoundManager.PlayOpen();
    }

    private void OnTick(object sender, EventArgs e)
    {
        UICore.Process();

        WeaponModderPage.OnTick();
        WorldBuilderPage.OnTick();
        HUDWidgetsPage.OnTick();
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Insert)
        {
            UICore.ToggleMenu();
            return;
        }

        if (UICore.MenuOpen)
            UICore.HandleInput();
    }
}
