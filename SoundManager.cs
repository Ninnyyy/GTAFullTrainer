using GTA;
using GTA.Native;

namespace NinnyTrainer.UI
{
    public static class SoundManager
    {
        public static void PlayNavigate()
        {
            if (ThemeManager.SoundEnabled)
                Function.Call(Hash.PLAY_SOUND_FRONTEND, -1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", true);
        }

        public static void PlaySelect()
        {
            if (ThemeManager.SoundEnabled)
                Function.Call(Hash.PLAY_SOUND_FRONTEND, -1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", true);
        }

        public static void PlayOpen()
        {
            if (ThemeManager.SoundEnabled)
                Function.Call(Hash.PLAY_SOUND_FRONTEND, -1, "FocusIn", "HintCamSounds", true);
        }

        public static void PlayClose()
        {
            if (ThemeManager.SoundEnabled)
                Function.Call(Hash.PLAY_SOUND_FRONTEND, -1, "FocusOut", "HintCamSounds", true);
        }
    }
}
