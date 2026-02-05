using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace ServicesChecker.UI.Controls;

/// <summary>
/// A reusable status indicator control that displays a colored circle.
/// Replaces duplicated Ellipse usage throughout the application.
/// </summary>
public class StatusIndicator : Ellipse
{
    /// <summary>
    /// Defines the <see cref="StatusColor"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> StatusColorProperty =
        AvaloniaProperty.Register<StatusIndicator, IBrush?>(
            nameof(StatusColor),
            new SolidColorBrush(Colors.Gray));

    /// <summary>
    /// Defines the <see cref="Size"/> property.
    /// </summary>
    public static readonly StyledProperty<double> SizeProperty =
        AvaloniaProperty.Register<StatusIndicator, double>(
            nameof(Size),
            10.0,
            coerce: CoerceSize);

    /// <summary>
    /// Defines the <see cref="IsAnimated"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsAnimatedProperty =
        AvaloniaProperty.Register<StatusIndicator, bool>(nameof(IsAnimated), false);

    static StatusIndicator()
    {
        // Set default alignment
        HorizontalAlignmentProperty.OverrideDefaultValue<StatusIndicator>(Avalonia.Layout.HorizontalAlignment.Center);
        VerticalAlignmentProperty.OverrideDefaultValue<StatusIndicator>(Avalonia.Layout.VerticalAlignment.Center);

        // Register property changed handlers
        StatusColorProperty.Changed.AddClassHandler<StatusIndicator>((indicator, args) =>
            indicator.UpdateFill(args.NewValue as IBrush));
        SizeProperty.Changed.AddClassHandler<StatusIndicator>((indicator, args) =>
            indicator.UpdateSize((double)args.NewValue!));
    }

    public StatusIndicator()
    {
        // Initialize with default values
        UpdateFill(StatusColor);
        UpdateSize(Size);
    }

    /// <summary>
    /// Gets or sets the brush used to fill the status indicator.
    /// </summary>
    public IBrush? StatusColor
    {
        get => GetValue(StatusColorProperty);
        set => SetValue(StatusColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the size (width and height) of the status indicator.
    /// </summary>
    public double Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the status indicator should be animated (e.g., pulsing for running services).
    /// This property is reserved for future animation implementation.
    /// </summary>
    public bool IsAnimated
    {
        get => GetValue(IsAnimatedProperty);
        set => SetValue(IsAnimatedProperty, value);
    }

    private static double CoerceSize(AvaloniaObject sender, double value)
    {
        // Ensure size is positive
        return Math.Max(0, value);
    }

    private void UpdateFill(IBrush? brush)
    {
        Fill = brush ?? new SolidColorBrush(Colors.Gray);
    }

    private void UpdateSize(double size)
    {
        Width = size;
        Height = size;
    }
}
