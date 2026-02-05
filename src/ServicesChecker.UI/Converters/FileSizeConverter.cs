using System.Globalization;
using Avalonia.Data.Converters;

namespace ServicesChecker.UI.Converters;

public class FileSizeConverter : IValueConverter
{
    private static readonly string[] Sizes = ["B", "KB", "MB", "GB", "TB"];

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not long bytes || bytes <= 0)
            return "0 B";

        int order = 0;
        double size = bytes;

        while (size >= 1024 && order < Sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }

        return $"{size:0.##} {Sizes[order]}";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
