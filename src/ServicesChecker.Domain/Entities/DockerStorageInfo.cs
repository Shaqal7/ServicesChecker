namespace ServicesChecker.Domain.Entities;

/// <summary>
/// Represents Docker storage statistics.
/// </summary>
public class DockerStorageInfo
{
    public bool IsDockerInstalled { get; set; }
    public string? VhdxFilePath { get; set; }
    public long VhdxFileSizeBytes { get; set; }
    public DateTime? VhdxLastModified { get; set; }
    public DiskSpaceInfo? DiskSpace { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Represents disk space information.
/// </summary>
public class DiskSpaceInfo
{
    public string DriveLetter { get; set; } = string.Empty;
    public long TotalBytes { get; set; }
    public long AvailableBytes { get; set; }
    public long UsedBytes => TotalBytes - AvailableBytes;
    public double UsedPercentage => TotalBytes > 0 ? (double)UsedBytes / TotalBytes * 100 : 0;
    public bool IsLowSpace => AvailableBytes < 50L * 1024 * 1024 * 1024; // Less than 50GB
}
