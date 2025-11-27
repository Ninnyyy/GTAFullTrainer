using GTA;

namespace GTAFullTrainer.UI
{
    public static class SoundManager
    {
        public static void PlayNavigate()
        {
            Function.Call(Hash.PLAY_SOUND_FRONTEND, -1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", true);
        }

        public static void PlaySelect()
        {
            Function.Call(Hash.PLAY_SOUND_FRONTEND, -1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", true);
        }

        public static void PlayOpen()
        {
            Function.Call(Hash.PLAY_SOUND_FRONTEND, -1, "FocusIn", "HintCamSounds", true);
        }

        public static void PlayClose()
        {
            Function.Call(Hash.PLAY_SOUND_FRONTEND, -1, "FocusOut", "HintCamSounds", true);
        }
    }
}
