using ServicesChecker.Domain.Entities;

namespace ServicesChecker.Application.Interfaces.Repositories;

/// <summary>
/// Repository for application settings (settings.json).
/// </summary>
public interface ISettingsRepository
{
    Task<AppSettings> GetAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default);
}
