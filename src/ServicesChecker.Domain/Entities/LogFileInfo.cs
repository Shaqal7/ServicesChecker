namespace ServicesChecker.Domain.Entities;

/// <summary>
/// Represents information about a monitored log file.
/// </summary>
public class LogFileInfo
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public bool Exists { get; set; }
    public DateTime? LastModified { get; set; }
}
