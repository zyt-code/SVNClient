using Xunit;
using Svns.ViewModels;

namespace Svns.Tests.ViewModels;

public class DiffViewModelTests
{
    [Fact]
    public void Constructor_SetsFilePathAndName()
    {
        var viewModel = new DiffViewModel(@"C:\repo\src\file.cs", "");
        Assert.Equal(@"C:\repo\src\file.cs", viewModel.FilePath);
        Assert.Equal("file.cs", viewModel.FileName);
    }

    [Fact]
    public void ParseDiff_CountsAddedLines()
    {
        var diff = @"@@ -1,3 +1,4 @@
 line1
+added line
 line2
 line3";
        var viewModel = new DiffViewModel("test.cs", diff);
        Assert.Equal(1, viewModel.AddedLines);
    }

    [Fact]
    public void ParseDiff_CountsRemovedLines()
    {
        var diff = @"@@ -1,4 +1,3 @@
 line1
-removed line
 line2
 line3";
        var viewModel = new DiffViewModel("test.cs", diff);
        Assert.Equal(1, viewModel.RemovedLines);
    }

    [Fact]
    public void ParseDiff_CountsTotalDiffs()
    {
        var diff = @"@@ -1,3 +1,3 @@
 line1
@@ -10,3 +10,3 @@
 line10";
        var viewModel = new DiffViewModel("test.cs", diff);
        Assert.Equal(2, viewModel.TotalDiffs);
    }

    [Fact]
    public void ParseDiff_HandlesMixedChanges()
    {
        var diff = @"@@ -1,5 +1,5 @@
 context
-old line
+new line
 more context
-another old
+another new";
        var viewModel = new DiffViewModel("test.cs", diff);
        Assert.Equal(2, viewModel.AddedLines);
        Assert.Equal(2, viewModel.RemovedLines);
    }

    [Fact]
    public void ParseDiff_HandlesEmptyDiff()
    {
        var viewModel = new DiffViewModel("test.cs", "");
        Assert.Equal(0, viewModel.AddedLines);
        Assert.Equal(0, viewModel.RemovedLines);
        Assert.Equal(0, viewModel.TotalDiffs);
        Assert.Empty(viewModel.DiffLines);
    }

    [Fact]
    public void DiffLines_ContainsAllLines()
    {
        var diff = @"@@ -1,2 +1,2 @@
 context
-removed";
        var viewModel = new DiffViewModel("test.cs", diff);
        Assert.Equal(3, viewModel.DiffLines.Count);
    }

    [Theory]
    [InlineData(DiffLineType.Added)]
    [InlineData(DiffLineType.Removed)]
    [InlineData(DiffLineType.Context)]
    [InlineData(DiffLineType.Header)]
    [InlineData(DiffLineType.FileHeader)]
    [InlineData(DiffLineType.Info)]
    public void DiffLineType_AllValuesExist(DiffLineType type)
    {
        Assert.True(Enum.IsDefined(typeof(DiffLineType), type));
    }

    [Fact]
    public void NextDiffCommand_IncrementsCurrentDiffIndex()
    {
        var diff = @"@@ -1,1 +1,1 @@
@@ -10,1 +10,1 @@";
        var viewModel = new DiffViewModel("test.cs", diff);
        viewModel.NextDiffCommand.Execute(null);
        Assert.Equal(1, viewModel.CurrentDiffIndex);
    }

    [Fact]
    public void PreviousDiffCommand_DecrementsCurrentDiffIndex()
    {
        var diff = @"@@ -1,1 +1,1 @@
@@ -10,1 +10,1 @@";
        var viewModel = new DiffViewModel("test.cs", diff);
        viewModel.CurrentDiffIndex = 1;
        viewModel.PreviousDiffCommand.Execute(null);
        Assert.Equal(0, viewModel.CurrentDiffIndex);
    }

    [Fact]
    public void PreviousDiffCommand_DoesNotGoBelowZero()
    {
        var viewModel = new DiffViewModel("test.cs", "@@ -1,1 +1,1 @@");
        viewModel.PreviousDiffCommand.Execute(null);
        Assert.Equal(0, viewModel.CurrentDiffIndex);
    }

    [Fact]
    public void CloseCommand_RaisesCloseRequestedEvent()
    {
        var viewModel = new DiffViewModel("test.cs", "");
        var eventRaised = false;
        viewModel.CloseRequested += (_, _) => eventRaised = true;
        viewModel.CloseCommand.Execute(null);
        Assert.True(eventRaised);
    }
}

public class DiffLineTests
{
    [Fact]
    public void DisplayOldLineNumber_ReturnsEmptyString_WhenNull()
    {
        var line = new DiffLine { OldLineNumber = null };
        Assert.Equal("", line.DisplayOldLineNumber);
    }

    [Fact]
    public void DisplayOldLineNumber_ReturnsNumber_WhenSet()
    {
        var line = new DiffLine { OldLineNumber = 10 };
        Assert.Equal("10", line.DisplayOldLineNumber);
    }

    [Fact]
    public void DisplayNewLineNumber_ReturnsEmptyString_WhenNull()
    {
        var line = new DiffLine { NewLineNumber = null };
        Assert.Equal("", line.DisplayNewLineNumber);
    }

    [Fact]
    public void DisplayNewLineNumber_ReturnsNumber_WhenSet()
    {
        var line = new DiffLine { NewLineNumber = 20 };
        Assert.Equal("20", line.DisplayNewLineNumber);
    }

    [Theory]
    [InlineData(DiffLineType.Context, " context line", "context line")]
    [InlineData(DiffLineType.Added, "+added line", "added line")]
    [InlineData(DiffLineType.Removed, "-removed line", "removed line")]
    [InlineData(DiffLineType.Header, "@@ -1,1 +1,1 @@", "@@ -1,1 +1,1 @@")]
    public void DisplayContent_StripsPrefix_ForRelevantTypes(DiffLineType type, string content, string expected)
    {
        var line = new DiffLine { Type = type, Content = content };
        Assert.Equal(expected, line.DisplayContent);
    }
}
