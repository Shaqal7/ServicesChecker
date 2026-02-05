using Avalonia;
using Avalonia.Headless;

namespace ServicesChecker.UI.Tests;

public static class AppBuilderHelper
{
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
    }
}
