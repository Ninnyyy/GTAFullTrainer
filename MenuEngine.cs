using GTA;
using GTA.Native;
using GTA.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using NinnyTrainer.Effects;

namespace GTAFullTrainer.Core
{
    public static class MenuEngine
    {
        public static bool IsOpen = false;

        private static int selectedCategory = 0;
        private static int selectedIndex = 0;

        private static float openAnim = 0f;
        private static float animSpeed = 0.12f;
        private static bool animationsEnabled = true;
        private static float uiScale = 1.0f;
        private static float categoryBarY = 0f;

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

        public static void SetAnimationsEnabled(bool enabled)
        {
            animationsEnabled = enabled;
            if (!animationsEnabled)
            {
                openAnim = 1f;
            }
        }

        public static void SetAnimationSpeed(float speed)
        {
            animSpeed = Math.Max(0.0f, Math.Min(speed, 0.5f));
        }

        public static void SetUiScale(float scale)
        {
            uiScale = Math.Max(0.5f, Math.Min(scale, 1.8f));
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
            float target = IsOpen ? 1f : 0f;

            if (!animationsEnabled)
            {
                openAnim = target;
                return;
            }

            openAnim = AnimationEngine.Damp(openAnim, target, animSpeed);
        }

        private static void DrawMenu()
        {
            float eased = AnimationEngine.EaseOut(openAnim);

            float width = 0.34f * eased * uiScale;
            float height = 0.54f * uiScale;
            float x = 0.03f + (0.01f * (1f - eased));
            float y = 0.18f + (0.02f * (1f - eased));

            // Backdrop and gradient glow
            new UIResRectangle(new Point(0, 0), new Size(1920, 1080), Color.FromArgb((int)(80 * eased), Theme.Black.R, Theme.Black.G, Theme.Black.B)).Draw();

            if (eased <= 0.01f) return;

            var baseRect = new Rectangle((int)(x * 1920), (int)(y * 1080), (int)(width * 1920), (int)(height * 1080));
            new UIResRectangle(baseRect.Location, baseRect.Size, Color.FromArgb(200, Theme.DarkGrey.R, Theme.DarkGrey.G, Theme.DarkGrey.B)).Draw();

            // Soft gradient overlay
            new UIResRectangle(baseRect.Location, baseRect.Size, Theme.SoftGradientTop).Draw();
            new UIResRectangle(baseRect.Location, baseRect.Size, Theme.SoftGradientBottom).Draw();

            DrawCategories(x, y, width, height);
            DrawCurrentPage(x, y, width, height);
        }

        private static void DrawCategories(float x, float y, float width, float height)
        {
            float categoryWidth = width * 0.35f;
            float indicatorHeight = 0.035f;

            for (int i = 0; i < categories.Count; i++)
            {
                float entryHeight = 0.04f;
                float entryY = y + 0.02f + (entryHeight * i);

                Color color = (i == selectedCategory)
                    ? Theme.Purple
                    : Color.FromArgb(200, Theme.TextDim.R, Theme.TextDim.G, Theme.TextDim.B);

                new UIResText(categories[i],
                    new Point((int)((x + 0.01f) * 1920), (int)(entryY * 1080)),
                    0.38f,
                    color)
                    .Draw();

                if (i == selectedCategory)
                    categoryBarY = AnimationEngine.Damp(categoryBarY, entryY, 0.22f);
            }

            // Animated category bar
            float pulse = AnimationEngine.Bounce(Game.GameTime / 1800f);
            int pulseBlue = Math.Min(255, Theme.PurpleGlow.B + (int)(20 * pulse));
            new UIResRectangle(
                new Point((int)((x + categoryWidth - 0.005f) * 1920), (int)(categoryBarY * 1080)),
                new Size((int)(0.004f * 1920), (int)(indicatorHeight * 1080)),
                Color.FromArgb(220, Theme.PurpleGlow.R, Theme.PurpleGlow.G, pulseBlue))
                .Draw();
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
