using Avalonia.Controls;
using Svns.ViewModels;

namespace Svns.Views.Dialogs;

public partial class CleanupDialog : Window
{
    public CleanupDialog()
    {
        InitializeComponent();
    }

    public CleanupDialog(CleanupViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += (_, _) => Close();
    }
}
