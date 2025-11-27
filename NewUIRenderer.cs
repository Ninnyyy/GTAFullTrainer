using System;
using System.Collections.Generic;
using System.Drawing;
using GTA;
using GTA.Native;
using NinnyTrainer.UI;
using NinnyTrainer.Utils;
using NinnyTrainer.Effects;

namespace NinnyTrainer.Rendering
{
    public static class NewUIRenderer
    {
        private static Dictionary<int, float> categoryY = new();

        public static void DrawBackdrop()
        {
            DrawUtils.RectFullScreen(Color.FromArgb(120, 10, 10, 10));
            Function.Call(Hash._SET_SCREEN_EFFECT, "MenuMGHeistOut", 0, true);
        }

        public static void DrawCategories(float slideX, List<string> categories, int active)
        {
            int baseX = (int)slideX + 60;
            int startY = 150;

            for (int i = 0; i < categories.Count; i++)
            {
                if (!categoryY.ContainsKey(i))
                    categoryY[i] = startY + i * 55;

                float targetY = startY + i * 55;
                categoryY[i] = Animation.Smooth(categoryY[i], targetY, 0.12f);

                bool selected = (i == active);
                Rectangle rect = new Rectangle(baseX, (int)categoryY[i], 250, 45);

                if (selected)
                {
                    Glow.NeonRect(rect, ThemeManager.GlowColor, Animation.Pulse(3));
                    DrawUtils.Text(categories[i], rect.X + 25, rect.Y + 10, 0.65f, ThemeManager.MainColor);
                }
                else
                {
                    DrawUtils.Rect(rect, Color.FromArgb(80, 15, 15, 15));
                    DrawUtils.Text(categories[i], rect.X + 25, rect.Y + 10, 0.55f, Color.White);
                }
            }
        }

        public static void DrawItems(float x, List<UIControl> items, int activeItem)
        {
            int startY = 150;

            for (int i = 0; i < items.Count; i++)
            {
                UIControl item = items[i];
                item.Selected = (i == activeItem);
                float y = startY + i * 55;
                item.Draw(x, y);
            }
        }
    }
}
