---
name: godot-change-checklist
description: Final-pass checklist for safe Godot 4 .NET changes in this repository.
---

# Godot Change Checklist

Run this checklist mentally or explicitly before closing a change that touches gameplay code, scenes, or UI.

## Always Check

- The edited script still matches its owning scene and node names.
- Any `GetNode()` or `GetNodeOrNull()` path still points to a real node.
- Signal names and emitted arguments still match connected listeners.
- Exported properties still make sense for existing scene instances.
- You did not accidentally edit generated directories such as `.godot/` or `.nuget/`.

## If You Touched Player Or Combat

- Check attack, damage, invincibility, knockback, and movement interactions.
- Mention one concrete smoke test scenario such as movement sandbox, combat sandbox, or a room encounter.

## If You Touched Level Or Scene Flow

- Check spawn points, room transitions, checkpoints, camera bounds, and boot flow.
- Mention which scene should be opened first for manual verification.

## If You Touched UI

- Check HUD, pause/menu flow, and any signal wiring between UI and gameplay nodes.
- Mention at least one menu or HUD scenario to verify manually.

## If You Touched Save/Core

- Check startup path, scene loading, and persistence assumptions.
- Call out any state that should be tested across death, reload, or restart.

## Closeout Format

When summarizing, include:

- Changed files
- Verification performed
- Suggested manual smoke test
- Residual risk, if any
