using XioApiExample.Models;

namespace XioApiExample.Services;

public sealed record StatusChange(string Group, string Field, string? OldValue, string? NewValue);

public static class StatusDiffer
{
    /// <summary>Returns the field values that differ between the previous and current status.</summary>
    public static IReadOnlyList<StatusChange> Diff(DeviceStatus? previous, DeviceStatus current)
    {
        var changes = new List<StatusChange>();

        foreach (var (group, fields) in current.Groups)
        {
            IReadOnlyDictionary<string, string?>? oldFields = null;
            previous?.Groups.TryGetValue(group, out oldFields);

            foreach (var (field, newValue) in fields)
            {
                string? oldValue = null;
                var existed = oldFields?.TryGetValue(field, out oldValue) ?? false;
                if (!existed ? newValue is not null : !string.Equals(oldValue, newValue, StringComparison.Ordinal))
                {
                    changes.Add(new StatusChange(group, field, oldValue, newValue));
                }
            }
        }

        if (previous is not null)
        {
            foreach (var (group, oldFields) in previous.Groups)
            {
                current.Groups.TryGetValue(group, out var newFields);
                foreach (var (field, oldValue) in oldFields)
                {
                    if (oldValue is not null && (newFields is null || !newFields.ContainsKey(field)))
                    {
                        changes.Add(new StatusChange(group, field, oldValue, null));
                    }
                }
            }
        }

        return changes;
    }
}
