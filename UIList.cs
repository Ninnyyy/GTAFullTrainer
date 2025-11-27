using System;
using System.Drawing;
using NinnyTrainer.Utils;
using NinnyTrainer.Effects;

namespace NinnyTrainer.UI
{
    public class UIList : UIControl
    {
        public string[] Items;
        public int Index = 0;
        public Action<int> OnListChangeLogic;

        public UIList(string label, string[] items, Action<int> callback = null)
            : base(label)
        {
            Items = items;
            OnListChangeLogic = callback;
        }

        public override void Draw(float x, float y)
        {
            Rectangle rect = new Rectangle((int)x, (int)y, 500, 40);

            if (Selected)
                Glow.NeonRect(rect, ThemeManager.GlowColor, Animation.Pulse());

            DrawUtils.Text($"{Label}: {Items[Index]}", rect.X + 20, rect.Y + 10,
                0.50f, Color.White);
        }

        public override void OnActivate()
        {
            Index++;
            if (Index >= Items.Length) Index = 0;
            OnListChangeLogic?.Invoke(Index);
        }
    }
}
