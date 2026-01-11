using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Svns.Services.Svn.Operations;

namespace Svns.ViewModels;

/// <summary>
/// ViewModel for the Property dialog
/// </summary>
public partial class PropertyViewModel : ViewModelBase
{
    private readonly WorkingCopyService _workingCopyService;
    private readonly string _filePath;

    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    private string _fullPath = string.Empty;

    [ObservableProperty]
    private ObservableCollection<PropertyItem> _properties = new();

    [ObservableProperty]
    private PropertyItem? _selectedProperty;

    [ObservableProperty]
    private string _newPropertyName = string.Empty;

    [ObservableProperty]
    private string _newPropertyValue = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    /// <summary>
    /// Common SVN property names
    /// </summary>
    public string[] CommonProperties { get; } = new[]
    {
        "svn:ignore",
        "svn:eol-style",
        "svn:mime-type",
        "svn:keywords",
        "svn:executable",
        "svn:needs-lock",
        "svn:externals",
        "svn:mergeinfo"
    };

    /// <summary>
    /// Common EOL styles
    /// </summary>
    public string[] EolStyles { get; } = new[]
    {
        "native",
        "CRLF",
        "LF",
        "CR"
    };

    /// <summary>
    /// Event raised when dialog should be closed
    /// </summary>
    public event EventHandler? CloseRequested;

    public PropertyViewModel(WorkingCopyService workingCopyService, string filePath)
    {
        _workingCopyService = workingCopyService;
        _filePath = filePath;
        FileName = System.IO.Path.GetFileName(filePath);
        FullPath = filePath;
    }

    /// <summary>
    /// Loads the properties for the file
    /// </summary>
    [RelayCommand]
    public async Task LoadPropertiesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading properties...";

            var props = await _workingCopyService.GetPropertiesAsync(_filePath);

            Properties.Clear();
            foreach (var kvp in props.OrderBy(p => p.Key))
            {
                Properties.Add(new PropertyItem
                {
                    Name = kvp.Key,
                    Value = kvp.Value,
                    OriginalValue = kvp.Value
                });
            }

            StatusMessage = $"Loaded {Properties.Count} property(s)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading properties: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Adds a new property
    /// </summary>
    [RelayCommand]
    private async Task AddPropertyAsync()
    {
        if (string.IsNullOrWhiteSpace(NewPropertyName))
        {
            StatusMessage = "Please enter a property name";
            return;
        }

        if (Properties.Any(p => p.Name == NewPropertyName))
        {
            StatusMessage = "Property already exists";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = $"Adding property {NewPropertyName}...";

            var result = await _workingCopyService.SetPropertyAsync(_filePath, NewPropertyName, NewPropertyValue);

            if (result.Success)
            {
                Properties.Add(new PropertyItem
                {
                    Name = NewPropertyName,
                    Value = NewPropertyValue,
                    OriginalValue = NewPropertyValue
                });

                NewPropertyName = string.Empty;
                NewPropertyValue = string.Empty;
                StatusMessage = "Property added successfully";
            }
            else
            {
                StatusMessage = $"Failed to add property: {result.StandardError}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error adding property: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Saves changes to the selected property
    /// </summary>
    [RelayCommand]
    private async Task SavePropertyAsync()
    {
        if (SelectedProperty == null)
        {
            StatusMessage = "No property selected";
            return;
        }

        if (SelectedProperty.Value == SelectedProperty.OriginalValue)
        {
            StatusMessage = "No changes to save";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = $"Saving property {SelectedProperty.Name}...";

            var result = await _workingCopyService.SetPropertyAsync(
                _filePath, SelectedProperty.Name, SelectedProperty.Value);

            if (result.Success)
            {
                SelectedProperty.OriginalValue = SelectedProperty.Value;
                SelectedProperty.IsModified = false;
                StatusMessage = "Property saved successfully";
            }
            else
            {
                StatusMessage = $"Failed to save property: {result.StandardError}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving property: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Deletes the selected property
    /// </summary>
    [RelayCommand]
    private async Task DeletePropertyAsync()
    {
        if (SelectedProperty == null)
        {
            StatusMessage = "No property selected";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = $"Deleting property {SelectedProperty.Name}...";

            var result = await _workingCopyService.DeletePropertyAsync(_filePath, SelectedProperty.Name);

            if (result.Success)
            {
                Properties.Remove(SelectedProperty);
                SelectedProperty = null;
                StatusMessage = "Property deleted successfully";
            }
            else
            {
                StatusMessage = $"Failed to delete property: {result.StandardError}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error deleting property: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Closes the dialog
    /// </summary>
    [RelayCommand]
    private void Close()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}

/// <summary>
/// Represents a property item
/// </summary>
public partial class PropertyItem : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _value = string.Empty;

    [ObservableProperty]
    private string _originalValue = string.Empty;

    [ObservableProperty]
    private bool _isModified;

    partial void OnValueChanged(string value)
    {
        IsModified = value != OriginalValue;
    }
}
