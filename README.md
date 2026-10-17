# Crestron XIO API Example

Console app that connects to a Crestron XIO account, lists devices, shows device status by data group, then listens over WebSocket for real-time changes.

## Usage

Build a self-contained single-file executable (no .NET install needed to run):

```
dotnet publish XioApiExample/XioApiExample.csproj -c Release -o publish
```

Run it:

```
publish\XioApiExample.exe --api-key <KEY> --account-id <ACCOUNT_ID>
```

Both arguments are required (`--name value` or `--name=value`). The API key is never printed.

(Details to be completed as features are implemented.)

See `docs/architecture.md` and `docs/api-notes.md`.
