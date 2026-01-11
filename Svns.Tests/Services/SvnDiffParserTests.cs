using Xunit;
using Svns.Services.Svn.Parsers;
using Svns.Models;

namespace Svns.Tests.Services;

public class SvnDiffParserTests
{
    private readonly SvnDiffParser _parser = new();

    [Fact]
    public void Parse_ReturnsEmptyResult_WhenOutputIsEmpty()
    {
        var result = _parser.Parse("");

        Assert.NotNull(result);
        Assert.Empty(result.Lines);
        Assert.False(result.IsBinary);
    }

    [Fact]
    public void Parse_ReturnsEmptyResult_WhenOutputIsNull()
    {
        var result = _parser.Parse(null!);

        Assert.NotNull(result);
        Assert.Empty(result.Lines);
    }

    [Fact]
    public void Parse_ReturnsEmptyResult_WhenOutputIsWhitespace()
    {
        var result = _parser.Parse("   \n\t  ");

        Assert.NotNull(result);
        Assert.Empty(result.Lines);
    }

    [Fact]
    public void Parse_DetectsBinaryDiff()
    {
        var output = "Binary files a/image.png and b/image.png differ";

        var result = _parser.Parse(output);

        Assert.True(result.IsBinary);
        Assert.NotNull(result.BinaryMessage);
    }

    [Fact]
    public void Parse_ParsesAdditionLine()
    {
        var output = "+this is a new line";

        var result = _parser.Parse(output);

        Assert.Single(result.Lines);
        Assert.Equal(SvnDiffLineType.Addition, result.Lines[0].Type);
        Assert.Equal("+this is a new line", result.Lines[0].Content);
    }

    [Fact]
    public void Parse_ParsesDeletionLine()
    {
        var output = "-this line was removed";

        var result = _parser.Parse(output);

        Assert.Single(result.Lines);
        Assert.Equal(SvnDiffLineType.Deletion, result.Lines[0].Type);
    }

    [Fact]
    public void Parse_ParsesContextLine()
    {
        var output = " this is a context line";

        var result = _parser.Parse(output);

        Assert.Single(result.Lines);
        Assert.Equal(SvnDiffLineType.Context, result.Lines[0].Type);
    }

    [Fact]
    public void Parse_ParsesHeaderLine()
    {
        // Header regex requires format: "Index ", "diff ", "--- ", or "+++ " followed by content
        var output = "--- file.cs	(revision 100)";

        var result = _parser.Parse(output);

        Assert.Single(result.Lines);
        Assert.Equal(SvnDiffLineType.Header, result.Lines[0].Type);
    }

    [Theory]
    [InlineData("--- file.cs")]
    [InlineData("+++ file.cs")]
    [InlineData("diff --git a/file b/file")]
    public void Parse_ParsesVariousHeaderFormats(string output)
    {
        var result = _parser.Parse(output);

        Assert.Single(result.Lines);
        Assert.Equal(SvnDiffLineType.Header, result.Lines[0].Type);
    }

    [Fact]
    public void Parse_ParsesHunkHeader()
    {
        var output = "@@ -1,5 +1,7 @@";

        var result = _parser.Parse(output);

        Assert.Single(result.Lines);
        Assert.Equal(SvnDiffLineType.HunkHeader, result.Lines[0].Type);
        Assert.Equal(1, result.Lines[0].OriginalLineNumber);
        Assert.Equal(1, result.Lines[0].ModifiedLineNumber);
    }

    [Fact]
    public void Parse_ParsesCompleteDiff()
    {
        var output = @"Index: file.cs
===================================================================
--- file.cs	(revision 100)
+++ file.cs	(working copy)
@@ -1,3 +1,4 @@
 line1
-line2
+line2 modified
+new line
 line3";

        var result = _parser.Parse(output);

        Assert.NotEmpty(result.Lines);
        Assert.False(result.IsBinary);
    }

    [Fact]
    public void Parse_CountsAdditionsCorrectly()
    {
        var output = @"+line1
+line2
-removed
 context";

        var result = _parser.Parse(output);

        Assert.Equal(2, result.AdditionCount);
    }

    [Fact]
    public void Parse_CountsDeletionsCorrectly()
    {
        var output = @"-line1
-line2
-line3
+added";

        var result = _parser.Parse(output);

        Assert.Equal(3, result.DeletionCount);
    }

    [Fact]
    public void ParseMultiple_ReturnsEmptyList_WhenOutputIsEmpty()
    {
        var result = _parser.ParseMultiple("");

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ParseMultiple_ParsesSingleDiff()
    {
        var output = @"Index: file1.cs
+line1";

        var result = _parser.ParseMultiple(output);

        Assert.Single(result);
    }

    [Fact]
    public void CreateUnifiedDiff_CreatesCorrectDiff()
    {
        var original = "line1\nline2\nline3";
        var modified = "line1\nmodified\nline3\nline4";

        var result = _parser.CreateUnifiedDiff(original, modified, "original.txt", "modified.txt");

        Assert.Equal("original.txt", result.OriginalPath);
        Assert.Equal("modified.txt", result.ModifiedPath);
        Assert.Equal("unified", result.DiffType);
        Assert.NotEmpty(result.Lines);
    }

    [Fact]
    public void CreateUnifiedDiff_DetectsAdditions()
    {
        var original = "line1";
        var modified = "line1\nline2";

        var result = _parser.CreateUnifiedDiff(original, modified, "a", "b");

        Assert.True(result.AdditionCount > 0);
    }

    [Fact]
    public void CreateUnifiedDiff_DetectsDeletions()
    {
        var original = "line1\nline2";
        var modified = "line1";

        var result = _parser.CreateUnifiedDiff(original, modified, "a", "b");

        Assert.True(result.DeletionCount > 0);
    }

    [Fact]
    public void CreateUnifiedDiff_DetectsModifications()
    {
        var original = "original line";
        var modified = "modified line";

        var result = _parser.CreateUnifiedDiff(original, modified, "a", "b");

        // Modification is shown as deletion + addition
        Assert.True(result.AdditionCount > 0);
        Assert.True(result.DeletionCount > 0);
    }

    [Fact]
    public void ParseXml_ReturnsEmptyResult()
    {
        // SVN doesn't output XML for diff
        var xml = new System.Xml.XmlDocument();
        xml.LoadXml("<diff></diff>");

        var result = _parser.ParseXml(xml);

        Assert.NotNull(result);
        Assert.False(result.IsBinary);
    }
}
