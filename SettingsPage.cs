using GTAFullTrainer.UI;
using GTAFullTrainer.Utils;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GTAFullTrainer.Pages
{
    public static class SettingsPage
    {
        private static List<UIElement> items = new List<UIElement>();

        static SettingsPage()
        {
            items.Add(new UIKeybind("Open Menu Key", 0, "OpenMenuKey", Keys.Insert));
            items.Add(new UIToggle("Animations Enabled", 1, true));
            items.Add(new UISlider("UI Scale", 2, 0.5f, 1.5f, 1.0f));
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
