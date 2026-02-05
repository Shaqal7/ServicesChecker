using FluentAssertions;
using Moq;
using ServicesChecker.Application.Interfaces.Services;
using ServicesChecker.Domain.Entities;
using ServicesChecker.Infrastructure.Services;

namespace ServicesChecker.Infrastructure.Tests.Services;

public class DockerStorageServiceTests
{
    private readonly Mock<IFileSystemService> _mockFileSystem;
    private readonly DockerStorageService _service;

    public DockerStorageServiceTests()
    {
        _mockFileSystem = new Mock<IFileSystemService>();
        _service = new DockerStorageService(_mockFileSystem.Object);
    }

    [Fact]
    public async Task GetStorageInfoAsync_DockerNotInstalled_ShouldReturnInfoWithInstalledFalse()
    {
        // Arrange
        _mockFileSystem.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);

        // Act
        var result = await _service.GetStorageInfoAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsDockerInstalled.Should().BeFalse();
        result.VhdxFilePath.Should().BeNull();
        result.VhdxFileSizeBytes.Should().Be(0);
        result.VhdxLastModified.Should().BeNull();
        result.DiskSpace.Should().BeNull();
        result.LastUpdated.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetStorageInfoAsync_DockerInstalledButNoVhdx_ShouldReturnInfoWithoutVhdxDetails()
    {
        // Arrange
        var dockerExePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Docker", "Docker", "Docker Desktop.exe");
        _mockFileSystem.Setup(x => x.FileExists(dockerExePath)).Returns(true);
        _mockFileSystem.Setup(x => x.FileExists(It.Is<string>(p => p != dockerExePath))).Returns(false);

        // Act
        var result = await _service.GetStorageInfoAsync();

        // Assert
        result.IsDockerInstalled.Should().BeTrue();
        result.VhdxFilePath.Should().BeNull();
        result.VhdxFileSizeBytes.Should().Be(0);
        result.DiskSpace.Should().BeNull();
    }

    [Fact]
    public async Task GetStorageInfoAsync_DockerInstalledWithVhdx_ShouldReturnCompleteInfo()
    {
        // Arrange
        var dockerExePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Docker", "Docker", "Docker Desktop.exe");
        var vhdxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Docker\wsl\data\ext4.vhdx");
        var lastModified = DateTime.UtcNow.AddDays(-1);
        var diskSpace = new DiskSpaceInfo
        {
            DriveLetter = "C:\\",
            TotalBytes = 500_000_000_000,
            AvailableBytes = 100_000_000_000
        };

        _mockFileSystem.Setup(x => x.FileExists(dockerExePath)).Returns(true);
        _mockFileSystem.Setup(x => x.FileExists(vhdxPath)).Returns(true);
        _mockFileSystem.Setup(x => x.GetFileSize(vhdxPath)).Returns(10_000_000_000);
        _mockFileSystem.Setup(x => x.GetLastModified(vhdxPath)).Returns(lastModified);
        _mockFileSystem.Setup(x => x.GetDiskSpaceInfo(vhdxPath)).Returns(diskSpace);

        // Act
        var result = await _service.GetStorageInfoAsync();

        // Assert
        result.IsDockerInstalled.Should().BeTrue();
        result.VhdxFilePath.Should().Be(vhdxPath);
        result.VhdxFileSizeBytes.Should().Be(10_000_000_000);
        result.VhdxLastModified.Should().Be(lastModified);
        result.DiskSpace.Should().BeEquivalentTo(diskSpace);
    }

    [Fact]
    public async Task GetStorageInfoAsync_WithCancellationToken_ShouldPassTokenThrough()
    {
        // Arrange
        _mockFileSystem.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = async () => await _service.GetStorageInfoAsync(cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task IsDockerInstalledAsync_DockerExeExists_ShouldReturnTrue()
    {
        // Arrange
        var dockerExePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Docker", "Docker", "Docker Desktop.exe");
        _mockFileSystem.Setup(x => x.FileExists(dockerExePath)).Returns(true);

        // Act
        var result = await _service.IsDockerInstalledAsync();

        // Assert
        result.Should().BeTrue();
        _mockFileSystem.Verify(x => x.FileExists(dockerExePath), Times.Once);
    }

    [Fact]
    public async Task IsDockerInstalledAsync_DockerExeDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        _mockFileSystem.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);

        // Act
        var result = await _service.IsDockerInstalledAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetVhdxPathAsync_VhdxInFirstLocation_ShouldReturnFirstPath()
    {
        // Arrange
        var expectedPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Docker\wsl\data\ext4.vhdx");
        _mockFileSystem.Setup(x => x.FileExists(expectedPath)).Returns(true);
        _mockFileSystem.Setup(x => x.FileExists(It.Is<string>(p => p != expectedPath))).Returns(false);

        // Act
        var result = await _service.GetVhdxPathAsync();

        // Assert
        result.Should().Be(expectedPath);
    }

    [Fact]
    public async Task GetVhdxPathAsync_VhdxInSecondLocation_ShouldReturnSecondPath()
    {
        // Arrange
        var firstPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Docker\wsl\data\ext4.vhdx");
        var expectedPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Docker\wsl\disk\docker_data.vhdx");

        _mockFileSystem.Setup(x => x.FileExists(firstPath)).Returns(false);
        _mockFileSystem.Setup(x => x.FileExists(expectedPath)).Returns(true);
        _mockFileSystem.Setup(x => x.FileExists(It.Is<string>(p => p != firstPath && p != expectedPath))).Returns(false);

        // Act
        var result = await _service.GetVhdxPathAsync();

        // Assert
        result.Should().Be(expectedPath);
    }

    [Fact]
    public async Task GetVhdxPathAsync_VhdxInThirdLocation_ShouldReturnThirdPath()
    {
        // Arrange
        var firstPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Docker\wsl\data\ext4.vhdx");
        var secondPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Docker\wsl\disk\docker_data.vhdx");
        var expectedPath = @"C:\ProgramData\Docker\wsl\data\ext4.vhdx";

        _mockFileSystem.Setup(x => x.FileExists(firstPath)).Returns(false);
        _mockFileSystem.Setup(x => x.FileExists(secondPath)).Returns(false);
        _mockFileSystem.Setup(x => x.FileExists(expectedPath)).Returns(true);

        // Act
        var result = await _service.GetVhdxPathAsync();

        // Assert
        result.Should().Be(expectedPath);
    }

    [Fact]
    public async Task GetVhdxPathAsync_NoVhdxFound_ShouldReturnNull()
    {
        // Arrange
        _mockFileSystem.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);

        // Act
        var result = await _service.GetVhdxPathAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetVhdxPathAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = async () => await _service.GetVhdxPathAsync(cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task GetStorageInfoAsync_VhdxFileDoesNotExist_ShouldNotCallFileSizeMethods()
    {
        // Arrange
        var dockerExePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Docker", "Docker", "Docker Desktop.exe");
        _mockFileSystem.Setup(x => x.FileExists(dockerExePath)).Returns(true);
        _mockFileSystem.Setup(x => x.FileExists(It.Is<string>(p => p != dockerExePath))).Returns(false);

        // Act
        var result = await _service.GetStorageInfoAsync();

        // Assert
        _mockFileSystem.Verify(x => x.GetFileSize(It.IsAny<string>()), Times.Never);
        _mockFileSystem.Verify(x => x.GetLastModified(It.IsAny<string>()), Times.Never);
        _mockFileSystem.Verify(x => x.GetDiskSpaceInfo(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetStorageInfoAsync_MultipleInvocations_ShouldUpdateLastUpdated()
    {
        // Arrange
        _mockFileSystem.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);

        // Act
        var result1 = await _service.GetStorageInfoAsync();
        await Task.Delay(100); // Small delay to ensure time difference
        var result2 = await _service.GetStorageInfoAsync();

        // Assert
        result2.LastUpdated.Should().BeAfter(result1.LastUpdated);
    }

    [Fact]
    public async Task GetStorageInfoAsync_VhdxWithDiskSpace_ShouldCalculateUsedBytes()
    {
        // Arrange
        var dockerExePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Docker", "Docker", "Docker Desktop.exe");
        var vhdxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Docker\wsl\data\ext4.vhdx");
        var diskSpace = new DiskSpaceInfo
        {
            DriveLetter = "C:\\",
            TotalBytes = 500_000_000_000,
            AvailableBytes = 100_000_000_000
        };

        _mockFileSystem.Setup(x => x.FileExists(dockerExePath)).Returns(true);
        _mockFileSystem.Setup(x => x.FileExists(vhdxPath)).Returns(true);
        _mockFileSystem.Setup(x => x.GetFileSize(vhdxPath)).Returns(5_000_000_000);
        _mockFileSystem.Setup(x => x.GetDiskSpaceInfo(vhdxPath)).Returns(diskSpace);

        // Act
        var result = await _service.GetStorageInfoAsync();

        // Assert
        result.DiskSpace.Should().NotBeNull();
        result.DiskSpace!.UsedBytes.Should().Be(400_000_000_000); // TotalBytes - AvailableBytes
        result.DiskSpace.UsedPercentage.Should().BeApproximately(80.0, 0.1); // (UsedBytes / TotalBytes) * 100
    }
}
