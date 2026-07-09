using System.IO;
using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Builder;
using Serilog;
using TransportExpenditureTracker;

namespace TransportExpenditureTracker.Desktop;

public class WebHostRunner
{
    private WebApplication? _app;

    public async Task<int> StartAsync()
    {
        var port = GetRandomPort();

        var dbFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TransportExpenditureTracker");

        Directory.CreateDirectory(dbFolder);

        var dbPath = Path.Combine(dbFolder, "ExpenseTracker.db");
        var connectionString = $"Data Source={dbPath}";

        _app = WebAppBuilder.Build([], connectionString);

        _app.Urls.Clear();
        _app.Urls.Add($"http://127.0.0.1:{port}");

        await _app.StartAsync();
        return port;
    }

    public async Task StopAsync()
    {
        if (_app is null) return;
        await _app.StopAsync();
        await _app.DisposeAsync();
        Log.CloseAndFlush();
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