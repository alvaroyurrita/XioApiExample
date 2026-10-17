namespace XioApiExample.Models;

/// <summary>
/// Status of one device: data group name (for example "audio") to field name to value.
/// Values are flattened to strings so they can be compared and printed uniformly.
/// </summary>
public sealed record DeviceStatus(
    string Cid,
    string Name,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, string?>> Groups);
