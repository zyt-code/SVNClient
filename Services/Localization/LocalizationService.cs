using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace Svns.Services.Localization;

/// <summary>
/// Localization service implementation using JSON resource files
/// </summary>
public class LocalizationService : ILocalizationService
{
    private static LocalizationService? _instance;
    private static readonly object _lock = new();

    private Dictionary<string, string> _strings = new();
    private CultureInfo _currentCulture;
    private readonly List<CultureInfo> _availableCultures;

    public static LocalizationService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new LocalizationService();
                }
            }
            return _instance;
        }
    }

    public CultureInfo CurrentCulture => _currentCulture;

    public IReadOnlyList<CultureInfo> AvailableCultures => _availableCultures;

    public event EventHandler<CultureInfo>? CultureChanged;

    private LocalizationService()
    {
        _availableCultures = new List<CultureInfo>
        {
            new CultureInfo("en-US"),
            new CultureInfo("zh-CN")
        };

        // Default to system culture or English
        var systemCulture = CultureInfo.CurrentUICulture;
        _currentCulture = _availableCultures.Find(c =>
            c.TwoLetterISOLanguageName == systemCulture.TwoLetterISOLanguageName)
            ?? _availableCultures[0];

        LoadStrings(_currentCulture);
    }

    public string GetString(string key)
    {
        if (string.IsNullOrEmpty(key))
            return string.Empty;

        return _strings.TryGetValue(key, out var value) ? value : $"[{key}]";
    }

    public string GetString(string key, params object[] args)
    {
        var format = GetString(key);
        try
        {
            return string.Format(format, args);
        }
        catch
        {
            return format;
        }
    }

    public void SetCulture(CultureInfo culture)
    {
        if (_currentCulture.Name == culture.Name)
            return;

        _currentCulture = culture;
        LoadStrings(culture);
        CultureChanged?.Invoke(this, culture);
    }

    public void SetCulture(string languageCode)
    {
        var culture = _availableCultures.Find(c =>
            c.Name.Equals(languageCode, StringComparison.OrdinalIgnoreCase) ||
            c.TwoLetterISOLanguageName.Equals(languageCode, StringComparison.OrdinalIgnoreCase));

        if (culture != null)
        {
            SetCulture(culture);
        }
    }

    private void LoadStrings(CultureInfo culture)
    {
        _strings = LoadResourceFile(culture.Name);

        // Fallback to English if the requested culture is not found
        if (_strings.Count == 0 && culture.Name != "en-US")
        {
            _strings = LoadResourceFile("en-US");
        }

        // Load embedded strings as fallback
        if (_strings.Count == 0)
        {
            _strings = GetDefaultStrings();
        }
    }

    private Dictionary<string, string> LoadResourceFile(string cultureName)
    {
        try
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var resourcePath = Path.Combine(basePath, "Resources", "Strings", $"Strings.{cultureName}.json");

            if (File.Exists(resourcePath))
            {
                var json = File.ReadAllText(resourcePath);
                return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
            }
        }
        catch
        {
            // Ignore errors
        }

        return new Dictionary<string, string>();
    }

    private Dictionary<string, string> GetDefaultStrings()
    {
        // Embedded default English strings as fallback
        return new Dictionary<string, string>
        {
            // Application
            ["App.Title"] = "Svns - SVN Client",
            ["App.Version"] = "Version",

            // Common
            ["Common.OK"] = "OK",
            ["Common.Cancel"] = "Cancel",
            ["Common.Close"] = "Close",
            ["Common.Apply"] = "Apply",
            ["Common.Save"] = "Save",
            ["Common.Delete"] = "Delete",
            ["Common.Edit"] = "Edit",
            ["Common.Add"] = "Add",
            ["Common.Remove"] = "Remove",
            ["Common.Refresh"] = "Refresh",
            ["Common.Search"] = "Search",
            ["Common.Filter"] = "Filter",
            ["Common.Yes"] = "Yes",
            ["Common.No"] = "No",
            ["Common.Error"] = "Error",
            ["Common.Warning"] = "Warning",
            ["Common.Info"] = "Information",
            ["Common.Success"] = "Success",
            ["Common.Loading"] = "Loading...",
            ["Common.Processing"] = "Processing...",
            ["Common.Browse"] = "Browse...",
            ["Common.Select"] = "Select",
            ["Common.SelectAll"] = "Select All",
            ["Common.DeselectAll"] = "Deselect All",
            ["Common.Copy"] = "Copy",
            ["Common.Paste"] = "Paste",
            ["Common.Cut"] = "Cut",

            // Main Window
            ["MainWindow.Title"] = "Svns - SVN Client",
            ["MainWindow.Menu.File"] = "File",
            ["MainWindow.Menu.Edit"] = "Edit",
            ["MainWindow.Menu.View"] = "View",
            ["MainWindow.Menu.Repository"] = "Repository",
            ["MainWindow.Menu.Help"] = "Help",
            ["MainWindow.Menu.File.Open"] = "Open Working Copy...",
            ["MainWindow.Menu.File.Checkout"] = "Checkout...",
            ["MainWindow.Menu.File.Import"] = "Import...",
            ["MainWindow.Menu.File.Settings"] = "Settings...",
            ["MainWindow.Menu.File.Exit"] = "Exit",
            ["MainWindow.Menu.Help.About"] = "About",

            // Start Page
            ["StartPage.Title"] = "Start",
            ["StartPage.OpenWorkingCopy"] = "Open Working Copy",
            ["StartPage.Checkout"] = "Checkout",
            ["StartPage.RecentProjects"] = "Recent Projects",
            ["StartPage.NoRecentProjects"] = "No recent projects",

            // Status
            ["Status.Modified"] = "Modified",
            ["Status.Added"] = "Added",
            ["Status.Deleted"] = "Deleted",
            ["Status.Unversioned"] = "Unversioned",
            ["Status.Missing"] = "Missing",
            ["Status.Conflicted"] = "Conflicted",
            ["Status.Normal"] = "Normal",
            ["Status.Ignored"] = "Ignored",
            ["Status.External"] = "External",
            ["Status.Incomplete"] = "Incomplete",
            ["Status.Merged"] = "Merged",
            ["Status.Replaced"] = "Replaced",

            // Operations
            ["Operation.Update"] = "Update",
            ["Operation.Commit"] = "Commit",
            ["Operation.Revert"] = "Revert",
            ["Operation.Add"] = "Add",
            ["Operation.Delete"] = "Delete",
            ["Operation.Rename"] = "Rename",
            ["Operation.Move"] = "Move",
            ["Operation.Copy"] = "Copy",
            ["Operation.Lock"] = "Lock",
            ["Operation.Unlock"] = "Unlock",
            ["Operation.Cleanup"] = "Cleanup",
            ["Operation.Switch"] = "Switch",
            ["Operation.Merge"] = "Merge",
            ["Operation.Branch"] = "Branch",
            ["Operation.Tag"] = "Tag",
            ["Operation.Diff"] = "Diff",
            ["Operation.Blame"] = "Blame",
            ["Operation.Log"] = "Show Log",
            ["Operation.Info"] = "Info",
            ["Operation.Properties"] = "Properties",
            ["Operation.Relocate"] = "Relocate",
            ["Operation.Export"] = "Export",
            ["Operation.ResolveConflict"] = "Resolve Conflict",

            // Commit Dialog
            ["Commit.Title"] = "Commit",
            ["Commit.Message"] = "Commit Message",
            ["Commit.MessagePlaceholder"] = "Enter commit message...",
            ["Commit.Files"] = "Files to Commit",
            ["Commit.KeepLocks"] = "Keep locks",
            ["Commit.NoFilesSelected"] = "No files selected for commit",
            ["Commit.EmptyMessage"] = "Commit message cannot be empty",
            ["Commit.Success"] = "Commit successful",
            ["Commit.Failed"] = "Commit failed",

            // Update Dialog
            ["Update.Title"] = "Update",
            ["Update.Revision"] = "Revision",
            ["Update.Head"] = "HEAD",
            ["Update.Specific"] = "Specific revision",
            ["Update.Depth"] = "Depth",
            ["Update.IgnoreExternals"] = "Ignore externals",
            ["Update.Success"] = "Update successful",
            ["Update.Failed"] = "Update failed",

            // Checkout Dialog
            ["Checkout.Title"] = "Checkout",
            ["Checkout.Url"] = "Repository URL",
            ["Checkout.Path"] = "Checkout Directory",
            ["Checkout.Revision"] = "Revision",
            ["Checkout.Depth"] = "Depth",
            ["Checkout.IgnoreExternals"] = "Ignore externals",
            ["Checkout.Success"] = "Checkout successful",
            ["Checkout.Failed"] = "Checkout failed",

            // Log/History
            ["Log.Title"] = "Log",
            ["Log.Revision"] = "Revision",
            ["Log.Author"] = "Author",
            ["Log.Date"] = "Date",
            ["Log.Message"] = "Message",
            ["Log.ChangedPaths"] = "Changed Paths",
            ["Log.ShowAll"] = "Show all",
            ["Log.StopOnCopy"] = "Stop on copy",
            ["Log.IncludeMerged"] = "Include merged revisions",

            // Diff
            ["Diff.Title"] = "Diff",
            ["Diff.Base"] = "Base",
            ["Diff.Working"] = "Working",
            ["Diff.Revision"] = "Revision",
            ["Diff.Unified"] = "Unified diff",
            ["Diff.SideBySide"] = "Side by side",

            // Blame
            ["Blame.Title"] = "Blame",
            ["Blame.Line"] = "Line",
            ["Blame.Revision"] = "Revision",
            ["Blame.Author"] = "Author",
            ["Blame.Date"] = "Date",
            ["Blame.Content"] = "Content",

            // Branch/Tag
            ["BranchTag.Title"] = "Branch/Tag",
            ["BranchTag.CreateBranch"] = "Create Branch",
            ["BranchTag.CreateTag"] = "Create Tag",
            ["BranchTag.Source"] = "Source",
            ["BranchTag.Target"] = "Target URL",
            ["BranchTag.Message"] = "Log Message",
            ["BranchTag.Success"] = "Branch/Tag created successfully",
            ["BranchTag.Failed"] = "Failed to create Branch/Tag",

            // Merge
            ["Merge.Title"] = "Merge",
            ["Merge.Source"] = "Source",
            ["Merge.Revisions"] = "Revisions",
            ["Merge.AllRevisions"] = "All revisions",
            ["Merge.SpecificRevisions"] = "Specific revisions",
            ["Merge.DryRun"] = "Dry run",
            ["Merge.IgnoreAncestry"] = "Ignore ancestry",
            ["Merge.Success"] = "Merge successful",
            ["Merge.Failed"] = "Merge failed",

            // Switch
            ["Switch.Title"] = "Switch",
            ["Switch.Url"] = "Switch to URL",
            ["Switch.Revision"] = "Revision",
            ["Switch.Success"] = "Switch successful",
            ["Switch.Failed"] = "Switch failed",

            // Cleanup
            ["Cleanup.Title"] = "Cleanup",
            ["Cleanup.RemoveUnversioned"] = "Remove unversioned files",
            ["Cleanup.RemoveIgnored"] = "Remove ignored files",
            ["Cleanup.VacuumPristines"] = "Vacuum pristines",
            ["Cleanup.Success"] = "Cleanup successful",
            ["Cleanup.Failed"] = "Cleanup failed",

            // Lock
            ["Lock.Title"] = "Lock",
            ["Lock.Comment"] = "Comment",
            ["Lock.StealLock"] = "Steal lock",
            ["Lock.Success"] = "Lock successful",
            ["Lock.Failed"] = "Lock failed",
            ["Unlock.Success"] = "Unlock successful",
            ["Unlock.Failed"] = "Unlock failed",

            // Conflict
            ["Conflict.Title"] = "Resolve Conflict",
            ["Conflict.UseBase"] = "Use base",
            ["Conflict.UseMine"] = "Use mine",
            ["Conflict.UseTheirs"] = "Use theirs",
            ["Conflict.MarkResolved"] = "Mark as resolved",
            ["Conflict.Success"] = "Conflict resolved",
            ["Conflict.Failed"] = "Failed to resolve conflict",

            // Info
            ["Info.Title"] = "Info",
            ["Info.Path"] = "Path",
            ["Info.Url"] = "URL",
            ["Info.RepositoryRoot"] = "Repository Root",
            ["Info.RepositoryUuid"] = "Repository UUID",
            ["Info.Revision"] = "Revision",
            ["Info.NodeKind"] = "Node Kind",
            ["Info.Schedule"] = "Schedule",
            ["Info.LastChangedAuthor"] = "Last Changed Author",
            ["Info.LastChangedRev"] = "Last Changed Rev",
            ["Info.LastChangedDate"] = "Last Changed Date",

            // Properties
            ["Properties.Title"] = "Properties",
            ["Properties.Name"] = "Name",
            ["Properties.Value"] = "Value",
            ["Properties.Add"] = "Add Property",
            ["Properties.Edit"] = "Edit Property",
            ["Properties.Delete"] = "Delete Property",

            // Settings
            ["Settings.Title"] = "Settings",
            ["Settings.General"] = "General",
            ["Settings.Appearance"] = "Appearance",
            ["Settings.Language"] = "Language",
            ["Settings.Theme"] = "Theme",
            ["Settings.Theme.Light"] = "Light",
            ["Settings.Theme.Dark"] = "Dark",
            ["Settings.Theme.System"] = "System",
            ["Settings.SvnPath"] = "SVN Path",
            ["Settings.SvnPath.Description"] = "Path to SVN executable",
            ["Settings.DefaultRepository"] = "Default Repository URL",
            ["Settings.Apply"] = "Apply",
            ["Settings.Reset"] = "Reset to Defaults",

            // Repository Browser
            ["RepoBrowser.Title"] = "Repository Browser",
            ["RepoBrowser.Url"] = "URL",
            ["RepoBrowser.Go"] = "Go",
            ["RepoBrowser.Up"] = "Up",
            ["RepoBrowser.Refresh"] = "Refresh",

            // Relocate
            ["Relocate.Title"] = "Relocate",
            ["Relocate.From"] = "From URL",
            ["Relocate.To"] = "To URL",
            ["Relocate.Success"] = "Relocate successful",
            ["Relocate.Failed"] = "Relocate failed",

            // Import
            ["Import.Title"] = "Import",
            ["Import.Path"] = "Path",
            ["Import.Url"] = "Repository URL",
            ["Import.Message"] = "Log Message",
            ["Import.Success"] = "Import successful",
            ["Import.Failed"] = "Import failed",

            // Export
            ["Export.Title"] = "Export",
            ["Export.Url"] = "URL",
            ["Export.Path"] = "Export to",
            ["Export.Revision"] = "Revision",
            ["Export.Success"] = "Export successful",
            ["Export.Failed"] = "Export failed",

            // About
            ["About.Title"] = "About Svns",
            ["About.Description"] = "A modern SVN client built with Avalonia UI",
            ["About.License"] = "License",
            ["About.Website"] = "Website",

            // Errors
            ["Error.Generic"] = "An error occurred",
            ["Error.Connection"] = "Connection error",
            ["Error.Authentication"] = "Authentication failed",
            ["Error.NotWorkingCopy"] = "Not a working copy",
            ["Error.PathNotFound"] = "Path not found",
            ["Error.InvalidUrl"] = "Invalid URL",
            ["Error.OperationFailed"] = "Operation failed",
            ["Error.WindowNotInitialized"] = "Error: Window not initialized",
            ["Error.NoWorkingCopyLoaded"] = "No working copy loaded",
            ["Error.NoFileSelected"] = "No file selected",
            ["Error.NoChangesToCommit"] = "No changes to commit",
            ["Error.NoDifferencesFound"] = "No differences found",
            ["Error.PleaseSelectAFile"] = "Please select a file",
            ["Error.PleaseEnterAValue"] = "Please enter a value",

            // Status Messages
            ["Status.Ready"] = "Ready",
            ["Status.Loading"] = "Loading...",
            ["Status.LoadingHistory"] = "Loading history...",
            ["Status.LoadingConflicts"] = "Loading conflicts...",
            ["Status.LoadingChanges"] = "Loading changes...",
            ["Status.LoadingBlame"] = "Loading blame information...",
            ["Status.LoadingProperties"] = "Loading properties...",
            ["Status.LoadingUrl"] = "Loading current URL...",
            ["Status.LoadingSources"] = "Loading available sources...",
            ["Status.LoadingBranches"] = "Loading branches...",
            ["Status.CommitSuccessful"] = "Commit successful!",
            ["Status.UpdateCompleted"] = "Update completed successfully",
            ["Status.UpdateWorkingCopy"] = "Updating working copy...",
            ["Status.RevertCancelled"] = "Revert cancelled",
            ["Status.DeleteCancelled"] = "Delete cancelled",
            ["Status.CheckoutCompleted"] = "Checkout completed successfully",
            ["Status.SwitchCompleted"] = "Switch completed successfully!",
            ["Status.MergeCompleted"] = "Merge completed successfully!",
            ["Status.AllConflictsResolved"] = "All conflicts resolved!",
            ["Status.NoConflictsToResolve"] = "No conflicts to resolve",
            ["Status.CreatingNewFolder"] = "Creating new folder...",
            ["Status.RunningMergePreview"] = "Running merge preview...",
            ["Status.MergingChanges"] = "Merging changes...",
            ["Status.PreviewCompleted"] = "Preview completed",
            ["Status.PreviewFailed"] = "Preview failed",
            ["Status.Renaming"] = "Renaming...",
            ["Status.RenameSuccessful"] = "Rename successful!",
            ["Status.RenameFailed"] = "Rename failed",
            ["Status.SwitchingWorkingCopy"] = "Switching working copy...",
            ["Status.RelocatingWorkingCopy"] = "Relocating working copy...",
            ["Status.WorkingCopyRelocated"] = "Working copy relocated successfully!",
            ["Status.NewUrlSameAsCurrent"] = "New URL is the same as the current URL",
            ["Status.PleaseEnterNewUrl"] = "Please enter a new repository URL",
            ["Status.PleaseSelectAFileToBlame"] = "Please select a file to blame",
            ["Status.PleaseSelectAFileOrFolder"] = "Please select a file or folder",
            ["Status.PleaseSelectAFileOrFolderToRename"] = "Please select a file or folder to rename",
            ["Status.PleaseSelectAFileToLock"] = "Please select a file to lock",
            ["Status.FileOrDirectoryDoesNotExist"] = "File or directory does not exist",
            ["Status.PropertyAlreadyExists"] = "Property already exists",
            ["Status.NoPropertySelected"] = "No property selected",
            ["Status.NoChangesToSave"] = "No changes to save",
            ["Status.PropertyAdded"] = "Property added successfully",
            ["Status.PropertySaved"] = "Property saved successfully",
            ["Status.PropertyDeleted"] = "Property deleted successfully",
            ["Status.PleaseEnterAPropertyName"] = "Please enter a property name",
            ["Status.CleanupCompleted"] = "Cleanup completed successfully!",
            ["Status.CleanupFailed"] = "Cleanup failed",
            ["Status.RunningCleanup"] = "Running cleanup...",
            ["Status.RemovingUnversionedFiles"] = "Removing unversioned files...",
            ["Status.RemovingIgnoredFiles"] = "Removing ignored files...",
            ["Status.ImportingFiles"] = "Importing files...",
            ["Status.FilesImportedSuccessfully"] = "Files imported successfully!",
            ["Status.PleaseSelectLocalPath"] = "Please select a local path",
            ["Status.PleaseEnterRepositoryUrl"] = "Please enter a repository URL",
            ["Status.PleaseEnterCommitMessage"] = "Please enter a commit message",
            ["Status.LockFailed"] = "Lock failed",
            ["Status.UnlockFailed"] = "Unlock failed",
            ["Status.CommittingChanges"] = "Committing changes...",
            ["Status.PleaseEnterOrPasteLocalPath"] = "Please enter or paste the local path",

            // Blame
            ["Blame.Loaded"] = "Blame loaded: {0} lines, {1} revisions, {2} authors",
            ["Blame.ErrorLoading"] = "Error loading blame: {0}",

            // History
            ["History.LoadedRevisions"] = "Loaded {0} revisions",
            ["History.ErrorLoading"] = "Error loading history: {0}",

            // Commit
            ["Commit.FilesWithChanges"] = "{0} file(s) with changes",
            ["Commit.ErrorLoading"] = "Error loading changes: {0}",
            ["Commit.FailedWithMessage"] = "Commit failed: {0}",
            ["Commit.ErrorMessage"] = "Error: {0}"
        };
    }
}
