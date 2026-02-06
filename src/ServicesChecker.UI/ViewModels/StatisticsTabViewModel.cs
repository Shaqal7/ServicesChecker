using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServicesChecker.Application.Interfaces.Services;
using ServicesChecker.Domain.Entities;

namespace ServicesChecker.UI.ViewModels;

public partial class StatisticsTabViewModel : ViewModelBase
{
    private readonly IDockerStorageService _dockerStorageService;
    private CancellationTokenSource? _refreshCts;

    [ObservableProperty]
    private bool _isDockerInstalled;

    [ObservableProperty]
    private string? _vhdxFilePath;

    [ObservableProperty]
    private long _vhdxFileSizeBytes;

    [ObservableProperty]
    private DateTime? _vhdxLastModified;

    [ObservableProperty]
    private DiskSpaceInfo? _diskSpace;

    [ObservableProperty]
    private DateTime _lastUpdated;

    [ObservableProperty]
    private bool _isLowDiskSpace;

    public string VhdxFileSizeFormatted => FormatBytes(VhdxFileSizeBytes);

    public string DiskAvailableFormatted => DiskSpace != null ? FormatBytes(DiskSpace.AvailableBytes) : "N/A";

    public string DiskTotalFormatted => DiskSpace != null ? FormatBytes(DiskSpace.TotalBytes) : "N/A";

    public string DiskUsedFormatted => DiskSpace != null ? FormatBytes(DiskSpace.UsedBytes) : "N/A";

    public double DiskUsedPercentage => DiskSpace?.UsedPercentage ?? 0;

    public StatisticsTabViewModel(IDockerStorageService dockerStorageService)
    {
        _dockerStorageService = dockerStorageService;
    }

    public virtual void StartAutoRefresh()
    {
        StopAutoRefresh();
        _refreshCts = new CancellationTokenSource();

        _ = Task.Run(async () =>
        {
            while (!_refreshCts.Token.IsCancellationRequested)
            {
                await RefreshAsync();
                await Task.Delay(30000, _refreshCts.Token);
            }
        }, _refreshCts.Token);
    }

    public virtual void StopAutoRefresh()
    {
        _refreshCts?.Cancel();
        _refreshCts = null;
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();

            var info = await _dockerStorageService.GetStorageInfoAsync();

            IsDockerInstalled = info.IsDockerInstalled;
            VhdxFilePath = info.VhdxFilePath;
            VhdxFileSizeBytes = info.VhdxFileSizeBytes;
            VhdxLastModified = info.VhdxLastModified;
            DiskSpace = info.DiskSpace;
            LastUpdated = info.LastUpdated;
            IsLowDiskSpace = info.DiskSpace?.IsLowSpace ?? false;

            // Notify computed properties
            OnPropertyChanged(nameof(VhdxFileSizeFormatted));
            OnPropertyChanged(nameof(DiskAvailableFormatted));
            OnPropertyChanged(nameof(DiskTotalFormatted));
            OnPropertyChanged(nameof(DiskUsedFormatted));
            OnPropertyChanged(nameof(DiskUsedPercentage));
        }
        catch (Exception ex)
        {
            SetError($"Failed to refresh statistics: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes <= 0) return "0 B";

        string[] sizes = ["B", "KB", "MB", "GB", "TB"];
        int order = 0;
        double size = bytes;

        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }

        return $"{size.ToString("0.##", CultureInfo.InvariantCulture)} {sizes[order]}";
    }
}
