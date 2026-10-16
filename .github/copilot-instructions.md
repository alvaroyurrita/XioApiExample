# Crestron XIO API Example

## Goal of this program

Create a console application that connects to a Crestron XIO account using an API key and an account ID, retrieves the account name and then a list of devices. Then it should create a user menu for the user to select which devices to get information from, and from there what group of data to get (such as device, audio etc).  This is done to avoid the console screen to scroll through to many pages.  It should store the status of the devices in memory.

After displaying the status the program should elevate the connection to a WebSocket connection and listen for real-time updates on device status changes. When a device status changes, the program should print the updated status to the console.

The updates will come in the form of a JSON payload indicating the device that changed.  The new status will then need to be read using the API and the updated status of only the things that changed should be printed to the console, indicating the device name and the new status.

The console command should require the user to enter the API key and account ID as command line arguments.

Create a `README.md` file that explains how to use the program

## References

Crestron XIO API Documentation can be found in a postman collenction [here](https://www.postman.com/mine77-5548/crestron-xio-cloud-api/collection/i2lohgb/crestron-xio-cloud-api?utm_source=postman-web-app&sideView=agentMode).

Device information should be retrieve using the `Get Device Status (v2)` endpoint.

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

When changing behavior, update README/architecture docs and add an ADR for significant decisions." Then ask at the end of each feature: "Update the docs for what we just changed.
