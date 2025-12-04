using System;
using System.IO;
using System.Text;

namespace GTAFullTrainer.Core
{
    public static class TrainerLogger
    {
        private static readonly object sync = new object();
        private static string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NinnyTrainer.log");
        private static bool initialized;

        public static void Initialize()
        {
            EnsureInitialized();
        }

        public static void Info(string message)
        {
            EnsureInitialized();
            Write(message);
        }

        private static void EnsureInitialized()
        {
            if (initialized)
                return;

            lock (sync)
            {
                if (initialized)
                    return;

                string directory = Path.GetDirectoryName(logPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                Write("Trainer logger initialized.");
                initialized = true;
            }
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
