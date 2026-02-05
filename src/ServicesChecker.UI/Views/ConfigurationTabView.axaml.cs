using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ServicesChecker.UI.ViewModels;

namespace ServicesChecker.UI.Views;

public partial class ConfigurationTabView : UserControl
{
    public ConfigurationTabView()
    {
        InitializeComponent();

        // Wire up browse button click
        var browseButton = this.FindControl<Button>("BrowseButton");
        if (browseButton != null)
        {
            browseButton.Click += BrowseButton_Click;
        }
    }

    private async void BrowseButton_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Log File",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Log Files") { Patterns = new[] { "*.log", "*.txt" } },
                new FilePickerFileType("All Files") { Patterns = new[] { "*.*" } }
            }
        });

        if (files.Count > 0 && DataContext is ConfigurationTabViewModel vm)
        {
            var path = files[0].Path.LocalPath;
            await vm.AddLogFileCommand.ExecuteAsync(path);
        }
    }
}
