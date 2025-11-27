using System.Drawing;

namespace GTAFullTrainer.UI
{
    public static class ThemeManager
    {
        public enum Theme
        {
            Purple,
            Blue,
            Red,
            Gold
        }

        public static Theme CurrentTheme = Theme.Purple;

        public static Color MainColor =>
            CurrentTheme switch
            {
                Theme.Blue => Color.FromArgb(120, 160, 255),
                Theme.Red => Color.FromArgb(255, 80, 80),
                Theme.Gold => Color.FromArgb(255, 215, 80),
                _ => Color.FromArgb(180, 0, 255)
            };

        public static Color GlowColor => Color.FromArgb(
            255,
            MainColor.R,
            MainColor.G,
            MainColor.B
        );
    }
}
