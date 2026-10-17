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

Console.WriteLine($"Account ID: {parsed.Options!.AccountId}");
await Task.CompletedTask;
return 0;
