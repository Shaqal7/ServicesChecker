using CommunityToolkit.Mvvm.ComponentModel;
using ServicesChecker.Domain.Entities;
using ServicesChecker.Domain.Enums;

namespace ServicesChecker.UI.ViewModels;

public partial class ServiceItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyPropertyChangedFor(nameof(StatusColor))]
    private ServiceStatus _status;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsWindowsService))]
    private ServiceType _type;

    [ObservableProperty]
    private string? _version;

    [ObservableProperty]
    private bool _isConnectingToDb;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    private string? _errorMessage;

    [ObservableProperty]
    private DateTime _lastChecked;

    [ObservableProperty]
    private bool _isBusy;

    public string StatusText => Status switch
    {
        ServiceStatus.Running => "Running",
        ServiceStatus.Stopped => "Stopped",
        ServiceStatus.StartPending => "Starting...",
        ServiceStatus.StopPending => "Stopping...",
        ServiceStatus.Paused => "Paused",
        ServiceStatus.Error => ErrorMessage ?? "Error",
        ServiceStatus.Available => "Available",
        ServiceStatus.Unavailable => "Unavailable",
        _ => "Unknown"
    };

    public string StatusColor => Status switch
    {
        ServiceStatus.Running => "#10B981",
        ServiceStatus.Available => "#10B981",
        ServiceStatus.Stopped => "#6B7280",
        ServiceStatus.Paused => "#6B7280",
        ServiceStatus.StartPending => "#F59E0B",
        ServiceStatus.StopPending => "#F59E0B",
        ServiceStatus.Error => "#EF4444",
        ServiceStatus.Unavailable => "#EF4444",
        _ => "#9CA3AF"
    };

    public bool IsWindowsService => Type == ServiceType.WindowsService;

    public static ServiceItemViewModel FromEntity(ServiceInfo entity)
    {
        return new ServiceItemViewModel
        {
            Name = entity.Name,
            Status = entity.Status,
            Type = entity.Type,
            Version = entity.Version,
            IsConnectingToDb = entity.IsConnectingToDb,
            ErrorMessage = entity.ErrorMessage,
            LastChecked = entity.LastChecked
        };
    }

    public ServiceInfo ToEntity()
    {
        return new ServiceInfo
        {
            Name = Name,
            Status = Status,
            Type = Type,
            Version = Version,
            IsConnectingToDb = IsConnectingToDb,
            ErrorMessage = ErrorMessage,
            LastChecked = LastChecked
        };
    }
}
