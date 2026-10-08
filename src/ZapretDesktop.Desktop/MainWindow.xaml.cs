using System.Windows;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace ZapretDesktop.Desktop;

public partial class MainWindow : FluentWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnToggleThemeClick(object sender, RoutedEventArgs e)
    {
        var currentTheme = ApplicationThemeManager.GetAppTheme();
        var newTheme = currentTheme == ApplicationTheme.Dark 
            ? ApplicationTheme.Light 
            : ApplicationTheme.Dark;

        ApplicationThemeManager.Apply(newTheme);
    }
}