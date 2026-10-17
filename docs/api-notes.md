# Crestron XIO Cloud API Notes

Source: `.github/Crestron XIO Cloud API.postman_collection.json` and `.github/websocket documentation.md`.

## Authentication

- Header `XiO-subscription-key: <API key>` on every request (never log it).
- Base URL: `https://api.crestron.io/api`

## Endpoints used

| Purpose | Method | Path |
|---|---|---|
| Account details | GET | `/v1/account/accountid/{accountId}/account` |
| Account devices | GET | `/v1/device/accountid/{accountId}/devices` |
| Device status (v2) | GET | `/v2/device/accountid/{accountId}/devicecid/{deviceCid}/status` |
| Notification info (WebSocket URL) | GET | `/v1/notifications/accountid/{accountId}/notifications-information` |

Notifications endpoint rate limit: 1 request per 5 minutes.

## Response shapes

- Account details: `{ "id": "...", "Name": "..." }`
- Devices: array of objects with `device-cid`, `device-name`, `device-model`, `device-category`, `device-status`, `device-groupid`, ...
- Device status v2: object of groups, each a flat object of fields (values may be `null`):
  `device`, `audio`, `connections`, `hdmi-input`, `hdmi-output`, ... Group names drive the data-group menu.
- Notification info: `{ AccountId, IsNotificationsEnabled, ErrorMessage, Url, ExpirationInMilliseconds }`.
  `Url` is a `wss://` Azure Web PubSub URL containing an access token (treat as secret); it expires after `ExpirationInMilliseconds`.

## WebSocket message

JSON array of `{ Cid, DeviceName, AccountId, GroupId, DeviceModel, Timestamp }`. Only identifies the changed device; re-fetch status via Device Status (v2) and diff against the cache.
