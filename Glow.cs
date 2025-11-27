using System.Drawing;
using GTA;
using GTAFullTrainer.Utils;

namespace GTAFullTrainer.Effects
{
    public static class Glow
    {
        public static void NeonRect(Rectangle rect, Color color, float intensity = 1f)
        {
            int alpha = (int)(150 * intensity);

            // Outer glow
            DrawUtils.Rect(new Rectangle(
                rect.X - 4, rect.Y - 4,
                rect.Width + 8, rect.Height + 8),
                Color.FromArgb(alpha, color.R, color.G, color.B));

            // Main rect
            DrawUtils.Rect(rect, Color.FromArgb(200, 20, 20, 20));
        }
    }
}
