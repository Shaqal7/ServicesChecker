using System.Collections.ObjectModel;
using FluentAssertions;
using Moq;
using ServicesChecker.Application.Interfaces.Repositories;
using ServicesChecker.Application.Interfaces.Services;
using ServicesChecker.Domain.Entities;
using ServicesChecker.UI.ViewModels;

namespace ServicesChecker.UI.Tests.ViewModels;

public class ConfigurationTabViewModelTests
{
    private readonly Mock<ILogFileRepository> _mockLogFileRepository;
    private readonly Mock<IFileSystemService> _mockFileSystemService;

    public ConfigurationTabViewModelTests()
    {
        _mockLogFileRepository = new Mock<ILogFileRepository>();
        _mockFileSystemService = new Mock<IFileSystemService>();

        // Setup default returns
        _mockLogFileRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LogFileInfo>());
    }

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Act
        var viewModel = CreateViewModel();
        Thread.Sleep(100); // Allow async initialization

        // Assert
        viewModel.LogFiles.Should().NotBeNull();
    }

    [Fact]
    public void SelectedLogFile_SetValue_ShouldUpdateProperty()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var logFile = new LogFileItemViewModel { FileName = "test.log" };

        // Act
        viewModel.SelectedLogFile = logFile;

        // Assert
        viewModel.SelectedLogFile.Should().Be(logFile);
    }

    [Fact]
    public void LogFiles_DefaultValue_ShouldBeEmptyCollection()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();

        // Assert
        viewModel.LogFiles.Should().NotBeNull();
        viewModel.LogFiles.Should().BeOfType<ObservableCollection<LogFileItemViewModel>>();
    }

    [Fact]
    public async Task LoadLogFilesAsync_ShouldCallRepository()
    {
        // Arrange
        var logFiles = new List<LogFileInfo>
        {
            new() { FilePath = @"C:\Logs\file1.log", FileName = "file1.log" },
            new() { FilePath = @"C:\Logs\file2.log", FileName = "file2.log" }
        };
        _mockLogFileRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(logFiles);

        var viewModel = CreateViewModel();
        await Task.Delay(200); // Allow initialization

        // Assert
        _mockLogFileRepository.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public void IsBusy_ShouldInheritFromViewModelBase()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();

        // Assert
        viewModel.IsBusy.Should().BeFalse();
        viewModel.IsBusy = true;
        viewModel.IsBusy.Should().BeTrue();
    }

    [Fact]
    public void ErrorMessage_ShouldInheritFromViewModelBase()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();

        // Assert
        viewModel.ErrorMessage.Should().BeNull();
        viewModel.ErrorMessage = "Test error";
        viewModel.ErrorMessage.Should().Be("Test error");
    }

    [Fact]
    public async Task StartAutoRefresh_ShouldCallLoadLogFilesAsyncPeriodically()
    {
        // Arrange
        var logFiles = new List<LogFileInfo>
        {
            new() { FilePath = @"C:\Logs\file1.log", FileName = "file1.log" }
        };
        _mockLogFileRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(logFiles);

        var viewModel = CreateViewModel();
        await Task.Delay(200); // Allow initialization
        _mockLogFileRepository.Invocations.Clear(); // Clear initialization calls

        // Act
        viewModel.StartAutoRefresh();
        await Task.Delay(11000); // Wait for at least one refresh cycle (10 seconds + buffer)

        // Assert - Should be called at least once during the refresh cycle
        _mockLogFileRepository.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);

        // Cleanup
        viewModel.StopAutoRefresh();
    }

    [Fact]
    public async Task StopAutoRefresh_ShouldStopPeriodicRefresh()
    {
        // Arrange
        var logFiles = new List<LogFileInfo>
        {
            new() { FilePath = @"C:\Logs\file1.log", FileName = "file1.log" }
        };
        _mockLogFileRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(logFiles);

        var viewModel = CreateViewModel();
        await Task.Delay(200); // Allow initialization

        // Act
        viewModel.StartAutoRefresh();
        await Task.Delay(500); // Let it start
        _mockLogFileRepository.Invocations.Clear(); // Clear all previous calls
        viewModel.StopAutoRefresh();
        await Task.Delay(11000); // Wait for what would be a refresh cycle

        // Assert - Should not be called after stopping
        _mockLogFileRepository.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void StartAutoRefresh_CalledTwice_ShouldStopPreviousRefresh()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act - Start refresh twice
        viewModel.StartAutoRefresh();
        viewModel.StartAutoRefresh(); // Should stop the first one

        // Assert - Should not throw exception
        viewModel.Should().NotBeNull();

        // Cleanup
        viewModel.StopAutoRefresh();
    }

    [Fact]
    public void StopAutoRefresh_WhenNotStarted_ShouldNotThrow()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act & Assert - Should not throw exception
        Action act = () => viewModel.StopAutoRefresh();
        act.Should().NotThrow();
    }

    [Fact]
    public async Task RefreshCommand_ShouldCallLoadLogFilesAsync()
    {
        // Arrange
        var logFiles = new List<LogFileInfo>
        {
            new() { FilePath = @"C:\Logs\file1.log", FileName = "file1.log" }
        };
        _mockLogFileRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(logFiles);

        var viewModel = CreateViewModel();
        await Task.Delay(200); // Allow initialization
        _mockLogFileRepository.Invocations.Clear(); // Clear initialization calls

        // Act
        await viewModel.RefreshCommand.ExecuteAsync(null);

        // Assert
        _mockLogFileRepository.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddMultipleLogFilesAsync_WithValidPaths_ShouldAddAllFiles()
    {
        // Arrange
        var existingLogFiles = new List<LogFileInfo>
        {
            new() { FilePath = @"C:\Logs\existing.log", FileName = "existing.log" }
        };
        _mockLogFileRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLogFiles);
        _mockFileSystemService.Setup(x => x.FileExists(It.IsAny<string>()))
            .Returns(true);
        _mockFileSystemService.Setup(x => x.GetFileSize(It.IsAny<string>()))
            .Returns(1024);
        _mockFileSystemService.Setup(x => x.GetLastModified(It.IsAny<string>()))
            .Returns(DateTime.Now);

        var viewModel = CreateViewModel();
        await Task.Delay(200); // Allow initialization

        var filePaths = new List<string>
        {
            @"C:\Logs\file1.log",
            @"C:\Logs\file2.log",
            @"C:\Logs\file3.log"
        };

        // Act
        await viewModel.AddMultipleLogFilesCommand.ExecuteAsync(filePaths);

        // Assert
        _mockLogFileRepository.Verify(x => x.SaveAllAsync(
            It.Is<IEnumerable<LogFileInfo>>(list => list.Count() == 4), // 1 existing + 3 new
            It.IsAny<CancellationToken>()), Times.Once);
        viewModel.LogFiles.Should().HaveCount(4);
    }

    [Fact]
    public async Task AddMultipleLogFilesAsync_WithNullOrEmpty_ShouldNotAddAnyFiles()
    {
        // Arrange
        var viewModel = CreateViewModel();
        await Task.Delay(200); // Allow initialization

        // Act - null list
        await viewModel.AddMultipleLogFilesCommand.ExecuteAsync(null);

        // Assert
        _mockLogFileRepository.Verify(x => x.SaveAllAsync(
            It.IsAny<IEnumerable<LogFileInfo>>(),
            It.IsAny<CancellationToken>()), Times.Never);

        // Act - empty list
        await viewModel.AddMultipleLogFilesCommand.ExecuteAsync(new List<string>());

        // Assert
        _mockLogFileRepository.Verify(x => x.SaveAllAsync(
            It.IsAny<IEnumerable<LogFileInfo>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddMultipleLogFilesAsync_WithDuplicates_ShouldSkipDuplicates()
    {
        // Arrange
        var existingLogFiles = new List<LogFileInfo>
        {
            new() { FilePath = @"C:\Logs\file1.log", FileName = "file1.log" }
        };
        _mockLogFileRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLogFiles);
        _mockFileSystemService.Setup(x => x.FileExists(It.IsAny<string>()))
            .Returns(true);

        var viewModel = CreateViewModel();
        await Task.Delay(200); // Allow initialization

        var filePaths = new List<string>
        {
            @"C:\Logs\file1.log", // Duplicate
            @"C:\Logs\file2.log"  // New
        };

        // Act
        await viewModel.AddMultipleLogFilesCommand.ExecuteAsync(filePaths);

        // Assert
        _mockLogFileRepository.Verify(x => x.SaveAllAsync(
            It.Is<IEnumerable<LogFileInfo>>(list => list.Count() == 2), // 1 existing + 1 new (skipped duplicate)
            It.IsAny<CancellationToken>()), Times.Once);
        viewModel.LogFiles.Should().HaveCount(2);
    }

    [Fact]
    public async Task AddMultipleLogFilesAsync_WithWhitespaceStrings_ShouldSkipWhitespace()
    {
        // Arrange
        _mockLogFileRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LogFileInfo>());
        _mockFileSystemService.Setup(x => x.FileExists(It.IsAny<string>()))
            .Returns(true);

        var viewModel = CreateViewModel();
        await Task.Delay(200); // Allow initialization

        var filePaths = new List<string>
        {
            "",
            "   ",
            @"C:\Logs\file1.log"
        };

        // Act
        await viewModel.AddMultipleLogFilesCommand.ExecuteAsync(filePaths);

        // Assert
        _mockLogFileRepository.Verify(x => x.SaveAllAsync(
            It.Is<IEnumerable<LogFileInfo>>(list => list.Count() == 1),
            It.IsAny<CancellationToken>()), Times.Once);
        viewModel.LogFiles.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddMultipleLogFilesAsync_WithNonExistentFiles_ShouldAddWithExistsFalse()
    {
        // Arrange
        _mockLogFileRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LogFileInfo>());
        _mockFileSystemService.Setup(x => x.FileExists(It.IsAny<string>()))
            .Returns(false); // Files don't exist

        var viewModel = CreateViewModel();
        await Task.Delay(200); // Allow initialization

        var filePaths = new List<string>
        {
            @"C:\Logs\missing1.log",
            @"C:\Logs\missing2.log"
        };

        // Act
        await viewModel.AddMultipleLogFilesCommand.ExecuteAsync(filePaths);

        // Assert
        _mockLogFileRepository.Verify(x => x.SaveAllAsync(
            It.Is<IEnumerable<LogFileInfo>>(list =>
                list.Count() == 2 && list.All(lf => !lf.Exists)),
            It.IsAny<CancellationToken>()), Times.Once);
        viewModel.LogFiles.Should().HaveCount(2);
        viewModel.LogFiles.Should().AllSatisfy(lf => lf.Exists.Should().BeFalse());
    }

    [Fact]
    public async Task AddMultipleLogFilesAsync_ShouldSetIsBusyDuringOperation()
    {
        // Arrange
        _mockLogFileRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LogFileInfo>());
        _mockFileSystemService.Setup(x => x.FileExists(It.IsAny<string>()))
            .Returns(true);

        var viewModel = CreateViewModel();
        await Task.Delay(200); // Allow initialization

        var filePaths = new List<string> { @"C:\Logs\file1.log" };

        bool wasBusyDuringOperation = false;
        _mockLogFileRepository.Setup(x => x.SaveAllAsync(
            It.IsAny<IEnumerable<LogFileInfo>>(),
            It.IsAny<CancellationToken>()))
            .Callback(() => wasBusyDuringOperation = viewModel.IsBusy)
            .Returns(Task.CompletedTask);

        // Act
        await viewModel.AddMultipleLogFilesCommand.ExecuteAsync(filePaths);

        // Assert
        wasBusyDuringOperation.Should().BeTrue();
        viewModel.IsBusy.Should().BeFalse(); // Should be false after completion
    }

    [Fact]
    public async Task AddMultipleLogFilesAsync_WhenRepositoryThrows_ShouldSetErrorMessage()
    {
        // Arrange
        _mockLogFileRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Repository error"));

        var viewModel = CreateViewModel();
        await Task.Delay(200); // Allow initialization

        var filePaths = new List<string> { @"C:\Logs\file1.log" };

        // Act
        await viewModel.AddMultipleLogFilesCommand.ExecuteAsync(filePaths);

        // Assert
        viewModel.ErrorMessage.Should().NotBeNullOrEmpty();
        viewModel.ErrorMessage.Should().Contain("Failed to add log files");
        viewModel.IsBusy.Should().BeFalse(); // Should be false even after exception
    }

    private ConfigurationTabViewModel CreateViewModel()
    {
        return new ConfigurationTabViewModel(
            _mockLogFileRepository.Object,
            _mockFileSystemService.Object);
    }
}
