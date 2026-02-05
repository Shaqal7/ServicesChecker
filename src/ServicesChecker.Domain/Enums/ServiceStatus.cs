namespace ServicesChecker.Domain.Enums;

/// <summary>
/// Represents the current status of a service.
/// </summary>
public enum ServiceStatus
{
    Unknown = 0,
    Running,
    Stopped,
    StartPending,
    StopPending,
    Paused,
    Error,
    Available,      // For REST endpoints
    Unavailable     // For REST endpoints
}
