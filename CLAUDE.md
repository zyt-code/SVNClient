# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Svns is a modern SVN client built with .NET 9 and Avalonia UI. It follows a single-working-copy model (similar to IntelliJ IDEA) where users open one SVN working copy at a time. The project supports all standard SVN commands through a command-line interface wrapper.

## Build and Test Commands

### Build
```bash
dotnet build Svns.sln
```

### Run
```bash
dotnet run --project Svns/Svns.csproj
```

### Run Tests
```bash
# Run all tests
dotnet test Svns.Tests

# Run specific test category
dotnet test Svns.Tests --filter "FullyQualifiedName~Converters"
dotnet test Svns.Tests --filter "FullyQualifiedName~Services"
dotnet test Svns.Tests --filter "FullyQualifiedName~ViewModels"

# Run specific test class
dotnet test Svns.Tests --filter "ClassName=MainWindowViewModelTests"

# With coverage
dotnet test Svns.Tests --collect:"XPlat Code Coverage"

# Detailed output
dotnet test Svns.Tests --verbosity detailed
```

### Solution Configuration
- Main project targets **.NET 9.0**
- Test project targets **.NET 10.0**
- Uses **xUnit** for testing with 700+ tests

## Architecture

### Layer Structure

```
Services/
├── Svn/
│   ├── Core/           # SVN command execution (SvnCommandService, SvnCommand, SvnResult, SvnException)
│   ├── Parsers/        # XML output parsers (Status, Log, Info, Diff, List)
│   └── Operations/     # High-level operations (WorkingCopyService - main facade)
├── Localization/       # i18n service (JSON-based, en-US and zh-CN)
├── ClipboardService.cs
├── AppSettingsService.cs
└── SvnLogCacheService.cs

Models/                 # Data models (SvnStatus, SvnInfo, SvnLogEntry, etc.)

ViewModels/             # MVVM ViewModels using CommunityToolkit.Mvvm source generators

Views/                  # Avalonia XAML views
Views/Dialogs/          # Dialog windows

Converters/             # Avalonia value converters

Utils/                  # Helper classes (PathHelper, SvnPathHelper, ProcessHelper)

Constants/              # Constants (SvnConstants with status icons/colors)

Resources/Strings/      # Localization JSON files (Strings.en-US.json, Strings.zh-CN.json)

Styles/                 # XAML styling resources
```

### Key Design Patterns

1. **SVN Command Execution**: `SvnCommandService` executes `svn.exe` via `Process.Start()`, using `--xml` flag for structured output parsed by dedicated parser classes.

2. **MVVM with Source Generators**: Uses `CommunityToolkit.Mvvm` with `[ObservableProperty]` and `[RelayCommand]` attributes - do not manually add property change notification or command boilerplate.

3. **ViewModel-View Naming Convention**: `ViewLocator` auto-maps ViewModels to Views by replacing "ViewModel" with "View" in the type name.

4. **Localization**: All UI strings must use `Localize.Get("Category.Key")` or XAML markup extension `{i18n:Localize Category.Key}`. Keys use dot notation (e.g., `Common.OK`, `MainWindow.Menu.File`).

5. **Service Facade**: `WorkingCopyService` provides high-level operations that combine command execution and parsing.

## Working with SVN Commands

To add a new SVN command:

1. Add static factory method to `SvnCommand` class (Services/Svn/Core/SvnCommand.cs)
2. Add operation method to `WorkingCopyService` if needed (Services/Svn/Operations/WorkingCopyService.cs)
3. Create parser in `Services/Svn/Parsers/` if XML output needs parsing
4. Add ViewModel if UI is needed
5. Add corresponding View in `Views/Dialogs/`

Example command structure:
```csharp
// In SvnCommand.cs
public static SvnCommand MyCommand(string path, bool someFlag)
{
    return new SvnCommand("mycommand", someFlag ? "--flag" : "", path);
}

// In WorkingCopyService.cs
public async Task<SvnResult> MyOperationAsync(string path, bool someFlag, CancellationToken ct = default)
{
    var command = SvnCommand.MyCommand(path, someFlag);
    return await _commandService.ExecuteAsync(command, ct);
}
```

## Localization

- Resource files: `Resources/Strings/Strings.{culture}.json`
- Supported languages: `en-US` (default), `zh-CN`
- In code: `Localize.Get("Category.Subcategory.Key")` or `Localize.T("Key")`
- In XAML: `{i18n:Localize Category.Key}`
- Adding new language: Create JSON file, register in `LocalizationService.cs` and `SettingsViewModel.cs`

## Status Types and Visuals

Status icons are defined in `Constants/SvnConstants.cs`:
- ✓ Normal (green)
- ✎ Modified (blue)
- + Added (cyan)
- − Deleted (red)
- ⚠ Conflicted (orange)
- ? Unversioned (gray)
- ! Missing (red)

## Common File Patterns

- **ViewModels**: Extend `ViewModelBase`, use `[ObservableProperty]` and `[RelayCommand]`
- **Views**: AXAML with code-behind `.axaml.cs` if needed
- **Converters**: Implement `IValueConverter`
- **Models**: Plain classes with properties

## Test Naming Convention

Tests follow `MethodName_ExpectedResult_Condition` pattern:
```csharp
[Fact]
public void Parse_ReturnsEmptyResult_WhenOutputIsEmpty() { }
```

## Important Notes

- The app requires `svn.exe` to be installed and available in PATH
- All SVN operations are async and accept `CancellationToken`
- Settings are persisted to `%LocalAppData%/Svns/svns-settings.json`
- Compiled bindings are enabled by default (`AvaloniaUseCompiledBindingsByDefault=true`)
- Nullable reference types are enabled
