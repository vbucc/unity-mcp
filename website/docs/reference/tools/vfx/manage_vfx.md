---
title: manage_vfx
sidebar_label: manage_vfx
description: "Manage Unity VFX: ParticleSystem, VisualEffect (VFX Graph), LineRenderer, TrailRenderer."
---

# `manage_vfx`

> **Auto-generated** from the Python tool registry. Do not hand-edit outside `<!-- examples:start --><!-- examples:end -->` blocks — the generator (`tools/generate_docs_reference.py`) will overwrite them.

**Group:** `vfx` &nbsp;·&nbsp; **Module:** `services.tools.manage_vfx`

## Description

Manage Unity VFX: ParticleSystem, VisualEffect (VFX Graph), LineRenderer, TrailRenderer.

PARTICLE: particle_create, particle_get_info, particle_set_main, particle_set_emission, particle_set_shape, particle_set_color_over_lifetime, particle_set_size_over_lifetime, particle_set_velocity_over_lifetime, particle_set_noise, particle_set_renderer, particle_enable_module, particle_play/stop/pause/restart/clear, particle_add_burst, particle_clear_bursts
VFX GRAPH: vfx_create_asset, vfx_assign_asset, vfx_list_templates, vfx_list_assets, vfx_get_info, vfx_set_float/int/bool/vector2/vector3/vector4/color/gradient/texture/mesh/curve, vfx_send_event, vfx_play/stop/pause/reinit, vfx_set_playback_speed, vfx_set_seed
LINE: line_get_info, line_set_positions, line_add_position, line_set_position, line_set_width/color/material/properties, line_clear, line_create_line/circle/arc/bezier
TRAIL: trail_get_info, trail_set_time/width/color/material/properties, trail_clear, trail_emit

Action-specific parameters go in `properties` (keys match ManageVFX.cs).

## Parameters

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `action` | `Literal['ping', 'particle_create', 'particle_get_info', 'particle_set_main', 'particle_set_emission', 'particle_set_shape', 'particle_set_color_over_lifetime', 'particle_set_size_over_lifetime', 'particle_set_velocity_over_lifetime', 'particle_set_noise', 'particle_set_renderer', 'particle_enable_module', 'particle_play', 'particle_stop', 'particle_pause', 'particle_restart', 'particle_clear', 'particle_add_burst', 'particle_clear_bursts', 'vfx_create_asset', 'vfx_assign_asset', 'vfx_list_templates', 'vfx_list_assets', 'vfx_get_info', 'vfx_set_float', 'vfx_set_int', 'vfx_set_bool', 'vfx_set_vector2', 'vfx_set_vector3', 'vfx_set_vector4', 'vfx_set_color', 'vfx_set_gradient', 'vfx_set_texture', 'vfx_set_mesh', 'vfx_set_curve', 'vfx_send_event', 'vfx_play', 'vfx_stop', 'vfx_pause', 'vfx_reinit', 'vfx_set_playback_speed', 'vfx_set_seed', 'line_get_info', 'line_set_positions', 'line_add_position', 'line_set_position', 'line_set_width', 'line_set_color', 'line_set_material', 'line_set_properties', 'line_clear', 'line_create_line', 'line_create_circle', 'line_create_arc', 'line_create_bezier', 'trail_get_info', 'trail_set_time', 'trail_set_width', 'trail_set_color', 'trail_set_material', 'trail_set_properties', 'trail_clear', 'trail_emit']` | yes | Action to perform (prefix: particle_, vfx_, line_, trail_). |
| `target` | `str \| None` | — | Target GameObject (name/path/id). |
| `search_method` | `Literal['by_id', 'by_name', 'by_path', 'by_tag', 'by_layer'] \| None` | — | How to find the target GameObject. |
| `properties` | `dict[str, Any] \| str \| None` | — | Action-specific parameters (dict or JSON string). |
| `component_index` | `int \| None` | — | Zero-based index to select which component when multiple of the same type exist (e.g., multiple ParticleSystems). If omitted, targets the first instance. |

## Returns

A `dict` containing the Unity response. The exact shape depends on the action.

## Examples

<!-- examples:start -->
*No examples yet. Add usage examples here — they will be preserved across regenerations.*
<!-- examples:end -->

