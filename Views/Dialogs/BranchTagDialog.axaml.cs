using Avalonia.Controls;
using Svns.ViewModels;

namespace Svns.Views.Dialogs;

public partial class BranchTagDialog : Window
{
    public BranchTagDialog()
    {
        InitializeComponent();
    }

    public BranchTagDialog(BranchTagViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += (_, success) =>
        {
            Close(success);
        };
    }
}
