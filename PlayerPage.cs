using GTA;
using GTA.Native;
using GTA.Math;

namespace NinnyTrainer.Pages
{
    public static class PlayerPage
    {
        private static bool godMode = false;
        private static bool neverWanted = false;

        public static void ToggleGod()
        {
            godMode = !godMode;
            Function.Call(Hash.SET_PLAYER_INVINCIBLE, Game.Player, godMode);
        }

        public static void ToggleNeverWanted()
        {
            neverWanted = !neverWanted;
            Function.Call(Hash.SET_MAX_WANTED_LEVEL, neverWanted ? 0 : 5);
            Function.Call(Hash.CLEAR_PLAYER_WANTED_LEVEL, Game.Player);
        }

        public static void Heal()
        {
            Ped p = Game.Player.Character;
            p.Health = p.MaxHealth;
            p.Armor = 100;
        }

        public static void SetRunSpeed(float v)
        {
            Function.Call(Hash.SET_RUN_SPRINT_MULTIPLIER_FOR_PLAYER, Game.Player, v);
        }

        public static void SetSwimSpeed(float v)
        {
            Function.Call(Hash.SET_SWIM_MULTIPLIER_FOR_PLAYER, Game.Player, v);
        }

        public static void SetWalkStyle(int index)
        {
            string[] styles = { "move_m@confident", "move_m@gangster", "move_m@swagger", "move_m@injured" };

            if (index >= styles.Length)
                return;

            Function.Call(Hash.REQUEST_ANIM_SET, styles[index]);
            Script.Wait(300);
            Function.Call(Hash.SET_PED_MOVEMENT_CLIPSET, Game.Player.Character, styles[index], 1.0f);
        }
    }
}
