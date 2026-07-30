---
title: manage_project_auditor
sidebar_label: manage_project_auditor
description: "Unity Project Auditor: static analysis of code, assets, shaders, settings."
---

# `manage_project_auditor`

> **Auto-generated** from the Python tool registry. Do not hand-edit outside `<!-- examples:start --><!-- examples:end -->` blocks — the generator (`tools/generate_docs_reference.py`) will overwrite them.

**Group:** `auditor` &nbsp;·&nbsp; **Module:** `services.tools.manage_project_auditor`

## Description

Unity Project Auditor: static analysis of code, assets, shaders, settings. Requires Unity 6.4+. Enable with manage_tools(action='activate', group='auditor').

WORKFLOW: status (check availability) → audit or load_report → get_summary → list_issues (filtered + paged) → get_issue_detail. Use add_rule with severity='None' to suppress noisy issues by descriptor ID.

AUDIT: audit (run analysis, filtered by categories/assemblies/platform), load_report (from autosave or custom path)

QUERY: get_summary (counts by category & severity), list_issues (filter by category/severity/area/path/search, paged), get_issue_detail (descriptor info + occurrence locations), list_categories, list_areas

RULES: list_rules, add_rule (suppress or change severity), remove_rule

STATUS: status (availability, report loaded, counts)

## Parameters

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `action` | `Literal['audit', 'load_report', 'get_summary', 'list_issues', 'get_issue_detail', 'list_categories', 'list_areas', 'list_rules', 'add_rule', 'remove_rule', 'status']` | yes | The project auditor action to perform. |
| `categories` | `str \| None` | — | Comma-separated IssueCategory names for audit (e.g. 'Code,Shader'). Omit for all. |
| `assemblies` | `str \| None` | — | Comma-separated assembly names to scope audit. |
| `platform` | `str \| None` | — | BuildTarget name for analysis (e.g. 'StandaloneWindows64'). Defaults to active. |
| `category` | `str \| None` | — | Single IssueCategory name to filter issues. |
| `severity` | `str \| None` | — | Minimum severity: Critical, Major, Moderate, Minor, Warning, Info. |
| `area` | `str \| None` | — | Area flag to filter: CPU, GPU, Memory, BuildSize, BuildTime, LoadTime, Quality. |
| `path_filter` | `str \| None` | — | File path substring to filter issues. |
| `search` | `str \| None` | — | Search term to match against issue description. |
| `page_size` | `int \| None` | — | Results per page (default 50, max 200). |
| `cursor` | `str \| None` | — | Pagination cursor (offset). |
| `descriptor_id` | `str \| None` | — | Descriptor ID (e.g. 'PAC2000') for get_issue_detail, add_rule, remove_rule. |
| `rule_severity` | `str \| None` | — | Severity for rule: None (suppress), Info, Minor, Moderate, Major, Critical. |
| `rule_filter` | `str \| None` | — | Optional location scope for rule (e.g. 'Assets/ThirdParty/'). |
| `report_path` | `str \| None` | — | Path to .projectauditor report file. Defaults to autosave. |

## Returns

A `dict` containing the Unity response. The exact shape depends on the action.

## Examples

<!-- examples:start -->
*No examples yet. Add usage examples here — they will be preserved across regenerations.*
<!-- examples:end -->

