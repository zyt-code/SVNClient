using System.Diagnostics;
using System.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using System.Text;

namespace Svns.Services.Svn.Core;

/// <summary>
/// Service for executing SVN commands
/// </summary>
public partial class SvnCommandService
{
    private readonly string _svnExecutablePath;

    /// <summary>
    /// Event raised when command output is received
    /// </summary>
    public event EventHandler<string>? OnOutputReceived;

    /// <summary>
    /// Event raised when command error is received
    /// </summary>
    public event EventHandler<string>? OnErrorReceived;

    public SvnCommandService(string? svnExecutablePath = null)
    {
        _svnExecutablePath = svnExecutablePath ?? "svn";
    }

    /// <summary>
    /// Executes an SVN command asynchronously
    /// </summary>
    public async Task<SvnResult> ExecuteAsync(
        SvnCommand command,
        CancellationToken cancellationToken = default)
    {
        var arguments = command.BuildArguments();
        var workingDirectory = command.WorkingCopyPath ?? Directory.GetCurrentDirectory();

        return await ExecuteAsync(workingDirectory, arguments, cancellationToken);
    }

    /// <summary>
    /// Executes SVN command with raw arguments asynchronously
    /// </summary>
    public async Task<SvnResult> ExecuteAsync(
        string workingCopy,
        string[] arguments,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _svnExecutablePath,
                WorkingDirectory = workingCopy,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8
            };

            foreach (var arg in arguments)
            {
                startInfo.ArgumentList.Add(arg);
            }

            using var process = new Process { StartInfo = startInfo };
            var outputBuilder = new System.Text.StringBuilder();
            var errorBuilder = new System.Text.StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    outputBuilder.AppendLine(e.Data);
                    OnOutputReceived?.Invoke(this, e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    errorBuilder.AppendLine(e.Data);
                    OnErrorReceived?.Invoke(this, e.Data);
                }
            };

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await WaitForExitAsync(process, cancellationToken);

            var exitCode = process.ExitCode;
            var output = outputBuilder.ToString();
            var error = errorBuilder.ToString();

            if (exitCode != 0)
            {
                return SvnResult.Fail(error, exitCode);
            }

            return SvnResult.Ok(output, exitCode);
        }
        catch (OperationCanceledException)
        {
            throw new SvnOperationCanceledException();
        }
        catch (Exception ex)
        {
            return SvnResult.Fail(ex.Message, -1, ex);
        }
    }

    /// <summary>
    /// Executes an SVN command and returns XML output as XmlDocument
    /// </summary>
    public async Task<XmlDocument?> ExecuteXmlAsync(
        SvnCommand command,
        CancellationToken cancellationToken = default)
    {
        command.UseXml = true;
        var result = await ExecuteAsync(command, cancellationToken);

        if (!result.Success)
        {
            return null;
        }

        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(result.StandardOutput);
            return doc;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Executes an SVN command and invokes progress callback
    /// </summary>
    public async Task<SvnResult> ExecuteWithProgressAsync(
        SvnCommand command,
        IProgress<SvnProgressInfo>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(command, cancellationToken);
    }

    private async Task WaitForExitAsync(Process process, CancellationToken cancellationToken)
    {
        while (!process.HasExited)
        {
            await Task.Delay(100, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch
                {
                    // Ignore errors when killing process
                }

                throw new OperationCanceledException(cancellationToken);
            }
        }

        process.WaitForExit();
    }

    /// <summary>
    /// Checks if SVN is available
    /// </summary>
    public async Task<bool> IsSvnAvailableAsync()
    {
        try
        {
            var result = await ExecuteAsync(Directory.GetCurrentDirectory(), new[] { "--version" });
            return result.Success && result.StandardOutput.Contains("svn");
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets SVN version information
    /// </summary>
    public async Task<string?> GetSvnVersionAsync()
    {
        try
        {
            var result = await ExecuteAsync(Directory.GetCurrentDirectory(), new[] { "--version" });
            return result.Success ? result.StandardOutput.Trim() : null;
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Progress information for SVN operations
/// </summary>
public class SvnProgressInfo
{
    public long Current { get; set; }
    public long Total { get; set; }
    public string Message { get; set; } = string.Empty;
    public int ProgressPercentage => Total > 0 ? (int)((Current * 100) / Total) : 0;
}

/// <summary>
/// Event arguments for SVN progress updates
/// </summary>
public class SvnProgressEventArgs : EventArgs
{
    public SvnProgressInfo Info { get; }
    public string Message => Info.Message;

    public SvnProgressEventArgs(SvnProgressInfo info)
    {
        Info = info;
    }
}
