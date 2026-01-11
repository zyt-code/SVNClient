using System;
using System.IO;

namespace Svns.Utils;

/// <summary>
/// Helper methods for SVN-specific path operations
/// </summary>
public static class SvnPathHelper
{
    /// <summary>
    /// Checks if a path is the SVN admin directory
    /// </summary>
    public static bool IsSvnAdminDirectory(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        var fileName = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        return string.Equals(fileName, ".svn", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if a path is hidden (starts with dot)
    /// </summary>
    public static bool IsHiddenPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        var fileName = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        return fileName.StartsWith('.');
    }

    /// <summary>
    /// Converts a repository URL to a local path representation
    /// </summary>
    public static string UrlToLocalPath(string url, string workingCopyRoot)
    {
        try
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            // Remove protocol
            var path = url;
            if (path.Contains("://"))
            {
                var parts = path.Split(new[] { "://" }, StringSplitOptions.None);
                if (parts.Length > 1)
                {
                    path = parts[1];
                }
            }

            // Convert URL separators to path separators
            path = path.Replace('/', Path.DirectorySeparatorChar);

            // Remove leading separator
            path = path.TrimStart(Path.DirectorySeparatorChar);

            // Combine with working copy root
            return PathHelper.CombinePath(workingCopyRoot, path);
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Converts a local path to a repository URL representation
    /// </summary>
    public static string LocalPathToUrl(string localPath, string repositoryRoot, string workingCopyRoot)
    {
        try
        {
            if (string.IsNullOrEmpty(localPath) || string.IsNullOrEmpty(repositoryRoot))
                return string.Empty;

            // Get relative path from working copy root
            var relativePath = PathHelper.GetRelativePath(workingCopyRoot, localPath);

            // Convert to URL format
            relativePath = relativePath.Replace(Path.DirectorySeparatorChar, '/');

            // Combine with repository root
            return repositoryRoot.TrimEnd('/') + "/" + relativePath.TrimStart('/');
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Gets the working copy root URL from a file's repository URL
    /// </summary>
    public static string GetWorkingCopyRootUrl(string fileUrl, string repositoryRoot)
    {
        try
        {
            if (string.IsNullOrEmpty(fileUrl) || string.IsNullOrEmpty(repositoryRoot))
                return string.Empty;

            // Ensure URLs end with /
            fileUrl = fileUrl.TrimEnd('/') + '/';
            repositoryRoot = repositoryRoot.TrimEnd('/') + '/';

            // The file URL should start with repository root
            if (!fileUrl.StartsWith(repositoryRoot, StringComparison.OrdinalIgnoreCase))
                return fileUrl;

            // Extract the path after repository root
            var relativePath = fileUrl.Substring(repositoryRoot.Length);
            var segments = relativePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length > 0)
            {
                // Return repository root + first segment (branch/tag/trunk)
                return repositoryRoot.TrimEnd('/') + '/' + segments[0];
            }

            return repositoryRoot.TrimEnd('/');
        }
        catch
        {
            return fileUrl;
        }
    }

    /// <summary>
    /// Checks if a path is within a working copy
    /// </summary>
    public static bool IsInWorkingCopy(string path, string workingCopyRoot)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(workingCopyRoot))
            return false;

        try
        {
            var normalizedPath = PathHelper.NormalizePath(path);
            var normalizedRoot = PathHelper.NormalizePath(workingCopyRoot);

            return normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase) ||
                   PathHelper.IsChildOf(normalizedRoot, normalizedPath);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the SVN conflict files for a given file
    /// </summary>
    public static (string? mine, string? theirs, string? @base) GetConflictFiles(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return (null, null, null);

        var directory = Path.GetDirectoryName(filePath) ?? string.Empty;
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
        var extension = Path.GetExtension(filePath);

        var mineFile = Path.Combine(directory, $"{fileNameWithoutExt}.mine{extension}");
        var theirsFile = Path.Combine(directory, $"{fileNameWithoutExt}.r{extension}"); // .rOLD
        var baseFile = Path.Combine(directory, $"{fileNameWithoutExt}.mine{extension}"); // This is simplified

        return (mineFile, theirsFile, baseFile);
    }

    /// <summary>
    /// Checks if a file is an SVN conflict file
    /// </summary>
    public static bool IsConflictFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return false;

        var fileName = Path.GetFileName(filePath);

        // Check for various conflict file patterns
        return fileName.EndsWith(".mine") ||
               fileName.EndsWith(".r") && fileName.Length > 2 && char.IsDigit(fileName[^2]) ||
               fileName.Contains(".merge-left.") ||
               fileName.Contains(".merge-right.");
    }

    /// <summary>
    /// Gets the patch file path for a working copy
    /// </summary>
    public static string GetPatchFilePath(string workingCopyRoot, string patchName)
    {
        try
        {
            var sanitizedPatchName = PathHelper.SanitizeFileName(patchName);
            return Path.Combine(workingCopyRoot, sanitizedPatchName);
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Converts an SVN path to a valid file system path
    /// </summary>
    public static string SvnPathToFileSystemPath(string svnPath, string workingCopyRoot)
    {
        try
        {
            if (string.IsNullOrEmpty(svnPath))
                return string.Empty;

            // Remove leading slash if present
            var path = svnPath.TrimStart('/', '\\');

            // Replace URL separators with path separators
            path = path.Replace('/', Path.DirectorySeparatorChar);

            return Path.Combine(workingCopyRoot, path);
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Gets the relative path from working copy root for display
    /// </summary>
    public static string GetDisplayRelativePath(string fullPath, string workingCopyRoot)
    {
        try
        {
            var relativePath = PathHelper.GetRelativePath(workingCopyRoot, fullPath);

            // Use forward slashes for display (more like SVN)
            return relativePath.Replace(Path.DirectorySeparatorChar, '/');
        }
        catch
        {
            return fullPath;
        }
    }

    /// <summary>
    /// Normalizes a URL by removing trailing slashes
    /// </summary>
    public static string NormalizeUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return url;

        return url.TrimEnd('/');
    }

    /// <summary>
    /// Checks if two URLs are the same (case-insensitive, ignoring trailing slashes)
    /// </summary>
    public static bool AreUrlsEqual(string url1, string url2)
    {
        if (string.IsNullOrEmpty(url1) && string.IsNullOrEmpty(url2))
            return true;

        if (string.IsNullOrEmpty(url1) || string.IsNullOrEmpty(url2))
            return false;

        return string.Equals(
            NormalizeUrl(url1),
            NormalizeUrl(url2),
            StringComparison.OrdinalIgnoreCase
        );
    }
}
