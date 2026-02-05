using FluentAssertions;
using ServicesChecker.Domain.Entities;
using ServicesChecker.Domain.Enums;
using ServicesChecker.UI.ViewModels;

namespace ServicesChecker.UI.Tests.ViewModels;

public class ServiceItemViewModelTests
{
    [Fact]
    public void Name_SetValue_ShouldUpdateProperty()
    {
        // Arrange
        var viewModel = new ServiceItemViewModel();
        const string name = "TestService";

        // Act
        viewModel.Name = name;

        // Assert
        viewModel.Name.Should().Be(name);
    }

    [Fact]
    public void Status_SetValue_ShouldUpdateProperty()
    {
        // Arrange
        var viewModel = new ServiceItemViewModel();

        // Act
        viewModel.Status = ServiceStatus.Running;

        // Assert
        viewModel.Status.Should().Be(ServiceStatus.Running);
    }

    [Fact]
    public void Status_Changed_ShouldNotifyStatusTextChanged()
    {
        // Arrange
        var viewModel = new ServiceItemViewModel();
        var notifiedProperties = new List<string>();
        viewModel.PropertyChanged += (sender, args) => notifiedProperties.Add(args.PropertyName!);

        // Act
        viewModel.Status = ServiceStatus.Running;

        // Assert
        notifiedProperties.Should().Contain(nameof(ServiceItemViewModel.StatusText));
    }

    [Fact]
    public void Status_Changed_ShouldNotifyStatusColorChanged()
    {
        // Arrange
        var viewModel = new ServiceItemViewModel();
        var notifiedProperties = new List<string>();
        viewModel.PropertyChanged += (sender, args) => notifiedProperties.Add(args.PropertyName!);

        // Act
        viewModel.Status = ServiceStatus.Running;

        // Assert
        notifiedProperties.Should().Contain(nameof(ServiceItemViewModel.StatusColor));
    }

    [Theory]
    [InlineData(ServiceStatus.Running, "Running")]
    [InlineData(ServiceStatus.Stopped, "Stopped")]
    [InlineData(ServiceStatus.StartPending, "Starting...")]
    [InlineData(ServiceStatus.StopPending, "Stopping...")]
    [InlineData(ServiceStatus.Paused, "Paused")]
    [InlineData(ServiceStatus.Available, "Available")]
    [InlineData(ServiceStatus.Unavailable, "Unavailable")]
    [InlineData(ServiceStatus.Unknown, "Unknown")]
    public void StatusText_ForDifferentStatuses_ShouldReturnCorrectText(ServiceStatus status, string expectedText)
    {
        // Arrange
        var viewModel = new ServiceItemViewModel { Status = status };

        // Act
        var result = viewModel.StatusText;

        // Assert
        result.Should().Be(expectedText);
    }

    [Fact]
    public void StatusText_ErrorStatusWithErrorMessage_ShouldReturnErrorMessage()
    {
        // Arrange
        var viewModel = new ServiceItemViewModel
        {
            Status = ServiceStatus.Error,
            ErrorMessage = "Connection timeout"
        };

        // Act
        var result = viewModel.StatusText;

        // Assert
        result.Should().Be("Connection timeout");
    }

    [Fact]
    public void StatusText_ErrorStatusWithoutErrorMessage_ShouldReturnError()
    {
        // Arrange
        var viewModel = new ServiceItemViewModel
        {
            Status = ServiceStatus.Error,
            ErrorMessage = null
        };

        // Act
        var result = viewModel.StatusText;

        // Assert
        result.Should().Be("Error");
    }

    [Theory]
    [InlineData(ServiceStatus.Running, "#10B981")]
    [InlineData(ServiceStatus.Available, "#10B981")]
    [InlineData(ServiceStatus.Stopped, "#6B7280")]
    [InlineData(ServiceStatus.Paused, "#6B7280")]
    [InlineData(ServiceStatus.StartPending, "#F59E0B")]
    [InlineData(ServiceStatus.StopPending, "#F59E0B")]
    [InlineData(ServiceStatus.Error, "#EF4444")]
    [InlineData(ServiceStatus.Unavailable, "#EF4444")]
    [InlineData(ServiceStatus.Unknown, "#9CA3AF")]
    public void StatusColor_ForDifferentStatuses_ShouldReturnCorrectColor(ServiceStatus status, string expectedColor)
    {
        // Arrange
        var viewModel = new ServiceItemViewModel { Status = status };

        // Act
        var result = viewModel.StatusColor;

        // Assert
        result.Should().Be(expectedColor);
    }

    [Fact]
    public void Type_SetValue_ShouldUpdateProperty()
    {
        // Arrange
        var viewModel = new ServiceItemViewModel();

        // Act
        viewModel.Type = ServiceType.WindowsService;

        // Assert
        viewModel.Type.Should().Be(ServiceType.WindowsService);
    }

    [Fact]
    public void Type_Changed_ShouldNotifyIsWindowsServiceChanged()
    {
        // Arrange
        var viewModel = new ServiceItemViewModel();
        var notifiedProperties = new List<string>();
        viewModel.PropertyChanged += (sender, args) => notifiedProperties.Add(args.PropertyName!);

        // Act
        viewModel.Type = ServiceType.WindowsService;

        // Assert
        notifiedProperties.Should().Contain(nameof(ServiceItemViewModel.IsWindowsService));
    }

    [Fact]
    public void IsWindowsService_WhenTypeIsWindowsService_ShouldReturnTrue()
    {
        // Arrange
        var viewModel = new ServiceItemViewModel { Type = ServiceType.WindowsService };

        // Act
        var result = viewModel.IsWindowsService;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsWindowsService_WhenTypeIsRestEndpoint_ShouldReturnFalse()
    {
        // Arrange
        var viewModel = new ServiceItemViewModel { Type = ServiceType.RestEndpoint };

        // Act
        var result = viewModel.IsWindowsService;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Version_SetValue_ShouldUpdateProperty()
    {
        // Arrange
        var viewModel = new ServiceItemViewModel();
        const string version = "1.0.0";

        // Act
        viewModel.Version = version;

        // Assert
        viewModel.Version.Should().Be(version);
    }

    [Fact]
    public void IsConnectingToDb_SetValue_ShouldUpdateProperty()
    {
        // Arrange
        var viewModel = new ServiceItemViewModel();

        // Act
        viewModel.IsConnectingToDb = true;

        // Assert
        viewModel.IsConnectingToDb.Should().BeTrue();
    }

    [Fact]
    public void ErrorMessage_SetValue_ShouldUpdateProperty()
    {
        // Arrange
        var viewModel = new ServiceItemViewModel();
        const string errorMessage = "Connection failed";

        // Act
        viewModel.ErrorMessage = errorMessage;

        // Assert
        viewModel.ErrorMessage.Should().Be(errorMessage);
    }

    [Fact]
    public void ErrorMessage_Changed_ShouldNotifyStatusTextChanged()
    {
        // Arrange
        var viewModel = new ServiceItemViewModel { Status = ServiceStatus.Error };
        var notifiedProperties = new List<string>();
        viewModel.PropertyChanged += (sender, args) => notifiedProperties.Add(args.PropertyName!);

        // Act
        viewModel.ErrorMessage = "New error";

        // Assert
        notifiedProperties.Should().Contain(nameof(ServiceItemViewModel.StatusText));
    }

    [Fact]
    public void LastChecked_SetValue_ShouldUpdateProperty()
    {
        // Arrange
        var viewModel = new ServiceItemViewModel();
        var lastChecked = DateTime.UtcNow;

        // Act
        viewModel.LastChecked = lastChecked;

        // Assert
        viewModel.LastChecked.Should().Be(lastChecked);
    }

    [Fact]
    public void FromEntity_WithValidEntity_ShouldMapAllProperties()
    {
        // Arrange
        var entity = new ServiceInfo
        {
            Name = "TestService",
            Status = ServiceStatus.Running,
            Type = ServiceType.WindowsService,
            Version = "2.0.0",
            IsConnectingToDb = true,
            ErrorMessage = "Test error",
            LastChecked = DateTime.UtcNow
        };

        // Act
        var viewModel = ServiceItemViewModel.FromEntity(entity);

        // Assert
        viewModel.Name.Should().Be(entity.Name);
        viewModel.Status.Should().Be(entity.Status);
        viewModel.Type.Should().Be(entity.Type);
        viewModel.Version.Should().Be(entity.Version);
        viewModel.IsConnectingToDb.Should().Be(entity.IsConnectingToDb);
        viewModel.ErrorMessage.Should().Be(entity.ErrorMessage);
        viewModel.LastChecked.Should().Be(entity.LastChecked);
    }

    [Fact]
    public void FromEntity_WithNullVersion_ShouldMapCorrectly()
    {
        // Arrange
        var entity = new ServiceInfo
        {
            Name = "TestService",
            Status = ServiceStatus.Running,
            Type = ServiceType.RestEndpoint,
            Version = null
        };

        // Act
        var viewModel = ServiceItemViewModel.FromEntity(entity);

        // Assert
        viewModel.Version.Should().BeNull();
    }

    [Fact]
    public void ToEntity_WithValidViewModel_ShouldMapAllProperties()
    {
        // Arrange
        var viewModel = new ServiceItemViewModel
        {
            Name = "TestService",
            Status = ServiceStatus.Stopped,
            Type = ServiceType.RestEndpoint,
            Version = "3.0.0",
            IsConnectingToDb = false,
            ErrorMessage = "Unavailable",
            LastChecked = DateTime.UtcNow
        };

        // Act
        var entity = viewModel.ToEntity();

        // Assert
        entity.Name.Should().Be(viewModel.Name);
        entity.Status.Should().Be(viewModel.Status);
        entity.Type.Should().Be(viewModel.Type);
        entity.Version.Should().Be(viewModel.Version);
        entity.IsConnectingToDb.Should().Be(viewModel.IsConnectingToDb);
        entity.ErrorMessage.Should().Be(viewModel.ErrorMessage);
        entity.LastChecked.Should().Be(viewModel.LastChecked);
    }

    [Fact]
    public void FromEntity_ToEntity_RoundTrip_ShouldPreserveAllData()
    {
        // Arrange
        var originalEntity = new ServiceInfo
        {
            Name = "RoundTripService",
            Status = ServiceStatus.Paused,
            Type = ServiceType.WindowsService,
            Version = "1.5.0",
            IsConnectingToDb = true,
            ErrorMessage = null,
            LastChecked = DateTime.UtcNow
        };

        // Act
        var viewModel = ServiceItemViewModel.FromEntity(originalEntity);
        var resultEntity = viewModel.ToEntity();

        // Assert
        resultEntity.Should().BeEquivalentTo(originalEntity);
    }
}
