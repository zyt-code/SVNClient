using Xunit;
using Svns.Services.Svn.Parsers;
using Svns.Models;
using System.Xml;

namespace Svns.Tests.Services;

public class SvnStatusParserTests
{
    private readonly SvnStatusParser _parser = new();

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
    public void Parse_SkipsStatusAgainstLine()
    {
        var output = "Status against revision: 123";
        var result = _parser.Parse(output);
        Assert.Empty(result);
    }

    [Fact]
    public void ParseXml_ReturnsEmptyList_WhenNoEntries()
    {
        var xml = new XmlDocument();
        xml.LoadXml("<status></status>");
        var result = _parser.ParseXml(xml);
        Assert.Empty(result);
    }

    [Fact]
    public void ParseXml_ParsesModifiedFile()
    {
        var xml = new XmlDocument();
        xml.LoadXml(@"<status>
            <entry path=""file.cs"">
                <wc-status item=""modified"" revision=""100"" props=""none"" />
            </entry>
        </status>");

        var result = _parser.ParseXml(xml);

        Assert.Single(result);
        Assert.Equal("file.cs", result[0].Path);
        Assert.Equal(SvnStatusType.Modified, result[0].WorkingCopyStatus);
        Assert.Equal(100, result[0].WorkingCopyRevision);
    }

    [Fact]
    public void ParseXml_ParsesAddedFile()
    {
        var xml = new XmlDocument();
        xml.LoadXml(@"<status>
            <entry path=""newfile.cs"">
                <wc-status item=""added"" props=""none"" />
            </entry>
        </status>");

        var result = _parser.ParseXml(xml);

        Assert.Single(result);
        Assert.Equal(SvnStatusType.Added, result[0].WorkingCopyStatus);
    }

    [Fact]
    public void ParseXml_ParsesDeletedFile()
    {
        var xml = new XmlDocument();
        xml.LoadXml(@"<status>
            <entry path=""deleted.cs"">
                <wc-status item=""deleted"" props=""none"" />
            </entry>
        </status>");

        var result = _parser.ParseXml(xml);

        Assert.Single(result);
        Assert.Equal(SvnStatusType.Deleted, result[0].WorkingCopyStatus);
    }

    [Fact]
    public void ParseXml_ParsesUnversionedFile()
    {
        var xml = new XmlDocument();
        xml.LoadXml(@"<status>
            <entry path=""newfile.txt"">
                <wc-status item=""unversioned"" props=""none"" />
            </entry>
        </status>");

        var result = _parser.ParseXml(xml);

        Assert.Single(result);
        Assert.Equal(SvnStatusType.Unversioned, result[0].WorkingCopyStatus);
    }

    [Fact]
    public void ParseXml_ParsesConflictedFile()
    {
        var xml = new XmlDocument();
        xml.LoadXml(@"<status>
            <entry path=""conflict.cs"">
                <wc-status item=""conflicted"" props=""none"" />
                <conflict />
            </entry>
        </status>");

        var result = _parser.ParseXml(xml);

        Assert.Single(result);
        Assert.Equal(SvnStatusType.Conflicted, result[0].WorkingCopyStatus);
        Assert.True(result[0].HasConflict);
    }

    [Fact]
    public void ParseXml_ParsesReposStatus()
    {
        var xml = new XmlDocument();
        xml.LoadXml(@"<status>
            <entry path=""file.cs"">
                <wc-status item=""modified"" revision=""100"" props=""none"" />
                <repos-status item=""modified"" />
            </entry>
        </status>");

        var result = _parser.ParseXml(xml);

        Assert.Single(result);
        Assert.Equal(SvnStatusType.Modified, result[0].RepositoryStatus);
    }

    [Fact]
    public void ParseXml_ParsesCommitInfo()
    {
        var xml = new XmlDocument();
        xml.LoadXml(@"<status>
            <entry path=""file.cs"">
                <wc-status item=""modified"" revision=""100"" props=""none"" />
                <commit revision=""99"">
                    <author>testuser</author>
                </commit>
            </entry>
        </status>");

        var result = _parser.ParseXml(xml);

        Assert.Single(result);
        Assert.Equal(99, result[0].LastChangedRevision);
        Assert.Equal("testuser", result[0].LastChangedAuthor);
    }

    [Fact]
    public void ParseXml_ParsesLockInfo()
    {
        var xml = new XmlDocument();
        xml.LoadXml(@"<status>
            <entry path=""file.cs"">
                <wc-status item=""normal"" revision=""100"" props=""none"" />
                <lock />
            </entry>
        </status>");

        var result = _parser.ParseXml(xml);

        Assert.Single(result);
        Assert.True(result[0].IsLocked);
    }

    [Fact]
    public void ParseXml_ParsesTreeConflict()
    {
        var xml = new XmlDocument();
        xml.LoadXml(@"<status>
            <entry path=""file.cs"">
                <wc-status item=""modified"" props=""none"" />
                <tree-conflict>local add, incoming add upon update</tree-conflict>
            </entry>
        </status>");

        var result = _parser.ParseXml(xml);

        Assert.Single(result);
        Assert.Equal("local add, incoming add upon update", result[0].TreeConflict);
    }

    [Fact]
    public void ParseXml_SkipsEntriesWithoutPath()
    {
        var xml = new XmlDocument();
        xml.LoadXml(@"<status>
            <entry>
                <wc-status item=""modified"" props=""none"" />
            </entry>
        </status>");

        var result = _parser.ParseXml(xml);

        Assert.Empty(result);
    }

    [Fact]
    public void ParseXml_ParsesMultipleEntries()
    {
        var xml = new XmlDocument();
        xml.LoadXml(@"<status>
            <entry path=""file1.cs"">
                <wc-status item=""modified"" props=""none"" />
            </entry>
            <entry path=""file2.cs"">
                <wc-status item=""added"" props=""none"" />
            </entry>
            <entry path=""file3.cs"">
                <wc-status item=""deleted"" props=""none"" />
            </entry>
        </status>");

        var result = _parser.ParseXml(xml);

        Assert.Equal(3, result.Count);
        Assert.Equal(SvnStatusType.Modified, result[0].WorkingCopyStatus);
        Assert.Equal(SvnStatusType.Added, result[1].WorkingCopyStatus);
        Assert.Equal(SvnStatusType.Deleted, result[2].WorkingCopyStatus);
    }
}
