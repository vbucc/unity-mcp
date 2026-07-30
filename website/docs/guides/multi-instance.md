---
id: multi-instance
slug: /guides/multi-instance
title: Multi-Instance Routing
sidebar_label: Multi-Instance Routing
description: Drive several Unity Editors from a single MCP session with set_active_instance and per-call routing.
---

# Multi-Instance Routing

You can have several Unity Editors open at once and aim a single MCP session at any of them.

## When this comes up

- You're refactoring a shared package and need to test the same change in two projects
- You're comparing behavior between Unity LTS and Unity 6
- You have a runtime project + a tooling project both connected
- You're driving a CI fixture project alongside your day-to-day work

## How instances are identified

Each connected Unity Editor advertises a stable ID of the form `Name@hash`, where:

- `Name` is the project's `productName` from Player Settings
- `hash` is a stable 8-character hash derived from the project path

Example: `MyGame@a1b2c3d4`.

You can also reference an instance by:

- **Hash prefix** (e.g. `a1b` if it's unambiguous)

## Discovering instances

Read the resource:

> `mcpforunity://instances`

It returns the list of currently connected Editors with their `Name@hash`, Unity version, and connection time. Most MCP clients expose this as the `unity_instances` resource.

## Setting the active instance for the session

```
set_active_instance(instance="MyGame@a1b2c3d4")
```

Once set, **every subsequent tool call** in the session routes to that instance until you change it. This is the most common pattern: choose once, then prompt normally.

You can also use:

```
set_active_instance(instance="a1b")         # hash prefix
```

## Routing a single call without changing the session default

Pass `unity_instance` on the individual tool call:

```
manage_scene(action="get_hierarchy", unity_instance="MyGame@a1b2c3d4")
```

This is useful for comparing two projects in the same prompt — e.g., "Read the same script from both projects and tell me what differs."

The server accepts the same value formats as `set_active_instance`: `Name@hash` or a hash prefix.

## What happens with no active instance

- **One Unity Editor connected** → it's used automatically.
- **Multiple Editors connected and no active set** → the server errors with the available instance list. Call `set_active_instance` and retry.

## How session isolation works

Instance state is keyed per MCP session (`Mcp-Session-Id`), so two MCP clients can target different Editors at the same time on the same Python server.

## Related reference

- [`set_active_instance`](/reference/tools/core/set_active_instance) — full tool reference
- [`unity_instances` resource](/reference/resources) — discovery surface
