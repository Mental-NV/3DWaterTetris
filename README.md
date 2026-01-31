# /docs — Floodline Design Docs

This directory contains the unified design specification for **Floodline**.

- `GDD_v0_2.md` — **Single source of truth** for MVP design + deterministic rules + subsystem specs + campaign plan.
- `README.md` — this file.

## How to use `GDD_v0_2.md`

### For designers / level authors
- Use **Part I** for the player-facing intent and content goals.
- Use **Part VIII** for the 30-level teaching beats and star constraints.
- When writing objectives, treat “height” as **world height** (`y`) unless explicitly marked otherwise.

### For engineers
- Implement simulation exactly per **Part II** (input timing/lock rules) and **Part III–V** (rules + hazards).
- Water equilibrium is fully defined in **Part IV**.
- World rotation must trigger a **Tilt Resolve** (Part III + Part II).
- Structural tuning:
  - use **ShiftVoxels*** for “collapse/instability” constraints,
  - use **LostVoxels*** only for explicit void/outflow levels.

## v0.2 changes (from v0.1 set)
- Split “height” into:
  - `worldHeight=y` (objectives, overflow, forbidden zones),
  - `gravElev=dot(c,u)` (solver ordering, water minimax).
- World rotation triggers immediate Tilt Resolve.
- “Collapse loss” star constraints migrated to **shift voxels**.
- Added: **Materials & Anchoring** + **Stabilize ability** spec.

## Editing conventions
- Keep deterministic rules in integer space; avoid floating-point in gameplay outcomes.
- If you add a new verb/hazard/material:
  1) specify it in the relevant Part (II–VI),
  2) add at least one deterministic test scenario in the engineering test suite.
