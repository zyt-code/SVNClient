using System;

namespace Svns.Services.Svn.Core;

/// <summary>
/// Exception thrown when SVN command execution fails
/// </summary>
public class SvnException : Exception
{
    /// <summary>
    /// The command that was being executed
    /// </summary>
    public string Command { get; }

    /// <summary>
    /// The arguments that were passed to the command
    /// </summary>
    public string[] Arguments { get; }

    /// <summary>
    /// The exit code from the process
    /// </summary>
    public int ExitCode { get; }

    /// <summary>
    /// The error output from the command
    /// </summary>
    public string ErrorOutput { get; }

    public SvnException(string command, string[] arguments, int exitCode, string errorOutput)
        : base($"SVN command '{command}' failed with exit code {exitCode}: {errorOutput}")
    {
        Command = command;
        Arguments = arguments;
        ExitCode = exitCode;
        ErrorOutput = errorOutput;
    }

    public SvnException(string message, Exception innerException)
        : base(message, innerException)
    {
        Command = string.Empty;
        Arguments = Array.Empty<string>();
        ExitCode = -1;
        ErrorOutput = innerException.Message;
    }
}

/// <summary>
/// Exception thrown when working copy is not found or invalid
/// </summary>
public class WorkingCopyNotFoundException : SvnException
{
    public string Path { get; }

    public WorkingCopyNotFoundException(string path)
        : base("status", Array.Empty<string>(), -1, $"Working copy not found at: {path}")
    {
        Path = path;
    }
}

/// <summary>
/// Exception thrown when SVN executable is not found
/// </summary>
public class SvnNotFoundException : Exception
{
    public SvnNotFoundException()
        : base("SVN executable not found. Please ensure SVN is installed and available in PATH.")
    {
    }
}

/// <summary>
/// Exception thrown when SVN operation is cancelled
/// </summary>
public class SvnOperationCanceledException : Exception
{
    public SvnOperationCanceledException()
        : base("SVN operation was cancelled by the user")
    {
    }
}
