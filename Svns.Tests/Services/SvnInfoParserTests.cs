using Xunit;
using Svns.Services.Svn.Parsers;
using Svns.Models;
using System.Xml;

namespace Svns.Tests.Services;

public class SvnInfoParserTests
{
    private readonly SvnInfoParser _parser = new();

    [Fact]
    public void Parse_ReturnsEmptyInfo_WhenOutputIsEmpty()
    {
        var result = _parser.Parse("");

        Assert.NotNull(result);
        Assert.Equal(string.Empty, result.Path);
    }

    [Fact]
    public void Parse_ReturnsEmptyInfo_WhenOutputIsNull()
    {
        var result = _parser.Parse(null!);

        Assert.NotNull(result);
    }

    [Fact]
    public void Parse_ParsesPath()
    {
        var output = "Path: myfile.cs";

        var result = _parser.Parse(output);

        Assert.Equal("myfile.cs", result.Path);
    }

    [Fact]
    public void Parse_ParsesUrl()
    {
        var output = "URL: https://svn.example.com/repos/trunk/file.cs";

        var result = _parser.Parse(output);

        Assert.Equal("https://svn.example.com/repos/trunk/file.cs", result.Url);
    }

    [Fact]
    public void Parse_ParsesRevision()
    {
        var output = "Revision: 12345";

        var result = _parser.Parse(output);

        Assert.Equal(12345, result.Revision);
    }

    [Fact]
    public void Parse_ParsesRepositoryRoot()
    {
        var output = "Repository Root: https://svn.example.com/repos";

        var result = _parser.Parse(output);

        Assert.Equal("https://svn.example.com/repos", result.RepositoryRootUrl);
    }

    [Fact]
    public void Parse_ParsesRepositoryUuid()
    {
        var output = "Repository UUID: 12345678-1234-1234-1234-123456789abc";

        var result = _parser.Parse(output);

        Assert.Equal("12345678-1234-1234-1234-123456789abc", result.RepositoryUuid);
    }

    [Fact]
    public void Parse_ParsesNodeKind()
    {
        var output = "Node Kind: file";

        var result = _parser.Parse(output);

        Assert.Equal("file", result.NodeKind);
    }

    [Fact]
    public void Parse_ParsesSchedule()
    {
        var output = "Schedule: normal";

        var result = _parser.Parse(output);

        Assert.Equal("normal", result.Schedule);
    }

    [Fact]
    public void Parse_ParsesLastChangedAuthor()
    {
        var output = "Last Changed Author: developer";

        var result = _parser.Parse(output);

        Assert.Equal("developer", result.LastChangedAuthor);
    }

    [Fact]
    public void Parse_ParsesLastChangedRev()
    {
        var output = "Last Changed Rev: 9999";

        var result = _parser.Parse(output);

        Assert.Equal(9999, result.LastChangedRevision);
    }

    [Fact]
    public void Parse_ParsesWorkingCopyRoot()
    {
        var output = "Working Copy Root Path: /home/user/project";

        var result = _parser.Parse(output);

        Assert.Equal("/home/user/project", result.WorkingCopyRoot);
    }

    [Fact]
    public void Parse_ParsesRelativeUrl()
    {
        var output = "Relative URL: ^/trunk/file.cs";

        var result = _parser.Parse(output);

        Assert.Equal("^/trunk/file.cs", result.RelativePath);
    }

    [Fact]
    public void Parse_ParsesMultipleFields()
    {
        var output = @"Path: file.cs
URL: https://svn.example.com/repos/trunk/file.cs
Repository Root: https://svn.example.com/repos
Revision: 100
Last Changed Author: admin
Last Changed Rev: 99";

        var result = _parser.Parse(output);

        Assert.Equal("file.cs", result.Path);
        Assert.Equal("https://svn.example.com/repos/trunk/file.cs", result.Url);
        Assert.Equal("https://svn.example.com/repos", result.RepositoryRootUrl);
        Assert.Equal(100, result.Revision);
        Assert.Equal("admin", result.LastChangedAuthor);
        Assert.Equal(99, result.LastChangedRevision);
    }

    [Fact]
    public void Parse_IgnoresInvalidLines()
    {
        var output = @"This line has no colon
Path: valid.cs
Another invalid line";

        var result = _parser.Parse(output);

        Assert.Equal("valid.cs", result.Path);
    }

    [Fact]
    public void ParseXml_ParsesEntryAttributes()
    {
        var xml = new XmlDocument();
        xml.LoadXml(@"<info>
            <entry path=""file.cs"" revision=""123"" kind=""file"">
                <url>https://svn.example.com/repos/file.cs</url>
            </entry>
        </info>");

        var result = _parser.ParseXml(xml);

        Assert.Equal("file.cs", result.Path);
        Assert.Equal(123, result.Revision);
        Assert.Equal("file", result.NodeKind);
    }

    [Fact]
    public void ParseXml_ParsesRepository()
    {
        var xml = new XmlDocument();
        xml.LoadXml(@"<info>
            <entry path=""file.cs"">
                <repository>
                    <root>https://svn.example.com/repos</root>
                    <uuid>12345678-abcd-1234-efgh-123456789abc</uuid>
                </repository>
            </entry>
        </info>");

        var result = _parser.ParseXml(xml);

        Assert.Equal("https://svn.example.com/repos", result.RepositoryRootUrl);
        Assert.Equal("12345678-abcd-1234-efgh-123456789abc", result.RepositoryUuid);
    }

    [Fact]
    public void ParseXml_ParsesCommit()
    {
        var xml = new XmlDocument();
        xml.LoadXml(@"<info>
            <entry path=""file.cs"">
                <commit revision=""456"">
                    <author>developer</author>
                    <date>2024-01-15T10:30:00.000000Z</date>
                </commit>
            </entry>
        </info>");

        var result = _parser.ParseXml(xml);

        Assert.Equal(456, result.LastChangedRevision);
        Assert.Equal("developer", result.LastChangedAuthor);
        Assert.NotEqual(default, result.LastChangedDate);
    }

    [Fact]
    public void ParseXml_ParsesWcInfo()
    {
        var xml = new XmlDocument();
        xml.LoadXml(@"<info>
            <entry path=""file.cs"">
                <wc-info>
                    <wcroot-abspath>/home/user/project</wcroot-abspath>
                    <schedule>normal</schedule>
                    <depth>infinity</depth>
                </wc-info>
            </entry>
        </info>");

        var result = _parser.ParseXml(xml);

        Assert.Equal("/home/user/project", result.WorkingCopyRoot);
        Assert.Equal("normal", result.Schedule);
        Assert.Equal(SvnDepth.Infinity, result.Depth);
    }

    [Fact]
    public void ParseXml_ParsesConflict()
    {
        var xml = new XmlDocument();
        xml.LoadXml(@"<info>
            <entry path=""file.cs"">
                <conflict>
                    <prev-file>file.cs.r99</prev-file>
                    <working-file>file.cs.mine</working-file>
                    <next-file>file.cs.r100</next-file>
                </conflict>
            </entry>
        </info>");

        var result = _parser.ParseXml(xml);

        Assert.True(result.HasConflict);
        Assert.Equal("file.cs.r99", result.ConflictOld);
        Assert.Equal("file.cs.mine", result.ConflictWorking);
        Assert.Equal("file.cs.r100", result.ConflictNew);
    }

    [Fact]
    public void ParseXml_ParsesLock()
    {
        var xml = new XmlDocument();
        xml.LoadXml(@"<info>
            <entry path=""file.cs"">
                <lock>
                    <owner>developer</owner>
                    <token>opaquelocktoken:abc123</token>
                    <comment>Working on this file</comment>
                    <created>2024-01-15T10:30:00.000000Z</created>
                </lock>
            </entry>
        </info>");

        var result = _parser.ParseXml(xml);

        Assert.True(result.IsLocked);
        Assert.Equal("developer", result.LockOwner);
        Assert.Equal("opaquelocktoken:abc123", result.LockToken);
        Assert.Equal("Working on this file", result.LockComment);
    }

    [Fact]
    public void ParseXml_ReturnsEmptyInfo_WhenNoEntry()
    {
        var xml = new XmlDocument();
        xml.LoadXml("<info></info>");

        var result = _parser.ParseXml(xml);

        Assert.NotNull(result);
        Assert.Equal(string.Empty, result.Path);
    }

    [Theory]
    [InlineData("infinity", SvnDepth.Infinity)]
    [InlineData("empty", SvnDepth.Empty)]
    [InlineData("files", SvnDepth.Files)]
    [InlineData("immediates", SvnDepth.Immediates)]
    [InlineData("exclude", SvnDepth.Exclude)]
    [InlineData("unknown", SvnDepth.Unknown)]
    [InlineData("", SvnDepth.Unknown)]
    public void Parse_ParsesDepthCorrectly(string depthStr, SvnDepth expected)
    {
        var output = $"Depth: {depthStr}";

        var result = _parser.Parse(output);

        Assert.Equal(expected, result.Depth);
    }
}
