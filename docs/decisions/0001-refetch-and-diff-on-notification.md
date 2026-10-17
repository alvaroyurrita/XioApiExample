# ADR 0001: Refetch and diff on WebSocket notifications

Status: Accepted

## Context
The XIO WebSocket payload only identifies the changed device (Cid, name, timestamp), not the new values.

## Decision
On each notification, call Get Device Status (v2), diff against the in-memory cache, update the cache and print only changed fields with the device name.

## Consequences
One extra API call per notification. Output stays concise. The cache must be seeded; a device with no cached status is cached without printing a diff.
