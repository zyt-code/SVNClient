using Avalonia.Controls;
using Svns.ViewModels;

namespace Svns.Views.Dialogs;

public partial class LockDialog : Window
{
    public LockDialog()
    {
        InitializeComponent();
    }

    public LockDialog(LockViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += (_, _) => Close();
    }
}
