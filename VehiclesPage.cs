using System.Collections.Generic;
using System.Drawing;
using GTA;
using GTA.Native;
using GTAFullTrainer.UI;
using GTAFullTrainer.Utils;
using GTAFullTrainer.Core;

namespace GTAFullTrainer.Pages
{
    public static class VehiclesPage
    {
        private static List<UIElement> items = new List<UIElement>();
        private static bool initialized = false;

        // Local states
        private static bool invincibleCar = false;
        private static bool rainbowPaint = false;
        private static bool driftMode = false;
        private static bool freezeCar = false;
        private static bool noCollision = false;
        private static bool neonLights = false;
        private static float turboMultiplier = 1f;

        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            // SPAWN OPTIONS
            items.Add(new UIList("Spawn Vehicle", 0,
                new string[]
                { "Adder", "Zentorno", "Kuruma", "Buffalo", "Police", "Rhino Tank" }));

            items.Add(new UIButton("Spawn & Enter", 1, () =>
            {
                SpawnSelectedVehicle(true);
            }));

            items.Add(new UIButton("Spawn In Front", 2, () =>
            {
                SpawnSelectedVehicle(false);
            }));

            items.Add(new UIButton("Delete Current Vehicle", 3, () =>
            {
                if (Game.Player.Character.IsInVehicle())
                    Game.Player.Character.CurrentVehicle.Delete();
            }));

            items.Add(new UIButton("Teleport Into Nearest Vehicle", 4, () =>
            {
                Vehicle v = World.GetClosestVehicle(Game.Player.Character.Position, 15f);
                if (v != null)
                    Game.Player.Character.SetIntoVehicle(v, VehicleSeat.Driver);
            }));

            // FIX & CLEAN
            items.Add(new UIButton("Repair Vehicle", 5, () =>
            {
                if (Game.Player.Character.IsInVehicle())
                    Game.Player.Character.CurrentVehicle.Repair();
            }));

            items.Add(new UIButton("Clean Vehicle", 6, () =>
            {
                if (Game.Player.Character.IsInVehicle())
                {
                    Vehicle v = Game.Player.Character.CurrentVehicle;
                    Function.Call(Hash.SET_VEHICLE_DIRT_LEVEL, v.Handle, 0f);
                }
            }));

            // TOGGLES
            items.Add(new UIToggle("Invincible Vehicle", 7));
            items.Add(new UIToggle("Neon Lights", 8));
            items.Add(new UIToggle("Rainbow Paint", 9));
            items.Add(new UIToggle("Drift Mode", 10));
            items.Add(new UIToggle("Freeze Vehicle", 11));
            items.Add(new UIToggle("No Collision (Ghost Car)", 12));

            // SLIDERS
            items.Add(new UISlider("Turbo Boost", 13, 1f, 10f, 1f));
            items.Add(new UISlider("Vehicle Gravity", 14, -5f, 5f, 1f));

            // BUTTONS FOR UTILITY
            items.Add(new UIButton("Burst All Tires", 15, () =>
            {
                if (Game.Player.Character.IsInVehicle())
                {
                    Vehicle v = Game.Player.Character.CurrentVehicle;
                    for (int i = 0; i < 8; i++)
                        v.BurstTire(i);
                }
            }));

            items.Add(new UIButton("Full Upgrade Vehicle", 16, () =>
            {
                FullUpgrade();
            }));

            items.Add(new UIButton("Flip Upright", 17, () =>
            {
                if (Game.Player.Character.IsInVehicle())
                {
                    Vehicle v = Game.Player.Character.CurrentVehicle;
                    v.Rotation = new Vector3(0, 0, v.Rotation.Z);
                }
            }));

            items.Add(new UIButton("Vehicle Jump", 18, () =>
            {
                if (Game.Player.Character.IsInVehicle())
                    Game.Player.Character.CurrentVehicle.ApplyForce(Vector3.WorldUp * 100f);
            }));
        }

        // ------------------------------
        // APPLY VEHICLE LOGIC
        // ------------------------------
        public static void ApplyLogic()
        {
            if (!Game.Player.Character.IsInVehicle()) return;

            Vehicle v = Game.Player.Character.CurrentVehicle;

            invincibleCar = ((UIToggle)items[7]).State;
            neonLights = ((UIToggle)items[8]).State;
            rainbowPaint = ((UIToggle)items[9]).State;
            driftMode = ((UIToggle)items[10]).State;
            freezeCar = ((UIToggle)items[11]).State;
            noCollision = ((UIToggle)items[12]).State;
            turboMultiplier = ((UISlider)items[13]).Value;

            float gravity = ((UISlider)items[14]).Value;

            // Invincible vehicle
            if (invincibleCar)
            {
                v.IsInvincible = true;
                v.CanBeVisiblyDamaged = false;
            }
            else
            {
                v.IsInvincible = false;
            }

            // Neon lights
            if (neonLights)
            {
                Function.Call(Hash.SET_VEHICLE_NEON_ENABLED, v.Handle, 0, true);
                Function.Call(Hash.SET_VEHICLE_NEON_ENABLED, v.Handle, 1, true);
                Function.Call(Hash.SET_VEHICLE_NEON_ENABLED, v.Handle, 2, true);
                Function.Call(Hash.SET_VEHICLE_NEON_ENABLED, v.Handle, 3, true);

                Function.Call(Hash._SET_VEHICLE_NEON_LIGHTS_COLOUR, v.Handle, 150, 0, 255);
            }

            // Rainbow Paint
            if (rainbowPaint)
            {
                int r = World.GetRandomInt(0, 255);
                int g = World.GetRandomInt(0, 255);
                int b = World.GetRandomInt(0, 255);
                v.Mods.CustomPrimaryColor = Color.FromArgb(r, g, b);
                v.Mods.CustomSecondaryColor = Color.FromArgb(r, g, b);
            }

            // Drift Mode
            if (driftMode)
                v.TractionCurveLateral = 0.5f;

            // Freeze vehicle
            if (freezeCar)
                v.FreezePosition = true;
            else
                v.FreezePosition = false;

            // No collision
            if (noCollision)
                v.IsCollisionEnabled = false;
            else
                v.IsCollisionEnabled = true;

            // Turbo Boost
            if (Game.IsKeyPressed(System.Windows.Forms.Keys.LShiftKey))
            {
                v.Speed += turboMultiplier;
            }

            // Gravity control
            Function.Call(Hash.SET_VEHICLE_GRAVITY, v.Handle, gravity >= 0);
        }

        // ------------------------------
        // SPAWN VEHICLE
        // ------------------------------
        public static void SpawnSelectedVehicle(bool enter)
        {
            string[] list =
            {
                "adder",
                "zentorno",
                "kuruma",
                "buffalo",
                "police",
                "rhino"
            };

            int idx = ((UIList)items[0]).CurrentIndex;
            string modelName = list[idx];

            Model m = new Model(modelName);
            m.Request(3000);

            Vehicle v = World.CreateVehicle(m,
                Game.Player.Character.Position + Game.Player.Character.ForwardVector * 5f);

            if (enter)
                Game.Player.Character.SetIntoVehicle(v, VehicleSeat.Driver);
        }

        // ------------------------------
        // FULL UPGRADE
        // ------------------------------
        public static void FullUpgrade()
        {
            if (!Game.Player.Character.IsInVehicle()) return;

            Vehicle v = Game.Player.Character.CurrentVehicle;

            v.Mods.InstallModKit();

            v.Mods.EngineLevel = 3;
            v.Mods.BrakesLevel = 2;
            v.Mods.TransmissionLevel = 2;
            v.Mods.SuspensionLevel = 2;
            v.Mods.ArmorLevel = 4;
            v.Mods.TurboInstalled = true;

            Function.Call(Hash.SET_VEHICLE_MOD_KIT, v.Handle, 0);
        }

        // ------------------------------
        // DRAW
        // ------------------------------
        public static void Draw(int index)
        {
            Init();
            ApplyLogic();

            for (int i = 0; i < items.Count; i++)
            {
                items[i].Selected = (i == index);
                items[i].Draw(800, 300 + (i * 45));
            }
        }
    }
}
