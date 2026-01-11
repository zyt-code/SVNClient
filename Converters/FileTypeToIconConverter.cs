using Avalonia.Data.Converters;
using Material.Icons;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Svns.Converters;

/// <summary>
/// Converts file path to appropriate Material icon based on file type
/// Uses Material.Icons.Avalonia for vector icons
/// </summary>
public class FileTypeToIconConverter : IValueConverter
{
    private static readonly Dictionary<string, MaterialIconKind> ExtensionIcons = new()
    {
        // Programming Languages
        { ".java", MaterialIconKind.Coffee },
        { ".class", MaterialIconKind.Coffee },
        { ".jar", MaterialIconKind.Coffee },

        { ".cs", MaterialIconKind.FileCode },
        { ".csproj", MaterialIconKind.FileCode },
        { ".sln", MaterialIconKind.FileCode },

        { ".py", MaterialIconKind.FileCode },
        { ".pyw", MaterialIconKind.FileCode },

        { ".js", MaterialIconKind.FileCode },
        { ".jsx", MaterialIconKind.FileCode },
        { ".ts", MaterialIconKind.FileCode },
        { ".tsx", MaterialIconKind.FileCode },
        { ".vue", MaterialIconKind.FileCode },
        { ".json", MaterialIconKind.CodeBraces },

        { ".go", MaterialIconKind.FileCode },
        { ".rs", MaterialIconKind.FileCode },
        { ".cpp", MaterialIconKind.FileCode },
        { ".cc", MaterialIconKind.FileCode },
        { ".cxx", MaterialIconKind.FileCode },
        { ".hpp", MaterialIconKind.FileCode },
        { ".h", MaterialIconKind.FileCode },
        { ".c", MaterialIconKind.FileCode },

        { ".php", MaterialIconKind.FileCode },
        { ".rb", MaterialIconKind.FileCode },
        { ".swift", MaterialIconKind.FileCode },
        { ".kt", MaterialIconKind.FileCode },
        { ".kts", MaterialIconKind.FileCode },
        { ".scala", MaterialIconKind.FileCode },

        // Web
        { ".html", MaterialIconKind.FileCode },
        { ".htm", MaterialIconKind.FileCode },
        { ".css", MaterialIconKind.Palette },
        { ".scss", MaterialIconKind.Palette },
        { ".sass", MaterialIconKind.Palette },
        { ".less", MaterialIconKind.Palette },

        // Config & Data
        { ".xml", MaterialIconKind.FileCode },
        { ".axaml", MaterialIconKind.FileCode },
        { ".yaml", MaterialIconKind.FileDocument },
        { ".yml", MaterialIconKind.FileDocument },
        { ".toml", MaterialIconKind.FileDocument },
        { ".ini", MaterialIconKind.FileDocument },
        { ".properties", MaterialIconKind.FileDocument },

        // Documentation
        { ".md", MaterialIconKind.FileDocument },
        { ".markdown", MaterialIconKind.FileDocument },
        { ".txt", MaterialIconKind.FileDocument },
        { ".rst", MaterialIconKind.FileDocument },

        // Build & Tools
        { ".gradle", MaterialIconKind.FileCode },
        { ".pom", MaterialIconKind.FileCode },
        { ".gitignore", MaterialIconKind.Git },
        { ".gitattributes", MaterialIconKind.Git },

        // Shell
        { ".sh", MaterialIconKind.Console },
        { ".bash", MaterialIconKind.Console },
        { ".zsh", MaterialIconKind.Console },
        { ".fish", MaterialIconKind.Console },
        { ".ps1", MaterialIconKind.Console },
        { ".bat", MaterialIconKind.Console },
        { ".cmd", MaterialIconKind.Console },

        // Database
        { ".sql", MaterialIconKind.Database },
        { ".db", MaterialIconKind.Database },
        { ".sqlite", MaterialIconKind.Database },

        // Images
        { ".png", MaterialIconKind.Image },
        { ".jpg", MaterialIconKind.Image },
        { ".jpeg", MaterialIconKind.Image },
        { ".gif", MaterialIconKind.Image },
        { ".svg", MaterialIconKind.Image },
        { ".ico", MaterialIconKind.Image },
        { ".bmp", MaterialIconKind.Image },
        { ".webp", MaterialIconKind.Image },

        // Archives
        { ".zip", MaterialIconKind.ZipBox },
        { ".tar", MaterialIconKind.ZipBox },
        { ".gz", MaterialIconKind.ZipBox },
        { ".rar", MaterialIconKind.ZipBox },
        { ".7z", MaterialIconKind.ZipBox },

        // Other
        { ".pdf", MaterialIconKind.FilePdfBox },
        { ".doc", MaterialIconKind.File },
        { ".docx", MaterialIconKind.File },
        { ".xls", MaterialIconKind.File },
        { ".xlsx", MaterialIconKind.File },
        { ".ppt", MaterialIconKind.File },
        { ".pptx", MaterialIconKind.File },

        { ".log", MaterialIconKind.FileDocument },
        { ".iml", MaterialIconKind.FileCode },
    };

    private static readonly Dictionary<string, MaterialIconKind> FileNameIcons = new()
    {
        { "dockerfile", MaterialIconKind.Docker },
        { "docker-compose.yml", MaterialIconKind.Docker },
        { "docker-compose.yaml", MaterialIconKind.Docker },
        { "makefile", MaterialIconKind.Gear },
        { "cmakelists.txt", MaterialIconKind.Gear },
        { "package.json", MaterialIconKind.Package },
        { "yarn.lock", MaterialIconKind.Package },
        { "composer.json", MaterialIconKind.Package },
        { "gemfile", MaterialIconKind.DiamondStone },
        { "cargo.toml", MaterialIconKind.Gear },
        { "go.mod", MaterialIconKind.FileCode },
        { "requirements.txt", MaterialIconKind.FileCode },
        { "pipfile", MaterialIconKind.FileCode },
        { ".gitignore", MaterialIconKind.Git },
        { ".gitattributes", MaterialIconKind.Git },
        { "license", MaterialIconKind.Certificate },
        { "readme", MaterialIconKind.FileDocument },
        { "changelog", MaterialIconKind.FileDocument },
        { "manifest", MaterialIconKind.FileDocument },
        { "appveyor.yml", MaterialIconKind.Server },
        { ".travis.yml", MaterialIconKind.Server },
        { "jenkinsfile", MaterialIconKind.Server },
        { "pom.xml", MaterialIconKind.FileCode },
    };

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string path)
        {
            var fileName = Path.GetFileName(path).ToLowerInvariant();

            // Check specific file names first
            if (FileNameIcons.TryGetValue(fileName, out var fileIcon))
            {
                return fileIcon;
            }

            // Check by extension
            var extension = Path.GetExtension(path).ToLowerInvariant();
            if (ExtensionIcons.TryGetValue(extension, out var extIcon))
            {
                return extIcon;
            }

            // Check if it's a directory (no extension usually means folder in tree view)
            if (string.IsNullOrEmpty(extension) || path.EndsWith("/") || path.EndsWith("\\"))
            {
                return MaterialIconKind.Folder;
            }

            // Default file icon
            return MaterialIconKind.File;
        }

        return MaterialIconKind.File;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
