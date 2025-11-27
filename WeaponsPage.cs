using System.Collections.Generic;
using System.Drawing;
using GTA;
using GTA.Native;
using GTAFullTrainer.UI;
using GTAFullTrainer.Utils;
using GTAFullTrainer.Core;

namespace GTAFullTrainer.Pages
{
    public static class WeaponsPage
    {
        private static List<UIElement> items = new List<UIElement>();
        private static bool initialized = false;

        private static bool explosiveAmmo = false;
        private static bool fireAmmo = false;
        private static bool noRecoil = false;
        private static bool noSpread = false;
        private static bool rapidFire = false;
        private static bool infiniteAmmo = false;
        private static bool noReload = false;

        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            items.Add(new UIButton("Give All Weapons", 0, () =>
            {
                Function.Call(Hash.GIVE_WEAPON_TO_PED, Game.Player.Character.Handle,
                    WeaponHash.Pistol, 9999, false, true);
                Function.Call(Hash.GIVE_WEAPON_TO_PED, Game.Player.Character.Handle,
                    WeaponHash.CombatMG, 9999, false, true);
                Function.Call(Hash.GIVE_WEAPON_TO_PED, Game.Player.Character.Handle,
                    WeaponHash.AssaultRifle, 9999, false, true);
                Function.Call(Hash.GIVE_WEAPON_TO_PED, Game.Player.Character.Handle,
                    WeaponHash.HeavySniper, 9999, false, true);
            }));

            items.Add(new UIButton("Remove All Weapons", 1, () =>
            {
                Function.Call(Hash.REMOVE_ALL_PED_WEAPONS, Game.Player.Character.Handle, true);
            }));

            items.Add(new UIToggle("Infinite Ammo", 2));
            items.Add(new UIToggle("No Reload", 3));
            items.Add(new UIToggle("Explosive Ammo", 4));
            items.Add(new UIToggle("Fire Ammo", 5));
            items.Add(new UIToggle("No Recoil", 6));
            items.Add(new UIToggle("No Spread", 7));
            items.Add(new UIToggle("Rapid Fire", 8));

            items.Add(new UISlider("Damage Multiplier", 9, 1f, 10f, 1f));

            items.Add(new UIList("Special Gun", 10,
                new string[] {
                    "None",
                    "Airstrike Gun",
                    "Vehicle Gun",
                    "Delete Gun",
                    "Gravity Gun",
                    "Teleport Gun",
                    "Money Gun"
                }));
        }

        public static void ApplyLogic()
        {
            Ped p = Game.Player.Character;
            Weapon w = p.Weapons.Current;

            // Sync toggles
            infiniteAmmo = ((UIToggle)items[2]).State;
            noReload = ((UIToggle)items[3]).State;
            explosiveAmmo = ((UIToggle)items[4]).State;
            fireAmmo = ((UIToggle)items[5]).State;
            noRecoil = ((UIToggle)items[6]).State;
            noSpread = ((UIToggle)items[7]).State;
            rapidFire = ((UIToggle)items[8]).State;

            // Projectiles
            float dmgMul = ((UISlider)items[9]).Value;
            Function.Call(Hash.SET_PLAYER_WEAPON_DAMAGE_MODIFIER, Game.Player.Handle, dmgMul);

            int specialGun = ((UIList)items[10]).CurrentIndex;

            // Infinite ammo
            if (infiniteAmmo)
                Function.Call(Hash.SET_PED_INFINITE_AMMO, p.Handle, true, w.Hash);

            // No reload
            if (noReload)
                Function.Call(Hash.SET_PED_INFINITE_AMMO_CLIP, p.Handle, true);

            // Explosive ammo
            if (explosiveAmmo)
                Function.Call(Hash.SET_EXPLOSIVE_AMMO_THIS_FRAME, Game.Player.Handle);

            // Fire ammo
            if (fireAmmo)
                Function.Call(Hash.SET_FIRE_AMMO_THIS_FRAME, Game.Player.Handle);

            // No recoil
            if (noRecoil)
                Function.Call(Hash.SET_WEAPON_RECOIL_SHAKE_AMPLITUDE, 0f);

            // No spread
            if (noSpread)
                Function.Call(Hash.SET_WEAPON_ACCURACY, p.Handle, 1000f);

            // Rapid Fire
            if (rapidFire)
                Function.Call(Hash.DISABLE_PLAYER_FIRING, Game.Player.Handle, false);

            // SPECIAL GUNS
            if (Game.IsKeyPressed(System.Windows.Forms.Keys.LButton) && specialGun > 0)
            {
                Vector3 from = GameplayCamera.Position;
                Vector3 dir = GameplayCamera.Direction;
                Vector3 to = from + dir * 200f;

                switch (specialGun)
                {
                    case 1: // Airstrike gun
                        Function.Call(Hash.SHOOT_SINGLE_BULLET_BETWEEN_COORDS_IGNORE_ENTITY,
                            to.X, to.Y, to.Z,
                            to.X, to.Y, to.Z,
                            0, true, (int)WeaponHash.RPG, p.Handle, true, false, -1f);
                        break;

                    case 2: // Vehicle gun
                        Vehicle veh = World.CreateVehicle(VehicleHash.Adder, to);
                        break;

                    case 3: // Delete gun
                        RaycastResult r = World.Raycast(from, to, 200f);
                        if (r.DitHitEntity)
                            r.HitEntity.Delete();
                        break;

                    case 4: // Gravity gun
                        RaycastResult r2 = World.Raycast(from, to, 200f);
                        if (r2.DitHitEntity)
                            r2.HitEntity.ApplyForce((dir * 50f));
                        break;

                    case 5: // Teleport gun
                        p.Position = to;
                        break;

                    case 6: // Money gun
                        Function.Call(Hash.CREATE_AMBIENT_PICKUP, 0x1E9A99F8,
                            to.X, to.Y, to.Z, 0, 2000, 1, false, true);
                        break;
                }
            }
        }

        public static void Draw(int index)
        {
            Init();
            ApplyLogic();

            for (int i = 0; i < items.Count; i++)
            {
                items[i].Selected = (i == index);
                items[i].Draw(800, 300 + (i * 45));
            }
        }
    }
}
