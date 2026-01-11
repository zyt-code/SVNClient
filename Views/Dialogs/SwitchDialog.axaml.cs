using Avalonia.Controls;
using Svns.ViewModels;

namespace Svns.Views.Dialogs;

public partial class SwitchDialog : Window
{
    public SwitchDialog()
    {
        InitializeComponent();
    }

    public SwitchDialog(SwitchViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += (_, success) =>
        {
            Close(success);
        };
    }
}
