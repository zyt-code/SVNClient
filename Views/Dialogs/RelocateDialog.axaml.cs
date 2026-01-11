using Avalonia.Controls;
using Svns.ViewModels;

namespace Svns.Views.Dialogs;

public partial class RelocateDialog : Window
{
    public RelocateDialog()
    {
        InitializeComponent();
    }

    public RelocateDialog(RelocateViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += (_, _) => Close();

        // Load current URL when dialog opens
        Opened += async (_, _) => await viewModel.LoadCurrentUrlCommand.ExecuteAsync(null);
    }
}
