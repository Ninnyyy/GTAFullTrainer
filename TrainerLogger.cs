using System;
using System.IO;
using System.Text;

namespace GTAFullTrainer.Core
{
    public static class TrainerLogger
    {
        private static readonly object sync = new object();
        private static string logPath;
        private static bool initialized;

        public static void Initialize()
        {
            if (initialized)
                return;

            logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NinnyTrainer.log");
            string directory = Path.GetDirectoryName(logPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            Write("Trainer logger initialized.");
            initialized = true;
        }

        public static void Info(string message)
        {
            Write(message);
        }

        private static void Write(string message)
        {
            try
            {
                string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
                lock (sync)
                {
                    File.AppendAllText(logPath, line, Encoding.UTF8);
                }
            }
            catch
            {
                // Logging should never crash the trainer; swallow errors silently.
            }
        }
    }
}
