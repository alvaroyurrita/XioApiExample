# Architecture

Single console project `XioApiExample` (.NET 10, BCL only), published as a self-contained single-file executable (see ADR 0002). `Program.cs` stays thin and wires services. Tests live in `XioApiExample.Tests` (NUnit).

| Service | Responsibility |
|---|---|
| ArgumentParser | Parse/validate `--api-key` and `--account-id` |
| XioApiClient | `HttpClient` wrapper: account name, devices, status v2, notification info |
| DeviceStatusParser | Convert the status v2 JSON into a `DeviceStatus` (group -> field -> string value) |
| DeviceStatusStore | Thread-safe in-memory cache of device status by CID |
| StatusPrefetcher | At startup, fetch status v2 for every device and fill the cache |
| ConsoleMenu | Select devices, then data group (device, audio, ...); starts the watcher on request |
| WebSocketListener | `ClientWebSocket` listener for change notifications; reconnects with a fresh URL |
| ChangeReporter | On notification: re-fetch status, diff against the cache, update cache, print changes |
| StatusDiffer | Compare cached vs. new status; return changed values only |

## Flow

1. Parse args, then fetch the account name and the device list.
2. `StatusPrefetcher` calls Get Device Status (v2) for each device and caches the results. A failure for one device is reported and skipped.
3. `ConsoleMenu` runs: choose devices, then a data group; status is read from the cache (fetched only if missing).
4. Pressing `w` starts `WebSocketListener` on a background task and returns to the menu, so the user can keep browsing cached status while watching.
5. On each notification (which only identifies the device), `ChangeReporter` re-fetches status, diffs it against the cache with `StatusDiffer`, updates the cache and prints only the changed values with the device name.
6. Quitting the menu (or Ctrl+C) cancels the watcher and exits.

## Concurrency

The menu loop runs on the main task; the listener and `ChangeReporter` run on a background task. They share `DeviceStatusStore`, which is thread-safe. Both write to the console, so change lines can appear between menu prompts.

## Conventions

- All async code takes a `CancellationToken`.
- The API key and the WebSocket URL (which carries an access token) are never printed or logged.
- Notification-info requests are limited to 1 per 5 minutes; the listener waits at least that long between reconnects (ADR 0004).

## Tests

`XioApiExample.Tests` (NUnit) covers `ArgumentParser` and `StatusDiffer`.

## Decisions

See `docs/decisions/` for ADRs.
