using GTA.UI;
using System.Drawing;

namespace GTAFullTrainer.Utils
{
    public static class DrawUtils
    {
        public static void Text(string text, int x, int y, float scale, Color color)
        {
            new UIResText(
                text,
                new Point(x, y),
                scale,
                color
            ).Draw();
        }

        public static void Rect(int x, int y, int w, int h, Color color)
        {
            new UIResRectangle(
                new Point(x, y),
                new Size(w, h),
                color
            ).Draw();
        }
    }
}
