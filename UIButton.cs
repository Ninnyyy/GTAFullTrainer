using System;
using System.Drawing;
using NinnyTrainer.Effects;
using NinnyTrainer.Utils;

namespace NinnyTrainer.UI
{
    public class UIButton : UIControl
    {
        private Action callback;

        public UIButton(string label, Action action) : base(label)
        {
            callback = action;
        }

        public override void Draw(float x, float y)
        {
            Rectangle rect = new Rectangle((int)x, (int)y, 500, 40);

            if (Selected)
                Glow.NeonRect(rect, ThemeManager.GlowColor, Animation.Pulse(3));

            DrawUtils.Text(Label, rect.X + 20, rect.Y + 10,
                Selected ? 0.55f : 0.50f,
                Selected ? ThemeManager.MainColor : Color.White);
        }

        public override void OnActivate()
        {
            callback?.Invoke();
        }
    }
}
