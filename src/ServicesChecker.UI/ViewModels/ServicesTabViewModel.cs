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
    private readonly IClipboardService _clipboardService;

    private CancellationTokenSource? _refreshCts;
    private readonly List<ServiceItemViewModel> _allServices = [];

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
        ISettingsRepository settingsRepository,
        IClipboardService clipboardService)
    {
        _serviceRepository = serviceRepository;
        _windowsServiceManager = windowsServiceManager;
        _restEndpointChecker = restEndpointChecker;
        _containerManager = containerManager;
        _settingsRepository = settingsRepository;
        _clipboardService = clipboardService;

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await LoadServicesAsync();
        await LoadContainersAsync();
    }

    public virtual void StartAutoRefresh()
    {
        StopAutoRefresh();
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

    public virtual void StopAutoRefresh()
    {
        _refreshCts?.Cancel();
        _refreshCts?.Dispose();
        _refreshCts = null;
    }

    [RelayCommand]
    private async Task LoadServicesAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();

            var entities = await _serviceRepository.GetAllAsync();
            _allServices.Clear();
            _allServices.AddRange(entities.Select(ServiceItemViewModel.FromEntity));

            await RefreshStatusesAsync();
            ApplyFilter();
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
            if (service.IsBusy) return;

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
        await SaveAllAsync();
    }

    private async Task RefreshSingleServiceAsync(ServiceItemViewModel service)
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
    }

    private async Task SaveAllAsync()
    {
        var entities = _allServices.Select(vm => vm.ToEntity()).ToList();
        await _serviceRepository.SaveAllAsync(entities);
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

        Services = new ObservableCollection<ServiceItemViewModel>(filtered.OrderBy(s => s.Name));
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

            var entity = new ServiceInfo
            {
                Name = serviceName,
                Type = isRest ? ServiceType.RestEndpoint : ServiceType.WindowsService,
                IsConnectingToDb = NewServiceConnectsToDb,
                Status = ServiceStatus.Unknown
            };

            await _serviceRepository.AddAsync(entity);
            _allServices.Add(ServiceItemViewModel.FromEntity(entity));

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

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task StartServiceAsync(ServiceItemViewModel? service)
    {
        if (service == null || !service.IsWindowsService || service.IsBusy) return;

        try
        {
            service.IsBusy = true;
            service.Status = ServiceStatus.StartPending;
            await _windowsServiceManager.StartAsync(service.Name);
            await RefreshSingleServiceAsync(service);
        }
        catch (Exception ex)
        {
            service.Status = ServiceStatus.Error;
            service.ErrorMessage = ex.Message;
        }
        finally
        {
            service.IsBusy = false;
        }
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task StopServiceAsync(ServiceItemViewModel? service)
    {
        if (service == null || !service.IsWindowsService || service.IsBusy) return;

        try
        {
            service.IsBusy = true;
            service.Status = ServiceStatus.StopPending;
            await _windowsServiceManager.StopAsync(service.Name);
            await RefreshSingleServiceAsync(service);
        }
        catch (Exception ex)
        {
            service.Status = ServiceStatus.Error;
            service.ErrorMessage = ex.Message;
        }
        finally
        {
            service.IsBusy = false;
        }
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task RestartServiceAsync(ServiceItemViewModel? service)
    {
        if (service == null || !service.IsWindowsService || service.IsBusy) return;

        try
        {
            service.IsBusy = true;
            service.Status = ServiceStatus.StopPending;
            await _windowsServiceManager.RestartAsync(service.Name);
            await RefreshSingleServiceAsync(service);
        }
        catch (Exception ex)
        {
            service.Status = ServiceStatus.Error;
            service.ErrorMessage = ex.Message;
        }
        finally
        {
            service.IsBusy = false;
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

    [RelayCommand]
    private async Task CopyVersionAsync(ServiceItemViewModel? service)
    {
        if (service == null || string.IsNullOrWhiteSpace(service.Version))
        {
            SetError("No version available to copy");
            return;
        }

        try
        {
            await _clipboardService.SetTextAsync(service.Version);
            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"Failed to copy version: {ex.Message}");
        }
    }
}
