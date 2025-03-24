using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Si.CoreHub.Utility
{
    public class LinuxRunCommand
    {
        public string FileName { get; set; }
        public string Arguments { get; set; }
        public string WorkingDirectory { get; set; }
        public LinuxRunCommand(string fileName, string arguments, string workingDirectory = "")
        {
            FileName = fileName;
            Arguments = arguments;
            WorkingDirectory = workingDirectory;
        }


        public string Execute(int milliseconds = 10000)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var psi = new ProcessStartInfo(FileName, Arguments) { RedirectStandardOutput = true };
                var proc = Process.Start(psi);
                if (proc != null)
                {
                    using (var sr = proc.StandardOutput)
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            return string.Empty;
        }
    }
}