using ServicesChecker.Domain.Entities;

namespace ServicesChecker.Application.Interfaces.Repositories;

/// <summary>
/// Repository for service configurations (services.json).
/// </summary>
public interface IServiceRepository
{
    Task<IReadOnlyList<ServiceInfo>> GetAllAsync(CancellationToken cancellationToken = default);
    Task SaveAllAsync(IEnumerable<ServiceInfo> services, CancellationToken cancellationToken = default);
    Task AddAsync(ServiceInfo service, CancellationToken cancellationToken = default);
    Task RemoveAsync(string serviceName, CancellationToken cancellationToken = default);
    Task UpdateAsync(ServiceInfo service, CancellationToken cancellationToken = default);
}
