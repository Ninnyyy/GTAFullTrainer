using System;
using System.Collections.Generic;
using System.Drawing;
using GTA;
using GTA.Native;
using GTA.Math;
using GTAFullTrainer.UI;
using GTAFullTrainer.Utils;

namespace GTAFullTrainer.Pages
{
    public static class HUDWidgetsPage
    {
        private static bool initialized = false;
        private static List<UIElement> items = new List<UIElement>();

        // Toggles
        private static bool showSpeed = false;
        private static bool showRPM = false;
        private static bool showGForce = false;
        private static bool showCompass = false;
        private static bool showHealthArmor = false;
        private static bool showDamageIndicators = false;

        // Superpower FX
        private static bool showFlashFX = false;
        private static bool showAuraMeter = false;
        private static bool showTimeDilationMeter = false;

        // Settings
        private static float opacity = 1.0f;
        private static float scale = 1.0f;

        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            // BASIC WIDGETS
            items.Add(new UIToggle("Speedometer", 0));
            items.Add(new UIToggle("RPM Gauge", 1));
            items.Add(new UIToggle("G-Force Meter", 2));
            items.Add(new UIToggle("Compass", 3));
            items.Add(new UIToggle("Health/Armor Bar", 4));
            items.Add(new UIToggle("Damage Indicators", 5));

            // SUPERPOWER HUD
            items.Add(new UIToggle("Flash FX", 6));
            items.Add(new UIToggle("Aura Meter", 7));
            items.Add(new UIToggle("Time Dilation Meter", 8));

            // OPTIONS
            items.Add(new UISlider("Opacity", 9, 0.2f, 1.0f, 1.0f));
            items.Add(new UISlider("Scale", 10, 0.6f, 2.0f, 1.0f));
        }

        public static void ApplyLogic()
        {
            showSpeed = ((UIToggle)items[0]).State;
            showRPM = ((UIToggle)items[1]).State;
            showGForce = ((UIToggle)items[2]).State;
            showCompass = ((UIToggle)items[3]).State;
            showHealthArmor = ((UIToggle)items[4]).State;
            showDamageIndicators = ((UIToggle)items[5]).State;

            showFlashFX = ((UIToggle)items[6]).State;
            showAuraMeter = ((UIToggle)items[7]).State;
            showTimeDilationMeter = ((UIToggle)items[8]).State;

            opacity = ((UISlider)items[9]).Value;
            scale = ((UISlider)items[10]).Value;

            Ped player = Game.Player.Character;

            if (showSpeed)
                DrawSpeedometer();

            if (showRPM)
                DrawRPM();

            if (showGForce)
                DrawGForceMeter();

            if (showCompass)
                DrawCompass();

            if (showHealthArmor)
                DrawHealthArmor(player);

            if (showDamageIndicators)
                DrawDamage();

            if (showFlashFX)
                DrawFlashEffects();

            if (showAuraMeter)
                DrawAuraMeter();

            if (showTimeDilationMeter)
                DrawTimeDilationMeter();
        }

        // ============================================================
        //  SPEEDOMETER (DIGITAL)
        // ============================================================
        private static void DrawSpeedometer()
        {
            if (!Game.Player.Character.IsInVehicle()) return;

            Vehicle v = Game.Player.Character.CurrentVehicle;
            float mph = v.Speed * 2.23694f;

            DrawUtils.Text(
                $"{mph:0} MPH",
                1600, 900,
                0.8f * scale,
                Color.FromArgb((int)(opacity * 255), 180, 0, 255));
        }

        // ============================================================
        // RPM GAUGE
        // ============================================================
        private static void DrawRPM()
        {
            if (!Game.Player.Character.IsInVehicle()) return;

            Vehicle v = Game.Player.Character.CurrentVehicle;

            float rpm = Function.Call<float>(Hash.GET_VEHICLE_CURRENT_RPM, v.Handle);
            int max = 100;

            int filled = (int)(rpm * max);

            DrawUtils.Bar(
                new Rectangle(1600, 850, 200, 15),
                filled,
                max,
                Color.FromArgb((int)(opacity * 255), 255, 80, 80),
                Color.FromArgb((int)(opacity * 255), 40, 40, 40));
        }

        // ============================================================
        // G-FORCE
        // ============================================================
        private static Vector3 lastVel = Vector3.Zero;

        private static void DrawGForceMeter()
        {
            if (!Game.Player.Character.IsInVehicle()) return;

            Vehicle v = Game.Player.Character.CurrentVehicle;

            Vector3 vel = v.Velocity;
            float g = (vel - lastVel).Length() / 9.81f;
            lastVel = vel;

            DrawUtils.Text(
                $"{g:0.0} G",
                1600, 820,
                0.7f * scale,
                Color.FromArgb((int)(opacity * 255), 255, 200, 50));
        }

        // ============================================================
        // COMPASS
        // ============================================================
        private static void DrawCompass()
        {
            float heading = Game.Player.Character.Heading;

            DrawUtils.Text(
                $"{heading:000}°",
                960, 50,
                0.8f * scale,
                Color.FromArgb((int)(opacity * 255), 255, 255, 255));
        }

        // ============================================================
        // HEALTH + ARMOR
        // ============================================================
        private static void DrawHealthArmor(Ped p)
        {
            int hp = p.Health;
            int maxHP = p.MaxHealth;

            int armor = p.Armor;

            // Health Bar
            DrawUtils.Bar(
                new Rectangle(50, 950, 300, 18),
                hp, maxHP,
                Color.FromArgb((int)(opacity * 255), 200, 40, 40),
                Color.FromArgb((int)(opacity * 255), 30, 30, 30));

            // Armor Bar
            DrawUtils.Bar(
                new Rectangle(50, 980, 300, 18),
                armor, 100,
                Color.FromArgb((int)(opacity * 255), 40, 100, 200),
                Color.FromArgb((int)(opacity * 255), 30, 30, 30));
        }

        // ============================================================
        // DAMAGE INDICATORS
        // ============================================================
        private static void DrawDamage()
        {
            Ped p = Game.Player.Character;
            if (!p.IsInjured) return;

            DrawUtils.Text(
                "⚠ DAMAGE",
                960, 100,
                1.0f * scale,
                Color.FromArgb((int)(opacity * 255), 255, 50, 50));
        }

        // ============================================================
        // SUPERPOWER FX
        // ============================================================
        private static void DrawFlashEffects()
        {
            if (!Game.Player.Character.IsRunning) return;

            // Motion streaks
            for (int i = 0; i < 4; i++)
            {
                Function.Call(Hash.DRAW_LIGHT_WITH_RANGE,
                    Game.Player.Character.Position.X,
                    Game.Player.Character.Position.Y,
                    Game.Player.Character.Position.Z - i * 0.3f,
                    255, 0, 120,
                    8f, 1.5f);
            }
        }

        private static void DrawAuraMeter()
        {
            DrawUtils.Bar(
                new Rectangle(1600, 600, 200, 12),
                80, 100,
                Color.FromArgb((int)(opacity * 255), 255, 140, 50),
                Color.FromArgb((int)(opacity * 255), 40, 40, 40));
        }

        private static void DrawTimeDilationMeter()
        {
            float t = Game.TimeScale;

            DrawUtils.Bar(
                new Rectangle(1600, 630, 200, 12),
                (int)(t * 100), 100,
                Color.FromArgb((int)(opacity * 255), 80, 180, 255),
                Color.FromArgb((int)(opacity * 255), 40, 40, 40));
        }

        public static void Draw(int index)
        {
            Init();
            ApplyLogic();

            for (int i = 0; i < items.Count; i++)
            {
                items[i].Selected = (i == index);
                items[i].Draw(800, 300 + i * 45);
            }
        }
    }
}
