using FluentAssertions;
using ServicesChecker.Domain.Entities;
using ServicesChecker.UI.ViewModels;

namespace ServicesChecker.UI.Tests.ViewModels;

public class LogFileItemViewModelTests
{
    [Fact]
    public void FilePath_SetValue_ShouldUpdateProperty()
    {
        // Arrange
        var viewModel = new LogFileItemViewModel();
        const string filePath = @"C:\Logs\app.log";

        // Act
        viewModel.FilePath = filePath;

        // Assert
        viewModel.FilePath.Should().Be(filePath);
    }

    [Fact]
    public void FileName_SetValue_ShouldUpdateProperty()
    {
        // Arrange
        var viewModel = new LogFileItemViewModel();
        const string fileName = "app.log";

        // Act
        viewModel.FileName = fileName;

        // Assert
        viewModel.FileName.Should().Be(fileName);
    }

    [Fact]
    public void FileSizeBytes_SetValue_ShouldUpdateProperty()
    {
        // Arrange
        var viewModel = new LogFileItemViewModel();
        const long fileSize = 1024;

        // Act
        viewModel.FileSizeBytes = fileSize;

        // Assert
        viewModel.FileSizeBytes.Should().Be(fileSize);
    }

    [Fact]
    public void FileSizeBytes_Changed_ShouldNotifyFileSizeFormattedChanged()
    {
        // Arrange
        var viewModel = new LogFileItemViewModel();
        var notifiedProperties = new List<string>();
        viewModel.PropertyChanged += (sender, args) => notifiedProperties.Add(args.PropertyName!);

        // Act
        viewModel.FileSizeBytes = 2048;

        // Assert
        notifiedProperties.Should().Contain(nameof(LogFileItemViewModel.FileSizeFormatted));
    }

    [Theory]
    [InlineData(0, "0 B")]
    [InlineData(-1, "0 B")]
    [InlineData(1, "1 B")]
    [InlineData(512, "512 B")]
    [InlineData(1023, "1023 B")]
    public void FileSizeFormatted_Bytes_ShouldFormatCorrectly(long bytes, string expectedFormat)
    {
        // Arrange
        var viewModel = new LogFileItemViewModel { FileSizeBytes = bytes };

        // Act
        var result = viewModel.FileSizeFormatted;

        // Assert
        result.Should().Be(expectedFormat);
    }

    [Theory]
    [InlineData(1024, "1 KB")]
    [InlineData(1536, "1.5 KB")]
    [InlineData(2048, "2 KB")]
    [InlineData(10240, "10 KB")]
    [InlineData(1048575, "1024 KB")]
    public void FileSizeFormatted_Kilobytes_ShouldFormatCorrectly(long bytes, string expectedFormat)
    {
        // Arrange
        var viewModel = new LogFileItemViewModel { FileSizeBytes = bytes };

        // Act
        var result = viewModel.FileSizeFormatted;

        // Assert
        result.Should().Be(expectedFormat);
    }

    [Theory]
    [InlineData(1048576, "1 MB")]
    [InlineData(1572864, "1.5 MB")]
    [InlineData(2097152, "2 MB")]
    [InlineData(10485760, "10 MB")]
    [InlineData(104857600, "100 MB")]
    public void FileSizeFormatted_Megabytes_ShouldFormatCorrectly(long bytes, string expectedFormat)
    {
        // Arrange
        var viewModel = new LogFileItemViewModel { FileSizeBytes = bytes };

        // Act
        var result = viewModel.FileSizeFormatted;

        // Assert
        result.Should().Be(expectedFormat);
    }

    [Theory]
    [InlineData(1073741824, "1 GB")]
    [InlineData(1610612736, "1.5 GB")]
    [InlineData(2147483648, "2 GB")]
    [InlineData(10737418240, "10 GB")]
    [InlineData(107374182400, "100 GB")]
    public void FileSizeFormatted_Gigabytes_ShouldFormatCorrectly(long bytes, string expectedFormat)
    {
        // Arrange
        var viewModel = new LogFileItemViewModel { FileSizeBytes = bytes };

        // Act
        var result = viewModel.FileSizeFormatted;

        // Assert
        result.Should().Be(expectedFormat);
    }

    [Theory]
    [InlineData(1099511627776, "1 TB")]
    [InlineData(1649267441664, "1.5 TB")]
    [InlineData(2199023255552, "2 TB")]
    public void FileSizeFormatted_Terabytes_ShouldFormatCorrectly(long bytes, string expectedFormat)
    {
        // Arrange
        var viewModel = new LogFileItemViewModel { FileSizeBytes = bytes };

        // Act
        var result = viewModel.FileSizeFormatted;

        // Assert
        result.Should().Be(expectedFormat);
    }

    [Fact]
    public void Exists_SetValue_ShouldUpdateProperty()
    {
        // Arrange
        var viewModel = new LogFileItemViewModel();

        // Act
        viewModel.Exists = true;

        // Assert
        viewModel.Exists.Should().BeTrue();
    }

    [Fact]
    public void Exists_Changed_ShouldNotifyStatusColorChanged()
    {
        // Arrange
        var viewModel = new LogFileItemViewModel();
        var notifiedProperties = new List<string>();
        viewModel.PropertyChanged += (sender, args) => notifiedProperties.Add(args.PropertyName!);

        // Act
        viewModel.Exists = true;

        // Assert
        notifiedProperties.Should().Contain(nameof(LogFileItemViewModel.StatusColor));
    }

    [Fact]
    public void StatusColor_WhenExists_ShouldReturnGreen()
    {
        // Arrange
        var viewModel = new LogFileItemViewModel { Exists = true };

        // Act
        var result = viewModel.StatusColor;

        // Assert
        result.Should().Be("#10B981");
    }

    [Fact]
    public void StatusColor_WhenNotExists_ShouldReturnRed()
    {
        // Arrange
        var viewModel = new LogFileItemViewModel { Exists = false };

        // Act
        var result = viewModel.StatusColor;

        // Assert
        result.Should().Be("#EF4444");
    }

    [Fact]
    public void LastModified_SetValue_ShouldUpdateProperty()
    {
        // Arrange
        var viewModel = new LogFileItemViewModel();
        var lastModified = DateTime.UtcNow;

        // Act
        viewModel.LastModified = lastModified;

        // Assert
        viewModel.LastModified.Should().Be(lastModified);
    }

    [Fact]
    public void LastModified_SetToNull_ShouldUpdateProperty()
    {
        // Arrange
        var viewModel = new LogFileItemViewModel { LastModified = DateTime.UtcNow };

        // Act
        viewModel.LastModified = null;

        // Assert
        viewModel.LastModified.Should().BeNull();
    }

    [Fact]
    public void FromEntity_WithValidEntity_ShouldMapAllProperties()
    {
        // Arrange
        var entity = new LogFileInfo
        {
            FilePath = @"C:\Logs\test.log",
            FileName = "test.log",
            FileSizeBytes = 2048,
            Exists = true,
            LastModified = DateTime.UtcNow
        };

        // Act
        var viewModel = LogFileItemViewModel.FromEntity(entity);

        // Assert
        viewModel.FilePath.Should().Be(entity.FilePath);
        viewModel.FileName.Should().Be(entity.FileName);
        viewModel.FileSizeBytes.Should().Be(entity.FileSizeBytes);
        viewModel.Exists.Should().Be(entity.Exists);
        viewModel.LastModified.Should().Be(entity.LastModified);
    }

    [Fact]
    public void FromEntity_WithNullLastModified_ShouldMapCorrectly()
    {
        // Arrange
        var entity = new LogFileInfo
        {
            FilePath = @"C:\Logs\test.log",
            FileName = "test.log",
            FileSizeBytes = 0,
            Exists = false,
            LastModified = null
        };

        // Act
        var viewModel = LogFileItemViewModel.FromEntity(entity);

        // Assert
        viewModel.LastModified.Should().BeNull();
    }

    [Fact]
    public void ToEntity_WithValidViewModel_ShouldMapAllProperties()
    {
        // Arrange
        var viewModel = new LogFileItemViewModel
        {
            FilePath = @"D:\Logs\error.log",
            FileName = "error.log",
            FileSizeBytes = 4096,
            Exists = false,
            LastModified = DateTime.UtcNow
        };

        // Act
        var entity = viewModel.ToEntity();

        // Assert
        entity.FilePath.Should().Be(viewModel.FilePath);
        entity.FileName.Should().Be(viewModel.FileName);
        entity.FileSizeBytes.Should().Be(viewModel.FileSizeBytes);
        entity.Exists.Should().Be(viewModel.Exists);
        entity.LastModified.Should().Be(viewModel.LastModified);
    }

    [Fact]
    public void FromEntity_ToEntity_RoundTrip_ShouldPreserveAllData()
    {
        // Arrange
        var originalEntity = new LogFileInfo
        {
            FilePath = @"E:\Logs\roundtrip.log",
            FileName = "roundtrip.log",
            FileSizeBytes = 8192,
            Exists = true,
            LastModified = DateTime.UtcNow.AddHours(-2)
        };

        // Act
        var viewModel = LogFileItemViewModel.FromEntity(originalEntity);
        var resultEntity = viewModel.ToEntity();

        // Assert
        resultEntity.Should().BeEquivalentTo(originalEntity);
    }

    [Fact]
    public void FileSizeFormatted_VeryLargeValue_ShouldFormatWithoutOverflow()
    {
        // Arrange
        var viewModel = new LogFileItemViewModel { FileSizeBytes = long.MaxValue };

        // Act
        var result = viewModel.FileSizeFormatted;

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().EndWith("TB");
    }

    [Fact]
    public void FileSizeFormatted_ShouldNotShowTrailingZeros()
    {
        // Arrange - 1.5 MB (exactly)
        var viewModel = new LogFileItemViewModel { FileSizeBytes = 1572864 };

        // Act
        var result = viewModel.FileSizeFormatted;

        // Assert
        result.Should().Be("1.5 MB");
        result.Should().NotContain(".00");
        result.Should().NotContain(".50");
    }
}
