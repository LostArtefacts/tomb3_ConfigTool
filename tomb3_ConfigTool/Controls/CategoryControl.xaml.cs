using System.Windows.Controls;
using tomb3_ConfigTool.Models;

namespace tomb3_ConfigTool.Controls;

public partial class CategoryControl : UserControl
{
    public CategoryControl()
    {
        InitializeComponent();
    }

    private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        (DataContext as CategoryViewModel).ViewPosition = e.VerticalOffset;
    }
}
