using System.Drawing;
using GTAFullTrainer.Utils;
using GTAFullTrainer.Effects;

namespace GTAFullTrainer.UI
{
    public class UIList : UIControl
    {
        public string[] Items;
        public int Index = 0;

        public UIList(string label, string[] items) : base(label)
        {
            Items = items;
        }

        public override void Draw(float x, float y)
        {
            Rectangle rect = new Rectangle((int)x, (int)y, 500, 40);

            if (Selected)
                GTAFullTrainer.Effects.Glow.NeonRect(rect, ThemeManager.GlowColor, Animation.Pulse());

            DrawUtils.Text($"{Label}: {Items[Index]}",
                rect.X + 20, rect.Y + 10,
                0.50f,
                Color.White);
        }

        public override void OnActivate()
        {
            Index++;
            if (Index >= Items.Length) Index = 0;
        }
    }
}
