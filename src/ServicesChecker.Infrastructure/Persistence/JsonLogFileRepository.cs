using System.Text.Json;
using ServicesChecker.Application.Interfaces.Repositories;
using ServicesChecker.Application.Interfaces.Services;
using ServicesChecker.Domain.Entities;

namespace ServicesChecker.Infrastructure.Persistence;

public class JsonLogFileRepository : ILogFileRepository
{
    private readonly IFileSystemService _fileSystem;
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public JsonLogFileRepository(IFileSystemService fileSystem)
    {
        _fileSystem = fileSystem;
        _filePath = Path.Combine(_fileSystem.GetAppDirectory(), "logfiles.json");
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<IReadOnlyList<LogFileInfo>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (!_fileSystem.FileExists(_filePath))
                return [];

            var json = await _fileSystem.ReadAllTextAsync(_filePath, cancellationToken);
            if (string.IsNullOrWhiteSpace(json))
                return [];

            return JsonSerializer.Deserialize<List<LogFileInfo>>(json, _jsonOptions) ?? [];
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SaveAllAsync(IEnumerable<LogFileInfo> logFiles, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var json = JsonSerializer.Serialize(logFiles.ToList(), _jsonOptions);
            await _fileSystem.WriteAllTextAsync(_filePath, json, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task AddAsync(LogFileInfo logFile, CancellationToken cancellationToken = default)
    {
        var logFiles = (await GetAllAsync(cancellationToken)).ToList();

        if (logFiles.Any(l => l.FilePath.Equals(logFile.FilePath, StringComparison.OrdinalIgnoreCase)))
            return;

        logFiles.Add(logFile);
        await SaveAllAsync(logFiles, cancellationToken);
    }

    public async Task RemoveAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var logFiles = (await GetAllAsync(cancellationToken)).ToList();
        var toRemove = logFiles.FirstOrDefault(l => l.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase));

        if (toRemove != null)
        {
            logFiles.Remove(toRemove);
            await SaveAllAsync(logFiles, cancellationToken);
        }
    }
}
