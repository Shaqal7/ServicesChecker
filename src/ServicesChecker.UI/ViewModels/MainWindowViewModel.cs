using Avalonia;
using Avalonia.Styling;
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
    private ThemeMode _currentTheme = ThemeMode.Dark;

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
}
