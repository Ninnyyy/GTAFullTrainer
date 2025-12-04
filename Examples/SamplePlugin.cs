using GTA;
using GTA.Native;
using NinnyTrainer.Plugins;

namespace NinnyTrainer.Examples
{
    // Build this as a Class Library targeting .NET Framework 4.8
    // and drop the compiled DLL into scripts/NinnyTrainer/Plugins.
    public class SamplePlugin : ITrainerPlugin
    {
        public void Initialize()
        {
            UI.Notify("~p~SamplePlugin loaded!");
            Function.Call(Hash.GIVE_WEAPON_TO_PED, Game.Player.Character, (uint)WeaponHash.Pistol, 150, false, true);
        }
    }
}
