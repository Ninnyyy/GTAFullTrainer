using System;
using System.Collections.Generic;
using System.Drawing;
using GTA;
using GTA.Native;
using GTAFullTrainer.UI;
using GTAFullTrainer.Utils;

namespace GTAFullTrainer.Pages
{
    public static class HandlingEditorPage
    {
        private static bool initialized = false;
        private static List<UIElement> items = new List<UIElement>();

        // Saved original values
        private static float origMass;
        private static float origDrag;
        private static float origDriveForce;
        private static float origBrakeForce;
        private static float origTraction;
        private static float origSuspensionHeight;
        private static float origDownforce;
        private static float origGravity;

        // Current values
        private static float mass = 1600f;
        private static float drag = 0.22f;
        private static float driveForce = 0.35f;
        private static float brakeForce = 0.35f;
        private static float traction = 1.0f;
        private static float suspensionHeight = 0.0f;
        private static float downforce = 1.0f;
        private static float gravity = 9.8f;

        private static bool driftMode = false;
        private static bool flightMode = false;
        private static bool hoverMode = false;

        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            // MASS
            items.Add(new UISlider("Mass", 0, 200f, 10000f, 1600f));

            // DRAG
            items.Add(new UISlider("Drag", 1, 0.01f, 2.0f, 0.22f));

            // ENGINE POWER
            items.Add(new UISlider("Engine Force", 2, 0.1f, 5.0f, 0.35f));

            // BRAKES
            items.Add(new UISlider("Brake Force", 3, 0.1f, 5.0f, 0.35f));

            // TRACTION / GRIP
            items.Add(new UISlider("Traction", 4, 0.1f, 5.0f, 1.0f));

            // SUSPENSION HEIGHT
            items.Add(new UISlider("Suspension Height", 5, -0.5f, 0.8f, 0.0f));

            // DOWNFORCE
            items.Add(new UISlider("Downforce", 6, 0.1f, 5.0f, 1.0f));

            // GRAVITY
            items.Add(new UISlider("Gravity", 7, 1.0f, 25f, 9.8f));

            // SPECIAL MODES
            items.Add(new UIToggle("Drift Mode", 8));
            items.Add(new UIToggle("Flight Mode", 9));
            items.Add(new UIToggle("Hover Mode", 10));

            // UTILITY BUTTONS
            items.Add(new UIButton("Save Handling Preset", 11, () =>
            {
                SavePreset();
            }));

            items.Add(new UIButton("Load Handling Preset", 12, () =>
            {
                LoadPreset();
            }));

            items.Add(new UIButton("Reset To Default", 13, () =>
            {
                ResetToDefault();
            }));
        }

        public static void ApplyLogic()
        {
            if (!Game.Player.Character.IsInVehicle()) return;

            Vehicle v = Game.Player.Character.CurrentVehicle;

            // INITIALIZE ORIGINAL VALUES ONE TIME
            if (origMass == 0)
            {
                origMass = v.Mass;
                origDrag = v.DragCoefficient;
                origDriveForce = v.EnginePowerMultiplier;
                origBrakeForce = v.BrakeForce;
                origTraction = v.TractionCurveMax;
                origSuspensionHeight = v.SuspensionHeight;
                origDownforce = v.DownforceModifier;
                origGravity = v.GravityScale;
            }

            // Sync UI -> live values
            mass = ((UISlider)items[0]).Value;
            drag = ((UISlider)items[1]).Value;
            driveForce = ((UISlider)items[2]).Value;
            brakeForce = ((UISlider)items[3]).Value;
            traction = ((UISlider)items[4]).Value;
            suspensionHeight = ((UISlider)items[5]).Value;
            downforce = ((UISlider)items[6]).Value;
            gravity = ((UISlider)items[7]).Value;

            driftMode = ((UIToggle)items[8]).State;
            flightMode = ((UIToggle)items[9]).State;
            hoverMode = ((UIToggle)items[10]).State;

            // Apply handling to vehicle continuously
            v.Mass = mass;
            v.DragCoefficient = drag;
            v.EnginePowerMultiplier = driveForce;
            v.BrakeForce = brakeForce;
            v.TractionCurveMax = traction;
            v.SuspensionHeight = suspensionHeight;
            v.DownforceModifier = downforce;
            v.GravityScale = gravity;

            // SPECIAL MODES
            if (driftMode)
            {
                v.GripLevel = 0.15f;
                v.TractionCurveLateral = 0.2f;
            }
            else
            {
                v.GripLevel = traction * 0.5f;
            }

            if (flightMode)
            {
                if (Game.IsKeyPressed(System.Windows.Forms.Keys.W))
                    v.ApplyForce(new Vector3(0, 0, 1.2f));

                if (Game.IsKeyPressed(System.Windows.Forms.Keys.S))
                    v.ApplyForce(new Vector3(0, 0, -1.2f));

                if (Game.IsKeyPressed(System.Windows.Forms.Keys.A))
                    v.ApplyForce(new Vector3(-1.0f, 0, 0));

                if (Game.IsKeyPressed(System.Windows.Forms.Keys.D))
                    v.ApplyForce(new Vector3(1.0f, 0, 0));
            }

            if (hoverMode)
            {
                v.ApplyForce(Vector3.WorldUp * 0.3f);
                v.GravityScale = 0.1f;
            }
        }

        // ===================================
        // PRESET SAVE / LOAD
        // ===================================
        private static void SavePreset()
        {
            try
            {
                string data =
                    $"{mass};{drag};{driveForce};{brakeForce};{traction};{suspensionHeight};{downforce};{gravity}";

                System.IO.File.WriteAllText("HandlingPreset.txt", data);
            }
            catch { }
        }

        private static void LoadPreset()
        {
            try
            {
                if (!System.IO.File.Exists("HandlingPreset.txt")) return;

                string data = System.IO.File.ReadAllText("HandlingPreset.txt");
                string[] parts = data.Split(';');

                float.TryParse(parts[0], out mass);
                float.TryParse(parts[1], out drag);
                float.TryParse(parts[2], out driveForce);
                float.TryParse(parts[3], out brakeForce);
                float.TryParse(parts[4], out traction);
                float.TryParse(parts[5], out suspensionHeight);
                float.TryParse(parts[6], out downforce);
                float.TryParse(parts[7], out gravity);

                // Update UI sliders
                ((UISlider)items[0]).Value = mass;
                ((UISlider)items[1]).Value = drag;
                ((UISlider)items[2]).Value = driveForce;
                ((UISlider)items[3]).Value = brakeForce;
                ((UISlider)items[4]).Value = traction;
                ((UISlider)items[5]).Value = suspensionHeight;
                ((UISlider)items[6]).Value = downforce;
                ((UISlider)items[7]).Value = gravity;
            }
            catch { }
        }

        private static void ResetToDefault()
        {
            mass = origMass;
            drag = origDrag;
            driveForce = origDriveForce;
            brakeForce = origBrakeForce;
            traction = origTraction;
            suspensionHeight = origSuspensionHeight;
            downforce = origDownforce;
            gravity = origGravity;

            ((UISlider)items[0]).Value = mass;
            ((UISlider)items[1]).Value = drag;
            ((UISlider)items[2]).Value = driveForce;
            ((UISlider)items[3]).Value = brakeForce;
            ((UISlider)items[4]).Value = traction;
            ((UISlider)items[5]).Value = suspensionHeight;
            ((UISlider)items[6]).Value = downforce;
            ((UISlider)items[7]).Value = gravity;
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
