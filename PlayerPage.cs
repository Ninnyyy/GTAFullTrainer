using System.Collections.Generic;
using System.Drawing;
using GTA;
using GTA.Native;
using GTA.UI;
using GTAFullTrainer.UI;
using GTAFullTrainer.Utils;
using GTAFullTrainer.Core;

namespace GTAFullTrainer.Pages
{
    public static class PlayerPage
    {
        private static List<UIElement> items = new List<UIElement>();
        private static bool initialized = false;

        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            items.Add(new UIToggle("God Mode", 0));
            items.Add(new UIToggle("Infinite Health", 1));
            items.Add(new UIToggle("Infinite Armor", 2));
            items.Add(new UIToggle("Infinite Stamina", 3));
            items.Add(new UIToggle("No Ragdoll", 4));
            items.Add(new UIToggle("Invisible Player", 5));

            items.Add(new UISlider("Run Speed", 6, 1f, 5f, 1f));
            items.Add(new UISlider("Swim Speed", 7, 1f, 5f, 1f));
            items.Add(new UISlider("Jump Multiplier", 8, 1f, 5f, 1f));

            items.Add(new UIList("Wanted Level", 9,
                new string[] { "0", "1", "2", "3", "4", "5" }));

            items.Add(new UIButton("Set Max Wanted", 10, () =>
            {
                Game.Player.WantedLevel = 5;
            }));

            items.Add(new UIButton("Clear Wanted", 11, () =>
            {
                Game.Player.WantedLevel = 0;
            }));

            items.Add(new UIList("Change Model", 12,
                new string[] { "Michael", "Franklin", "Trevor", "Cop", "Clown" }));
        }

        public static void ApplyLogic()
        {
            Ped p = Game.Player.Character;

            // Toggles
            if (((UIToggle)items[0]).State)
                p.IsInvincible = true;
            else
                p.IsInvincible = false;

            if (((UIToggle)items[1]).State)
                p.Health = p.MaxHealth;

            if (((UIToggle)items[2]).State)
                p.Armor = 100;

            if (((UIToggle)items[3]).State)
                p.Stamina = 100f;

            if (((UIToggle)items[4]).State)
                Function.Call(Hash.SET_PED_CAN_RAGDOLL, p.Handle, false);

            if (((UIToggle)items[5]).State)
                p.IsVisible = false;
            else
                p.IsVisible = true;

            // Movement multipliers
            float run = ((UISlider)items[6]).Value;
            float swim = ((UISlider)items[7]).Value;
            float jump = ((UISlider)items[8]).Value;

            p.SetMoveSpeedMultiplier(run);
            p.SetSwimSpeedMultiplier(swim);
            Function.Call(Hash.SET_SUPER_JUMP_THIS_FRAME, Game.Player.Handle);

            // Wanted level
            int wanted = ((UIList)items[9]).CurrentIndex;
            Game.Player.WantedLevel = wanted;

            // Model switcher
            string model = ((UIList)items[12]).Options[((UIList)items[12]).CurrentIndex];

            if (InputManager.NavSelect() && items[12].Selected)
            {
                uint hash = 0;

                switch (model)
                {
                    case "Michael": hash = (uint)PedHash.Michael; break;
                    case "Franklin": hash = (uint)PedHash.Franklin; break;
                    case "Trevor": hash = (uint)PedHash.Trevor; break;
                    case "Cop": hash = (uint)PedHash.Cop01SFY; break;
                    case "Clown": hash = (uint)PedHash.Clown01SMY; break;
                }

                if (hash != 0)
                {
                    Function.Call(Hash.SET_PLAYER_MODEL, Game.Player, hash);
                    p = Game.Player.Character;
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
