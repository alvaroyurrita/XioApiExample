using System.Net;
using System.Net.Http.Json;
using XioApiExample.Models;

namespace XioApiExample.Services;

public sealed class XioApiException(string message, HttpStatusCode? statusCode = null, Exception? inner = null)
    : Exception(message, inner)
{
    public HttpStatusCode? StatusCode { get; } = statusCode;
}

public sealed class XioApiClient
{
    public const string BaseUrl = "https://api.crestron.io/api/";
    private const string KeyHeader = "XiO-subscription-key";

    private readonly HttpClient _http;
    private readonly string _accountId;

    public XioApiClient(HttpClient http, AppOptions options)
    {
        _http = http;
        _accountId = Uri.EscapeDataString(options.AccountId);
        _http.BaseAddress ??= new Uri(BaseUrl);
        _http.DefaultRequestHeaders.Remove(KeyHeader);
        _http.DefaultRequestHeaders.Add(KeyHeader, options.ApiKey);
    }

    public async Task<string> GetAccountNameAsync(CancellationToken ct)
    {
        var account = await GetAsync<AccountInfo>($"v1/account/accountid/{_accountId}/account", ct);
        return account?.Name ?? "(unknown)";
    }

    public async Task<IReadOnlyList<DeviceInfo>> GetDevicesAsync(CancellationToken ct)
    {
        var devices = await GetAsync<List<DeviceInfo>>($"v1/device/accountid/{_accountId}/devices", ct);
        return devices ?? [];
    }

    public async Task<DeviceStatus> GetDeviceStatusAsync(string cid, CancellationToken ct)
    {
        var root = await GetAsync<System.Text.Json.JsonElement?>(
            $"v2/device/accountid/{_accountId}/devicecid/{Uri.EscapeDataString(cid)}/status", ct);
        return DeviceStatusParser.Parse(cid, root ?? default);
    }

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
