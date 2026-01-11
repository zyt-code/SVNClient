using System;
using System.Xml;
using Svns.Models;

namespace Svns.Services.Svn.Parsers;

/// <summary>
/// Parser for SVN info command output
/// </summary>
public class SvnInfoParser : ISvnOutputParser<SvnInfo>
{
    /// <summary>
    /// Parses text-based info output (non-XML format)
    /// </summary>
    public SvnInfo Parse(string output)
    {
        var info = new SvnInfo();

        if (string.IsNullOrWhiteSpace(output))
            return info;

        var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var colonIndex = line.IndexOf(':');
            if (colonIndex < 0)
                continue;

            var key = line.Substring(0, colonIndex).Trim();
            var value = line.Substring(colonIndex + 1).Trim();

            switch (key)
            {
                case "Path":
                    info.Path = value;
                    break;
                case "Working Copy Root Path":
                    info.WorkingCopyRoot = value;
                    break;
                case "URL":
                    info.Url = value;
                    break;
                case "Relative URL":
                    info.RelativePath = value;
                    break;
                case "Repository Root":
                    info.RepositoryRootUrl = value;
                    break;
                case "Repository UUID":
                    info.RepositoryUuid = value;
                    break;
                case "Revision":
                    if (long.TryParse(value, out var revision))
                        info.Revision = revision;
                    break;
                case "Node Kind":
                    info.NodeKind = value;
                    break;
                case "Schedule":
                    info.Schedule = value;
                    break;
                case "Last Changed Author":
                    info.LastChangedAuthor = value;
                    break;
                case "Last Changed Rev":
                    if (long.TryParse(value, out var lastChangedRev))
                        info.LastChangedRevision = lastChangedRev;
                    break;
                case "Last Changed Date":
                    info.LastChangedDate = ParseSvnDate(value);
                    break;
                case "Depth":
                    info.Depth = ParseDepth(value);
                    break;
            }
        }

        return info;
    }

    /// <summary>
    /// Parses XML-based info output
    /// </summary>
    public SvnInfo ParseXml(XmlDocument xmlDocument)
    {
        var info = new SvnInfo();

        try
        {
            var entryNode = xmlDocument.SelectSingleNode("//entry");
            if (entryNode == null)
                return info;

            // Parse entry attributes
            var pathAttr = entryNode.Attributes?["path"];
            var revisionAttr = entryNode.Attributes?["revision"];
            var kindAttr = entryNode.Attributes?["kind"];

            if (pathAttr != null)
                info.Path = pathAttr.Value;
            if (revisionAttr != null && long.TryParse(revisionAttr.Value, out var rev))
                info.Revision = rev;
            if (kindAttr != null)
                info.NodeKind = kindAttr.Value;

            // Parse URL
            var urlNode = entryNode.SelectSingleNode("url");
            if (urlNode != null)
                info.Url = urlNode.InnerText;

            // Parse relative URL
            var relativeUrlNode = entryNode.SelectSingleNode("relative-url");
            if (relativeUrlNode != null)
                info.RelativePath = relativeUrlNode.InnerText;

            // Parse repository
            var repositoryNode = entryNode.SelectSingleNode("repository");
            if (repositoryNode != null)
            {
                var root_node = repositoryNode.SelectSingleNode("root");
                if (root_node != null)
                    info.RepositoryRootUrl = root_node.InnerText;

                var uuidNode = repositoryNode.SelectSingleNode("uuid");
                if (uuidNode != null)
                    info.RepositoryUuid = uuidNode.InnerText;
            }

            // Parse WC info
            var wcInfoNode = entryNode.SelectSingleNode("wc-info");
            if (wcInfoNode != null)
            {
                var wcRootNode = wcInfoNode.SelectSingleNode("wcroot-abspath");
                if (wcRootNode != null)
                    info.WorkingCopyRoot = wcRootNode.InnerText;

                var scheduleNode = wcInfoNode.SelectSingleNode("schedule");
                if (scheduleNode != null)
                    info.Schedule = scheduleNode.InnerText;

                var depthNode = wcInfoNode.SelectSingleNode("depth");
                if (depthNode != null)
                    info.Depth = ParseDepth(depthNode.InnerText);
            }

            // Parse commit
            var commitNode = entryNode.SelectSingleNode("commit");
            if (commitNode != null)
            {
                var commitRevAttr = commitNode.Attributes?["revision"];
                if (commitRevAttr != null && long.TryParse(commitRevAttr.Value, out var commitRev))
                    info.LastChangedRevision = commitRev;

                var authorNode = commitNode.SelectSingleNode("author");
                if (authorNode != null)
                    info.LastChangedAuthor = authorNode.InnerText;

                var dateNode = commitNode.SelectSingleNode("date");
                if (dateNode != null)
                    info.LastChangedDate = XmlConvert.ToDateTime(dateNode.InnerText, XmlDateTimeSerializationMode.Utc);
            }

            // Parse conflicts
            var conflictNode = entryNode.SelectSingleNode("conflict");
            if (conflictNode != null)
            {
                info.HasConflict = true;

                var prevConflictNode = conflictNode.SelectSingleNode("prev-file");
                if (prevConflictNode != null)
                    info.ConflictOld = prevConflictNode.InnerText;

                var workingConflictNode = conflictNode.SelectSingleNode("working-file");
                if (workingConflictNode != null)
                    info.ConflictWorking = workingConflictNode.InnerText;

                var nextConflictNode = conflictNode.SelectSingleNode("next-file");
                if (nextConflictNode != null)
                    info.ConflictNew = nextConflictNode.InnerText;
            }

            // Parse lock
            var lockNode = entryNode.SelectSingleNode("lock");
            if (lockNode != null)
            {
                info.IsLocked = true;

                var ownerNode = lockNode.SelectSingleNode("owner");
                if (ownerNode != null)
                    info.LockOwner = ownerNode.InnerText;

                var tokenNode = lockNode.SelectSingleNode("token");
                if (tokenNode != null)
                    info.LockToken = tokenNode.InnerText;

                var commentNode = lockNode.SelectSingleNode("comment");
                if (commentNode != null)
                    info.LockComment = commentNode.InnerText;

                var createdNode = lockNode.SelectSingleNode("created");
                if (createdNode != null)
                    info.LockCreationDate = XmlConvert.ToDateTime(createdNode.InnerText, XmlDateTimeSerializationMode.Utc);
            }
        }
        catch
        {
            // Return empty info on parse error
        }

        return info;
    }

    /// <summary>
    /// Parses an SVN date string
    /// </summary>
    private DateTime ParseSvnDate(string dateStr)
    {
        try
        {
            // SVN date format: 2024-01-10 12:34:56 +0000 (Thu, 10 Jan 2024)
            // Or: 2024-01-10T12:34:56.123456Z
            var dateStrClean = dateStr.Split('(')[0].Trim();

            if (DateTime.TryParse(dateStrClean, out var date))
                return date;

            if (DateTime.TryParse(dateStrClean, null, System.Globalization.DateTimeStyles.AssumeUniversal, out var utcDate))
                return utcDate;
        }
        catch
        {
            // Ignore
        }

        return DateTime.MinValue;
    }

    /// <summary>
    /// Parses a depth string
    /// </summary>
    private SvnDepth ParseDepth(string depthStr)
    {
        return depthStr?.ToLowerInvariant() switch
        {
            "exclude" => SvnDepth.Exclude,
            "empty" => SvnDepth.Empty,
            "files" => SvnDepth.Files,
            "immediates" => SvnDepth.Immediates,
            "infinity" => SvnDepth.Infinity,
            _ => SvnDepth.Unknown
        };
    }
}
