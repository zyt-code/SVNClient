using System;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Svns.ViewModels;

/// <summary>
/// ViewModel for the About dialog
/// </summary>
public partial class AboutViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _appName = "Svns";

    [ObservableProperty]
    private string _version = GetAssemblyVersion();

    [ObservableProperty]
    private string _description = "Modern SVN Client for cross-platform version control";

    [ObservableProperty]
    private string _copyright = "Copyright © 2024";

    [ObservableProperty]
    private string _companyName = "Svns Project";

    [ObservableProperty]
    private string _websiteUrl = "https://github.com/yourusername/svns";

    [ObservableProperty]
    private string _license = "MIT License";

    [ObservableProperty]
    private string _licenseText = "Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the \"Software\"), to deal in the Software without restriction.";

    [ObservableProperty]
    private string _dotNetVersion = ".NET 9.0";

    [ObservableProperty]
    private string _avaloniaVersion = "11.3.10";

    [ObservableProperty]
    private int _testCount = 516;

    [ObservableProperty]
    private int _svnCommandsCount = 29;

    /// <summary>
    /// Event raised when dialog should be closed
    /// </summary>
    public event EventHandler? CloseRequested;

    /// <summary>
    /// Gets the full version string for display
    /// </summary>
    public string FullVersion => $"Version {Version}";

    /// <summary>
    /// Gets the build date
    /// </summary>
    public string BuildDate => GetBuildDate();

    private static string GetAssemblyVersion()
    {
        return Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";
    }

    private static string GetBuildDate()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var buildDate = assembly.GetCustomAttribute<BuildDateAttribute>()?.Date
            ?? System.IO.File.GetLastWriteTime(assembly.Location);
        return buildDate.ToString("yyyy-MM-dd");
    }

    /// <summary>
    /// Opens the website URL
    /// </summary>
    [RelayCommand]
    private void OpenWebsite()
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = WebsiteUrl,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        }
        catch
        {
            // Ignore errors opening URL
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
/// Attribute to store build date
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
internal class BuildDateAttribute : Attribute
{
    public DateTime Date { get; }

    public BuildDateAttribute(string date)
    {
        Date = DateTime.Parse(date);
    }
}
