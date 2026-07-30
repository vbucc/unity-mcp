---
title: manage_project_settings
sidebar_label: manage_project_settings
description: "Read, write, and discover Unity project settings across categories: quality (QualitySettings), physics, physics2d, time, editor (EditorSettings)."
---

# `manage_project_settings`

> **Auto-generated** from the Python tool registry. Do not hand-edit outside `<!-- examples:start --><!-- examples:end -->` blocks — the generator (`tools/generate_docs_reference.py`) will overwrite them.

**Group:** `core` &nbsp;·&nbsp; **Module:** `services.tools.manage_project_settings`

## Description

Read, write, and discover Unity project settings across categories: quality (QualitySettings), physics, physics2d, time, editor (EditorSettings). Supports any static property via reflection, including snake_case names. For PlayerSettings, use manage_build(action='settings') instead. Actions: get, set, list, list_categories.

## Parameters

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `action` | `Literal['get', 'set', 'list', 'list_categories']` | yes | Action: get, set, list, list_categories |
| `category` | `str \| None` | — | Settings category: quality, physics, physics2d, time, editor |
| `property` | `str \| None` | — | Property name (snake_case or camelCase, e.g. shadow_distance or shadowDistance) |
| `value` | `str \| None` | — | Value to set. Scalars as strings, vectors as JSON arrays (e.g. '[0, -9.81, 0]') |

## Returns

A `dict` containing the Unity response. The exact shape depends on the action.

## Examples

<!-- examples:start -->
*No examples yet. Add usage examples here — they will be preserved across regenerations.*
<!-- examples:end -->

