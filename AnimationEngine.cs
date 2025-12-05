using System;

namespace NinnyTrainer.Effects
{
    public static class AnimationEngine
    {
        public static float Damp(float current, float target, float smoothing)
        {
            smoothing = Math.Clamp(smoothing, 0.01f, 1.0f);
            return current + (target - current) * smoothing;
        }

        public static float EaseOut(float progress)
        {
            progress = Math.Clamp(progress, 0f, 1f);
            return 1f - (float)Math.Pow(1f - progress, 3);
        }

        public static float Bounce(float progress)
        {
            progress = Math.Clamp(progress, 0f, 1f);
            return (float)Math.Abs(Math.Sin(progress * Math.PI * 1.5f));
        }
    }
}
