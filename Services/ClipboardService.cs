using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Avalonia.Controls;

#pragma warning disable AVLN1000 // Suppress obsolete warning for GetTextAsync

namespace Svns.Services;

/// <summary>
/// Service for clipboard operations
/// </summary>
public class ClipboardService
{
    /// <summary>
    /// Copies text to the clipboard
    /// </summary>
    /// <param name="text">The text to copy</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful, false otherwise</returns>
    public async Task<bool> SetTextAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                    && desktop.MainWindow?.Clipboard != null)
                {
                    await desktop.MainWindow.Clipboard.SetTextAsync(text);
                }
            });
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Gets text from the clipboard
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The clipboard text, or empty string if unavailable</returns>
    public async Task<string> GetTextAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = string.Empty;
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                    && desktop.MainWindow?.Clipboard != null)
                {
                    result = await desktop.MainWindow.Clipboard.GetTextAsync() ?? string.Empty;
                }
            });
            return result;
        }
        catch (OperationCanceledException)
        {
            return string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Clears the clipboard
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                    && desktop.MainWindow?.Clipboard != null)
                {
                    await desktop.MainWindow.Clipboard.ClearAsync();
                }
            });
        }
        catch (OperationCanceledException)
        {
            // Ignore cancellation
        }
        catch (Exception)
        {
            // Ignore other errors
        }
    }
}
