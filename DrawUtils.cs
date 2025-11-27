using System.Drawing;
using GTA;
using GTA.Native;

namespace NinnyTrainer.Utils
{
    public static class DrawUtils
    {
        public static void Rect(Rectangle rect, Color color)
        {
            Function.Call(Hash.DRAW_RECT,
                (float)rect.X / UI.WIDTH,
                (float)rect.Y / UI.HEIGHT,
                (float)rect.Width / UI.WIDTH,
                (float)rect.Height / UI.HEIGHT,
                color.R, color.G, color.B, color.A);
        }

        public static void RectFullScreen(Color color)
        {
            Rect(new Rectangle(0, 0, UI.WIDTH, UI.HEIGHT), color);
        }

        public static void Text(string text, float x, float y, float scale, Color color)
        {
            Function.Call(Hash.SET_TEXT_FONT, 0);
            Function.Call(Hash.SET_TEXT_SCALE, 0.0f, scale);
            Function.Call(Hash.SET_TEXT_COLOUR, color.R, color.G, color.B, color.A);
            Function.Call(Hash.SET_TEXT_OUTLINE);
            Function.Call(Hash.SET_TEXT_DROPSHADOW);
            Function.Call(Hash.SET_TEXT_CENTRE, false);
            Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_TEXT, "STRING");
            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, text);
            Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_TEXT,
                x / UI.WIDTH, y / UI.HEIGHT);
        }
    }

    public static class UI
    {
        public const float WIDTH = 1920f;
        public const float HEIGHT = 1080f;
    }
}
