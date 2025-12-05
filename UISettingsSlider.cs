using GTA.UI;
using GTAFullTrainer.Core;
using System;
using System.Drawing;

namespace GTAFullTrainer.UI
{
    /// <summary>
    /// Simple slider element that persists float values and supports left/right adjustments.
    /// </summary>
    public class UISettingsSlider : UIElement
    {
        private readonly float min;
        private readonly float max;
        private readonly float step;
        private float value;
        private readonly string configKey;
        private readonly Action<float>? onChange;

        public UISettingsSlider(string label, int index, string configKey, float min, float max, float defaultValue, float step = 0.05f, Action<float>? onChange = null)
            : base(label, index)
        {
            this.min = min;
            this.max = max;
            this.step = step;
            this.configKey = configKey;
            this.onChange = onChange;

            string stored = ConfigManager.Get(configKey, defaultValue.ToString());
            if (!float.TryParse(stored, out value))
            {
                value = defaultValue;
                ConfigManager.Set(configKey, value.ToString("0.00"));
                ConfigManager.Save();
            }
            value = Math.Clamp(value, min, max);
        }

        public override void Draw(int x, int y)
        {
            Color textColor = Selected ? Color.FromArgb(200, 155, 77, 255) : Color.White;
            Color accent = Color.FromArgb(200, 155, 77, 255);

            new UIResText($"{Label}: {value:0.00}",
                new Point(x, y),
                0.45f,
                textColor)
                .Draw();

            if (Selected)
            {
                new UIResRectangle(new Point(x - 8, y + 22), new Size(360, 4), accent).Draw();
            }
        }

        public override void Activate()
        {
            Increment(step);
        }

        public override void Left()
        {
            Increment(-step);
        }

        public override void Right()
        {
            Increment(step);
        }

        private void Increment(float delta)
        {
            value = Math.Clamp(value + delta, min, max);
            Persist();
        }

        private void Persist()
        {
            ConfigManager.Set(configKey, value.ToString("0.00"));
            ConfigManager.Save();
            onChange?.Invoke(value);
        }
    }
}
