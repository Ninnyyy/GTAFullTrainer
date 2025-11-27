using System.Drawing;
using GTAFullTrainer.Utils;
using GTAFullTrainer.Effects;

namespace GTAFullTrainer.UI
{
    public class UIToggle : UIControl
    {
        public bool State;

        private float anim = 0f; // 0 = off, 1 = on

        public UIToggle(string label, bool defaultState = false) : base(label)
        {
            State = defaultState;
            anim = State ? 1f : 0f;
        }

        public override void Draw(float x, float y)
        {
            anim = Animation.Smooth(anim, State ? 1f : 0f, 0.18f);

            Rectangle rect = new Rectangle((int)x, (int)y, 500, 40);

            // Glow outline only if selected
            if (Selected)
                GTAFullTrainer.Effects.Glow.NeonRect(rect, ThemeManager.GlowColor, Animation.Pulse());

            // Label
            DrawUtils.Text(Label, rect.X + 20, rect.Y + 10, 0.50f, Color.White);

            // Toggle box
            Rectangle box = new Rectangle(rect.X + 430, rect.Y + 10, 50, 20);
            DrawUtils.Rect(box, Color.FromArgb(60, 60, 60));

            // Knob
            int knobX = (int)(box.X + anim * 25);
            DrawUtils.Rect(new Rectangle(knobX, box.Y, 25, 20), ThemeManager.MainColor);
        }

        public override void OnActivate()
        {
            State = !State;
        }
    }
}
