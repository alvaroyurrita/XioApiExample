using System.Collections.Concurrent;
using XioApiExample.Models;

namespace XioApiExample.Services;

public sealed class DeviceStatusStore
{
    private readonly ConcurrentDictionary<string, DeviceStatus> _statuses =
        new(StringComparer.OrdinalIgnoreCase);

    public int Count => _statuses.Count;

    public bool Contains(string cid) => _statuses.ContainsKey(cid);

    public bool TryGet(string cid, out DeviceStatus? status)
    {
        var found = _statuses.TryGetValue(cid, out var value);
        status = value;
        return found;
    }

    /// <summary>Stores the status and returns the previous one, or null if none was cached.</summary>
    public DeviceStatus? Set(DeviceStatus status)
    {
        DeviceStatus? previous = null;
        _statuses.AddOrUpdate(
            status.Cid,
            status,
            (_, existing) =>
            {
                previous = existing;
                return status;
            });
        return previous;
    }

    public bool Remove(string cid) => _statuses.TryRemove(cid, out _);

    public IReadOnlyCollection<string> Cids => _statuses.Keys.ToArray();
}
