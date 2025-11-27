using System.Collections.Generic;
using System.Drawing;
using GTA;
using GTA.Native;
using GTAFullTrainer.UI;
using GTAFullTrainer.Utils;
using GTAFullTrainer.Core;

namespace GTAFullTrainer.Pages
{
    public static class ChaosPage
    {
        private static bool initialized = false;
        private static List<UIElement> items = new List<UIElement>();

        // Toggles
        private static bool blackHole = false;
        private static bool tornado = false;
        private static bool carRain = false;
        private static bool pedRain = false;
        private static bool explosionRain = false;
        private static bool randomChaos = false;
        private static bool carMagnet = false;
        private static bool pedMagnet = false;

        // Sliders
        private static float chaosIntensity = 1f;

        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            // MAIN CHAOS
            items.Add(new UIToggle("Black Hole", 0));
            items.Add(new UIToggle("Tornado", 1));
            items.Add(new UIToggle("Car Rain", 2));
            items.Add(new UIToggle("Ped Rain", 3));
            items.Add(new UIToggle("Explosion Rain", 4));
            items.Add(new UIToggle("Random Chaos Mode", 5));

            // MAGNETS
            items.Add(new UIToggle("Car Magnet", 6));
            items.Add(new UIToggle("Ped Magnet", 7));

            // INTENSITY
            items.Add(new UISlider("Chaos Intensity", 8, 0.5f, 5f, 1f));

            // BUTTON EFFECTS
            items.Add(new UIButton("Gravity Pulse", 9, () =>
            {
                Vector3 pos = Game.Player.Character.Position;
                GameplayCamera.Shake(CameraShake.LargeExplosion, 1.5f);

                foreach (Ped p in World.GetAllPeds())
                    p.ApplyForce((p.Position - pos).Normalized * 100f);

                foreach (Vehicle v in World.GetAllVehicles())
                    v.ApplyForce((v.Position - pos).Normalized * 250f);
            }));

            items.Add(new UIButton("Vehicle Launcher", 10, () =>
            {
                foreach (Vehicle v in World.GetAllVehicles())
                    v.ApplyForce(new Vector3(0, 0, 100f));
            }));

            items.Add(new UIButton("Ped Launcher", 11, () =>
            {
                foreach (Ped p in World.GetAllPeds())
                    if (!p.IsPlayer) p.ApplyForce(new Vector3(0, 0, 80f));
            }));

            items.Add(new UIButton("Nuke", 12, () =>
            {
                Vector3 pos = Game.Player.Character.Position;
                World.AddExplosion(pos,
                    ExplosionType.Rocket, 20f, 1f);

                for (int i = 0; i < 12; i++)
                {
                    World.AddExplosion(
                        pos + new Vector3(World.GetRandomInt(-15, 15), World.GetRandomInt(-15, 15), 0),
                        ExplosionType.Grenade, 8f, 1f);
                }

                GameplayCamera.Shake(CameraShake.LargeExplosion, 3f);
            }));

            items.Add(new UIButton("Earth Rupture", 13, () =>
            {
                Vector3 pos = Game.Player.Character.Position;

                for (int i = 0; i < 20; i++)
                {
                    Vector3 offset = new Vector3(
                        World.GetRandomInt(-20, 20),
                        World.GetRandomInt(-20, 20),
                        0);

                    World.AddExplosion(pos + offset,
                        ExplosionType.Grenade, 5f, 1f);
                }

                GameplayCamera.Shake(CameraShake.LargeExplosion, 2f);
            }));

            items.Add(new UIButton("Clear Spawned Chaos", 14, () =>
            {
                foreach (Vehicle v in World.GetAllVehicles())
                    if (!v.IsSeatFree(VehicleSeat.Driver))
                        v.Delete();

                foreach (Ped p in World.GetAllPeds())
                    if (!p.IsPlayer)
                        p.Delete();
            }));
        }

        public static void ApplyLogic()
        {
            Ped player = Game.Player.Character;
            Vector3 pPos = player.Position;

            blackHole = ((UIToggle)items[0]).State;
            tornado = ((UIToggle)items[1]).State;
            carRain = ((UIToggle)items[2]).State;
            pedRain = ((UIToggle)items[3]).State;
            explosionRain = ((UIToggle)items[4]).State;
            randomChaos = ((UIToggle)items[5]).State;
            carMagnet = ((UIToggle)items[6]).State;
            pedMagnet = ((UIToggle)items[7]).State;

            chaosIntensity = ((UISlider)items[8]).Value;

            // -----------------------
            // BLACK HOLE
            // -----------------------
            if (blackHole)
            {
                foreach (Entity e in World.GetAllEntities())
                {
                    if (!e.IsPlayer)
                    {
                        Vector3 dir = pPos - e.Position;
                        e.ApplyForce(dir.Normalized * (chaosIntensity * 25f));
                    }
                }

                GameplayCamera.Shake(CameraShake.Jolt, 0.2f);
            }

            // -----------------------
            // TORNADO
            // -----------------------
            if (tornado)
            {
                foreach (Entity e in World.GetAllEntities())
                {
                    if (!e.IsPlayer)
                    {
                        Vector3 dir = e.Position - pPos;
                        Vector3 swirl = new Vector3(-dir.Y, dir.X, 0).Normalized;

                        e.ApplyForce((swirl * 20f + Vector3.WorldUp * 30f) * chaosIntensity);
                    }
                }

                GameplayCamera.Shake(CameraShake.SmallExplosion, 0.4f);
            }

            // -----------------------
            // CAR RAIN
            // -----------------------
            if (carRain)
            {
                Vector3 spawnPos = pPos + new Vector3(
                    World.GetRandomInt(-15, 15),
                    World.GetRandomInt(-15, 15),
                    30);

                World.CreateVehicle(VehicleHash.Adder, spawnPos);
            }

            // -----------------------
            // PED RAIN
            // -----------------------
            if (pedRain)
            {
                Vector3 spawnPos = pPos + new Vector3(
                    World.GetRandomInt(-10, 10),
                    World.GetRandomInt(-10, 10),
                    25);

                Ped ped = World.CreatePed(PedHash.Clown01SMY, spawnPos);
                ped.ApplyForce(new Vector3(0, 0, -50f));
            }

            // -----------------------
            // EXPLOSION RAIN
            // -----------------------
            if (explosionRain)
            {
                Vector3 explodePos = pPos + new Vector3(
                    World.GetRandomInt(-10, 10),
                    World.GetRandomInt(-10, 10),
                    Word.GetRandomInt(10, 20));

                World.AddExplosion(
                    explodePos,
                    ExplosionType.Grenade,
                    6f,
                    1f);
            }

            // -----------------------
            // CAR MAGNET
            // -----------------------
            if (carMagnet)
            {
                foreach (Vehicle v in World.GetAllVehicles())
                {
                    if (!v.Equals(player.CurrentVehicle))
                    {
                        Vector3 dir = pPos - v.Position;
                        v.ApplyForce(dir.Normalized * 15f * chaosIntensity);
                    }
                }
            }

            // -----------------------
            // PED MAGNET
            // -----------------------
            if (pedMagnet)
            {
                foreach (Ped ped in World.GetAllPeds())
                {
                    if (!ped.IsPlayer)
                    {
                        Vector3 dir = pPos - ped.Position;
                        ped.ApplyForce(dir.Normalized * 10f * chaosIntensity);
                    }
                }
            }

            // -----------------------
            // RANDOM CHAOS
            // -----------------------
            if (randomChaos)
            {
                int rnd = World.GetRandomInt(0, 300);

                if (rnd < 3) World.AddExplosion(pPos + RandomOffset(), ExplosionType.Rocket, 8f, 1f);
                if (rnd == 50) carRain = !carRain;
                if (rnd == 80) pedRain = !pedRain;
                if (rnd == 120) tornado = !tornado;
                if (rnd == 150) carMagnet = !carMagnet;
                if (rnd == 200) pedMagnet = !pedMagnet;
                if (rnd == 250) blackHole = !blackHole;
            }
        }

        private static Vector3 RandomOffset()
        {
            return new Vector3(
                World.GetRandomInt(-15, 15),
                World.GetRandomInt(-15, 15),
                1);
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
