using GTA;

namespace NinnyTrainer.Effects
{
    public static class Animation
    {
        public static float Smooth(float current, float target, float smoothing)
        {
            return current + (target - current) * smoothing;
        }

        public static float Pulse(float speed = 2.0f)
        {
            return (float)((System.Math.Sin(Game.GameTime / 200.0 * speed) + 1f) / 2f);
        }
    }
}
