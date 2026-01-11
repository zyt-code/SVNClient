using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Svns.ViewModels;

namespace Svns.Views.Dialogs;

public partial class ImportDialog : Window
{
    public ImportDialog()
    {
        InitializeComponent();
    }

    public ImportDialog(ImportViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += (_, _) => Close();
        viewModel.BrowseFolderRequested += OnBrowseFolderRequested;
    }

    private async void OnBrowseFolderRequested(object? sender, System.EventArgs e)
    {
        var viewModel = DataContext as ImportViewModel;
        if (viewModel == null) return;

        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Folder to Import",
            AllowMultiple = false
        });

        if (folders.Count > 0)
        {
            viewModel.LocalPath = folders[0].Path.LocalPath;
        }
    }
}
