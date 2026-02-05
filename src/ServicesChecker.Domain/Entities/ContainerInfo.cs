namespace ServicesChecker.Domain.Entities;

/// <summary>
/// Represents information about a Docker container.
/// </summary>
public class ContainerInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsRunning { get; set; }
    public DateTime? CreatedAt { get; set; }
}
