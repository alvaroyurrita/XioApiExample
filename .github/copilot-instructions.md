# Crestron XIO API Example

## Goal of this program

Create a console application that connects to a Crestron XIO account using an API key and an account ID, retrieves the account name and then a list of devices. Then it should create a user menu for the user to select which devices to get information from, and from there what group of data to get (such as device, audio etc).  This is done to avoid the console screen to scroll through to many pages.  It should store the status of the devices in memory.

After displaying the status the program should elevate the connection to a WebSocket connection and listen for real-time updates on device status changes. When a device status changes, the program should print the updated status to the console.

The updates will come in the form of a JSON payload indicating the device that changed.  The new status will then need to be read using the API and the updated status of only the things that changed should be printed to the console, indicating the device name and the new status.

The console command should require the user to enter the API key and account ID as command line arguments.

Create a `README.md` file that explains how to use the program

## References

Device information should be retrieved using the `Get Device Status (v2)` endpoint.

The API is read from a JSON export of the relevant Postman collection, stored at `.github/Crestron XIO Cloud API.postman_collection.json` (the Postman page itself is not machine-readable). 

The websocket connection is described in `.github/websocket documentation.md`

Distilled findings are in `docs/api-notes.md`. 


## Architecture and conventions

- Target .NET 10; use only BCL (`HttpClient`, `ClientWebSocket`, `System.Text.Json`) unless asked before adding packages.
- Keep `Program.cs` thin; split into services: argument parsing, XIO API client, device status store, console menu, WebSocket listener, status differ.
- Use async/await with `CancellationToken`; enable nullable reference types.
- Never log or print the API key.
- The WebSocket payload only identifies the changed device. Re-fetch status with `Get Device Status (v2)`, diff against the in-memory cache, and print only changed values with the device name.
- Docs: `README.md`, `docs/architecture.md`, `docs/api-notes.md`, and ADRs in `docs/decisions/`.
- Build with `dotnet build`; test with `dotnet test`. Add tests for the argument parser and differ.

## General agent Instructions

1. Plan first (it should list files and steps, no edits).
2. Review and adjust the plan.
3. Have it implement one step at a time, then build and test.
4. Commit after each working step so you can roll back easily.

When changing behavior, update README/architecture docs and add an ADR for significant decisions. At the end of each feature, ask: "Update the docs for what we just changed."

## Implementation Plan

Status legend: [ ] pending, [~] in progress, [x] done. Update this list as each step completes.

Assumptions:
- Single project `XioApiExample` targeting .NET 10, BCL only.
- Published as a self-contained single-file executable (win-x64) that runs without `dotnet`.
- Endpoints, auth header and WebSocket details are taken from the Postman JSON export (see `docs/api-notes.md`).
- The WebSocket payload only identifies the changed device; status is re-fetched and diffed against the cache.

Steps:
1. [x] Revise `.github/copilot-instructions.md` with conventions, structure and this plan
2. [x] Review the Postman collection export found under `.github/Crestron XIO Cloud API.postman_collection.json` 
3. [x] Create `README.md` skeleton and `docs/architecture.md`
4. [x] Add command-line argument parsing and validation
5. [x] Add the XIO API client for account name and device list
6. [x] Add the in-memory device status store
7. [x] Add the Get Device Status (v2) call and models
8. [x] Add the console menu for device and data-group selection
9. [x] Add the WebSocket listener for update notifications
10. [x] Add the status differ and changed-only output
11. [x] Add tests for the argument parser and differ
12. [ ] Update README and add ADRs for key decisions



