# CLAUDE.md

This file provides guidance to Claude Code when working with code in this repository.

> 📋 **See also**: [plan.md](plan.md) - Complete implementation plan with phase breakdown, architecture details, and implementation status (94% complete).

## Project Overview

**ServicesChecker** is an Avalonia UI (.NET 9) application for monitoring and managing Windows services, REST endpoints, log files, and Docker storage. It implements Clean Architecture with Dependency Injection and modern MVVM using CommunityToolkit.Mvvm source generators.

### Implementation Status (as of 2026-02-05)

**Overall Completion: 94-95%** - Production ready, fully functional

✅ **Completed**:
- All 4 Clean Architecture layers (Domain, Application, Infrastructure, UI)
- All 7 ViewModels with CommunityToolkit.Mvvm source generators
- All 4 AXAML Views with Fluent theme
- Thread-safe JSON repositories with SemaphoreSlim locking
- Docker container switching
- Auto-refresh (Services: 5s, Statistics: 30s)
- Dark/Light/Auto theme with persistence

❌ **Missing**:
- Test projects (Domain.Tests, Application.Tests, Infrastructure.Tests, UI.Tests)
- Custom StatusIndicator control (functionality exists via XAML Ellipse controls)
- Advanced animations (basic styles present)

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

**Thread-Safety**: All JSON repositories use `SemaphoreSlim` for thread-safe read/write operations.

### Theme Support

- Fluent theme with dark/light mode
- Auto (follows system) as default
- Manual toggle button in header
- Preference saved in settings.json

**Theme Modes** (`ThemeMode` enum):
- `System` (0) - Follows Windows system theme (default)
- `Light` (1) - Force light mode
- `Dark` (2) - Force dark mode

## Domain Entities Details

### ServiceInfo
Core properties: `Name`, `Status` (ServiceStatus), `Type` (ServiceType), `Version`, `IsConnectingToDb`
**Enhanced**: `ErrorMessage`, `LastChecked` (DateTime)

### LogFileInfo
Core properties: `FilePath`, `FileName`, `FileSizeBytes`, `Exists` (bool)
**Enhanced**: `LastModified` (DateTime?)

### DockerStorageInfo
Properties: `IsDockerInstalled`, `VhdxFilePath`, `VhdxFileSizeBytes`, `VhdxLastModified`, `DiskSpace` (DiskSpaceInfo), `LastUpdated`
**Nested**: `DiskSpaceInfo` with `DriveLetter`, `TotalBytes`, `AvailableBytes`, `UsedBytes`, `UsedPercentage`, `IsLowSpace`

### ContainerInfo
Properties: `Id`, `Name`, `Image`, `Status`, `IsRunning`, `CreatedAt`

### AppSettings
Properties: `Theme` (ThemeMode), `ServiceRefreshIntervalSeconds` (default: 5), `StatisticsRefreshIntervalSeconds` (default: 30), `SelectedContainerId`
**Enhanced**: `WindowState` with `Left`, `Top`, `Width`, `Height`, `IsMaximized`

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
4. Add CancellationToken parameter to async methods
5. Include proper exception handling and logging

Example:
```csharp
// Application/Interfaces/Services/IMyNewService.cs
public interface IMyNewService
{
    Task<MyResult> DoSomethingAsync(CancellationToken cancellationToken = default);
}

// Infrastructure/Services/MyNewService.cs
public class MyNewService : IMyNewService
{
    public async Task<MyResult> DoSomethingAsync(CancellationToken cancellationToken = default)
    {
        // Implementation with proper error handling
    }
}

// Infrastructure/DependencyInjection/InfrastructureServiceExtensions.cs
services.AddSingleton<IMyNewService, MyNewService>();
```

### Creating New ViewModels
1. Inherit from `ViewModelBase` (provides `IsBusy`, `ErrorMessage`)
2. Use `[ObservableProperty]` for bindable properties
3. Use `[RelayCommand]` for commands
4. Register as transient in `App.axaml.cs`
5. Use `SetError()` method for user-facing errors

Example:
```csharp
public partial class MyNewViewModel : ViewModelBase
{
    private readonly IMyNewService _myService;

    [ObservableProperty]
    private string _myProperty = string.Empty;

    public MyNewViewModel(IMyNewService myService)
    {
        _myService = myService;
    }

    [RelayCommand]
    private async Task DoSomethingAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();
            var result = await _myService.DoSomethingAsync();
            MyProperty = result.Value;
        }
        catch (Exception ex)
        {
            SetError($"Failed to do something: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
```

### Adding New Repository
1. Define interface in `Application/Interfaces/Repositories/`
2. Implement in `Infrastructure/Persistence/`
3. Use `SemaphoreSlim` for thread-safe JSON operations
4. Register in `InfrastructureServiceExtensions.AddInfrastructure()`

Example:
```csharp
// Application/Interfaces/Repositories/IMyRepository.cs
public interface IMyRepository
{
    Task<List<MyEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task SaveAllAsync(List<MyEntity> items, CancellationToken cancellationToken = default);
}

// Infrastructure/Persistence/JsonMyRepository.cs
public class JsonMyRepository : IMyRepository
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly string _filePath;

    // Implementation with locking pattern (see existing repositories)
}

// Registration
services.AddSingleton<IMyRepository, JsonMyRepository>();
```

### Adding New Tab to UI
1. Create `MyTabViewModel.cs` in `UI/ViewModels/`
2. Create `MyTabView.axaml(.cs)` in `UI/Views/`
3. Add property in `MainWindowViewModel`:
   ```csharp
   [ObservableProperty]
   private MyTabViewModel _myTab = null!;
   ```
4. Initialize in `MainWindowViewModel` constructor:
   ```csharp
   MyTab = serviceProvider.GetRequiredService<MyTabViewModel>();
   ```
5. Add TabItem in `MainWindow.axaml`:
   ```xml
   <TabItem Header="My Tab">
       <views:MyTabView DataContext="{Binding MyTab}" />
   </TabItem>
   ```
6. Register ViewModel in `App.axaml.cs`:
   ```csharp
   services.AddTransient<MyTabViewModel>();
   ```

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

## Enhancements Beyond Original Plan

### Infrastructure Improvements
1. **Thread-Safe Repositories**: All JSON repositories (`JsonServiceRepository`, `JsonLogFileRepository`, `JsonSettingsRepository`) use `SemaphoreSlim` for concurrent access protection
2. **CancellationToken Support**: All async interface methods support cancellation
3. **SSL Validation Bypass**: `RestEndpointChecker` bypasses SSL validation for self-signed certificates
4. **Multi-Path VHDX Detection**: `DockerStorageService` checks multiple common Docker Desktop VHDX locations
5. **Enhanced Error Handling**: Proper exception handling with detailed error messages throughout

### Application Layer
- `IWindowsServiceManager.GetExecutablePathAsync()` - Added method to retrieve service executable path
- All service interfaces designed with async/await and CancellationToken from the start

### UI Layer
- **Docker Container Switching**: Dropdown to select and switch between Docker containers (added beyond original plan)
- **Formatted Properties**: ViewModels include formatted strings (`FileSizeFormatted`, `DiskAvailableFormatted`) for better UX
- **Low Disk Space Detection**: Automatic warning when available disk space < 50GB

### Color Palette (Custom)

Defined in `UI/Styles/Colors.axaml`:
- **Accent**: `#0078D4` (Azure Blue)
- **Success/Running/Available**: `#10B981` (Emerald Green)
- **Stopped/Paused**: `#6B7280` (Cool Gray)
- **Error/Unavailable**: `#EF4444` (Red)
- **Pending**: `#F59E0B` (Amber)
- **Unknown**: `#9CA3AF` (Light Gray)

## Architecture Decisions Made

### Why No Application Services Layer?
The original plan included `ServiceMonitorService` and `LogFileMonitorService` in the Application layer. These were **intentionally omitted** because:
- Logic fits naturally in ViewModels (better for MVVM pattern)
- Reduces unnecessary abstraction layers
- ViewModels already orchestrate service calls effectively

### Why No DTOs?
`ServiceStatusDto` and `LogFileStatusDto` were planned but **not implemented** because:
- Direct use of Domain entities works well with current architecture
- No complex mapping needed between layers
- Entity properties already match UI requirements

## Future Development (Optional Enhancements)

### Missing Test Projects (Highest Priority if Testing Needed)

Create test projects:
```bash
dotnet new xunit -n ServicesChecker.Domain.Tests -o tests/ServicesChecker.Domain.Tests
dotnet new xunit -n ServicesChecker.Application.Tests -o tests/ServicesChecker.Application.Tests
dotnet new xunit -n ServicesChecker.Infrastructure.Tests -o tests/ServicesChecker.Infrastructure.Tests
dotnet new xunit -n ServicesChecker.UI.Tests -o tests/ServicesChecker.UI.Tests
dotnet sln add tests/**/*.csproj
```

Add to `Directory.Packages.props`:
```xml
<PackageVersion Include="xunit" Version="2.9.*" />
<PackageVersion Include="Moq" Version="4.20.*" />
<PackageVersion Include="FluentAssertions" Version="7.0.*" />
<PackageVersion Include="xunit.runner.visualstudio" Version="2.8.*" />
<PackageVersion Include="Microsoft.NET.Test.Sdk" Version="17.12.*" />
```

### Custom StatusIndicator Control (Optional Polish)

Currently status indicators use `Ellipse` controls directly in XAML. To improve reusability:

1. Create `UI/Controls/StatusIndicator.axaml(.cs)`
2. Add properties: `Status` (ServiceStatus), `IsAnimated` (bool)
3. Implement pulse animation for `Running` status
4. Use throughout the application for consistency

Example usage:
```xml
<controls:StatusIndicator Status="{Binding Status}" IsAnimated="True" />
```

### Advanced Animations (Optional Polish)

Consider adding:
- Fade-in transitions for tab switching
- Pulse animation for running services (green dot)
- Smooth color transitions on status changes
- Skeleton loading states during refresh

### Potential New Features

1. **Service Groups**: Organize services into collapsible groups
2. **Export to CSV**: Export service statuses or log file lists
3. **Notification System**: Toast notifications for service status changes
4. **REST Endpoint History**: Track uptime/downtime over time
5. **Service Dependencies**: Visualize service dependency chains
6. **Configuration Import/Export**: Share service configurations between machines

## Troubleshooting

### Build Issues
- Ensure .NET 9 SDK is installed: `dotnet --version`
- Clean and rebuild: `dotnet clean && dotnet build`
- Check NuGet restore: `dotnet restore`

### Runtime Issues
- **Services not starting/stopping**: Run as Administrator
- **Docker stats not showing**: Ensure Docker Desktop is installed and running
- **Container list empty**: Check Docker is running and containers exist
- **JSON file errors**: Check app directory has write permissions

### Performance Considerations
- Auto-refresh timers: Services (5s) and Statistics (30s) can be adjusted in AppSettings
- Repository locks: SemaphoreSlim prevents race conditions but may cause brief delays under high concurrency
- Docker CLI calls: May be slow if Docker Desktop is unresponsive

## Related Documentation

- **[plan.md](plan.md)** - Complete implementation plan with phase breakdown and detailed architecture
- **Directory.Build.props** - Centralized MSBuild properties (LangVersion, Nullable, TreatWarningsAsErrors)
- **Directory.Packages.props** - Central Package Management for all NuGet packages
- **global.json** - .NET SDK version specification
