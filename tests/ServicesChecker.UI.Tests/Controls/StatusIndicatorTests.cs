using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using FluentAssertions;
using ServicesChecker.UI.Controls;

namespace ServicesChecker.UI.Tests.Controls;

public class StatusIndicatorTests
{
    [AvaloniaFact]
    public void Constructor_SetsDefaultValues()
    {
        // Arrange & Act
        var indicator = new StatusIndicator();

        // Assert
        indicator.Size.Should().Be(10.0);
        indicator.StatusColor.Should().NotBeNull();
        indicator.IsAnimated.Should().BeFalse();
    }

    [AvaloniaFact]
    public void Constructor_InitializesWidthAndHeight()
    {
        // Arrange & Act
        var indicator = new StatusIndicator();

        // Assert
        indicator.Width.Should().Be(10.0);
        indicator.Height.Should().Be(10.0);
    }

    [AvaloniaFact]
    public void Constructor_InitializesFill()
    {
        // Arrange & Act
        var indicator = new StatusIndicator();

        // Assert
        indicator.Fill.Should().NotBeNull();
        indicator.Fill.Should().BeOfType<SolidColorBrush>();
    }

    [AvaloniaFact]
    public void Size_SetValue_UpdatesWidthAndHeight()
    {
        // Arrange
        var indicator = new StatusIndicator();
        const double newSize = 20.0;

        // Act
        indicator.Size = newSize;

        // Assert
        indicator.Size.Should().Be(newSize);
        indicator.Width.Should().Be(newSize);
        indicator.Height.Should().Be(newSize);
    }

    [AvaloniaFact]
    public void Size_SetToZero_IsAllowed()
    {
        // Arrange
        var indicator = new StatusIndicator();

        // Act
        indicator.Size = 0;

        // Assert
        indicator.Size.Should().Be(0);
        indicator.Width.Should().Be(0);
        indicator.Height.Should().Be(0);
    }

    [AvaloniaFact]
    public void Size_SetToNegativeValue_CoercesToZero()
    {
        // Arrange
        var indicator = new StatusIndicator();

        // Act
        indicator.Size = -5.0;

        // Assert
        indicator.Size.Should().Be(0);
        indicator.Width.Should().Be(0);
        indicator.Height.Should().Be(0);
    }

    [AvaloniaFact]
    public void StatusColor_SetValue_UpdatesFill()
    {
        // Arrange
        var indicator = new StatusIndicator();
        var greenBrush = new SolidColorBrush(Colors.Green);

        // Act
        indicator.StatusColor = greenBrush;

        // Assert
        indicator.StatusColor.Should().Be(greenBrush);
        indicator.Fill.Should().Be(greenBrush);
    }

    [AvaloniaFact]
    public void StatusColor_SetToNull_UsesFallbackBrush()
    {
        // Arrange
        var indicator = new StatusIndicator();
        var greenBrush = new SolidColorBrush(Colors.Green);
        indicator.StatusColor = greenBrush;

        // Act
        indicator.StatusColor = null;

        // Assert
        indicator.Fill.Should().NotBeNull();
        indicator.Fill.Should().BeOfType<SolidColorBrush>();
        var fill = (SolidColorBrush)indicator.Fill!;
        fill.Color.Should().Be(Colors.Gray);
    }

    [AvaloniaFact]
    public void StatusColor_SetDifferentColors_UpdatesFillEachTime()
    {
        // Arrange
        var indicator = new StatusIndicator();
        var redBrush = new SolidColorBrush(Colors.Red);
        var blueBrush = new SolidColorBrush(Colors.Blue);

        // Act & Assert - First color
        indicator.StatusColor = redBrush;
        indicator.Fill.Should().Be(redBrush);

        // Act & Assert - Second color
        indicator.StatusColor = blueBrush;
        indicator.Fill.Should().Be(blueBrush);
    }

    [AvaloniaFact]
    public void HorizontalAlignment_DefaultValue_IsCenter()
    {
        // Arrange & Act
        var indicator = new StatusIndicator();

        // Assert
        indicator.HorizontalAlignment.Should().Be(Avalonia.Layout.HorizontalAlignment.Center);
    }

    [AvaloniaFact]
    public void VerticalAlignment_DefaultValue_IsCenter()
    {
        // Arrange & Act
        var indicator = new StatusIndicator();

        // Assert
        indicator.VerticalAlignment.Should().Be(Avalonia.Layout.VerticalAlignment.Center);
    }

    [AvaloniaFact]
    public void IsAnimated_SetValue_UpdatesProperty()
    {
        // Arrange
        var indicator = new StatusIndicator();

        // Act
        indicator.IsAnimated = true;

        // Assert
        indicator.IsAnimated.Should().BeTrue();
    }

    [AvaloniaFact]
    public void Size_MultipleUpdates_WorksCorrectly()
    {
        // Arrange
        var indicator = new StatusIndicator();

        // Act & Assert - Multiple size changes
        indicator.Size = 15.0;
        indicator.Width.Should().Be(15.0);
        indicator.Height.Should().Be(15.0);

        indicator.Size = 8.0;
        indicator.Width.Should().Be(8.0);
        indicator.Height.Should().Be(8.0);

        indicator.Size = 25.0;
        indicator.Width.Should().Be(25.0);
        indicator.Height.Should().Be(25.0);
    }

    [AvaloniaTheory]
    [InlineData(5.0)]
    [InlineData(8.0)]
    [InlineData(10.0)]
    [InlineData(15.0)]
    [InlineData(20.0)]
    public void Size_VariousValidValues_UpdatesCorrectly(double size)
    {
        // Arrange
        var indicator = new StatusIndicator();

        // Act
        indicator.Size = size;

        // Assert
        indicator.Size.Should().Be(size);
        indicator.Width.Should().Be(size);
        indicator.Height.Should().Be(size);
    }

    [AvaloniaFact]
    public void StatusColor_WithSolidColorBrush_UpdatesFillCorrectly()
    {
        // Arrange
        var indicator = new StatusIndicator();
        var color = Color.Parse("#10B981"); // Green from StatusToColorConverter
        var brush = new SolidColorBrush(color);

        // Act
        indicator.StatusColor = brush;

        // Assert
        indicator.Fill.Should().Be(brush);
        var fillBrush = indicator.Fill as SolidColorBrush;
        fillBrush.Should().NotBeNull();
        fillBrush!.Color.Should().Be(color);
    }
}
