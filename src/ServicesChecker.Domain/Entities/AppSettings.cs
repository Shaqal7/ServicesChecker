using ServicesChecker.Domain.Enums;

namespace ServicesChecker.Domain.Entities;

/// <summary>
/// Represents application settings.
/// </summary>
public class AppSettings
{
    public ThemeMode Theme { get; set; } = ThemeMode.System;
    public int ServiceRefreshIntervalSeconds { get; set; } = 5;
    public int StatisticsRefreshIntervalSeconds { get; set; } = 30;
    public string? SelectedContainerId { get; set; }
    public WindowState Window { get; set; } = new();
}

/// <summary>
/// Represents window state for persistence.
/// </summary>
public class WindowState
{
    public double? Left { get; set; }
    public double? Top { get; set; }
    public double? Width { get; set; }
    public double? Height { get; set; }
    public bool IsMaximized { get; set; }
}
