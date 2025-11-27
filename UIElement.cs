using System.Drawing;

namespace GTAFullTrainer.UI
{
    public abstract class UIElement
    {
        public string Label;
        public int Index;
        public bool Selected;

        public UIElement(string label, int index)
        {
            Label = label;
            Index = index;
        }

        public abstract void Draw(int x, int y);
        public abstract void Activate();
        public virtual void Left() { }
        public virtual void Right() { }
    }
}
