using Avalonia.Controls;
using ServicesChecker.UI.ViewModels;

namespace ServicesChecker.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Closing += OnClosing;
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.Cleanup();
        }
    }
}
