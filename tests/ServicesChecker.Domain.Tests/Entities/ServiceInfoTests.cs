using FluentAssertions;
using ServicesChecker.Domain.Entities;
using ServicesChecker.Domain.Enums;

namespace ServicesChecker.Domain.Tests.Entities;

public class ServiceInfoTests
{
    [Fact]
    public void ServiceInfo_DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var serviceInfo = new ServiceInfo();

        // Assert
        serviceInfo.Name.Should().BeEmpty();
        serviceInfo.Status.Should().Be(ServiceStatus.Unknown);
        serviceInfo.Type.Should().Be(ServiceType.WindowsService);
        serviceInfo.Version.Should().BeNull();
        serviceInfo.IsConnectingToDb.Should().BeFalse();
        serviceInfo.ErrorMessage.Should().BeNull();
        serviceInfo.LastChecked.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ServiceInfo_SetProperties_ShouldUpdateValues()
    {
        // Arrange
        var serviceInfo = new ServiceInfo();
        var now = DateTime.Now;

        // Act
        serviceInfo.Name = "TestService";
        serviceInfo.Status = ServiceStatus.Running;
        serviceInfo.Type = ServiceType.RestEndpoint;
        serviceInfo.Version = "1.0.0";
        serviceInfo.IsConnectingToDb = true;
        serviceInfo.ErrorMessage = "Test error";
        serviceInfo.LastChecked = now;

        // Assert
        serviceInfo.Name.Should().Be("TestService");
        serviceInfo.Status.Should().Be(ServiceStatus.Running);
        serviceInfo.Type.Should().Be(ServiceType.RestEndpoint);
        serviceInfo.Version.Should().Be("1.0.0");
        serviceInfo.IsConnectingToDb.Should().BeTrue();
        serviceInfo.ErrorMessage.Should().Be("Test error");
        serviceInfo.LastChecked.Should().Be(now);
    }

    [Fact]
    public void ServiceInfo_WithWindowsService_ShouldHaveCorrectType()
    {
        // Arrange & Act
        var serviceInfo = new ServiceInfo
        {
            Name = "MyWindowsService",
            Type = ServiceType.WindowsService,
            Status = ServiceStatus.Running
        };

        // Assert
        serviceInfo.Type.Should().Be(ServiceType.WindowsService);
        serviceInfo.Status.Should().Be(ServiceStatus.Running);
    }

    [Fact]
    public void ServiceInfo_WithRestEndpoint_ShouldHaveCorrectType()
    {
        // Arrange & Act
        var serviceInfo = new ServiceInfo
        {
            Name = "https://api.example.com",
            Type = ServiceType.RestEndpoint,
            Status = ServiceStatus.Available
        };

        // Assert
        serviceInfo.Type.Should().Be(ServiceType.RestEndpoint);
        serviceInfo.Status.Should().Be(ServiceStatus.Available);
    }

    [Theory]
    [InlineData(ServiceStatus.Running)]
    [InlineData(ServiceStatus.Stopped)]
    [InlineData(ServiceStatus.StartPending)]
    [InlineData(ServiceStatus.StopPending)]
    [InlineData(ServiceStatus.Paused)]
    [InlineData(ServiceStatus.Error)]
    [InlineData(ServiceStatus.Available)]
    [InlineData(ServiceStatus.Unavailable)]
    [InlineData(ServiceStatus.Unknown)]
    public void ServiceInfo_AllStatusTypes_ShouldBeValid(ServiceStatus status)
    {
        // Arrange & Act
        var serviceInfo = new ServiceInfo { Status = status };

        // Assert
        serviceInfo.Status.Should().Be(status);
    }

    [Fact]
    public void ServiceInfo_WithNullVersion_ShouldBeAllowed()
    {
        // Arrange & Act
        var serviceInfo = new ServiceInfo
        {
            Name = "TestService",
            Version = null
        };

        // Assert
        serviceInfo.Version.Should().BeNull();
    }

    [Fact]
    public void ServiceInfo_WithErrorMessage_ShouldStoreMessage()
    {
        // Arrange
        const string errorMessage = "Connection timeout";

        // Act
        var serviceInfo = new ServiceInfo
        {
            Status = ServiceStatus.Error,
            ErrorMessage = errorMessage
        };

        // Assert
        serviceInfo.ErrorMessage.Should().Be(errorMessage);
        serviceInfo.Status.Should().Be(ServiceStatus.Error);
    }
}
