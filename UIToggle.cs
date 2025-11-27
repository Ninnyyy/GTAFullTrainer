using System;
using System.Drawing;
using NinnyTrainer.Utils;
using NinnyTrainer.Effects;

namespace NinnyTrainer.UI
{
    public class UIToggle : UIControl
    {
        public bool State;
        public Action<bool> OnToggleLogic;

        private float anim = 0f;

        public UIToggle(string label, bool defaultState, Action<bool> callback = null) : base(label)
        {
            State = defaultState;
            anim = State ? 1f : 0f;
            OnToggleLogic = callback;
        }

        public override void Draw(float x, float y)
        {
            anim = Animation.Smooth(anim, State ? 1f : 0f, 0.20f);

            Rectangle rect = new Rectangle((int)x, (int)y, 500, 40);

            if (Selected)
                Glow.NeonRect(rect, ThemeManager.GlowColor, Animation.Pulse());

            DrawUtils.Text(Label, rect.X + 20, rect.Y + 10, 0.50f, Color.White);

            Rectangle box = new Rectangle(rect.X + 430, rect.Y + 10, 50, 20);
            DrawUtils.Rect(box, Color.FromArgb(60, 60, 60));

            int knobX = (int)(box.X + anim * 25);
            DrawUtils.Rect(new Rectangle(knobX, box.Y, 25, 20), ThemeManager.MainColor);
        }

        public override void OnActivate()
        {
            State = !State;
            OnToggleLogic?.Invoke(State);
        }
    }
}
