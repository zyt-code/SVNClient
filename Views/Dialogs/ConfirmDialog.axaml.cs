using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Svns.Views.Dialogs;

public partial class ConfirmDialog : Window
{
    public bool Result { get; private set; }

    public ConfirmDialog()
    {
        InitializeComponent();
    }

    public ConfirmDialog(string title, string message, string confirmText = "Confirm", string cancelText = "Cancel") : this()
    {
        TitleText.Text = title;
        MessageText.Text = message;
        ConfirmButton.Content = confirmText;
        CancelButton.Content = cancelText;
        Title = title;
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
    /// Shows a confirmation dialog and returns the result
    /// </summary>
    public static async Task<bool> ShowAsync(Window owner, string title, string message, string confirmText = "Confirm", string cancelText = "Cancel")
    {
        var dialog = new ConfirmDialog(title, message, confirmText, cancelText);
        await dialog.ShowDialog(owner);
        return dialog.Result;
    }
}
