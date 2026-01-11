using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
namespace Svns.Services.Svn.Core;

/// <summary>
/// Represents the result of an SVN command execution
/// </summary>
public class SvnResult
{
    /// <summary>
    /// Indicates whether the command executed successfully
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// The standard output from the command
    /// </summary>
    public string StandardOutput { get; init; } = string.Empty;

    /// <summary>
    /// The standard error output from the command
    /// </summary>
    public string StandardError { get; init; } = string.Empty;

    /// <summary>
    /// The exit code from the process
    /// </summary>
    public int ExitCode { get; init; }

    /// <summary>
    /// Any exception that occurred during execution
    /// </summary>
    public Exception? Exception { get; init; }

    /// <summary>
    /// Creates a successful result
    /// </summary>
    public static SvnResult Ok(string output, int exitCode = 0)
    {
        return new SvnResult
        {
            Success = true,
            StandardOutput = output,
            ExitCode = exitCode
        };
    }

    /// <summary>
    /// Creates a failed result
    /// </summary>
    public static SvnResult Fail(string error, int exitCode = -1, Exception? exception = null)
    {
        return new SvnResult
        {
            Success = false,
            StandardError = error,
            ExitCode = exitCode,
            Exception = exception
        };
    }
}
