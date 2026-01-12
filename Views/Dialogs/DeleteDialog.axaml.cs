using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Svns.Models;
using Svns.ViewModels;

namespace Svns.Views.Dialogs;

public partial class DeleteDialog : Window
{
    public bool Result { get; private set; }

    public DeleteDialog()
    {
        InitializeComponent();
    }

    public DeleteDialog(string title, IEnumerable<SvnStatus> files) : this()
    {
        Title = title;
        DataContext = new DeleteDialogViewModel(files);
    }

    private void OnConfirmClick(object? sender, RoutedEventArgs e)
    {
        Result = true;
        Close();
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Result = false;
        Close();
    }

    /// <summary>
    /// Shows a delete confirmation dialog and returns the result
    /// </summary>
    public static async Task<bool> ShowAsync(Window owner, IEnumerable<SvnStatus> files)
    {
        var count = files.Count();
        var title = count == 1 ? "Delete File" : $"Delete {count} Files";
        var dialog = new DeleteDialog(title, files);
        await dialog.ShowDialog(owner);
        return dialog.Result;
    }
}
