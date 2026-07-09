using System.Windows;

namespace TransportExpenditureTracker.Desktop;

public partial class App : Application
{
    private async void OnStartup(object sender, StartupEventArgs e)
    {
        var webHost = new WebHostRunner();
        var port = await webHost.StartAsync();
        var mainWindow = new MainWindow(port, webHost);
        mainWindow.Show();
    }
}