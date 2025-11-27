using System;
using System.IO;
using System.Reflection;
using GTA;
using GTAFullTrainer.API;

namespace GTAFullTrainer.Plugins
{
    public static class PluginLoader
    {
        public static void LoadPlugins()
        {
            string folder = "Plugins";

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            foreach (string file in Directory.GetFiles(folder, "*.dll"))
            {
                try
                {
                    Assembly asm = Assembly.LoadFrom(file);

                    foreach (Type t in asm.GetTypes())
                    {
                        if (t.GetInterface("ITrainerPlugin") != null)
                        {
                            ITrainerPlugin plugin = (ITrainerPlugin)Activator.CreateInstance(t);
                            plugin.Initialize();
                        }
                    }
                }
                catch (Exception ex)
                {
                    UI.SoundManager.PlaySelect();
                    GTA.UI.Notify("Plugin Error: " + ex.Message);
                }
            }
        }
    }
}
