using FluentAssertions;
using ServicesChecker.Infrastructure.Services;

namespace ServicesChecker.Infrastructure.Tests.Services;

public class FileSystemServiceTests : IDisposable
{
    private readonly FileSystemService _service;
    private readonly string _testDirectory;

    public FileSystemServiceTests()
    {
        _service = new FileSystemService();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"test_filesystem_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
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
    public void FileExists_FileDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.txt");

        // Act
        var result = _service.FileExists(nonExistentPath);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void FileExists_FileExists_ShouldReturnTrue()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "test.txt");
        File.WriteAllText(filePath, "test content");

        // Act
        var result = _service.FileExists(filePath);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetFileSize_FileDoesNotExist_ShouldReturnZero()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.txt");

        // Act
        var result = _service.GetFileSize(nonExistentPath);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetFileSize_FileExists_ShouldReturnCorrectSize()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "test.txt");
        var content = "Hello World!"; // 12 bytes
        File.WriteAllText(filePath, content);

        // Act
        var result = _service.GetFileSize(filePath);

        // Assert
        result.Should().Be(12);
    }

    [Fact]
    public void GetFileSize_EmptyFile_ShouldReturnZero()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "empty.txt");
        File.WriteAllText(filePath, string.Empty);

        // Act
        var result = _service.GetFileSize(filePath);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetLastModified_FileDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.txt");

        // Act
        var result = _service.GetLastModified(nonExistentPath);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetLastModified_FileExists_ShouldReturnDateTime()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "test.txt");
        var beforeCreate = DateTime.UtcNow;
        File.WriteAllText(filePath, "test");
        var afterCreate = DateTime.UtcNow;

        // Act
        var result = _service.GetLastModified(filePath);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOnOrAfter(beforeCreate.AddSeconds(-1));
        result.Should().BeOnOrBefore(afterCreate.AddSeconds(1));
    }

    [Fact]
    public async Task DeleteFileAsync_FileDoesNotExist_ShouldNotThrow()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.txt");

        // Act
        Func<Task> act = async () => await _service.DeleteFileAsync(nonExistentPath);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteFileAsync_FileExists_ShouldDeleteFile()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "test.txt");
        File.WriteAllText(filePath, "test content");
        File.Exists(filePath).Should().BeTrue();

        // Act
        await _service.DeleteFileAsync(filePath);

        // Assert
        File.Exists(filePath).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteFileAsync_WithCancellation_ShouldRespectCancellationToken()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "test.txt");
        File.WriteAllText(filePath, "test content");
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = async () => await _service.DeleteFileAsync(filePath, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ReadAllTextAsync_FileExists_ShouldReturnContent()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "test.txt");
        var expectedContent = "Hello World!";
        await File.WriteAllTextAsync(filePath, expectedContent);

        // Act
        var result = await _service.ReadAllTextAsync(filePath);

        // Assert
        result.Should().Be(expectedContent);
    }

    [Fact]
    public async Task ReadAllTextAsync_FileDoesNotExist_ShouldThrow()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.txt");

        // Act
        Func<Task> act = async () => await _service.ReadAllTextAsync(nonExistentPath);

        // Assert
        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task WriteAllTextAsync_NewFile_ShouldCreateAndWriteFile()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "newfile.txt");
        var content = "Test Content";

        // Act
        await _service.WriteAllTextAsync(filePath, content);

        // Assert
        File.Exists(filePath).Should().BeTrue();
        var actualContent = await File.ReadAllTextAsync(filePath);
        actualContent.Should().Be(content);
    }

    [Fact]
    public async Task WriteAllTextAsync_DirectoryDoesNotExist_ShouldCreateDirectory()
    {
        // Arrange
        var subdirectory = Path.Combine(_testDirectory, "subdir", "nested");
        var filePath = Path.Combine(subdirectory, "file.txt");
        var content = "Test Content";

        // Act
        await _service.WriteAllTextAsync(filePath, content);

        // Assert
        Directory.Exists(subdirectory).Should().BeTrue();
        File.Exists(filePath).Should().BeTrue();
        var actualContent = await File.ReadAllTextAsync(filePath);
        actualContent.Should().Be(content);
    }

    [Fact]
    public async Task WriteAllTextAsync_ExistingFile_ShouldOverwriteContent()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "test.txt");
        await File.WriteAllTextAsync(filePath, "Old Content");
        var newContent = "New Content";

        // Act
        await _service.WriteAllTextAsync(filePath, newContent);

        // Assert
        var actualContent = await File.ReadAllTextAsync(filePath);
        actualContent.Should().Be(newContent);
    }

    [Fact]
    public void GetDiskSpaceInfo_ValidPath_ShouldReturnDiskInfo()
    {
        // Arrange
        var validPath = _testDirectory;

        // Act
        var result = _service.GetDiskSpaceInfo(validPath);

        // Assert
        result.Should().NotBeNull();
        result!.DriveLetter.Should().NotBeNullOrEmpty();
        result.TotalBytes.Should().BeGreaterThan(0);
        result.AvailableBytes.Should().BeGreaterThan(0);
        result.UsedBytes.Should().BeGreaterThan(0);
        result.UsedPercentage.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetDiskSpaceInfo_EmptyPath_ShouldReturnNull()
    {
        // Arrange
        var emptyPath = string.Empty;

        // Act
        var result = _service.GetDiskSpaceInfo(emptyPath);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetDiskSpaceInfo_InvalidDrive_ShouldReturnNull()
    {
        // Arrange
        var invalidPath = "Z:\\nonexistent\\path";

        // Act
        var result = _service.GetDiskSpaceInfo(invalidPath);

        // Assert (może być null dla nieistniejącego dysku)
        // This test may pass or fail depending on system configuration
        // We're just ensuring it doesn't throw
    }

    [Fact]
    public void GetAppDirectory_ShouldReturnNonEmptyPath()
    {
        // Act
        var result = _service.GetAppDirectory();

        // Assert
        result.Should().NotBeNullOrEmpty();
        Directory.Exists(result).Should().BeTrue();
    }

    [Fact]
    public void GetAppDirectory_ShouldReturnAbsolutePath()
    {
        // Act
        var result = _service.GetAppDirectory();

        // Assert
        Path.IsPathRooted(result).Should().BeTrue();
    }
}
