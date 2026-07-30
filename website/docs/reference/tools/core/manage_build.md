---
title: manage_build
sidebar_label: manage_build
description: "Manage Unity player builds — trigger builds, switch platforms, configure settings, manage build scenes and profiles, run batch builds across platforms. settings accepts any PlayerSettings property (e.g. bakeCollisionMeshes, allowUnsafeCo…"
---

# `manage_build`

> **Auto-generated** from the Python tool registry. Do not hand-edit outside `<!-- examples:start --><!-- examples:end -->` blocks — the generator (`tools/generate_docs_reference.py`) will overwrite them.

**Group:** `core` &nbsp;·&nbsp; **Module:** `services.tools.manage_build`

## Description

Manage Unity player builds — trigger builds, switch platforms, configure settings, manage build scenes and profiles, run batch builds across platforms. settings accepts any PlayerSettings property (e.g. bakeCollisionMeshes, allowUnsafeCode), not just the well-known shortcuts. Use list_settings to discover all available properties. Actions: build, status, platform, settings, list_settings, scenes, profiles, batch, cancel.

## Parameters

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `action` | `Literal['build', 'status', 'platform', 'settings', 'list_settings', 'scenes', 'profiles', 'batch', 'cancel']` | yes | Action: build, status, platform, settings, list_settings, scenes, profiles, batch, cancel |
| `target` | `str \| None` | — | Build target: windows64, osx, linux64, android, ios, webgl, uwp, tvos, visionos |
| `output_path` | `str \| None` | — | Output path for the build |
| `scenes` | `str \| None` | — | JSON array of scene paths, or comma-separated paths |
| `development` | `str \| None` | — | Development build (true/false) |
| `options` | `str \| None` | — | JSON array of BuildOptions: clean_build, auto_run, deep_profiling, compress_lz4, strict_mode, detailed_report |
| `subtarget` | `str \| None` | — | Build subtarget: player or server |
| `scripting_backend` | `str \| None` | — | Scripting backend: mono or il2cpp (persistent change) |
| `profile` | `str \| None` | — | Build Profile asset path (Unity 6+ only) |
| `property` | `str \| None` | — | Settings property name. Well-known shortcuts: product_name, company_name, version, bundle_id, scripting_backend, defines, architecture. Any PlayerSettings property also works (e.g. bakeCollisionMeshes, allowUnsafeCode). Use action='list_settings' to discover all. |
| `value` | `str \| None` | — | Value to set for the property (omit to read) |
| `activate` | `str \| None` | — | Activate a build profile (true/false) |
| `targets` | `str \| None` | — | JSON array of targets for batch build |
| `profiles` | `str \| None` | — | JSON array of profile paths for batch build (Unity 6+) |
| `output_dir` | `str \| None` | — | Base output directory for batch builds |
| `job_id` | `str \| None` | — | Job ID for status/cancel |

## Returns

A `dict` containing the Unity response. The exact shape depends on the action.

## Examples

<!-- examples:start -->
*No examples yet. Add usage examples here — they will be preserved across regenerations.*
<!-- examples:end -->

