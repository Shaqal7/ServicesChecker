using FluentAssertions;
using Moq;
using ServicesChecker.Application.Interfaces.Services;
using ServicesChecker.Domain.Entities;
using ServicesChecker.Domain.Enums;
using ServicesChecker.Infrastructure.Persistence;

namespace ServicesChecker.Infrastructure.Tests.Persistence;

public class JsonSettingsRepositoryTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly Mock<IFileSystemService> _mockFileSystem;
    private readonly JsonSettingsRepository _repository;
    private readonly Dictionary<string, string> _fileStorage;
    private string _testFilePath => Path.Combine(_testDirectory, "settings.json");

    public JsonSettingsRepositoryTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"test_settings_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        _fileStorage = new Dictionary<string, string>();

        _mockFileSystem = new Mock<IFileSystemService>();
        _mockFileSystem.Setup(x => x.GetAppDirectory()).Returns(_testDirectory);
        _mockFileSystem.Setup(x => x.FileExists(It.IsAny<string>()))
            .Returns<string>(path => _fileStorage.ContainsKey(path));
        _mockFileSystem.Setup(x => x.ReadAllTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync<string, CancellationToken, IFileSystemService, string>((path, ct) =>
                _fileStorage.ContainsKey(path) ? _fileStorage[path] : string.Empty);
        _mockFileSystem.Setup(x => x.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns<string, string, CancellationToken>((path, content, ct) =>
            {
                _fileStorage[path] = content;
                return Task.CompletedTask;
            });

        _repository = new JsonSettingsRepository(_mockFileSystem.Object);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetAsync_FileNotExists_ShouldReturnDefaultSettings()
    {
        // Act
        var settings = await _repository.GetAsync();

        // Assert
        settings.Should().NotBeNull();
        settings.Theme.Should().Be(ThemeMode.System);
        settings.ServiceRefreshIntervalSeconds.Should().Be(5);
        settings.StatisticsRefreshIntervalSeconds.Should().Be(30);
    }

    [Fact]
    public async Task SaveAsync_WithSettings_ShouldPersistToFile()
    {
        // Arrange
        var settings = new AppSettings
        {
            Theme = ThemeMode.Dark,
            ServiceRefreshIntervalSeconds = 10,
            StatisticsRefreshIntervalSeconds = 60,
            SelectedContainerId = "container123"
        };

        // Act
        await _repository.SaveAsync(settings);

        // Assert
        var savedSettings = await _repository.GetAsync();
        savedSettings.Theme.Should().Be(ThemeMode.Dark);
        savedSettings.ServiceRefreshIntervalSeconds.Should().Be(10);
        savedSettings.StatisticsRefreshIntervalSeconds.Should().Be(60);
        savedSettings.SelectedContainerId.Should().Be("container123");
    }

    [Fact]
    public async Task GetAsync_AfterSave_ShouldReturnSavedSettings()
    {
        // Arrange
        var windowState = new WindowState
        {
            Left = 100,
            Top = 50,
            Width = 1200,
            Height = 800,
            IsMaximized = true
        };

        var settings = new AppSettings
        {
            Theme = ThemeMode.Light,
            ServiceRefreshIntervalSeconds = 15,
            StatisticsRefreshIntervalSeconds = 45,
            SelectedContainerId = "mycontainer",
            Window = windowState
        };
        await _repository.SaveAsync(settings);

        // Act
        var retrieved = await _repository.GetAsync();

        // Assert
        retrieved.Theme.Should().Be(ThemeMode.Light);
        retrieved.ServiceRefreshIntervalSeconds.Should().Be(15);
        retrieved.StatisticsRefreshIntervalSeconds.Should().Be(45);
        retrieved.SelectedContainerId.Should().Be("mycontainer");
        retrieved.Window.Should().NotBeNull();
        retrieved.Window.Left.Should().Be(100);
        retrieved.Window.Top.Should().Be(50);
        retrieved.Window.Width.Should().Be(1200);
        retrieved.Window.Height.Should().Be(800);
        retrieved.Window.IsMaximized.Should().BeTrue();
    }

    [Fact]
    public async Task SaveAsync_Overwrite_ShouldReplaceExistingSettings()
    {
        // Arrange
        var initialSettings = new AppSettings { Theme = ThemeMode.Dark };
        await _repository.SaveAsync(initialSettings);

        var newSettings = new AppSettings
        {
            Theme = ThemeMode.Light,
            ServiceRefreshIntervalSeconds = 20
        };

        // Act
        await _repository.SaveAsync(newSettings);
        var retrieved = await _repository.GetAsync();

        // Assert
        retrieved.Theme.Should().Be(ThemeMode.Light);
        retrieved.ServiceRefreshIntervalSeconds.Should().Be(20);
    }

    [Theory]
    [InlineData(ThemeMode.System)]
    [InlineData(ThemeMode.Light)]
    [InlineData(ThemeMode.Dark)]
    public async Task SaveAsync_AllThemeModes_ShouldPersistCorrectly(ThemeMode theme)
    {
        // Arrange
        var settings = new AppSettings { Theme = theme };

        // Act
        await _repository.SaveAsync(settings);
        var retrieved = await _repository.GetAsync();

        // Assert
        retrieved.Theme.Should().Be(theme);
    }

    [Fact]
    public async Task GetAsync_InvalidJsonFile_ShouldReturnDefaultSettings()
    {
        // Arrange
        await File.WriteAllTextAsync(_testFilePath, "{ invalid json }");

        // Act
        var settings = await _repository.GetAsync();

        // Assert
        settings.Should().NotBeNull();
        settings.Theme.Should().Be(ThemeMode.System);
    }
}
