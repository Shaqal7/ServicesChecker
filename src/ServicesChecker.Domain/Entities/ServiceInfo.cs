using ServicesChecker.Domain.Enums;

namespace ServicesChecker.Domain.Entities;

/// <summary>
/// Represents information about a monitored service (Windows service or REST endpoint).
/// </summary>
public class ServiceInfo
{
    public string Name { get; set; } = string.Empty;
    public ServiceStatus Status { get; set; } = ServiceStatus.Unknown;
    public ServiceType Type { get; set; } = ServiceType.WindowsService;
    public string? Version { get; set; }
    public bool IsConnectingToDb { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime LastChecked { get; set; } = DateTime.UtcNow;
}
