using Microsoft.Extensions.DependencyInjection;
using ServicesChecker.Application.Interfaces.Repositories;
using ServicesChecker.Application.Interfaces.Services;
using ServicesChecker.Infrastructure.Persistence;
using ServicesChecker.Infrastructure.Services;

namespace ServicesChecker.Infrastructure.DependencyInjection;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Services
        services.AddSingleton<IFileSystemService, FileSystemService>();
        services.AddSingleton<IWindowsServiceManager, WindowsServiceManager>();
        services.AddSingleton<IDockerStorageService, DockerStorageService>();
        services.AddSingleton<IDockerContainerManager, DockerContainerManager>();

        // HTTP client for REST endpoint checking
        services.AddHttpClient<IRestEndpointChecker, RestEndpointChecker>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true // Allow self-signed certs
        });

        // Repositories
        services.AddSingleton<IServiceRepository, JsonServiceRepository>();
        services.AddSingleton<ILogFileRepository, JsonLogFileRepository>();
        services.AddSingleton<ISettingsRepository, JsonSettingsRepository>();

        return services;
    }
}
