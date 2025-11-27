using GTA;
using GTA.Native;
using GTA.Math;
using System.Collections.Generic;

namespace NinnyTrainer.Pages
{
    public static class WorldBuilderPage
    {
        private static bool placementMode = false;
        private static bool freecam = false;
        private static Entity selected = null;

        private static List<Vector3> savedPositions = new();

        public static void SetPlacementMode(bool state)
        {
            placementMode = state;
        }

        public static void SetFreecam(bool state)
        {
            freecam = state;
        }

        public static void SelectTarget()
        {
            selected = World.GetClosestEntity(Game.Player.Character.Position, 20f);
        }

        public static void SaveMap()
        {
            savedPositions.Clear();

            foreach (var v in World.GetAllProps())
                savedPositions.Add(v.Position);

            Game.PlaySound("SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET");
        }

        public static void LoadMap()
        {
            foreach (var pos in savedPositions)
                World.CreateProp("prop_barrel_02a", pos, true, true);
        }

        public static void ClearMap()
        {
            foreach (var v in World.GetAllProps())
                v.Delete();
        }

        public static void OnTick()
        {
            if (placementMode && selected != null)
            {
                Vector3 newPos = Game.Player.Character.Position + Game.Player.Character.ForwardVector * 3f;
                selected.Position = newPos;
            }
        }
    }
}
