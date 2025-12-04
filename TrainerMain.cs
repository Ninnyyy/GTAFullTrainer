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

        public static TrainerMain Instance;

        public TrainerMain()
        {
            Instance = this;

            Tick += OnTick;
            KeyUp += OnKeyUp;

            ConfigManager.Load();

            string loadedKey = ConfigManager.Get("OpenMenuKey", Keys.Insert.ToString());

            if (Enum.TryParse(loadedKey, out Keys parsedKey))
            {
                openMenuKey = parsedKey;
            }
            else
            {
                openMenuKey = Keys.Insert;
            }

            ConfigManager.Set("OpenMenuKey", openMenuKey.ToString());
            ConfigManager.Save();

            Theme.Initialize();
            MenuEngine.Initialize();
            InputManager.Initialize();

            UI.Notify("~p~Full Trainer Loaded");
        }

        private void OnTick(object sender, EventArgs e)
        {
            InputManager.Update();

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
                }
                else
                {
                    MenuEngine.CloseMenu();
                }
            }
        }

        public void SetOpenKey(Keys newKey)
        {
            openMenuKey = newKey;
            ConfigManager.Set("OpenMenuKey", newKey.ToString());
            ConfigManager.Save();
        }
    }
}
