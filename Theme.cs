using System;
using System.Drawing;

namespace GTAFullTrainer.Core
{
    public static class Theme
    {
        // Primary theme colors (dark purple, grey, black)
        public static Color Purple = Color.FromArgb(255, 150, 90, 255);   // Rich neon purple
        public static Color Grey = Color.FromArgb(255, 58, 58, 66);        // Graphite grey
        public static Color DarkGrey = Color.FromArgb(255, 22, 22, 28);    // Deep slate
        public static Color Black = Color.FromArgb(210, 8, 8, 10);         // Soft black background

        // Accent glows and gradients
        public static Color PurpleGlow = Color.FromArgb(255, 195, 140, 255);
        public static Color SoftGradientTop = Color.FromArgb(120, 70, 40, 80);
        public static Color SoftGradientBottom = Color.FromArgb(120, 15, 10, 20);

        // Text colors
        public static Color TextBright = Color.FromArgb(255, 245, 245, 255);
        public static Color TextDim = Color.FromArgb(200, 200, 200, 220);

        // UI scale (changeable in config/settings)
        public static float UIScale = 1.0f;

        // Fonts, margins, spacing
        public static float ItemHeight = 0.038f;
        public static float ItemPadding = 0.005f;

        public static void Initialize()
        {
            // Load configurable settings if desired
            try
            {
                string scale = ConfigManager.Get("UIScale", "1.0");
                UIScale = float.Parse(scale);
            }
            catch
            {
                UIScale = 1.0f;
            }
        }
    }
}
