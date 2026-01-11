using Avalonia.Controls;
using Svns.ViewModels;

namespace Svns.Views.Dialogs;

public partial class BlameDialog : Window
{
    public BlameDialog()
    {
        InitializeComponent();
    }

    public BlameDialog(BlameViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += (_, _) => Close();
    }
}
