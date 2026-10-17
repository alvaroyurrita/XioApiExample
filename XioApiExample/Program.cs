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
    Console.WriteLine($"Devices ({devices.Count}):");
    foreach (var device in devices)
    {
        Console.WriteLine($"  {device.DisplayName} [{device.Model}] {device.Status}");
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
