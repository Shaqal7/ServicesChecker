using System.Text.Json;
using ServicesChecker.Application.Interfaces.Repositories;
using ServicesChecker.Application.Interfaces.Services;
using ServicesChecker.Domain.Entities;

namespace ServicesChecker.Infrastructure.Persistence;

public class JsonSettingsRepository : ISettingsRepository
{
    private readonly IFileSystemService _fileSystem;
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public JsonSettingsRepository(IFileSystemService fileSystem)
    {
        _fileSystem = fileSystem;
        _filePath = Path.Combine(_fileSystem.GetAppDirectory(), "settings.json");
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<AppSettings> GetAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (!_fileSystem.FileExists(_filePath))
                return new AppSettings();

            var json = await _fileSystem.ReadAllTextAsync(_filePath, cancellationToken);
            if (string.IsNullOrWhiteSpace(json))
                return new AppSettings();

            return JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions) ?? new AppSettings();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var json = JsonSerializer.Serialize(settings, _jsonOptions);
            await _fileSystem.WriteAllTextAsync(_filePath, json, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }
}
