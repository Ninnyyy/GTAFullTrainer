using System;
using System.IO;
using System.Reflection;
using GTA;

namespace NinnyTrainer.Plugins
{
    public static class PluginLoader
    {
        public static void LoadPlugins()
        {
            string folder = "scripts/NinnyTrainer/Plugins";

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            foreach (string dll in Directory.GetFiles(folder, "*.dll"))
            {
                try
                {
                    Assembly asm = Assembly.LoadFrom(dll);

                    foreach (var type in asm.GetTypes())
                    {
                        if (typeof(ITrainerPlugin).IsAssignableFrom(type) && !type.IsInterface)
                        {
                            ITrainerPlugin plugin = (ITrainerPlugin)Activator.CreateInstance(type);
                            plugin.Initialize();
                        }
                    }
                }
                catch (Exception ex)
                {
                    GTA.UI.Notify($"~r~Plugin Error:~s~ {ex.Message}");
                }
            }
        }
    }
}
