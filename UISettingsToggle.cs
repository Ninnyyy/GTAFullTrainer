using GTA.UI;
using GTAFullTrainer.Core;
using System;
using System.Drawing;

namespace GTAFullTrainer.UI
{
    /// <summary>
    /// Lightweight toggle element for the classic trainer menu that persists state to the config file.
    /// </summary>
    public class UISettingsToggle : UIElement
    {
        private bool state;
        private readonly string configKey;
        private readonly Action<bool>? onChange;

        public UISettingsToggle(string label, int index, string configKey, bool defaultValue, Action<bool>? onChange = null)
            : base(label, index)
        {
            this.configKey = configKey;
            this.onChange = onChange;

            string stored = ConfigManager.Get(configKey, defaultValue.ToString());
            if (!bool.TryParse(stored, out state))
            {
                state = defaultValue;
                ConfigManager.Set(configKey, state.ToString());
                ConfigManager.Save();
            }
        }

        public override void Draw(int x, int y)
        {
            Color textColor = Selected ? Color.FromArgb(200, 155, 77, 255) : Color.White;
            Color accent = Color.FromArgb(200, 155, 77, 255);

            new UIResText(Label,
                new Point(x, y),
                0.45f,
                textColor)
                .Draw();

            string valueText = state ? "On" : "Off";
            new UIResText(valueText,
                new Point(x + 300, y),
                0.45f,
                accent)
                .Draw();

            if (Selected)
            {
                new UIResRectangle(new Point(x - 8, y + 22), new Size(360, 4), accent).Draw();
            }
        }

        public override void Activate()
        {
            state = !state;
            Persist();
        }

        public override void Left()
        {
            state = !state;
            Persist();
        }

        public override void Right()
        {
            state = !state;
            Persist();
        }

        private void Persist()
        {
            ConfigManager.Set(configKey, state.ToString());
            ConfigManager.Save();
            onChange?.Invoke(state);
        }
    }
}
