using System.Xml;
using Svns.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using System.Text;

namespace Svns.Services.Svn.Parsers;

/// <summary>
/// Parser for SVN status command output
/// </summary>
public class SvnStatusParser : ISvnOutputParser<IList<SvnStatus>>
{
    /// <summary>
    /// Parses text-based status output (non-XML format)
    /// </summary>
    public IList<SvnStatus> Parse(string output)
    {
        var statuses = new List<SvnStatus>();

        if (string.IsNullOrWhiteSpace(output))
            return statuses;

        var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            if (line.Trim().Length < 8 || line.Trim().StartsWith("Status against"))
                continue;

            var status = ParseStatusLine(line);
            if (status != null)
            {
                statuses.Add(status);
            }
        }

        return statuses;
    }

    /// <summary>
    /// Parses XML-based status output
    /// </summary>
    public IList<SvnStatus> ParseXml(XmlDocument xmlDocument)
    {
        var statuses = new List<SvnStatus>();

        try
        {
            var entryNodes = xmlDocument.SelectNodes("//entry");

            if (entryNodes == null)
                return statuses;

            foreach (XmlNode? entryNode in entryNodes)
            {
                if (entryNode == null)
                    continue;

                var status = ParseStatusEntry(entryNode);
                if (status != null)
                {
                    statuses.Add(status);
                }
            }
        }
        catch
        {
            // Return empty list on parse error
        }

        return statuses;
    }

    /// <summary>
    /// Parses a single line of status output
    /// </summary>
    private SvnStatus? ParseStatusLine(string line)
    {
        try
        {
            var trimmed = line.Trim();

            // Extract the status characters
            if (trimmed.Length < 8)
                return null;

            var workingCopyStatus = ParseStatusChar(trimmed[0]);
            var propertyStatus = ParseStatusChar(trimmed[1]);
            var workingCopyRevision = ParseRevision(trimmed, 2, 7);
            var path = trimmed.Substring(8).Trim();

            // Handle paths with spaces
            path = path.TrimStart(' ', '+', '*', 'S', 'B', 'C', 'E', 'O');

            return new SvnStatus
            {
                Path = path,
                WorkingCopyStatus = workingCopyStatus,
                PropertyStatus = propertyStatus,
                WorkingCopyRevision = workingCopyRevision,
                NodeType = DetermineNodeType(path),
                Depth = SvnDepth.Unknown
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parses a single status entry from XML
    /// </summary>
    private SvnStatus? ParseStatusEntry(XmlNode entryNode)
    {
        try
        {
            var pathAttr = entryNode.Attributes?["path"];
            if (pathAttr == null)
                return null;

            var status = new SvnStatus
            {
                Path = pathAttr.Value
            };

            // Parse working copy status
            var wcStatusNode = entryNode.SelectSingleNode("wc-status");
            if (wcStatusNode != null)
            {
                var itemAttr = wcStatusNode.Attributes?["item"];
                if (itemAttr != null)
                {
                    status.WorkingCopyStatus = ParseStatusString(itemAttr.Value);
                }

                var revisionAttr = wcStatusNode.Attributes?["revision"];
                if (revisionAttr != null && long.TryParse(revisionAttr.Value, out var rev))
                {
                    status.WorkingCopyRevision = rev;
                }

                var propsAttr = wcStatusNode.Attributes?["props"];
                if (propsAttr != null)
                {
                    status.PropertyStatus = ParseStatusString(propsAttr.Value);
                }
            }

            // Parse repository status
            var reposStatusNode = entryNode.SelectSingleNode("repos-status");
            if (reposStatusNode != null)
            {
                var itemAttr = reposStatusNode.Attributes?["item"];
                if (itemAttr != null)
                {
                    status.RepositoryStatus = ParseStatusString(itemAttr.Value);
                }
            }

            // Parse commit info
            var commitNode = entryNode.SelectSingleNode("commit");
            if (commitNode != null)
            {
                var revisionAttr = commitNode.Attributes?["revision"];
                if (revisionAttr != null && long.TryParse(revisionAttr.Value, out var rev))
                {
                    status.LastChangedRevision = rev;
                }

                var authorNode = commitNode.SelectSingleNode("author");
                if (authorNode != null)
                {
                    status.LastChangedAuthor = authorNode.InnerText;
                }
            }

            // Parse lock info
            var lockNode = entryNode.SelectSingleNode("lock");
            if (lockNode != null)
            {
                status.IsLocked = true;
            }

            // Parse conflict info
            var conflictNode = entryNode.SelectSingleNode("conflict");
            if (conflictNode != null)
            {
                status.HasConflict = true;
            }

            // Parse tree conflict
            var treeConflictNode = entryNode.SelectSingleNode("tree-conflict");
            if (treeConflictNode != null)
            {
                status.TreeConflict = treeConflictNode.InnerText;
            }

            status.NodeType = DetermineNodeType(status.Path);

            return status;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parses a status character into a SvnStatusType
    /// </summary>
    private SvnStatusType ParseStatusChar(char c)
    {
        return c switch
        {
            ' ' => SvnStatusType.None,
            'A' => SvnStatusType.Added,
            'C' => SvnStatusType.Conflicted,
            'D' => SvnStatusType.Deleted,
            'I' => SvnStatusType.Ignored,
            'M' => SvnStatusType.Modified,
            'R' => SvnStatusType.Replaced,
            'X' => SvnStatusType.External,
            '?' => SvnStatusType.Unversioned,
            '!' => SvnStatusType.Missing,
            '~' => SvnStatusType.Obstructed,
            'L' => SvnStatusType.Incomplete,
            _ => SvnStatusType.None
        };
    }

    /// <summary>
    /// Parses a status string into a SvnStatusType
    /// </summary>
    private SvnStatusType ParseStatusString(string status)
    {
        return status?.ToLowerInvariant() switch
        {
            "added" => SvnStatusType.Added,
            "conflicted" => SvnStatusType.Conflicted,
            "deleted" => SvnStatusType.Deleted,
            "ignored" => SvnStatusType.Ignored,
            "modified" => SvnStatusType.Modified,
            "replaced" => SvnStatusType.Replaced,
            "external" => SvnStatusType.External,
            "unversioned" => SvnStatusType.Unversioned,
            "missing" => SvnStatusType.Missing,
            "obstructed" => SvnStatusType.Obstructed,
            "incomplete" => SvnStatusType.Incomplete,
            "none" or "normal" => SvnStatusType.None,
            _ => SvnStatusType.None
        };
    }

    /// <summary>
    /// Parses a revision number from a substring
    /// </summary>
    private long? ParseRevision(string str, int startIndex, int length)
    {
        try
        {
            var substr = str.Substring(startIndex, length).Trim();
            if (string.IsNullOrEmpty(substr) || substr == "-")
                return null;

            if (long.TryParse(substr, out var revision))
                return revision;
        }
        catch
        {
            // Ignore parse errors
        }

        return null;
    }

    /// <summary>
    /// Determines if a path is a file or directory based on the path string
    /// </summary>
    private SvnNodeType DetermineNodeType(string path)
    {
        // This is a simple heuristic - in practice, we'd check the actual filesystem
        // or look at the XML "kind" attribute if available
        if (path.EndsWith("/") || path.EndsWith("\\"))
            return SvnNodeType.Directory;

        // Check if it exists
        if (Directory.Exists(path))
            return SvnNodeType.Directory;

        if (File.Exists(path))
            return SvnNodeType.File;

        // Default to file for new items that don't exist yet
        return SvnNodeType.File;
    }
}
