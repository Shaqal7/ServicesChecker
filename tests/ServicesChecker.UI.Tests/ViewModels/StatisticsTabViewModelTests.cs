using FluentAssertions;
using Moq;
using ServicesChecker.Application.Interfaces.Services;
using ServicesChecker.Domain.Entities;
using ServicesChecker.UI.ViewModels;

namespace ServicesChecker.UI.Tests.ViewModels;

public class StatisticsTabViewModelTests
{
    private readonly Mock<IDockerStorageService> _mockDockerStorageService;

    public StatisticsTabViewModelTests()
    {
        _mockDockerStorageService = new Mock<IDockerStorageService>();

        // Setup default returns
        _mockDockerStorageService.Setup(x => x.GetStorageInfoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DockerStorageInfo { IsDockerInstalled = false });
    }

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Act
        var viewModel = CreateViewModel();
        Thread.Sleep(100); // Allow async initialization

        // Assert
        viewModel.IsDockerInstalled.Should().BeFalse(); // Default
    }

    [Fact]
    public void IsDockerInstalled_SetValue_ShouldUpdateProperty()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act
        viewModel.IsDockerInstalled = true;

        // Assert
        viewModel.IsDockerInstalled.Should().BeTrue();
    }

    [Fact]
    public void VhdxFilePath_SetValue_ShouldUpdateProperty()
    {
        // Arrange
        var viewModel = CreateViewModel();
        const string path = @"C:\Docker\vhdx\ext4.vhdx";

        // Act
        viewModel.VhdxFilePath = path;

        // Assert
        viewModel.VhdxFilePath.Should().Be(path);
    }

    [Fact]
    public void VhdxFileSizeBytes_SetValue_ShouldUpdateProperty()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act
        viewModel.VhdxFileSizeBytes = 1073741824; // 1 GB

        // Assert
        viewModel.VhdxFileSizeBytes.Should().Be(1073741824);
    }

    [Fact]
    public void DiskSpace_SetValue_ShouldUpdateProperty()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var diskSpace = new DiskSpaceInfo
        {
            TotalBytes = 500_000_000_000,
            AvailableBytes = 100_000_000_000
        };

        // Act
        viewModel.DiskSpace = diskSpace;

        // Assert
        viewModel.DiskSpace.Should().Be(diskSpace);
    }

    [Fact]
    public void IsLowDiskSpace_SetValue_ShouldUpdateProperty()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act
        viewModel.IsLowDiskSpace = true;

        // Assert
        viewModel.IsLowDiskSpace.Should().BeTrue();
    }

    [Fact]
    public async Task LoadStatisticsAsync_ShouldCallDockerStorageService()
    {
        // Arrange
        var storageInfo = new DockerStorageInfo
        {
            IsDockerInstalled = true,
            VhdxFilePath = @"C:\Docker\vhdx\ext4.vhdx",
            VhdxFileSizeBytes = 5_000_000_000,
            DiskSpace = new DiskSpaceInfo
            {
                TotalBytes = 500_000_000_000,
                AvailableBytes = 100_000_000_000
            }
        };
        _mockDockerStorageService.Setup(x => x.GetStorageInfoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(storageInfo);

        var viewModel = CreateViewModel();
        await Task.Delay(200); // Allow initialization

        // Assert
        _mockDockerStorageService.Verify(x => x.GetStorageInfoAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
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

    private StatisticsTabViewModel CreateViewModel()
    {
        return new StatisticsTabViewModel(_mockDockerStorageService.Object);
    }
}
