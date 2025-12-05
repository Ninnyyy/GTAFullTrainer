using System;
using GTA;
using GTA.Math;
using GTA.Native;

namespace NinnyTrainer.Pages
{
    public static class PremiumCheatsPage
    {
        private static bool cloakMode;
        private static bool adaptiveArmor;
        private static bool autoRepairRide;
        private static bool kineticShield;
        private static bool infiniteAmmo;
        private static bool hyperFocus;
        private static bool vehicleBooster;
        private static bool heatNullifier;
        private static bool gravityBubble;
        private static bool featherFall;
        private static bool impactGuard;
        private static bool autoCleanse;
        private static bool stasisField;
        private static int shockwaveReadyAt;
        private static bool hyperFocusEngaged;
        private const float HyperFocusTimescale = 0.35f;
        private const float GravityBubbleLift = 2.4f;
        private const float BlinkDistance = 8.5f;

        public static void ToggleCloak(bool enabled)
        {
            cloakMode = enabled;
            Player player = Game.Player;
            Function.Call(Hash.SET_POLICE_IGNORE_PLAYER, player, enabled);
            Function.Call(Hash.SET_EVERYONE_IGNORE_PLAYER, player, enabled);
            Function.Call(Hash.SET_PLAYER_CAN_BE_HASSLED_BY_GANGS, player, !enabled);

            Ped ped = player.Character;
            if (ped != null && ped.Exists())
            {
                ped.IsVisible = !enabled;
                if (enabled)
                    ped.IsInvincible = true;
            }
        }

        public static void ToggleAdaptiveArmor(bool enabled) => adaptiveArmor = enabled;

        public static void ToggleAutoRepairRide(bool enabled) => autoRepairRide = enabled;

        public static void ToggleKineticShield(bool enabled) => kineticShield = enabled;

        public static void ToggleInfiniteAmmo(bool enabled)
        {
            infiniteAmmo = enabled;

            Ped ped = Game.Player.Character;
            if (ped == null || !ped.Exists())
                return;

            Weapon weapon = ped.Weapons.Current;
            if (weapon != null && weapon.IsPresent)
            {
                weapon.InfiniteAmmo = enabled;
                weapon.InfiniteAmmoClip = enabled;
            }
        }

        public static void ToggleHyperFocus(bool enabled)
        {
            hyperFocus = enabled;

            if (!enabled && hyperFocusEngaged)
            {
                Game.TimeScale = 1f;
                hyperFocusEngaged = false;
            }
        }

        public static void ToggleVehicleBooster(bool enabled) => vehicleBooster = enabled;

        public static void ToggleHeatNullifier(bool enabled)
        {
            heatNullifier = enabled;
            if (!enabled)
                return;

            Game.Player.WantedLevel = 0;
        }

        public static void ToggleGravityBubble(bool enabled) => gravityBubble = enabled;

        public static void ToggleFeatherFall(bool enabled)
        {
            featherFall = enabled;
            Ped ped = Game.Player.Character;
            if (ped != null && ped.Exists() && !enabled)
                ped.CanRagdoll = true;
        }

        public static void ToggleImpactGuard(bool enabled)
        {
            impactGuard = enabled;
            Ped ped = Game.Player.Character;
            if (ped == null || !ped.Exists())
                return;

            Function.Call(Hash.SET_ENTITY_PROOFS, ped, enabled, enabled, enabled, enabled, enabled, enabled, enabled, enabled);
        }

        public static void ToggleAutoCleanse(bool enabled) => autoCleanse = enabled;

        public static void ToggleStasisField(bool enabled) => stasisField = enabled;

        public static void TriggerBlinkDash()
        {
            Ped ped = Game.Player.Character;
            if (ped == null || !ped.Exists())
                return;

            Vector3 forward = ped.ForwardVector;
            if (forward.Length() < 0.1f)
                return;

            Vector3 dash = ped.Position + forward.Normalized * BlinkDistance;
            dash.Z += 0.2f;
            ped.Position = dash;
        }

        public static void TriggerShockwave()
        {
            if (Game.GameTime < shockwaveReadyAt)
                return;

            shockwaveReadyAt = Game.GameTime + 4500;
            Ped ped = Game.Player.Character;
            if (ped == null || !ped.Exists())
                return;

            Vector3 position = ped.Position;
            World.AddExplosion(position, ExplosionType.StickyBomb, 1f, 2f);
            World.ShakeGameplayCam(CameraShake.Explosion, 0.4f);
        }

        public static void TriggerEmpBurst()
        {
            Ped ped = Game.Player.Character;
            if (ped == null || !ped.Exists())
                return;

            foreach (Vehicle vehicle in World.GetNearbyVehicles(ped, 45f))
            {
                if (vehicle == null || !vehicle.Exists())
                    continue;

                if (ped.IsInVehicle() && vehicle.Handle == ped.CurrentVehicle.Handle)
                    continue;

                vehicle.EngineHealth = Math.Min(vehicle.EngineHealth, 50f);
                vehicle.EngineRunning = false;
                Function.Call(Hash.SET_VEHICLE_ENGINE_ON, vehicle, false, true, true);
            }

            World.ShakeGameplayCam(CameraShake.SmallExplosion, 0.35f);
        }

        public static void OnTick()
        {
            Ped ped = Game.Player.Character;
            if (ped == null || !ped.Exists())
                return;

            if (cloakMode)
            {
                ped.IsVisible = false;
                ped.IsInvincible = true;
                Function.Call(Hash.SET_POLICE_IGNORE_PLAYER, Game.Player, true);
                Function.Call(Hash.SET_EVERYONE_IGNORE_PLAYER, Game.Player, true);
                Function.Call(Hash.SET_PLAYER_CAN_BE_HASSLED_BY_GANGS, Game.Player, false);
            }
            else
            {
                ped.IsVisible = true;
            }

            if (adaptiveArmor)
            {
                ped.Health = Math.Max(ped.Health, ped.MaxHealth);
                ped.Armor = Math.Max(ped.Armor, 200);
            }

            if (autoRepairRide && ped.IsInVehicle())
            {
                Vehicle veh = ped.CurrentVehicle;
                if (veh != null && veh.Exists())
                {
                    veh.Repair();
                    veh.IsInvincible = true;
                    Function.Call(Hash.SET_VEHICLE_TYRES_CAN_BURST, veh, false);
                    Function.Call(Hash.SET_VEHICLE_STRONG, veh, true);
                }
            }

            if (kineticShield)
            {
                foreach (Ped nearby in World.GetNearbyPeds(ped, 12f))
                {
                    if (nearby == null || !nearby.Exists() || nearby == ped)
                        continue;

                    if (!nearby.IsPersistent)
                        nearby.IsPersistent = true;

                    Vector3 direction = (nearby.Position - ped.Position);
                    if (direction.Length() > 0.1f)
                    {
                        direction.Normalize();
                        nearby.ApplyForce(direction * 2f * nearby.Mass);
                    }
                }
            }

            if (infiniteAmmo)
            {
                Weapon weapon = ped.Weapons.Current;
                if (weapon != null && weapon.IsPresent)
                {
                    weapon.InfiniteAmmo = true;
                    weapon.InfiniteAmmoClip = true;
                    weapon.Ammo = Math.Max(weapon.Ammo, weapon.MaxAmmo);
                }
            }

            if (hyperFocus)
            {
                if (ped.IsAiming)
                {
                    Game.TimeScale = HyperFocusTimescale;
                    hyperFocusEngaged = true;
                }
                else if (hyperFocusEngaged)
                {
                    Game.TimeScale = 1f;
                    hyperFocusEngaged = false;
                }
            }

            if (vehicleBooster && ped.IsInVehicle())
            {
                Vehicle veh = ped.CurrentVehicle;
                if (veh != null && veh.Exists())
                {
                    Vector3 push = veh.ForwardVector * 0.7f;
                    veh.ApplyForce(push);
                }
            }

            if (heatNullifier)
                Game.Player.WantedLevel = 0;

            if (gravityBubble)
            {
                foreach (Ped nearby in World.GetNearbyPeds(ped, 20f))
                {
                    if (nearby == null || !nearby.Exists() || nearby == ped)
                        continue;

                    nearby.ApplyForce(new Vector3(0f, 0f, GravityBubbleLift));
                }

                foreach (Vehicle vehicle in World.GetNearbyVehicles(ped, 25f))
                {
                    if (vehicle == null || !vehicle.Exists())
                        continue;

                    vehicle.ApplyForce(new Vector3(0f, 0f, GravityBubbleLift * 0.65f));
                }
            }

            if (featherFall)
            {
                bool isAirborne = ped.IsInAir || ped.IsJumping || ped.IsFalling;
                if (isAirborne && ped.Velocity.Z < -2.5f)
                {
                    ped.CanRagdoll = false;
                    ped.ApplyForce(new Vector3(0f, 0f, 4.5f));
                    ped.Velocity = new Vector3(ped.Velocity.X, ped.Velocity.Y, ped.Velocity.Z * 0.55f);
                }
                else
                {
                    ped.CanRagdoll = true;
                }
            }

            if (impactGuard)
            {
                Function.Call(Hash.SET_ENTITY_PROOFS, ped, true, true, true, true, true, true, true, true);
            }

            if (autoCleanse)
            {
                ped.ClearBloodDamage();
                Function.Call(Hash.CLEAR_PED_WETNESS, ped);
                Function.Call(Hash.STOP_ENTITY_FIRE, ped);
            }

            if (stasisField)
            {
                foreach (Ped nearby in World.GetNearbyPeds(ped, 18f))
                {
                    if (nearby == null || !nearby.Exists() || nearby == ped)
                        continue;

                    nearby.Velocity = nearby.Velocity * 0.45f;
                }

                foreach (Vehicle vehicle in World.GetNearbyVehicles(ped, 25f))
                {
                    if (vehicle == null || !vehicle.Exists())
                        continue;

                    vehicle.Velocity = vehicle.Velocity * 0.6f;
                }
            }
        }
    }
}
