using Xunit;
using Svns.Converters;
using Svns.Models;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Svns.Tests.Converters;

public class InverseBoolConverterTests
{
    private readonly InverseBoolConverter _converter = new();

    [Fact]
    public void Convert_ReturnsTrue_WhenValueIsFalse()
    {
        var result = _converter.Convert(false, typeof(bool), null, CultureInfo.InvariantCulture);

        Assert.Equal(true, result);
    }

    [Fact]
    public void Convert_ReturnsFalse_WhenValueIsTrue()
    {
        var result = _converter.Convert(true, typeof(bool), null, CultureInfo.InvariantCulture);

        Assert.Equal(false, result);
    }

    [Fact]
    public void Convert_ReturnsFalse_WhenValueIsNull()
    {
        var result = _converter.Convert(null, typeof(bool), null, CultureInfo.InvariantCulture);

        Assert.Equal(false, result);
    }

    [Fact]
    public void Convert_ReturnsFalse_WhenValueIsNotBool()
    {
        var result = _converter.Convert("string", typeof(bool), null, CultureInfo.InvariantCulture);

        Assert.Equal(false, result);
    }

    [Fact]
    public void ConvertBack_ReturnsTrue_WhenValueIsFalse()
    {
        var result = _converter.ConvertBack(false, typeof(bool), null, CultureInfo.InvariantCulture);

        Assert.Equal(true, result);
    }

    [Fact]
    public void ConvertBack_ReturnsFalse_WhenValueIsTrue()
    {
        var result = _converter.ConvertBack(true, typeof(bool), null, CultureInfo.InvariantCulture);

        Assert.Equal(false, result);
    }
}

public class FilePathConverterTests
{
    private readonly FilePathToFileNameConverter _fileNameConverter = new();
    private readonly FilePathToDirectoryNameConverter _directoryConverter = new();
    private readonly FilePathShortenerConverter _shortenerConverter = new();

    [Theory]
    [InlineData("/home/user/projects/file.cs", "file.cs")]
    [InlineData("file.cs", "file.cs")]
    public void FilePathToFileName_ConvertsCorrectly(string input, string expected)
    {
        var result = _fileNameConverter.Convert(input, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void FilePathToFileName_ConvertsWindowsPath_WhenOnWindows()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Skip on Unix systems
            return;
        }

        var result = _fileNameConverter.Convert(@"C:\file.txt", typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal("file.txt", result);
    }

    [Fact]
    public void FilePathToFileName_ReturnsNull_WhenValueIsNull()
    {
        var result = _fileNameConverter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Null(result);
    }

    [Fact]
    public void FilePathToFileName_ReturnsValue_WhenEmpty()
    {
        var result = _fileNameConverter.Convert("", typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal("", result);
    }

    [Theory]
    [InlineData("/home/user/projects/file.cs", "/home/user/projects")]
    [InlineData("file.cs", "file.cs")] // No directory, returns original
    public void FilePathToDirectoryName_ConvertsCorrectly(string input, string expected)
    {
        var result = _directoryConverter.Convert(input, typeof(string), null, CultureInfo.InvariantCulture);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && expected.Contains('/'))
        {
            expected = expected.Replace('/', '\\');
        }

        Assert.Equal(expected, result);
    }

    [Fact]
    public void FilePathToDirectoryName_ConvertsWindowsPath_WhenOnWindows()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Skip on Unix systems
            return;
        }

        var result = _directoryConverter.Convert(@"C:\Projects\MyProject\file.cs", typeof(string), null, CultureInfo.InvariantCulture);
        Assert.Equal(@"C:\Projects\MyProject", result);
    }

    [Fact]
    public void FilePathToDirectoryName_ReturnsNull_WhenValueIsNull()
    {
        var result = _directoryConverter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Null(result);
    }

    [Fact]
    public void FilePathShortener_ReturnsOriginal_WhenPathIsShort()
    {
        var shortPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? @"C:\short.txt"
            : "/short.txt";
        var result = _shortenerConverter.Convert(shortPath, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal(shortPath, result);
    }

    [Fact]
    public void FilePathShortener_ShortensLongPath()
    {
        var longPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? @"C:\Very\Long\Path\That\Exceeds\The\Maximum\Length\Allowed\For\Display\file.txt"
            : "/very/long/path/that/exceeds/the/maximum/length/allowed/for/display/file.txt";

        var result = _shortenerConverter.Convert(longPath, typeof(string), null, CultureInfo.InvariantCulture) as string;

        Assert.NotNull(result);
        Assert.True(result.Length <= 50 + 3); // Default MaxLength + "..."
        Assert.Contains("...", result);
    }

    [Fact]
    public void FilePathShortener_RespectsParameterLength()
    {
        var path = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? @"C:\Projects\MyProject\SubFolder\file.cs"
            : "/projects/myproject/subfolder/file.cs";

        var result = _shortenerConverter.Convert(path, typeof(string), "20", CultureInfo.InvariantCulture) as string;

        Assert.NotNull(result);
        Assert.True(result.Length <= 23); // 20 + "..."
    }

    [Fact]
    public void FilePathShortener_ReturnsNull_WhenValueIsNull()
    {
        var result = _shortenerConverter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Null(result);
    }
}

public class BoolToVisibilityConverterTests
{
    private readonly BoolToVisibilityConverter _converter = new();

    [Fact]
    public void Convert_ReturnsTrue_WhenValueIsTrue()
    {
        var result = _converter.Convert(true, typeof(bool), null, CultureInfo.InvariantCulture);

        Assert.Equal(true, result);
    }

    [Fact]
    public void Convert_ReturnsFalse_WhenValueIsFalse()
    {
        var result = _converter.Convert(false, typeof(bool), null, CultureInfo.InvariantCulture);

        Assert.Equal(false, result);
    }

    [Fact]
    public void Convert_ReturnsFalse_WhenValueIsNull()
    {
        var result = _converter.Convert(null, typeof(bool), null, CultureInfo.InvariantCulture);

        Assert.Equal(false, result);
    }

    [Fact]
    public void Convert_ReturnsFalse_WhenValueIsNotBool()
    {
        var result = _converter.Convert("not a bool", typeof(bool), null, CultureInfo.InvariantCulture);

        Assert.Equal(false, result);
    }
}

public class SvnStatusToStringConverterTests
{
    private readonly SvnStatusToStringConverter _converter = new();

    [Theory]
    [InlineData(SvnStatusType.Modified, "Modified")]
    [InlineData(SvnStatusType.Added, "Added")]
    [InlineData(SvnStatusType.Deleted, "Deleted")]
    [InlineData(SvnStatusType.Conflicted, "Conflicted")]
    [InlineData(SvnStatusType.Unversioned, "Unversioned")]
    [InlineData(SvnStatusType.Missing, "Missing")]
    [InlineData(SvnStatusType.Normal, "Normal")]
    [InlineData(SvnStatusType.Ignored, "Ignored")]
    [InlineData(SvnStatusType.Replaced, "Replaced")]
    [InlineData(SvnStatusType.External, "External")]
    [InlineData(SvnStatusType.Merged, "Merged")]
    [InlineData(SvnStatusType.Incomplete, "Incomplete")]
    [InlineData(SvnStatusType.Obstructed, "Obstructed")]
    public void Convert_ReturnsCorrectString_ForStatus(SvnStatusType status, string expected)
    {
        var result = _converter.Convert(status, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Convert_ReturnsUnknown_WhenValueIsNull()
    {
        var result = _converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal("Unknown", result);
    }

    [Fact]
    public void Convert_ReturnsUnknown_WhenValueIsNotStatus()
    {
        var result = _converter.Convert("invalid", typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal("Unknown", result);
    }
}

public class SvnStatusToIconConverterTests
{
    private readonly SvnStatusToIconConverter _converter = new();

    [Theory]
    [InlineData(SvnStatusType.Normal, "✓")]
    [InlineData(SvnStatusType.Modified, "✎")]
    [InlineData(SvnStatusType.Added, "+")]
    [InlineData(SvnStatusType.Deleted, "−")]
    [InlineData(SvnStatusType.Conflicted, "⚠")]
    [InlineData(SvnStatusType.Unversioned, "?")]
    [InlineData(SvnStatusType.Missing, "!")]
    [InlineData(SvnStatusType.Ignored, "⊘")]
    [InlineData(SvnStatusType.Replaced, "↻")]
    [InlineData(SvnStatusType.External, "→")]
    [InlineData(SvnStatusType.Merged, "⤝")]
    public void Convert_ReturnsCorrectIcon_ForStatus(SvnStatusType status, string expected)
    {
        var result = _converter.Convert(status, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Convert_ReturnsEmptyString_WhenValueIsNull()
    {
        var result = _converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal(string.Empty, result);
    }
}

public class IntToStringConverterTests
{
    private readonly IntToStringConverter _converter = new();

    [Theory]
    [InlineData(0, "0")]
    [InlineData(1, "1")]
    [InlineData(-1, "-1")]
    [InlineData(12345, "12345")]
    [InlineData(int.MaxValue, "2147483647")]
    public void Convert_ReturnsString_ForInteger(int value, string expected)
    {
        var result = _converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Convert_ReturnsZero_WhenValueIsNull()
    {
        var result = _converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal("0", result);
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("123", 123)]
    [InlineData("-456", -456)]
    public void ConvertBack_ReturnsInteger_ForValidString(string input, int expected)
    {
        var result = _converter.ConvertBack(input, typeof(int), null, CultureInfo.InvariantCulture);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ConvertBack_ReturnsZero_ForInvalidString()
    {
        var result = _converter.ConvertBack("not a number", typeof(int), null, CultureInfo.InvariantCulture);

        Assert.Equal(0, result);
    }
}

public class DateTimeConverterTests
{
    private readonly DateTimeConverter _converter = new();

    [Fact]
    public void Convert_ReturnsFormattedString_ForDateTime()
    {
        var dateTime = new DateTime(2024, 1, 15, 14, 30, 0);
        var result = _converter.Convert(dateTime, typeof(string), null, CultureInfo.InvariantCulture) as string;

        Assert.NotNull(result);
        Assert.Contains("2024", result);
    }

    [Fact]
    public void Convert_ReturnsNA_WhenValueIsNull()
    {
        var result = _converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

        // Result should be a localized "N/A" string, not empty
        Assert.NotNull(result);
        Assert.NotEqual(string.Empty, result);
    }

    [Fact]
    public void Convert_UsesCustomFormat_WhenProvided()
    {
        var dateTime = new DateTime(2024, 6, 20);
        var result = _converter.Convert(dateTime, typeof(string), "yyyy-MM-dd", CultureInfo.InvariantCulture) as string;

        Assert.Equal("2024-06-20", result);
    }

    [Fact]
    public void Convert_ReturnsNA_ForMinValue()
    {
        var result = _converter.Convert(DateTime.MinValue, typeof(string), null, CultureInfo.InvariantCulture);

        // Result should be a localized "N/A" string, not empty
        Assert.NotNull(result);
        Assert.NotEqual(string.Empty, result);
    }
}

public class MergeAcceptTypeConverterTests
{
    private readonly MergeAcceptTypeConverter _converter = new();

    [Theory]
    [InlineData(MergeAcceptType.Postpone)]
    [InlineData(MergeAcceptType.Base)]
    [InlineData(MergeAcceptType.MineConflict)]
    [InlineData(MergeAcceptType.TheirsConflict)]
    [InlineData(MergeAcceptType.MineFull)]
    [InlineData(MergeAcceptType.TheirsFull)]
    public void Convert_ReturnsDescription(MergeAcceptType type)
    {
        var result = _converter.Convert(type, typeof(string), null, CultureInfo.InvariantCulture) as string;

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }
}
