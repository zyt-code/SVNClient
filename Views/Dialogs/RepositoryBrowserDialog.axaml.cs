using Avalonia.Controls;
using Svns.ViewModels;

namespace Svns.Views.Dialogs;

public partial class RepositoryBrowserDialog : Window
{
    public string? SelectedPath { get; private set; }

    public RepositoryBrowserDialog()
    {
        InitializeComponent();
    }

    public RepositoryBrowserDialog(RepositoryBrowserViewModel viewModel) : this()
    {
        DataContext = viewModel;

        viewModel.ItemSelected += (_, path) =>
        {
            SelectedPath = path;
            Close(path);
        };

        viewModel.CloseRequested += (_, _) =>
        {
            Close(null);
        };
    }
}
