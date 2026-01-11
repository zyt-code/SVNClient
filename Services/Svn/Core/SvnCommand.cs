using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
namespace Svns.Services.Svn.Core;

/// <summary>
/// Represents an SVN command with its arguments
/// </summary>
public class SvnCommand
{
    /// <summary>
    /// The command name (e.g., "status", "update", "commit")
    /// </summary>
    public string Command { get; }

    /// <summary>
    /// Arguments for the command
    /// </summary>
    public List<string> Arguments { get; }

    /// <summary>
    /// Whether to use XML output format
    /// </summary>
    public bool UseXml { get; set; }

    /// <summary>
    /// The working copy path for the command
    /// </summary>
    public string? WorkingCopyPath { get; set; }

    public SvnCommand(string command, params string[] arguments)
    {
        Command = command;
        Arguments = arguments.ToList();
        UseXml = false;
    }

    /// <summary>
    /// Builds the complete argument list for the SVN command line
    /// </summary>
    public string[] BuildArguments()
    {
        var args = new List<string>();

        args.Add(Command);

        if (UseXml)
        {
            args.Add("--xml");
        }

        args.AddRange(Arguments);

        return args.ToArray();
    }

    /// <summary>
    /// Creates a status command
    /// </summary>
    public static SvnCommand Status(string path = "", bool verbose = false)
    {
        var args = new List<string>();
        if (!string.IsNullOrEmpty(path))
        {
            args.Add(path);
        }

        if (verbose)
        {
            args.Add("-v");
        }

        return new SvnCommand("status", args.ToArray());
    }

    /// <summary>
    /// Creates an update command
    /// </summary>
    public static SvnCommand Update(string path = "", long? revision = null)
    {
        var args = new List<string>();
        if (!string.IsNullOrEmpty(path))
        {
            args.Add(path);
        }

        if (revision.HasValue)
        {
            args.Add($"-r{revision.Value}");
        }

        return new SvnCommand("update", args.ToArray());
    }

    /// <summary>
    /// Creates a commit command
    /// </summary>
    public static SvnCommand Commit(string message, string[] paths)
    {
        var args = new List<string>
        {
            "-m", message
        };

        args.AddRange(paths);

        return new SvnCommand("commit", args.ToArray());
    }

    /// <summary>
    /// Creates an add command
    /// </summary>
    public static SvnCommand Add(string path, bool force = false)
    {
        var args = new List<string>();
        if (force)
        {
            args.Add("--force");
        }

        args.Add(path);

        return new SvnCommand("add", args.ToArray());
    }

    /// <summary>
    /// Creates a delete command
    /// </summary>
    public static SvnCommand Delete(string path, bool force = false)
    {
        var args = new List<string>();
        if (force)
        {
            args.Add("--force");
        }

        args.Add(path);

        return new SvnCommand("delete", args.ToArray());
    }

    /// <summary>
    /// Creates a revert command
    /// </summary>
    public static SvnCommand Revert(string path)
    {
        return new SvnCommand("revert", path);
    }

    /// <summary>
    /// Creates a diff command
    /// </summary>
    public static SvnCommand Diff(string path, long? revisionStart = null, long? revisionEnd = null)
    {
        var args = new List<string>();

        if (revisionStart.HasValue && revisionEnd.HasValue)
        {
            args.Add($"-r{revisionStart.Value}:{revisionEnd.Value}");
        }
        else if (revisionStart.HasValue)
        {
            args.Add($"-r{revisionStart.Value}");
        }

        args.Add(path);

        return new SvnCommand("diff", args.ToArray());
    }

    /// <summary>
    /// Creates a log command
    /// </summary>
    public static SvnCommand Log(string path, int? limit = null, bool verbose = false)
    {
        var args = new List<string>();
        args.Add(path);

        if (limit.HasValue)
        {
            args.Add($"-l{limit.Value}");
        }

        if (verbose)
        {
            args.Add("-v");
        }

        return new SvnCommand("log", args.ToArray());
    }

    /// <summary>
    /// Creates an info command
    /// </summary>
    public static SvnCommand Info(string path = "")
    {
        return string.IsNullOrEmpty(path)
            ? new SvnCommand("info")
            : new SvnCommand("info", path);
    }

    /// <summary>
    /// Creates a cleanup command
    /// </summary>
    public static SvnCommand Cleanup(string path = "", bool removeUnversioned = false, bool removeIgnored = false)
    {
        var args = new List<string>();

        if (removeUnversioned)
        {
            args.Add("--remove-unversioned");
        }

        if (removeIgnored)
        {
            args.Add("--remove-ignored");
        }

        if (!string.IsNullOrEmpty(path))
        {
            args.Add(path);
        }

        return new SvnCommand("cleanup", args.ToArray());
    }

    /// <summary>
    /// Creates a copy command (for branching/tagging)
    /// Uses URL-to-URL copy to enable the -m flag (log message).
    /// Do NOT convert file:// URLs to local paths - they must remain as URLs
    /// for this to be a repository operation rather than a working copy operation.
    /// </summary>
    public static SvnCommand Copy(string sourcePath, string destinationPath, string message)
    {
        // For branching/tagging with a commit message, we need URL-to-URL copy.
        // Keep paths as-is (URLs) even if they are file:// URLs.
        // The -m flag is only valid for URL-to-URL copies, not for WC operations.
        return new SvnCommand("copy", sourcePath, destinationPath, "-m", message);
    }

    /// <summary>
    /// Creates a switch command
    /// </summary>
    public static SvnCommand Switch(string path, string url, long? revision = null)
    {
        var args = new List<string>();

        if (revision.HasValue)
        {
            args.Add($"-r{revision.Value}");
        }

        args.AddRange(new[] { path, url });

        return new SvnCommand("switch", args.ToArray());
    }

    /// <summary>
    /// Creates a merge command
    /// </summary>
    public static SvnCommand Merge(string sourcePath, long revisionStart, long revisionEnd, string targetPath, bool dryRun = false, string? accept = null)
    {
        var args = new List<string>();

        if (dryRun)
        {
            args.Add("--dry-run");
        }

        if (!string.IsNullOrEmpty(accept))
        {
            args.Add($"--accept={accept}");
        }

        args.AddRange(new[] { sourcePath, $"-r{revisionStart}:{revisionEnd}", targetPath });

        return new SvnCommand("merge", args.ToArray());
    }

    /// <summary>
    /// Creates a blame command
    /// </summary>
    public static SvnCommand Blame(string path)
    {
        return new SvnCommand("blame", path);
    }

    /// <summary>
    /// Creates a list command
    /// </summary>
    public static SvnCommand List(string path, long? revision = null)
    {
        var args = new List<string>();

        if (revision.HasValue)
        {
            args.Add($"-r{revision.Value}");
        }

        args.Add(path);

        return new SvnCommand("list", args.ToArray());
    }

    /// <summary>
    /// Creates a checkout command
    /// </summary>
    public static SvnCommand Checkout(string url, string path, long? revision = null)
    {
        var args = new List<string>();

        if (revision.HasValue)
        {
            args.Add($"-r{revision.Value}");
        }

        args.AddRange(new[] { url, path });

        return new SvnCommand("checkout", args.ToArray());
    }

    /// <summary>
    /// Creates a property list command
    /// </summary>
    public static SvnCommand PropList(string path, bool recursive = false)
    {
        var args = new List<string>();

        if (recursive)
        {
            args.Add("-R");
        }

        args.Add(path);

        return new SvnCommand("proplist", args.ToArray());
    }

    /// <summary>
    /// Creates a property get command
    /// </summary>
    public static SvnCommand PropGet(string propertyName, string path)
    {
        return new SvnCommand("propget", propertyName, path);
    }

    /// <summary>
    /// Creates a property set command
    /// </summary>
    public static SvnCommand PropSet(string propertyName, string value, string path)
    {
        return new SvnCommand("propset", propertyName, value, path);
    }

    /// <summary>
    /// Creates a property delete command
    /// </summary>
    public static SvnCommand PropDelete(string propertyName, string path)
    {
        return new SvnCommand("propdel", propertyName, path);
    }

    /// <summary>
    /// Creates a move/rename command
    /// </summary>
    public static SvnCommand Move(string sourcePath, string destinationPath)
    {
        return new SvnCommand("move", sourcePath, destinationPath);
    }

    /// <summary>
    /// Creates a lock command
    /// </summary>
    public static SvnCommand Lock(string path, string message, bool force = false)
    {
        var args = new List<string>();

        if (force)
        {
            args.Add("--force");
        }

        args.AddRange(new[] { path, "-m", message });

        return new SvnCommand("lock", args.ToArray());
    }

    /// <summary>
    /// Creates an unlock command
    /// </summary>
    public static SvnCommand Unlock(string path, bool force = false)
    {
        var args = new List<string>();

        if (force)
        {
            args.Add("--force");
        }

        args.Add(path);

        return new SvnCommand("unlock", args.ToArray());
    }

    /// <summary>
    /// Creates a resolve command
    /// </summary>
    public static SvnCommand Resolve(string path, string accept = "working")
    {
        return new SvnCommand("resolve", $"--accept={accept}", path);
    }

    /// <summary>
    /// Creates a mkdir command to create a directory under version control
    /// </summary>
    public static SvnCommand Mkdir(string path, string? message = null, bool parents = false)
    {
        var args = new List<string>();

        if (parents)
        {
            args.Add("--parents");
        }

        if (!string.IsNullOrEmpty(message))
        {
            args.AddRange(new[] { "-m", message });
        }

        args.Add(path);

        return new SvnCommand("mkdir", args.ToArray());
    }

    /// <summary>
    /// Creates a cat command to view file contents from repository
    /// </summary>
    public static SvnCommand Cat(string path, long? revision = null)
    {
        var args = new List<string>();

        if (revision.HasValue)
        {
            args.Add($"-r{revision.Value}");
        }

        args.Add(path);

        return new SvnCommand("cat", args.ToArray());
    }

    /// <summary>
    /// Creates an import command to import files into repository
    /// </summary>
    public static SvnCommand Import(string localPath, string repositoryUrl, string message, bool noIgnore = false)
    {
        var args = new List<string>();

        if (noIgnore)
        {
            args.Add("--no-ignore");
        }

        args.AddRange(new[] { localPath, repositoryUrl, "-m", message });

        return new SvnCommand("import", args.ToArray());
    }

    /// <summary>
    /// Creates a relocate command to change repository URL
    /// </summary>
    public static SvnCommand Relocate(string fromUrl, string toUrl, string? path = null)
    {
        var args = new List<string> { fromUrl, toUrl };

        if (!string.IsNullOrEmpty(path))
        {
            args.Add(path);
        }

        return new SvnCommand("relocate", args.ToArray());
    }

    /// <summary>
    /// Creates an export command to export clean copy from repository
    /// </summary>
    public static SvnCommand Export(string source, string destination, long? revision = null, bool force = false)
    {
        var args = new List<string>();

        if (revision.HasValue)
        {
            args.Add($"-r{revision.Value}");
        }

        if (force)
        {
            args.Add("--force");
        }

        args.AddRange(new[] { source, destination });

        return new SvnCommand("export", args.ToArray());
    }
}
