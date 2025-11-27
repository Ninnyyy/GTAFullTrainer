using System.Drawing;

namespace GTAFullTrainer.UI
{
    public abstract class UIControl
    {
        public string Label;
        public bool Selected = false;

        public UIControl(string label)
        {
            Label = label;
        }

        public abstract void Draw(float x, float y);
        public abstract void OnActivate();
    }
}
