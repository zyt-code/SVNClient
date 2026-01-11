using System.Xml;
using Svns.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;

namespace Svns.Services.Svn.Parsers;

/// <summary>
/// Parser for SVN log command output
/// </summary>
public class SvnLogParser : ISvnOutputParser<IList<SvnLogEntry>>
{
    /// <summary>
    /// Parses text-based log output (non-XML format)
    /// </summary>
    public IList<SvnLogEntry> Parse(string output)
    {
        var logEntries = new List<SvnLogEntry>();

        if (string.IsNullOrWhiteSpace(output))
            return logEntries;

        var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        SvnLogEntry? currentEntry = null;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            // Check for separator line
            if (trimmed.StartsWith("----"))
            {
                if (currentEntry != null)
                {
                    logEntries.Add(currentEntry);
                }
                currentEntry = null;
                continue;
            }

            // Check for revision line: r123 | author | date | lines
            if (currentEntry == null && trimmed.StartsWith("r"))
            {
                currentEntry = ParseRevisionLine(trimmed);
                continue;
            }

            // Check for changed path line
            if (currentEntry != null && trimmed.StartsWith("Changed paths:"))
            {
                continue;
            }

            // Check for actual changed path
            if (currentEntry != null && (trimmed.StartsWith("A /") ||
                                        trimmed.StartsWith("D /") ||
                                        trimmed.StartsWith("M /") ||
                                        trimmed.StartsWith("R /")))
            {
                var changedPath = ParseChangedPathLine(trimmed);
                if (changedPath != null)
                {
                    currentEntry.ChangedPaths.Add(changedPath);
                }
                continue;
            }

            // Message content
            if (currentEntry != null)
            {
                if (!string.IsNullOrEmpty(currentEntry.Message))
                {
                    currentEntry.Message += "\n";
                }
                currentEntry.Message += trimmed;
            }
        }

        // Add the last entry
        if (currentEntry != null)
        {
            logEntries.Add(currentEntry);
        }

        return logEntries;
    }

    /// <summary>
    /// Parses XML-based log output
    /// </summary>
    public IList<SvnLogEntry> ParseXml(XmlDocument xmlDocument)
    {
        var logEntries = new List<SvnLogEntry>();

        try
        {
            var logEntryNodes = xmlDocument.SelectNodes("//logentry");

            if (logEntryNodes == null)
                return logEntries;

            foreach (XmlNode? logEntryNode in logEntryNodes)
            {
                if (logEntryNode == null)
                    continue;

                var entry = ParseLogEntry(logEntryNode);
                if (entry != null)
                {
                    logEntries.Add(entry);
                }
            }
        }
        catch
        {
            // Return empty list on parse error
        }

        return logEntries;
    }

    /// <summary>
    /// Parses a single revision line
    /// </summary>
    private SvnLogEntry? ParseRevisionLine(string line)
    {
        try
        {
            // Format: r123 | author | 2024-01-10 12:34:56 +0000 (Thu, 10 Jan 2024) | N lines
            var parts = line.Split('|');
            if (parts.Length < 3)
                return null;

            var revisionPart = parts[0].Trim();
            var authorPart = parts[1].Trim();
            var datePart = parts[2].Trim();

            if (!revisionPart.StartsWith("r"))
                return null;

            if (!long.TryParse(revisionPart.Substring(1), out var revision))
                return null;

            // Parse date (simplified - in real implementation, use proper date parsing)
            var date = ParseSvnDate(datePart);

            return new SvnLogEntry
            {
                Revision = revision,
                Author = authorPart,
                Date = date,
                Message = string.Empty
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parses a changed path line
    /// </summary>
    private SvnChangedPath? ParseChangedPathLine(string line)
    {
        try
        {
            // Format: A /trunk/file.txt (from /trunk/oldfile.txt:r100)
            //         M /trunk/other.txt
            var trimmed = line.Trim();
            if (trimmed.Length < 3)
                return null;

            var actionChar = trimmed[0];
            var action = actionChar switch
            {
                'A' => SvnPathAction.Added,
                'D' => SvnPathAction.Deleted,
                'M' => SvnPathAction.Modified,
                'R' => SvnPathAction.Replaced,
                _ => SvnPathAction.Modified
            };

            var pathAndRest = trimmed.Substring(2);
            var path = pathAndRest;

            // Check for copy info
            string? copyFromPath = null;
            long? copyFromRevision = null;

            var copyIndex = pathAndRest.IndexOf(" (from ");
            if (copyIndex >= 0)
            {
                path = pathAndRest.Substring(0, copyIndex);
                var copyPart = pathAndRest.Substring(copyIndex + 7); // " (from "

                var colonIndex = copyPart.LastIndexOf(":r");
                if (colonIndex >= 0)
                {
                    copyFromPath = copyPart.Substring(0, colonIndex);
                    var revisionStr = copyPart.Substring(colonIndex + 2).TrimEnd(')');
                    if (long.TryParse(revisionStr, out var rev))
                    {
                        copyFromRevision = rev;
                    }
                }
            }

            return new SvnChangedPath
            {
                Action = action,
                Path = path,
                CopyFromPath = copyFromPath,
                CopyFromRevision = copyFromRevision
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parses a log entry from XML
    /// </summary>
    private SvnLogEntry? ParseLogEntry(XmlNode logEntryNode)
    {
        try
        {
            var revisionAttr = logEntryNode.Attributes?["revision"];
            if (revisionAttr == null || !long.TryParse(revisionAttr.Value, out var revision))
                return null;

            var entry = new SvnLogEntry
            {
                Revision = revision
            };

            // Parse author
            var authorNode = logEntryNode.SelectSingleNode("author");
            if (authorNode != null)
            {
                entry.Author = authorNode.InnerText;
            }

            // Parse date
            var dateNode = logEntryNode.SelectSingleNode("date");
            if (dateNode != null)
            {
                entry.Date = XmlConvert.ToDateTime(dateNode.InnerText, XmlDateTimeSerializationMode.Utc);
            }

            // Parse message
            var msgNode = logEntryNode.SelectSingleNode("msg");
            if (msgNode != null)
            {
                entry.Message = msgNode.InnerText;
            }

            // Parse changed paths
            var pathsNode = logEntryNode.SelectSingleNode("paths");
            if (pathsNode != null)
            {
                var pathNodes = pathsNode.SelectNodes("path");
                if (pathNodes != null)
                {
                    foreach (XmlNode? pathNode in pathNodes)
                    {
                        if (pathNode == null)
                            continue;

                        var changedPath = ParseChangedPath(pathNode);
                        if (changedPath != null)
                        {
                            entry.ChangedPaths.Add(changedPath);
                        }
                    }
                }
            }

            return entry;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parses a changed path from XML
    /// </summary>
    private SvnChangedPath? ParseChangedPath(XmlNode pathNode)
    {
        try
        {
            var actionAttr = pathNode.Attributes?["action"];
            if (actionAttr == null)
                return null;

            var action = actionAttr.Value[0] switch
            {
                'A' => SvnPathAction.Added,
                'D' => SvnPathAction.Deleted,
                'M' => SvnPathAction.Modified,
                'R' => SvnPathAction.Replaced,
                _ => SvnPathAction.Modified
            };

            var changedPath = new SvnChangedPath
            {
                Action = action,
                Path = pathNode.InnerText
            };

            // Parse copyfrom info
            var copyFromRevAttr = pathNode.Attributes?["copyfrom-rev"];
            var copyFromPathAttr = pathNode.Attributes?["copyfrom-path"];

            if (copyFromRevAttr != null && copyFromPathAttr != null)
            {
                if (long.TryParse(copyFromRevAttr.Value, out var rev))
                {
                    changedPath.CopyFromRevision = rev;
                    changedPath.CopyFromPath = copyFromPathAttr.Value;
                }
            }

            return changedPath;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parses an SVN date string
    /// </summary>
    private DateTime ParseSvnDate(string dateStr)
    {
        try
        {
            // SVN date format: 2024-01-10 12:34:56 +0000 (Thu, 10 Jan 2024)
            // We'll extract the ISO-like part and parse it
            var spaceIndex = dateStr.IndexOf(' ');
            if (spaceIndex >= 0)
            {
                var datePart = dateStr.Substring(0, spaceIndex);
                var timePart = dateStr.Substring(spaceIndex + 1, 8);
                var dateTimeStr = $"{datePart}T{timePart}";

                if (DateTime.TryParse(dateTimeStr, out var date))
                    return date;
            }
        }
        catch
        {
            // Ignore
        }

        return DateTime.UtcNow;
    }
}
