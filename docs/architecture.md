# Architecture

Single console project `XioApiExample` (.NET 10, BCL only). `Program.cs` stays thin and wires services.

| Service | Responsibility |
|---|---|
| ArgumentParser | Parse/validate `--api-key` and `--account-id` |
| XioApiClient | `HttpClient` wrapper: account name, devices, status v2, notification info |
| DeviceStatusStore | In-memory cache of device status by CID |
| ConsoleMenu | Select devices, then data group (device, audio, ...) |
| WebSocketListener | `ClientWebSocket` listener for change notifications |
| StatusDiffer | Compare cached vs. new status; return changed values only |

## Flow

1. Parse args -> fetch account name and devices.
2. Menu: choose devices, then data group; fetch and cache status.
3. Connect WebSocket; on notification, re-fetch status for the Cid, diff, print changes with device name.

All async code takes a `CancellationToken`. The API key is never printed.
