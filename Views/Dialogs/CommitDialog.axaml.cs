using Avalonia.Controls;
using Svns.ViewModels;

namespace Svns.Views.Dialogs;

public partial class CommitDialog : Window
{
    public CommitDialog()
    {
        InitializeComponent();
    }

    public CommitDialog(CommitViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += (_, _) => Close();
    }
}
