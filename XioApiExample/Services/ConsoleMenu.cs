using XioApiExample.Models;

namespace XioApiExample.Services;

public sealed class ConsoleMenu(
    XioApiClient client,
    DeviceStatusStore store,
    Action startWatching,
    Func<bool> isWatching,
    TextWriter? output = null,
    TextReader? input = null)
{
    private readonly TextWriter _out = output ?? Console.Out;
    private readonly TextReader _in = input ?? Console.In;

    public async Task RunAsync(IReadOnlyList<DeviceInfo> devices, CancellationToken ct)    {
        if (devices.Count == 0)
        {
            _out.WriteLine("No devices found in this account.");
            return;
        }

        while (!ct.IsCancellationRequested)
        {
            PrintDevices(devices);
            var watchHint = isWatching() ? "watching for changes" : "'w' = watch for changes";
            _out.Write($"Select devices (e.g. 1,3), 'a' = all, {watchHint}, 'q' = quit: ");
            var line = _in.ReadLine()?.Trim();

            if (line is null || line.Equals("q", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            if (line.Equals("w", StringComparison.OrdinalIgnoreCase))
            {
                if (isWatching())
                {
                    _out.WriteLine("Already watching for changes.");
                }
                else
                {
                    startWatching();
                }
                continue;
            }

            var selected = ParseSelection(line, devices);
            if (selected.Count == 0)
            {
                _out.WriteLine("Invalid selection.");
                continue;
            }

            var statuses = await LoadStatusesAsync(selected, ct);
            if (statuses.Count > 0)
            {
                await ShowGroupsAsync(selected, statuses, ct);
            }
        }
    }

    private void PrintDevices(IReadOnlyList<DeviceInfo> devices)
    {
        _out.WriteLine();
        _out.WriteLine("Devices:");
        for (var i = 0; i < devices.Count; i++)
        {
            var d = devices[i];
            var cached = store.Contains(d.Cid) ? "*" : " ";
            _out.WriteLine($" {cached}{i + 1,3}. {d.DisplayName} [{d.Model}] {d.Status}");
        }
        _out.WriteLine(" (* = status cached)");
    }

    internal static List<DeviceInfo> ParseSelection(string line, IReadOnlyList<DeviceInfo> devices)
    {
        if (line.Equals("a", StringComparison.OrdinalIgnoreCase))
        {
            return [.. devices];
        }

        var result = new List<DeviceInfo>();
        foreach (var part in line.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries))
        {
            if (!int.TryParse(part, out var n) || n < 1 || n > devices.Count)
            {
                return [];
            }
            var device = devices[n - 1];
            if (!result.Contains(device))
            {
                result.Add(device);
            }
        }
        return result;
    }

    private async Task<List<DeviceStatus>> LoadStatusesAsync(List<DeviceInfo> selected, CancellationToken ct)
    {
        var loaded = new List<DeviceStatus>();
        foreach (var device in selected)
        {
            try
            {
                if (store.TryGet(device.Cid, out var cached) && cached is not null)
                {
                    loaded.Add(cached);
                    continue;
                }

                var status = await client.GetDeviceStatusAsync(device.Cid, ct);
                store.Set(status);
                loaded.Add(status);
            }
            catch (XioApiException ex)
            {
                _out.WriteLine($"{device.DisplayName}: {ex.Message}");
            }
        }
        return loaded;
    }

    private Task ShowGroupsAsync(List<DeviceInfo> selected, List<DeviceStatus> statuses, CancellationToken ct)
    {
        var groups = statuses
            .SelectMany(s => s.Groups.Keys)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        while (!ct.IsCancellationRequested)
        {
            _out.WriteLine();
            _out.WriteLine("Data groups:");
            for (var i = 0; i < groups.Count; i++)
            {
                _out.WriteLine($"  {i + 1,2}. {groups[i]}");
            }
            _out.Write("Select a group, 'b' = back to devices: ");
            var line = _in.ReadLine()?.Trim();

            if (line is null || line.Equals("b", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }
            if (!int.TryParse(line, out var n) || n < 1 || n > groups.Count)
            {
                _out.WriteLine("Invalid selection.");
                continue;
            }

            PrintGroup(statuses, groups[n - 1]);
        }

        return Task.CompletedTask;
    }

    private void PrintGroup(IEnumerable<DeviceStatus> statuses, string group)
    {
        foreach (var status in statuses)
        {
            _out.WriteLine();
            _out.WriteLine($"== {status.Name} / {group} ==");
            if (!status.Groups.TryGetValue(group, out var fields))
            {
                _out.WriteLine("  (not reported by this device)");
                continue;
            }
            foreach (var (key, value) in fields)
            {
                _out.WriteLine($"  {key}: {value ?? "(none)"}");
            }
        }
    }
}
