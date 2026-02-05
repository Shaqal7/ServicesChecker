using FluentAssertions;
using ServicesChecker.Domain.Entities;

namespace ServicesChecker.Domain.Tests.Entities;

public class LogFileInfoTests
{
    [Fact]
    public void LogFileInfo_DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var logFileInfo = new LogFileInfo();

        // Assert
        logFileInfo.FilePath.Should().BeEmpty();
        logFileInfo.FileName.Should().BeEmpty();
        logFileInfo.FileSizeBytes.Should().Be(0);
        logFileInfo.Exists.Should().BeFalse();
        logFileInfo.LastModified.Should().BeNull();
    }

    [Fact]
    public void LogFileInfo_SetProperties_ShouldUpdateValues()
    {
        // Arrange
        var logFileInfo = new LogFileInfo();
        var lastModified = DateTime.Now;

        // Act
        logFileInfo.FilePath = @"C:\Logs\app.log";
        logFileInfo.FileName = "app.log";
        logFileInfo.FileSizeBytes = 1024;
        logFileInfo.Exists = true;
        logFileInfo.LastModified = lastModified;

        // Assert
        logFileInfo.FilePath.Should().Be(@"C:\Logs\app.log");
        logFileInfo.FileName.Should().Be("app.log");
        logFileInfo.FileSizeBytes.Should().Be(1024);
        logFileInfo.Exists.Should().BeTrue();
        logFileInfo.LastModified.Should().Be(lastModified);
    }

    [Fact]
    public void LogFileInfo_WithNonExistentFile_ShouldHaveExistsFalse()
    {
        // Arrange & Act
        var logFileInfo = new LogFileInfo
        {
            FilePath = @"C:\NonExistent\file.log",
            FileName = "file.log",
            Exists = false
        };

        // Assert
        logFileInfo.Exists.Should().BeFalse();
        logFileInfo.FileSizeBytes.Should().Be(0);
        logFileInfo.LastModified.Should().BeNull();
    }

    [Fact]
    public void LogFileInfo_WithExistingFile_ShouldHaveValidProperties()
    {
        // Arrange
        var lastModified = DateTime.Now.AddDays(-1);

        // Act
        var logFileInfo = new LogFileInfo
        {
            FilePath = @"C:\Logs\application.log",
            FileName = "application.log",
            FileSizeBytes = 2048,
            Exists = true,
            LastModified = lastModified
        };

        // Assert
        logFileInfo.Exists.Should().BeTrue();
        logFileInfo.FileSizeBytes.Should().BeGreaterThan(0);
        logFileInfo.LastModified.Should().NotBeNull();
        logFileInfo.LastModified.Should().Be(lastModified);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1024)]
    [InlineData(1048576)]
    [InlineData(1073741824)]
    public void LogFileInfo_WithDifferentFileSizes_ShouldStoreCorrectly(long fileSize)
    {
        // Arrange & Act
        var logFileInfo = new LogFileInfo { FileSizeBytes = fileSize };

        // Assert
        logFileInfo.FileSizeBytes.Should().Be(fileSize);
    }

    [Fact]
    public void LogFileInfo_WithNullLastModified_ShouldBeAllowed()
    {
        // Arrange & Act
        var logFileInfo = new LogFileInfo
        {
            FilePath = @"C:\Logs\test.log",
            FileName = "test.log",
            LastModified = null
        };

        // Assert
        logFileInfo.LastModified.Should().BeNull();
    }
}
