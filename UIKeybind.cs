using System.Drawing;
using System.Windows.Forms;
using GTAFullTrainer.Core;
using GTAFullTrainer.Utils;

namespace GTAFullTrainer.UI
{
    public class UIKeybind : UIElement
    {
        private Keys boundKey;
        private readonly string configKey;

        public UIKeybind(string label, int index, string configName, Keys defaultKey)
            : base(label, index)
        {
            configKey = configName;
            string stored = ConfigManager.Get(configKey, defaultKey.ToString());

            if (!Enum.TryParse(stored, out boundKey))
            {
                boundKey = defaultKey;
                ConfigManager.Set(configKey, boundKey.ToString());
                ConfigManager.Save();
            }
        }

        public override void Draw(int x, int y)
        {
            DrawUtils.Text(Label, x, y, 0.45f, Selected ? Theme.Purple : Theme.TextBright);
            DrawUtils.Text(boundKey.ToString(), x + 300, y, 0.45f, Theme.PurpleGlow);

            if (Selected)
                DrawUtils.Rect(x - 10, y + 25, 360, 4, Theme.PurpleGlow);
        }

        public override void Activate()
        {
            InputManager.CaptureKey(k =>
            {
                boundKey = k;
                ConfigManager.Set(configKey, k.ToString());
                ConfigManager.Save();
                TrainerMain.Instance.SetOpenKey(k);
            });
        }
    }
}
