using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using tomb3_ConfigTool.Models;
using tomb3_ConfigTool.Utils;

namespace tomb3_ConfigTool.Controls;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        (DataContext as MainWindowViewModel).Exit(e);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        (DataContext as MainWindowViewModel).Load();
    }

    private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0
            && e.AddedItems[0] is CategoryViewModel
            && VisualUtils.GetChild(sender as DependencyObject, typeof(ScrollViewer)) is ScrollViewer scroller)
        {
            scroller.ScrollToVerticalOffset((DataContext as MainWindowViewModel).SelectedCategory.ViewPosition);
        }
    }
}
