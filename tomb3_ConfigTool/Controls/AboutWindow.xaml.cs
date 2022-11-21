using System.Windows;
using tomb3_ConfigTool.Models;
using tomb3_ConfigTool.Utils;

namespace tomb3_ConfigTool.Controls;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        DataContext = new BaseLanguageViewModel();
        Owner = Application.Current.MainWindow;
    }

    private void GitHubHyperlink_Click(object sender, RoutedEventArgs e)
    {
        ProcessUtils.Start(Tomb3Config.GitHubURL);
    }
}
