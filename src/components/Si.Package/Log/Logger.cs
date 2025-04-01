using System.Diagnostics;

namespace Si.Package.Log
{
    internal class Logger
    {
        private static ReaderWriterLockSlim LogWriteLock = new ReaderWriterLockSlim();
        private static readonly string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Logs");
        private static string logFileNameBase = "XLog";
        private static string logFileExtension = ".txt";
        private static string currentLogFile = GetLatestLogFile();
        private const long MaxFileSize = 50 * 1024 * 1024; // 30MB
        private const int MaxLogFiles = 30;

        public static void Info(string message) => WriteLog("INFO", message);
        public static void Error(string message) => WriteLog("ERROR", message);
        public static void Fatal(string message) => WriteLog("FATAL", message);
        public static void Warning(string message) => WriteLog("WARNING", message);

        private static void WriteLog(string level, string message)
        {
            try
            {
                LogWriteLock.EnterWriteLock();
                DateTime now = DateTime.Now;
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                    if (IsLinux()) SetFilePermissions(logDirectory, "777");
                }

                if (new FileInfo(currentLogFile).Length > MaxFileSize)
                {
                    currentLogFile = GetNextLogFile();
                    CleanupOldLogs();
                }
                using (StreamWriter writer = File.AppendText(currentLogFile))
                {
                    writer.WriteLine($"{now:yyyy-MM-dd HH:mm:ss} >> {level} >> {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing log: {ex}");
            }
            finally
            {
                LogWriteLock.ExitWriteLock();
            }
        }

        private static string GetLatestLogFile()
        {
            if (!Directory.Exists(logDirectory)) return Path.Combine(logDirectory, logFileNameBase + logFileExtension);
            var logFiles = Directory.GetFiles(logDirectory, logFileNameBase + "_*.txt")
                                   .Select(f => new FileInfo(f))
                                   .OrderByDescending(f => f.Name)
                                   .ToList();
            return logFiles.Any() ? logFiles.First().FullName : Path.Combine(logDirectory, logFileNameBase + "_1" + logFileExtension);
        }

        private static string GetNextLogFile()
        {
            int newIndex = 1;
            var existingLogs = Directory.GetFiles(logDirectory, logFileNameBase + "_*.txt");
            if (existingLogs.Length > 0)
            {
                var lastFile = existingLogs.Select(f => new FileInfo(f))
                                           .OrderByDescending(f => f.Name)
                                           .First();
                string lastFileName = Path.GetFileNameWithoutExtension(lastFile.Name);
                if (int.TryParse(lastFileName.Split('_').Last(), out int lastIndex))
                {
                    newIndex = lastIndex + 1;
                }
            }
            return Path.Combine(logDirectory, $"{logFileNameBase}_{newIndex}{logFileExtension}");
        }

        private static void CleanupOldLogs()
        {
            var logFiles = Directory.GetFiles(logDirectory, logFileNameBase + "_*.txt")
                                     .Select(f => new FileInfo(f))
                                     .OrderBy(f => f.CreationTime)
                                     .ToList();
            while (logFiles.Count > MaxLogFiles)
            {
                try
                {
                    logFiles.First().Delete();
                    logFiles.RemoveAt(0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to delete log file: {ex}");
                }
            }
        }

        private static void SetFilePermissions(string filePath, string permissions)
        {
            if (!IsLinux()) return;
            try
            {
                Process chmod = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "chmod",
                        Arguments = $"{permissions} {filePath}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                chmod.Start();
                chmod.WaitForExit();
                if (chmod.ExitCode != 0)
                {
                    Console.WriteLine($"Failed to set permissions: {chmod.StandardError.ReadToEnd()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting file permissions: {ex}");
            }
        }

        private static bool IsLinux() => Environment.OSVersion.Platform == PlatformID.Unix ||
                                          Environment.OSVersion.Platform == PlatformID.MacOSX;
    }
}
