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
    private CancellationTokenSource? _refreshCts;

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
    private async Task AddMultipleLogFilesAsync(IEnumerable<string>? filePaths)
    {
        if (filePaths == null || !filePaths.Any())
            return;

        try
        {
            IsBusy = true;
            ClearError();

            // Get current log files
            var currentLogFiles = await _logFileRepository.GetAllAsync();
            var logFilesList = currentLogFiles.ToList();

            // Create new log file entities
            var newLogFiles = new List<LogFileInfo>();
            foreach (var filePath in filePaths)
            {
                if (string.IsNullOrWhiteSpace(filePath))
                    continue;

                // Skip if already exists in repository
                if (logFilesList.Any(lf => lf.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase)))
                    continue;

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

                newLogFiles.Add(logFile);
                logFilesList.Add(logFile);
            }

            // Save all at once (more efficient than multiple AddAsync calls)
            if (newLogFiles.Count > 0)
            {
                await _logFileRepository.SaveAllAsync(logFilesList);

                // Add to UI collection
                foreach (var logFile in newLogFiles)
                {
                    LogFiles.Add(LogFileItemViewModel.FromEntity(logFile));
                }
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to add log files: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
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

    public void StartAutoRefresh()
    {
        StopAutoRefresh();
        _refreshCts = new CancellationTokenSource();

        _ = Task.Run(async () =>
        {
            while (!_refreshCts.Token.IsCancellationRequested)
            {
                await LoadLogFilesAsync();
                try
                {
                    await Task.Delay(10000, _refreshCts.Token);
                }
                catch (TaskCanceledException)
                {
                    // Expected when cancellation is requested
                    break;
                }
            }
        }, _refreshCts.Token);
    }

    public void StopAutoRefresh()
    {
        _refreshCts?.Cancel();
        _refreshCts?.Dispose();
        _refreshCts = null;
    }
}
