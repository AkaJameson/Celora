using System.Diagnostics;

namespace Si.CoreHub.Utility
{
    // <summary>
    /// Wmic工具 可方便调用查询winodws系统资源信息，相关命令请百度
    /// </summary>
    /// <param name="query"></param>
    /// <param name="redirectStandardOutput"></param>
    /// <returns></returns>
    public static class WmicUtils
    {
        public static string GetWmicOutput(string query, bool redirectStandardOutput = true)
        {
            var info = new ProcessStartInfo("wmic")
            {
                Arguments = query,
                RedirectStandardOutput = redirectStandardOutput,
                Verb = "runas"
            };
            var output = "";
            using var process = Process.Start(info);
            output = process?.StandardOutput.ReadToEnd();
            return output?.Trim();
        }
    }
}
