# ADR 0002: Self-contained single-file executable

Status: Accepted

## Context
The program should run by itself without invoking `dotnet`.

## Decision
The project sets `RuntimeIdentifier` win-x64, `SelfContained` and `PublishSingleFile` (with compression and invariant globalization). `dotnet publish` produces one `.exe` (about 36 MB).

## Consequences
No runtime install needed. Larger binary. Other platforms need `-r <rid>` at publish time.
