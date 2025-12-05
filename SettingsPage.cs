using GTAFullTrainer.Core;
using GTAFullTrainer.UI;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GTAFullTrainer.Pages
{
    public static class SettingsPage
    {
        private static readonly List<UIElement> items = new List<UIElement>();

        static SettingsPage()
        {
            items.Add(new UIKeybind("Open Menu Key", 0, "OpenMenuKey", Keys.Insert));

            items.Add(new UISettingsToggle("Auto-Open Menu", 1, "AutoOpenMenuOnStart", true, state =>
            {
                TrainerMain.Instance?.SetAutoOpen(state);
            }));

            items.Add(new UISettingsToggle("Menu Animations", 2, "MenuAnimationsEnabled", true, enabled =>
            {
                MenuEngine.SetAnimationsEnabled(enabled);
            }));

            items.Add(new UISettingsSlider("Menu Animation Speed", 3, "MenuAnimationSpeed", 0.02f, 0.25f, 0.08f, 0.01f, speed =>
            {
                MenuEngine.SetAnimationSpeed(speed);
            }));

            items.Add(new UISettingsSlider("UI Scale", 4, "MenuUiScale", 0.75f, 1.50f, 1.00f, 0.05f, scale =>
            {
                MenuEngine.SetUiScale(scale);
            }));

            items.Add(new UISettingsToggle("Trainer Logging", 5, "TrainerLoggingEnabled", true, enabled =>
            {
                TrainerLogger.SetEnabled(enabled);
            }));
        }

        public static void Draw(int index)
        {
            for (int i = 0; i < items.Count; i++)
            {
                items[i].Selected = (i == index);
                items[i].Draw(800, 300 + (i * 50));
            }
        }
    }
}
