using System;
using System.Drawing;
using GTAFullTrainer.Effects;
using GTAFullTrainer.Utils;

namespace GTAFullTrainer.UI
{
    public class UIButton : UIControl
    {
        private Action callback;

        public UIButton(string label, Action callback) : base(label)
        {
            this.callback = callback;
        }

        public override void Draw(float x, float y)
        {
            Color text = Selected ? ThemeManager.MainColor : Color.White;
            float glow = Selected ? Animation.Pulse(3) : 0f;

            Rectangle rect = new Rectangle((int)x, (int)y, 500, 40);

            // Glow + base button
            GTAFullTrainer.Effects.Glow.NeonRect(rect, ThemeManager.GlowColor, glow);

            DrawUtils.Text(Label,
                rect.X + 20,
                rect.Y + 8,
                Selected ? 0.55f : 0.50f,
                text);
        }

        public override void OnActivate()
        {
            callback?.Invoke();
        }
    }
}
