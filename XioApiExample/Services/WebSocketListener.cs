using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using XioApiExample.Models;

namespace XioApiExample.Services;

/// <summary>
/// Listens for device change notifications. The notification URL contains an access token
/// and is never printed or logged.
/// </summary>
public sealed class WebSocketListener(XioApiClient client)
{
    // The notification-information endpoint is limited to 1 request per 5 minutes.
    private static readonly TimeSpan MinInfoInterval = TimeSpan.FromMinutes(5);

    public static IReadOnlyList<DeviceChangeNotification> ParseMessage(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.ValueKind switch
            {
                JsonValueKind.Array => doc.RootElement.Deserialize<List<DeviceChangeNotification>>() ?? [],
                JsonValueKind.Object => doc.RootElement.Deserialize<DeviceChangeNotification>() is { } n ? [n] : [],
                _ => []
            };
        }
        catch (JsonException)
        {
            return [];
        }
    }

    /// <summary>Connects and reconnects until cancelled, invoking onNotification for each change.</summary>
    public async Task RunAsync(
        Func<DeviceChangeNotification, CancellationToken, Task> onNotification,
        Action<string> onStatus,
        CancellationToken ct)
    {
        var lastRequest = DateTimeOffset.MinValue;

        while (!ct.IsCancellationRequested)
        {
            var wait = lastRequest + MinInfoInterval - DateTimeOffset.UtcNow;
            if (wait > TimeSpan.Zero)
            {
                onStatus($"Reconnecting in {Math.Ceiling(wait.TotalSeconds)} s (API rate limit).");
                await Task.Delay(wait, ct);
            }

            try
            {
                lastRequest = DateTimeOffset.UtcNow;
                var info = await client.GetNotificationInfoAsync(ct);
                if (!info.IsNotificationsEnabled || string.IsNullOrWhiteSpace(info.Url))
                {
                    onStatus($"Notifications are not available: {info.ErrorMessage ?? "not enabled for this account"}");
                    return;
                }

                using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(ct);
                if (info.ExpirationInMilliseconds > 0)
                {
                    lifetime.CancelAfter(TimeSpan.FromMilliseconds(info.ExpirationInMilliseconds));
                }

                onStatus("Connecting to notification service...");
                await ListenAsync(new Uri(info.Url), onNotification, onStatus, lifetime.Token);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return;
            }
            catch (OperationCanceledException)
            {
                onStatus("Notification session expired.");
            }
            catch (XioApiException ex)
            {
                onStatus(ex.Message);
            }
            catch (WebSocketException ex)
            {
                onStatus($"WebSocket error: {ex.Message}");
            }
        }
    }

    private static async Task ListenAsync(
        Uri uri,
        Func<DeviceChangeNotification, CancellationToken, Task> onNotification,
        Action<string> onStatus,
        CancellationToken ct)
    {
        using var socket = new ClientWebSocket();
        await socket.ConnectAsync(uri, ct);
        onStatus("Connected. Listening for device changes (Ctrl+C to stop).");

        var buffer = new byte[8192];
        using var message = new MemoryStream();

        try
        {
            while (socket.State == WebSocketState.Open && !ct.IsCancellationRequested)
            {
                var result = await socket.ReceiveAsync(buffer, ct);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    onStatus("Server closed the connection.");
                    return;
                }

                message.Write(buffer, 0, result.Count);
                if (!result.EndOfMessage)
                {
                    continue;
                }

                var text = Encoding.UTF8.GetString(message.GetBuffer(), 0, (int)message.Length);
                message.SetLength(0);

                foreach (var notification in ParseMessage(text))
                {
                    await onNotification(notification, ct);
                }
            }
        }
        finally
        {
            if (socket.State == WebSocketState.Open)
            {
                using var closeCts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                try
                {
                    await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "bye", closeCts.Token);
                }
                catch (Exception ex) when (ex is WebSocketException or OperationCanceledException)
                {
                }
            }
        }
    }
}
