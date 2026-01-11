using Avalonia.Controls;
using Svns.ViewModels;

namespace Svns.Views.Dialogs;

public partial class PropertyDialog : Window
{
    public PropertyDialog()
    {
        InitializeComponent();
    }

    public PropertyDialog(PropertyViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += (_, _) => Close();
    }
}
