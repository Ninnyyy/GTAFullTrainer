using GTA;
using GTA.Math;
using System.Drawing;
using NinnyTrainer.Utils;

namespace NinnyTrainer.Pages
{
    public static class HUDWidgetsPage
    {
        private static bool speed = false;
        private static bool rpm = false;
        private static bool gforce = false;
        private static bool compass = false;
        private static bool healthArmor = false;
        private static bool damage = false;

        public static void ToggleSpeed(bool s) => speed = s;
        public static void ToggleRPM(bool s) => rpm = s;
        public static void ToggleGForce(bool s) => gforce = s;
        public static void ToggleCompass(bool s) => compass = s;
        public static void ToggleHealthArmor(bool s) => healthArmor = s;
        public static void ToggleDamage(bool s) => damage = s;

        public static void OnTick()
        {
            Ped p = Game.Player.Character;

            if (speed && p.IsInVehicle())
            {
                float mph = p.CurrentVehicle.Speed * 2.237f;
                DrawUtils.Text($"Speed: {mph:0}", 30, 900, 0.55f, Color.White);
            }

            if (healthArmor)
            {
                DrawUtils.Text($"HP: {p.Health}/{p.MaxHealth}", 30, 930, 0.50f, Color.Lime);
                DrawUtils.Text($"Armor: {p.Armor}", 30, 960, 0.50f, Color.Cyan);
            }
        }
    }
}
