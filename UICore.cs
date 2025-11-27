using System;
using System.Collections.Generic;
using System.Drawing;
using GTA;
using GTA.Native;
using GTAFullTrainer.UI;
using GTAFullTrainer.Rendering;
using GTAFullTrainer.Effects;

namespace GTAFullTrainer.CoreUI
{
    public static class UICore
    {
        public static bool MenuOpen = false;

        public static int ActiveCategory = 0;
        public static int ActiveItem = 0;

        public static List<string> Categories = new List<string>();
        public static Dictionary<int, List<UIControl>> CategoryItems = new();

        // Smooth animation variables
        private static float slideX = -500f;
        private static float targetSlideX = 0f;

        // Theme
        public static float GlobalScale = 1.0f;

        public static void Initialize()
        {
            Categories.Clear();
            CategoryItems.Clear();
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

                // Play open SFX
                SoundManager.PlayOpen();

                // Apply background blur
                Function.Call(Hash._SET_SCREEN_EFFECT, "MenuMGIsland", 0, true);
            }
            else
            {
                targetSlideX = -600f;

                // Stop blur
                Function.Call(Hash._STOP_ALL_SCREEN_EFFECTS);

                // Play close SFX
                SoundManager.PlayClose();
            }
        }

        public static void Process()
        {
            if (!MenuOpen) return;

            // Smooth slide animation
            slideX = Animation.Smooth(slideX, targetSlideX, 0.12f);

            // Draw backdrop
            NewUIRenderer.DrawBackdrop();

            // Draw animated sidebar (categories)
            NewUIRenderer.DrawCategories(slideX, Categories, ActiveCategory);

            // Draw the active category’s items
            if (CategoryItems.ContainsKey(ActiveCategory))
                NewUIRenderer.DrawItems(slideX + 350, CategoryItems[ActiveCategory], ActiveItem);
        }

        public static void HandleInput()
        {
            if (!MenuOpen) return;

            if (Game.IsKeyPressed(System.Windows.Forms.Keys.Up))
            {
                ActiveItem--;
                if (ActiveItem < 0) ActiveItem = CategoryItems[ActiveCategory].Count - 1;

                SoundManager.PlayNavigate();
                Script.Wait(120);
            }

            if (Game.IsKeyPressed(System.Windows.Forms.Keys.Down))
            {
                ActiveItem++;
                if (ActiveItem >= CategoryItems[ActiveCategory].Count) ActiveItem = 0;

                SoundManager.PlayNavigate();
                Script.Wait(120);
            }

            if (Game.IsKeyPressed(System.Windows.Forms.Keys.Left))
            {
                ActiveCategory--;
                if (ActiveCategory < 0) ActiveCategory = Categories.Count - 1;

                ActiveItem = 0;

                SoundManager.PlayNavigate();
                Script.Wait(120);
            }

            if (Game.IsKeyPressed(System.Windows.Forms.Keys.Right))
            {
                ActiveCategory++;
                if (ActiveCategory >= Categories.Count) ActiveCategory = 0;

                ActiveItem = 0;

                SoundManager.PlayNavigate();
                Script.Wait(120);
            }

            if (Game.IsKeyPressed(System.Windows.Forms.Keys.Enter))
            {
                CategoryItems[ActiveCategory][ActiveItem].OnActivate();
                SoundManager.PlaySelect();
                Script.Wait(150);
            }
        }
    }
}
