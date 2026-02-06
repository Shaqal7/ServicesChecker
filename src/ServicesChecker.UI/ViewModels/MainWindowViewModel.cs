using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServicesChecker.Application.Interfaces.Repositories;
using ServicesChecker.Application.Interfaces.Services;
using ServicesChecker.Domain.Entities;
using ServicesChecker.Domain.Enums;

namespace ServicesChecker.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ISettingsRepository _settingsRepository;
    private readonly IUpdateService _updateService;
    private readonly PeriodicTimer _updateCheckTimer = new(TimeSpan.FromMinutes(1));

    [ObservableProperty]
    private ServicesTabViewModel _servicesTab;

    [ObservableProperty]
    private ConfigurationTabViewModel _configurationTab;

    [ObservableProperty]
    private StatisticsTabViewModel _statisticsTab;

    [ObservableProperty]
    private int _selectedTabIndex;

    [ObservableProperty]
    private ThemeMode _currentTheme = ThemeMode.Dark;

    [ObservableProperty]
    private bool _isUpdateAvailable;

    [ObservableProperty]
    private UpdateInfo? _availableUpdate;

    [ObservableProperty]
    private bool _isDownloadingUpdate;

    [ObservableProperty]
    private double _downloadProgress;

    [ObservableProperty]
    private string? _updateStatusMessage;

    public string CurrentVersion => _updateService.GetCurrentVersion();

    public MainWindowViewModel(
        ServicesTabViewModel servicesTab,
        ConfigurationTabViewModel configurationTab,
        StatisticsTabViewModel statisticsTab,
        ISettingsRepository settingsRepository,
        IUpdateService updateService)
    {
        _servicesTab = servicesTab;
        _configurationTab = configurationTab;
        _statisticsTab = statisticsTab;
        _settingsRepository = settingsRepository;
        _updateService = updateService;

        _ = LoadSettingsAsync();
        _ = CheckForUpdateAsync();
        _ = StartPeriodicUpdateCheckAsync();
    }

    private async Task LoadSettingsAsync()
    {
        var settings = await _settingsRepository.GetAsync();
        // If System theme is loaded, default to Dark
        CurrentTheme = settings.Theme == ThemeMode.System
            ? ThemeMode.Dark
            : settings.Theme;
    }

    partial void OnCurrentThemeChanged(ThemeMode value)
    {
        if (Avalonia.Application.Current is not null)
        {
            Avalonia.Application.Current.RequestedThemeVariant = value switch
            {
                ThemeMode.Light => ThemeVariant.Light,
                ThemeMode.Dark => ThemeVariant.Dark,
                ThemeMode.System => ThemeVariant.Default,
                _ => ThemeVariant.Default
            };
        }
    }

    [RelayCommand]
    private async Task ToggleThemeAsync()
    {
        // Toggle between Light and Dark only
        CurrentTheme = CurrentTheme == ThemeMode.Light
            ? ThemeMode.Dark
            : ThemeMode.Light;

        var settings = await _settingsRepository.GetAsync();
        settings.Theme = CurrentTheme;
        await _settingsRepository.SaveAsync(settings);
    }

    partial void OnSelectedTabIndexChanged(int value)
    {
        // Refresh data when Statistics tab is selected
        if (value == 2)
        {
            _ = StatisticsTab.RefreshCommand.ExecuteAsync(null);
        }
    }

    [RelayCommand]
    private async Task CheckForUpdateAsync()
    {
        try
        {
            var update = await _updateService.CheckForUpdateAsync();
            if (update is { IsNewerThanCurrent: true })
            {
                AvailableUpdate = update;
                IsUpdateAvailable = true;
                UpdateStatusMessage = $"Version {update.TagName} available";
            }
        }
        catch
        {
            // Silent failure - network issues should not disturb the user
        }
    }

    [RelayCommand]
    private async Task DownloadAndApplyUpdateAsync()
    {
        if (AvailableUpdate is null) return;

        try
        {
            IsDownloadingUpdate = true;
            IsUpdateAvailable = false;
            UpdateStatusMessage = "Downloading update...";

            var progress = new Progress<double>(p => DownloadProgress = p);
            var stagingPath = await _updateService.DownloadUpdateAsync(AvailableUpdate, progress);

            UpdateStatusMessage = "Applying update and restarting...";
            _updateService.ApplyUpdateAndRestart(stagingPath);
        }
        catch (Exception ex)
        {
            IsDownloadingUpdate = false;
            IsUpdateAvailable = true;
            UpdateStatusMessage = $"Update failed: {ex.Message}";
        }
    }

    private async Task StartPeriodicUpdateCheckAsync()
    {
        try
        {
            while (await _updateCheckTimer.WaitForNextTickAsync())
            {
                // Check for updates every minute
                await CheckForUpdateAsync();
            }
        }
        catch
        {
            // Timer disposed or cancelled - silent exit
        }
    }
}
