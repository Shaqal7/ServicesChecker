using FluentAssertions;
using ServicesChecker.Domain.Entities;

namespace ServicesChecker.Domain.Tests.Entities;

public class DockerStorageInfoTests
{
    [Fact]
    public void DockerStorageInfo_DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var storageInfo = new DockerStorageInfo();

        // Assert
        storageInfo.IsDockerInstalled.Should().BeFalse();
        storageInfo.VhdxFilePath.Should().BeNull();
        storageInfo.VhdxFileSizeBytes.Should().Be(0);
        storageInfo.VhdxLastModified.Should().BeNull();
        storageInfo.DiskSpace.Should().BeNull();
        storageInfo.LastUpdated.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void DockerStorageInfo_WithDockerInstalled_ShouldHaveValidProperties()
    {
        // Arrange
        var lastModified = DateTime.Now.AddDays(-1);
        var lastUpdated = DateTime.Now;
        var diskSpace = new DiskSpaceInfo
        {
            DriveLetter = "C",
            TotalBytes = 1000000000000,
            AvailableBytes = 500000000000
        };

        // Act
        var storageInfo = new DockerStorageInfo
        {
            IsDockerInstalled = true,
            VhdxFilePath = @"C:\ProgramData\Docker\windowsfilter\ext4.vhdx",
            VhdxFileSizeBytes = 10737418240,
            VhdxLastModified = lastModified,
            DiskSpace = diskSpace,
            LastUpdated = lastUpdated
        };

        // Assert
        storageInfo.IsDockerInstalled.Should().BeTrue();
        storageInfo.VhdxFilePath.Should().Be(@"C:\ProgramData\Docker\windowsfilter\ext4.vhdx");
        storageInfo.VhdxFileSizeBytes.Should().Be(10737418240);
        storageInfo.VhdxLastModified.Should().Be(lastModified);
        storageInfo.DiskSpace.Should().NotBeNull();
        storageInfo.LastUpdated.Should().Be(lastUpdated);
    }

    [Fact]
    public void DockerStorageInfo_WithoutDockerInstalled_ShouldHaveNullProperties()
    {
        // Arrange & Act
        var storageInfo = new DockerStorageInfo
        {
            IsDockerInstalled = false
        };

        // Assert
        storageInfo.IsDockerInstalled.Should().BeFalse();
        storageInfo.VhdxFilePath.Should().BeNull();
        storageInfo.DiskSpace.Should().BeNull();
    }

    [Fact]
    public void DiskSpaceInfo_DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var diskSpace = new DiskSpaceInfo();

        // Assert
        diskSpace.DriveLetter.Should().BeEmpty();
        diskSpace.TotalBytes.Should().Be(0);
        diskSpace.AvailableBytes.Should().Be(0);
        diskSpace.UsedBytes.Should().Be(0); // Calculated property
        diskSpace.UsedPercentage.Should().Be(0); // Calculated property
        diskSpace.IsLowSpace.Should().BeTrue(); // Available < 50GB, so true
    }

    [Fact]
    public void DiskSpaceInfo_WithLowSpace_ShouldHaveIsLowSpaceTrue()
    {
        // Arrange & Act
        var diskSpace = new DiskSpaceInfo
        {
            DriveLetter = "C",
            TotalBytes = 500000000000,
            AvailableBytes = 30000000000 // 30GB
        };

        // Assert
        diskSpace.IsLowSpace.Should().BeTrue(); // Calculated: Available < 50GB
        diskSpace.UsedBytes.Should().Be(470000000000); // Calculated: Total - Available
        diskSpace.UsedPercentage.Should().BeApproximately(94.0, 0.1); // Calculated
        diskSpace.AvailableBytes.Should().BeLessThan(50000000000); // Less than 50GB
    }

    [Fact]
    public void DiskSpaceInfo_WithEnoughSpace_ShouldHaveIsLowSpaceFalse()
    {
        // Arrange & Act
        var diskSpace = new DiskSpaceInfo
        {
            DriveLetter = "C",
            TotalBytes = 1000000000000,
            AvailableBytes = 600000000000 // 600GB
        };

        // Assert
        diskSpace.IsLowSpace.Should().BeFalse(); // Calculated: Available >= 50GB
        diskSpace.UsedBytes.Should().Be(400000000000); // Calculated: Total - Available
        diskSpace.UsedPercentage.Should().BeApproximately(40.0, 0.1); // Calculated
        diskSpace.AvailableBytes.Should().BeGreaterThan(50000000000); // More than 50GB
    }

    [Theory]
    [InlineData(1000000000000, 100000000000, 900000000000, 90.0)]
    [InlineData(500000000000, 250000000000, 250000000000, 50.0)]
    [InlineData(2000000000000, 1500000000000, 500000000000, 25.0)]
    public void DiskSpaceInfo_WithDifferentValues_ShouldCalculateCorrectly(
        long total, long available, long expectedUsed, double expectedUsedPercentage)
    {
        // Arrange & Act
        var diskSpace = new DiskSpaceInfo
        {
            TotalBytes = total,
            AvailableBytes = available
        };

        // Assert
        diskSpace.TotalBytes.Should().Be(total);
        diskSpace.AvailableBytes.Should().Be(available);
        diskSpace.UsedBytes.Should().Be(expectedUsed); // Calculated
        diskSpace.UsedPercentage.Should().BeApproximately(expectedUsedPercentage, 0.1); // Calculated
    }
}
