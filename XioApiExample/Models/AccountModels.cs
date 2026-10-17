using System.Text.Json.Serialization;

namespace XioApiExample.Models;

public sealed record AccountInfo(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("Name")] string? Name);

public sealed record DeviceInfo(
    [property: JsonPropertyName("device-cid")] string Cid,
    [property: JsonPropertyName("device-name")] string? Name,
    [property: JsonPropertyName("user-device-name")] string? UserName,
    [property: JsonPropertyName("device-model")] string? Model,
    [property: JsonPropertyName("device-category")] string? Category,
    [property: JsonPropertyName("device-groupid")] string? GroupId,
    [property: JsonPropertyName("device-status")] string? Status)
{
    public string DisplayName =>
        !string.IsNullOrWhiteSpace(UserName) ? UserName :
        !string.IsNullOrWhiteSpace(Name) ? Name : Cid;
}
