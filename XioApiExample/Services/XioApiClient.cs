using System.Net;
using System.Net.Http.Json;
using XioApiExample.Models;

namespace XioApiExample.Services;

public sealed class XioApiException(string message, HttpStatusCode? statusCode = null, Exception? inner = null)
    : Exception(message, inner)
{
    public HttpStatusCode? StatusCode { get; } = statusCode;
}

/// <summary>
/// Wraps the Crestron XiO API for account metadata, device discovery, status polling,
/// and notification configuration.
/// </summary>
public sealed class XioApiClient
{
    public const string BaseUrl = "https://api.crestron.io/api/";
    private const string KeyHeader = "XiO-subscription-key";

    private readonly HttpClient _http;
    private readonly string _accountId;

    /// <summary>
    /// Creates a client bound to a specific account and injects the subscription key for all requests.
    /// </summary>
    /// <param name="http">The shared HttpClient used for API calls.</param>
    /// <param name="options">The application credentials and account identifier.</param>
    public XioApiClient(HttpClient http, AppOptions options)
    {
        _http = http;
        _accountId = Uri.EscapeDataString(options.AccountId);
        _http.BaseAddress ??= new Uri(BaseUrl);
        // Replace rather than append so a reused HttpClient always uses the supplied credential.
        _http.DefaultRequestHeaders.Remove(KeyHeader);
        _http.DefaultRequestHeaders.Add(KeyHeader, options.ApiKey);
    }

    /// <summary>
    /// Fetches the account display name from the XiO account metadata endpoint.
    /// </summary>
    public async Task<string> GetAccountNameAsync(CancellationToken ct)
    {
        var account = await GetAsync<AccountInfo>($"v1/account/accountid/{_accountId}/account", ct);
        return account?.Name ?? "(unknown)";
    }

    /// <summary>
    /// Lists every device associated with the configured account.
    /// </summary>
    public async Task<IReadOnlyList<DeviceInfo>> GetDevicesAsync(CancellationToken ct)
    {
        var devices = await GetAsync<List<DeviceInfo>>($"v1/device/accountid/{_accountId}/devices", ct);
        return devices ?? [];
    }

    /// <summary>
    /// Requests the notification configuration for this account, including the WebSocket URL.
    /// </summary>
    public async Task<NotificationInfo> GetNotificationInfoAsync(CancellationToken ct)
    {
        var info = await GetAsync<NotificationInfo>(
            $"v1/notifications/accountid/{_accountId}/notifications-information", ct);
        return info ?? new NotificationInfo(false, "Empty response.", null, 0);
    }

    /// <summary>
    /// Gets the current status payload for a single device using the v2 status endpoint.
    /// </summary>
    /// <param name="cid">The XiO device identifier to query.</param>
    public async Task<DeviceStatus> GetDeviceStatusAsync(string cid, CancellationToken ct)
    {
        // The v2 endpoint returns the grouped status payload used by the menu and differ.
        var root = await GetAsync<System.Text.Json.JsonElement?>(
            $"v2/device/accountid/{_accountId}/devicecid/{Uri.EscapeDataString(cid)}/status", ct);
        return DeviceStatusParser.Parse(cid, root ?? default);
    }

    /// <summary>
    /// Executes a XiO API GET request and converts deserialization failures into app-specific exceptions.
    /// </summary>
    private async Task<T?> GetAsync<T>(string path, CancellationToken ct)
    {
        try
        {
            using var response = await _http.GetAsync(path, ct);
            if (!response.IsSuccessStatusCode)
            {
                var hint = response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden
                    ? " Check the API key and account ID."
                    : string.Empty;
                throw new XioApiException(
                    $"XIO API request failed: {(int)response.StatusCode} {response.ReasonPhrase}.{hint}",
                    response.StatusCode);
            }
            return await response.Content.ReadFromJsonAsync<T>(ct);
        }
        catch (HttpRequestException ex)
        {
            throw new XioApiException($"Unable to reach the XIO API: {ex.Message}", ex.StatusCode, ex);
        }
        catch (System.Text.Json.JsonException ex)
        {
            throw new XioApiException("Unexpected response format from the XIO API.", null, ex);
        }
    }
}
