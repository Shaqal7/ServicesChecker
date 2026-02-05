using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServicesChecker.Application.Interfaces.Repositories;
using ServicesChecker.Domain.Enums;

namespace ServicesChecker.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ISettingsRepository _settingsRepository;

    [ObservableProperty]
    private ServicesTabViewModel _servicesTab;

    [ObservableProperty]
    private ConfigurationTabViewModel _configurationTab;

    [ObservableProperty]
    private StatisticsTabViewModel _statisticsTab;

    [ObservableProperty]
    private int _selectedTabIndex;

    [ObservableProperty]
    private ThemeMode _currentTheme = ThemeMode.System;

    public MainWindowViewModel(
        ServicesTabViewModel servicesTab,
        ConfigurationTabViewModel configurationTab,
        StatisticsTabViewModel statisticsTab,
        ISettingsRepository settingsRepository)
    {
        _servicesTab = servicesTab;
        _configurationTab = configurationTab;
        _statisticsTab = statisticsTab;
        _settingsRepository = settingsRepository;

        _ = LoadSettingsAsync();
    }

    private async Task LoadSettingsAsync()
    {
        var settings = await _settingsRepository.GetAsync();
        CurrentTheme = settings.Theme;
    }

    [RelayCommand]
    private async Task ToggleThemeAsync()
    {
        CurrentTheme = CurrentTheme switch
        {
            ThemeMode.System => ThemeMode.Light,
            ThemeMode.Light => ThemeMode.Dark,
            ThemeMode.Dark => ThemeMode.System,
            _ => ThemeMode.System
        };

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
}
