---
id: transports
slug: /architecture/transports
title: Transport
sidebar_label: Transport
description: How the MCP client, the Python server, and the Unity Editor connect — and how multi-agent isolation works.
---

# Transport

MCP for Unity uses **HTTP** between the MCP client and the Python server, and a **WebSocket** between the Python server and the Unity Editor. There is one transport; the only choice is whether the server runs locally or is remote-hosted.

```text
MCP client  ──HTTP /mcp──▶  Python server  ◀──WebSocket /hub/plugin──  Unity Editor
```

Note the direction of the second arrow: **Unity dials out to the server**, not the other way round. The Editor opens the WebSocket and registers itself with `PluginHub`. That is why the server has nothing to connect to at startup, and why an Editor can come and go across domain reloads without the server noticing anything but a reconnect.

## Local vs remote-hosted

| | HTTP Local | HTTP Remote |
|---|---|---|
| Where the server runs | your machine | another machine or container |
| Default endpoint | `http://localhost:8080/mcp` | your URL |
| Auth | none | API key (`--http-remote-hosted`) |
| Scheme | `http://` loopback | `https://` required (opt-in for plaintext) |

Switch between them in the Unity Editor: **Window → MCP for Unity**, pick `HTTP Local` or `HTTP Remote`, then **Configure All Detected Clients**. The configurator rewrites each client's MCP config to match.

## MCP client config

```json
{
  "mcpServers": {
    "unityMCP": {
      "type": "http",
      "url": "http://localhost:8080/mcp"
    }
  }
}
```

Every supported client uses this shape. Unity writes it for you — the JSON above is only for hand-editing or for a client MCP for Unity doesn't detect.

## Multi-agent isolation

One server hosts many clients at once. Per-session state — the active Unity instance, tool-group visibility, middleware state — is held in FastMCP's session-scoped store, keyed by `ctx.session_id` (the `Mcp-Session-Id` header). Two MCP sessions can never share a selection, so Claude Code and Cursor can be open simultaneously against different Editors.

When no active instance is set, the server auto-selects: a sole connected Editor is used automatically, and with several connected it matches the client's `list_roots()` working directory against each Unity project path. Only if that is still ambiguous does it ask you to call `set_active_instance`.

See [Multi-Instance Routing](/guides/multi-instance) for the routing API.

## Ports

The local server defaults to **8080**. Two things make that port worth respecting:

- Unity's "Start Server" button stops whatever process currently owns the configured port before binding it. Pointing a second Editor at a port someone else is using will take down that server.
- The endpoint lives in `EditorPrefs`, which Unity shares across **every project** for a given Editor version — so changing it in one project changes it everywhere.

For anything ephemeral (the local test harness, CI), set the `UNITY_MCP_HTTP_URL` environment variable on the Editor process instead. `HttpEndpointUtility.GetLocalBaseUrl()` reads it ahead of the pref, so a throwaway Editor can use a throwaway port without disturbing your everyday setup. `tools/local_harness.py` does exactly this, and refuses outright to bind 8080.

## Network security

By default the server binds loopback (`127.0.0.1` / `::1`). Binding all interfaces (`0.0.0.0` / `::`) requires explicit opt-in: **Advanced Settings → Allow LAN Bind (HTTP Local)**.

Remote endpoints require `https://`. To allow plaintext `http://` for a remote URL, opt in via **Allow Insecure Remote HTTP**. Both guards are fail-closed: if you don't flip the switch, the server refuses the unsafe configuration.

## Upgrading from stdio

Older versions also shipped a stdio transport, where the MCP client spawned a dedicated Python process that reached Unity over a legacy TCP bridge on port 6400. That transport and its bridge have been removed.

You do not need to do anything by hand:

- `--transport` is still accepted on the command line (only `http` is valid), so an existing config that passes `--transport http` keeps working.
- On the first Editor launch after the package updates, `LegacyClientConfigMigration` rewrites any client config still carrying a stdio `command`/`args` entry.
- If a stale stdio config survives that, the Clients tab reports the client as **not configured** rather than healthy — click **Configure** to rewrite it.

## Where this is implemented

- Python: `Server/src/transport/` — `plugin_hub.py` (the WebSocket hub), `unity_transport.py` (the single send path), `unity_instance_middleware.py` (session-scoped routing)
- C#: `MCPForUnity/Editor/Services/Transport/` — `WebSocketTransportClient`, `TransportManager`; plus `Services/Server/` for local server lifecycle
- v8 migration notes: [/migrations/v8](/migrations/v8) — the architectural story of HTTP arriving
