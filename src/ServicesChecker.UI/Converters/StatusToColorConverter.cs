using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace ServicesChecker.UI.Converters;

public class StatusToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isRunning)
        {
            return isRunning
                ? new SolidColorBrush(Color.Parse("#10B981"))
                : new SolidColorBrush(Color.Parse("#6B7280"));
        }

        if (value is string colorString && !string.IsNullOrEmpty(colorString))
        {
            return new SolidColorBrush(Color.Parse(colorString));
        }

        return new SolidColorBrush(Color.Parse("#9CA3AF"));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
