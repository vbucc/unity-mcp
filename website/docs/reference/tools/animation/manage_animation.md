---
title: manage_animation
sidebar_label: manage_animation
description: "Manage Unity animation: Animator control, AnimatorController editing, and AnimationClip creation."
---

# `manage_animation`

> **Auto-generated** from the Python tool registry. Do not hand-edit outside `<!-- examples:start --><!-- examples:end -->` blocks — the generator (`tools/generate_docs_reference.py`) will overwrite them.

**Group:** `animation` &nbsp;·&nbsp; **Module:** `services.tools.manage_animation`

## Description

Manage Unity animation: Animator control, AnimatorController editing, and AnimationClip creation.

ANIMATOR (runtime): animator_get_info, animator_get_parameter, animator_play, animator_crossfade, animator_set_parameter, animator_set_speed, animator_set_enabled
CONTROLLER (asset): controller_create, controller_get_info, controller_assign, controller_add_state, controller_remove_state, controller_modify_state, controller_set_state_motion, controller_add_transition, controller_remove_transition, controller_modify_transition, controller_add_parameter, controller_modify_parameter, controller_remove_parameter, controller_add_sub_state_machine, controller_remove_sub_state_machine, controller_modify_sub_state_machine, controller_add_entry_transition, controller_remove_entry_transition, controller_add_layer, controller_remove_layer, controller_rename_layer, controller_set_layer_weight, controller_create_blend_tree_1d, controller_create_blend_tree_2d, controller_add_blend_tree_child, controller_add_blend_tree_child_tree
CLIP (asset): clip_create, clip_get_info, clip_add_curve, clip_set_curve, clip_set_vector_curve, clip_create_preset, clip_assign, clip_add_event, clip_remove_event

Top-level params: `controller_path` (controller_*), `clip_path` + `clip_name` (clip_*), `target` + `search_method` (animator_*). Action-specific params go in `properties` (snake_case and camelCase both accepted).

STATE PROPERTIES (controller_add_state / controller_modify_state):
  stateName (required, supports path 'Sub/Inner'), layerIndex (default 0),
  clipPath, clipName (FBX sub-clip), speed, tag, isDefault,
  writeDefaultValues, iKOnFeet, mirror, cycleOffset,
  speedParameter, cycleOffsetParameter, mirrorParameter, timeParameter,
  newName (modify_state only — preserves AnimatorState fileID; transitions survive).
TRANSITION PROPERTIES (controller_add_transition / controller_modify_transition):
  fromState, toState (required; toState may be a state, sub-state machine, or 'Exit'
    for the Exit pseudo-state — case-insensitive 'Exit'/'_Exit'/'<Exit>' all accepted),
  layerIndex, hasExitTime, exitTime, duration, offset, hasFixedDuration,
  interruptionSource (none|source|destination|sourceThenDestination|destinationThenSource),
  orderedInterruption, canTransitionToSelf,
  conditions ([{parameter, mode, threshold}]) — replaced wholesale on modify.
  modify_transition also accepts transitionIndex (default 0) to disambiguate.
  Note: AnyState→Exit is not supported by Unity; state-machine-level exit transitions
    (from a sub-state machine itself, not from a state inside it) are not yet supported —
    use exit transitions on a specific state inside the sub-SM instead.
PARAMETER PROPERTIES (controller_add_parameter / controller_modify_parameter / controller_remove_parameter):
  parameterName (required), parameterType (float|int|bool|trigger), defaultValue,
  newName (modify_parameter only — also rewrites all condition references),
  force (remove_parameter only — strip dangling refs; default false errors if any exist).
LAYER PROPERTIES (controller_add_layer / controller_set_layer_weight / controller_rename_layer):
  layerName or layerIndex, weight, blendingMode (override|additive),
  newName (rename_layer only).
SUB-STATE MACHINE PROPERTIES (controller_add_/modify_/remove_sub_state_machine):
  name (required, path), parentPath, newName (modify), defaultState (modify), position [x,y,z] (modify).
BLEND TREE PROPERTIES (controller_create_blend_tree_1d/2d, controller_add_blend_tree_child[_tree]):
  stateName, blendType, blendParameter, blendParameterX, blendParameterY,
  childTreeName, childBlendType, childBlendParameter[X|Y].
ENTRY TRANSITIONS (controller_add_entry_transition / controller_remove_entry_transition):
  stateMachinePath (default root), toState (required), conditions.

CONVENTIONS:
  Paths: '/' is canonical ('Idle/Idle_Standing'). '.' is also accepted on lookups
    (translated to '/' on miss). For state CREATION, always use '/'.
  AnyState as fromState: 'AnyState', 'Any', or 'Any State' (case-insensitive).
  Condition modes by parameter type:
    Bool/Trigger → 'if', 'ifNot' (also: true/false)
    Int          → 'equals', 'notEqual', 'greater', 'less'
    Float        → 'greater', 'less'
    Mismatched mode/type errors at validation time.
  Identity preservation: prefer controller_modify_state / controller_modify_parameter /
    controller_rename_layer for renames. Remove+add destroys the AnimatorState sub-asset
    (new fileID) and breaks external references like Timeline tracks and animation events.

## Parameters

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `action` | `Literal['animator_get_info', 'animator_get_parameter', 'animator_play', 'animator_crossfade', 'animator_set_parameter', 'animator_set_speed', 'animator_set_enabled', 'controller_create', 'controller_add_state', 'controller_add_transition', 'controller_add_parameter', 'controller_modify_parameter', 'controller_get_info', 'controller_assign', 'controller_set_state_motion', 'controller_remove_state', 'controller_remove_transition', 'controller_remove_parameter', 'controller_modify_state', 'controller_modify_transition', 'controller_add_sub_state_machine', 'controller_remove_sub_state_machine', 'controller_modify_sub_state_machine', 'controller_add_entry_transition', 'controller_remove_entry_transition', 'controller_add_layer', 'controller_remove_layer', 'controller_rename_layer', 'controller_set_layer_weight', 'controller_create_blend_tree_1d', 'controller_create_blend_tree_2d', 'controller_add_blend_tree_child', 'controller_add_blend_tree_child_tree', 'clip_create', 'clip_get_info', 'clip_add_curve', 'clip_set_curve', 'clip_set_vector_curve', 'clip_create_preset', 'clip_assign', 'clip_add_event', 'clip_remove_event']` | yes | Action to perform (prefix: animator_, controller_, clip_). |
| `target` | `str \| None` | — | Target GameObject (name/path/id). |
| `search_method` | `Literal['by_id', 'by_name', 'by_path', 'by_tag', 'by_layer'] \| None` | — | How to find the target GameObject. |
| `clip_path` | `str \| None` | — | Asset path for AnimationClip (e.g. 'Assets/Animations/Walk.anim'). |
| `clip_name` | `str \| None` | — | Name of a specific clip within a multi-clip asset (e.g. FBX). Required when clip_path points to an FBX with multiple animations. |
| `controller_path` | `str \| None` | — | Asset path for AnimatorController (e.g. 'Assets/Animators/Player.controller'). |
| `properties` | `dict[str, Any] \| str \| None` | — | Action-specific parameters (dict or JSON string). |

## Returns

A `dict` containing the Unity response. The exact shape depends on the action.

## Examples

<!-- examples:start -->
*No examples yet. Add usage examples here — they will be preserved across regenerations.*
<!-- examples:end -->

