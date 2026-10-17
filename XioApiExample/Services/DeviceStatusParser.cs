using System.Text.Json;
using XioApiExample.Models;

namespace XioApiExample.Services;

public static class DeviceStatusParser
{
    private const string DeviceGroup = "device";

    public static DeviceStatus Parse(string cid, JsonElement root)
    {
        var groups = new Dictionary<string, IReadOnlyDictionary<string, string?>>(StringComparer.Ordinal);

        if (root.ValueKind == JsonValueKind.Object)
        {
            foreach (var group in root.EnumerateObject())
            {
                if (group.Value.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                var fields = new Dictionary<string, string?>(StringComparer.Ordinal);
                foreach (var field in group.Value.EnumerateObject())
                {
                    fields[field.Name] = ToText(field.Value);
                }
                groups[group.Name] = fields;
            }
        }

        return new DeviceStatus(cid, ResolveName(cid, groups), groups);
    }

    private static string ResolveName(string cid, Dictionary<string, IReadOnlyDictionary<string, string?>> groups)
    {
        if (groups.TryGetValue(DeviceGroup, out var device))
        {
            foreach (var key in new[] { "user-device-name", "device-name" })
            {
                if (device.TryGetValue(key, out var name) && !string.IsNullOrWhiteSpace(name))
                {
                    return name;
                }
            }
        }
        return cid;
    }

    private static string? ToText(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.Null or JsonValueKind.Undefined => null,
        JsonValueKind.String => value.GetString(),
        _ => value.GetRawText()
    };
}
