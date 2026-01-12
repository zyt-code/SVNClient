# Svns - Modern SVN Client

<div align="center">

![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)
![Avalonia UI](https://img.shields.io/badge/Avalonia-11.3-FF3777.svg)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20macOS%20%7C%20Linux-blue.svg)
![License](https://img.shields.io/badge/license-MIT-green.svg)

A modern, cross-platform SVN client built with .NET 9 and Avalonia UI

[Features](#-features) • [Download](#-download) • [Quick Start](#-quick-start) • [Build](#-build-from-source)

</div>

---

## ✨ Features

### Core Functionality
- **Complete SVN Command Support** - All standard operations: Update, Commit, Revert, Add, Delete, Merge, Branch/Tag, Switch, and more
- **Working Copy Management** - Single-working-copy model for focused development
- **File History** - View complete revision history with log entries and changed paths
- **Diff Viewer** - Compare file changes between revisions
- **Blame Annotations** - Trace line-by-line authorship
- **Conflict Resolution** - Visual tools for resolving merge conflicts

### User Experience
- **Modern Glass UI** - Clean, contemporary interface inspired by modern design trends
- **Tree View File Browser** - Hierarchical display of your working copy
- **Real-time Status Indicators** - Color-coded badges for file status
- **Smart Selection** - Hierarchical checkboxes with parent-child cascading
- **Search & Filter** - Quickly find files by name
- **Keyboard Shortcuts** - Power user friendly

### Advanced Features
- **State Machine Validation** - Smart file operation validation prevents errors
- **Delete Confirmation** - Preview files before deletion with recovery hints
- **Batch Operations** - Select multiple files for commit, revert, or delete
- **Repository Browser** - Navigate remote repositories
- **Property Editor** - View and edit SVN properties

## 📥 Download

Get the latest release for your platform:

| Platform | Download |
|----------|----------|
| **Windows x64** | [Svns-win-x64.zip](https://github.com/zyt-code/SVNClient/releases/latest) |
| **macOS Intel** | [Svns-osx-x64.zip](https://github.com/zyt-code/SVNClient/releases/latest) |
| **macOS Apple Silicon** | [Svns-osx-arm64.zip](https://github.com/zyt-code/SVNClient/releases/latest) |

### Requirements
- **Windows**: Windows 10 or later
- **macOS**: macOS 10.15 (Catalina) or later
- **SVN Client**: Subversion command-line tool must be installed and in PATH

> **Note**: Svns requires `svn` to be installed on your system. Download from [subversion.apache.org](https://subversion.apache.org/download.cgi).

## 🚀 Quick Start

1. **Download** the latest release for your platform
2. **Extract** the archive to a folder of your choice
3. **Run** the executable:
   - Windows: `Svns.exe`
   - macOS: `Svns`

### First Steps

1. **Open Working Copy** - Use `File > Open Working Copy` to select your SVN working directory
2. **Review Changes** - See all modified, added, and deleted files at a glance
3. **Commit Changes** - Select files, enter a message, and commit

## 📸 Screenshots

#### Main Window
<img src="docs/images/main-window.png" alt="Main Window" width="800"/>

#### Commit Dialog
<img src="docs/images/commit-dialog.png" alt="Commit Dialog" width="600"/>

#### File History
<img src="docs/images/file-history.png" alt="File History" width="800"/>

## 🔧 Build from Source

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Git

### Steps

```bash
# Clone the repository
git clone https://github.com/zyt-code/SVNClient.git
cd SVNClient

# Restore dependencies
dotnet restore

# Build
dotnet build -c Release

# Run
dotnet run --project Svns.csproj
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## ⌨️ Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+N` | Checkout |
| `Ctrl+O` | Open Working Copy |
| `Ctrl+R` | Refresh Status |
| `Ctrl+K` | Commit |
| `Ctrl+U` | Update |
| `Ctrl+D` | Diff |
| `Ctrl+L` | Show Log |
| `Ctrl+H` | Blame |
| `Delete` | Delete Selected |
| `F5` | Refresh |

## 🌍 Localization

Svns supports multiple languages:

- 🇺🇸 English (en-US)
- 🇨🇳 简体中文 (zh-CN)

Add a new language by creating a `Strings.{culture}.json` file in `Resources/Strings/`.

## 🛠️ Development

### Project Structure

```
Svns/
├── Models/           # Data models and state machine
├── ViewModels/       # MVVM ViewModels
├── Views/            # Avalonia XAML views
├── Services/         # SVN operations and utilities
├── Converters/       # Value converters
├── Resources/        # Localization and assets
└── Styles/           # UI styling resources
```

### Adding Features

1. **New SVN Command**: Add to `Services/Svn/Core/SvnCommand.cs`
2. **New Operation**: Add to `Services/Svn/Operations/WorkingCopyService.cs`
3. **New Dialog**: Create View in `Views/Dialogs/` and ViewModel in `ViewModels/`

See [CLAUDE.md](CLAUDE.md) for detailed development guidelines.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- [Avalonia UI](https://avaloniaui.net/) - Cross-platform XAML-based UI framework
- [Material Icons](https://materialdesignicons.com/) - Icon set
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/) - MVVM utilities
- [Subversion](https://subversion.apache.org/) - Version control system

---

<div align="center">

**Built with ❤️ using Avalonia UI**

[Website](https://github.com/zyt-code/SVNClient) • [Issues](https://github.com/zyt-code/SVNClient/issues) • [Releases](https://github.com/zyt-code/SVNClient/releases)

</div>
