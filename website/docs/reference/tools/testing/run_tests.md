---
title: run_tests
sidebar_label: run_tests
description: "Starts a Unity test run asynchronously and returns a job_id immediately."
---

# `run_tests`

> **Auto-generated** from the Python tool registry. Do not hand-edit outside `<!-- examples:start --><!-- examples:end -->` blocks — the generator (`tools/generate_docs_reference.py`) will overwrite them.

**Group:** `testing` &nbsp;·&nbsp; **Module:** `services.tools.run_tests`

## Description

Starts a Unity test run asynchronously and returns a job_id immediately. Poll with get_test_job for progress. Pass clear_stuck=True to recover from a stuck 'tests_running' state without starting a new run. Fails fast with error 'unsaved_untitled_scene' when a dirty untitled scene is open (a run would block the editor on a native Save dialog); remediate by saving it via manage_scene(action='save', name=..., path=...) or re-run with discard_untitled_scenes=True to discard it.

## Parameters

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `mode` | `Literal['EditMode', 'PlayMode']` | — | Unity test mode to run |
| `test_names` | `list[str] \| str \| None` | — | Full names of specific tests to run |
| `group_names` | `list[str] \| str \| None` | — | Same as test_names, except it allows for Regex |
| `category_names` | `list[str] \| str \| None` | — | NUnit category names to filter by |
| `assembly_names` | `list[str] \| str \| None` | — | Assembly names to filter tests by |
| `include_failed_tests` | `bool` | — | Include details for failed/skipped tests only (default: false) |
| `include_details` | `bool` | — | Include details for all tests (default: false) |
| `clear_stuck` | `bool` | — | Force-clear an orphaned/stuck test job and return immediately without starting a new run. Use to recover from a stuck 'tests_running' state. When set, other filter params are ignored. |
| `discard_untitled_scenes` | `bool` | — | Discard any dirty untitled (never-saved) scenes before running. Default false: run_tests fails fast with error 'unsaved_untitled_scene' instead of risking a blocking Save dialog. Set true only when the untitled scene's contents are disposable. |
| `init_timeout` | `int \| None` | — | Initialization timeout in milliseconds. PlayMode tests may need longer due to domain reload (default: 15000). Recommended: 120000 for PlayMode. |

## Returns

A `dict` containing the Unity response. The exact shape depends on the action.

## Examples

<!-- examples:start -->
*No examples yet. Add usage examples here — they will be preserved across regenerations.*
<!-- examples:end -->

