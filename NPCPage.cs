using System.Collections.Generic;
using System.Drawing;
using GTA;
using GTA.Native;
using GTAFullTrainer.UI;
using GTAFullTrainer.Utils;
using GTAFullTrainer.Core;

namespace GTAFullTrainer.Pages
{
    public static class NPCPage
    {
        private static List<UIElement> items = new List<UIElement>();
        private static bool initialized = false;

        private static List<Ped> bodyguards = new List<Ped>();
        private static List<Ped> hostilePeds = new List<Ped>();

        private static string[] npcModels =
        {
            "a_m_m_business_01",
            "a_m_m_skater_01",
            "a_f_y_hipster_01",
            "s_f_y_cop_01",
            "s_m_m_paramedic_01",
            "s_m_m_security_01",
            "u_m_y_imporage",
            "u_m_y_rsranger_01"
        };

        private static string[] animalModels =
        {
            "a_c_rottweiler",
            "a_c_chop",
            "a_c_husky",
            "a_c_cat_01",
            "a_c_cow",
            "a_c_deer",
            "a_c_pig",
            "a_c_sharktiger"
        };

        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            // NPC SPAWNING
            items.Add(new UIList("Spawn NPC", 0, npcModels));
            items.Add(new UIButton("Spawn NPC (in front)", 1, () =>
            {
                SpawnNPC(false);
            }));

            items.Add(new UIButton("Spawn Bodyguard", 2, () =>
            {
                SpawnNPC(true);
            }));

            items.Add(new UIList("Spawn Animal", 3, animalModels));
            items.Add(new UIButton("Spawn Animal (in front)", 4, () =>
            {
                SpawnAnimal();
            }));

            // PED CONTROL
            items.Add(new UIButton("Recruit Nearest Ped", 5, () =>
            {
                Ped p = World.GetClosestPed(Game.Player.Character.Position, 20f);
                if (p != null && !p.IsPlayer) RecruitPed(p);
            }));

            items.Add(new UIButton("Clone Nearest Ped", 6, () =>
            {
                Ped p = World.GetClosestPed(Game.Player.Character.Position, 20f);
                if (p != null && !p.IsPlayer) ClonePed(p);
            }));

            items.Add(new UIButton("Delete Nearest Ped", 7, () =>
            {
                Ped p = World.GetClosestPed(Game.Player.Character.Position, 20f);
                if (p != null && !p.IsPlayer) p.Delete();
            }));

            items.Add(new UIButton("Clear All Nearby NPCs", 8, () =>
            {
                foreach (Ped ped in World.GetAllPeds())
                {
                    if (!ped.IsPlayer)
                        ped.Delete();
                }
            }));

            // FOLLOW & ATTACK CONTROLS
            items.Add(new UIToggle("Followers Follow Player", 9));
            items.Add(new UIToggle("Hostiles Attack Player", 10));

            // GROUP COMMANDS
            items.Add(new UIButton("Make All Peds Friendly", 11, () =>
            {
                foreach (Ped ped in World.GetAllPeds())
                    if (!ped.IsPlayer)
                        ped.RelationshipGroup = Game.Player.Character.RelationshipGroup;
            }));

            items.Add(new UIButton("Make All Peds Hostile", 12, () =>
            {
                foreach (Ped ped in World.GetAllPeds())
                {
                    if (!ped.IsPlayer)
                    {
                        ped.Task.FightAgainst(Game.Player.Character);
                    }
                }
            }));

            items.Add(new UIButton("Give Bodyguards Weapons", 13, () =>
            {
                foreach (Ped g in bodyguards)
                    GiveWeapon(g);
            }));
        }

        public static void ApplyLogic()
        {
            Ped player = Game.Player.Character;

            // FOLLOWER LOGIC
            if (((UIToggle)items[9]).State)
            {
                foreach (Ped g in bodyguards)
                {
                    if (g != null && g.Exists())
                    {
                        g.Task.FollowToOffsetFromEntity(player, new Vector3(1f, -2f, 0), 2f, -1);
                    }
                }
            }

            // HOSTILE LOGIC
            if (((UIToggle)items[10]).State)
            {
                foreach (Ped h in hostilePeds)
                {
                    if (h != null && h.Exists())
                        h.Task.FightAgainst(player);
                }
            }
        }

        // ----------------------------------
        // SPAWN NPC OR BODYGUARD
        // ----------------------------------
        public static void SpawnNPC(bool bodyguard)
        {
            string modelName = npcModels[((UIList)items[0]).CurrentIndex];
            Ped p = CreatePed(modelName);

            if (bodyguard)
            {
                bodyguards.Add(p);
                SetupBodyguard(p);
            }
        }

        // SPAWN ANIMAL
        public static void SpawnAnimal()
        {
            string modelName = animalModels[((UIList)items[3]).CurrentIndex];
            CreatePed(modelName);
        }

        // CREATE A PED
        private static Ped CreatePed(string modelName)
        {
            Model m = new Model(modelName);
            m.Request(3000);

            Vector3 spawnPos = Game.Player.Character.Position +
                               Game.Player.Character.ForwardVector * 3f;

            Ped p = World.CreatePed(m, spawnPos);
            return p;
        }

        // TURN A PED INTO A BODYGUARD
        private static void SetupBodyguard(Ped p)
        {
            p.Weapons.Give(WeaponHash.CarbineRifle, 9999, true, true);
            p.Armor = 100;
            p.CanSwitchWeapons = true;

            // Same relationship group as player = ally
            Function.Call(Hash.SET_PED_RELATIONSHIP_GROUP_HASH, p.Handle,
                Game.Player.Character.RelationshipGroup);
        }

        // GIVE WEAPON TO PED
        private static void GiveWeapon(Ped p)
        {
            p.Weapons.Give(WeaponHash.SMG, 9999, true, true);
        }

        // RECRUIT PED AS BODYGUARD
        private static void RecruitPed(Ped p)
        {
            if (p == null) return;

            SetupBodyguard(p);
            bodyguards.Add(p);
        }

        // CLONE PED
        private static void ClonePed(Ped p)
        {
            Ped clone = World.CreatePed(p.Model, p.Position + new Vector3(1f, 0, 0));
            SetupBodyguard(clone);
        }

        // ----------------------------------
        // DRAW
        // ----------------------------------
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
