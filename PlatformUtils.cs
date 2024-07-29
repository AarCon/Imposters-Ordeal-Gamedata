using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace ImpostersOrdeal
{
    internal class PlatformUtils
    {
        public static string[] GetPathList()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                var pathString = Environment.GetEnvironmentVariable("PATH");
                return pathString?.Split(';');
            }
            else if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                var pathString = Environment.GetEnvironmentVariable("PATH");
                return pathString?.Split(':');
            }
            else
            {
                return null;
            }
        }

        public static bool UsingMsys()
        {
            var msystemString = Environment.GetEnvironmentVariable("MSYSTEM");
            return msystemString == "MINGW64" || msystemString == "MINGW32";
        }

        public static string[] FindPythonExePaths(string[] pathList)
        {
            HashSet<string> pythonNames;
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                pythonNames = new() { "python.exe", "python3.exe", "py.exe", "py3.exe" };
            }
            else if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                pythonNames = new() { "python3", "python" };
            }
            else
            {
                return null;
            }

            var pythonExePaths = new List<string>();
            foreach (var path in pathList)
            {
                foreach (var name in pythonNames)
                {
                    var possiblePythonExePath = Path.Combine(path, name);
                    if (File.Exists(possiblePythonExePath))
                    {
                        pythonExePaths.Add(possiblePythonExePath);
                    }
                }
            }

            return pythonExePaths.ToArray();
        }

        public static (int, int) GetPythonVersion()
        {
            var commandLine = "import sys; print(sys.version_info.major, sys.version_info.minor, 'kwgood')";
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"-c \"{commandLine}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            var process = Process.Start(processStartInfo);
            if (process == null || !process.WaitForExit(5000))
            {
                return (0, 0);
            }

            var result = process.StandardOutput.ReadToEnd().Trim();
            var splits = result.Split(' ');
            if (splits.Length != 3 || splits[2] != "kwgood")
            {
                return (0, 0);
            }

            if (!int.TryParse(splits[0], out var majorVer) || !int.TryParse(splits[1], out var minorVer))
            {
                return (0, 0);
            }

            return (majorVer, minorVer);
        }

        public static string FindPythonLibPath(string[] pathList)
        {
            var exePaths = FindPythonExePaths(pathList);
            if (exePaths.Length == 0)
            {
                return null;
            }

            foreach (var path in exePaths)
            {
                var (majorVer, minorVer) = GetPythonVersion();
                if (majorVer == 0 && minorVer == 0)
                {
                    continue;
                }

                var dir = Path.GetDirectoryName(path);
                if (dir == null)
                {
                    continue;
                }

                string pythonLibName;
                if (Environment.OSVersion.Platform == PlatformID.Win32NT && !UsingMsys())
                {
                    pythonLibName = Path.Combine(dir, $"python{majorVer}{minorVer}.dll");
                }
                else if (Environment.OSVersion.Platform == PlatformID.Unix || UsingMsys())
                {
                    pythonLibName = Path.Combine(dir, $"libpython{majorVer}.{minorVer}.so");
                }
                else
                {
                    // shouldn't happen
                    return null;
                }

                foreach (var searchPath in pathList)
                {
                    var possiblePythonExePath = Path.Combine(searchPath, pythonLibName);
                    if (File.Exists(possiblePythonExePath))
                    {
                        return possiblePythonExePath;
                    }
                }
            }

            return null;
        }
    }
}