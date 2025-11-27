using GTA;
using GTA.Native;

namespace NinnyTrainer.Pages
{
    public static class WeaponModderPage
    {
        private static int bulletType = 0;
        private static float damageMultiplier = 1f;
        private static bool explosiveAmmo = false;

        public static void SetBulletType(int index)
        {
            bulletType = index;
        }

        public static void SetDamageMultiplier(float v)
        {
            damageMultiplier = v;
        }

        public static void SetExplosive(bool state)
        {
            explosiveAmmo = state;
        }

        public static void SavePreset()
        {
            Script.Settings.SetValue("wep_type", bulletType);
            Script.Settings.SetValue("wep_dmg", damageMultiplier);
            Script.Settings.SetValue("wep_explo", explosiveAmmo);
            Script.Settings.Save();
        }

        public static void LoadPreset()
        {
            bulletType = Script.Settings.GetValue("wep_type", 0);
            damageMultiplier = Script.Settings.GetValue("wep_dmg", 1f);
            explosiveAmmo = Script.Settings.GetValue("wep_explo", false);
        }

        public static void OnTick()
        {
            Ped p = Game.Player.Character;
            if (!p.IsShooting) return;

            Weapon w = p.Weapons.Current;

            if (w == null) return;

            // damage boost
            w.Damage = (int)(50 * damageMultiplier);

            // explosive
            if (explosiveAmmo)
                Function.Call(Hash.SET_EXPLOSIVE_AMMO_THIS_FRAME, Game.Player);

            // custom bullet types
            switch (bulletType)
            {
                case 1:
                    Function.Call(Hash.SHOOT_SINGLE_BULLET_BETWEEN_COORDS, p.Position, p.Position + p.ForwardVector * 50f, 0, 100, true, 615608432, p, true, false, 1f);
                    break;

                case 2:
