using System;
using System.Collections.Generic;
using System.IO;

namespace GTAFullTrainer.Core
{
    public static class ConfigManager
    {
        private static readonly string configPath =
            AppDomain.CurrentDomain.BaseDirectory + "TrainerConfig.ini";

        private static Dictionary<string, string> settings =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public static void Load()
        {
            settings.Clear();

            if (!File.Exists(configPath))
            {
                Save(); // Create default config
                return;
            }

            string[] lines = File.ReadAllLines(configPath);

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";") || line.StartsWith("["))
                    continue;

                string[] data = line.Split('=');
                if (data.Length == 2)
                {
                    string key = data[0].Trim();
                    string value = data[1].Trim();
                    settings[key] = value;
                }
            }
        }

        public static string Get(string key, string defaultValue = "")
        {
            if (settings.ContainsKey(key))
                return settings[key];

            return defaultValue;
        }

        public static void Set(string key, string value)
        {
            settings[key] = value;
        }

        public static void Save()
        {
            List<string> lines = new List<string>
            {
"[Trainer]"
};

foreach (var pair in settings)
{
lines.Add($"{pair.Key}={pair.Value}");
}

File.WriteAllLines(configPath, lines.ToArray());
}