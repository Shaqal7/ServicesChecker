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

    public MainWindowViewModelTests()
    {
        // Create mock tab ViewModels - these will need proper constructor parameters in real tests
        _mockServicesTab = new Mock<ServicesTabViewModel>(
            Mock.Of<IServiceRepository>(),
            Mock.Of<IWindowsServiceManager>(),
            Mock.Of<IRestEndpointChecker>(),
            Mock.Of<IDockerContainerManager>(),
            Mock.Of<ISettingsRepository>());

        _mockConfigurationTab = new Mock<ConfigurationTabViewModel>(
            Mock.Of<ILogFileRepository>(),
            Mock.Of<IFileSystemService>());

        _mockStatisticsTab = new Mock<StatisticsTabViewModel>(
            Mock.Of<IDockerStorageService>());

        _mockSettingsRepository = new Mock<ISettingsRepository>();
        _mockSettingsRepository.Setup(x => x.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AppSettings { Theme = ThemeMode.System });
    }

    [Fact]
    public void CurrentTheme_SetValue_ShouldUpdateProperty()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act
        viewModel.CurrentTheme = ThemeMode.Dark;

        // Assert
        viewModel.CurrentTheme.Should().Be(ThemeMode.Dark);
    }

    [Fact]
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

    [Theory]
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

    private MainWindowViewModel CreateViewModel()
    {
        return new MainWindowViewModel(
            _mockServicesTab.Object,
            _mockConfigurationTab.Object,
            _mockStatisticsTab.Object,
            _mockSettingsRepository.Object);
    }
}
