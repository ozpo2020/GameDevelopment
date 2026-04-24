---
name: lightweight-superpowers
description: Lightweight default workflow for normal Codex work in this Godot 4 .NET repository.
---
# Lightweight Superpowers

It is only allowed to use when Superpowers has been installed on this device.Use this skill for ordinary repo work when the task does not need a heavyweight design or execution process.

## Goals

- Keep momentum high for normal changes
- Preserve the useful parts of superpowers
- Avoid forcing large plans for small gameplay edits

## Default Flow

1. Read the nearest relevant README, script, and scene before editing.
2. Decide the size:
   - Small: one area, a few files, clear request
   - Medium: crosses multiple modules or scenes
   - Large: architecture, ambiguous feature, or handoff-heavy
3. For small work, keep a short plan in chat only.
4. For medium/large work, write a focused plan to `docs/plans/YYYY-MM-DD-<topic>.md`.
5. Edit the minimum number of files needed.
6. Verify before completion and state any remaining manual checks.

## When To Pull In Heavier Skills

- Use `brainstorming` for net-new mechanics, feature design, or unclear requirements.
- Use `systematic-debugging` when reproducing a bug or regression.
- Use `writing-plans` only when the work meaningfully crosses systems.
- Use `subagent-driven-development` only when tasks can be cleanly split by file ownership.

## Small-Task Rules

- Do not create a design doc for a simple script tweak.
- Do not introduce cleanup refactors unless they directly support the requested change.
- Match the surrounding style and architecture.
- Keep summaries short and concrete.

## Verification Minimum

Before saying the task is done, explicitly cover:

- What files changed
- What you verified
- What still needs manual Godot/editor checking, if anything

If a real build command is unavailable in the repo state, say that plainly instead of guessing.
