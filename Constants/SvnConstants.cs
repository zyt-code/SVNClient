namespace Svns.Constants;

/// <summary>
/// Constants related to SVN operations
/// </summary>
public static class SvnConstants
{
    /// <summary>
    /// The name of the SVN administrative directory
    /// </summary>
    public const string SvnDirectoryName = ".svn";

    /// <summary>
    /// The default SVN executable name (platform-agnostic, without extension)
    /// </summary>
    public const string SvnExecutableName = "svn";

    /// <summary>
    /// The default configuration directory name
    /// </summary>
    public const string ConfigDirectoryName = ".subversion";

    /// <summary>
    /// The command-line argument for XML output
    /// </summary>
    public const string XmlOutputFlag = "--xml";

    /// <summary>
    /// The command-line argument for verbose output
    /// </summary>
    public const string VerboseFlag = "--verbose";

    /// <summary>
    /// The command-line argument for quiet output
    /// </summary>
    public const string QuietFlag = "--quiet";

    /// <summary>
    /// The command-line argument for recursive operations
    /// </summary>
    public const string RecursiveFlag = "--recursive";

    /// <summary>
    /// The command-line argument for non-recursive operations
    /// </summary>
    public const string NonRecursiveFlag = "--non-recursive";

    /// <summary>
    /// The command-line argument for force operations
    /// </summary>
    public const string ForceFlag = "--force";

    /// <summary>
    /// The command-line argument for dry-run operations
    /// </summary>
    public const string DryRunFlag = "--dry-run";

    /// <summary>
    /// Maximum number of log entries to retrieve by default
    /// </summary>
    public const int DefaultLogLimit = 100;

    /// <summary>
    /// Maximum number of log entries to retrieve in a single query
    /// </summary>
    public const int MaxLogLimit = 10000;

    /// <summary>
    /// Default timeout for SVN operations (in milliseconds)
    /// </summary>
    public const int DefaultTimeout = 300000; // 5 minutes

    /// <summary>
    /// Minimum timeout for SVN operations (in milliseconds)
    /// </summary>
    public const int MinTimeout = 10000; // 10 seconds

    /// <summary>
    /// Maximum timeout for SVN operations (in milliseconds)
    /// </summary>
    public const int MaxTimeout = 3600000; // 1 hour

    /// <summary>
    /// Size of buffer for reading command output (in bytes)
    /// </summary>
    public const int OutputBufferSize = 8192;

    /// <summary>
    /// Maximum number of recent working copies to remember
    /// </summary>
    public const int MaxRecentWorkingCopies = 10;

    /// <summary>
    /// Name of the settings file
    /// </summary>
    public const string SettingsFileName = "svns-settings.json";

    /// <summary>
    /// Name of the recent working copies file
    /// </summary>
    public const string RecentWorkingCopiesFileName = "svns-recent.json";

    /// <summary>
    /// Application name for user data directory
    /// </summary>
    public const string ApplicationName = "Svns";
}

/// <summary>
/// SVN file status icons
/// </summary>
public static class SvnStatusIcons
{
    public const string Normal = "✓";
    public const string Modified = "✎";
    public const string Added = "+";
    public const string Deleted = "−";
    public const string Conflicted = "⚠";
    public const string Ignored = "⊘";
    public const string Unversioned = "?";
    public const string Missing = "!";
    public const string Replaced = "↻";
    public const string External = "→";
    public const string Merged = "⤝";
}

/// <summary>
/// SVN status colors (hex)
/// </summary>
public static class SvnStatusColors
{
    public const string Normal = "#4CAF50";      // Green
    public const string Modified = "#2196F3";    // Blue
    public const string Added = "#00BCD4";       // Cyan
    public const string Deleted = "#F44336";     // Red
    public const string Conflicted = "#FF9800";  // Orange
    public const string Ignored = "#9E9E9E";     // Gray
    public const string Unversioned = "#BDBDBD"; // Light Gray
    public const string Missing = "#D32F2F";     // Dark Red
    public const string Replaced = "#9C27B0";    // Purple
    public const string External = "#607D8B";    // Blue Gray
    public const string Merged = "#8BC34A";      // Light Green
}
