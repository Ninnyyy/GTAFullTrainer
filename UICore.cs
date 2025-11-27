using System;
using System.Collections.Generic;
using GTA;
using GTA.Native;
using System.Windows.Forms;
using NinnyTrainer.UI;
using NinnyTrainer.Rendering;
using NinnyTrainer.Effects;

namespace NinnyTrainer.CoreUI
{
    public static class UICore
    {
        public static bool MenuOpen = false;

        public static int ActiveCategory = 0;
        public static int ActiveItem = 0;

        public static List<string> Categories = new();
        public static Dictionary<int, List<UIControl>> CategoryItems = new();

        private static float slideX = -600f;
        private static float targetSlideX = 0f;

        public static void Initialize()
        {
            Categories.Clear();
            CategoryItems.Clear();
            ActiveCategory = 0;
            ActiveItem = 0;
        }

        public static void RegisterCategory(string name, List<UIControl> items)
        {
            int index = Categories.Count;
            Categories.Add(name);
            CategoryItems[index] = items;
        }

        public static void ToggleMenu()
        {
            MenuOpen = !MenuOpen;

            if (MenuOpen)
            {
                slideX = -600f;
                targetSlideX = 0f;
                SoundManager.PlayOpen();
                Function.Call(Hash._SET_SCREEN_EFFECT, "MenuMGIsland", 0, true);
            }
            else
            {
                targetSlideX = -600f;
                SoundManager.PlayClose();
                Function.Call(Hash._STOP_ALL_SCREEN_EFFECTS);
            }
        }

        public static void Process()
        {
            if (!MenuOpen) return;

            slideX = Animation.Smooth(slideX, targetSlideX, 0.12f);

            NewUIRenderer.DrawBackdrop();
            NewUIRenderer.DrawCategories(slideX, Categories, ActiveCategory);

            if (CategoryItems.ContainsKey(ActiveCategory))
                NewUIRenderer.DrawItems(slideX + 350, CategoryItems[ActiveCategory], ActiveItem);
        }

        public static void HandleInput()
        {
            if (!MenuOpen) return;

            if (Game.IsKeyPressed(Keys.Up))
            {
                ActiveItem--;
                if (ActiveItem < 0) ActiveItem = CategoryItems[ActiveCategory].Count - 1;
                SoundManager.PlayNavigate();
                Script.Wait(120);
            }

            if (Game.IsKeyPressed(Keys.Down))
            {
                ActiveItem++;
                if (ActiveItem >= CategoryItems[ActiveCategory].Count) ActiveItem = 0;
                SoundManager.PlayNavigate();
                Script.Wait(120);
            }

            if (Game.IsKeyPressed(Keys.Left))
            {
                ActiveCategory--;
                if (ActiveCategory < 0) ActiveCategory = Categories.Count - 1;
                ActiveItem = 0;
                SoundManager.PlayNavigate();
                Script.Wait(120);
            }

            if (Game.IsKeyPressed(Keys.Right))
            {
                ActiveCategory++;
                if (ActiveCategory >= Categories.Count) ActiveCategory = 0;
                ActiveItem = 0;
                SoundManager.PlayNavigate();
                Script.Wait(120);
            }

            if (Game.IsKeyPressed(Keys.Enter))
            {
                CategoryItems[ActiveCategory][ActiveItem].OnActivate();
                SoundManager.PlaySelect();
                Script.Wait(150);
            }
        }
    }
}
