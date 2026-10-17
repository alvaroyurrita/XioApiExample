# ADR 0004: WebSocket URL handling and reconnect

Status: Accepted

## Context
The notification-information endpoint returns a `wss://` URL with an access token that expires, and is rate limited to 1 request per 5 minutes.

## Decision
The URL is never printed or logged. The listener reconnects with a fresh URL after expiry or disconnect, waiting at least 5 minutes between URL requests.

## Consequences
A dropped connection can take up to 5 minutes to recover; changes during the gap are not reported.
