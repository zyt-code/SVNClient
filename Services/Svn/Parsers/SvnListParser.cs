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
/// Parser for SVN list command output
/// </summary>
public class SvnListParser : ISvnOutputParser<IList<SvnListItem>>
{
    /// <summary>
    /// Parses text-based list output (non-XML format)
    /// </summary>
    public IList<SvnListItem> Parse(string output)
    {
        var items = new List<SvnListItem>();

        if (string.IsNullOrWhiteSpace(output))
            return items;

        var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var item = ParseListLine(line);
            if (item != null)
            {
                items.Add(item);
            }
        }

        return items;
    }

    /// <summary>
    /// Parses XML-based list output
    /// </summary>
    public IList<SvnListItem> ParseXml(XmlDocument xmlDocument)
    {
        var items = new List<SvnListItem>();

        try
        {
            var entryNodes = xmlDocument.SelectNodes("//entry");

            if (entryNodes == null)
                return items;

            foreach (XmlNode? entryNode in entryNodes)
            {
                if (entryNode == null)
                    continue;

                var item = ParseListEntry(entryNode);
                if (item != null)
                {
                    items.Add(item);
                }
            }
        }
        catch
        {
            // Return empty list on parse error
        }

        return items;
    }

    /// <summary>
    /// Parses a single line of list output
    /// </summary>
    private SvnListItem? ParseListLine(string line)
    {
        try
        {
            // Format (verbose): 12345 author            Jan 10 12:34 filename/
            // Format (non-verbose): filename/
            var trimmed = line.Trim();

            // If it starts with a number, it's verbose format
            if (char.IsDigit(trimmed[0]))
            {
                return ParseVerboseListLine(trimmed);
            }
            else
            {
                return ParseSimpleListLine(trimmed);
            }
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parses a verbose list line
    /// </summary>
    private SvnListItem? ParseVerboseListLine(string line)
    {
        try
        {
            // Format: 12345 author            Jan 10 12:34 filename/
            // Break it down by parts
            var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 5)
                return null;

            var revision = long.Parse(parts[0], CultureInfo.InvariantCulture);
            var author = parts[1];
            var dateStr = $"{parts[2]} {parts[3]} {parts[4]}";
            var date = ParseSvnDate(dateStr);

            // The rest is the filename
            var nameStart = line.IndexOf(dateStr) + dateStr.Length + 1;
            var name = line.Substring(nameStart).Trim();

            return new SvnListItem
            {
                Name = name,
                Revision = revision,
                Author = author,
                Date = date,
                Kind = name.EndsWith("/") ? SvnNodeType.Directory : SvnNodeType.File,
                Size = 0 // Not available in verbose text format
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parses a simple list line
    /// </summary>
    private SvnListItem? ParseSimpleListLine(string line)
    {
        try
        {
            return new SvnListItem
            {
                Name = line.Trim(),
                Kind = line.Trim().EndsWith("/") ? SvnNodeType.Directory : SvnNodeType.File
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parses a list entry from XML
    /// </summary>
    private SvnListItem? ParseListEntry(XmlNode entryNode)
    {
        try
        {
            var kindAttr = entryNode.Attributes?["kind"];
            var nameNode = entryNode.SelectSingleNode("name");
            var sizeNode = entryNode.SelectSingleNode("size");
            var commitNode = entryNode.SelectSingleNode("commit");

            if (nameNode == null)
                return null;

            var item = new SvnListItem
            {
                Name = nameNode.InnerText,
                Kind = kindAttr?.Value == "dir" ? SvnNodeType.Directory : SvnNodeType.File
            };

            if (sizeNode != null && int.TryParse(sizeNode.InnerText, out var size))
            {
                item.Size = size;
            }

            if (commitNode != null)
            {
                var revisionAttr = commitNode.Attributes?["revision"];
                if (revisionAttr != null && long.TryParse(revisionAttr.Value, out var revision))
                {
                    item.Revision = revision;
                }

                var authorNode = commitNode.SelectSingleNode("author");
                if (authorNode != null)
                {
                    item.Author = authorNode.InnerText;
                }

                var dateNode = commitNode.SelectSingleNode("date");
                if (dateNode != null)
                {
                    item.Date = XmlConvert.ToDateTime(dateNode.InnerText, System.Xml.XmlDateTimeSerializationMode.Utc);
                }
            }

            return item;
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
            // Try standard formats
            if (DateTime.TryParse(dateStr, out var date))
                return date;

            // Try parsing custom SVN format
            // Format: "Jan 10 12:34" or "Jan 10  2024"
            var parts = dateStr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 3)
            {
                var monthStr = parts[0];
                var day = int.Parse(parts[1], CultureInfo.InvariantCulture);
                var timeOrYear = parts[2];

                int month = monthStr switch
                {
                    "Jan" => 1,
                    "Feb" => 2,
                    "Mar" => 3,
                    "Apr" => 4,
                    "May" => 5,
                    "Jun" => 6,
                    "Jul" => 7,
                    "Aug" => 8,
                    "Sep" => 9,
                    "Oct" => 10,
                    "Nov" => 11,
                    "Dec" => 12,
                    _ => 1
                };

                int year = DateTime.Now.Year;

                // If the third part is a time, it's this year
                if (timeOrYear.Contains(":"))
                {
                    var timeParts = timeOrYear.Split(':');
                    var hour = int.Parse(timeParts[0], CultureInfo.InvariantCulture);
                    var minute = int.Parse(timeParts[1], CultureInfo.InvariantCulture);
                    return new DateTime(year, month, day, hour, minute, 0);
                }
                else
                {
                    // It's a year
                    year = int.Parse(timeOrYear, CultureInfo.InvariantCulture);
                    return new DateTime(year, month, day);
                }
            }
        }
        catch
        {
            // Ignore
        }

        return DateTime.MinValue;
    }
}

/// <summary>
/// Represents an item in an SVN list
/// </summary>
public class SvnListItem
{
    /// <summary>
    /// The name of the item
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The kind (file or directory)
    /// </summary>
    public SvnNodeType Kind { get; set; }

    /// <summary>
    /// The revision (if available)
    /// </summary>
    public long Revision { get; set; }

    /// <summary>
    /// The author of the last change (if available)
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// The date of the last change (if available)
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// The size in bytes (for files, if available)
    /// </summary>
    public int Size { get; set; }

    /// <summary>
    /// Whether this item is a directory
    /// </summary>
    public bool IsDirectory => Kind == SvnNodeType.Directory;

    /// <summary>
    /// Whether this item is a file
    /// </summary>
    public bool IsFile => Kind == SvnNodeType.File;

    /// <summary>
    /// Gets the name without trailing slash
    /// </summary>
    public string DisplayName => Name.TrimEnd('/', '\\');
}
