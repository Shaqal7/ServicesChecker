using ServicesChecker.Domain.Entities;

namespace ServicesChecker.Application.Interfaces.Services;

/// <summary>
/// Provides Docker storage information (VHDX file, disk space).
/// </summary>
public interface IDockerStorageService
{
    Task<DockerStorageInfo> GetStorageInfoAsync(CancellationToken cancellationToken = default);
    Task<bool> IsDockerInstalledAsync(CancellationToken cancellationToken = default);
    Task<string?> GetVhdxPathAsync(CancellationToken cancellationToken = default);
}
