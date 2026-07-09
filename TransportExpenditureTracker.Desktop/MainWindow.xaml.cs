using System.Windows;

namespace TransportExpenditureTracker.Desktop;

public partial class MainWindow : Window
{
    private readonly int _port;
    private readonly WebHostRunner _webHost;

    public MainWindow(int port, WebHostRunner webHost)
    {
        InitializeComponent();
        _port = port;
        _webHost = webHost;
        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await Browser.EnsureCoreWebView2Async();
        Browser.CoreWebView2.Settings.AreDevToolsEnabled = false;
        Browser.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
        Browser.CoreWebView2.Navigate($"http://127.0.0.1:{_port}");
    }

    private async void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        await _webHost.StopAsync();
    }
}