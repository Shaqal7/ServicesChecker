using System.Text.Json;
using ServicesChecker.Application.Interfaces.Repositories;
using ServicesChecker.Application.Interfaces.Services;
using ServicesChecker.Domain.Entities;

namespace ServicesChecker.Infrastructure.Persistence;

public class JsonServiceRepository : IServiceRepository
{
    private readonly IFileSystemService _fileSystem;
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public JsonServiceRepository(IFileSystemService fileSystem)
    {
        _fileSystem = fileSystem;
        _filePath = Path.Combine(_fileSystem.GetAppDirectory(), "services.json");
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<IReadOnlyList<ServiceInfo>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (!_fileSystem.FileExists(_filePath))
                return [];

            var json = await _fileSystem.ReadAllTextAsync(_filePath, cancellationToken);
            if (string.IsNullOrWhiteSpace(json))
                return [];

            return JsonSerializer.Deserialize<List<ServiceInfo>>(json, _jsonOptions) ?? [];
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SaveAllAsync(IEnumerable<ServiceInfo> services, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var json = JsonSerializer.Serialize(services.ToList(), _jsonOptions);
            await _fileSystem.WriteAllTextAsync(_filePath, json, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task AddAsync(ServiceInfo service, CancellationToken cancellationToken = default)
    {
        var services = (await GetAllAsync(cancellationToken)).ToList();

        if (services.Any(s => s.Name.Equals(service.Name, StringComparison.OrdinalIgnoreCase)))
            return;

        services.Add(service);
        await SaveAllAsync(services, cancellationToken);
    }

    public async Task RemoveAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        var services = (await GetAllAsync(cancellationToken)).ToList();
        var toRemove = services.FirstOrDefault(s => s.Name.Equals(serviceName, StringComparison.OrdinalIgnoreCase));

        if (toRemove != null)
        {
            services.Remove(toRemove);
            await SaveAllAsync(services, cancellationToken);
        }
    }

    public async Task UpdateAsync(ServiceInfo service, CancellationToken cancellationToken = default)
    {
        var services = (await GetAllAsync(cancellationToken)).ToList();
        var index = services.FindIndex(s => s.Name.Equals(service.Name, StringComparison.OrdinalIgnoreCase));

        if (index >= 0)
        {
            services[index] = service;
            await SaveAllAsync(services, cancellationToken);
        }
    }
}
