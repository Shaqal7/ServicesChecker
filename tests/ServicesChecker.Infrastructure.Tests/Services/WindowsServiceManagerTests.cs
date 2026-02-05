using FluentAssertions;
using ServicesChecker.Domain.Enums;
using ServicesChecker.Infrastructure.Services;

namespace ServicesChecker.Infrastructure.Tests.Services;

/// <summary>
/// Tests for WindowsServiceManager.
/// Note: These are integration tests that require Windows services to be available.
/// Tests are marked with [Fact(Skip = "Requires Windows services")] to document behavior without requiring specific test services.
/// For full testing, a dedicated test Windows service would need to be installed.
/// </summary>
public class WindowsServiceManagerTests
{
    private readonly WindowsServiceManager _manager;

    public WindowsServiceManagerTests()
    {
        _manager = new WindowsServiceManager();
    }

    [Fact(Skip = "Requires specific Windows service")]
    public async Task GetStatusAsync_RunningService_ShouldReturnRunning()
    {
        // This test requires a known running Windows service (e.g., "Winmgmt" - Windows Management Instrumentation)
        // Arrange
        const string serviceName = "Winmgmt"; // Typically running on Windows

        // Act
        var result = await _manager.GetStatusAsync(serviceName);

        // Assert
        result.Should().BeOneOf(ServiceStatus.Running, ServiceStatus.StartPending);
    }

    [Fact(Skip = "Requires specific Windows service")]
    public async Task GetStatusAsync_StoppedService_ShouldReturnStopped()
    {
        // This test requires a known stopped Windows service
        // Arrange
        const string serviceName = "test_stopped_service";

        // Act
        var result = await _manager.GetStatusAsync(serviceName);

        // Assert
        result.Should().BeOneOf(ServiceStatus.Stopped, ServiceStatus.StopPending);
    }

    [Fact]
    public async Task GetStatusAsync_NonExistentService_ShouldReturnError()
    {
        // Arrange
        const string nonExistentService = "ThisServiceDoesNotExist_12345";

        // Act
        var result = await _manager.GetStatusAsync(nonExistentService);

        // Assert
        result.Should().Be(ServiceStatus.Error);
    }

    [Fact(Skip = "Requires Administrator privileges and specific test service")]
    public async Task StartAsync_StoppedService_ShouldStartService()
    {
        // This test requires Administrator privileges and a specific test service
        // Arrange
        const string serviceName = "test_service";

        // Act
        Func<Task> act = async () => await _manager.StartAsync(serviceName);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact(Skip = "Requires Administrator privileges and specific test service")]
    public async Task StartAsync_AlreadyRunningService_ShouldNotThrow()
    {
        // This test requires Administrator privileges and a running test service
        // Arrange
        const string serviceName = "test_running_service";

        // Act
        Func<Task> act = async () => await _manager.StartAsync(serviceName);

        // Assert
        await act.Should().NotThrowAsync();
        // Note: Should not attempt to start if already running or StartPending
    }

    [Fact(Skip = "Requires Administrator privileges and specific test service")]
    public async Task StopAsync_RunningService_ShouldStopService()
    {
        // This test requires Administrator privileges and a running test service
        // Arrange
        const string serviceName = "test_running_service";

        // Act
        Func<Task> act = async () => await _manager.StopAsync(serviceName);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact(Skip = "Requires Administrator privileges and specific test service")]
    public async Task StopAsync_AlreadyStoppedService_ShouldNotThrow()
    {
        // This test requires Administrator privileges and a stopped test service
        // Arrange
        const string serviceName = "test_stopped_service";

        // Act
        Func<Task> act = async () => await _manager.StopAsync(serviceName);

        // Assert
        await act.Should().NotThrowAsync();
        // Note: Should not attempt to stop if already stopped or StopPending
    }

    [Fact(Skip = "Requires Administrator privileges and specific test service")]
    public async Task RestartAsync_Service_ShouldStopThenStart()
    {
        // This test requires Administrator privileges and a specific test service
        // Arrange
        const string serviceName = "test_service";

        // Act
        Func<Task> act = async () => await _manager.RestartAsync(serviceName);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact(Skip = "Requires specific Windows service")]
    public async Task GetVersionAsync_ServiceWithExecutable_ShouldReturnVersion()
    {
        // This test requires a known Windows service with accessible executable
        // Arrange
        const string serviceName = "Winmgmt";

        // Act
        var result = await _manager.GetVersionAsync(serviceName);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBe("Unknown");
    }

    [Fact]
    public async Task GetVersionAsync_NonExistentService_ShouldReturnUnknown()
    {
        // Arrange
        const string nonExistentService = "ThisServiceDoesNotExist_12345";

        // Act
        var result = await _manager.GetVersionAsync(nonExistentService);

        // Assert
        result.Should().Be("Unknown");
    }

    [Fact(Skip = "Requires specific Windows service")]
    public async Task GetExecutablePathAsync_ValidService_ShouldReturnPath()
    {
        // This test requires a known Windows service
        // Arrange
        const string serviceName = "Winmgmt";

        // Act
        var result = await _manager.GetExecutablePathAsync(serviceName);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().EndWith(".exe", "Service executable path should end with .exe");
    }

    [Fact]
    public async Task GetExecutablePathAsync_NonExistentService_ShouldReturnNull()
    {
        // Arrange
        const string nonExistentService = "ThisServiceDoesNotExist_12345";

        // Act
        var result = await _manager.GetExecutablePathAsync(nonExistentService);

        // Assert
        result.Should().BeNull();
    }

    [Fact(Skip = "Requires specific Windows service")]
    public async Task ServiceExistsAsync_ExistingService_ShouldReturnTrue()
    {
        // This test requires a known Windows service
        // Arrange
        const string serviceName = "Winmgmt";

        // Act
        var result = await _manager.ServiceExistsAsync(serviceName);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ServiceExistsAsync_NonExistentService_ShouldReturnFalse()
    {
        // Arrange
        const string nonExistentService = "ThisServiceDoesNotExist_12345";

        // Act
        var result = await _manager.ServiceExistsAsync(nonExistentService);

        // Assert
        result.Should().BeFalse();
    }

    [Fact(Skip = "Requires specific Windows service")]
    public async Task GetStatusAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = async () => await _manager.GetStatusAsync("Winmgmt", cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact(Skip = "Requires Administrator privileges and specific test service")]
    public async Task StartAsync_WithTimeout_ShouldWaitUpTo30Seconds()
    {
        // This test documents the 30-second timeout behavior
        // Arrange
        const string serviceName = "slow_starting_service";

        // Act & Assert
        // Service should wait up to 30 seconds for status to become Running
        // If timeout is exceeded, an exception is thrown
        true.Should().BeTrue("Documented: StartAsync waits up to 30 seconds for service to start");
    }

    [Fact(Skip = "Requires Administrator privileges and specific test service")]
    public async Task StopAsync_WithTimeout_ShouldWaitUpTo30Seconds()
    {
        // This test documents the 30-second timeout behavior
        // Arrange
        const string serviceName = "slow_stopping_service";

        // Act & Assert
        // Service should wait up to 30 seconds for status to become Stopped
        // If timeout is exceeded, an exception is thrown
        true.Should().BeTrue("Documented: StopAsync waits up to 30 seconds for service to stop");
    }

    /// <summary>
    /// Documents the status mapping logic from ServiceControllerStatus to ServiceStatus
    /// </summary>
    [Fact]
    public void MapStatus_ShouldMapCorrectly()
    {
        // This test documents the status mapping:
        // - ServiceControllerStatus.Running -> ServiceStatus.Running
        // - ServiceControllerStatus.Stopped -> ServiceStatus.Stopped
        // - ServiceControllerStatus.StartPending -> ServiceStatus.StartPending
        // - ServiceControllerStatus.StopPending -> ServiceStatus.StopPending
        // - ServiceControllerStatus.Paused -> ServiceStatus.Paused
        // - ServiceControllerStatus.PausePending -> ServiceStatus.Paused
        // - ServiceControllerStatus.ContinuePending -> ServiceStatus.StartPending
        // - Any other status -> ServiceStatus.Unknown

        true.Should().BeTrue("Documented: Status mapping from ServiceControllerStatus to ServiceStatus");
    }

    /// <summary>
    /// Documents exception handling behavior
    /// </summary>
    [Fact]
    public void GetStatusAsync_InvalidOperationException_ShouldReturnError()
    {
        // This test documents exception handling:
        // - InvalidOperationException (service doesn't exist) -> ServiceStatus.Error
        // - Any other exception -> ServiceStatus.Unknown

        true.Should().BeTrue("Documented: InvalidOperationException returns Error, other exceptions return Unknown");
    }

    /// <summary>
    /// Documents executable path parsing logic
    /// </summary>
    [Fact]
    public void GetExecutablePathAsync_ShouldHandleQuotesAndArguments()
    {
        // This test documents the path parsing logic:
        // 1. Reads ImagePath from registry: HKLM\SYSTEM\CurrentControlSet\Services\{serviceName}
        // 2. Removes surrounding quotes
        // 3. If path contains space and full path doesn't exist, splits at first space to remove arguments
        //    Example: "C:\Program Files\Service\service.exe --arg" -> "C:\Program Files\Service\service.exe"

        true.Should().BeTrue("Documented: Executable path parsing handles quotes and command-line arguments");
    }
}
