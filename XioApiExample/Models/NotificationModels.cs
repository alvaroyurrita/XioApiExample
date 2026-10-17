using System.Text.Json.Serialization;

namespace XioApiExample.Models;

public sealed record NotificationInfo(
    [property: JsonPropertyName("IsNotificationsEnabled")] bool IsNotificationsEnabled,
    [property: JsonPropertyName("ErrorMessage")] string? ErrorMessage,
    [property: JsonPropertyName("Url")] string? Url,
    [property: JsonPropertyName("ExpirationInMilliseconds")] long ExpirationInMilliseconds);

public sealed record DeviceChangeNotification(
    [property: JsonPropertyName("Cid")] string? Cid,
    [property: JsonPropertyName("DeviceName")] string? DeviceName,
    [property: JsonPropertyName("AccountId")] string? AccountId,
    [property: JsonPropertyName("GroupId")] string? GroupId,
    [property: JsonPropertyName("DeviceModel")] string? DeviceModel,
    [property: JsonPropertyName("Timestamp")] string? Timestamp);
