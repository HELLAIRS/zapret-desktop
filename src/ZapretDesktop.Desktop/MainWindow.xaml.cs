using System.Globalization;
using System.Windows.Data;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace ZapretDesktop.Desktop;

public partial class MainWindow : FluentWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnToggleThemeClick(object sender, object e)
    {
        var currentTheme = ApplicationThemeManager.GetAppTheme();
        var newTheme = currentTheme == ApplicationTheme.Dark 
            ? ApplicationTheme.Light 
            : ApplicationTheme.Dark;

        ApplicationThemeManager.Apply(newTheme);
    }
}

public class BooleanToStopStartConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool isRunning && isRunning ? "Остановить" : "Запустить";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}