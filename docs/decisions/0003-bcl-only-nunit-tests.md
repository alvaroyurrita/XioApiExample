# ADR 0003: BCL only, NUnit for tests

Status: Accepted

## Context
The app should avoid third-party dependencies; tests need a framework.

## Decision
The app uses only `HttpClient`, `ClientWebSocket` and `System.Text.Json`. Tests live in `XioApiExample.Tests` using NUnit (chosen by the project owner).

## Consequences
Hand-written argument parsing and menu. NUnit is a test-only dependency.
