using GTAFullTrainer.UI;

namespace GTAFullTrainer.Pages
{
    public static class MenuThemePage
    {
        private static bool initialized = false;
        private static List<UIElement> items = new();

        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            items.Add(new UIList("UI Theme", 0,
                new[] { "Purple", "Blue", "Red", "Gold" }));

            items.Add(new UIToggle("Menu Glow", 1));
            items.Add(new UIToggle("Sound Effects", 2));
        }

        public static void ApplyLogic()
        {
            int theme = ((UIList)items[0]).CurrentIndex;
            ThemeManager.CurrentTheme = (ThemeManager.Theme)theme;

            bool glow = ((UIToggle)items[1]).State;
            bool sounds = ((UIToggle)items[2]).State;
        }

        public static void Draw(int index)
        {
            Init();
            ApplyLogic();

            for (int i = 0; i < items.Count; i++)
            {
                items[i].Selected = (i == index);
                items[i].Draw(800, 300 + i * 50);
            }
        }
    }
}
