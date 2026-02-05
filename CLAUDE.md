# CLAUDE.md

This file provides guidance to Claude Code when working with code in this repository.

## Project Overview

**ServicesChecker** is an Avalonia UI (.NET 9) application for monitoring and managing Windows services, REST endpoints, log files, and Docker storage. It implements Clean Architecture with Dependency Injection and modern MVVM using CommunityToolkit.Mvvm source generators.

## Commands

### Build
```bash
dotnet build ServicesChecker.sln
```

### Run
```bash
dotnet run --project src/ServicesChecker.UI
```

### Clean
```bash
dotnet clean ServicesChecker.sln
```

## Architecture

### Clean Architecture Layers

```
ServicesChecker/
├── src/
│   ├── ServicesChecker.Domain/        # Core entities, enums - zero dependencies
│   ├── ServicesChecker.Application/   # Interfaces, use cases - depends on Domain
│   ├── ServicesChecker.Infrastructure/# Implementations - depends on Application
│   └── ServicesChecker.UI/            # Avalonia Views + ViewModels - depends on Infrastructure
```

### Layer Responsibilities

- **Domain**: Business entities (`ServiceInfo`, `LogFileInfo`, `DockerStorageInfo`), enums (`ServiceStatus`, `ServiceType`, `ThemeMode`)
- **Application**: Service interfaces (`IWindowsServiceManager`, `IRestEndpointChecker`, `IDockerStorageService`, `IDockerContainerManager`, `IFileSystemService`), Repository interfaces (`IServiceRepository`, `ILogFileRepository`, `ISettingsRepository`)
- **Infrastructure**: Implementations using Windows APIs, HttpClient, file system, Docker CLI
- **UI**: Avalonia views, ViewModels with CommunityToolkit.Mvvm, converters

### MVVM with CommunityToolkit.Mvvm Source Generators

ViewModels use source generators for clean, concise code:

```csharp
public partial class ServicesTabViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ServiceItemViewModel> _services = [];

    [ObservableProperty]
    private string _filterText = string.Empty;

    [RelayCommand]
    private async Task RefreshServicesAsync() { ... }

    [RelayCommand]
    private async Task StartServiceAsync(ServiceItemViewModel service) { ... }
}
```

### Dependency Injection Setup

DI is configured in `App.axaml.cs`:

```csharp
private static void ConfigureServices(IServiceCollection services)
{
    // Infrastructure
    services.AddInfrastructure(); // Extension method in Infrastructure

    // ViewModels
    services.AddTransient<MainWindowViewModel>();
    services.AddTransient<ServicesTabViewModel>();
    services.AddTransient<ConfigurationTabViewModel>();
    services.AddTransient<StatisticsTabViewModel>();
}
```

### Key Features

1. **Services Tab**
   - Monitor Windows services and REST endpoints
   - Start/Stop/Restart services (context menu)
   - Filter by name and "Connecting to DB"
   - Switch Docker containers
   - Auto-refresh every 5 seconds

2. **Configuration Tab**
   - Track log files (browse to add)
   - Status indicator (exists/missing)
   - Delete files from disk
   - Persistent tracking even after deletion

3. **Statistics Tab**
   - Docker Desktop installation status
   - VHDX file size and location
   - Disk space usage with progress bar
   - Low disk space warning (<50GB)
   - Auto-refresh every 30 seconds

### Data Persistence

JSON files in application directory (portable mode):
- `services.json` - Service configurations
- `logfiles.json` - Log file paths
- `settings.json` - Application settings (theme, window state, selected container)

### Theme Support

- Fluent theme with dark/light mode
- Auto (follows system) as default
- Manual toggle button in header
- Preference saved in settings.json

## Project Structure

```
src/
├── ServicesChecker.Domain/
│   ├── Entities/
│   │   ├── ServiceInfo.cs
│   │   ├── LogFileInfo.cs
│   │   ├── DockerStorageInfo.cs
│   │   ├── ContainerInfo.cs
│   │   └── AppSettings.cs
│   └── Enums/
│       ├── ServiceStatus.cs
│       ├── ServiceType.cs
│       └── ThemeMode.cs
│
├── ServicesChecker.Application/
│   └── Interfaces/
│       ├── Services/
│       │   ├── IWindowsServiceManager.cs
│       │   ├── IRestEndpointChecker.cs
│       │   ├── IDockerStorageService.cs
│       │   ├── IDockerContainerManager.cs
│       │   └── IFileSystemService.cs
│       └── Repositories/
│           ├── IServiceRepository.cs
│           ├── ILogFileRepository.cs
│           └── ISettingsRepository.cs
│
├── ServicesChecker.Infrastructure/
│   ├── Services/
│   │   ├── WindowsServiceManager.cs
│   │   ├── RestEndpointChecker.cs
│   │   ├── DockerStorageService.cs
│   │   ├── DockerContainerManager.cs
│   │   └── FileSystemService.cs
│   ├── Persistence/
│   │   ├── JsonServiceRepository.cs
│   │   ├── JsonLogFileRepository.cs
│   │   └── JsonSettingsRepository.cs
│   └── DependencyInjection/
│       └── InfrastructureServiceExtensions.cs
│
└── ServicesChecker.UI/
    ├── App.axaml(.cs)
    ├── Program.cs
    ├── ViewModels/
    │   ├── ViewModelBase.cs
    │   ├── MainWindowViewModel.cs
    │   ├── ServicesTabViewModel.cs
    │   ├── ConfigurationTabViewModel.cs
    │   ├── StatisticsTabViewModel.cs
    │   ├── ServiceItemViewModel.cs
    │   └── LogFileItemViewModel.cs
    ├── Views/
    │   ├── MainWindow.axaml(.cs)
    │   ├── ServicesTabView.axaml(.cs)
    │   ├── ConfigurationTabView.axaml(.cs)
    │   └── StatisticsTabView.axaml(.cs)
    └── Converters/
        ├── StatusToColorConverter.cs
        ├── BoolToVisibilityConverter.cs
        └── FileSizeConverter.cs
```

## NuGet Packages

Managed centrally in `Directory.Packages.props`:

- **Avalonia** 11.2.3 - UI framework
- **Avalonia.Themes.Fluent** - Modern Fluent theme
- **Avalonia.Controls.DataGrid** - DataGrid control
- **CommunityToolkit.Mvvm** 8.4.0 - MVVM with source generators
- **Microsoft.Extensions.DependencyInjection** - DI container
- **System.ServiceProcess.ServiceController** - Windows service management

## Important Patterns

### Adding New Service Interfaces
1. Define interface in `Application/Interfaces/Services/`
2. Implement in `Infrastructure/Services/`
3. Register in `InfrastructureServiceExtensions.AddInfrastructure()`

### Creating New ViewModels
1. Inherit from `ViewModelBase` (provides `IsBusy`, `ErrorMessage`)
2. Use `[ObservableProperty]` for bindable properties
3. Use `[RelayCommand]` for commands
4. Register as transient in `App.axaml.cs`

### Status Colors
```
Running/Available: #10B981 (Green)
Stopped/Paused:    #6B7280 (Gray)
Error/Unavailable: #EF4444 (Red)
Pending:           #F59E0B (Amber)
Unknown:           #9CA3AF (Light Gray)
```

## Windows-Specific Notes

- `WindowsServiceManager` uses `ServiceController` API (Windows-only)
- `DockerContainerManager` uses Docker CLI (`docker` command)
- Application requires administrator privileges to start/stop services
