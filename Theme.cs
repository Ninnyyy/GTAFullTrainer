using System;
using System.Drawing;

namespace GTAFullTrainer.Core
{
    public static class Theme
    {
        // Primary theme colors
        public static Color Purple = Color.FromArgb(255, 155, 77, 255);   // Neon purple
        public static Color Grey = Color.FromArgb(255, 60, 60, 60);       // Steel grey
        public static Color DarkGrey = Color.FromArgb(255, 25, 25, 25);   // Dark grey
        public static Color Black = Color.FromArgb(200, 10, 10, 10);      // Soft black background

        // Glowing purple for accents
        public static Color PurpleGlow = Color.FromArgb(255, 180, 100, 255);

        // Text colors
        public static Color TextBright = Color.FromArgb(255, 255, 255, 255);
        public static Color TextDim = Color.FromArgb(200, 200, 200, 200);

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
