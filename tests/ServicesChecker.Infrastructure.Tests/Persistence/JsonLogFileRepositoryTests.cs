using FluentAssertions;
using Moq;
using ServicesChecker.Application.Interfaces.Services;
using ServicesChecker.Domain.Entities;
using ServicesChecker.Infrastructure.Persistence;

namespace ServicesChecker.Infrastructure.Tests.Persistence;

public class JsonLogFileRepositoryTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly Mock<IFileSystemService> _mockFileSystem;
    private readonly JsonLogFileRepository _repository;

    public JsonLogFileRepositoryTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"test_logfiles_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);

        _mockFileSystem = new Mock<IFileSystemService>();
        _mockFileSystem.Setup(x => x.GetAppDirectory()).Returns(_testDirectory);

        _repository = new JsonLogFileRepository(_mockFileSystem.Object);
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
    public async Task GetAllAsync_EmptyRepository_ShouldReturnEmptyList()
    {
        // Act
        var logFiles = await _repository.GetAllAsync();

        // Assert
        logFiles.Should().NotBeNull();
        logFiles.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveAllAsync_WithLogFiles_ShouldPersistToFile()
    {
        // Arrange
        var logFiles = new List<LogFileInfo>
        {
            new() { FilePath = @"C:\Logs\app.log", FileName = "app.log", FileSizeBytes = 1024, Exists = true },
            new() { FilePath = @"C:\Logs\error.log", FileName = "error.log", FileSizeBytes = 2048, Exists = false }
        };

        // Act
        await _repository.SaveAllAsync(logFiles);

        // Assert
        File.Exists(_testFilePath).Should().BeTrue();
        var savedLogFiles = await _repository.GetAllAsync();
        savedLogFiles.Should().HaveCount(2);
        savedLogFiles[0].FileName.Should().Be("app.log");
        savedLogFiles[1].FileName.Should().Be("error.log");
    }

    [Fact]
    public async Task GetAllAsync_AfterSave_ShouldReturnSavedLogFiles()
    {
        // Arrange
        var lastModified = DateTime.Now;
        var logFiles = new List<LogFileInfo>
        {
            new()
            {
                FilePath = @"C:\Logs\test.log",
                FileName = "test.log",
                FileSizeBytes = 4096,
                Exists = true,
                LastModified = lastModified
            }
        };
        await _repository.SaveAllAsync(logFiles);

        // Act
        var retrieved = await _repository.GetAllAsync();

        // Assert
        retrieved.Should().HaveCount(1);
        var logFile = retrieved[0];
        logFile.FilePath.Should().Be(@"C:\Logs\test.log");
        logFile.FileName.Should().Be("test.log");
        logFile.FileSizeBytes.Should().Be(4096);
        logFile.Exists.Should().BeTrue();
        logFile.LastModified.Should().BeCloseTo(lastModified, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task SaveAllAsync_Overwrite_ShouldReplaceExistingData()
    {
        // Arrange
        var initialLogFiles = new List<LogFileInfo>
        {
            new() { FilePath = @"C:\Logs\old.log", FileName = "old.log" }
        };
        await _repository.SaveAllAsync(initialLogFiles);

        var newLogFiles = new List<LogFileInfo>
        {
            new() { FilePath = @"C:\Logs\new1.log", FileName = "new1.log" },
            new() { FilePath = @"C:\Logs\new2.log", FileName = "new2.log" }
        };

        // Act
        await _repository.SaveAllAsync(newLogFiles);
        var retrieved = await _repository.GetAllAsync();

        // Assert
        retrieved.Should().HaveCount(2);
        retrieved.Should().NotContain(f => f.FileName == "old.log");
        retrieved.Should().Contain(f => f.FileName == "new1.log");
        retrieved.Should().Contain(f => f.FileName == "new2.log");
    }

    [Fact]
    public async Task SaveAllAsync_EmptyList_ShouldClearRepository()
    {
        // Arrange
        var logFiles = new List<LogFileInfo>
        {
            new() { FilePath = @"C:\Logs\test.log", FileName = "test.log" }
        };
        await _repository.SaveAllAsync(logFiles);

        // Act
        await _repository.SaveAllAsync(new List<LogFileInfo>());
        var retrieved = await _repository.GetAllAsync();

        // Assert
        retrieved.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_FileNotExists_ShouldReturnEmptyList()
    {
        // Arrange
        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }

        // Act
        var logFiles = await _repository.GetAllAsync();

        // Assert
        logFiles.Should().NotBeNull();
        logFiles.Should().BeEmpty();
    }
}
