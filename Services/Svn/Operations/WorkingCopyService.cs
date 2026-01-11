using Svns.Models;
using Svns.Services.Svn.Core;
using Svns.Services.Svn.Parsers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using System.Text;

namespace Svns.Services.Svn.Operations;

/// <summary>
/// Service for managing working copies
/// </summary>
public class WorkingCopyService
{
    private readonly SvnCommandService _commandService;
    private readonly SvnInfoParser _infoParser;
    private readonly SvnStatusParser _statusParser;

    public WorkingCopyService(SvnCommandService commandService)
    {
        _commandService = commandService;
        _infoParser = new SvnInfoParser();
        _statusParser = new SvnStatusParser();
    }

    /// <summary>
    /// Detects if a path is a valid working copy
    /// </summary>
    public async Task<bool> IsValidWorkingCopyAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        try
        {
            // Check for .svn directory
            var svnDir = Path.Combine(path, ".svn");
            if (!Directory.Exists(svnDir))
                return false;

            // Try to get info
            var command = SvnCommand.Info(path);
            var result = await _commandService.ExecuteAsync(command);

            return result.Success;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Finds the working copy root for a given path
    /// </summary>
    public async Task<string?> FindWorkingCopyRootAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        try
        {
            var currentPath = Path.GetFullPath(path);

            while (!string.IsNullOrEmpty(currentPath))
            {
                if (await IsValidWorkingCopyAsync(currentPath))
                {
                    return currentPath;
                }

                // Move up to parent directory
                var parentPath = Path.GetDirectoryName(currentPath);
                if (string.IsNullOrEmpty(parentPath) || parentPath == currentPath)
                {
                    break;
                }

                currentPath = parentPath;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets working copy information
    /// </summary>
    public async Task<WorkingCopyInfo?> GetWorkingCopyInfoAsync(string path)
    {
        try
        {
            // Find the working copy root first
            var rootPath = await FindWorkingCopyRootAsync(path);
            if (string.IsNullOrEmpty(rootPath))
                return null;

            // Get SVN info
            var infoCommand = SvnCommand.Info(rootPath);
            infoCommand.UseXml = true;

            var xmlDoc = await _commandService.ExecuteXmlAsync(infoCommand);
            if (xmlDoc == null)
                return null;

            var svnInfo = _infoParser.ParseXml(xmlDoc);

            // Get status to count changes
            var statusCommand = SvnCommand.Status(rootPath);
            statusCommand.UseXml = true;

            var statusXmlDoc = await _commandService.ExecuteXmlAsync(statusCommand);
            var statuses = statusXmlDoc != null
                ? _statusParser.ParseXml(statusXmlDoc)
                : new List<SvnStatus>();

            // Create working copy info
            var info = new WorkingCopyInfo
            {
                Path = rootPath,
                RepositoryUrl = svnInfo.Url,
                RepositoryRoot = svnInfo.RepositoryRootUrl,
                RepositoryUuid = svnInfo.RepositoryUuid,
                Revision = svnInfo.Revision,
                LastChangedRevision = svnInfo.LastChangedRevision,
                LastChangedAuthor = svnInfo.LastChangedAuthor,
                LastChangedDate = svnInfo.LastChangedDate,
                RelativePath = svnInfo.RelativePath,
                Depth = svnInfo.Depth,
                ModifiedFileCount = statuses.Count(s => s.WorkingCopyStatus == SvnStatusType.Modified),
                AddedFileCount = statuses.Count(s => s.WorkingCopyStatus == SvnStatusType.Added),
                DeletedFileCount = statuses.Count(s => s.WorkingCopyStatus == SvnStatusType.Deleted),
                ConflictedFileCount = statuses.Count(s => s.WorkingCopyStatus == SvnStatusType.Conflicted),
                HasUncommittedChanges = statuses.Any(s => s.HasLocalModifications)
            };

            return info;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the status of all files in a working copy
    /// </summary>
    public async Task<IList<SvnStatus>> GetStatusAsync(string path, bool recursive = true)
    {
        if (string.IsNullOrWhiteSpace(path))
            return new List<SvnStatus>();

        try
        {
            var command = SvnCommand.Status(path, recursive);
            command.UseXml = true;

            var xmlDoc = await _commandService.ExecuteXmlAsync(command);
            if (xmlDoc == null)
                return new List<SvnStatus>();

            return _statusParser.ParseXml(xmlDoc);
        }
        catch
        {
            return new List<SvnStatus>();
        }
    }

    /// <summary>
    /// Gets the status as a hierarchical tree structure
    /// </summary>
    public async Task<ObservableCollection<SvnStatus>> GetStatusTreeAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return new ObservableCollection<SvnStatus>();

        var statuses = await GetStatusAsync(path, recursive: true);
        return BuildTree(statuses, path);
    }

    /// <summary>
    /// Builds a hierarchical tree from a flat list of statuses
    /// </summary>
    private ObservableCollection<SvnStatus> BuildTree(IList<SvnStatus> statuses, string rootPath)
    {
        var result = new ObservableCollection<SvnStatus>();
        var pathToNode = new Dictionary<string, SvnStatus>();

        // Normalize root path
        rootPath = rootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        // Group statuses by their parent directory
        var childrenMap = new Dictionary<string, List<SvnStatus>>();
        foreach (var status in statuses)
        {
            var fullPath = status.Path;
            var parentDir = Path.GetDirectoryName(fullPath);

            if (string.IsNullOrEmpty(parentDir))
            {
                parentDir = rootPath;
            }

            if (!childrenMap.ContainsKey(parentDir))
            {
                childrenMap[parentDir] = new List<SvnStatus>();
            }
            childrenMap[parentDir].Add(status);

            pathToNode[fullPath] = status;
        }

        // Build the tree structure
        foreach (var status in statuses)
        {
            var fullPath = status.Path;

            // Add children to this node if it's a directory
            if (childrenMap.ContainsKey(fullPath))
            {
                foreach (var child in childrenMap[fullPath])
                {
                    // Only add if not already in children
                    if (status.Children.All(c => c.Path != child.Path))
                    {
                        status.Children.Add(child);
                    }
                }
            }
        }

        // Find root-level items (direct children of root path)
        foreach (var status in statuses)
        {
            var parentDir = Path.GetDirectoryName(status.Path);
            var normalizedParent = parentDir?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            // If parent is the root path, this is a root-level item
            if (string.Equals(normalizedParent, rootPath, StringComparison.OrdinalIgnoreCase) ||
                (string.IsNullOrEmpty(parentDir) && string.Equals(status.Path, rootPath, StringComparison.OrdinalIgnoreCase)))
            {
                result.Add(status);
            }
        }

        // If no items found (edge case), return all items as root
        if (result.Count == 0 && statuses.Count > 0)
        {
            foreach (var status in statuses)
            {
                result.Add(status);
            }
        }

        return result;
    }

    /// <summary>
    /// Gets the status of a specific file or directory
    /// </summary>
    public async Task<SvnStatus?> GetFileStatusAsync(string path)
    {
        try
        {
            var statuses = await GetStatusAsync(path, recursive: false);
            return statuses.FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the log history for a working copy
    /// </summary>
    public async Task<IList<SvnLogEntry>> GetLogAsync(
        string path,
        long? startRevision = null,
        long? endRevision = null,
        int limit = 100,
        bool includeChangedPaths = true)
    {
        try
        {
            // Use the static Log method which has correct argument formatting
            var command = SvnCommand.Log(path, limit, includeChangedPaths);
            command.UseXml = true;

            var xmlDoc = await _commandService.ExecuteXmlAsync(command);
            if (xmlDoc == null)
                return new List<SvnLogEntry>();

            var parser = new SvnLogParser();
            return parser.ParseXml(xmlDoc);
        }
        catch (Exception ex)
        {
            // Log the error for debugging
            System.Diagnostics.Debug.WriteLine($"Error getting SVN log: {ex.Message}");
            return new List<SvnLogEntry>();
        }
    }

    /// <summary>
    /// Cleans up a working copy
    /// </summary>
    public async Task<SvnResult> CleanupAsync(
        string path,
        bool removeUnversioned = false,
        bool removeIgnored = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = SvnCommand.Cleanup(path, removeUnversioned, removeIgnored);
            return await _commandService.ExecuteAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            return SvnResult.Fail(ex.Message, -1, ex);
        }
    }

    /// <summary>
    /// Updates a working copy
    /// </summary>
    public async Task<SvnResult> UpdateAsync(
        string path,
        long? revision = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = SvnCommand.Update(path, revision);
            return await _commandService.ExecuteAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            return SvnResult.Fail(ex.Message, -1, ex);
        }
    }

    /// <summary>
    /// Checks if a working copy has uncommitted changes
    /// </summary>
    public async Task<bool> HasUncommittedChangesAsync(string path)
    {
        try
        {
            var info = await GetWorkingCopyInfoAsync(path);
            return info?.HasUncommittedChanges ?? false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets all conflicted files in a working copy
    /// </summary>
    public async Task<IList<SvnStatus>> GetConflictedFilesAsync(string path)
    {
        try
        {
            var statuses = await GetStatusAsync(path);
            return statuses.Where(s => s.WorkingCopyStatus == SvnStatusType.Conflicted).ToList();
        }
        catch
        {
            return new List<SvnStatus>();
        }
    }

    /// <summary>
    /// Gets the repository URL for a working copy
    /// </summary>
    public async Task<string?> GetRepositoryUrlAsync(string path)
    {
        try
        {
            var info = await GetWorkingCopyInfoAsync(path);
            return info?.RepositoryUrl;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Relocates a working copy to a new repository URL
    /// </summary>
    public async Task<SvnResult> RelocateAsync(
        string path,
        string fromUrl,
        string toUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new SvnCommand("switch", $"--relocate", fromUrl, toUrl, path);
            return await _commandService.ExecuteAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            return SvnResult.Fail(ex.Message, -1, ex);
        }
    }

    /// <summary>
    /// Gets the working copy depth
    /// </summary>
    public async Task<SvnDepth> GetDepthAsync(string path)
    {
        try
        {
            var info = await GetWorkingCopyInfoAsync(path);
            return info?.Depth ?? SvnDepth.Unknown;
        }
        catch
        {
            return SvnDepth.Unknown;
        }
    }

    /// <summary>
    /// Validates that a path exists and is accessible
    /// </summary>
    public bool IsPathAccessible(string path)
    {
        try
        {
            return Directory.Exists(path) || File.Exists(path);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets all recent working copies from application settings
    /// </summary>
    public Task<IList<string>> GetRecentWorkingCopiesAsync()
    {
        // TODO: Implement persistent storage (settings file, registry, etc.)
        return Task.FromResult<IList<string>>(new List<string>());
    }

    /// <summary>
    /// Adds a working copy to recent list
    /// </summary>
    public Task AddRecentWorkingCopyAsync(string path)
    {
        // TODO: Implement persistent storage
        return Task.CompletedTask;
    }

    /// <summary>
    /// Clears recent working copies list
    /// </summary>
    public Task ClearRecentWorkingCopiesAsync()
    {
        // TODO: Implement persistent storage
        return Task.CompletedTask;
    }

    /// <summary>
    /// Adds a file or directory to version control
    /// </summary>
    public async Task<SvnResult> AddAsync(
        string path,
        bool force = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = SvnCommand.Add(path, force);
            return await _commandService.ExecuteAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            return SvnResult.Fail(ex.Message, -1, ex);
        }
    }

    /// <summary>
    /// Adds multiple files to version control
    /// </summary>
    public async Task<SvnResult> AddAsync(
        IEnumerable<string> paths,
        bool force = false,
        CancellationToken cancellationToken = default)
    {
        var results = new StringBuilder();
        var hasError = false;

        foreach (var path in paths)
        {
            var result = await AddAsync(path, force, cancellationToken);
            if (!result.Success)
            {
                hasError = true;
                results.AppendLine($"Failed to add {path}: {result.StandardError}");
            }
            else
            {
                results.AppendLine($"Added: {path}");
            }
        }

        return hasError
            ? SvnResult.Fail(results.ToString(), -1)
            : SvnResult.Ok(results.ToString(), 0);
    }

    /// <summary>
    /// Deletes a file or directory from version control
    /// </summary>
    public async Task<SvnResult> DeleteAsync(
        string path,
        bool force = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = SvnCommand.Delete(path, force);
            return await _commandService.ExecuteAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            return SvnResult.Fail(ex.Message, -1, ex);
        }
    }

    /// <summary>
    /// Deletes multiple files from version control
    /// </summary>
    public async Task<SvnResult> DeleteAsync(
        IEnumerable<string> paths,
        bool force = false,
        CancellationToken cancellationToken = default)
    {
        var results = new StringBuilder();
        var hasError = false;

        foreach (var path in paths)
        {
            var result = await DeleteAsync(path, force, cancellationToken);
            if (!result.Success)
            {
                hasError = true;
                results.AppendLine($"Failed to delete {path}: {result.StandardError}");
            }
            else
            {
                results.AppendLine($"Deleted: {path}");
            }
        }

        return hasError
            ? SvnResult.Fail(results.ToString(), -1)
            : SvnResult.Ok(results.ToString(), 0);
    }

    /// <summary>
    /// Reverts a file or directory to its original state
    /// </summary>
    public async Task<SvnResult> RevertAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = SvnCommand.Revert(path);
            return await _commandService.ExecuteAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            return SvnResult.Fail(ex.Message, -1, ex);
        }
    }

    /// <summary>
    /// Reverts multiple files to their original state
    /// </summary>
    public async Task<SvnResult> RevertAsync(
        IEnumerable<string> paths,
        CancellationToken cancellationToken = default)
    {
        var results = new StringBuilder();
        var hasError = false;

        foreach (var path in paths)
        {
            var result = await RevertAsync(path, cancellationToken);
            if (!result.Success)
            {
                hasError = true;
                results.AppendLine($"Failed to revert {path}: {result.StandardError}");
            }
            else
            {
                results.AppendLine($"Reverted: {path}");
            }
        }

        return hasError
            ? SvnResult.Fail(results.ToString(), -1)
            : SvnResult.Ok(results.ToString(), 0);
    }

    /// <summary>
    /// Gets the diff for a file
    /// </summary>
    public async Task<string> GetDiffAsync(
        string path,
        long? revisionStart = null,
        long? revisionEnd = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = SvnCommand.Diff(path, revisionStart, revisionEnd);
            var result = await _commandService.ExecuteAsync(command, cancellationToken);
            return result.Success ? result.StandardOutput : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Commits changes to the repository
    /// </summary>
    public async Task<SvnResult> CommitAsync(
        string message,
        IEnumerable<string> paths,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var pathArray = paths.ToArray();
            if (pathArray.Length == 0)
            {
                return SvnResult.Fail("No files specified for commit", -1);
            }

            var command = SvnCommand.Commit(message, pathArray);
            return await _commandService.ExecuteAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            return SvnResult.Fail(ex.Message, -1, ex);
        }
    }

    /// <summary>
    /// Commits all modified files in the working copy
    /// </summary>
    public async Task<SvnResult> CommitAllAsync(
        string path,
        string message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = SvnCommand.Commit(message, new[] { path });
            return await _commandService.ExecuteAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            return SvnResult.Fail(ex.Message, -1, ex);
        }
    }

    /// <summary>
    /// Resolves a conflicted file
    /// </summary>
    public async Task<SvnResult> ResolveAsync(
        string path,
        string accept = "working",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = SvnCommand.Resolve(path, accept);
            return await _commandService.ExecuteAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            return SvnResult.Fail(ex.Message, -1, ex);
        }
    }

    /// <summary>
    /// Creates a branch or tag (svn copy)
    /// Both source and destination must be repository URLs for the -m flag to work
    /// </summary>
    public async Task<SvnResult> CopyAsync(
        string sourcePath,
        string destinationPath,
        string message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(sourcePath))
                return SvnResult.Fail("Source path cannot be empty", -1);

            if (string.IsNullOrWhiteSpace(destinationPath))
                return SvnResult.Fail("Destination path cannot be empty", -1);

            if (string.IsNullOrWhiteSpace(message))
                return SvnResult.Fail("Commit message cannot be empty", -1);

            // Ensure source and destination are URLs (not local paths)
            // SVN copy with -m flag requires URL-to-URL copy
            var sourceUrl = EnsureIsUrl(sourcePath);
            var destUrl = EnsureIsUrl(destinationPath);

            var command = SvnCommand.Copy(sourceUrl, destUrl, message);
            return await _commandService.ExecuteAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            return SvnResult.Fail(ex.Message, -1, ex);
        }
    }

    /// <summary>
    /// Ensures the path is a repository URL. If it's a local path, converts it to a file:// URL
    /// </summary>
    private string EnsureIsUrl(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;

        // If it's already a URL, return as-is
        if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("svn://", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("svn+ssh://", StringComparison.OrdinalIgnoreCase))
        {
            return path;
        }

        // If it's a file:// URL, return as-is
        if (path.StartsWith("file:///", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("file://localhost/", StringComparison.OrdinalIgnoreCase))
        {
            return path;
        }

        // If it's a local absolute path, convert to file:// URL
        if (Path.IsPathRooted(path))
        {
            // Convert to file:// URL format
            // On Windows: C:\path -> file:///C:/path
            // On Unix: /path -> file:///path
            var normalizedPath = path.Replace('\\', '/');
            if (!normalizedPath.StartsWith('/'))
            {
                // Windows path like C:/path -> /C:/path
                normalizedPath = "/" + normalizedPath;
            }
            return "file://" + normalizedPath;
        }

        // If we can't determine, return as-is
        return path;
    }

    /// <summary>
    /// Switches the working copy to a different URL
    /// </summary>
    public async Task<SvnResult> SwitchAsync(
        string path,
        string url,
        long? revision = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = SvnCommand.Switch(path, url, revision);
            return await _commandService.ExecuteAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            return SvnResult.Fail(ex.Message, -1, ex);
        }
    }

    /// <summary>
    /// Merges changes from a source to the working copy
    /// </summary>
    public async Task<SvnResult> MergeAsync(
        string sourcePath,
        long revisionStart,
        long revisionEnd,
        string targetPath,
        bool dryRun = false,
        string? accept = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = SvnCommand.Merge(sourcePath, revisionStart, revisionEnd, targetPath, dryRun, accept);
            return await _commandService.ExecuteAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            return SvnResult.Fail(ex.Message, -1, ex);
        }
    }

    /// <summary>
    /// Lists the contents of a repository path
    /// </summary>
    public async Task<IList<SvnListItem>> ListAsync(
        string url,
        long? revision = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = SvnCommand.List(url, revision);
            command.UseXml = true;

            var xmlDoc = await _commandService.ExecuteXmlAsync(command, cancellationToken);
            if (xmlDoc == null)
                return new List<SvnListItem>();

            var parser = new SvnListParser();
            return parser.ParseXml(xmlDoc);
        }
        catch
        {
            return new List<SvnListItem>();
        }
    }

    /// <summary>
    /// Checks out a repository to a local path
    /// </summary>
    public async Task<SvnResult> CheckoutAsync(
        string url,
        string path,
        long? revision = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = SvnCommand.Checkout(url, path, revision);
            return await _commandService.ExecuteAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            return SvnResult.Fail(ex.Message, -1, ex);
        }
    }

    /// <summary>
    /// Exports a repository to a local path (without .svn directories)
    /// </summary>
    public async Task<SvnResult> ExportAsync(
        string url,
        string path,
        long? revision = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var args = new List<string>();
            if (revision.HasValue)
            {
                args.Add($"-r{revision.Value}");
            }
            args.Add(url);
            args.Add(path);

            var command = new SvnCommand("export", args.ToArray());
            return await _commandService.ExecuteAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            return SvnResult.Fail(ex.Message, -1, ex);
        }
    }

    /// <summary>
    /// Gets the blame (annotate) information for a file
    /// </summary>
    public async Task<SvnBlameResult> BlameAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = SvnCommand.Blame(path);
            command.UseXml = true;

            var xmlDoc = await _commandService.ExecuteXmlAsync(command, cancellationToken);
            if (xmlDoc == null)
                return new SvnBlameResult { Path = path };

            // Parse the blame output
            var result = new SvnBlameResult { Path = path };
            var entries = xmlDoc.SelectNodes("//entry");
            if (entries != null)
            {
                int lineNumber = 1;
                foreach (System.Xml.XmlNode entry in entries)
                {
                    var commitNode = entry.SelectSingleNode("commit");
                    var line = new SvnBlameLine
                    {
                        LineNumber = lineNumber++,
                        Content = entry.InnerText
                    };

                    if (commitNode != null)
                    {
                        var revAttr = commitNode.Attributes?["revision"];
                        if (revAttr != null && long.TryParse(revAttr.Value, out var rev))
                        {
                            line.Revision = rev;
                        }

                        var authorNode = commitNode.SelectSingleNode("author");
                        if (authorNode != null)
                        {
                            line.Author = authorNode.InnerText;
                        }

                        var dateNode = commitNode.SelectSingleNode("date");
                        if (dateNode != null && DateTime.TryParse(dateNode.InnerText, out var date))
                        {
                            line.Date = date;
                        }
                    }

                    result.Lines.Add(line);
                }
            }

            return result;
        }
        catch
        {
            return new SvnBlameResult { Path = path };
        }
    }

    /// <summary>
    /// Locks a file
    /// </summary>
    public async Task<SvnResult> LockAsync(
        string path,
        string message,
        bool force = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = SvnCommand.Lock(path, message, force);
            return await _commandService.ExecuteAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            return SvnResult.Fail(ex.Message, -1, ex);
        }
    }

    /// <summary>
    /// Unlocks a file
    /// </summary>
    public async Task<SvnResult> UnlockAsync(
        string path,
        bool force = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = SvnCommand.Unlock(path, force);
            return await _commandService.ExecuteAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            return SvnResult.Fail(ex.Message, -1, ex);
        }
    }

    /// <summary>
    /// Gets all properties for a path
    /// </summary>
    public async Task<Dictionary<string, string>> GetPropertiesAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        var properties = new Dictionary<string, string>();
        try
        {
            // Get property list
            var listCommand = SvnCommand.PropList(path);
            listCommand.UseXml = true;

            var xmlDoc = await _commandService.ExecuteXmlAsync(listCommand, cancellationToken);
            if (xmlDoc == null)
                return properties;

            // Parse property names
            var propNodes = xmlDoc.SelectNodes("//property/@name");
            if (propNodes == null)
                return properties;

            foreach (System.Xml.XmlNode node in propNodes)
            {
                var propName = node.Value;
                if (string.IsNullOrEmpty(propName))
                    continue;

                // Get property value
                var getCommand = SvnCommand.PropGet(propName, path);
                var result = await _commandService.ExecuteAsync(getCommand, cancellationToken);
                if (result.Success)
                {
                    properties[propName] = result.StandardOutput.Trim();
                }
            }

            return properties;
        }
        catch
        {
            return properties;
        }
    }

    /// <summary>
    /// Gets a specific property value
    /// </summary>
    public async Task<string?> GetPropertyAsync(
        string path,
        string propertyName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = SvnCommand.PropGet(propertyName, path);
            var result = await _commandService.ExecuteAsync(command, cancellationToken);
            return result.Success ? result.StandardOutput.Trim() : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Sets a property value
    /// </summary>
    public async Task<SvnResult> SetPropertyAsync(
        string path,
        string propertyName,
        string value,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = SvnCommand.PropSet(propertyName, value, path);
            return await _commandService.ExecuteAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            return SvnResult.Fail(ex.Message, -1, ex);
        }
    }

    /// <summary>
    /// Deletes a property
    /// </summary>
    public async Task<SvnResult> DeletePropertyAsync(
        string path,
        string propertyName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = SvnCommand.PropDelete(propertyName, path);
            return await _commandService.ExecuteAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            return SvnResult.Fail(ex.Message, -1, ex);
        }
    }

    /// <summary>
    /// Moves/renames a file or directory
    /// </summary>
    public async Task<SvnResult> MoveAsync(
        string sourcePath,
        string destinationPath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = SvnCommand.Move(sourcePath, destinationPath);
            return await _commandService.ExecuteAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            return SvnResult.Fail(ex.Message, -1, ex);
        }
    }

    /// <summary>
    /// Creates a directory under version control
    /// </summary>
    public async Task<SvnResult> MkdirAsync(
        string path,
        string? message = null,
        bool parents = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = SvnCommand.Mkdir(path, message, parents);
            return await _commandService.ExecuteAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            return SvnResult.Fail(ex.Message, -1, ex);
        }
    }

    /// <summary>
    /// Gets the contents of a file from the repository
    /// </summary>
    public async Task<string?> CatAsync(
        string path,
        long? revision = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = SvnCommand.Cat(path, revision);
            var result = await _commandService.ExecuteAsync(command, cancellationToken);
            return result.Success ? result.StandardOutput : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Imports files into the repository
    /// </summary>
    public async Task<SvnResult> ImportAsync(
        string localPath,
        string repositoryUrl,
        string message,
        bool noIgnore = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = SvnCommand.Import(localPath, repositoryUrl, message, noIgnore);
            return await _commandService.ExecuteAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            return SvnResult.Fail(ex.Message, -1, ex);
        }
    }
}
