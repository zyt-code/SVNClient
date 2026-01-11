using Avalonia.Controls;
using Svns.ViewModels;

namespace Svns.Views.Dialogs;

public partial class CheckoutDialog : Window
{
    public CheckoutDialog()
    {
        InitializeComponent();
    }

    public CheckoutDialog(CheckoutViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += (_, success) =>
        {
            Close(success);
        };
    }
}
