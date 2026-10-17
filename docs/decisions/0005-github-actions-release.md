# ADR 0005: Tag-triggered GitHub Actions release

Status: Accepted

## Context
The repository is hosted on GitHub. Releases should only be produced from tested code and carry a version number.

## Decision
`.github/workflows/release.yml` runs when a tag like `v1.2.3` is pushed.
1. `test`: verifies the tagged commit is on `main`, then runs `dotnet test`.
2. `build`: only if tests pass, publishes the self-contained single-file executable for win-x64, linux-x64 and osx-arm64 with `-p:Version` taken from the tag, and packages each (zip for Windows, tar.gz otherwise).
3. `release`: creates a GitHub Release named after the tag and attaches all packages.

## Consequences
Releasing is `git tag v1.2.3` then `git push origin v1.2.3`. Tests run on Windows; builds are cross-published from Linux. Binaries are not signed or notarized, so macOS may require clearing the quarantine flag.
