using System;
using System.Collections.Generic;
using System.Drawing;
using GTA;
using GTA.Native;
using GTA.Math;
using GTAFullTrainer.UI;
using GTAFullTrainer.Utils;

namespace GTAFullTrainer.Pages
{
    public static class WeaponModderPage
    {
        private static bool initialized = false;
        private static List<UIElement> items = new List<UIElement>();

        // Bullet Types
        private enum BulletType
        {
            Normal,
            Laser,
            Freeze,
            Fire,
            GravityPull,
            GravityPush,
            Explosive,
            Shockwave
        }

        private static BulletType bulletMode = BulletType.Normal;

        // Sliders
        private static float bulletSpeed = 1.0f;
        private static float damageMultiplier = 1.0f;
        private static float recoilMultiplier = 1.0f;
        private static float spreadMultiplier = 1.0f;

        // Effects
        private static Color trailColor = Color.Purple;
        private static Color muzzleColor = Color.Cyan;

        // Attachments
        private static bool suppressor = false;
        private static bool flashlight = false;
        private static bool extendedClip = false;

        // SAVE/LOAD PRESETS
        private static string presetFile = "WeaponPreset.txt";

        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            // BULLET TYPE LIST
            items.Add(new UIList("Bullet Type", 0,
                new[] { "Normal", "Laser", "Freeze", "Fire", "Gravity Pull", "Gravity Push", "Explosive", "Shockwave" }));

            // SLIDERS
            items.Add(new UISlider("Bullet Speed", 1, 0.2f, 5.0f, 1.0f));
            items.Add(new UISlider("Damage Multiplier", 2, 0.1f, 10.0f, 1.0f));
            items.Add(new UISlider("Recoil Multiplier", 3, 0.0f, 5.0f, 1.0f));
            items.Add(new UISlider("Spread Multiplier", 4, 0.0f, 5.0f, 1.0f));

            // ATTACHMENTS
            items.Add(new UIToggle("Suppressor", 5));
            items.Add(new UIToggle("Flashlight", 6));
            items.Add(new UIToggle("Extended Clip", 7));

            // PRESETS
            items.Add(new UIButton("Save Weapon Preset", 8, () => SavePreset()));
            items.Add(new UIButton("Load Weapon Preset", 9, () => LoadPreset()));
        }

        public static void ApplyLogic()
        {
            Ped player = Game.Player.Character;
            if (!player.IsShooting) return;

            Weapon w = player.Weapons.Current;

            // Read UI
            bulletMode = (BulletType)((UIList)items[0]).CurrentIndex;

            bulletSpeed = ((UISlider)items[1]).Value;
            damageMultiplier = ((UISlider)items[2]).Value;
            recoilMultiplier = ((UISlider)items[3]).Value;
            spreadMultiplier = ((UISlider)items[4]).Value;

            suppressor = ((UIToggle)items[5]).State;
            flashlight = ((UIToggle)items[6]).State;
            extendedClip = ((UIToggle)items[7]).State;

            // APPLY ATTACHMENTS
            ApplyAttachments(w);

            // APPLY MODIFIERS
            w.Damage = (int)(w.Damage * damageMultiplier);
            w.RecoilShakeAmplitude *= recoilMultiplier;
            w.Spread *= spreadMultiplier;

            // CUSTOM BULLET SYSTEM
            ApplyCustomBullet(player, w);
        }

        private static void ApplyAttachments(Weapon w)
        {
            // Not all weapons support these, but we do safe checks:
            if (suppressor)
                Function.Call(Hash.GIVE_WEAPON_COMPONENT_TO_PED, Game.Player.Character.Handle, w.Hash, (uint)WeaponComponent.AtPiSuppressor);

            if (flashlight)
                Function.Call(Hash.GIVE_WEAPON_COMPONENT_TO_PED, Game.Player.Character.Handle, w.Hash, (uint)WeaponComponent.AtArFlsh);

            if (extendedClip)
                Function.Call(Hash.GIVE_WEAPON_COMPONENT_TO_PED, Game.Player.Character.Handle, w.Hash, (uint)WeaponComponent.AtArAfGrip);
        }

        private static void ApplyCustomBullet(Ped player, Weapon w)
        {
            Vector3 from = GameplayCamera.Position;
            Vector3 dir = GameplayCamera.Direction;
            Vector3 to = from + dir * (200f * bulletSpeed);

            RaycastResult hit = World.Raycast(from, to, 200f);

            if (hit.DitHitEntity)
            {
                Entity target = hit.HitEntity;

                switch (bulletMode)
                {
                    case BulletType.Laser:
                        DrawLaser(from, hit.HitPosition);
                        target.ApplyForce(dir * 40f);
                        break;

                    case BulletType.Freeze:
                        FreezeEntity(target);
                        break;

                    case BulletType.Fire:
                        target.IsOnFire = true;
                        break;

                    case BulletType.GravityPull:
                        target.ApplyForce((player.Position - target.Position).Normalized * 50f);
                        break;

                    case BulletType.GravityPush:
                        target.ApplyForce(dir * 80f);
                        break;

                    case BulletType.Explosive:
                        World.AddExplosion(hit.HitPosition, ExplosionType.Grenade, 3.0f, 1.0f);
                        break;

                    case BulletType.Shockwave:
                        Shockwave(hit.HitPosition, 30f, 60f);
                        break;
                }
            }
        }

        private static void DrawLaser(Vector3 from, Vector3 to)
        {
            // Simple line draw
            Function.Call(Hash.DRAW_LINE,
                from.X, from.Y, from.Z,
                to.X, to.Y, to.Z,
                muzzleColor.R, muzzleColor.G, muzzleColor.B, 255);
        }

        private static void FreezeEntity(Entity e)
        {
            if (!e.Exists()) return;

            e.FreezePosition = true;
            Function.Call(Hash.SET_ENTITY_INVINCIBLE, e.Handle, true);

            // Ice effect
            World.AddExplosion(e.Position, ExplosionType.Snowball, 0.5f, 0.1f);
        }

        private static void Shockwave(Vector3 pos, float radius, float force)
        {
            foreach (Entity e in World.GetAllEntities())
            {
                if (!e.IsPlayer)
                {
                    float dist = e.Position.DistanceTo(pos);
                    if (dist < radius)
                    {
                        Vector3 push = (e.Position - pos).Normalized * force;
                        e.ApplyForce(push);
                    }
                }
            }

            GameplayCamera.Shake(CameraShake.MediumExplosion, 1.0f);
            World.AddExplosion(pos, ExplosionType.Grenade, 1.5f, 0.2f);
        }

        // ================================
        // PRESETS
        // ================================
        private static void SavePreset()
        {
            string data =
                $"{bulletMode};{bulletSpeed};{damageMultiplier};{recoilMultiplier};{spreadMultiplier};{suppressor};{flashlight};{extendedClip}";

            System.IO.File.WriteAllText(presetFile, data);
        }

        private static void LoadPreset()
        {
            if (!System.IO.File.Exists(presetFile)) return;

            string data = System.IO.File.ReadAllText(presetFile);
            string[] p = data.Split(';');

            bulletMode = (BulletType)int.Parse(p[0]);
            bulletSpeed = float.Parse(p[1]);
            damageMultiplier = float.Parse(p[2]);
            recoilMultiplier = float.Parse(p[3]);
            spreadMultiplier = float.Parse(p[4]);
            suppressor = bool.Parse(p[5]);
            flashlight = bool.Parse(p[6]);
            extendedClip = bool.Parse(p[7]);

            // Sync UI
            ((UIList)items[0]).CurrentIndex = (int)bulletMode;
            ((UISlider)items[1]).Value = bulletSpeed;
            ((UISlider)items[2]).Value = damageMultiplier;
            ((UISlider)items[3]).Value = recoilMultiplier;
            ((UISlider)items[4]).Value = spreadMultiplier;
            ((UIToggle)items[5]).State = suppressor;
            ((UIToggle)items[6]).State = flashlight;
            ((UIToggle)items[7]).State = extendedClip;
        }

        public static void Draw(int index)
        {
            Init();

            ApplyLogic();

            for (int i = 0; i < items.Count; i++)
            {
                items[i].Selected = (i == index);
                items[i].Draw(800, 300 + i * 45);
            }
        }
    }
}
