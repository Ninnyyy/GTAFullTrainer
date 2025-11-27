using GTA;
using System.Drawing;
using NinnyTrainer.Utils;

namespace NinnyTrainer.Pages
{
    public static class DevPage
    {
        private static bool showCoords = false;

        public static void ToggleCoords(bool s)
        {
            showCoords = s;
        }

        public static void OnTick()
        {
            if (!showCoords) return;

            var pos = Game.Player.Character.Position;

            DrawUtils.Text(
                $"X:{pos.X:0.00} Y:{pos.Y:0.00} Z:{pos.Z:0.00}",
                1600, 50, 0.50f, Color.Yellow);
        }
    }
}
