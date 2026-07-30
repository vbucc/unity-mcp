---
title: manage_tools
sidebar_label: manage_tools
description: "Manage which tool groups are visible in this session."
---

# `manage_tools`

> **Auto-generated** from the Python tool registry. Do not hand-edit outside `<!-- examples:start --><!-- examples:end -->` blocks — the generator (`tools/generate_docs_reference.py`) will overwrite them.

**Group:** `core` &nbsp;·&nbsp; **Module:** `services.tools.manage_tools`

## Description

Manage which tool groups are visible in this session. Additional capabilities are in disabled groups — activate them when needed.
Groups: docs (API reflection & docs lookup), scripting_ext (execute_code + ScriptableObjects), vfx (particles, VFX Graph, shaders), animation (Animator, AnimatorController, AnimationClips), ui (UI Toolkit UXML/USS), testing (test runner), probuilder (3D modeling), auditor (static analysis), profiling (Profiler & Frame Debugger).
Actions: list_groups, activate, deactivate, sync, reset. Activating a group makes its tools appear; deactivating hides them.

## Parameters

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `action` | `Literal['list_groups', 'activate', 'deactivate', 'sync', 'reset']` | yes | Action to perform. |
| `group` | `str \| None` | — | Group name (required for activate / deactivate). Valid groups: animation, asset_gen, auditor, core, docs, probuilder, profiling, scripting_ext, testing, ui, vfx |

## Returns

A `dict` containing the Unity response. The exact shape depends on the action.

## Examples

<!-- examples:start -->
*No examples yet. Add usage examples here — they will be preserved across regenerations.*
<!-- examples:end -->

