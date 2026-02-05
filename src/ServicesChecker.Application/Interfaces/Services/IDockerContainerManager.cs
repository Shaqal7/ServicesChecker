using ServicesChecker.Domain.Entities;

namespace ServicesChecker.Application.Interfaces.Services;

/// <summary>
/// Manages Docker containers - list, start, stop, switch.
/// </summary>
public interface IDockerContainerManager
{
    Task<IReadOnlyList<ContainerInfo>> GetContainersAsync(CancellationToken cancellationToken = default);
    Task StartContainerAsync(string containerId, CancellationToken cancellationToken = default);
    Task StopContainerAsync(string containerId, CancellationToken cancellationToken = default);
    Task SwitchContainerAsync(string? currentContainerId, string newContainerId, CancellationToken cancellationToken = default);
}
