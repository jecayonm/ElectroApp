using System;
using System.IO;

namespace ElectroApp.Utilities
{
    public static class Logger
    {
        private static readonly object _lock = new object();
        private static readonly string _logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ElectroApp", "Logs");
        private static readonly string _logFile = Path.Combine(_logDir, $"app_{DateTime.Now:yyyyMMdd}.log");

        public static void Log(Exception ex, string area = null)
        {
            try
            {
                Directory.CreateDirectory(_logDir);
                var lines = new[]
                {
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {(string.IsNullOrWhiteSpace(area) ? "" : ($"[{area}] "))}{ex.GetType().FullName}: {ex.Message}",
                    ex.StackTrace,
                    ""
                };
                lock (_lock)
                {
                    File.AppendAllLines(_logFile, lines);
                }
            }
            catch
            {
                // Ignorar errores de logging
            }
        }

        public static void Log(string message, string area = null)
        {
            try
            {
                Directory.CreateDirectory(_logDir);
                var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {(string.IsNullOrWhiteSpace(area) ? "" : ($"[{area}] "))}{message}";
                lock (_lock)
                {
                    File.AppendAllLines(_logFile, new[] { line });
                }
            }
            catch { }
        }
    }
}
