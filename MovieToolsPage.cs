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
    public static class MovieToolsPage
    {
        private static bool initialized = false;
        private static List<UIElement> items = new List<UIElement>();

        // Keyframe data structure
        private class Keyframe
        {
            public Vector3 Position;
            public Vector3 Rotation;
            public float FOV;
            public float Time; // seconds
        }

        private static List<Keyframe> keyframes = new List<Keyframe>();
        private static bool playback = false;
        private static float playbackTimer = 0f;
        private static Camera movieCam;

        // CAMERA PRESETS
        private static bool orbitMode = false;
        private static bool followPlayer = false;

        // SCENE BUILDER
        private static List<Entity> sceneEntities = new List<Entity>();

        // STATE
        private static float orbitSpeed = 0.7f;

        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            // CAMERA KEYFRAME FUNCTIONS
            items.Add(new UIButton("Add Keyframe", 0, () =>
            {
                AddKeyframe();
            }));

            items.Add(new UIButton("Clear Keyframes", 1, () =>
            {
                keyframes.Clear();
            }));

            items.Add(new UIButton("Play Keyframes", 2, () =>
            {
                StartPlayback();
            }));

            items.Add(new UIButton("Stop Playback", 3, () =>
            {
                StopPlayback();
            }));

            // CAMERA PRESETS
            items.Add(new UIToggle("Orbit Player", 4));
            items.Add(new UISlider("Orbit Speed", 5, 0.2f, 3f, 0.7f));

            items.Add(new UIToggle("Follow Player", 6));

            // SCENE BUILDER
            items.Add(new UIButton("Save Scene", 7, () =>
            {
                SaveScene();
            }));

            items.Add(new UIButton("Load Scene", 8, () =>
            {
                LoadScene();
            }));

            items.Add(new UIButton("Clear Scene Entities", 9, () =>
            {
                foreach (var e in sceneEntities)
                    if (e.Exists()) e.Delete();

                sceneEntities.Clear();
            }));

            // NPC Animation Tools
            items.Add(new UIButton("Play NPC Animation", 10, () =>
            {
                PlayAnimationPrompt();
            }));

            // VISUALS
            items.Add(new UIToggle("Letterbox Bars", 11));
            items.Add(new UIToggle("Freeze World Time", 12));
            items.Add(new UIToggle("Hide HUD", 13));
        }

        // ========================================
        // KEYFRAME FUNCTIONS
        // ========================================
        private static void AddKeyframe()
        {
            Vector3 pos = GameplayCamera.Position;
            Vector3 rot = GameplayCamera.Rotation;
            float fov = GameplayCamera.FieldOfView;

            float t = keyframes.Count == 0 ? 0 : keyframes[keyframes.Count - 1].Time + 2f; // 2 seconds apart

            keyframes.Add(new Keyframe()
            {
                Position = pos,
                Rotation = rot,
                FOV = fov,
                Time = t
            });
        }

        private static void StartPlayback()
        {
            if (keyframes.Count < 2) return;

            playback = true;
            playbackTimer = 0f;

            if (movieCam != null)
                movieCam.Delete();

            movieCam = World.CreateCamera(keyframes[0].Position, keyframes[0].Rotation, keyframes[0].FOV);
            World.RenderingCamera = movieCam;
        }

        private static void StopPlayback()
        {
            playback = false;

            if (movieCam != null)
            {
                World.RenderingCamera = null;
                movieCam.Delete();
                movieCam = null;
            }
        }

        private static void UpdatePlayback()
        {
            if (!playback) return;

            playbackTimer += Game.LastFrameTime;

            float totalTime = keyframes[keyframes.Count - 1].Time;

            if (playbackTimer > totalTime)
            {
                StopPlayback();
                return;
            }

            // Find keyframes to interpolate between
            Keyframe k1 = keyframes[0];
            Keyframe k2 = keyframes[keyframes.Count - 1];

            for (int i = 0; i < keyframes.Count - 1; i++)
            {
                if (playbackTimer >= keyframes[i].Time && playbackTimer <= keyframes[i + 1].Time)
                {
                    k1 = keyframes[i];
                    k2 = keyframes[i + 1];
                    break;
                }
            }

            float segmentTime = playbackTimer - k1.Time;
            float segmentDuration = k2.Time - k1.Time;
            float t = segmentTime / segmentDuration;
            t = SmoothStep(t);

            // Interpolate
            Vector3 pos = Vector3.Lerp(k1.Position, k2.Position, t);
            Vector3 rot = Vector3.Lerp(k1.Rotation, k2.Rotation, t);
            float fov = Lerp(k1.FOV, k2.FOV, t);

            movieCam.Position = pos;
            movieCam.Rotation = rot;
            movieCam.FieldOfView = fov;
        }

        private static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        private static float SmoothStep(float t)
        {
            return t * t * (3 - 2 * t);
        }

        // ========================================
        // CAMERA PRESETS
        // ========================================
        private static void UpdateCameraPresets()
        {
            orbitMode = ((UIToggle)items[4]).State;
            orbitSpeed = ((UISlider)items[5]).Value;
            followPlayer = ((UIToggle)items[6]).State;

            Ped player = Game.Player.Character;

            if (orbitMode)
            {
                if (movieCam == null)
                {
                    movieCam = World.CreateCamera(GameplayCamera.Position, GameplayCamera.Rotation, 60f);
                    World.RenderingCamera = movieCam;
                }

                float angle = (float)Game.GameTime / (2000f / orbitSpeed);
                Vector3 offset = new Vector3((float)Math.Cos(angle) * 5f, (float)Math.Sin(angle) * 5f, 2f);

                movieCam.Position = player.Position + offset;
                movieCam.PointAt(player);
            }

            if (followPlayer)
            {
                if (movieCam == null)
                {
                    movieCam = World.CreateCamera(GameplayCamera.Position, GameplayCamera.Rotation, 60f);
                    World.RenderingCamera = movieCam;
                }

                Vector3 behind = player.Position - player.ForwardVector * 4f + new Vector3(0, 0, 1.4f);

                movieCam.Position = Vector3.Lerp(movieCam.Position, behind, 0.2f);
                movieCam.PointAt(player);
            }

            if (!orbitMode && !followPlayer && !playback)
            {
                if (movieCam != null)
                {
                    World.RenderingCamera = null;
                    movieCam.Delete();
                    movieCam = null;
                }
            }
        }

        // ========================================
        // SCENE BUILDER
        // ========================================
        private static void SaveScene()
        {
            List<string> lines = new List<string>();

            foreach (var e in sceneEntities)
            {
                if (e.Exists())
                {
                    Vector3 p = e.Position;
                    Vector3 r = e.Rotation;
                    lines.Add($"{e.Model.Hash};{p.X};{p.Y};{p.Z};{r.X};{r.Y};{r.Z}");
                }
            }

            System.IO.File.WriteAllLines("SavedScene.txt", lines.ToArray());
        }

        private static void LoadScene()
        {
            if (!System.IO.File.Exists("SavedScene.txt")) return;

            foreach (var e in sceneEntities)
                if (e.Exists()) e.Delete();

            sceneEntities.Clear();

            string[] lines = System.IO.File.ReadAllLines("SavedScene.txt");

            foreach (string line in lines)
            {
                string[] parts = line.Split(';');

                if (parts.Length == 7)
                {
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

                    sceneEntities.Add(p);
                }
            }
        }

        private static void PlayAnimationPrompt()
        {
            Ped target = World.GetClosestPed(Game.Player.Character.Position, 10f);

            if (target == null || target.IsPlayer) return;

            string dict = Game.GetUserInput(WindowTitle.EnterMessage, "anim_dict", 30);
            string anim = Game.GetUserInput(WindowTitle.EnterMessage, "anim_name", 30);

            Function.Call(Hash.REQUEST_ANIM_DICT, dict);

            Script.Wait(800);

            target.Task.PlayAnimation(dict, anim, 8f, -8f, -1, AnimationFlags.Loop, 0);
        }

        public static void ApplyLogic()
        {
            UpdatePlayback();
            UpdateCameraPresets();

            // LETTERBOX BARS
            if (((UIToggle)items[11]).State)
            {
                new UIResRectangle(new Point(0, 0), new Size(1920, 140), Color.FromArgb(200, 0, 0, 0)).Draw();
                new UIResRectangle(new Point(0, 940), new Size(1920, 140), Color.FromArgb(200, 0, 0, 0)).Draw();
            }

            // FREEZE WORLD TIME
            if (((UIToggle)items[12]).State)
            {
                Function.Call(Hash.PAUSE_CLOCK, true);
            }
            else
            {
                Function.Call(Hash.PAUSE_CLOCK, false);
            }

            // HIDE HUD
            if (((UIToggle)items[13]).State)
            {
                Function.Call(Hash.DISPLAY_HUD, false);
                Function.Call(Hash.DISPLAY_RADAR, false);
            }
            else
            {
                Function.Call(Hash.DISPLAY_HUD, true);
                Function.Call(Hash.DISPLAY_RADAR, true);
            }
        }

        public static void Draw(int index)
        {
            Init();

            ApplyLogic();

            for (int i = 0; i < items.Count; i++)
            {
                items[i].Selected = i == index;
                items[i].Draw(800, 300 + i * 45);
            }
        }
    }
}
