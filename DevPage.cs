using System;
using System.Collections.Generic;
using System.Drawing;
using GTA;
using GTA.Native;
using GTA.UI;
using GTAFullTrainer.UI;
using GTAFullTrainer.Utils;

namespace GTAFullTrainer.Pages
{
    public static class DevToolsPage
    {
        private static bool initialized = false;
        private static List<UIElement> items = new List<UIElement>();

        private static bool debugOverlay = false;
        private static bool highlightEntities = false;
        private static bool showFPS = false;

        private static float timeScale = 1.0f;

        private static Entity selectedEntity = null;

        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            // BASIC DEV FEATURES
            items.Add(new UIToggle("Debug Overlay", 0));
            items.Add(new UIToggle("Highlight Entities", 1));
            items.Add(new UIToggle("Show FPS", 2));

            items.Add(new UISlider("Time Scale", 3, 0.1f, 3.0f, 1.0f));

            // ENTITY INSPECTOR
            items.Add(new UIButton("Select Nearest Entity", 4, () =>
            {
                Entity p = World.GetClosestEntity(Game.Player.Character.Position, 50f,
                    GetEntities());

                if (p != null) selectedEntity = p;
            }));

            items.Add(new UIButton("Teleport To Entity", 5, () =>
            {
                if (selectedEntity != null)
                    Game.Player.Character.Position = selectedEntity.Position + new Vector3(0, 0, 1.0f);
            }));

            items.Add(new UIButton("Delete Selected Entity", 6, () =>
            {
                selectedEntity?.Delete();
                selectedEntity = null;
            }));

            // COORDINATES
            items.Add(new UIButton("Copy Player Coords", 7, () =>
            {
                Vector3 pos = Game.Player.Character.Position;
                System.Windows.Forms.Clipboard.SetText($"{pos.X}, {pos.Y}, {pos.Z}");
            }));

            // MODEL SPAWN BY HASH
            items.Add(new UIButton("Spawn Entity By Hash", 8, () =>
            {
                Game.FadeScreenOut(300);

                string input = Game.GetUserInput(WindowTitle.EnterModel, "", 20);

                if (uint.TryParse(input, out uint hash))
                {
                    Model m = new Model(hash);
                    m.Request(2000);

                    World.CreateProp(m, Game.Player.Character.Position + Game.Player.Character.ForwardVector * 2f,
                        true, true);
                }

                Game.FadeScreenIn(300);
            }));

            // SCRIPT CONTROL
            items.Add(new UIButton("Reload Trainer", 9, () =>
            {
                ScriptDomain.ReloadScriptDomain();
            }));
        }

        public static Entity[] GetEntities()
        {
            List<Entity> output = new List<Entity>();

            foreach (Ped p in World.GetAllPeds())
                if (!p.IsPlayer) output.Add(p);

            foreach (Vehicle v in World.GetAllVehicles())
                output.Add(v);

            foreach (Prop pr in World.GetAllProps())
                output.Add(pr);

            return output.ToArray();
        }

        public static void ApplyLogic()
        {
            debugOverlay = ((UIToggle)items[0]).State;
            highlightEntities = ((UIToggle)items[1]).State;
            showFPS = ((UIToggle)items[2]).State;

            timeScale = ((UISlider)items[3]).Value;
            Game.TimeScale = timeScale;

            // HIGHLIGHT ENTITIES
            if (highlightEntities)
            {
                foreach (Entity e in GetEntities())
                {
                    Function.Call(Hash.SET_ENTITY_ALPHA, e.Handle, 180, false);
                }
            }
            else
            {
                foreach (Entity e in GetEntities())
                {
                    Function.Call(Hash.RESET_ENTITY_ALPHA, e.Handle);
                }
            }

            // DEBUG OVERLAY
            if (debugOverlay)
            {
                Vector3 pos = Game.Player.Character.Position;
                Vector3 rot = Game.Player.Character.Rotation;

                DrawUtils.Text($"Coords: {pos.X:0.00}, {pos.Y:0.00}, {pos.Z:0.00}", 40, 50, 0.45f, Color.White);
                DrawUtils.Text($"Heading: {Game.Player.Character.Heading:0.0}", 40, 90, 0.45f, Color.White);
                DrawUtils.Text($"Rotation: {rot.X:0.0}, {rot.Y:0.0}, {rot.Z:0.0}", 40, 130, 0.45f, Color.White);

                if (selectedEntity != null)
                {
                    DrawUtils.Text($"Selected: {selectedEntity.Model.Hash}", 40, 180, 0.45f, Color.LightGreen);
                }
            }

            // FPS COUNTER
            if (showFPS)
            {
                int fps = (int)(1.0f / Game.LastFrameTime);
                DrawUtils.Text($"FPS: {fps}", 1700, 50, 0.5f, Color.Lime);
            }
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
