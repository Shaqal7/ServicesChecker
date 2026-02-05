using FluentAssertions;
using ServicesChecker.Domain.Entities;
using ServicesChecker.Domain.Enums;

namespace ServicesChecker.Domain.Tests.Entities;

public class AppSettingsTests
{
    [Fact]
    public void AppSettings_DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var settings = new AppSettings();

        // Assert
        settings.Theme.Should().Be(ThemeMode.System);
        settings.ServiceRefreshIntervalSeconds.Should().Be(5);
        settings.StatisticsRefreshIntervalSeconds.Should().Be(30);
        settings.SelectedContainerId.Should().BeNull();
        settings.Window.Should().NotBeNull();
    }

    [Fact]
    public void AppSettings_SetTheme_ShouldUpdateValue()
    {
        // Arrange
        var settings = new AppSettings();

        // Act
        settings.Theme = ThemeMode.Dark;

        // Assert
        settings.Theme.Should().Be(ThemeMode.Dark);
    }

    [Theory]
    [InlineData(ThemeMode.System)]
    [InlineData(ThemeMode.Light)]
    [InlineData(ThemeMode.Dark)]
    public void AppSettings_AllThemeModes_ShouldBeValid(ThemeMode theme)
    {
        // Arrange & Act
        var settings = new AppSettings { Theme = theme };

        // Assert
        settings.Theme.Should().Be(theme);
    }

    [Fact]
    public void AppSettings_CustomRefreshIntervals_ShouldStoreCorrectly()
    {
        // Arrange & Act
        var settings = new AppSettings
        {
            ServiceRefreshIntervalSeconds = 10,
            StatisticsRefreshIntervalSeconds = 60
        };

        // Assert
        settings.ServiceRefreshIntervalSeconds.Should().Be(10);
        settings.StatisticsRefreshIntervalSeconds.Should().Be(60);
    }

    [Fact]
    public void AppSettings_WithSelectedContainer_ShouldStoreId()
    {
        // Arrange & Act
        var settings = new AppSettings
        {
            SelectedContainerId = "container123"
        };

        // Assert
        settings.SelectedContainerId.Should().Be("container123");
    }

    [Fact]
    public void AppSettings_WithWindow_ShouldStoreAllProperties()
    {
        // Arrange
        var windowState = new WindowState
        {
            Left = 100,
            Top = 50,
            Width = 1200,
            Height = 800,
            IsMaximized = false
        };

        // Act
        var settings = new AppSettings
        {
            Window = windowState
        };

        // Assert
        settings.Window.Should().NotBeNull();
        settings.Window.Left.Should().Be(100);
        settings.Window.Top.Should().Be(50);
        settings.Window.Width.Should().Be(1200);
        settings.Window.Height.Should().Be(800);
        settings.Window.IsMaximized.Should().BeFalse();
    }

    [Fact]
    public void WindowState_DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var windowState = new WindowState();

        // Assert
        windowState.Left.Should().BeNull();
        windowState.Top.Should().BeNull();
        windowState.Width.Should().BeNull();
        windowState.Height.Should().BeNull();
        windowState.IsMaximized.Should().BeFalse();
    }

    [Fact]
    public void WindowState_MaximizedWindow_ShouldHaveIsMaximizedTrue()
    {
        // Arrange & Act
        var windowState = new WindowState
        {
            IsMaximized = true
        };

        // Assert
        windowState.IsMaximized.Should().BeTrue();
    }

    [Fact]
    public void AppSettings_CompleteConfiguration_ShouldStoreAllProperties()
    {
        // Arrange
        var windowState = new WindowState
        {
            Left = 200,
            Top = 100,
            Width = 1600,
            Height = 900,
            IsMaximized = true
        };

        // Act
        var settings = new AppSettings
        {
            Theme = ThemeMode.Dark,
            ServiceRefreshIntervalSeconds = 15,
            StatisticsRefreshIntervalSeconds = 45,
            SelectedContainerId = "mycontainer",
            Window = windowState
        };

        // Assert
        settings.Theme.Should().Be(ThemeMode.Dark);
        settings.ServiceRefreshIntervalSeconds.Should().Be(15);
        settings.StatisticsRefreshIntervalSeconds.Should().Be(45);
        settings.SelectedContainerId.Should().Be("mycontainer");
        settings.Window.Should().NotBeNull();
        settings.Window.IsMaximized.Should().BeTrue();
    }
}
