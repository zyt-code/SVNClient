using Xunit;
using Svns.ViewModels;

namespace Svns.Tests.ViewModels;

public class AboutViewModelTests
{
    [Fact]
    public void Constructor_InitializesProperties()
    {
        var vm = new AboutViewModel();

        Assert.Equal("Svns", vm.AppName);
        Assert.NotNull(vm.Version);
        Assert.NotEmpty(vm.Version);
        Assert.Equal("Modern SVN Client for cross-platform version control", vm.Description);
        Assert.Equal("Copyright © 2024", vm.Copyright);
        Assert.Equal("Svns Project", vm.CompanyName);
        Assert.NotNull(vm.WebsiteUrl);
        Assert.NotEmpty(vm.WebsiteUrl);
        Assert.Equal("MIT License", vm.License);
        Assert.NotNull(vm.LicenseText);
        Assert.NotEmpty(vm.LicenseText);
        Assert.Equal(".NET 9.0", vm.DotNetVersion);
        Assert.NotNull(vm.AvaloniaVersion);
        Assert.NotEmpty(vm.AvaloniaVersion);
    }

    [Fact]
    public void FullVersion_ReturnsVersionWithPrefix()
    {
        var vm = new AboutViewModel();

        Assert.StartsWith("Version ", vm.FullVersion);
        Assert.Contains(vm.Version, vm.FullVersion);
    }

    [Fact]
    public void BuildDate_ReturnsValidDate()
    {
        var vm = new AboutViewModel();

        Assert.NotNull(vm.BuildDate);
        Assert.Matches(@"\d{4}-\d{2}-\d{2}", vm.BuildDate);
    }

    [Fact]
    public void CloseRequested_Event_CanBeNull()
    {
        var vm = new AboutViewModel();
        var eventInfo = typeof(AboutViewModel).GetEvent("CloseRequested");
        Assert.NotNull(eventInfo);
    }

    [Fact]
    public void CloseCommand_Exists()
    {
        var vm = new AboutViewModel();
        var propertyInfo = typeof(AboutViewModel).GetProperty("CloseCommand");
        Assert.NotNull(propertyInfo);
    }

    [Fact]
    public void OpenWebsiteCommand_Exists()
    {
        var vm = new AboutViewModel();
        var propertyInfo = typeof(AboutViewModel).GetProperty("OpenWebsiteCommand");
        Assert.NotNull(propertyInfo);
    }

    [Fact]
    public void CloseRequested_CanBeRaised()
    {
        var vm = new AboutViewModel();
        bool eventRaised = false;
        vm.CloseRequested += (s, e) => eventRaised = true;

        // Trigger Close through reflection since it's a private method with RelayCommand
        var closeMethod = typeof(AboutViewModel).GetMethod("Close", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        closeMethod?.Invoke(vm, null);

        Assert.True(eventRaised);
    }

    [Theory]
    [InlineData("AppName")]
    [InlineData("Version")]
    [InlineData("Description")]
    [InlineData("Copyright")]
    [InlineData("CompanyName")]
    [InlineData("WebsiteUrl")]
    [InlineData("License")]
    [InlineData("LicenseText")]
    [InlineData("DotNetVersion")]
    [InlineData("AvaloniaVersion")]
    [InlineData("TestCount")]
    [InlineData("SvnCommandsCount")]
    public void Properties_AreObservable(string propertyName)
    {
        var propertyInfo = typeof(AboutViewModel).GetProperty(propertyName);
        Assert.NotNull(propertyInfo);
    }
}
