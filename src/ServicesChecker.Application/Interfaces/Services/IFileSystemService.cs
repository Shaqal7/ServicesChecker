using ServicesChecker.Domain.Entities;

namespace ServicesChecker.Application.Interfaces.Services;

/// <summary>
/// Provides file system operations.
/// </summary>
public interface IFileSystemService
{
    bool FileExists(string path);
    long GetFileSize(string path);
    DateTime? GetLastModified(string path);
    Task DeleteFileAsync(string path, CancellationToken cancellationToken = default);
    Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default);
    Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default);
    DiskSpaceInfo? GetDiskSpaceInfo(string path);
    string GetAppDirectory();
}
