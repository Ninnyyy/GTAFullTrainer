using System.Drawing;
using GTAFullTrainer.Utils;
using GTAFullTrainer.Effects;

namespace GTAFullTrainer.UI
{
    public class UISlider : UIControl
    {
        public float Min, Max;
        public float Value;

        private float animValue;

        public UISlider(string label, float min, float max, float def) : base(label)
        {
            Min = min;
            Max = max;
            Value = def;
            animValue = def;
        }

        public override void Draw(float x, float y)
        {
            animValue = Animation.Smooth(animValue, Value, 0.25f);

            Rectangle rect = new Rectangle((int)x, (int)y, 500, 40);

            if (Selected)
                GTAFullTrainer.Effects.Glow.NeonRect(rect, ThemeManager.GlowColor, Animation.Pulse());

            DrawUtils.Text(Label + $"  [{Value:0.00}]",
                rect.X + 20, rect.Y + 10,
                0.50f,
                Color.White);

            Rectangle bar = new Rectangle(rect.X + 20, rect.Y + 28, 460, 6);
            DrawUtils.Rect(bar, Color.FromArgb(60, 60, 60));

            int fill = (int)(animValue / Max * 460);
            Rectangle fillRect = new Rectangle(bar.X, bar.Y, fill, 6);

            DrawUtils.Rect(fillRect, ThemeManager.MainColor);
        }

        public override void OnActivate()
        {
            Value += 0.1f;
            if (Value > Max) Value = Min;
        }
    }
}
