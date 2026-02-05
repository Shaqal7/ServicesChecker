using ServicesChecker.Application.Interfaces.Services;
using ServicesChecker.Domain.Entities;

namespace ServicesChecker.Infrastructure.Services;

public class DockerStorageService : IDockerStorageService
{
    private readonly IFileSystemService _fileSystem;

    private static readonly string[] PossibleVhdxPaths =
    [
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Docker\wsl\data\ext4.vhdx"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Docker\wsl\disk\docker_data.vhdx"),
        @"C:\ProgramData\Docker\wsl\data\ext4.vhdx"
    ];

    public DockerStorageService(IFileSystemService fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public async Task<DockerStorageInfo> GetStorageInfoAsync(CancellationToken cancellationToken = default)
    {
        var info = new DockerStorageInfo
        {
            IsDockerInstalled = await IsDockerInstalledAsync(cancellationToken),
            LastUpdated = DateTime.UtcNow
        };

        if (!info.IsDockerInstalled)
            return info;

        info.VhdxFilePath = await GetVhdxPathAsync(cancellationToken);

        if (!string.IsNullOrEmpty(info.VhdxFilePath) && _fileSystem.FileExists(info.VhdxFilePath))
        {
            info.VhdxFileSizeBytes = _fileSystem.GetFileSize(info.VhdxFilePath);
            info.VhdxLastModified = _fileSystem.GetLastModified(info.VhdxFilePath);
            info.DiskSpace = _fileSystem.GetDiskSpaceInfo(info.VhdxFilePath);
        }

        return info;
    }

    public Task<bool> IsDockerInstalledAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            // Check if Docker Desktop is installed by looking for the executable
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var dockerPath = Path.Combine(programFiles, "Docker", "Docker", "Docker Desktop.exe");

            return _fileSystem.FileExists(dockerPath);
        }, cancellationToken);
    }

    public Task<string?> GetVhdxPathAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            // Try to find VHDX in known locations
            foreach (var path in PossibleVhdxPaths)
            {
                if (_fileSystem.FileExists(path))
                    return path;
            }

            // Try to find in other drives
            foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveType == DriveType.Fixed))
            {
                var possiblePaths = new[]
                {
                    Path.Combine(drive.Name, @"Docker\wsl\data\ext4.vhdx"),
                    Path.Combine(drive.Name, @"Docker\ws\data\ext4.vhdx")
                };

                foreach (var path in possiblePaths)
                {
                    if (_fileSystem.FileExists(path))
                        return path;
                }
            }

            return null;
        }, cancellationToken);
    }
}
