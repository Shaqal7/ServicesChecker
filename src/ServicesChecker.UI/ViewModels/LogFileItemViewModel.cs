using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using ServicesChecker.Domain.Entities;

namespace ServicesChecker.UI.ViewModels;

public partial class LogFileItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _filePath = string.Empty;

    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FileSizeFormatted))]
    private long _fileSizeBytes;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusColor))]
    private bool _exists;

    [ObservableProperty]
    private DateTime? _lastModified;

    public string FileSizeFormatted
    {
        get
        {
            if (FileSizeBytes <= 0) return "0 B";

            string[] sizes = ["B", "KB", "MB", "GB", "TB"];
            int order = 0;
            double size = FileSizeBytes;

            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            return $"{size.ToString("0.##", CultureInfo.InvariantCulture)} {sizes[order]}";
        }
    }

    public string StatusColor => Exists ? "#10B981" : "#EF4444";

    public static LogFileItemViewModel FromEntity(LogFileInfo entity)
    {
        return new LogFileItemViewModel
        {
            FilePath = entity.FilePath,
            FileName = entity.FileName,
            FileSizeBytes = entity.FileSizeBytes,
            Exists = entity.Exists,
            LastModified = entity.LastModified
        };
    }

    public LogFileInfo ToEntity()
    {
        return new LogFileInfo
        {
            FilePath = FilePath,
            FileName = FileName,
            FileSizeBytes = FileSizeBytes,
            Exists = Exists,
            LastModified = LastModified
        };
    }
}
