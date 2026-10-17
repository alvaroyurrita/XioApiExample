using XioApiExample.Models;
using XioApiExample.Services;

namespace XioApiExample.Tests;

public class StatusDifferTests
{
    private static DeviceStatus Status(params (string Group, (string Field, string? Value)[] Fields)[] groups)
    {
        var dict = groups.ToDictionary(
            g => g.Group,
            g => (IReadOnlyDictionary<string, string?>)g.Fields.ToDictionary(f => f.Field, f => f.Value));
        return new DeviceStatus("cid1", "dev1", dict);
    }

    [Test]
    public void Diff_IdenticalStatus_ReturnsNoChanges()
    {
        var a = Status(("audio", [("volume", "10"), ("mute-status", null)]));
        var b = Status(("audio", [("volume", "10"), ("mute-status", null)]));

        Assert.That(StatusDiffer.Diff(a, b), Is.Empty);
    }

    [Test]
    public void Diff_ChangedValue_ReturnsOnlyThatField()
    {
        var a = Status(("audio", [("volume", "10"), ("mute-status", "false")]));
        var b = Status(("audio", [("volume", "20"), ("mute-status", "false")]));

        var changes = StatusDiffer.Diff(a, b);

        Assert.That(changes, Has.Count.EqualTo(1));
        Assert.That(changes[0], Is.EqualTo(new StatusChange("audio", "volume", "10", "20")));
    }

    [Test]
    public void Diff_NullToValue_IsChange()
    {
        var a = Status(("audio", [("volume", null)]));
        var b = Status(("audio", [("volume", "5")]));

        var changes = StatusDiffer.Diff(a, b);

        Assert.That(changes, Has.Count.EqualTo(1));
        Assert.That(changes[0].OldValue, Is.Null);
        Assert.That(changes[0].NewValue, Is.EqualTo("5"));
    }

    [Test]
    public void Diff_ValueToNull_IsChange()
    {
        var a = Status(("audio", [("volume", "5")]));
        var b = Status(("audio", [("volume", null)]));

        var changes = StatusDiffer.Diff(a, b);

        Assert.That(changes, Has.Count.EqualTo(1));
        Assert.That(changes[0].NewValue, Is.Null);
    }

    [Test]
    public void Diff_NewField_IsReported()
    {
        var a = Status(("audio", [("volume", "5")]));
        var b = Status(("audio", [("volume", "5"), ("mute-status", "true")]));

        var changes = StatusDiffer.Diff(a, b);

        Assert.That(changes, Has.Count.EqualTo(1));
        Assert.That(changes[0].Field, Is.EqualTo("mute-status"));
    }

    [Test]
    public void Diff_RemovedField_IsReported()
    {
        var a = Status(("audio", [("volume", "5"), ("mute-status", "true")]));
        var b = Status(("audio", [("volume", "5")]));

        var changes = StatusDiffer.Diff(a, b);

        Assert.That(changes, Has.Count.EqualTo(1));
        Assert.That(changes[0], Is.EqualTo(new StatusChange("audio", "mute-status", "true", null)));
    }

    [Test]
    public void Diff_NoPrevious_ReportsNonNullValuesOnly()
    {
        var b = Status(("audio", [("volume", "5"), ("mute-status", null)]));

        var changes = StatusDiffer.Diff(null, b);

        Assert.That(changes, Has.Count.EqualTo(1));
        Assert.That(changes[0].Field, Is.EqualTo("volume"));
    }

    [Test]
    public void Diff_NewGroup_IsReported()
    {
        var a = Status(("audio", [("volume", "5")]));
        var b = Status(("audio", [("volume", "5")]), ("connections", [("hdmi-in", "true")]));

        var changes = StatusDiffer.Diff(a, b);

        Assert.That(changes, Has.Count.EqualTo(1));
        Assert.That(changes[0].Group, Is.EqualTo("connections"));
    }
}
