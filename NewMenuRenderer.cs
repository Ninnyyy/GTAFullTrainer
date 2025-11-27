using System;
using System.Collections.Generic;
using System.Drawing;
using GTA;
using GTA.Native;
using GTAFullTrainer.UI;
using GTAFullTrainer.Effects;
using GTAFullTrainer.Utils;

namespace GTAFullTrainer.Rendering
{
    public static class NewMenuRenderer
    {
        private static float targetY = 0f;
        private static float currentY = -400f;

        public static void DrawBackground()
        {
            Function.Call(Hash.SET_NOISEOVERIDE, true);
            Function.Call(Hash.SET_NOISEOVERIDE, 0.2f);

            // Blur screen
            Function.Call(Hash._SET_SCREEN_EFFECT, "MenuMGIsland", 0, true);

            // Dark overlay
            DrawUtils.RectFullScreen(Color.FromArgb(120, 0, 0, 0));
        }

        public static void DrawMenu(List<string> categories, int activeCategory)
        {
            // Slide animation
            targetY = 200f;
            currentY = Animation.Smooth(currentY, targetY, 0.15f);

            int x = 200;
            int y = (int)currentY;

            // Render category sidebar
            for (int i = 0; i < categories.Count; i++)
            {
                bool selected = (i == activeCategory);
                float pulse = selected ? Animation.Pulse(3) : 0f;

                Rectangle rect = new Rectangle(x, y + i * 55, 300, 45);

                // Selected with neon glow
                if (selected)
                    Effects.Glow.NeonRect(rect, ThemeManager.GlowColor, 1.0f);

                // Label
                DrawUtils.Text(
                    categories[i],
                    rect.X + 20, rect.Y + 10,
                    selected ? 0.6f : 0.5f,
                    selected ? ThemeManager.MainColor : Color.White);
            }
        }
    }
}
