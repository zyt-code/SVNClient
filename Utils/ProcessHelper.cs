using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace Svns.Utils;

/// <summary>
/// Helper methods for process operations
/// </summary>
public static class ProcessHelper
{
    // Cached platform detection results for performance
    private static bool? _isWindows;
    private static bool? _isMacOS;
    private static bool? _isLinux;

    /// <summary>
    /// Checks if a process is running
    /// </summary>
    public static bool IsProcessRunning(string processName)
    {
        try
        {
            var processes = Process.GetProcessesByName(processName);
            foreach (var process in processes)
            {
                process.Dispose();
            }
            return processes.Length > 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the full path to an executable in PATH
    /// </summary>
    public static string? FindExecutableInPath(string executableName)
    {
        try
        {
            var pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(pathEnv))
                return null;

            var paths = pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);

            foreach (var path in paths)
            {
                var fullPath = Path.Combine(path, executableName);

                if (File.Exists(fullPath))
                {
                    return fullPath;
                }

                // Try with .exe on Windows
                if (IsWindows())
                {
                    var exePath = fullPath + ".exe";
                    if (File.Exists(exePath))
                    {
                        return exePath;
                    }
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Checks if an executable is available
    /// </summary>
    public static bool IsExecutableAvailable(string executableName)
    {
        return FindExecutableInPath(executableName) != null;
    }

    /// <summary>
    /// Gets the version of an executable
    /// </summary>
    public static string? GetExecutableVersion(string executablePath)
    {
        try
        {
            if (!File.Exists(executablePath))
                return null;

            var versionInfo = FileVersionInfo.GetVersionInfo(executablePath);

            if (!string.IsNullOrEmpty(versionInfo.FileVersion))
                return versionInfo.FileVersion;

            if (!string.IsNullOrEmpty(versionInfo.ProductVersion))
                return versionInfo.ProductVersion;

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Starts a process and returns immediately
    /// </summary>
    public static bool StartProcess(
        string fileName,
        string arguments,
        string? workingDirectory = null,
        bool useShellExecute = false)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory ?? string.Empty,
                UseShellExecute = useShellExecute,
                WindowStyle = ProcessWindowStyle.Normal
            };

            Process.Start(startInfo);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Kills a process and all its children
    /// </summary>
    public static bool KillProcessTree(int processId)
    {
        try
        {
            var process = Process.GetProcessById(processId);
            process.Kill(entireProcessTree: true);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Waits for a process to complete with a timeout
    /// </summary>
    public static bool WaitForProcess(
        Process process,
        int timeoutMs,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return process.WaitForExit(timeoutMs);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets environment variables for a process
    /// </summary>
    public static IDictionary<string, string> GetProcessEnvironment(int processId)
    {
        try
        {
            var process = Process.GetProcessById(processId);
            process.Refresh();

            var env = new Dictionary<string, string>();

            // StartInfo.Environment is only available before starting
            // For running processes, we'd need to use platform-specific APIs
            // This is a simplified version

            return env;
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Gets memory usage for a process
    /// </summary>
    public static long GetProcessMemoryUsage(int processId)
    {
        try
        {
            var process = Process.GetProcessById(processId);
            process.Refresh();
            return process.WorkingSet64;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Escapes arguments for command line
    /// </summary>
    public static string EscapeCommandLineArgument(string argument)
    {
        // Escape backslashes and quotes according to MS C runtime rules
        var escaped = new StringBuilder();

        for (int i = 0; i < argument.Length; i++)
        {
            var c = argument[i];

            if (c == '\\')
            {
                // Count consecutive backslashes
                int backslashCount = 1;
                while (i + backslashCount < argument.Length && argument[i + backslashCount] == '\\')
                {
                    backslashCount++;
                }

                // Check if they're followed by a quote
                bool followedByQuote = i + backslashCount < argument.Length && argument[i + backslashCount] == '"';

                if (followedByQuote)
                {
                    // Double all backslashes
                    escaped.Append('\\', backslashCount * 2);
                    i += backslashCount - 1;
                }
                else
                {
                    escaped.Append('\\', backslashCount);
                    i += backslashCount - 1;
                }
            }
            else if (c == '"')
            {
                escaped.Append("\\\"");
            }
            else
            {
                escaped.Append(c);
            }
        }

        return escaped.ToString();
    }

    /// <summary>
    /// Formats bytes to human-readable size
    /// </summary>
    public static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    /// <summary>
    /// Gets the current user name
    /// </summary>
    public static string GetCurrentUserName()
    {
        try
        {
            return Environment.UserName;
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// Gets the machine name
    /// </summary>
    public static string GetMachineName()
    {
        try
        {
            return Environment.MachineName;
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// Checks if running on Windows
    /// </summary>
    public static bool IsWindows()
    {
        if (_isWindows.HasValue)
            return _isWindows.Value;

        _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        return _isWindows.Value;
    }

    /// <summary>
    /// Checks if running on macOS
    /// </summary>
    public static bool IsMacOS()
    {
        if (_isMacOS.HasValue)
            return _isMacOS.Value;

        _isMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        return _isMacOS.Value;
    }

    /// <summary>
    /// Checks if running on Linux
    /// </summary>
    public static bool IsLinux()
    {
        if (_isLinux.HasValue)
            return _isLinux.Value;

        _isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        return _isLinux.Value;
    }

    /// <summary>
    /// Gets a temporary file path
    /// </summary>
    public static string GetTempFilePath(string? extension = null)
    {
        var fileName = Path.GetRandomFileName();
        if (!string.IsNullOrEmpty(extension))
        {
            fileName = Path.ChangeExtension(fileName, extension);
        }
        return Path.Combine(Path.GetTempPath(), fileName);
    }

    /// <summary>
    /// Gets a temporary directory path
    /// </summary>
    public static string GetTempDirectoryPath()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        return tempDir;
    }
}
