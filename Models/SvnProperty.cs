using System;

namespace Svns.Models;

/// <summary>
/// Represents an SVN property on a file or directory
/// </summary>
public class SvnProperty
{
    /// <summary>
    /// The property name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The property value
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// The path this property is set on
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is a regular property (not svn:* prefix)
    /// </summary>
    public bool IsRegularProperty => !Name.StartsWith("svn:", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Whether this is an SVN built-in property
    /// </summary>
    public bool IsSvnProperty => Name.StartsWith("svn:", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the display name
    /// </summary>
    public string DisplayName => Name;
}

/// <summary>
/// Common SVN property names
/// </summary>
public static class SvnProperties
{
    /// <summary>
    /// File mime type
    /// </summary>
    public const string MimeType = "svn:mime-type";

    /// <summary>
    /// EOL style (native, CRLF, LF, CR)
    /// </summary>
    public const string EolStyle = "svn:eol-style";

    /// <summary>
    /// Keywords to expand
    /// </summary>
    public const string Keywords = "svn:keywords";

    /// <summary>
    /// Whether the file is executable
    /// </summary>
    public const string Executable = "svn:executable";

    /// <summary>
    /// Whether the item needs lock
    /// </summary>
    public const string NeedsLock = "svn:needs-lock";

    /// <summary>
    /// File externals definition
    /// </summary>
    public const string Externals = "svn:externals";

    /// <summary>
    /// Ignore patterns
    /// </summary>
    public const string Ignore = "svn:ignore";

    /// <summary>
    /// Auto-props file
    /// </summary>
    public const string AutoProps = "svn:auto-props";

    /// <summary>
    /// Merge info
    /// </summary>
    public const string MergeInfo = "svn:mergeinfo";

    /// <summary>
    /// Special file (empty value)
    /// </summary>
    public const string Special = "svn:special";

    /// <summary>
    /// Depth
    /// </summary>
    public const string Depth = "svn:depth";

    /// <summary>
    /// Mergeinfo (for directories)
    /// </summary>
    public const string MergeInfoInherited = "svn:mergeinfo-inherited";

    /// <summary>
    /// All built-in SVN properties
    /// </summary>
    public static readonly string[] BuiltInProperties =
    {
        MimeType,
        EolStyle,
        Keywords,
        Executable,
        NeedsLock,
        Externals,
        Ignore,
        AutoProps,
        MergeInfo,
        Special,
        Depth
    };

    /// <summary>
    /// Gets a friendly display name for a property
    /// </summary>
    public static string GetDisplayName(string propertyName)
    {
        return propertyName switch
        {
            MimeType => "MIME Type",
            EolStyle => "EOL Style",
            Keywords => "Keywords",
            Executable => "Executable",
            NeedsLock => "Needs Lock",
            Externals => "Externals",
            Ignore => "Ignore",
            AutoProps => "Auto Props",
            MergeInfo => "Merge Info",
            Special => "Special",
            Depth => "Depth",
            _ => propertyName
        };
    }

    /// <summary>
    /// Gets a description for a property
    /// </summary>
    public static string GetDescription(string propertyName)
    {
        return propertyName switch
        {
            MimeType => "The MIME type of the file (e.g., 'text/plain', 'image/png')",
            EolStyle => "End-of-line style (native, CRLF, LF, CR)",
            Keywords => "Keywords to expand (e.g., 'Id, Revision, Author, Date')",
            Executable => "Mark file as executable on Unix systems",
            NeedsLock => "File must be locked before editing",
            Externals => "Definitions for external items",
            Ignore => "Patterns for files to ignore",
            AutoProps => "Automatic property settings",
            MergeInfo => "Merge history information",
            Special => "Mark as special file (symlink)",
            Depth => "Working copy depth",
            _ => string.Empty
        };
    }
}
