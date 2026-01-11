using Xunit;
using Svns.Services.Svn.Parsers;
using Svns.Models;
using System.Xml;

namespace Svns.Tests.Services;

public class SvnLogParserTests
{
    private readonly SvnLogParser _parser = new();

    [Fact]
    public void Parse_ReturnsEmptyList_WhenOutputIsEmpty()
    {
        var result = _parser.Parse("");
        Assert.Empty(result);
    }

    [Fact]
    public void Parse_ReturnsEmptyList_WhenOutputIsNull()
    {
        var result = _parser.Parse(null!);
        Assert.Empty(result);
    }

    [Fact]
    public void Parse_ReturnsEmptyList_WhenOutputIsWhitespace()
    {
        var result = _parser.Parse("   \n\t  ");
        Assert.Empty(result);
    }

    [Fact]
    public void Parse_ParsesSingleEntry()
    {
        var output = @"------------------------------------------------------------------------
r100 | testuser | 2024-01-10 12:34:56 +0000 | 1 line

Test commit message
------------------------------------------------------------------------";

        var result = _parser.Parse(output);

        Assert.Single(result);
        Assert.Equal(100, result[0].Revision);
        Assert.Equal("testuser", result[0].Author);
        Assert.Equal("Test commit message", result[0].Message);
    }

    [Fact]
    public void Parse_ParsesMultipleEntries()
    {
        var output = @"------------------------------------------------------------------------
r100 | user1 | 2024-01-10 12:34:56 +0000 | 1 line

First commit
------------------------------------------------------------------------
r99 | user2 | 2024-01-09 10:00:00 +0000 | 1 line

Second commit
------------------------------------------------------------------------";

        var result = _parser.Parse(output);

        Assert.Equal(2, result.Count);
        Assert.Equal(100, result[0].Revision);
        Assert.Equal(99, result[1].Revision);
    }

    [Fact]
    public void Parse_ParsesChangedPaths()
    {
        var output = @"------------------------------------------------------------------------
r100 | testuser | 2024-01-10 12:34:56 +0000 | 1 line
Changed paths:
   A /trunk/newfile.cs
   M /trunk/existing.cs
   D /trunk/deleted.cs

Commit with changes
------------------------------------------------------------------------";

        var result = _parser.Parse(output);

        Assert.Single(result);
        Assert.Equal(3, result[0].ChangedPaths.Count);
        Assert.Equal(SvnPathAction.Added, result[0].ChangedPaths[0].Action);
        Assert.Equal(SvnPathAction.Modified, result[0].ChangedPaths[1].Action);
        Assert.Equal(SvnPathAction.Deleted, result[0].ChangedPaths[2].Action);
    }

    [Fact]
    public void ParseXml_ReturnsEmptyList_WhenNoLogEntries()
    {
        var xml = new XmlDocument();
        xml.LoadXml("<log></log>");
        var result = _parser.ParseXml(xml);
        Assert.Empty(result);
    }

    [Fact]
    public void ParseXml_ParsesSingleEntry()
    {
        var xml = new XmlDocument();
        xml.LoadXml(@"<log>
            <logentry revision=""100"">
                <author>testuser</author>
                <date>2024-01-10T12:34:56.000000Z</date>
                <msg>Test commit message</msg>
            </logentry>
        </log>");

        var result = _parser.ParseXml(xml);

        Assert.Single(result);
        Assert.Equal(100, result[0].Revision);
        Assert.Equal("testuser", result[0].Author);
        Assert.Equal("Test commit message", result[0].Message);
    }

    [Fact]
    public void ParseXml_ParsesMultipleEntries()
    {
        var xml = new XmlDocument();
        xml.LoadXml(@"<log>
            <logentry revision=""100"">
                <author>user1</author>
                <date>2024-01-10T12:34:56.000000Z</date>
                <msg>First</msg>
            </logentry>
            <logentry revision=""99"">
                <author>user2</author>
                <date>2024-01-09T10:00:00.000000Z</date>
                <msg>Second</msg>
            </logentry>
        </log>");

        var result = _parser.ParseXml(xml);

        Assert.Equal(2, result.Count);
        Assert.Equal(100, result[0].Revision);
        Assert.Equal(99, result[1].Revision);
    }

    [Fact]
    public void ParseXml_ParsesChangedPaths()
    {
        var xml = new XmlDocument();
        xml.LoadXml(@"<log>
            <logentry revision=""100"">
                <author>testuser</author>
                <date>2024-01-10T12:34:56.000000Z</date>
                <msg>Test</msg>
                <paths>
                    <path action=""A"">/trunk/newfile.cs</path>
                    <path action=""M"">/trunk/modified.cs</path>
                    <path action=""D"">/trunk/deleted.cs</path>
                    <path action=""R"">/trunk/replaced.cs</path>
                </paths>
            </logentry>
        </log>");

        var result = _parser.ParseXml(xml);

        Assert.Single(result);
        Assert.Equal(4, result[0].ChangedPaths.Count);
        Assert.Equal(SvnPathAction.Added, result[0].ChangedPaths[0].Action);
        Assert.Equal("/trunk/newfile.cs", result[0].ChangedPaths[0].Path);
        Assert.Equal(SvnPathAction.Modified, result[0].ChangedPaths[1].Action);
        Assert.Equal(SvnPathAction.Deleted, result[0].ChangedPaths[2].Action);
        Assert.Equal(SvnPathAction.Replaced, result[0].ChangedPaths[3].Action);
    }

    [Fact]
    public void ParseXml_ParsesCopyInfo()
    {
        var xml = new XmlDocument();
        xml.LoadXml(@"<log>
            <logentry revision=""100"">
                <author>testuser</author>
                <date>2024-01-10T12:34:56.000000Z</date>
                <msg>Copy file</msg>
                <paths>
                    <path action=""A"" copyfrom-path=""/trunk/original.cs"" copyfrom-rev=""99"">/trunk/copy.cs</path>
                </paths>
            </logentry>
        </log>");

        var result = _parser.ParseXml(xml);

        Assert.Single(result);
        Assert.Single(result[0].ChangedPaths);
        var path = result[0].ChangedPaths[0];
        Assert.Equal("/trunk/original.cs", path.CopyFromPath);
        Assert.Equal(99, path.CopyFromRevision);
    }

    [Fact]
    public void ParseXml_SkipsEntriesWithoutRevision()
    {
        var xml = new XmlDocument();
        xml.LoadXml(@"<log>
            <logentry>
                <author>testuser</author>
                <msg>No revision</msg>
            </logentry>
        </log>");

        var result = _parser.ParseXml(xml);

        Assert.Empty(result);
    }

    [Fact]
    public void ParseXml_HandlesEmptyMessage()
    {
        var xml = new XmlDocument();
        xml.LoadXml(@"<log>
            <logentry revision=""100"">
                <author>testuser</author>
                <date>2024-01-10T12:34:56.000000Z</date>
                <msg></msg>
            </logentry>
        </log>");

        var result = _parser.ParseXml(xml);

        Assert.Single(result);
        Assert.Equal("", result[0].Message);
    }

    [Fact]
    public void ParseXml_HandlesMissingAuthor()
    {
        var xml = new XmlDocument();
        xml.LoadXml(@"<log>
            <logentry revision=""100"">
                <date>2024-01-10T12:34:56.000000Z</date>
                <msg>No author</msg>
            </logentry>
        </log>");

        var result = _parser.ParseXml(xml);

        Assert.Single(result);
        Assert.Equal("", result[0].Author);
    }
}
