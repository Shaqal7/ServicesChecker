using ServicesChecker.Domain.Entities;

namespace ServicesChecker.Application.Interfaces.Services;

public interface IUpdateService
{
    string GetCurrentVersion();

    Task<UpdateInfo?> CheckForUpdateAsync(CancellationToken cancellationToken = default);

    Task<string> DownloadUpdateAsync(
        UpdateInfo updateInfo,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default);

    void ApplyUpdateAndRestart(string stagingPath);
}
