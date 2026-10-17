using XioApiExample.Models;

namespace XioApiExample.Services;

/// <summary>Handles change notifications: re-fetches status, diffs against the cache, prints changes.</summary>
public sealed class ChangeReporter(XioApiClient client, DeviceStatusStore store, TextWriter? output = null)
{
    private readonly TextWriter _out = output ?? Console.Out;

    public async Task HandleAsync(DeviceChangeNotification notification, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(notification.Cid))
        {
            return;
        }

        var label = notification.DeviceName ?? notification.Cid;
        try
        {
            var current = await client.GetDeviceStatusAsync(notification.Cid, ct);
            var hadCache = store.TryGet(notification.Cid, out var previous);
            store.Set(current);

            if (!hadCache)
            {
                _out.WriteLine($"[{DateTime.Now:T}] {current.Name}: status cached (no previous data to compare).");
                return;
            }

            var changes = StatusDiffer.Diff(previous, current);
            if (changes.Count == 0)
            {
                _out.WriteLine($"[{DateTime.Now:T}] {current.Name}: notification received, no value changes.");
                return;
            }

            _out.WriteLine($"[{DateTime.Now:T}] {current.Name}:");
            foreach (var c in changes)
            {
                _out.WriteLine($"  {c.Group}.{c.Field}: {c.OldValue ?? "(none)"} -> {c.NewValue ?? "(none)"}");
            }
        }
        catch (XioApiException ex)
        {
            _out.WriteLine($"[{DateTime.Now:T}] {label}: could not refresh status: {ex.Message}");
        }
    }
}
