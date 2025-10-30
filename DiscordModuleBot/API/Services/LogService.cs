using Discord;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace DiscordModuleBot.API.Services
{
    public static class LogService
    {
        public static bool EnableDebug { get; set; } = true;
        private static readonly object FileLock = new();
        private const long MaxFileSize = 10 * 1024 * 1024;

        private static string LogDirectory =>
            Path.Combine(AppContext.BaseDirectory, "logs");

        private static string GetLogPath(string module = "main") =>
            Path.Combine(LogDirectory, $"{module}.log");

        public static void PrintBanner()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var framework = RuntimeInformation.FrameworkDescription;
            var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            string[] banner = new[]
            {
                @"//    888b. w                         8    8b   d8          8       8       ",
                @"//    8   8 w d88b .d8b .d8b. 8d8b .d88    8YbmdP8 .d8b. .d88 8   8 8 .d88b ",
                @"//    8   8 8 `Yb. 8    8' .8 8P   8  8    8  '  8 8' .8 8  8 8b d8 8 8.dP' ",
                @"//    888P' 8 Y88P `Y8P `Y8P' 8    `Y88    8     8 `Y8P' `Y88 `Y8P8 8 `Y88P ",
                @"//                                                                          ",
                @"+------------------------------------------------------+",
                @"|                                                      |",
                $"|     Version : v{version,-38}|",
                $"|     Dotnet  : {framework,-39}|",
                $"|     Time    : {time,-39}|",
                "|                                                      |",
                "+------------------------------------------------------+"
            };

            foreach (var line in banner)
                Info(nameof(PrintBanner), line);
        }


        public static Task Info(string module, object message) =>
            WriteAsync(module, LogSeverity.Info, message);

        public static Task Debug(string module, object message)
        {
            if (!EnableDebug) return Task.CompletedTask;
            return WriteAsync(module, LogSeverity.Debug, message);
        }

        public static Task Error(string module, object message) =>
            WriteAsync(module, LogSeverity.Error, message);

        public static Task Warning(string module, object message) =>
            WriteAsync(module, LogSeverity.Warning, message);

        private static async Task WriteAsync(string module, LogSeverity severity, object message)
        {
            string logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{severity}] {message}";

            Console.WriteLine(logLine);

            string path = GetLogPath(module);

            try
            {
                lock (FileLock)
                {
                    if (!Directory.Exists(LogDirectory))
                        Directory.CreateDirectory(LogDirectory);

                    File.AppendAllText(path, logLine + Environment.NewLine, Encoding.UTF8);
                    RotateIfNeeded(path);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LogService] Failed to write log: {ex}");
            }
        }

        private static void RotateIfNeeded(string path)
        {
            FileInfo file = new(path);

            if (file.Exists && file.Length > MaxFileSize)
            {
                string archive = path + "." + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".bak";
                File.Move(path, archive, true);
            }
        }
    }
}
