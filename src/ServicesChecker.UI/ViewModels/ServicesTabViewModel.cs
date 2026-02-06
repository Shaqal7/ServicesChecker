using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServicesChecker.Application.Interfaces.Repositories;
using ServicesChecker.Application.Interfaces.Services;
using ServicesChecker.Domain.Entities;
using ServicesChecker.Domain.Enums;

namespace ServicesChecker.UI.ViewModels;

public partial class ServicesTabViewModel : ViewModelBase
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IWindowsServiceManager _windowsServiceManager;
    private readonly IRestEndpointChecker _restEndpointChecker;
    private readonly IDockerContainerManager _containerManager;
    private readonly ISettingsRepository _settingsRepository;

    private CancellationTokenSource? _refreshCts;
    private readonly List<ServiceInfo> _allServices = [];

    [ObservableProperty]
    private ObservableCollection<ServiceItemViewModel> _services = [];

    [ObservableProperty]
    private ServiceItemViewModel? _selectedService;

    [ObservableProperty]
    private string _filterText = string.Empty;

    [ObservableProperty]
    private bool _filterConnectingToDb;

    [ObservableProperty]
    private ObservableCollection<ContainerInfo> _containers = [];

    [ObservableProperty]
    private ContainerInfo? _selectedContainer;

    [ObservableProperty]
    private string _newServiceName = string.Empty;

    [ObservableProperty]
    private bool _newServiceIsRest;

    [ObservableProperty]
    private bool _newServiceConnectsToDb;

    public ServicesTabViewModel(
        IServiceRepository serviceRepository,
        IWindowsServiceManager windowsServiceManager,
        IRestEndpointChecker restEndpointChecker,
        IDockerContainerManager containerManager,
        ISettingsRepository settingsRepository)
    {
        _serviceRepository = serviceRepository;
        _windowsServiceManager = windowsServiceManager;
        _restEndpointChecker = restEndpointChecker;
        _containerManager = containerManager;
        _settingsRepository = settingsRepository;

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await LoadServicesAsync();
        await LoadContainersAsync();
        StartAutoRefresh();
    }

    private void StartAutoRefresh()
    {
        _refreshCts?.Cancel();
        _refreshCts = new CancellationTokenSource();

        _ = Task.Run(async () =>
        {
            while (!_refreshCts.Token.IsCancellationRequested)
            {
                await Task.Delay(5000, _refreshCts.Token);
                await RefreshStatusesAsync();
            }
        }, _refreshCts.Token);
    }

    [RelayCommand]
    private async Task LoadServicesAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();

            var services = await _serviceRepository.GetAllAsync();
            _allServices.Clear();
            _allServices.AddRange(services);

            await RefreshStatusesAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to load services: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RefreshStatusesAsync()
    {
        var tasks = _allServices.Select(async service =>
        {
            try
            {
                if (service.Type == ServiceType.RestEndpoint)
                {
                    service.Status = await _restEndpointChecker.CheckHealthAsync(service.Name);
                }
                else
                {
                    service.Status = await _windowsServiceManager.GetStatusAsync(service.Name);
                    service.Version = await _windowsServiceManager.GetVersionAsync(service.Name);
                }
                service.ErrorMessage = null;
            }
            catch (Exception ex)
            {
                service.Status = ServiceStatus.Error;
                service.ErrorMessage = ex.Message;
            }

            service.LastChecked = DateTime.UtcNow;
        });

        await Task.WhenAll(tasks);
        await _serviceRepository.SaveAllAsync(_allServices);
        ApplyFilter();
    }

    partial void OnFilterTextChanged(string value) => ApplyFilter();

    partial void OnFilterConnectingToDbChanged(bool value) => ApplyFilter();

    private void ApplyFilter()
    {
        var filtered = _allServices.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(FilterText))
        {
            filtered = filtered.Where(s => s.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase));
        }

        if (FilterConnectingToDb)
        {
            filtered = filtered.Where(s => s.IsConnectingToDb);
        }

        Services = new ObservableCollection<ServiceItemViewModel>(
            filtered.OrderBy(s => s.Name).Select(ServiceItemViewModel.FromEntity));
    }

    [RelayCommand]
    private async Task LoadContainersAsync()
    {
        try
        {
            var containers = await _containerManager.GetContainersAsync();
            Containers = new ObservableCollection<ContainerInfo>(containers);

            var settings = await _settingsRepository.GetAsync();
            SelectedContainer = Containers.FirstOrDefault(c => c.Id == settings.SelectedContainerId);
        }
        catch
        {
            // Docker might not be running
        }
    }

    [RelayCommand]
    private async Task AddServiceAsync()
    {
        if (string.IsNullOrWhiteSpace(NewServiceName))
            return;

        try
        {
            ClearError();
            var serviceName = NewServiceName.Trim();
            var isRest = NewServiceIsRest;

            // Validate Windows service exists in the system
            if (!isRest)
            {
                var exists = await _windowsServiceManager.ServiceExistsAsync(serviceName);
                if (!exists)
                {
                    SetError($"Usługa Windows '{serviceName}' nie istnieje w systemie. Sprawdź nazwę i spróbuj ponownie.");
                    return;
                }
            }

            var service = new ServiceInfo
            {
                Name = serviceName,
                Type = isRest ? ServiceType.RestEndpoint : ServiceType.WindowsService,
                IsConnectingToDb = NewServiceConnectsToDb,
                Status = ServiceStatus.Unknown
            };

            await _serviceRepository.AddAsync(service);
            _allServices.Add(service);

            // Clear input fields
            NewServiceName = string.Empty;
            NewServiceIsRest = false;
            NewServiceConnectsToDb = false;

            // Clear filters to ensure the newly added service is visible
            FilterText = string.Empty;
            FilterConnectingToDb = false;

            ApplyFilter();
        }
        catch (Exception ex)
        {
            SetError($"Nie udało się dodać usługi: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task RemoveServiceAsync(ServiceItemViewModel? service)
    {
        if (service == null) return;

        await _serviceRepository.RemoveAsync(service.Name);
        _allServices.RemoveAll(s => s.Name == service.Name);
        ApplyFilter();
    }

    [RelayCommand]
    private async Task StartServiceAsync(ServiceItemViewModel? service)
    {
        if (service == null || !service.IsWindowsService) return;

        try
        {
            IsBusy = true;
            await _windowsServiceManager.StartAsync(service.Name);
            await RefreshStatusesAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to start service: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task StopServiceAsync(ServiceItemViewModel? service)
    {
        if (service == null || !service.IsWindowsService) return;

        try
        {
            IsBusy = true;
            await _windowsServiceManager.StopAsync(service.Name);
            await RefreshStatusesAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to stop service: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RestartServiceAsync(ServiceItemViewModel? service)
    {
        if (service == null || !service.IsWindowsService) return;

        try
        {
            IsBusy = true;
            await _windowsServiceManager.RestartAsync(service.Name);
            await RefreshStatusesAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to restart service: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SwitchContainerAsync()
    {
        if (SelectedContainer == null) return;

        try
        {
            IsBusy = true;
            var settings = await _settingsRepository.GetAsync();
            var currentId = settings.SelectedContainerId;

            await _containerManager.SwitchContainerAsync(currentId, SelectedContainer.Id);

            settings.SelectedContainerId = SelectedContainer.Id;
            await _settingsRepository.SaveAsync(settings);

            await LoadContainersAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to switch container: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
