using Xunit;
using Svns.Models;

namespace Svns.Tests.Models;

public class SvnBlameTests
{
    [Fact]
    public void SvnBlameLine_DefaultValues_AreInitialized()
    {
        var line = new SvnBlameLine();
        Assert.Equal(0, line.LineNumber);
        Assert.Equal(0, line.Revision);
        Assert.Equal(string.Empty, line.Author);
        Assert.Equal(string.Empty, line.Content);
        Assert.False(line.IsMerged);
    }

    [Fact]
    public void SvnBlameLine_Properties_CanBeSet()
    {
        var date = DateTime.Now;
        var line = new SvnBlameLine
        {
            LineNumber = 10,
            Revision = 100,
            Author = "user",
            Date = date,
            Content = "public void Method()",
            IsMerged = true
        };

        Assert.Equal(10, line.LineNumber);
        Assert.Equal(100, line.Revision);
        Assert.Equal("user", line.Author);
        Assert.Equal(date, line.Date);
        Assert.Equal("public void Method()", line.Content);
        Assert.True(line.IsMerged);
    }

    [Fact]
    public void SvnBlameResult_DefaultValues_AreInitialized()
    {
        var result = new SvnBlameResult();
        Assert.Equal(string.Empty, result.Path);
        Assert.Empty(result.Lines);
        Assert.Equal(0, result.Revision);
    }

    [Fact]
    public void SvnBlameResult_UniqueRevisions_ReturnsDistinctOrdered()
    {
        var result = new SvnBlameResult
        {
            Lines = new List<SvnBlameLine>
            {
                new() { Revision = 5 },
                new() { Revision = 3 },
                new() { Revision = 5 },
                new() { Revision = 1 }
            }
        };

        var revisions = result.UniqueRevisions.ToList();
        Assert.Equal(new long[] { 1, 3, 5 }, revisions);
    }

    [Fact]
    public void SvnBlameResult_UniqueAuthors_ReturnsDistinctOrdered()
    {
        var result = new SvnBlameResult
        {
            Lines = new List<SvnBlameLine>
            {
                new() { Author = "charlie" },
                new() { Author = "alice" },
                new() { Author = "charlie" },
                new() { Author = "bob" }
            }
        };

        var authors = result.UniqueAuthors.ToList();
        Assert.Equal(new[] { "alice", "bob", "charlie" }, authors);
    }

    [Fact]
    public void SvnBlameResult_AuthorLineCount_ReturnsCorrectCounts()
    {
        var result = new SvnBlameResult
        {
            Lines = new List<SvnBlameLine>
            {
                new() { Author = "alice" },
                new() { Author = "bob" },
                new() { Author = "alice" },
                new() { Author = "alice" }
            }
        };

        var counts = result.AuthorLineCount;
        Assert.Equal(3, counts["alice"]);
        Assert.Equal(1, counts["bob"]);
    }

    [Fact]
    public void SvnBlameResult_DateRange_ReturnsMinMax()
    {
        var date1 = new DateTime(2024, 1, 1);
        var date2 = new DateTime(2024, 6, 15);
        var date3 = new DateTime(2024, 12, 31);

        var result = new SvnBlameResult
        {
            Lines = new List<SvnBlameLine>
            {
                new() { Date = date2 },
                new() { Date = date1 },
                new() { Date = date3 }
            }
        };

        var range = result.DateRange;
        Assert.Equal(date1, range.Start);
        Assert.Equal(date3, range.End);
    }

    [Fact]
    public void SvnBlameResult_DateRange_ReturnsNull_WhenEmpty()
    {
        var result = new SvnBlameResult();
        var range = result.DateRange;
        Assert.Null(range.Start);
        Assert.Null(range.End);
    }
}
