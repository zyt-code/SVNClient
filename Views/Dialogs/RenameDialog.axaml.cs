using Avalonia.Controls;
using Svns.ViewModels;

namespace Svns.Views.Dialogs;

public partial class RenameDialog : Window
{
    public RenameDialog()
    {
        InitializeComponent();
    }

    public RenameDialog(RenameViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += (_, _) => Close();
    }
}
