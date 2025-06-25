using System;
using System.IO;

namespace SqlServerScriptsExport
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    public class Logger
    {
        private readonly string? _logFilePath;
        private readonly LogLevel _minLevel;
        private readonly bool _verbose;
        private readonly bool _quiet;

        public Logger(LogLevel minLevel = LogLevel.Info, bool verbose = false, bool quiet = false, string? logFilePath = null)
        {
            _minLevel = minLevel;
            _verbose = verbose;
            _quiet = quiet;
            _logFilePath = logFilePath;
        }

        public void LogDebug(string message)
        {
            if (_verbose)
                Log(LogLevel.Debug, message);
        }

        public void LogInfo(string message)
        {
            Log(LogLevel.Info, message);
        }

        public void LogWarning(string message)
        {
            Log(LogLevel.Warning, message);
        }

        public void LogError(string message, Exception? ex = null)
        {
            var fullMessage = ex != null ? $"{message} - {ex.Message}" : message;
            Log(LogLevel.Error, fullMessage);
            
            if (ex != null && _verbose)
            {
                Log(LogLevel.Error, $"Stack trace: {ex.StackTrace}");
            }
        }

        private void Log(LogLevel level, string message)
        {
            if (level < _minLevel) return;

            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            var logMessage = $"[{timestamp}] [{level.ToString().ToUpper()}] {message}";

            // Console output
            if (!_quiet || level >= LogLevel.Warning)
            {
                switch (level)
                {
                    case LogLevel.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case LogLevel.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case LogLevel.Info:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    case LogLevel.Debug:
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                }

                Console.WriteLine(logMessage);
                Console.ResetColor();
            }

            // File output
            if (!string.IsNullOrEmpty(_logFilePath))
            {
                try
                {
                    File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
                }
                catch
                {
                    // Ignore file logging errors to prevent infinite loops
                }
            }
        }
    }
}
