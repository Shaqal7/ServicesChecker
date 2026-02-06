# Services Checker

A modern Windows desktop application for monitoring and managing Windows services, REST endpoints, log files, and Docker storage.

![.NET Version](https://img.shields.io/badge/.NET-10.0-blue)
![Avalonia](https://img.shields.io/badge/Avalonia-11.2.3-purple)
![License](https://img.shields.io/badge/license-MIT-green)

## 🎯 Overview

**ServicesChecker** is a cross-platform desktop application built with Avalonia UI and .NET 10, designed to help developers and system administrators monitor and manage various system resources from a single, intuitive interface. The application follows Clean Architecture principles and implements modern MVVM patterns using CommunityToolkit.Mvvm source generators.

## ✨ Features

### 🔧 Services Management
- **Monitor Windows Services** - Real-time status tracking of Windows services
- **Monitor REST Endpoints** - Check availability and health of HTTP/REST APIs
- **Service Control** - Start, stop, and restart services with context menu actions
- **Smart Filtering** - Filter services by name and connection type
- **Docker Container Switching** - Easily switch between Docker containers
- **Auto-refresh** - Automatic status updates every 5 seconds

### 📄 Log File Management
- **Track Log Files** - Monitor multiple log file locations
- **Status Indicators** - Visual indicators for file existence
- **File Operations** - Delete files directly from the application
- **Persistent Tracking** - Maintain tracking even after file deletion
- **Browse & Add** - Easy file browser integration

### 📊 Docker Storage Statistics
- **Docker Desktop Status** - Check if Docker Desktop is installed
- **VHDX File Monitoring** - Track Docker virtual hard disk size and location
- **Disk Space Analysis** - Real-time disk space usage with progress visualization
- **Low Space Warnings** - Automatic alerts when disk space falls below 50GB
- **Auto-refresh** - Updates every 30 seconds

### 🎨 User Interface
- **Modern Fluent Design** - Beautiful, modern UI with Fluent theme
- **Dark/Light Mode** - Full theme support with automatic system detection
- **Responsive Layout** - Adaptive interface for different screen sizes
- **Tabbed Interface** - Organized tabs for Services, Configuration, and Statistics

## 🏗️ Architecture

The application follows **Clean Architecture** principles with clear separation of concerns:

```
ServicesChecker/
├── Domain/          # Core entities, enums (zero dependencies)
├── Application/     # Interfaces, use cases
├── Infrastructure/  # Implementations (Windows APIs, Docker CLI, file system)
└── UI/              # Avalonia Views & ViewModels
```

### Technology Stack

- **UI Framework**: Avalonia UI 11.2.3
- **Runtime**: .NET 10.0
- **MVVM**: CommunityToolkit.Mvvm 8.4.0 (with source generators)
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Service Management**: System.ServiceProcess.ServiceController
- **Theme**: Avalonia.Themes.Fluent

## 📋 Requirements

- **Operating System**: Windows 10/11 (for Windows service management features)
- **.NET SDK**: 10.0 or higher
- **Docker Desktop**: (Optional) Required for Docker-related features
- **Administrator Privileges**: Required for starting/stopping Windows services

## 🚀 Getting Started

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/Shaqal7/ServicesChecker.git
   cd ServicesChecker
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore ServicesChecker.sln
   ```

3. **Build the solution**
   ```bash
   dotnet build ServicesChecker.sln
   ```

4. **Run the application**
   ```bash
   dotnet run --project src/ServicesChecker.UI
   ```

### Building for Release

```bash
dotnet build ServicesChecker.sln -c Release
```

### Cleaning Build Artifacts

```bash
dotnet clean ServicesChecker.sln
```

## 📖 Usage

### Services Tab

1. **Monitor Services**: View real-time status of configured Windows services and REST endpoints
2. **Control Services**: Right-click on a service to access Start/Stop/Restart options
3. **Filter Services**: Use the filter box to search by service name
4. **Switch Containers**: Select different Docker containers from the dropdown menu

### Configuration Tab

1. **Add Log Files**: Click "Browse" to add log file paths to monitor
2. **Track Status**: See if files exist or are missing with visual indicators
3. **Delete Files**: Remove files from disk with the delete action

### Statistics Tab

1. **View Docker Status**: Check if Docker Desktop is installed
2. **Monitor Storage**: See VHDX file size and location
3. **Check Disk Space**: View available disk space with progress bar
4. **Warnings**: Get notified when disk space is low

### Data Storage

Application data is stored in JSON files in the application directory:
- `services.json` - Service configurations
- `logfiles.json` - Log file paths
- `settings.json` - Application settings (theme, window state, selected container)

## 🛠️ Development

### Project Structure

```
src/
├── ServicesChecker.Domain/          # Core business entities and enums
│   ├── Entities/                    # ServiceInfo, LogFileInfo, DockerStorageInfo, etc.
│   └── Enums/                       # ServiceStatus, ServiceType, ThemeMode
│
├── ServicesChecker.Application/     # Business logic interfaces
│   └── Interfaces/
│       ├── Services/                # Service abstractions
│       └── Repositories/            # Repository abstractions
│
├── ServicesChecker.Infrastructure/  # External concerns implementation
│   ├── Services/                    # Windows API, Docker CLI, HTTP clients
│   ├── Persistence/                 # JSON repositories
│   └── DependencyInjection/         # DI configuration
│
└── ServicesChecker.UI/              # Avalonia UI layer
    ├── ViewModels/                  # MVVM ViewModels with CommunityToolkit
    ├── Views/                       # AXAML views
    └── Converters/                  # Value converters
```

### Adding New Features

1. **Define entities** in `Domain` layer
2. **Create interfaces** in `Application` layer
3. **Implement services** in `Infrastructure` layer
4. **Register in DI** container via `InfrastructureServiceExtensions`
5. **Create ViewModels** using `[ObservableProperty]` and `[RelayCommand]` attributes
6. **Build views** in Avalonia AXAML

### MVVM Pattern

ViewModels leverage CommunityToolkit.Mvvm source generators for clean code:

```csharp
public partial class MyViewModel : ObservableObject
{
    [ObservableProperty]
    private string _myProperty = string.Empty;

    [RelayCommand]
    private async Task MyCommandAsync()
    {
        // Implementation
    }
}
```

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgments

- Built with [Avalonia UI](https://avaloniaui.net/) - A cross-platform UI framework
- Uses [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) for MVVM implementation
- Clean Architecture principles inspired by [Jason Taylor](https://github.com/jasontaylordev)

## 📞 Contact

Project Link: [https://github.com/Shaqal7/ServicesChecker](https://github.com/Shaqal7/ServicesChecker)

---

Made with ❤️ using .NET and Avalonia
