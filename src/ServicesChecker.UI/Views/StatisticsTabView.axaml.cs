using Avalonia.Controls;
using ServicesChecker.UI.ViewModels;

namespace ServicesChecker.UI.Views;

public partial class StatisticsTabView : UserControl
{
    public StatisticsTabView()
    {
        InitializeComponent();

        // Handle attach/detach for auto-refresh
        AttachedToVisualTree += (s, e) =>
        {
            if (DataContext is StatisticsTabViewModel vm)
            {
                vm.StartAutoRefresh();
            }
        };

        DetachedFromVisualTree += (s, e) =>
        {
            if (DataContext is StatisticsTabViewModel vm)
            {
                vm.StopAutoRefresh();
            }
        };
    }
}
