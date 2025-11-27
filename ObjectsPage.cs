using System.Collections.Generic;
using System.Drawing;
using GTA;
using GTA.Native;
using GTAFullTrainer.UI;
using GTAFullTrainer.Utils;
using GTAFullTrainer.Core;

namespace GTAFullTrainer.Pages
{
    public static class ObjectsPage
    {
        private static bool initialized = false;
        private static List<UIElement> items = new List<UIElement>();

        private static List<Prop> spawnedProps = new List<Prop>();

        private static string[] propModels =
        {
            "prop_chair_01a",
            "prop_table_01",
            "prop_barrel_02a",
            "prop_bin_05a",
            "prop_ld_health_pack",
            "prop_tv_02",
            "prop_tyre_01",
            "prop_roadcone02a",
            "prop_bench_01a",
            "prop_ld_fireaxe",
            "prop_ld_binbag",
        };

        private static string[] specialModes =
        {
            "None",
            "Delete Gun",
            "Object Gun",
            "Force Gun",
            "Object Thrower"
        };

        private static int selectedPropIndex = 0;

        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            // PROP SPAWN
            items.Add(new UIList("Select Prop", 0, propModels));
            items.Add(new UIButton("Spawn Prop In Front", 1, () =>
            {
                SpawnProp();
            }));

            items.Add(new UIButton("Attach Prop To Player", 2, () =>
            {
                AttachProp();
            }));

            items.Add(new UIButton("Throw Prop", 3, () =>
            {
                ThrowProp();
            }));

            // SPECIAL MODES
            items.Add(new UIList("Special Object Mode", 4, specialModes));

            // UTILITY
            items.Add(new UIButton("Clone Nearest Prop", 5, () =>
            {
                CloneNearestProp();
            }));

            items.Add(new UIButton("Delete Nearest Prop", 6, () =>
            {
                DeleteNearestProp();
            }));

            items.Add(new UIButton("Clear All Spawned Props", 7, () =>
            {
                foreach (var p in spawnedProps)
                    if (p.Exists()) p.Delete();

                spawnedProps.Clear();
            }));
        }

        public static void ApplyLogic()
        {
            Ped player = Game.Player.Character;

            int specialMode = ((UIList)items[4]).CurrentIndex;

            if (Game.IsKeyPressed(System.Windows.Forms.Keys.LButton))
            {
                Vector3 from = GameplayCamera.Position;
                Vector3 dir = GameplayCamera.Direction;
                Vector3 to = from + dir * 200f;

                RaycastResult r = World.Raycast(from, to, 200f);

                switch (specialMode)
                {
                    case 1: // DELETE GUN
                        if (r.DitHitEntity && r.HitEntity is Prop)
                            r.HitEntity.Delete();
                        break;

                    case 2: // OBJECT GUN
                        Prop p = World.CreateProp(
                            propModels[selectedPropIndex],
                            to, true, true);
                        spawnedProps.Add(p);
                        break;

                    case 3: // FORCE GUN
                        if (r.DitHitEntity)
                            r.HitEntity.ApplyForce(dir * 80f);
                        break;

                    case 4: // OBJECT THROWER
                        Prop pr = World.CreateProp(propModels[selectedPropIndex], player.Position, true, true);
                        pr.ApplyForce(dir * 50f);
                        spawnedProps.Add(pr);
                        break;
                }
            }
        }

        public static void SpawnProp()
        {
            Ped player = Game.Player.Character;
            Vector3 pos = player.Position + player.ForwardVector * 2f;

            Prop p = World.CreateProp(propModels[((UIList)items[0]).CurrentIndex], pos, true, true);
            spawnedProps.Add(p);
        }

        public static void AttachProp()
        {
            Ped player = Game.Player.Character;
            Prop p = World.CreateProp(propModels[((UIList)items[0]).CurrentIndex], player.Position, true, true);

            p.AttachTo(player, player.GetBoneIndex(Bone.SKEL_R_HAND),
                new Vector3(0.2f, 0f, -0.1f),
                new Vector3(0, 90, 0));

            spawnedProps.Add(p);
        }

        public static void ThrowProp()
        {
            Ped player = Game.Player.Character;
            Vector3 dir = GameplayCamera.Direction;

            Prop p = World.CreateProp(propModels[((UIList)items[0]).CurrentIndex], player.Position, true, true);
            p.ApplyForce(dir * 80f);

            spawnedProps.Add(p);
        }

        public static void CloneNearestProp()
        {
            Prop nearest = World.GetClosest<Prop>(Game.Player.Character.Position, 10f);
            if (nearest != null)
            {
                Prop clone = World.CreateProp(nearest.Model.Hash, nearest.Position, true, true);
                spawnedProps.Add(clone);
            }
        }

        public static void DeleteNearestProp()
        {
            Prop nearest = World.GetClosest<Prop>(Game.Player.Character.Position, 10f);
            if (nearest != null)
                nearest.Delete();
        }

        public static void Draw(int index)
        {
            Init();
            ApplyLogic();

            for (int i = 0; i < items.Count; i++)
            {
                items[i].Selected = i == index;
                items[i].Draw(800, 300 + i * 45);
            }
        }
    }
}
