using System;

namespace GTAFullTrainer.Effects
{
    public static class Animation
    {
        // EASING
        public static float EaseInOut(float t)
        {
            return t * t * (3 - 2 * t);
        }

        public static float Smooth(float current, float target, float smoothing)
        {
            return current + (target - current) * smoothing;
        }

        // PULSE effect
        public static float Pulse(float speed = 2.0f)
        {
            return (float)((Math.Sin(Game.GameTime / 200.0 * speed) + 1f) / 2f);
        }
    }
}
