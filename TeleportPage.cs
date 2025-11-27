using System.Collections.Generic;
using System.Drawing;
using GTA;
using GTA.Native;
using GTAFullTrainer.UI;
using GTAFullTrainer.Utils;
using GTAFullTrainer.Core;

namespace GTAFullTrainer.Pages
{
    public static class TeleportPage
    {
        private static List<UIElement> items = new List<UIElement>();
        private static bool initialized = false;

        // Saved coords
        private static Vector3 savedCoord = Vector3.Zero;

        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            // BASIC TELEPORTS
            items.Add(new UIButton("Teleport To Waypoint", 0, () =>
            {
                if (Game.IsWaypointActive)
                {
                    Vector3 wp = World.GetWaypointPosition();
                    Game.Player.Character.Position = new Vector3(wp.X, wp.Y, wp.Z + 2f);
                }
            }));

            items.Add(new UIButton("Teleport Forward 10m", 1, () =>
            {
                Ped p = Game.Player.Character;
                p.Position += p.ForwardVector * 10f;
            }));

            items.Add(new UIButton("Teleport Up 10m", 2, () =>
            {
                Ped p = Game.Player.Character;
                p.Position += new Vector3(0, 0, 10f);
            }));

            items.Add(new UIButton("Teleport Into Nearest Car", 3, () =>
            {
                Vehicle v = World.GetClosestVehicle(Game.Player.Character.Position, 20f);
                if (v != null)
                    Game.Player.Character.SetIntoVehicle(v, VehicleSeat.Driver);
            }));

            // SAVED LOCATION OPTIONS
            items.Add(new UIButton("Save Current Location", 4, () =>
            {
                savedCoord = Game.Player.Character.Position;
            }));

            items.Add(new UIButton("Teleport To Saved Location", 5, () =>
            {
                if (savedCoord != Vector3.Zero)
                    Game.Player.Character.Position = savedCoord;
            }));

            // PRESET LOCATIONS
            items.Add(new UIList("Landmarks", 6,
                new string[]
                {
                    "None",
                    "Mount Chiliad",
                    "Airport",
                    "Maze Bank Tower",
                    "Military Base",
                    "Beach",
                    "Vespucci",
                    "Sandy Shores",
                    "Paleto Bay"
                }));

            // INTERIOR LOCATIONS
            items.Add(new UIList("Interiors", 7,
                new string[]
                {
                    "None",
                    "Michael's House",
                    "Franklin's House",
                    "Trevor's Trailer",
                    "Police Station",
                    "Hospital",
                    "Strip Club",
                    "Ammu-Nation",
                    "Nightclub"
                }));
        }

        public static void ApplyLogic()
        {
            // LANDMARK TELEPORTS
            if (InputManager.NavSelect() && items[6].Selected)
            {
                Vector3 pos = Vector3.Zero;

                switch (((UIList)items[6]).CurrentIndex)
                {
                    case 1: pos = new Vector3(500f, 5596f, 800f); break;        // Chiliad
                    case 2: pos = new Vector3(-1034f, -2733f, 20f); break;      // Airport
                    case 3: pos = new Vector3(-67f, -818f, 325f); break;        // Maze Bank
                    case 4: pos = new Vector3(-2070f, 3135f, 32f); break;       // Military Base
                    case 5: pos = new Vector3(-1600f, -1200f, 1f); break;       // Beach
                    case 6: pos = new Vector3(-1150f, -1500f, 4f); break;       // Vespucci
                    case 7: pos = new Vector3(1700f, 3600f, 35f); break;        // Sandy Shores
                    case 8: pos = new Vector3(-100f, 6500f, 31f); break;        // Paleto Bay
                }

                if (pos != Vector3.Zero)
                    Game.Player.Character.Position = pos;
            }

            // INTERIOR TELEPORTS
            if (InputManager.NavSelect() && items[7].Selected)
            {
                Vector3 pos = Vector3.Zero;

                switch (((UIList)items[7]).CurrentIndex)
                {
                    case 1: pos = new Vector3(-813.6f, 179.4f, 72.1f); break;  // Michael
                    case 2: pos = new Vector3(10.2f, 545.9f, 174.7f); break;   // Franklin
                    case 3: pos = new Vector3(1973f, 3818f, 33.5f); break;     // Trevor
                    case 4: pos = new Vector3(440f, -979f, 30f); break;        // Police
                    case 5: pos = new Vector3(298f, -584f, 43f); break;        // Hospital
                    case 6: pos = new Vector3(127f, -1298f, 29f); break;       // Strip club
                    case 7: pos = new Vector3(24f, -1106f, 29f); break;        // Ammu-Nation
                    case 8: pos = new Vector3(-1615f, -3013f, -75f); break;    // Nightclub
                }

                if (pos != Vector3.Zero)
                {
                    Game.Player.Character.Position = pos;
                }
            }
        }

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
