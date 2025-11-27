using System.Collections.Generic;
using System.Drawing;
using GTA;
using GTA.Native;
using GTAFullTrainer.UI;
using GTAFullTrainer.Utils;

namespace GTAFullTrainer.Pages
{
    public static class CameraPage
    {
        private static bool initialized = false;
        private static List<UIElement> items = new List<UIElement>();

        private static bool freecamEnabled = false;
        private static bool hideHUD = false;
        private static bool cinematicBars = false;

        private static bool nightVision = false;
        private static bool thermalVision = false;

        private static bool slowMotion = false;

        private static Camera freeCam;
        private static float freecamSpeed = 1.0f;
        private static float fovValue = 60f;

        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            // MAIN CAM FEATURES
            items.Add(new UIToggle("Freecam (noclip)", 0));
            items.Add(new UISlider("Freecam Speed", 1, 0.5f, 10f, 1f));
            items.Add(new UISlider("Camera FOV", 2, 30f, 120f, 60f));

            // VISUALS
            items.Add(new UIToggle("Night Vision", 3));
            items.Add(new UIToggle("Thermal Vision", 4));

            // SCREEN EFFECTS
            items.Add(new UIToggle("Hide HUD", 5));
            items.Add(new UIToggle("Cinematic Bars", 6));
            items.Add(new UIToggle("Slow Motion", 7));

            // UTILITIES
            items.Add(new UIButton("Small Camera Shake", 8, () =>
            {
                GameplayCamera.Shake(CameraShake.Hand, 0.7f);
            }));

            items.Add(new UIButton("Big Camera Shake", 9, () =>
            {
                GameplayCamera.Shake(CameraShake.LargeExplosion, 2f);
            }));

            items.Add(new UIButton("Freeze Time", 10, () =>
            {
                Function.Call(Hash.PAUSE_CLOCK, true);
            }));

            items.Add(new UIButton("Unfreeze Time", 11, () =>
            {
                Function.Call(Hash.PAUSE_CLOCK, false);
            }));
        }

        public static void ApplyLogic()
        {
            // READ UI VALUES
            freecamEnabled = ((UIToggle)items[0]).State;
            freecamSpeed = ((UISlider)items[1]).Value;
            fovValue = ((UISlider)items[2]).Value;

            nightVision = ((UIToggle)items[3]).State;
            thermalVision = ((UIToggle)items[4]).State;

            hideHUD = ((UIToggle)items[5]).State;
            cinematicBars = ((UIToggle)items[6]).State;

            slowMotion = ((UIToggle)items[7]).State;

            // APPLY VISUAL STATES
            Function.Call(Hash.SET_NIGHTVISION, nightVision);
            Function.Call(Hash.SET_SEETHROUGH, thermalVision);

            // HUD
            if (hideHUD)
            {
                Function.Call(Hash.DISPLAY_HUD, false);
                Function.Call(Hash.DISPLAY_RADAR, false);
            }
            else
            {
                Function.Call(Hash.DISPLAY_HUD, true);
                Function.Call(Hash.DISPLAY_RADAR, true);
            }

            // CINEMATIC BARS
            if (cinematicBars)
            {
                new UIResRectangle(new Point(0, 0), new Size(1920, 150), Color.FromArgb(200, 0, 0, 0)).Draw();
                new UIResRectangle(new Point(0, 930), new Size(1920, 150), Color.FromArgb(200, 0, 0, 0)).Draw();
            }

            // SLOW MOTION
            if (slowMotion)
                Game.TimeScale = 0.3f;
            else
                Game.TimeScale = 1.0f;

            // FREECAM SYSTEM
            if (freecamEnabled)
                EnableFreecam();
            else
                DisableFreecam();
        }

        // =====================================
        // FREECAM SYSTEM
        // =====================================
        private static void EnableFreecam()
        {
            Ped player = Game.Player.Character;

            if (freeCam == null)
            {
                freeCam = World.CreateCamera(GameplayCamera.Position, GameplayCamera.Rotation, fovValue);
                World.RenderingCamera = freeCam;
                player.FreezePosition = true;
            }

            // Update FOV
            freeCam.FieldOfView = fovValue;

            float speed = freecamSpeed;

            // MOVEMENT (WASD + SPACE + CTRL)
            if (Game.IsKeyPressed(System.Windows.Forms.Keys.W))
                freeCam.Position += freeCam.Direction * speed;

            if (Game.IsKeyPressed(System.Windows.Forms.Keys.S))
                freeCam.Position -= freeCam.Direction * speed;

            if (Game.IsKeyPressed(System.Windows.Forms.Keys.A))
                freeCam.Position -= freeCam.RightVector * speed;

            if (Game.IsKeyPressed(System.Windows.Forms.Keys.D))
                freeCam.Position += freeCam.RightVector * speed;

            if (Game.IsKeyPressed(System.Windows.Forms.Keys.Space))
                freeCam.Position += Vector3.WorldUp * speed;

            if (Game.IsKeyPressed(System.Windows.Forms.Keys.LControlKey))
                freeCam.Position -= Vector3.WorldUp * speed;

            // ROTATION (mouse control)
            float pitch = GameplayCamera.Rotation.X;
            float yaw = GameplayCamera.Rotation.Z;

            freeCam.Rotation = new Vector3(pitch, 0, yaw);
        }

        private static void DisableFreecam()
        {
            Ped player = Game.Player.Character;

            if (freeCam != null)
            {
                World.RenderingCamera = null;
                freeCam.Delete();
                freeCam = null;
                player.FreezePosition = false;
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
