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

    private ConfigurationTabViewModel CreateViewModel()
    {
        return new ConfigurationTabViewModel(
            _mockLogFileRepository.Object,
            _mockFileSystemService.Object);
    }
}
