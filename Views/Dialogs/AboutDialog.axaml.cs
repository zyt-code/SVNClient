using Avalonia.Controls;
using Svns.ViewModels;

namespace Svns.Views.Dialogs;

/// <summary>
/// About dialog showing application information
/// </summary>
public partial class AboutDialog : Window
{
    public AboutDialog()
    {
        InitializeComponent();
    }

    public AboutDialog(AboutViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += (_, _) => Close();
    }
}
