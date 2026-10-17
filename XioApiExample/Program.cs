using XioApiExample.Services;

var parsed = ArgumentParser.Parse(args);
if (!parsed.Success)
{
    Console.Error.WriteLine(parsed.Error);
    Console.Error.WriteLine(ArgumentParser.Usage);
    return 1;
}

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

using var http = new HttpClient();
var client = new XioApiClient(http, parsed.Options!);

try
{
    var accountName = await client.GetAccountNameAsync(cts.Token);
    Console.WriteLine($"Account: {accountName}");

    var devices = await client.GetDevicesAsync(cts.Token);
    Console.WriteLine($"Devices found: {devices.Count}");

    var store = new DeviceStatusStore();
    var menu = new ConsoleMenu(client, store);
    var result = await menu.RunAsync(devices, cts.Token);
    if (result == MenuResult.StartMonitoring)
    {
        var listener = new WebSocketListener(client);
        await listener.RunAsync(
            (n, token) =>
            {
                Console.WriteLine($"[{DateTime.Now:T}] Change notification: {n.DeviceName ?? n.Cid} ({n.Cid}) at {n.Timestamp}");
                return Task.CompletedTask;
            },
            message => Console.WriteLine(message),
            cts.Token);
    }
}
catch (XioApiException ex)
{
    Console.Error.WriteLine(ex.Message);
    return 2;
}
catch (OperationCanceledException)
{
    return 130;
}

return 0;
