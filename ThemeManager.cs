using System.Drawing;

namespace NinnyTrainer.UI
{
    public static class ThemeManager
    {
        public static int Theme = 0; // 0=Purple,1=Blue,2=Red,3=Gold
        public static bool GlowEnabled = true;
        public static bool SoundEnabled = true;

        public static Color MainColor =>
            Theme switch
            {
                1 => Color.FromArgb(120, 160, 255),
                2 => Color.FromArgb(255, 80, 80),
                3 => Color.FromArgb(255, 215, 80),
                _ => Color.FromArgb(180, 0, 255)
            };

        public static Color GlowColor =>
            Color.FromArgb(255, MainColor.R, MainColor.G, MainColor.B);

        public static void SetTheme(int i) => Theme = i;
        public static void ToggleGlow(bool s) => GlowEnabled = s;
        public static void ToggleSound(bool s) => SoundEnabled = s;
    }
}
