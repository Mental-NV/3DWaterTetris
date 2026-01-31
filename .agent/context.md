# Floodline — Project Context (Agent-Readable)

**Source of truth:** `/docs/GDD_v0_2.md` (v0.2).  
This file exists to make the agent autonomous without re-reading the full GDD every step.

---

## 1) Glossary (canonical terms)

- **Tick**: one fixed simulation step. MVP runs at **60 ticks/sec**.
- **Resolve Phase**: the deterministic post-lock step that settles solids/water, applies drains, and evaluates objectives/fail.
- **Tilt Resolve**: Resolve Phase triggered immediately after a successful world rotation for the *settled world*.
- **Gravity `g`**: one of `{DOWN, NORTH, SOUTH, EAST, WEST}` (no `UP` during gameplay).
- **Up `u`**: `u = -g`.
- **World height**: `worldHeight(c) = y` (player-facing; used for objectives/overflow/forbidden zones).
- **Gravity elevation**: `gravElev(c) = dot(c, u)` (solver-facing ordering scalar).
- **Tie coordinate**: `tieCoord(c) = (dot(c,U), dot(c,R), dot(c,F))` where `U=u`, `R` is chosen by the gravity table, `F=cross(U,R)`. Used only to break ties deterministically.
- **Support**: a solid voxel at `c` is supported if `c+g` is support-capable (`SOLID`, `WALL/BEDROCK`, `ICE`, `DRAIN`, etc.). `WATER` never supports.
- **Connected component**: 6-connected solid voxel group used by the settling algorithm.
- **Displaced water**: when a solid enters a water cell, the water unit is removed and re-inserted as a source for the next water settle.
- **Water settle**: equilibrium solver that computes stable water distribution (minimax path req + fill lowest).
- **Drain**: support-capable tile that removes deterministic water units after water settles.
- **Freeze / ICE**: converts water into temporary support-capable ICE for T resolves; then thaws back to water and re-settles.
- **Anchored voxel**: cannot move during solid settling; any component containing an anchored voxel is immovable.
- **Stabilize**: ability that temporarily anchors the locking piece for a fixed number of *successful rotations* (event-based countdown).
- **Shift voxels**: number of solid voxels that changed cell positions during settling (primary stability metric).
- **Lost voxels**: voxels removed by out-of-bounds/void (should be rare unless authored).
- **Golden test**: deterministic regression test pinned to exact expected outputs/hashes for a small scenario.
- **Replay**: record of level hash + seed + per-tick inputs + rules version; must reproduce identical outcomes.

---

## 2) Hard invariants (must never drift)

### 2.1 Determinism contract
Given the same:
- level file,
- RNG seed,
- per-tick input stream,
- rules version,

…simulation must produce identical results (grid state, objective progress, score).

**Core invariants**
- Fixed tick model (Resolve executed at tick boundaries).
- No floating-point in **Core** gameplay solver logic (solids/water/objectives).
- Canonical tie-break ordering: `(gravElev(c), tieCoord(c))` whenever ordering is required.

### 2.2 Canonical Resolve Phase order
Resolve is atomic and canonical:

1) Merge active piece -> record displaced water
2) Settle solids (until stable)
3) Settle water (equilibrium)
4) Recheck solids once (water moved)
5) Apply drains -> optionally re-settle water once
6) Evaluate objectives + fail states

**Loop cap:** at most **2 full solid-water cycles** per resolve.

### 2.3 World rotation rules (must match GDD)
- Rotation is snap 90° around horizontal axes.
- A successful rotation triggers **Tilt Resolve** for the settled world under new gravity.
- Active piece remains controllable and is treated as an immovable obstacle during Tilt Resolve.
- If Tilt Resolve would move a settled voxel into an occupied active-piece cell, the rotation is rejected (state rollback; no partial change).

### 2.4 Water rules (must match GDD)
- Water is discrete: 1 unit per cell.
- Water is **non-supporting** (treated as EMPTY for support).
- Passability vs occupiability:
  - Water can path through `EMPTY`, `POROUS` (passable), and cleared `WATER`.
  - Water cannot path through `SOLID`, `WALL/BEDROCK`, `ICE`, `DRAIN`.
  - Water can occupy only `EMPTY` (POROUS is passable but **not** occupiable in MVP).
- Equilibrium settle uses minimax-Dijkstra:
  - compute `req[c]` = minimal possible maximum elevation along any path from sources
  - fill the first N occupiable cells sorted by `(req[c], gravElev(c), tieCoord(c))`
- Displacement: solids entering water remove that unit and re-insert it as a source for the next settle.
- Drains:
  - processed deterministically by `(gravElev, tieCoord)`
  - remove up to `ratePerResolve` from a sorted candidate set by `(gravElev, tieCoord)`
  - then re-settle water once (recommended by spec)
- Freeze:
  - WATER -> ICE for T resolves (ICE supports)
  - on thaw: ICE -> WATER, add as source, re-settle

### 2.5 Input & feel invariants (for later Unity work)
- 60 ticks/sec.
- Input application order per tick: World rotate -> Stabilize -> piece rotate -> piece move -> soft drop -> hard drop.
- Lock behavior:
  - Lock delay = 12 ticks
  - Max lock resets = 4 per piece
  - Hard drop locks immediately and triggers Resolve

### 2.6 Architectural invariants
- **Core** is a pure C# assembly:
  - no UnityEngine usage
  - deterministic solvers and state transitions
- **Unity layer**:
  - rendering, input, camera, UI, VFX/SFX
  - may animate Resolve over frames but must not alter outcomes

---

## 3) Strict .NET baseline (repo policy)

These are repo-wide rules to maximize agent autonomy and avoid “works on my machine” drift:

- Central Package Management (Directory.Packages.props) is mandatory.
- Lock files are mandatory (`packages.lock.json` + locked restore in CI).
- Treat warnings as errors (`TreatWarningsAsErrors=true`) across all projects.
- Nullable reference types enabled (`Nullable=enable`) and enforced.
- Latest analysis level and strict analyzers enabled.
- Style rules enforced in build (`EnforceCodeStyleInBuild=true`).
- Formatting is a gate (dotnet format verify).
- Prefer deterministic builds: pin SDK via `global.json`.

---

## 4) Out-of-scope (do not implement in MVP unless explicitly requested)

- Smooth rotation with inertia (advanced mode)
- Additional materials beyond MVP list (porous “holds water”, floating, absorbent)
- Procedural endless mode mutators & leaderboards
- User-generated levels/editor UI
- Cosmetics/progression systems

---

## 5) MVP acceptance criteria (autonomy targets)

A change set is acceptable only if it preserves all hard invariants and achieves:

### A) Headless deterministic sim
- Core library supports grid, pieces, snap gravity, lock, Resolve, objectives, fail.
- A CLI runner can load a level, run a scripted input stream, and output:
  - final metrics (objectives, fail/win)
  - determinism hash (see contract policy)

### B) Golden regression suite
- Golden tests exist for:
  - water solver cases (basins, ridge spill, displacement, drain, freeze, gravity change)
  - solid settling ordering and “water does not support”
  - objective evaluation for the MVP types

### C) Replay + determinism hash
- Replay format records per-tick inputs and reproduces identical outcomes.
- Determinism hash is stable across machines for the same rules version.

### D) Level schema + validator
- Level JSON schema is versioned.
- Level validator detects invalid/ambiguous content (bad piece IDs, invalid gravity set, impossible objectives, etc.).
- Campaign validation can validate all 30 authored levels.

### E) Unity shell comes last
- Unity client work begins only after A–D are green.
