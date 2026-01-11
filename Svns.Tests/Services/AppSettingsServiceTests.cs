using Xunit;
using Svns.Services;

namespace Svns.Tests.Services;

public class AppSettingsServiceTests
{
    [Fact]
    public void WindowSettings_HasDefaultValues()
    {
        var settings = new AppSettingsService.WindowSettings();

        Assert.Equal(1200, settings.Width);
        Assert.Equal(700, settings.Height);
        Assert.Equal(0, settings.X);
        Assert.Equal(0, settings.Y);
        Assert.False(settings.IsMaximized);
    }

    [Fact]
    public void AppSettings_HasDefaultValues()
    {
        var settings = new AppSettingsService.AppSettings();

        Assert.Equal(string.Empty, settings.LastWorkingCopy);
        Assert.Equal("System", settings.Theme);
        Assert.Equal("svn", settings.SvnPath);
        Assert.Equal(string.Empty, settings.DefaultRepositoryUrl);
        Assert.NotNull(settings.RecentProjects);
        Assert.Empty(settings.RecentProjects);
        Assert.NotNull(settings.Window);
    }

    [Fact]
    public void RecentProject_HasDefaultValues()
    {
        var project = new AppSettingsService.RecentProject();

        Assert.Equal(string.Empty, project.Name);
        Assert.Equal(string.Empty, project.Path);
    }

    [Theory]
    [InlineData("Light")]
    [InlineData("Dark")]
    [InlineData("System")]
    public void Theme_CanBeSet(string theme)
    {
        var settings = new AppSettingsService.AppSettings
        {
            Theme = theme
        };

        Assert.Equal(theme, settings.Theme);
    }

    [Fact]
    public void WindowSettings_CanBeModified()
    {
        var settings = new AppSettingsService.WindowSettings
        {
            Width = 1600,
            Height = 900,
            X = 100,
            Y = 50,
            IsMaximized = true
        };

        Assert.Equal(1600, settings.Width);
        Assert.Equal(900, settings.Height);
        Assert.Equal(100, settings.X);
        Assert.Equal(50, settings.Y);
        Assert.True(settings.IsMaximized);
    }

    [Fact]
    public void RecentProjects_CanAddItems()
    {
        var settings = new AppSettingsService.AppSettings();

        settings.RecentProjects.Add(new AppSettingsService.RecentProject
        {
            Name = "Project1",
            Path = @"C:\Projects\Project1",
            LastOpened = DateTime.Now
        });

        Assert.Single(settings.RecentProjects);
        Assert.Equal("Project1", settings.RecentProjects[0].Name);
    }

    [Fact]
    public void RecentProjects_CanRemoveItems()
    {
        var settings = new AppSettingsService.AppSettings();

        settings.RecentProjects.Add(new AppSettingsService.RecentProject
        {
            Name = "Project1",
            Path = @"C:\Projects\Project1"
        });

        settings.RecentProjects.Add(new AppSettingsService.RecentProject
        {
            Name = "Project2",
            Path = @"C:\Projects\Project2"
        });

        Assert.Equal(2, settings.RecentProjects.Count);

        settings.RecentProjects.RemoveAll(p => p.Name == "Project1");

        Assert.Single(settings.RecentProjects);
        Assert.Equal("Project2", settings.RecentProjects[0].Name);
    }
}
