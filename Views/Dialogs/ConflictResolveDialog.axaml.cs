using Avalonia.Controls;
using Svns.ViewModels;

namespace Svns.Views.Dialogs;

public partial class ConflictResolveDialog : Window
{
    public ConflictResolveDialog()
    {
        InitializeComponent();
    }

    public ConflictResolveDialog(ConflictResolveViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += (_, success) =>
        {
            Close(success);
        };
    }
}
