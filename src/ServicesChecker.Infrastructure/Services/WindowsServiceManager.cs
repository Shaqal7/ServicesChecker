using System.Diagnostics;
using System.ServiceProcess;
using Microsoft.Win32;
using ServicesChecker.Application.Interfaces.Services;
using ServicesChecker.Domain.Enums;

namespace ServicesChecker.Infrastructure.Services;

public class WindowsServiceManager : IWindowsServiceManager
{
    private const int TimeoutSeconds = 30;

    public Task<ServiceStatus> GetStatusAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            try
            {
                using var service = new ServiceController(serviceName);
                return MapStatus(service.Status);
            }
            catch (InvalidOperationException)
            {
                return ServiceStatus.Error;
            }
            catch (Exception)
            {
                return ServiceStatus.Unknown;
            }
        }, cancellationToken);
    }

    public Task StartAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            using var service = new ServiceController(serviceName);
            if (service.Status != ServiceControllerStatus.Running &&
                service.Status != ServiceControllerStatus.StartPending)
            {
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(TimeoutSeconds));
            }
        }, cancellationToken);
    }

    public Task StopAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            using var service = new ServiceController(serviceName);
            if (service.Status != ServiceControllerStatus.Stopped &&
                service.Status != ServiceControllerStatus.StopPending)
            {
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(TimeoutSeconds));
            }
        }, cancellationToken);
    }

    public async Task RestartAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        await StopAsync(serviceName, cancellationToken);
        await StartAsync(serviceName, cancellationToken);
    }

    public Task<string?> GetVersionAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        return Task.Run(async () =>
        {
            var exePath = await GetExecutablePathAsync(serviceName, cancellationToken);
            if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
                return "Unknown";

            try
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(exePath);
                return versionInfo.FileVersion ?? versionInfo.ProductVersion ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }, cancellationToken);
    }

    public Task<string?> GetExecutablePathAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Services\{serviceName}");
                var imagePath = key?.GetValue("ImagePath")?.ToString();

                if (string.IsNullOrEmpty(imagePath))
                    return null;

                // Remove quotes and arguments
                imagePath = imagePath.Trim('"');
                var spaceIndex = imagePath.IndexOf(' ');
                if (spaceIndex > 0 && !File.Exists(imagePath))
                {
                    imagePath = imagePath[..spaceIndex];
                }

                return imagePath;
            }
            catch
            {
                return null;
            }
        }, cancellationToken);
    }

    public Task<bool> ServiceExistsAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            try
            {
                using var service = new ServiceController(serviceName);
                // Access a property to trigger validation
                _ = service.Status;
                return true;
            }
            catch (InvalidOperationException)
            {
                // Service doesn't exist
                return false;
            }
            catch
            {
                // Other errors (e.g., access denied) - assume service exists but we can't access it
                return true;
            }
        }, cancellationToken);
    }

    private static ServiceStatus MapStatus(ServiceControllerStatus status)
    {
        return status switch
        {
            ServiceControllerStatus.Running => ServiceStatus.Running,
            ServiceControllerStatus.Stopped => ServiceStatus.Stopped,
            ServiceControllerStatus.StartPending => ServiceStatus.StartPending,
            ServiceControllerStatus.StopPending => ServiceStatus.StopPending,
            ServiceControllerStatus.Paused => ServiceStatus.Paused,
            ServiceControllerStatus.PausePending => ServiceStatus.Paused,
            ServiceControllerStatus.ContinuePending => ServiceStatus.StartPending,
            _ => ServiceStatus.Unknown
        };
    }
}
