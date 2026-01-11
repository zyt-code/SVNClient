using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Svns.Models;
using System.IO;
using System.Text;

namespace Svns.Services.Svn.Parsers;

/// <summary>
/// Parser for SVN diff command output
/// </summary>
public class SvnDiffParser : ISvnOutputParser<SvnDiffResult>
{
    private static readonly Regex HunkHeaderRegex = new(@"^@@\s+-(\d+)(?:,(\d+))?\s+\+(\d+)(?:,(\d+))?\s+@@");
    private static readonly Regex BinaryFileRegex = new(@"Binary\s+files?\s+(.+?)\s+and\s+(.+?)\s+differ", RegexOptions.IgnoreCase);
    private static readonly Regex HeaderRegex = new(@"^(Index|diff|---|\+\+\+)\s+.+");

    /// <summary>
    /// Parses diff output
    /// </summary>
    public SvnDiffResult Parse(string output)
    {
        var result = new SvnDiffResult();

        if (string.IsNullOrWhiteSpace(output))
            return result;

        // Check for binary diff
        if (output.Contains("Binary files") || output.Contains("Binary file"))
        {
            result.IsBinary = true;
            result.BinaryMessage = output.Trim();
            return result;
        }

        var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
        ParseDiffLines(lines, result);

        return result;
    }

    /// <summary>
    /// Parses XML diff output (not commonly used by SVN)
    /// </summary>
    public SvnDiffResult ParseXml(System.Xml.XmlDocument xmlDocument)
    {
        // SVN doesn't typically output XML for diff, so we'll return an empty result
        return new SvnDiffResult
        {
            IsBinary = false,
            Lines = new List<SvnDiffLine>()
        };
    }

    /// <summary>
    /// Parses diff lines into a diff result
    /// </summary>
    private void ParseDiffLines(string[] lines, SvnDiffResult result)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            // Check for binary file indicator
            var binaryMatch = BinaryFileRegex.Match(line);
            if (binaryMatch.Success)
            {
                result.IsBinary = true;
                result.BinaryMessage = line.Trim();
                return;
            }

            // Check for header lines
            if (IsHeaderLine(line))
            {
                result.Lines.Add(new SvnDiffLine
                {
                    Type = SvnDiffLineType.Header,
                    Content = line
                });
                continue;
            }

            // Check for hunk header
            var hunkMatch = HunkHeaderRegex.Match(line);
            if (hunkMatch.Success)
            {
                result.Lines.Add(new SvnDiffLine
                {
                    Type = SvnDiffLineType.HunkHeader,
                    Content = line,
                    OriginalLineNumber = int.TryParse(hunkMatch.Groups[1].Value, out var origLine) ? origLine : null,
                    ModifiedLineNumber = int.TryParse(hunkMatch.Groups[3].Value, out var modLine) ? modLine : null
                });
                continue;
            }

            // Parse diff content lines
            if (line.Length > 0)
            {
                var diffLine = ParseDiffLine(line);
                result.Lines.Add(diffLine);
            }
        }
    }

    /// <summary>
    /// Parses a single diff line
    /// </summary>
    private SvnDiffLine ParseDiffLine(string line)
    {
        char firstChar = line[0];

        var type = firstChar switch
        {
            '+' => SvnDiffLineType.Addition,
            '-' => SvnDiffLineType.Deletion,
            ' ' => SvnDiffLineType.Context,
            '\\' => SvnDiffLineType.Context, // Lines starting with "\ " are context (e.g., "\ No newline at end of file")
            _ => SvnDiffLineType.Context
        };

        return new SvnDiffLine
        {
            Type = type,
            Content = line
        };
    }

    /// <summary>
    /// Checks if a line is a header line
    /// </summary>
    private bool IsHeaderLine(string line)
    {
        return HeaderRegex.IsMatch(line);
    }

    /// <summary>
    /// Parses multiple diffs from a combined diff output
    /// </summary>
    public IList<SvnDiffResult> ParseMultiple(string output)
    {
        var results = new List<SvnDiffResult>();

        if (string.IsNullOrWhiteSpace(output))
            return results;

        // Split by "Index:" line which indicates a new file
        var sections = output.Split(new[] { "\nIndex: ", "\r\nIndex: ", "\rIndex: " }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var section in sections)
        {
            var lines = section.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
            if (lines.Length > 0)
            {
                var diff = new SvnDiffResult();
                ParseDiffLines(lines, diff);
                results.Add(diff);
            }
        }

        return results;
    }

    /// <summary>
    /// Creates a unified diff from two strings
    /// </summary>
    public SvnDiffResult CreateUnifiedDiff(string originalContent, string modifiedContent, string originalPath, string modifiedPath)
    {
        var result = new SvnDiffResult
        {
            OriginalPath = originalPath,
            ModifiedPath = modifiedPath,
            DiffType = "unified"
        };

        var originalLines = originalContent.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
        var modifiedLines = modifiedContent.Split(new[] { '\r', '\n' }, StringSplitOptions.None);

        // Simple line-by-line comparison (not a full diff algorithm)
        int maxLines = Math.Max(originalLines.Length, modifiedLines.Length);

        for (int i = 0; i < maxLines; i++)
        {
            bool originalHasLine = i < originalLines.Length;
            bool modifiedHasLine = i < modifiedLines.Length;

            if (!originalHasLine && modifiedHasLine)
            {
                // Addition
                result.Lines.Add(new SvnDiffLine
                {
                    Type = SvnDiffLineType.Addition,
                    Content = "+" + modifiedLines[i],
                    ModifiedLineNumber = i + 1
                });
            }
            else if (originalHasLine && !modifiedHasLine)
            {
                // Deletion
                result.Lines.Add(new SvnDiffLine
                {
                    Type = SvnDiffLineType.Deletion,
                    Content = "-" + originalLines[i],
                    OriginalLineNumber = i + 1
                });
            }
            else if (originalLines[i] != modifiedLines[i])
            {
                // Modification (both deletion and addition)
                result.Lines.Add(new SvnDiffLine
                {
                    Type = SvnDiffLineType.Deletion,
                    Content = "-" + originalLines[i],
                    OriginalLineNumber = i + 1
                });
                result.Lines.Add(new SvnDiffLine
                {
                    Type = SvnDiffLineType.Addition,
                    Content = "+" + modifiedLines[i],
                    ModifiedLineNumber = i + 1
                });
            }
            else
            {
                // Context
                result.Lines.Add(new SvnDiffLine
                {
                    Type = SvnDiffLineType.Context,
                    Content = " " + originalLines[i],
                    OriginalLineNumber = i + 1,
                    ModifiedLineNumber = i + 1
                });
            }
        }

        return result;
    }
}
