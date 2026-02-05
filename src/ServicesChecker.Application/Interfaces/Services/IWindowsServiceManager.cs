using ServicesChecker.Domain.Enums;

namespace ServicesChecker.Application.Interfaces.Services;

/// <summary>
/// Manages Windows services - start, stop, restart and status checking.
/// </summary>
public interface IWindowsServiceManager
{
    Task<ServiceStatus> GetStatusAsync(string serviceName, CancellationToken cancellationToken = default);
    Task StartAsync(string serviceName, CancellationToken cancellationToken = default);
    Task StopAsync(string serviceName, CancellationToken cancellationToken = default);
    Task RestartAsync(string serviceName, CancellationToken cancellationToken = default);
    Task<string?> GetVersionAsync(string serviceName, CancellationToken cancellationToken = default);
    Task<string?> GetExecutablePathAsync(string serviceName, CancellationToken cancellationToken = default);
}
