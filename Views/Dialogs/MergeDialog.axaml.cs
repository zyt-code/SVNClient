using Avalonia.Controls;
using Svns.ViewModels;

namespace Svns.Views.Dialogs;

public partial class MergeDialog : Window
{
    public MergeDialog()
    {
        InitializeComponent();
    }

    public MergeDialog(MergeViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += (_, success) =>
        {
            Close(success);
        };
    }
}
