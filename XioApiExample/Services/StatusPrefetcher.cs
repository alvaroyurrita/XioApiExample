using XioApiExample.Models;

namespace XioApiExample.Services;

public sealed class StatusPrefetcher(XioApiClient client, DeviceStatusStore store)
{
    /// <summary>Fetches Get Device Status (v2) for every device and caches the results.</summary>
    public async Task<int> LoadAllAsync(
        IReadOnlyList<DeviceInfo> devices,
        Action<string> onProgress,
        CancellationToken ct)
    {
        var loaded = 0;
        for (var i = 0; i < devices.Count; i++)
        {
            var device = devices[i];
            onProgress($"Loading status {i + 1}/{devices.Count}: {device.DisplayName}...");
            try
            {
                store.Set(await client.GetDeviceStatusAsync(device.Cid, ct));
                loaded++;
            }
            catch (XioApiException ex)
            {
                onProgress($"  Could not load {device.DisplayName}: {ex.Message}");
            }
        }
        return loaded;
    }
}
