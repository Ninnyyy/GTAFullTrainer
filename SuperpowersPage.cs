using System.Collections.Generic;
using System.Drawing;
using GTA;
using GTA.Native;
using GTAFullTrainer.UI;
using GTAFullTrainer.Utils;

namespace GTAFullTrainer.Pages
{
    public static class SuperpowersPage
    {
        private static bool initialized = false;
        private static List<UIElement> items = new List<UIElement>();

        // Powers
        private static bool telekinesis = false;
        private static bool flashSpeed = false;
        private static bool hulkJump = false;
        private static bool groundPunch = false;
        private static bool freezeBeam = false;
        private static bool fireAura = false;
        private static bool timeDilation = false;

        // State
        private static Entity telekinesisTarget = null;
        private static float telekinesisDistance = 5f;

        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            // POWER TOGGLES
            items.Add(new UIToggle("Telekinesis", 0));
            items.Add(new UIToggle("Flash-Speed Mode", 1));
            items.Add(new UIToggle("Hulk Jump", 2));
            items.Add(new UIToggle("Ground Punch Shockwave", 3));
            items.Add(new UIToggle("Freeze Beam", 4));
            items.Add(new UIToggle("Fire Aura", 5));
            items.Add(new UIToggle("Time Dilation", 6));

            // TELEKINESIS DISTANCE SLIDER
            items.Add(new UISlider("Telekinesis Distance", 7, 3f, 20f, 5f));
        }

        public static void ApplyLogic()
        {
            telekinesis = ((UIToggle)items[0]).State;
            flashSpeed = ((UIToggle)items[1]).State;
            hulkJump = ((UIToggle)items[2]).State;
            groundPunch = ((UIToggle)items[3]).State;
            freezeBeam = ((UIToggle)items[4]).State;
            fireAura = ((UIToggle)items[5]).State;
            timeDilation = ((UIToggle)items[6]).State;

            telekinesisDistance = ((UISlider)items[7]).Value;

            Ped player = Game.Player.Character;

            // ============================================================
            // TELEKINESIS
            // ============================================================
            if (telekinesis)
            {
                if (Game.IsKeyPressed(System.Windows.Forms.Keys.RButton))
                {
                    Vector3 from = GameplayCamera.Position;
                    Vector3 dir = GameplayCamera.Direction;
                    Vector3 to = from + dir * 200f;

                    RaycastResult r = World.Raycast(from, to, 200f);

                    if (r.DitHitEntity)
                        telekinesisTarget = r.HitEntity;

                    if (telekinesisTarget != null && telekinesisTarget.Exists())
                    {
                        Vector3 targetPos = from + dir * telekinesisDistance;
                        telekinesisTarget.Position = Vector3.Lerp(
                            telekinesisTarget.Position, targetPos, 0.45f);

                        // THROW TARGET
                        if (Game.IsKeyPressed(System.Windows.Forms.Keys.Q))
                        {
                            telekinesisTarget.ApplyForce(dir * 150f);
                            telekinesisTarget = null;
                        }
                    }
                }
                else
                {
                    telekinesisTarget = null;
                }
            }

            // ============================================================
            // FLASH-SPEED MODE
            // ============================================================
            if (flashSpeed)
            {
                Game.Player.Character.SetMoveSpeedMultiplier(5f);
                Game.Player.Character.SetRunSpeedMultiplier(5f);

                // Visual speed effect
                Function.Call(Hash._SET_SCREEN_EFFECT, "RaceTurbo", 0, true);
            }
            else
            {
                Game.Player.Character.SetMoveSpeedMultiplier(1f);
                Game.Player.Character.SetRunSpeedMultiplier(1f);
                Function.Call(Hash._STOP_ALL_SCREEN_EFFECTS);
            }

            // ============================================================
            // HULK JUMP
            // ============================================================
            if (hulkJump)
            {
                if (Game.IsKeyPressed(System.Windows.Forms.Keys.Space))
                {
                    player.ApplyForce(new Vector3(0, 0, 40f));

                    // Landing shockwave
                    if (player.IsInAir && player.Position.Z > 8f)
                    {
                        if (player.IsTouchingGround)
                        {
                            Shockwave(player.Position, 40f, 100f);
                        }
                    }
                }
            }

            // ============================================================
            // GROUND PUNCH
            // ============================================================
            if (groundPunch)
            {
                if (Game.IsKeyPressed(System.Windows.Forms.Keys.E))
                {
                    // Punch ground animation
                    player.Task.PlayAnimation("melee@large_wpn@streamed_core", "car_down_player", 2f, 2f, false, false, false);

                    Script.Wait(300);

                    Shockwave(player.Position, 30f, 70f);
                }
            }

            // ============================================================
            // FREEZE BEAM
            // ============================================================
            if (freezeBeam)
            {
                if (Game.IsKeyPressed(System.Windows.Forms.Keys.LButton))
                {
                    Vector3 from = GameplayCamera.Position;
                    Vector3 dir = GameplayCamera.Direction;
                    Vector3 to = from + dir * 200f;

                    RaycastResult r = World.Raycast(from, to, 200f);

                    if (r.DitHitEntity && r.HitEntity.Exists())
                    {
                        Entity e = r.HitEntity;

                        // Freeze
                        Function.Call(Hash.SET_ENTITY_ANIM_SPEED, e.Handle, 0.0f);
                        Function.Call(Hash.FREEZE_ENTITY_POSITION, e.Handle, true);

                        // Cold effect
                        World.AddExplosion(e.Position, ExplosionType.Snowball, 1f, 0.2f);
                    }
                }
            }

            // ============================================================
            // FIRE AURA
            // ============================================================
            if (fireAura)
            {
                foreach (Ped p in World.GetAllPeds())
                {
                    if (!p.IsPlayer && p.Position.DistanceToSquared(player.Position) < 12f)
                    {
                        p.IsOnFire = true;
                    }
                }
            }

            // ============================================================
            // TIME DILATION
            // ============================================================
            if (timeDilation)
            {
                Game.TimeScale = 0.2f;

                // Player unaffected
                player.Task.ClearAllImmediately();
                player.SetMaxSpeed(999f);
            }
            else
            {
                Game.TimeScale = 1.0f;
            }
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

            World.AddExplosion(pos, ExplosionType.Grenade, 1f, 0.1f);
            GameplayCamera.Shake(CameraShake.MediumExplosion, 1.2f);
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
