using System.Diagnostics;
using System.Windows;
using Microsoft.Web.WebView2.Core;

namespace TransportExpenditureTracker.Desktop;

public partial class MainWindow : Window
{
    private readonly int _port;
    private readonly WebHostRunner _webHost;
    private readonly Uri _rootUri;

    public MainWindow(int port, WebHostRunner webHost)
    {
        InitializeComponent();
        _port = port;
        _webHost = webHost;
        _rootUri = new Uri($"http://127.0.0.1:{_port}/", UriKind.Absolute);
        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await Browser.EnsureCoreWebView2Async();
            var settings = Browser.CoreWebView2.Settings;
            settings.AreDevToolsEnabled = false;
            settings.AreDefaultContextMenusEnabled = false;
            settings.IsZoomControlEnabled = false;
            settings.IsStatusBarEnabled = false;
            Browser.CoreWebView2.NavigationStarting += OnNavigationStarting;
            Browser.CoreWebView2.Navigate(_rootUri.AbsoluteUri);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                $"The app could not start. Microsoft Edge WebView2 Runtime may not be installed.\n\n{ex.Message}",
                "Expense Tracker",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Close();
        }
    }

    private void OnNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
    {
        if (e.Uri.StartsWith(_rootUri.AbsoluteUri, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        e.Cancel = true;
        if (Uri.TryCreate(e.Uri, UriKind.Absolute, out var external) &&
            (external.Scheme == Uri.UriSchemeHttp || external.Scheme == Uri.UriSchemeHttps))
        {
            Process.Start(new ProcessStartInfo(external.AbsoluteUri) { UseShellExecute = true });
        }
    }

    private async void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        try
        {
            await _webHost.StopAsync();
        }
        catch
        {
            // Best-effort shutdown; the process exits regardless.
        }
    }
}