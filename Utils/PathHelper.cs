using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Svns.Utils;

/// <summary>
/// Helper methods for path operations
/// </summary>
public static class PathHelper
{
    /// <summary>
    /// Normalizes a path for display and comparison
    /// </summary>
    public static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        try
        {
            return Path.GetFullPath(path)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        catch
        {
            return path;
        }
    }

    /// <summary>
    /// Gets the relative path from a base path
    /// </summary>
    public static string GetRelativePath(string basePath, string fullPath)
    {
        try
        {
            return Path.GetRelativePath(basePath, fullPath) ?? fullPath;
        }
        catch
        {
            return fullPath;
        }
    }

    /// <summary>
    /// Combines two paths safely
    /// </summary>
    public static string CombinePath(string path1, string path2)
    {
        try
        {
            return Path.Combine(path1, path2);
        }
        catch
        {
            return path2;
        }
    }

    /// <summary>
    /// Checks if a path is a child of another path
    /// </summary>
    public static bool IsChildOf(string childPath, string parentPath)
    {
        try
        {
            var relative = GetRelativePath(parentPath, childPath);
            return !relative.StartsWith("..") && !Path.IsPathRooted(relative);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the common path between two paths
    /// </summary>
    public static string GetCommonPath(string path1, string path2)
    {
        try
        {
            path1 = NormalizePath(path1);
            path2 = NormalizePath(path2);

            var parts1 = path1.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var parts2 = path2.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            var commonParts = new List<string>();
            var minLength = Math.Min(parts1.Length, parts2.Length);

            for (int i = 0; i < minLength; i++)
            {
                if (string.Equals(parts1[i], parts2[i], StringComparison.OrdinalIgnoreCase))
                {
                    commonParts.Add(parts1[i]);
                }
                else
                {
                    break;
                }
            }

            return commonParts.Count > 0
                ? Path.Combine(commonParts.ToArray())
                : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Shortens a path for display by replacing parent directory with ellipsis
    /// </summary>
    public static string ShortenPath(string path, int maxLength = 50)
    {
        if (string.IsNullOrEmpty(path) || path.Length <= maxLength)
            return path;

        try
        {
            var fileName = Path.GetFileName(path);
            var directory = Path.GetDirectoryName(path);

            if (!string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(directory))
            {
                var availableLength = maxLength - fileName.Length - 4; // 4 for "...\"
                if (availableLength > 0)
                {
                    return directory.Substring(0, Math.Min(availableLength, directory.Length)) +
                           "...\\" + fileName;
                }
            }

            // If we can't shorten nicely, just truncate
            return path.Substring(0, maxLength - 3) + "...";
        }
        catch
        {
            return path;
        }
    }

    /// <summary>
    /// Gets a display-friendly path (with forward slashes for consistency)
    /// </summary>
    public static string GetDisplayPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        return path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
    }

    /// <summary>
    /// Validates if a path is valid
    /// </summary>
    public static bool IsValidPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        try
        {
            Path.GetFullPath(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Sanitizes a filename by removing invalid characters
    /// </summary>
    public static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return fileName;

        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = fileName;

        foreach (var c in invalidChars)
        {
            sanitized = sanitized.Replace(c, '_');
        }

        return sanitized;
    }

    /// <summary>
    /// Ensures a trailing separator in the path
    /// </summary>
    public static string EnsureTrailingSeparator(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()) &&
            !path.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
        {
            return path + Path.DirectorySeparatorChar;
        }

        return path;
    }

    /// <summary>
    /// Removes a trailing separator from the path
    /// </summary>
    public static string RemoveTrailingSeparator(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        return path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    /// <summary>
    /// Gets the file name without extension
    /// </summary>
    public static string GetFileNameWithoutExtension(string path)
    {
        try
        {
            return Path.GetFileNameWithoutExtension(path);
        }
        catch
        {
            return path;
        }
    }

    /// <summary>
    /// Changes the file extension
    /// </summary>
    public static string ChangeExtension(string path, string newExtension)
    {
        try
        {
            return Path.ChangeExtension(path, newExtension);
        }
        catch
        {
            return path;
        }
    }
}
