using System;

namespace Svns.Models;

/// <summary>
/// Represents information about an SVN repository
/// </summary>
public class SvnRepositoryInfo
{
    /// <summary>
    /// The repository root URL
    /// </summary>
    public string RootUrl { get; set; } = string.Empty;

    /// <summary>
    /// The repository UUID
    /// </summary>
    public string Uuid { get; set; } = string.Empty;

    /// <summary>
    /// The repository URL
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// The relative path from root
    /// </summary>
    public string RelativePath { get; set; } = string.Empty;

    /// <summary>
    /// The repository type (svn, svn+ssh, http, https, etc.)
    /// </summary>
    public string RepositoryType { get; set; } = string.Empty;

    /// <summary>
    /// The server host
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// The server port (if applicable)
    /// </summary>
    public int? Port { get; set; }

    /// <summary>
    /// Whether this repository requires authentication
    /// </summary>
    public bool RequiresAuthentication { get; set; }

    /// <summary>
    /// Gets the display name
    /// </summary>
    public string DisplayName => !string.IsNullOrEmpty(RelativePath)
        ? RelativePath
        : RootUrl;

    /// <summary>
    /// Gets the protocol scheme
    /// </summary>
    public string? Protocol
    {
        get
        {
            try
            {
                var uri = new Uri(RootUrl);
                return uri.Scheme;
            }
            catch
            {
                return null;
            }
        }
    }
}
