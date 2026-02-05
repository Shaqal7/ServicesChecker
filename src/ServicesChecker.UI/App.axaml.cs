using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using ServicesChecker.Infrastructure.DependencyInjection;
using ServicesChecker.UI.ViewModels;
using ServicesChecker.UI.Views;

namespace ServicesChecker.UI;

public partial class App : Avalonia.Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Remove Avalonia data validation to avoid duplicates with CommunityToolkit
        BindingPlugins.DataValidators.RemoveAt(0);

        // Configure DI
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainViewModel = Services.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Infrastructure
        services.AddInfrastructure();

        // ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<ServicesTabViewModel>();
        services.AddTransient<ConfigurationTabViewModel>();
        services.AddTransient<StatisticsTabViewModel>();
    }
}
