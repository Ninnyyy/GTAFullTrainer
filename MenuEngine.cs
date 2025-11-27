using GTA;
using GTA.Native;
using GTA.UI;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace GTAFullTrainer.Core
{
    public static class MenuEngine
    {
        public static bool IsOpen = false;

        private static int selectedCategory = 0;
        private static int selectedIndex = 0;

        private static float openAnim = 0f;
        private static float animSpeed = 0.08f;

        private static Dictionary<string, Action> pages = new Dictionary<string, Action>();

        private static List<string> categories = new List<string>
        {
            "Player",
            "Weapons",
            "Vehicles",
            "World",
            "Teleport",
            "NPC",
            "Objects",
            "Camera",
            "Chaos",
            "Developer",
            "Settings"
        };

        public static void Initialize()
        {
            RegisterPages();
        }

        public static void RegisterPages()
        {
            pages["Player"] = () => Pages.PlayerPage.Draw(selectedIndex);
            pages["Weapons"] = () => Pages.WeaponsPage.Draw(selectedIndex);
            pages["Vehicles"] = () => Pages.VehiclesPage.Draw(selectedIndex);
            pages["World"] = () => Pages.WorldPage.Draw(selectedIndex);
            pages["Teleport"] = () => Pages.TeleportPage.Draw(selectedIndex);
            pages["NPC"] = () => Pages.NPCPage.Draw(selectedIndex);
            pages["Objects"] = () => Pages.ObjectsPage.Draw(selectedIndex);
            pages["Camera"] = () => Pages.CameraPage.Draw(selectedIndex);
            pages["Chaos"] = () => Pages.ChaosPage.Draw(selectedIndex);
            pages["Developer"] = () => Pages.DevPage.Draw(selectedIndex);
            pages["Settings"] = () => Pages.SettingsPage.Draw(selectedIndex);
        }

        public static void OpenMenu()
        {
            IsOpen = true;
        }

        public static void CloseMenu()
        {
            IsOpen = false;
        }

        public static void Update()
        {
            AnimateOpenClose();

            if (!IsOpen && openAnim <= 0f) return;

            HandleNavigation();
            DrawMenu();
        }

        private static void HandleNavigation()
        {
            if (InputManager.NavUp())
                selectedIndex--;

            if (InputManager.NavDown())
                selectedIndex++;

            if (InputManager.NavLeft())
                selectedCategory--;

            if (InputManager.NavRight())
                selectedCategory++;

            if (selectedCategory < 0) selectedCategory = categories.Count - 1;
            if (selectedCategory >= categories.Count) selectedCategory = 0;

            if (selectedIndex < 0) selectedIndex = 0;
            if (selectedIndex > 20) selectedIndex = 20; // Placeholder max
        }

        private static void AnimateOpenClose()
        {
            if (IsOpen)
            {
                if (openAnim < 1f)
                    openAnim += animSpeed;
                if (openAnim > 1f)
                    openAnim = 1f;
            }
            else
            {
                if (openAnim > 0f)
                    openAnim -= animSpeed;
                if (openAnim < 0f)
                    openAnim = 0f;
            }
        }

        private static void DrawMenu()
        {
            float width = 0.32f * openAnim;
            float height = 0.52f;
            float x = 0.03f;
            float y = 0.20f;

            // Background panel
            new UIResRectangle(new Point((int)(x * 1920), (int)(y * 1080)),
                new Size((int)(width * 1920), (int)(height * 1080)),
                Color.FromArgb(180, 15, 15, 15))
                .Draw();

            if (openAnim <= 0.01f) return;

            DrawCategories(x, y, width, height);
            DrawCurrentPage(x, y, width, height);
        }

        private static void DrawCategories(float x, float y, float width, float height)
        {
            float categoryWidth = width * 0.35f;

            for (int i = 0; i < categories.Count; i++)
            {
                float entryHeight = 0.04f;
                float entryY = y + 0.02f + (entryHeight * i);

                Color color = (i == selectedCategory)
                    ? Color.FromArgb(255, 155, 77, 255)
                    : Color.FromArgb(200, 90, 90, 90);

                new UIResText(categories[i],
                    new Point((int)((x + 0.01f) * 1920), (int)(entryY * 1080)),
                    0.38f,
                    color)
                    .Draw();

                if (i == selectedCategory)
                {
                    new UIResRectangle(
                        new Point((int)((x + categoryWidth - 0.005f) * 1920), (int)(entryY * 1080)),
                        new Size((int)(0.004f * 1920), (int)(0.035f * 1080)),
                        Color.FromArgb(255, 155, 77, 255))
                        .Draw();
                }
            }
        }

        private static void DrawCurrentPage(float x, float y, float width, float height)
        {
            float contentX = x + width * 0.40f;

            string current = categories[selectedCategory];

            if (pages.ContainsKey(current))
                pages[current].Invoke();
            else
                new UIResText("Page not implemented",
                    new Point((int)(contentX * 1920), (int)(y * 1080)),
                    0.45f,
                    Color.White)
                    .Draw();
        }
    }
}
