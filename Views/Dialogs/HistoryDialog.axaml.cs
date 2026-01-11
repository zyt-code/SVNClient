using Avalonia.Controls;
using Svns.ViewModels;

namespace Svns.Views.Dialogs;

public partial class HistoryDialog : Window
{
    public HistoryDialog()
    {
        InitializeComponent();
    }

    public HistoryDialog(HistoryViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += (_, _) => Close();
    }
}
