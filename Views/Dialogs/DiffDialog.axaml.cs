using Avalonia.Controls;
using Svns.ViewModels;

namespace Svns.Views.Dialogs;

public partial class DiffDialog : Window
{
    public DiffDialog()
    {
        InitializeComponent();
    }

    public DiffDialog(DiffViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += (_, _) => Close();
    }
}
