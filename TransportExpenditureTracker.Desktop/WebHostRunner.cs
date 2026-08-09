using Serilog;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace TransportExpenditureTracker.Desktop;

public class WebHostRunner
{
    private WebApplication? _app;

    public async Task<int> StartAsync()
    {
        var port = await StartWithRetryAsync();
        return port;
    }

    private async Task<int> StartWithRetryAsync()
    {
        const int maxAttempts = 10;
        Exception? lastError = null;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var port = GetRandomPort();
            var dbFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TransportExpenditureTracker");

            Directory.CreateDirectory(dbFolder);

            var dbPath = Path.Combine(dbFolder, "ExpenseTracker.db");
            var connectionString = $"Data Source={dbPath}";

            var app = WebAppBuilder.Build([], connectionString, localOnly: true);
            app.Urls.Clear();
            app.Urls.Add($"http://127.0.0.1:{port}");

            try
            {
                await app.StartAsync();
                _app = app;
                return port;
            }
            catch (IOException ex)
            {
                lastError = ex;
                await app.DisposeAsync();
            }
        }

        throw new InvalidOperationException(
            $"Failed to bind to a local port after {maxAttempts} attempts.",
            lastError);
    }

    public async Task StopAsync()
    {
        if (_app is null) return;
        try
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
        finally
        {
            await Log.CloseAndFlushAsync();
            _app = null;
        }
    }

    private static int GetRandomPort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}