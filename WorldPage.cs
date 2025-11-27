using System.Collections.Generic;
using System.Drawing;
using GTA;
using GTA.Native;
using GTAFullTrainer.UI;
using GTAFullTrainer.Utils;
using GTAFullTrainer.Core;

namespace GTAFullTrainer.Pages
{
    public static class WorldPage
    {
        private static List<UIElement> items = new List<UIElement>();
        private static bool initialized = false;

        // Toggles
        private static bool freezeWeather = false;
        private static bool freezeTime = false;
        private static bool blackout = false;
        private static bool lowGravity = false;
        private static bool disableCops = false;
        private static bool riotMode = false;
        private static bool zombieMode = false;
        private static bool worldOnFire = false;
        private static bool earthquake = false;
        private static bool meteorShower = false;

        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            // WEATHER
            items.Add(new UIList("Weather", 0,
                new string[] {
                    "EXTRASUNNY",
                    "CLEAR",
                    "CLOUDS",
                    "SMOG",
                    "FOGGY",
                    "OVERCAST",
                    "RAIN",
                    "THUNDER",
                    "SNOW",
                    "BLIZZARD",
                    "SNOWLIGHT"
                }));

            items.Add(new UIToggle("Freeze Weather", 1));

            // TIME
            items.Add(new UISlider("Hour", 2, 0, 23, World.CurrentTime.Hour));
            items.Add(new UISlider("Minute", 3, 0, 59, World.CurrentTime.Minute));
            items.Add(new UIToggle("Freeze Time", 4));

            // WORLD
            items.Add(new UIToggle("Blackout", 5));
            items.Add(new UIToggle("Low Gravity", 6));
            items.Add(new UIToggle("Disable Cops", 7));
            items.Add(new UIToggle("Riot Mode", 8));
            items.Add(new UIToggle("Zombie Mode", 9));
            items.Add(new UIToggle("World On Fire", 10));
            items.Add(new UIToggle("Earthquake Mode", 11));
            items.Add(new UIToggle("Meteor Shower", 12));

            // DENSITY
            items.Add(new UISlider("Traffic Density", 13, 0f, 1f, 1f));
            items.Add(new UISlider("Ped Density", 14, 0f, 1f, 1f));

            // UTILITY
            items.Add(new UIButton("Clear All Peds", 15, () =>
            {
                foreach (Ped p in World.GetAllPeds())
                {
                    if (!p.IsPlayer)
                        p.Delete();
                }
            }));

            items.Add(new UIButton("Clear All Vehicles", 16, () =>
            {
                foreach (Vehicle v in World.GetAllVehicles())
                {
                    if (!v.Equals(Game.Player.Character.CurrentVehicle))
                        v.Delete();
                }
            }));
        }

        public static void ApplyLogic()
        {
            freezeWeather = ((UIToggle)items[1]).State;
            freezeTime = ((UIToggle)items[4]).State;
            blackout = ((UIToggle)items[5]).State;
            lowGravity = ((UIToggle)items[6]).State;
            disableCops = ((UIToggle)items[7]).State;
            riotMode = ((UIToggle)items[8]).State;
            zombieMode = ((UIToggle)items[9]).State;
            worldOnFire = ((UIToggle)items[10]).State;
            earthquake = ((UIToggle)items[11]).State;
            meteorShower = ((UIToggle)items[12]).State;

            float traffic = ((UISlider)items[13]).Value;
            float peds = ((UISlider)items[14]).Value;

            // WEATHER
            if (!freezeWeather)
            {
                string weather = ((UIList)items[0]).Options[((UIList)items[0]).CurrentIndex];
                Function.Call(Hash.SET_WEATHER_TYPE_NOW_PERSIST, weather);
            }

            // TIME
            if (!freezeTime)
            {
                int hour = (int)((UISlider)items[2]).Value;
                int minute = (int)((UISlider)items[3]).Value;

                World.CurrentTime = new System.TimeSpan(hour, minute, 0);
            }

            // BLACKOUT
            Function.Call(Hash.SET_ARTIFICIAL_LIGHTS_STATE, blackout);

            // LOW GRAVITY
            if (lowGravity)
                Function.Call(Hash.SET_GRAVITY_LEVEL, 1); // Moon gravity
            else
                Function.Call(Hash.SET_GRAVITY_LEVEL, 0);

            // DISABLE COPS
            if (disableCops)
                Function.Call(Hash.SET_CREATE_RANDOM_COPS, false);
            else
                Function.Call(Hash.SET_CREATE_RANDOM_COPS, true);

            // TRAFFIC / PEDS
            Function.Call(Hash.SET_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME, traffic);
            Function.Call(Hash.SET_RANDOM_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME, traffic);
            Function.Call(Hash.SET_PARKED_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME, traffic);

            Function.Call(Hash.SET_PED_DENSITY_MULTIPLIER_THIS_FRAME, peds);
            Function.Call(Hash.SET_SCENARIO_PED_DENSITY_MULTIPLIER_THIS_FRAME, peds, peds);

            // RIOT MODE
            if (riotMode)
                Function.Call(Hash.SET_RANDOM_EVENT_FLAG, true);

            // ZOMBIE MODE
            if (zombieMode)
            {
                foreach (Ped ped in World.GetAllPeds())
                {
                    if (!ped.IsPlayer)
                    {
                        ped.Task.FightAgainst(Game.Player.Character);
                    }
                }
            }

            // WORLD ON FIRE
            if (worldOnFire)
            {
                Vector3 pos = Game.Player.Character.Position;
                World.AddExplosion(pos + new Vector3(World.GetRandomInt(-5, 5), World.GetRandomInt(-5, 5), 0),
                    GTA.ExplosionType.Molotov, 1f, 0.1f);
            }

            // EARTHQUAKE
            if (earthquake)
            {
                GameplayCamera.Shake(CameraShake.SmallExplosion, 1.5f);
                Game.Player.Character.ApplyForce(new Vector3(
                    World.GetRandomFloat(-1f, 1f),
                    World.GetRandomFloat(-1f, 1f),
                    World.GetRandomFloat(-1f, 1f)
                ));
            }

            // METEOR SHOWER
            if (meteorShower)
            {
                Vector3 pPos = Game.Player.Character.Position;
                Vector3 strike = pPos + new Vector3(
                    World.GetRandomInt(-25, 25),
                    World.GetRandomInt(-25, 25),
                    30);

                World.AddExplosion(strike,
                    GTA.ExplosionType.Rocket,
                    10f,
                    1f);
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
