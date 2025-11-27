using GTA;
using GTA.Native;

namespace NinnyTrainer.Pages
{
    public static class SuperpowerPage
    {
        private static bool flash = false;
        private static bool superJump = false;
        private static bool timeSlow = false;
        private static float strength = 1f;

        public static void ToggleFlash(bool s) => flash = s;
        public static void ToggleSuperJump(bool s) => superJump = s;
        public static void ToggleTimeSlow(bool s) => timeSlow = s;
        public static void SetStrength(float f) => strength = f;

        public static void OnTick()
        {
            Ped p = Game.Player.Character;

            if (flash)
                Function.Call(Hash.SET_RUN_SPRINT_MULTIPLIER_FOR_PLAYER, Game.Player, 2.5f);

            if (superJump)
                Function.Call(Hash.SET_SUPER_JUMP_THIS_FRAME, Game.Player);

            if (timeSlow)
                Function.Call(Hash.SET_TIME_SCALE, 0.4f);
            else
                Function.Call(Hash.SET_TIME_SCALE, 1f);
        }
    }
}
