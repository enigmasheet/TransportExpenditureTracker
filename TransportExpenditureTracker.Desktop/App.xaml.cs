using System.Windows;

namespace TransportExpenditureTracker.Desktop;

public partial class App : Application, IDisposable
{
    private Mutex? _singleInstanceMutex;

    private async void OnStartup(object sender, StartupEventArgs e)
    {
        _singleInstanceMutex = new Mutex(true, "TransportExpenditureTracker.SingleInstance", out var acquiredNew);
        if (!acquiredNew)
        {
            MessageBox.Show(
                "Expense Tracker is already running.",
                "Expense Tracker",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            Shutdown();
            return;
        }

        try
        {
            var webHost = new WebHostRunner();
            var port = await webHost.StartAsync();
            var mainWindow = new MainWindow(port, webHost);
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"The app failed to start.\n\n{ex.Message}",
                "Expense Tracker",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Dispose();
        base.OnExit(e);
    }

    public void Dispose()
    {
        if (_singleInstanceMutex is not null)
        {
            try
            {
                _singleInstanceMutex.ReleaseMutex();
            }
            catch (ApplicationException)
            {
            }
            _singleInstanceMutex.Dispose();
            _singleInstanceMutex = null;
        }
        GC.SuppressFinalize(this);
    }
}