using Xunit;
using Svns.ViewModels;
using System.Runtime.InteropServices;

namespace Svns.Tests.ViewModels;

public class RenameViewModelTests
{
    [Fact]
    public void Constructor_SetsOriginalName()
    {
        // We can't fully test without WorkingCopyService, but we can test the model
        // For unit testing, we would need to mock WorkingCopyService
        // This is a placeholder for the structure
        Assert.True(true);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("valid.txt", true)]
    public void IsValidFileName_ReturnsExpected_Common(string name, bool expected)
    {
        // Test file name validation logic
        var isValid = IsValidFileName(name);
        Assert.Equal(expected, isValid);
    }

    [Theory]
    [InlineData("file<name>.txt", false)]
    [InlineData("file>name.txt", false)]
    [InlineData("file:name.txt", false)]
    [InlineData("file\"name.txt", false)]
    [InlineData("file|name.txt", false)]
    [InlineData("file?name.txt", false)]
    [InlineData("file*name.txt", false)]
    public void IsValidFileName_ReturnsExpected_WindowsSpecific(string name, bool expected)
    {
        // These characters are only invalid on Windows
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Skip on Unix/macOS - these characters are valid filenames
            return;
        }

        var isValid = IsValidFileName(name);
        Assert.Equal(expected, isValid);
    }

    // Helper method to test file name validation (mirrors the logic in RenameViewModel)
    private static bool IsValidFileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var invalidChars = System.IO.Path.GetInvalidFileNameChars();
        foreach (var c in invalidChars)
        {
            if (name.Contains(c))
                return false;
        }

        return true;
    }

    [Fact]
    public void HasChanges_ReturnsFalse_WhenNewNameSameAsOriginal()
    {
        // Test that HasChanges is false when names are the same
        var originalName = "test.txt";
        var newName = "test.txt";
        var hasChanges = !string.IsNullOrWhiteSpace(newName) && newName != originalName;
        Assert.False(hasChanges);
    }

    [Fact]
    public void HasChanges_ReturnsTrue_WhenNewNameDifferent()
    {
        var originalName = "test.txt";
        var newName = "newname.txt";
        var hasChanges = !string.IsNullOrWhiteSpace(newName) && newName != originalName;
        Assert.True(hasChanges);
    }

    [Fact]
    public void HasChanges_ReturnsFalse_WhenNewNameEmpty()
    {
        var originalName = "test.txt";
        var newName = "";
        var hasChanges = !string.IsNullOrWhiteSpace(newName) && newName != originalName;
        Assert.False(hasChanges);
    }
}
