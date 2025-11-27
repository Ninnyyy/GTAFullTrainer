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
    public static class WorldBuilderPage
    {
        private static bool initialized = false;
        private static List<UIElement> items = new List<UIElement>();

        // Editor state
        private static Entity selected = null;
        private static bool placementMode = false;
        private static bool freecamMode = false;
        private static bool gridSnap = false;

        private static float moveSpeed = 0.3f;
        private static float rotateSpeed = 2f;
        private static float gridSize = 0.5f;

        // For multi-select
        private static List<Entity> selectionGroup = new List<Entity>();

        // For save/load
        private static string mapFile = "CustomMap.txt";

        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            // ENTITY SELECTION
            items.Add(new UIButton("Select Target (Crosshair)", 0, () =>
            {
                SelectTarget();
            }));

            items.Add(new UIButton("Select Nearest Prop", 1, () =>
            {
                selected = World.GetClosest<Prop>(Game.Player.Character.Position, 15f);
            }));

            items.Add(new UIButton("Duplicate Selected", 2, () =>
            {
                DuplicateSelected();
            }));

            items.Add(new UIButton("Delete Selected", 3, () =>
            {
                DeleteSelected();
            }));

            // MOVEMENT
            items.Add(new UISlider("Move Speed", 4, 0.1f, 2f, 0.3f));
            items.Add(new UISlider("Rotate Speed", 5, 1f, 20f, 2f));

            items.Add(new UIToggle("Grid Snap", 6));
            items.Add(new UISlider("Grid Size", 7, 0.1f, 5f, 0.5f));

            // MODES
            items.Add(new UIToggle("Placement Mode", 8));
            items.Add(new UIToggle("Freecam Build Mode", 9));

            // SAVE / LOAD MAP
            items.Add(new UIButton("Save Map", 10, () => SaveMap()));
            items.Add(new UIButton("Load Map", 11, () => LoadMap()));
            items.Add(new UIButton("Clear Map", 12, () => ClearMap()));

            // LIGHTS
            items.Add(new UIButton("Place Light", 13, () => PlaceLight()));
        }

        public static void ApplyLogic()
        {
            moveSpeed = ((UISlider)items[4]).Value;
            rotateSpeed = ((UISlider)items[5]).Value;

            gridSnap = ((UIToggle)items[6]).State;
            gridSize = ((UISlider)items[7]).Value;

            placementMode = ((UIToggle)items[8]).State;
            freecamMode = ((UIToggle)items[9]).State;

            // FREECAM MODE
            if (freecamMode)
                World.RenderingCamera = GameplayCamera;

            if (!freecamMode && World.RenderingCamera == GameplayCamera)
                World.RenderingCamera = null;

            // PLACEMENT MODE
            if (placementMode)
                HandlePlacementMode();

            // MAIN EDITOR CONTROLS
            if (selected != null && selected.Exists())
                HandleEntityEdit(selected);
        }

        private static void HandlePlacementMode()
        {
            Vector3 placePos = GameplayCamera.Position + GameplayCamera.Direction * 4f;

            // Ghost preview
            Function.Call(Hash.DRAW_MARKER, 28, placePos.X, placePos.Y, placePos.Z,
                0, 0, 0, 0, 0, 0,
                0.3f, 0.3f, 0.3f,
                80, 80, 255, 150, false, true, 2, false, 0, 0, false);

            // Place with LMB
            if (Game.IsKeyPressed(System.Windows.Forms.Keys.LButton))
            {
                Model m = new Model("prop_barrel_02a");
                m.Request(500);

                Prop p = World.CreateProp(m, placePos, true, true);
                if (gridSnap)
                    p.Position = SnapToGrid(p.Position);

                selected = p;
            }
        }

        private static void SelectTarget()
        {
            Vector3 from = GameplayCamera.Position;
            Vector3 to = from + GameplayCamera.Direction * 200f;

            RaycastResult r = World.Raycast(from, to, 200f);

            if (r.DitHitEntity)
                selected = r.HitEntity;
        }

        private static void HandleEntityEdit(Entity ent)
        {
            // MOVE X/Y/Z
            if (Game.IsKeyPressed(System.Windows.Forms.Keys.NumPad8)) // forward
                Move(ent, ent.ForwardVector * moveSpeed);

            if (Game.IsKeyPressed(System.Windows.Forms.Keys.NumPad2)) // backward
                Move(ent, -ent.ForwardVector * moveSpeed);

            if (Game.IsKeyPressed(System.Windows.Forms.Keys.NumPad4)) // left
                Move(ent, -ent.RightVector * moveSpeed);

            if (Game.IsKeyPressed(System.Windows.Forms.Keys.NumPad6)) // right
                Move(ent, ent.RightVector * moveSpeed);

            if (Game.IsKeyPressed(System.Windows.Forms.Keys.NumPad9)) // up
                Move(ent, Vector3.WorldUp * moveSpeed);

            if (Game.IsKeyPressed(System.Windows.Forms.Keys.NumPad3)) // down
                Move(ent, -Vector3.WorldUp * moveSpeed);

            // ROTATION
            Vector3 rot = ent.Rotation;

            if (Game.IsKeyPressed(System.Windows.Forms.Keys.NumPad7))
                rot.Z += rotateSpeed;

            if (Game.IsKeyPressed(System.Windows.Forms.Keys.NumPad1))
                rot.Z -= rotateSpeed;

            if (Game.IsKeyPressed(System.Windows.Forms.Keys.NumPad5))
                rot.X += rotateSpeed;

            if (Game.IsKeyPressed(System.Windows.Forms.Keys.NumPad0))
                rot.X -= rotateSpeed;

            ent.Rotation = rot;

            // SNAP TO GROUND
            if (Game.IsKeyPressed(System.Windows.Forms.Keys.G))
                SnapToGround(ent);

            // ADD TO MULTI-SELECTION
            if (Game.IsKeyPressed(System.Windows.Forms.Keys.M))
                selectionGroup.Add(ent);

            // DISPLAY INFO
            DrawEntityInfo(ent);
        }

        private static void Move(Entity e, Vector3 offset)
        {
            Vector3 newPos = e.Position + offset;

            if (gridSnap)
                newPos = SnapToGrid(newPos);

            e.Position = newPos;

            // Move multi-selected
            foreach (Entity ent in selectionGroup)
                if (ent.Exists() && ent != e)
                    ent.Position += offset;
        }

        private static Vector3 SnapToGrid(Vector3 pos)
        {
            return new Vector3(
                (float)Math.Round(pos.X / gridSize) * gridSize,
                (float)Math.Round(pos.Y / gridSize) * gridSize,
                (float)Math.Round(pos.Z / gridSize) * gridSize
            );
        }

        private static void SnapToGround(Entity e)
        {
            Vector3 pos = e.Position;
            float groundZ;
            World.GetGroundHeight(pos, out groundZ);

            e.Position = new Vector3(pos.X, pos.Y, groundZ);
        }

        private static void DuplicateSelected()
        {
            if (selected == null || !selected.Exists()) return;

            Entity copy = null;

            if (selected is Prop)
                copy = World.CreateProp(selected.Model, selected.Position + new Vector3(0, 1, 0), true, true);
            else if (selected is Vehicle)
                copy = World.CreateVehicle(selected.Model, selected.Position + new Vector3(0, 1, 0));
            else if (selected is Ped)
                copy = World.CreatePed(selected.Model, selected.Position + new Vector3(0, 1, 0));

            if (copy != null)
                selected = copy;
        }

        private static void DeleteSelected()
        {
            if (selected != null && selected.Exists())
                selected.Delete();

            selected = null;
        }

        private static void PlaceLight()
        {
            Vector3 pos = GameplayCamera.Position + GameplayCamera.Direction * 3f;

            Function.Call(Hash.DRAW_LIGHT_WITH_RANGE,
                pos.X, pos.Y, pos.Z,
                255, 200, 150,
                12f, 5f);
        }

        // MAP SAVE/LOAD
        private static void SaveMap()
        {
            List<string> lines = new List<string>();

            foreach (Prop p in World.GetAllProps())
            {
                if (p.Position.DistanceTo(Game.Player.Character.Position) < 200f)
                {
                    Vector3 pos = p.Position;
                    Vector3 rot = p.Rotation;

                    lines.Add($"{p.Model.Hash};{pos.X};{pos.Y};{pos.Z};{rot.X};{rot.Y};{rot.Z}");
                }
            }

            System.IO.File.WriteAllLines(mapFile, lines.ToArray());
        }

        private static void LoadMap()
        {
            if (!System.IO.File.Exists(mapFile)) return;

            string[] lines = System.IO.File.ReadAllLines(mapFile);

            foreach (string line in lines)
            {
                string[] parts = line.Split(';');

                uint hash = uint.Parse(parts[0]);
                float px = float.Parse(parts[1]);
                float py = float.Parse(parts[2]);
                float pz = float.Parse(parts[3]);
                float rx = float.Parse(parts[4]);
                float ry = float.Parse(parts[5]);
                float rz = float.Parse(parts[6]);

                Model m = new Model(hash);
                m.Request(1000);

                Prop p = World.CreateProp(m, new Vector3(px, py, pz), true, true);
                p.Rotation = new Vector3(rx, ry, rz);
            }
        }

        private static void ClearMap()
        {
            foreach (Prop p in World.GetAllProps())
            {
                if (p.Position.DistanceTo(Game.Player.Character.Position) < 200f)
                    p.Delete();
            }
        }

        private static void DrawEntityInfo(Entity e)
        {
            Vector3 pos = e.Position;
            Vector3 rot = e.Rotation;

            DrawUtils.Text(
                $"Selected: {e.Model.Hash}\n" +
                $"Pos: {pos.X:F2}, {pos.Y:F2}, {pos.Z:F2}\n" +
                $"Rot: {rot.X:F2}, {rot.Y:F2}, {rot.Z:F2}",
                40, 400, 0.45f,
                Color.White);
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
