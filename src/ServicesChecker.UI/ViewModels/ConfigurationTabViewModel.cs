using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServicesChecker.Application.Interfaces.Repositories;
using ServicesChecker.Application.Interfaces.Services;
using ServicesChecker.Domain.Entities;

namespace ServicesChecker.UI.ViewModels;

public partial class ConfigurationTabViewModel : ViewModelBase
{
    private readonly ILogFileRepository _logFileRepository;
    private readonly IFileSystemService _fileSystemService;

    [ObservableProperty]
    private ObservableCollection<LogFileItemViewModel> _logFiles = [];

    [ObservableProperty]
    private LogFileItemViewModel? _selectedLogFile;

    public ConfigurationTabViewModel(
        ILogFileRepository logFileRepository,
        IFileSystemService fileSystemService)
    {
        _logFileRepository = logFileRepository;
        _fileSystemService = fileSystemService;

        _ = LoadLogFilesAsync();
    }

    [RelayCommand]
    private async Task LoadLogFilesAsync()
    {
        try
        {
            IsBusy = true;
            ClearError();

            var logFiles = await _logFileRepository.GetAllAsync();
            var viewModels = new List<LogFileItemViewModel>();

            foreach (var logFile in logFiles)
            {
                logFile.Exists = _fileSystemService.FileExists(logFile.FilePath);
                if (logFile.Exists)
                {
                    logFile.FileSizeBytes = _fileSystemService.GetFileSize(logFile.FilePath);
                    logFile.LastModified = _fileSystemService.GetLastModified(logFile.FilePath);
                }

                viewModels.Add(LogFileItemViewModel.FromEntity(logFile));
            }

            LogFiles = new ObservableCollection<LogFileItemViewModel>(viewModels);
        }
        catch (Exception ex)
        {
            SetError($"Failed to load log files: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddLogFileAsync(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return;

        try
        {
            var logFile = new LogFileInfo
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                Exists = _fileSystemService.FileExists(filePath)
            };

            if (logFile.Exists)
            {
                logFile.FileSizeBytes = _fileSystemService.GetFileSize(filePath);
                logFile.LastModified = _fileSystemService.GetLastModified(filePath);
            }

            await _logFileRepository.AddAsync(logFile);
            LogFiles.Add(LogFileItemViewModel.FromEntity(logFile));
        }
        catch (Exception ex)
        {
            SetError($"Failed to add log file: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task RemoveLogFileAsync(LogFileItemViewModel? logFile)
    {
        if (logFile == null) return;

        try
        {
            await _logFileRepository.RemoveAsync(logFile.FilePath);
            LogFiles.Remove(logFile);
        }
        catch (Exception ex)
        {
            SetError($"Failed to remove log file: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task DeleteFromDiskAsync(LogFileItemViewModel? logFile)
    {
        if (logFile == null || !logFile.Exists) return;

        try
        {
            IsBusy = true;
            await _fileSystemService.DeleteFileAsync(logFile.FilePath);

            // Refresh the list to update status
            await LoadLogFilesAsync();
        }
        catch (Exception ex)
        {
            SetError($"Failed to delete file: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadLogFilesAsync();
    }
}
