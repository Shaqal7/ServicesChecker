using ServicesChecker.Application.Interfaces.Services;
using ServicesChecker.Domain.Entities;

namespace ServicesChecker.Infrastructure.Services;

public class FileSystemService : IFileSystemService
{
    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public long GetFileSize(string path)
    {
        if (!File.Exists(path))
            return 0;

        var fileInfo = new FileInfo(path);
        return fileInfo.Length;
    }

    public DateTime? GetLastModified(string path)
    {
        if (!File.Exists(path))
            return null;

        var fileInfo = new FileInfo(path);
        return fileInfo.LastWriteTimeUtc;
    }

    public async Task DeleteFileAsync(string path, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(path))
            return;

        await Task.Run(() => File.Delete(path), cancellationToken);
    }

    public async Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
    {
        return await File.ReadAllTextAsync(path, cancellationToken);
    }

    public async Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(path, content, cancellationToken);
    }

    public DiskSpaceInfo? GetDiskSpaceInfo(string path)
    {
        try
        {
            var root = Path.GetPathRoot(path);
            if (string.IsNullOrEmpty(root))
                return null;

            var driveInfo = new DriveInfo(root);
            if (!driveInfo.IsReady)
                return null;

            return new DiskSpaceInfo
            {
                DriveLetter = driveInfo.Name,
                TotalBytes = driveInfo.TotalSize,
                AvailableBytes = driveInfo.AvailableFreeSpace
            };
        }
        catch
        {
            return null;
        }
    }

    public string GetAppDirectory()
    {
        return AppDomain.CurrentDomain.BaseDirectory;
    }
}
