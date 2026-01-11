# Cross-Platform Compatibility

This document describes the cross-platform compatibility considerations and implementation details for Svns.

## Supported Platforms

- **Windows**: .NET 9.0+
- **macOS**: .NET 9.0+
- **Linux**: .NET 9.0+

## Architecture Principles

### 1. Use Standard .NET APIs

Always use cross-platform .NET APIs instead of platform-specific ones:

```csharp
// ✅ Good - Cross-platform
var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
var settingsPath = Path.Combine(appDataPath, "Svns", "settings.json");

// ❌ Bad - Windows only
var appDataPath = @"C:\Users\%USERNAME%\AppData\Local";
```

### 2. Use Path.Combine for Paths

Never hard-code path separators. Use `Path.Combine()` or `Path.DirectorySeparatorChar`:

```csharp
// ✅ Good
var fullPath = Path.Combine(directory, filename);
var displayPath = prefix + "..." + Path.DirectorySeparatorChar + filename;

// ❌ Bad
var fullPath = directory + "\\" + filename;
```

### 3. Use ProcessHelper for Platform Detection

The `ProcessHelper` class provides utility methods for platform detection:

```csharp
if (ProcessHelper.IsWindows())
{
    // Windows-specific code
}
else if (ProcessHelper.IsMacOS())
{
    // macOS-specific code
}
else if (ProcessHelper.IsLinux())
{
    // Linux-specific code
}
```

## Platform-Specific Considerations

### Executable Names

- **Windows**: `svn.exe`
- **macOS/Linux**: `svn`

Never hard-code `.exe` extension in source code. Let the platform detection handle it:

```csharp
// In ProcessHelper.FindExecutableInPath
if (ProcessHelper.IsWindows())
{
    var exePath = fullPath + ".exe";
    if (File.Exists(exePath))
        return exePath;
}
```

### Application Data Directory

| Platform | Path |
|----------|------|
| Windows | `%LocalAppData%\Svns` |
| macOS | `~/Library/Application Support/Svns` |
| Linux | `~/.local/share/Svns` |

Implemented via `Environment.SpecialFolder.LocalApplicationData`.

### File Open Dialog Filters

File picker patterns should be platform-specific:

```csharp
var executablePatterns = ProcessHelper.IsWindows()
    ? new[] { "*.exe", "svn.exe" }
    : ProcessHelper.IsMacOS()
        ? new[] { "svn" }
        : new[] { "svn", "svn.bin" };
```

## URL vs Local Path Handling

### SVN Branch/Tag Creation

For branch/tag operations with `-m` (log message), both source and destination must be **repository URLs**, not local paths. The `-m` flag is only valid for URL-to-URL copies:

```csharp
// ✅ Good - URL-to-URL copy (server-side operation)
public static SvnCommand Copy(string sourceUrl, string destinationUrl, string message)
{
    return new SvnCommand("copy", sourceUrl, destinationUrl, "-m", message);
}
```

**Important**: Do NOT convert `file://` URLs to local paths for branch/tag operations. Keep them as URLs so SVN performs the operation on the repository.

## Common Pitfalls

### 1. Hard-coded Path Separators

```csharp
// ❌ Bad
path = path.Replace('/', '\\');

// ✅ Good - Use Path API or handle both
var trimmed = path.TrimEnd('/', '\\');
```

### 2. Platform-Specific Error Messages

```csharp
// ❌ Bad
"SVN executable (svn.exe) not found"

// ✅ Good
"SVN executable not found"
```

### 3. Assuming Windows in Constants

```csharp
// ❌ Bad
public const string SvnExecutableNameWindows = "svn.exe";

// ✅ Good
public const string SvnExecutableName = "svn";
```

## Changes Made

### Summary of All Changes (2025-01)

This section documents all cross-platform and internationalization improvements made to Svns.

**Total Changes**: 17 files modified
**Categories**: Cross-platform compatibility, Performance optimization, Internationalization, Cultural formatting
**Test Coverage**: 683 tests passing

### Initial Fixes (2025-01)

1. **SvnException.cs** - Removed hardcoded ".exe" from error message
2. **SettingsWindow.axaml.cs** - Platform-specific file picker patterns
3. **SvnCommand.cs** - Removed Windows-only `ConvertFileUrlToLocalPath` method
4. **SvnConstants.cs** - Removed `SvnExecutableNameWindows` constant
5. **FilePathConverter.cs** - Use `Path.DirectorySeparatorChar` instead of hardcoded `\`
6. **SvnExceptionTests.cs** - Updated test assertions

### Platform Detection Optimizations (2025-01)

7. **ProcessHelper.cs** - Unified platform detection using `RuntimeInformation.IsOSPlatform()`
8. **ProcessHelper.cs** - Added caching for platform detection results
9. **ProcessHelper.cs** - Refactored to use `IsWindows()` helper instead of inline checks

```csharp
// Optimized platform detection with caching
private static bool? _isWindows;
private static bool? _isMacOS;
private static bool? _isLinux;

public static bool IsWindows()
{
    if (_isWindows.HasValue)
        return _isWindows.Value;
    _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    return _isWindows.Value;
}
```

### Internationalization Improvements (2025-01)

10. **DateTimeConverter.cs** - Updated to use localization service for "N/A" text
11. **RelativeTimeConverter.cs** - Now uses localized strings for relative time display
12. **Strings.en-US.json** - Added relative time localization keys
13. **Strings.zh-CN.json** - Added Chinese translations for relative time

```csharp
// Before: Hardcoded English strings
return "just now";
return $"{minutes} minutes ago";

// After: Localized strings
return LocalizationService.Instance.GetString("RelativeTime.JustNow");
return LocalizationService.Instance.GetString("RelativeTime.MinutesAgo", minutes);
```

14. **ConvertersTests.cs** - Updated tests to handle localized "N/A" strings

### Cultural Formatting Fixes (2025-01)

15. **SvnListParser.cs** - Fixed `int.Parse` and `long.Parse` to use `CultureInfo.InvariantCulture`
16. **DiffViewModel.cs** - Fixed `int.Parse` to use `CultureInfo.InvariantCulture`

**Why this matters**: SVN output always uses standard numeric format (period for decimals, no thousand separators). Using the current culture for parsing could fail on systems with different numeric separators (e.g., French locale uses comma as decimal separator).

```csharp
// Before: Uses current culture (may fail on non-English locales)
var revision = long.Parse(parts[0]);
var day = int.Parse(parts[1]);

// After: Always uses invariant culture
var revision = long.Parse(parts[0], CultureInfo.InvariantCulture);
var day = int.Parse(parts[1], CultureInfo.InvariantCulture);
```

## Testing

Cross-platform compatibility is tested on:

- **Windows**: Primary development platform
- **macOS**: Tested via CI/manual testing
- **Linux**: Tested via CI/manual testing

To ensure cross-platform compatibility:

1. Use `Path.Combine()` for all path construction
2. Use `Environment.SpecialFolder` for system paths
3. Use `ProcessHelper` for platform detection
4. Avoid platform-specific APIs unless guarded by platform checks
5. Keep URLs as URLs - don't convert to local paths unnecessarily

## Future Considerations

### File Version Info

The `GetExecutableVersion` method in `ProcessHelper` uses `FileVersionInfo.GetVersionInfo`, which is Windows-only. On macOS/Linux, this returns null. Consider using platform-specific alternatives if version info is needed:

- **macOS**: `otool` or `mdls`
- **Linux**: `ldd` or `readelf`

### Process Environment

The `GetProcessEnvironment` method is currently a stub. Cross-platform implementation would require:

- **Windows**: WMI or P/Invoke
- **macOS/Linux**: `/proc` filesystem (Linux) or `sysctl` (macOS)
