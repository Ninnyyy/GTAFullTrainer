using System;
using System.Collections.Generic;
using System.Drawing;
using GTA;
using GTA.Native;
using GTAFullTrainer.UI;
using GTAFullTrainer.Utils;
using GTAFullTrainer.Effects;

namespace GTAFullTrainer.Rendering
{
    public static class NewUIRenderer
    {
        // Sidebar category animation offsets
        private static float categoryPulse = 0f;

        // Smooth Y positions for each category (for animated hover)
        private static Dictionary<int, float> catY = new();

        public static void DrawBackdrop()
        {
            // Dark transparent overlay (behind menu)
            DrawUtils.RectFullScreen(Color.FromArgb(120, 10, 10, 10));

            // Soft blur effect (GTA built-in screen filter)
            Function.Call(Hash._SET_SCREEN_EFFECT, "MenuMGHeistOut", 0, true);
        }

        public static void DrawCategories(float slideX, List<string> categories, int active)
        {
            int baseX = (int)slideX + 60;
            int startY = 150;

            for (int i = 0; i < categories.Count; i++)
            {
                if (!catY.ContainsKey(i))
                    catY[i] = startY + i * 55;

                // Smooth vertical easing
                float targetY = startY + i * 55;
                catY[i] = Animation.Smooth(catY[i], targetY, 0.10f);

                bool selected = (i == active);

                // Rectangle for category button
                Rectangle rect = new Rectangle(
                    baseX,
                    (int)catY[i],
                    250,
                    45
                );

                if (selected)
                {
                    // Neon glow behind selected category
                    Glow.NeonRect(rect, ThemeManager.GlowColor, Animation.Pulse(3));

                    DrawUtils.Text(
                        categories[i],
                        rect.X + 25, rect.Y + 10,
                        0.65f,
                        ThemeManager.MainColor
                    );
                }
                else
                {
                    DrawUtils.Rect(rect, Color.FromArgb(80, 15, 15, 15));

                    DrawUtils.Text(
                        categories[i],
                        rect.X + 25, rect.Y + 10,
                        0.55f,
                        Color.White
                    );
                }
            }
        }

        public static void DrawItems(float x, List<UIControl> items, int activeItem)
        {
            int startY = 150;

            for (int i = 0; i < items.Count; i++)
            {
                UIControl item = items[i];

                bool selected = (i == activeItem);
                item.Selected = selected;

                float y = startY + i * 55;

                item.Draw(x, y);
            }
        }
    }
}
