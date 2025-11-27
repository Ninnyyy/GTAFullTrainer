using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GTA;

namespace GTAFullTrainer.Core
{
    public static class InputManager
    {
        private static Dictionary<Keys, bool> keyStates = new Dictionary<Keys, bool>();
        private static bool capturingKey = false;
        private static Action<Keys> keyCapturedCallback;

        // Navigation delay
        private static int navCooldown = 150;
        private static int lastNavTime = 0;

        public static void Initialize()
        {
            // Preload states for all possible keys
            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                if (!keyStates.ContainsKey(key))
                    keyStates[key] = false;
            }
        }

        public static void Update()
        {
            // Update key states every tick
            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                bool isDown = Game.IsKeyPressed(key);

                if (!keyStates[key] && isDown)
                {
                    keyStates[key] = true;

                    // If we're capturing a key, process it
                    if (capturingKey)
                    {
                        capturingKey = false;
                        keyCapturedCallback?.Invoke(key);
                        return;
                    }
                }
                else if (!isDown)
                {
                    keyStates[key] = false;
                }
            }
        }

        public static bool IsPressed(Keys key)
        {
            bool isDown = Game.IsKeyPressed(key);
            if (isDown && !keyStates[key])
            {
                keyStates[key] = true;
                return true;
            }
            return false;
        }

        public static bool NavUp()
        {
            if (Game.IsKeyPressed(Keys.Up))
                return CanNavigate();
            return false;
        }

        public static bool NavDown()
        {
            if (Game.IsKeyPressed(Keys.Down))
                return CanNavigate();
            return false;
        }

        public static bool NavLeft()
        {
            if (Game.IsKeyPressed(Keys.Left))
                return CanNavigate();
            return false;
        }

        public static bool NavRight()
        {
            if (Game.IsKeyPressed(Keys.Right))
                return CanNavigate();
            return false;
        }

        public static bool NavSelect()
        {
            return IsPressed(Keys.Enter);
        }

        public static bool NavBack()
        {
            return IsPressed(Keys.Back);
        }

        private static bool CanNavigate()
        {
            int now = Game.GameTime;
            if (now - lastNavTime > navCooldown)
            {
                lastNavTime = now;
                return true;
            }
            return false;
        }

        // Start capturing next key press
        public static void CaptureKey(Action<Keys> callback)
        {
            capturingKey = true;
            keyCapturedCallback = callback;
            UI.ShowSubtitle("~p~Press any key...", 2000);
        }
    }
}
