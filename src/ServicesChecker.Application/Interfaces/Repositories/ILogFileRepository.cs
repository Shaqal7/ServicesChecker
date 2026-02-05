using ServicesChecker.Domain.Entities;

namespace ServicesChecker.Application.Interfaces.Repositories;

/// <summary>
/// Repository for log file configurations (logfiles.json).
/// </summary>
public interface ILogFileRepository
{
    Task<IReadOnlyList<LogFileInfo>> GetAllAsync(CancellationToken cancellationToken = default);
    Task SaveAllAsync(IEnumerable<LogFileInfo> logFiles, CancellationToken cancellationToken = default);
    Task AddAsync(LogFileInfo logFile, CancellationToken cancellationToken = default);
    Task RemoveAsync(string filePath, CancellationToken cancellationToken = default);
}
