using FluentAssertions;
using Moq;
using ServicesChecker.Application.Interfaces.Repositories;
using ServicesChecker.Application.Interfaces.Services;
using ServicesChecker.Domain.Entities;
using ServicesChecker.Domain.Enums;
using ServicesChecker.UI.ViewModels;

namespace ServicesChecker.UI.Tests.ViewModels;

public class MainWindowViewModelTests
{
    private readonly Mock<ServicesTabViewModel> _mockServicesTab;
    private readonly Mock<ConfigurationTabViewModel> _mockConfigurationTab;
    private readonly Mock<StatisticsTabViewModel> _mockStatisticsTab;
    private readonly Mock<ISettingsRepository> _mockSettingsRepository;
    private readonly Mock<IUpdateService> _mockUpdateService;

    public MainWindowViewModelTests()
    {
        // Create mock tab ViewModels - these will need proper constructor parameters in real tests
        _mockServicesTab = new Mock<ServicesTabViewModel>(
            Mock.Of<IServiceRepository>(),
            Mock.Of<IWindowsServiceManager>(),
            Mock.Of<IRestEndpointChecker>(),
            Mock.Of<IDockerContainerManager>(),
            Mock.Of<ISettingsRepository>(),
            Mock.Of<IClipboardService>());

        _mockConfigurationTab = new Mock<ConfigurationTabViewModel>(
            Mock.Of<ILogFileRepository>(),
            Mock.Of<IFileSystemService>());

        _mockStatisticsTab = new Mock<StatisticsTabViewModel>(
            Mock.Of<IDockerStorageService>());

        _mockSettingsRepository = new Mock<ISettingsRepository>();
        _mockSettingsRepository.Setup(x => x.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AppSettings { Theme = ThemeMode.System });

        _mockUpdateService = new Mock<IUpdateService>();
        _mockUpdateService.Setup(x => x.GetCurrentVersion()).Returns("dev");
        _mockUpdateService.Setup(x => x.CheckForUpdateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((UpdateInfo?)null);
    }

    [Fact(Skip = "Requires Avalonia UI thread (Dispatcher) - not available in CI/CD")]
    public void CurrentTheme_SetValue_ShouldUpdateProperty()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act
        viewModel.CurrentTheme = ThemeMode.Dark;

        // Assert
        viewModel.CurrentTheme.Should().Be(ThemeMode.Dark);
    }

    [Fact(Skip = "Requires Avalonia UI thread (Dispatcher) - not available in CI/CD")]
    public void CurrentTheme_Changed_ShouldRaisePropertyChanged()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(MainWindowViewModel.CurrentTheme))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.CurrentTheme = ThemeMode.Light;

        // Assert
        propertyChangedRaised.Should().BeTrue();
    }

    [Theory(Skip = "Requires Avalonia UI thread (Dispatcher) - not available in CI/CD")]
    [InlineData(ThemeMode.System)]
    [InlineData(ThemeMode.Light)]
    [InlineData(ThemeMode.Dark)]
    public void CurrentTheme_SupportedValues_ShouldSetCorrectly(ThemeMode theme)
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act
        viewModel.CurrentTheme = theme;

        // Assert
        viewModel.CurrentTheme.Should().Be(theme);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithServicesTab()
    {
        // Act
        var viewModel = CreateViewModel();

        // Assert
        viewModel.ServicesTab.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ShouldInitializeWithConfigurationTab()
    {
        // Act
        var viewModel = CreateViewModel();

        // Assert
        viewModel.ConfigurationTab.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ShouldInitializeWithStatisticsTab()
    {
        // Act
        var viewModel = CreateViewModel();

        // Assert
        viewModel.StatisticsTab.Should().NotBeNull();
    }

    [Fact]
    public async Task LoadThemeAsync_ShouldCallSettingsRepository()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();
        await Task.Delay(200); // Allow initialization

        // Assert
        _mockSettingsRepository.Verify(x => x.GetAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CheckForUpdateCommand_WhenUpdateAvailable_SetsIsUpdateAvailableTrue()
    {
        // Arrange
        var update = new UpdateInfo
        {
            TagName = "v2026.02.06-abc1234",
            ReleaseName = "Test Release",
            IsNewerThanCurrent = true
        };
        _mockUpdateService.Setup(x => x.CheckForUpdateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(update);
        var viewModel = CreateViewModel();
        await Task.Delay(300); // Allow startup check to complete

        // Assert
        viewModel.IsUpdateAvailable.Should().BeTrue();
        viewModel.AvailableUpdate.Should().NotBeNull();
        viewModel.AvailableUpdate!.TagName.Should().Be("v2026.02.06-abc1234");
    }

    [Fact]
    public async Task CheckForUpdateCommand_WhenNoUpdate_IsUpdateAvailableRemainsFalse()
    {
        // Arrange
        _mockUpdateService.Setup(x => x.CheckForUpdateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((UpdateInfo?)null);
        var viewModel = CreateViewModel();
        await Task.Delay(300);

        // Assert
        viewModel.IsUpdateAvailable.Should().BeFalse();
        viewModel.AvailableUpdate.Should().BeNull();
    }

    [Fact]
    public async Task CheckForUpdateCommand_WhenNetworkError_SilentFailure()
    {
        // Arrange
        _mockUpdateService.Setup(x => x.CheckForUpdateAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));
        var viewModel = CreateViewModel();
        await Task.Delay(300);

        // Assert
        viewModel.IsUpdateAvailable.Should().BeFalse();
        viewModel.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void CurrentVersion_ShouldDelegateToUpdateService()
    {
        // Arrange
        _mockUpdateService.Setup(x => x.GetCurrentVersion()).Returns("v2026.01.01-abc1234");
        var viewModel = CreateViewModel();

        // Assert
        viewModel.CurrentVersion.Should().Be("v2026.01.01-abc1234");
    }

    [Fact]
    public void Constructor_ShouldStartAutoRefreshForAllTabs()
    {
        // Act
        var viewModel = CreateViewModel();

        // Assert
        _mockServicesTab.Verify(x => x.StartAutoRefresh(), Times.Once);
        _mockConfigurationTab.Verify(x => x.StartAutoRefresh(), Times.Once);
        _mockStatisticsTab.Verify(x => x.StartAutoRefresh(), Times.Once);
    }

    [Fact]
    public void Cleanup_ShouldStopAutoRefreshForAllTabs()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act
        viewModel.Cleanup();

        // Assert
        _mockServicesTab.Verify(x => x.StopAutoRefresh(), Times.Once);
        _mockConfigurationTab.Verify(x => x.StopAutoRefresh(), Times.Once);
        _mockStatisticsTab.Verify(x => x.StopAutoRefresh(), Times.Once);
    }

    private MainWindowViewModel CreateViewModel()
    {
        return new MainWindowViewModel(
            _mockServicesTab.Object,
            _mockConfigurationTab.Object,
            _mockStatisticsTab.Object,
            _mockSettingsRepository.Object,
            _mockUpdateService.Object);
    }
}
