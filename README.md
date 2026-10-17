# Crestron XIO API Example

Console app that connects to a Crestron XIO account using an API key and account ID, shows the account name and devices, lets you browse device status by data group, then listens over WebSocket for real-time changes and prints only what changed.

## Build

Self-contained single-file executable (no .NET install needed to run):

```
dotnet publish XioApiExample/XioApiExample.csproj -c Release -o publish
```

Run tests:

```
dotnet test
```

## Usage

```
publish\XioApiExample.exe --api-key <KEY> --account-id <ACCOUNT_ID>
```

Both arguments are required (`--name value` or `--name=value`). The API key is never printed.

Exit codes: `0` normal, `1` bad arguments, `2` API error, `130` cancelled.

## Walkthrough

1. The app prints the account name and fetches the device list.
2. **Device menu:** enter numbers (`1,3`), `a` for all, `w` to watch for changes, or `q` to quit. Devices marked `*` already have cached status.
3. **Group menu:** after selecting devices, status is fetched and cached. Pick a data group (`device`, `audio`, `connections`, ...) to print its fields, or `b` to go back.
When a device changes, the app re-fetches its status, compares it with the cache and prints only the changed fields:

```
[14:02:11] cp4-home:
  audio.volume: 10 -> 20
```

   Press `q` or Ctrl+C to stop.

## More

- [Architecture](docs/architecture.md)
- [API notes](docs/api-notes.md)
- [Decisions](docs/decisions/)
