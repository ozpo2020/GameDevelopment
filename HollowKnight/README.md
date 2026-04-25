# Mossbound

Godot 4 C# 2D action game based on the Hollow Knight architecture roadmap.

## Run

1. Open Godot 4 .NET.
2. Import this folder as a project.
3. Press Play.

## Controls

- Move: A/D, Arrow Keys, D-pad, or left stick
- Jump: Space, K, or gamepad A
- Attack: J, Left Mouse Button, or gamepad X
- Down strike: hold Down while attacking in the air
- Dash: Shift, L, or right shoulder button
- Pause: Esc or Start

## Feel and Display

- Pixel HUD with health cells, dash marker, room title, objective marker, and pause overlay.
- Room-specific color palettes, background pillars, lamps, glowing transitions, and animated checkpoints.
- Attack hits trigger camera shake; down strike bounces the player upward.
- More detailed code-drawn placeholder models for the player, patrol enemy, flying enemy, mini boss, and stone platforms.
- Player attacks now have directional slash poses, blade trails, up-slash, down-strike, and body lean.
- Enemies use animated model states: patrol stepping, alert lunging, wing flapping, boss windup, and dash trails.

## Goal

Clear the game route.

- Move from the start room into the combat room.
- Defeat the patrol enemies to unlock dash.
- Use dash to pass the ability gate.
- Cross the flying enemy room.
- Defeat MiniBoss_01.
- Reach the game clear screen, then start a fresh run.

## Architecture

- `scripts/Core`: boot, input, save, scene/root orchestration, debug overlay.
- `scripts/Combat`: `DamageInfo`, `DamageTeam`, `Hitbox`, `Hurtbox`, health, invincibility, knockback.
- `scripts/Player`: controller, motor, combat, abilities, animation bridge.
- `scripts/Entities`: shared enemy base plus ground, flying, and mini boss enemies.
- `scripts/Level`: rooms, transitions, checkpoints, camera bounds, ability gate, map markers.
- `scripts/UI`: HUD, menu, settings presenter.
