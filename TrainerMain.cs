using GTA;
using GTA.UI;
using System;
using System.Windows.Forms;

namespace GTAFullTrainer.Core
{
    public class TrainerMain : Script
    {
        private Keys openMenuKey = Keys.Insert;
        private bool menuVisible = false;
        private bool autoOpenOnStart = true;
        private bool autoOpenTriggered = false;

        public static TrainerMain Instance;

        public TrainerMain()
        {
            Instance = this;

            TrainerLogger.Initialize();

            Tick += OnTick;
            KeyUp += OnKeyUp;

            ConfigManager.Load();

            string loadedKey = ConfigManager.Get("OpenMenuKey", Keys.Insert.ToString());
            string autoOpenValue = ConfigManager.Get("AutoOpenMenuOnStart", "true");

            if (!bool.TryParse(autoOpenValue, out autoOpenOnStart))
            {
                autoOpenOnStart = true;
            }

            if (Enum.TryParse(loadedKey, out Keys parsedKey))
            {
                openMenuKey = parsedKey;
            }
            else
            {
                openMenuKey = Keys.Insert;
            }

            ConfigManager.Set("OpenMenuKey", openMenuKey.ToString());
            ConfigManager.Set("AutoOpenMenuOnStart", autoOpenOnStart.ToString());
            ConfigManager.Save();

            TrainerLogger.Info($"Trainer initialized with open key {openMenuKey} (auto-open: {autoOpenOnStart}).");

            Theme.Initialize();
            MenuEngine.Initialize();
            InputManager.Initialize();

            UI.Notify("~p~Full Trainer Loaded");
        }

        private void OnTick(object sender, EventArgs e)
        {
            InputManager.Update();

            if (!autoOpenTriggered && autoOpenOnStart && Game.Player?.Character != null && Game.Player.Character.Exists() && !Game.IsLoading)
            {
                autoOpenTriggered = true;
                menuVisible = true;
                MenuEngine.OpenMenu();
                TrainerLogger.Info("Automatically opened trainer menu on Story Mode load.");
                UI.Notify("~p~Trainer menu opened");
            }

            if (menuVisible)
                MenuEngine.Update();
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == openMenuKey)
            {
                menuVisible = !menuVisible;

                if (menuVisible)
                {
                    MenuEngine.OpenMenu();
                    TrainerLogger.Info($"Trainer menu opened via hotkey ({openMenuKey}).");
                }
                else
                {
                    MenuEngine.CloseMenu();
                    TrainerLogger.Info($"Trainer menu closed via hotkey ({openMenuKey}).");
                }
            }
        }

        public void SetOpenKey(Keys newKey)
        {
            openMenuKey = newKey;
            ConfigManager.Set("OpenMenuKey", newKey.ToString());
            ConfigManager.Save();
            TrainerLogger.Info($"Trainer open key changed to {newKey}.");
        }
    }
}
