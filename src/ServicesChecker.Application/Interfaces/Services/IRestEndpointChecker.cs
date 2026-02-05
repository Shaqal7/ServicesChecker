using ServicesChecker.Domain.Enums;

namespace ServicesChecker.Application.Interfaces.Services;

/// <summary>
/// Checks health status of REST endpoints.
/// </summary>
public interface IRestEndpointChecker
{
    Task<ServiceStatus> CheckHealthAsync(string url, CancellationToken cancellationToken = default);
    Task<(ServiceStatus Status, string? ResponseBody)> CheckHealthWithResponseAsync(string url, CancellationToken cancellationToken = default);
}
