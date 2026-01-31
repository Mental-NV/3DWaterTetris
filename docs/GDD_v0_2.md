# Floodline — Game Design Document v0.2 (Unified)
*Location:* `/docs/GDD_v0_2.md`
*Date:* 2026-01-31
*Target platform:* PC (Windows)
*Engine:* Unity (C#)
*Status:* MVP-focused, implementation-ready, deterministic

---


## 0. What this document contains

This single file is the v0.2 consolidation of the prior v0.1 document set:
- Player-facing GDD
- Input & Feel spec
- Rules Bible
- Water algorithm spec
- Wind hazard spec
- Piece library
- 30-level campaign plan

### v0.2 headline changes (applied)
- **Height semantics split:** player-facing “height” uses **world height** `y`; solver uses **gravity elevation** `gravElev(c)=dot(c,u)`.
- **Tilt has immediate consequences:** a successful world rotation triggers a **Tilt Resolve** for the settled world.
- **Collapse tuning metric fixed:** replace ambiguous “collapse loss” stars with **structural shift** (`ShiftVoxels*`); keep out-of-bounds removals as **LostVoxels*`.
- **Controls are single-source:** bindings live in Input & Feel (Part II).
- **Missing mechanics covered:** add **Materials & Anchoring** + **Stabilize ability** spec (Part VI).

---

## Table of contents

- **Part I — Player-Facing Design**
- **Part II — Input & Feel**
- **Part III — Rules Bible**
- **Part IV — Water Algorithm**
- **Part V — Wind Hazard**
- **Part VI — Materials, Anchoring, Stabilize**
- **Part VII — Piece Library**
- **Part VIII — 30-Level Campaign Plan**


---

# Part I — Player-Facing Design

## 1. High Concept
**Floodline** is a 3D voxel block-stacking puzzle where the player can **rotate the world** (changing gravity) to shape a growing “city” of blocks. A hostile fluid system—**Water**—flows to the lowest levels, **levels out in basins**, and **cannot support blocks**, causing collapses and forcing structural planning.

**Core fantasy:** *Architect a stable skyline while tilting gravity and managing floods.*

---

## 2. Design Pillars
1. **Gravity is a tool**  
   World rotation is a primary verb; it creates and resolves problems.

2. **Height is pressure**  
   The field becomes a vertical landscape. Tall structures are rewarding but unstable.

3. **Water is the antagonist**  
   Water undermines supports, forces basin engineering, and creates emergent collapses.

4. **Deterministic fairness**  
   Identical inputs produce identical outcomes. Outcomes must be readable and previewable.

5. **Readable > realistic**  
   Discrete voxel rules over continuous physics. Clarity beats simulation fidelity.

---

## 3. Target Audience & Experience Goals
- **Audience:** puzzle/strategy players; fans of Tetris variants, voxel puzzles, physics-lite builders.
- **Session length:** 5–15 minutes per run; campaign levels 3–8 minutes.
- **Experience goals:**
  - “Planned collapse” moments (intentional, rewarding).
  - Tight control over gravity changes.
  - Clear understanding of why a collapse or flood happened.

---

## 4. Core Gameplay Loop
1. **Spawn** an active piece above the board.
2. Player **moves/rotates** the piece and may **rotate the world** (snap 90°; gravity changes).
3. Piece **locks** when it can’t advance in gravity direction.
4. **Resolve Phase** executes deterministically:
   - merge piece → settle solids → settle water → recheck solids → apply drains → evaluate objectives.
5. Repeat until **win** (objectives met) or **lose** (overflow/constraints).

---

## 5. Core Systems

### 5.1 Playfield
- **Shape:** square base `N×N` (MVP: 10×10; some levels 8×8 or 9×9).
- **Height:** capped per level (e.g., 18–30) with overflow fail.
- **Walls/bedrock:** optional per level to shape basins and channels.

### 5.2 World Rotation / Gravity
- Rotation is **snap 90°** around horizontal axes.
- Gravity directions allowed per level: `{DOWN, NORTH, SOUTH, EAST, WEST}`.
- Constraints per level: cooldown, max rotations, tilt budget.

### 5.3 Pieces
- Pieces are **polycubes** (voxel-based shapes). Early levels use simpler sets.
- Piece system includes:
  - **Next preview** (3–5)
  - **Hold** (once per drop; per-level toggle)
  - Bag modes: fixed sequence (tutorial), weighted seeded (standard)

### 5.4 Solids & Stability (Player-facing)
- Solids must be supported in the gravity direction.
- Overhangs and unsupported structures collapse during resolve.
- Support **never** comes from water.

### 5.5 Water (Core Mechanic)
- Water occupies voxel cells (1 unit per cell).
- Water:
  - flows to **lowest reachable** cells under gravity,
  - **levels out** within basins,
  - can spill over ridges only if enough volume exists,
  - **cannot support** any block.
- If a solid moves into water, water is **displaced** (not destroyed).

### 5.6 Drains & Freeze (MVP tools)
- **Drain tiles:** remove nearby water units at a deterministic rate per resolve.
- **Freeze:** converts water to **ice** temporarily; ice supports blocks until thaw.

---

## 6. Objectives, Win/Lose, Scoring

### 6.1 Objective Types (MVP)
- **Drain Water:** remove `X` units via drains.
- **Reach Height:** achieve maximum **world height (y)** ≥ `H`.
- **Build Plateau:** create flat area `A` at a given **world height (y)**.
- **Stay Under Weight:** total mass placed ≤ limit (High-Rise).
- **Survive Rotations:** execute `K` rotations without failing.

### 6.2 Fail States (MVP)
- **Overflow:** any solid exceeds the configured **world height (y)** limit/out-of-bounds.
- **Weight exceeded:** mass placed > max (where configured).
- **Water forbidden zone:** water reaches a forbidden **world height (y)** threshold/region (where configured).
- (Optional) **Collapse threshold:** lose if collapse loss > threshold.

### 6.3 Scoring (for stars/endless)
- Placement: points per voxel placed.
- Water drainage: points per unit removed.
- Efficiency: multipliers for fewer pieces / fewer rotations.
- Penalties: collapse loss, water in forbidden areas.

### 6.4 Star Rating (recommended)
- Star 1: complete primary objectives.
- Star 2: efficiency constraint (pieces used or rotation budget).
- Star 3: stability constraint (max collapse loss).

---

## 7. Game Modes

### 7.1 Campaign (Primary MVP)
A curated 30-level campaign teaching mechanics in phases:
- Chapter 1: Foundation (rotation + stability)
- Chapter 2: Floodplain (water + drains + freeze)
- Chapter 3: High-Rise (height vs weight + wind)

### 7.2 Endless (Post-MVP or light MVP)
- Seeded progression with stacked mutators:
  - higher piece complexity,
  - tighter height limit,
  - periodic water injections,
  - wind forcing.
- Score attack + leaderboards (later).

### 7.3 Challenge Puzzles (Optional)
- Fixed sequences, limited rotations.
- One-solution “engineering” problems.

---

## 8. Campaign Content Plan (First 30 Levels)
**Chapter 1 — Foundation (Lv 1–10):** introduce rotation, collapse, plateau building, tilt budget.  
**Chapter 2 — Floodplain (Lv 11–20):** water basins, drain usage, channel building, freeze tactics.  
**Chapter 3 — High-Rise (Lv 21–30):** weight limits, smaller footprints, wind forcing, stabilization.

(Use the separate “30-level campaign plan” document as the design reference for specific beats.)

---

## 9. Difficulty & Progression Knobs
- Piece complexity (shape entropy)
- Rotation scarcity (cooldown / tilt budget)
- Height limit (pressure)
- Water volume and basin count
- Drain power (rate) and placement constraints
- Wind intensity and patterning (High-Rise)

Design goal: difficulty emerges from **system interaction**, not raw speed.

---

## 10. UX / Readability Requirements (Must-Have)
To maintain fairness perception:

### 10.1 HUD
- Prominent **gravity arrow** + label
- Objectives + progress bars
- Next pieces + hold
- Rotation budget/cooldown indicator
- Ability charges (drain/freeze)

### 10.2 World Feedback
- Highlight **unsupported solids** before/after resolve.
- Water surface readability (clear flat top plane).
- Warnings:
  - “Resting on water”
  - “Unsupported component”
  - “Overflow risk”

### 10.3 Optional Previews (Nice-to-have)
- Rotation preview: projected settle ghost (expensive but powerful).
- Basin preview: show lowest basins and drain influence zones.

---

## 11. Art Direction
- Clean voxel aesthetic with strong silhouette readability.
- Minimal, high-contrast materials:
  - Standard: neutral
  - Heavy: hazard stripe / darker
  - Reinforced: metal frame
  - Water: translucent with strong surface line
  - Ice: bright, crystalline

Environment: floating platform over clouds/mountains (non-distracting).

---

## 12. Audio Direction
- Reactive SFX: lock, drop, rotate, collapse, water flow, drain, freeze/thaw.
- Music: ambient/low-intensity that ramps with danger state (near overflow / flood threshold).

---

## 13.

### Controls & bindings
Controls are defined once in **Input & Feel Spec v0.2** (see §8 in this unified document). Rebinding is supported via Unity Input System.

## 14. Technical & Architecture (Unity)
### 14.1 Code Separation
- **Core (pure C# assembly):**
  - grid state, piece logic, settle solvers (solids + water), objective evaluator, PRNG
  - unit tests + deterministic regression tests
- **Unity layer:**
  - rendering voxels, UI, camera, input, animations, VFX, audio

### 14.2 Rendering Approach
- MVP: GPU instanced cubes (per voxel) with chunking later.
- Avoid Rigidbody for gameplay outcomes; use for VFX only.

### 14.3 Data & Tools
- Levels in JSON (schema-based) + Unity importer.
- Debug overlays:
  - gravity vector
  - unsupported highlight
  - water req-level heatmap
  - component IDs

### 14.4 Determinism & Replay
- Fixed tick simulation.
- Replay format: level hash + seed + tick inputs + rules version.
- Golden tests for water + settle order.

---

## 15. Production Plan (MVP)
### Milestone A — Core Sim (No visuals)
- Grid, pieces, rotation, lock, resolve.
- Solids settling + water solver + drains + freeze.
- Unit tests + determinism tests.

### Milestone B — Unity Shell
- Rendering + camera + UI + input.
- Debug overlays.

### Milestone C — Campaign Authoring
- Level importer + validator.
- Implement first 30 levels; tune difficulty.

### Milestone D — Polish
- VFX/SFX, readability improvements, tutorial prompts.
- Build pipeline, installer/Steam package.

---

## 16. Risks & Mitigations
1. **“Feels random”** due to collapse cascades  
   - deterministic order, strong debug/highlights, optional previews.

2. **Water confusion**  
   - basin-level visuals, drain scope highlight, tutorial beats.

3. **Content bottleneck**  
   - level template + validator + in-engine debug tools.

4. **Performance at high heights**  
   - cap height in campaign, chunk rendering, avoid per-frame full-grid scans.

---

## 17. Open Decisions (Acceptable to Defer Past MVP)
- Smooth rotation with inertia (advanced mode)
- More materials (porous, absorbent, floating)
- Procedural endless mode mutators & leaderboards
- User-generated levels / editor UI
- Cosmetic progression (skins/themes)

---

## 18. Appendices

### A) Terminology
- **Resolve Phase:** deterministic post-lock simulation step.
- **Elevation:** scalar height along “up” direction (`u = -g`).
- **Basin leveling:** water fills lowest reachable cells then levels across connected areas.

### B) References
- Rules & Algorithms (this document Part II)
- Level JSON template (schema + examples)
- 30-level campaign plan (this document §13)

---

*End of Part I — Player-Facing Design*


---

# Part II — Input & Feel

# Floodline — Input & Feel Spec v0.2 (MVP)
*Engine:* Unity (C#)  
*Scope:* Player input timing rules (grid movement), drop behavior, lock behavior, and camera rules that affect “feel.”  
*Goal:* Skillful but readable control, minimal implementation risk, deterministic.

---

## 1) Simulation Timing
- Fixed tick simulation: **60 ticks/sec**.
- Active piece motion is evaluated once per tick.
- Resolve Phase runs immediately on lock (outcomes deterministic).

---

## 2) Input Model (Actions)
Actions are sampled each tick and applied in canonical order:

1. **World rotate** (snap 90°) if pressed and allowed.  
   - On success, immediately execute a **Tilt Resolve** for the settled world (locked solids + water) under the new gravity.  
   - The active piece remains controllable and is treated as an immovable obstacle during Tilt Resolve.
2. **Stabilize ability** (if available) — arms the current active piece to anchor on lock (see §7).
3. **Piece rotate** (local rotation) if pressed.
4. **Piece move** (horizontal translate) via hold-repeat.
5. **Soft drop** (accelerate gravity step rate).
6. **Hard drop** (immediate place+lock).

If multiple inputs of the same class occur, apply in the order they were received this tick (or by fixed priority).

---

## 3) Movement Repeat (DAS/ARR Equivalent)
All movement is in integer grid cells.

### 3.1 Horizontal move (X/Z)
- **Initial delay (DAS):** 10 ticks (≈166 ms)
- **Repeat rate (ARR):** 2 ticks per step (≈33 ms)
- If direction changes, restart DAS timer.

### 3.2 Soft drop
- Soft drop increases gravity stepping to **1 cell per tick** (60 cells/sec) until lock.
- Soft drop does not change lock delay policy.

### 3.3 No “infinite slide”
Active piece movement is permitted while falling, but lock delay limits late adjustments.

---

## 4) Lock Behavior (MVP)
### 4.1 Lock condition
Piece locks when it cannot advance one cell along gravity direction due to collision/out-of-bounds.

### 4.2 Lock delay
- **Lock delay:** 12 ticks (≈200 ms)
- Lock delay starts when the piece first becomes “grounded.”
- If the piece becomes ungrounded (due to world rotation or successful move), lock delay resets.

### 4.3 Lock delay reset limits
To prevent abuse:
- Maximum **4 lock resets** per piece.
- A “lock reset” occurs when grounded → ungrounded transition happens.

### 4.4 Hard drop
- Hard drop moves the piece along gravity as far as possible and **locks immediately** (no lock delay).
- Then Resolve Phase runs.

---

## 5) Piece Rotation Rules
### 5.1 Rotation kick
Piece rotation uses the deterministic kick set defined in **Piece Library v0.2**:
- Try no kick, then small translations in fixed order.
- If all fail, rotation is rejected.

### 5.2 Rotation while grounded
Allowed. If rotation succeeds and changes grounded state, it can trigger a lock reset (counts toward reset limit).

---

## 6) World Rotation Rules (Player-facing)
- World rotation is snap 90°.
- Allowed while active piece is falling.
- Not allowed during Resolve Phase (input buffered until after).

---

## 7) Camera Rules (MVP)
### 7.1 Camera vs gravity
MVP uses **camera-stable horizon**:
- The camera does not auto-rotate to match gravity.
- World rotation is visually animated (board tilts), but the camera remains comfortable.

### 7.2 Snap views
Provide 4 corner snap views (hotkeys) for readability:
- NE, NW, SE, SW isometric angles.

### 7.3 Focus behavior
On spawn, camera recenters to keep the active piece and the top of the structure visible.

---

## 8) Input Defaults (PC)
- Move: A/D (x-) / (x+), W/S (z+) / (z-)
- Local piece rotation:
  - yaw: Q/E
  - pitch: R/F
  - roll: Z/C
- Soft drop: Left Ctrl (or S, if not used for move)
- Hard drop: Space
- Hold: Left Shift
- World rotation:
  - Tilt forward/back: 1 / 2
  - Tilt left/right: 3 / 4
- Stabilize (ability): V (if level enables)
- Camera snap: F1–F4

(Rebinding supported via Unity Input System.)

---

## 9) Tuneables (Expose in a config asset)
- Tick rate (fixed)
- DAS, ARR
- Soft drop speed
- Lock delay ticks
- Max lock resets
- World rotation cooldown ticks

---

*End of Input & Feel Spec v0.2*


---

# Part III — Rules Bible

*Status:* MVP-lock specification (deterministic).  
*Target engine:* Unity (C#), Windows PC.  
*Scope:* This document defines the **canonical gameplay rules** and **resolution order** for solids, gravity rotation, water, drains/freeze, objectives, and fail states. It is intended to prevent implementation drift.

---

## 0. Non‑Goals
- No continuous fluid simulation.
- No Unity Rigidbody physics for gameplay outcomes (visual-only allowed).
- No smooth rotation/inertia in MVP (snap only).

---

## 1. Determinism Contract (Hard Requirement)
Given:
- the same level file,
- the same RNG seed,
- the same input stream (player actions with timestamps in fixed ticks),
- the same rules version,

…the simulation must produce identical results (grid state, objective progress, score).

### 1.1 Fixed Tick Model
- Simulation advances in **fixed ticks** (e.g., 60 Hz).  
- **Resolve Phase** is executed atomically at tick boundaries (may be animated over multiple frames, but outcomes are locked at start).

### 1.2 No Floating-Point in Core
Core solver (solids, water, objectives) uses:
- integer coordinates,
- integer comparisons,
- deterministic iteration order.

### 1.3 Canonical Tie-Break
Whenever ordering is needed, sort by the tuple:
1. `gravElev(c)` (see §2.3) ascending  
2. `tieCoord(c)` ascending (see §2.4)

---

## 2. Grid Model

### 2.1 Coordinates
Grid cells are integer coordinates:
- `c = (x, y, z)` where `x∈[0..X-1]`, `z∈[0..Z-1]`, `y∈[0..H-1]`
- Base footprint is square or rectangular; MVP uses square.

### 2.2 Cell Occupancy Types
Each cell contains **exactly one** of:
- `EMPTY`
- `SOLID(materialId)` (supports blocks)
- `WALL` / `BEDROCK` (immovable SOLID)
- `WATER` (1 unit, non-supporting)
- `ICE` (frozen water; supports and blocks)
- `POROUS(materialId)` (supports; passable for water pathing; not occupiable by water)
- `DRAIN(params)` (acts as SOLID for support; removes nearby water)

> MVP materials: `STANDARD`, `HEAVY`, `REINFORCED`. (Porous can be off initially.)

### 2.3 Gravity and Height Semantics
Gravity is a cardinal direction vector `g ∈ {±X, ±Y, ±Z}`.  
In MVP, the player can rotate around horizontal axes, so gravity is restricted to:
- `DOWN`, `NORTH`, `SOUTH`, `EAST`, `WEST` (no `UP` during gameplay).

We use **two different height concepts**:

1) **World height (player-facing, objectives/overflow):**
- `worldHeight(c) = y` (constant “vertical” axis in the level file and camera).

2) **Gravity elevation (solver-facing, settling + water equilibrium ordering):**
- Define gravity-up direction: `u = -g`.
- `gravElev(c) = dot(c, u)` (integer scalar).

Examples:
- If `g = (0, -1, 0)` then `u=(0,1,0)`, `gravElev = y`.
- If `g = (1,0,0)` then `u=(-1,0,0)`, `gravElev = -x` (used for ordering and minimax water paths).

**Rule:** objectives that say “height” or “forbidden zone” use `worldHeight`, never `gravElev`, unless explicitly stated.

### 2.4 Gravity-Dependent Tie Coordinates

To keep tie-break stable across gravity, define a local orthonormal axis triple `(U, R, F)`:
- `U = u`
- `R` is selected by table per gravity (fixed constants)
- `F = cross(U, R)` (also fixed)

Then:
- `tieCoord(c) = (dot(c,U), dot(c,R), dot(c,F))`

#### 2.4.1 Right-axis table (MVP)
Use the following `R` mapping:

| Gravity `g` | Up `U=-g` | Right `R` |
|---|---|---|
| DOWN (0,-1,0) | (0,1,0) | (1,0,0) |
| NORTH (0,0,-1) | (0,0,1) | (1,0,0) |
| SOUTH (0,0,1) | (0,0,-1) | (1,0,0) |
| EAST (1,0,0) | (-1,0,0) | (0,0,1) |
| WEST (-1,0,0) | (1,0,0) | (0,0,1) |

This avoids rotation-jitter in “same cost” cases.

---

## 3. World Rotation (Snap 90°)

### 3.1 Rotation Inputs
- World rotation is a **90° snap** around X or Z axis (horizontal axes).
- Rotation is allowed:
  - while an active piece is falling,
  - while no piece is falling,
  - during post-lock resolve is **not** allowed (inputs buffered for after resolve).

### 3.2 Rotation Effects
On successful rotation (snap 90°):
1. Gravity `g` is updated instantly.
2. The **active piece** continues falling under the new gravity.
3. A **Tilt Resolve** is executed immediately for the *settled world* (locked solids + water), using the new gravity:
   - treat the active piece’s cells as occupied, immovable obstacles during Tilt Resolve;
   - if Tilt Resolve would require moving a settled voxel into an occupied active-piece cell, the rotation is **rejected** (state rolls back; no partial changes).

*Rationale:* Tilting should have immediate, readable consequences, and authored levels rely on this.

### 3.3 Rotation Constraints (per level)
 Rotation Constraints (per level)
A level can constrain rotation via:
- allowed directions set `{DOWN, NORTH, SOUTH, EAST, WEST}`,
- cooldown seconds,
- max rotations or tilt budget.

If a rotation input violates constraints, it is ignored.

---

## 4. Active Piece Rules

### 4.1 Representation
The active piece is a set of local voxel offsets `P = {p_i}` around an origin cell.

### 4.2 Movement & Collision
- Piece can translate/rotate in discrete grid steps.
- A move is valid if all occupied destination cells are in bounds and are `EMPTY` or `WATER` (see §4.4).
- The active piece does **not** merge into the grid until it locks.

### 4.3 Lock Condition
A piece locks when, after applying gravity step attempts:
- it cannot advance one cell along `g` because at least one voxel would collide with `SOLID/WALL/BEDROCK/ICE/DRAIN` or out-of-bounds.

(Optionally allow “lock delay”; if used, lock delay is measured in fixed ticks and must be deterministic.)

### 4.4 Landing Into Water
If a piece voxel would enter a `WATER` cell on lock placement:
- it is allowed; the water will be **displaced** during resolve (see §6.4).

---

## 5. Resolve Phase (Canonical Order)
Resolve is executed atomically immediately after lock (and also after any scripted events that demand stability).

Order:

1. **Merge Active Piece**
   - Convert its voxels into `SOLID(materialId)` (or special block voxels).
   - Record all overlaps with water as **displaced water sources** (see §6.4).

2. **Settle Solids**
   - Drop unsupported solid components along `g` until stable (§6).

3. **Settle Water**
   - Compute deterministic equilibrium distribution (§7).

4. **Recheck Solids**
   - Because water moved, re-run solid settling once more (§6).
   - (If this creates new water displacement, add to sources and re-settle water once.)

5. **Apply Drains**
   - Remove water per drain rules (§8), then optionally re-settle water once.

6. **Evaluate Objectives & Fail States**
   - Update progress, award score, check win/lose (§10, §11).

> Loop cap: At most **2 full solid-water cycles** per resolve to avoid pathological oscillations. (Empirically should converge; cap is safety.)

---

## 6. Solid Stability & Settling (Deterministic)

### 6.1 Support Rule
A solid voxel at cell `c` is supported if the adjacent cell `b = c + g` is one of:
- `SOLID` (any material),
- `WALL/BEDROCK`,
- `ICE`,
- `DRAIN` (support-capable),
- or other support-capable special tile.

It is **not supported** if `b` is:
- `EMPTY`, or
- `WATER`.

### 6.2 Connected Components
Solids settle as connected components using 6-connectivity (faces touch).

Procedure:
1. Build all solid voxels set `S` (including bedrock, walls excluded if immovable).
2. Identify components `C_k` among **movable** solids (exclude immovable walls/bedrock).
3. For each component, compute whether it is supported (any voxel in component has support under rule, or component is attached to immovable support via adjacency).

### 6.3 Component Drop Distance
For each unsupported component `C`:
- Compute maximum integer drop distance `d ≥ 1` such that moving all voxels `c ∈ C` to `c + d*g` stays in bounds and does not collide with:
  - immovable solids,
  - other solids not in C,
  - ice,
  - drains,
  - walls/bedrock.

`WATER` counts as empty for collision purposes (since water will be displaced).

Drop the component by `d` in one step.

### 6.4 Displaced Water From Solid Movement
If a solid voxel enters a water cell during merging or dropping:
- the water unit is removed and counted as displaced.
- displaced units become water sources in the next water settle (§7.1).

### 6.5 Iteration Order
To avoid “which component falls first” ambiguity, process unsupported components in deterministic order:
Sort components by:
1. `minElev(C) = min(gravElev(c))` over voxels in C ascending
2. `minTie(C) = min(tieCoord(c))` ascending

Then process in that order and repeat until no unsupported components remain or step cap reached.

### 6.6 Out-of-Bounds & Lost Voxels
If during any movement (active piece fall, settling, or special level rules) a solid voxel would go out-of-bounds, that voxel is removed and counted as **lost**.

- This metric is **not** used as the primary “collapse” tuning knob in v0.2.
- Structural instability is tuned primarily via **shift** (`ShiftVoxels*`), i.e., how much the structure moves during settling.

In MVP, correct collision/bounds enforcement should make **lost voxels** rare unless the level explicitly includes void openings or “outflow” rules.

---



## 7. Water Rules (Deterministic Equilibrium)

Water is simulated as **discrete units** that re-settle deterministically after every Resolve Phase (and after a successful world rotation, via Tilt Resolve).

**Key properties**
- Water occupies cells as `WATER`.
- Water is **non-supporting**: it never counts as structural support for solids.
- Water can be **displaced** by solids entering a cell; displaced units are re-inserted during the next water settle.
- Drains remove water deterministically after water has settled.

**Authoritative algorithm**
- See **Part IV — Water Algorithm Spec v0.2** (this document).

## 8. Drains (Exact)

### 8.1 Drain Scope
Each drain has:
- `ratePerResolve` (integer)
- `scope` ∈ {`SELF`, `ADJ6`, `ADJ26`}

### 8.2 Drain Order
Drain tiles are processed in deterministic order by:
- `gravElev(d)` ascending, then `tieCoord(d)`.

### 8.3 Removal Rule
After water settle:
1. Gather all water cells within drain scope.
2. Sort by `(gravElev(c), tieCoord(c))` ascending.
3. Remove up to `ratePerResolve` units (set cells to `EMPTY`).

Then **re-run water settle once** (recommended for clarity).

---

## 9. Freeze / Ice (Exact)

### 9.1 Freeze Action
Freeze converts target `WATER` cells to `ICE` for `T` resolves (integer duration).

- `ICE` behaves as `SOLID` for collision and support.
- `ICE` is **impassable** and **not occupiable** by water.

### 9.2 Thaw
When timer expires:
- `ICE` cells convert back to `WATER`,
- those positions are added to water sources,
- water settle is executed.

---

## 10. Objectives & Evaluation Order

### 10.1 Evaluation Timing
Objectives are evaluated **only after the full Resolve Phase** completes (§5).

### 10.2 Canonical Metrics
Metrics are computed at end-of-resolve unless noted.

**Placement / progression**
- `PiecesUsed`: count of locked pieces (includes held pieces when locked later).
- `MaxWorldHeight`: `max(worldHeight(c))` over all **solid** voxels.

**Structural change**
- `ShiftVoxelsResolve`: number of solid voxels that changed cell position during **Solid Settling** in the current Resolve Phase.
- `ShiftVoxelsTotal`: cumulative shift over the level.
- `LostVoxelsResolve`: number of solid voxels removed in the current resolve due to out-of-bounds / explicit void rules.
- `LostVoxelsTotal`: cumulative lost voxels over the level.

**Water**
- `WaterRemovedTotal`: cumulative water units removed by drains or other removals.
- `WaterInForbiddenZone`: any water cell with `worldHeight(c) >= threshold`.

### 10.3 Objective Types (MVP)
 Objective Types (MVP)
- `DRAIN_WATER(targetUnits)` measured as `WaterRemovedTotal`
- `REACH_HEIGHT(height)` measured as `MaxWorldHeight`
- `BUILD_PLATEAU(area, worldLevel)` exact set match or floodfill area on same world height
- `STAY_UNDER_WEIGHT(maxMass)` mass = sum of placed voxels weighted by material mass
- `SURVIVE_ROTATIONS(k)` count of successful rotations executed

### 10.4 “No Resting On Water”
If a level enforces “no voxel resting on water”:
- Evaluate at end-of-resolve:
  - For every solid voxel `c`, if cell `c+g` is `WATER`, the condition fails.

(If you need stricter “ever happened” behavior, track a boolean flag when detected during resolve.)

---

## 11. Fail States (MVP)
Lose immediately if any condition becomes true after resolve:
- `OVERFLOW`: any solid voxel outside allowed height limit
- `TILT_BUDGET_EXCEEDED`: budget < 0 (rotation input rejected before this in normal flow)
- `WEIGHT_EXCEEDED`: total placed mass > maxMass
- `WATER_FORBIDDEN`: water in forbidden zone (if configured)

Win when all primary objectives are satisfied (checked after resolve).

---

## 12. RNG, Bags, and Hold

### 12.1 RNG
- Single PRNG (e.g., XorShift128+ or PCG32) seeded per level.
- Only used for:
  - piece selection (unless fixed sequence),
  - material roll (if enabled),
  - cosmetic-only particles may use Unity RNG but must not affect gameplay.

### 12.2 Piece Bag Modes
- `FIXED_SEQUENCE`: exact list for tutorials.
- `WEIGHTED`: weighted draw with replacement.
- (Optional later) `BAG_N`: shuffle bag, draw without replacement.

### 12.3 Hold (MVP)
- Hold enabled per level.
- Rule: can hold **once per piece drop** (classic restriction).
- Holding swaps current piece with hold slot; held piece resets to spawn orientation.

---

## 13. Replay & Versioning (Recommended)
To guarantee determinism across updates:
- Record:
  - `rulesVersion` (e.g., 0.1),
  - level id/hash,
  - seed,
  - per-tick inputs (move/rotate/tilt/hold/drop).
- Maintain backward compatibility by keeping old solvers or migrating replays.

---

## 14. Debug Views (Must-Have for Development)
- Show gravity arrow and `g` label.
- Toggle “unsupported solids” highlight (cells where support fails).
- Water debug:
  - show `req[c]` heatmap (minimax required level),
  - show basin boundaries.
- Component debug:
  - color connected components by ID,
  - print `minElev` ordering list.
- Determinism test mode:
  - run 100 resolves with random seeds and compare hashes to golden outputs.

---

## 15. Implementation Notes (Unity)
- Keep solvers in a **pure C# assembly** (no UnityEngine).
- Unity layer only:
  - renders the voxel state,
  - plays animations during resolve,
  - collects inputs and feeds fixed ticks.

---

*End of Rules Bible v0.2*


---

# Part IV — Water Algorithm

*Engine:* Unity (C#)  
*Scope:* Canonical, deterministic **water equilibrium solver** and exact rules for **displacement**, **drains**, and **freeze/ice**.  
*Status:* MVP-lock specification.

> This document is the single source of truth for all water behavior.  
> It is compatible with **Rules Bible v0.2** and referenced by level JSON.

---

## 1) Goals & Constraints
Water must:
- **Spread evenly** at the lowest reachable level(s).
- **Never support** solids.
- React deterministically to gravity changes and block movement.
- Be implementable as a fast discrete solver for typical boards (10×10×20–60).

Non-goals:
- continuous fluid simulation
- turbulence / velocities
- fractional water volumes (MVP uses one unit per cell)

---

## 2) Grid & Terminology

### 2.1 Grid
- Cells `c = (x, y, z)` with integer coordinates.
- Board footprint: `0 ≤ x < X`, `0 ≤ z < Z`
- Simulation height: `0 ≤ y < H` (use `H = heightLimit + margin`)

### 2.2 Occupancy
Each cell holds exactly one:
- `EMPTY`
- `SOLID(materialId)`
- `WALL/BEDROCK` (immovable)
- `WATER` (one unit)
- `ICE` (frozen water; supports)
- `POROUS` (supports; passable for water pathing; not occupiable)
- `DRAIN` (support-capable; removes water)

### 2.3 Gravity & Elevation
Gravity is cardinal `g`. Up direction `u = -g`.

Define elevation (integer):
- `gravElev(c) = dot(c, u)`

### 2.4 Canonical tie-break coordinate
Use local axes `(U,R,F)` from Rules Bible v0.2:
- `U = u`
- `R` from table
- `F = cross(U,R)`

Then:
- `tieCoord(c) = (dot(c,U), dot(c,R), dot(c,F))`

Whenever sorting is required, order by:
1) `gravElev(c)` ascending  
2) `tieCoord(c)` ascending

---

## 3) Core Water Rules (Gameplay)

### 3.1 Water is non-supporting
A solid voxel is supported by the cell in direction `g` only if that cell is support-capable.  
`WATER` is treated as `EMPTY` for support.

### 3.2 Water occupies cells as discrete units
In MVP:
- each `WATER` cell represents exactly one unit
- no fractional volumes

### 3.3 Water cannot overlap solids
A cell cannot contain both water and solid.  
When solids enter water, water is **displaced** (see §6).

---

## 4) Passability vs Occupiability

### 4.1 Passability (for water pathfinding)
Water can traverse (graph edges) through:
- `EMPTY`
- `POROUS`
- (conceptually) `WATER` itself (during solver it’s cleared to empty)

Water cannot traverse through:
- `SOLID`, `WALL/BEDROCK`, `ICE`, `DRAIN`

### 4.2 Occupiability (where water can end up)
Water can occupy only:
- `EMPTY` cells

Water cannot occupy:
- `POROUS` (default MVP), solids, walls, drains, ice

> If you later want porous “to hold water,” allow occupiable porous. MVP default: **passable but not occupiable**.

---

## 5) Water Settle Algorithm — Deterministic Equilibrium
This solver computes the stable water distribution after changes (block placement, collapse, rotation).

### 5.1 Inputs & Outputs
**Input:**
- current grid state
- gravity direction `g`
- current water cells (as units)
- displaced water sources (from solids entering water)
- optional spawned water sources (later)

**Output:**
- set of water cells after settling (equilibrium)

### 5.2 High-level idea
Compute which cells are reachable from water sources if the water surface were allowed to rise.  
Then fill the lowest reachable cells first, producing:
- flat surfaces inside basins,
- correct spillover when volume is sufficient,
- deterministic, no update-order artifacts.

### 5.3 Algorithm steps (exact)

#### Step A — Collect water units and sources
Let:
- `W = { c | grid[c] == WATER }`
- `N = |W|` (number of water units)
- `S = list(W)` (water sources)

Additionally:
- For each displaced water event at cell `d` (solid entered water):
  - increment `N += 1`
  - add `d` to `S`

Optionally later:
- Add spawned water units similarly.

Then:
- set all `grid[c] == WATER` to `EMPTY` temporarily.

#### Step B — Compute minimax flood levels (`req[c]`)
We compute a value for each passable cell `c`:
- `req[c]` = minimal possible *maximum elevation* encountered along any path from any source `s ∈ S` to `c`.

This is a minimax path problem solved by Dijkstra with transition:
- `cand = max(req[cur], gravElev(next))`

Initialize:
- `req[c] = +∞` for all cells
- For each source `s` that is passable:
  - `req[s] = gravElev(s)`
  - push `s` into a priority queue

Priority queue order:
- `(reqVal, gravElev(cell), tieCoord(cell))`

Traverse:
- use 6-neighbor adjacency
- only traverse into passable cells

#### Step C — Build fill candidate list
Create list:
- `C = { c | c is occupiable (EMPTY) and req[c] != +∞ }`

Sort `C` by:
1) `req[c]` ascending (minimum water surface needed to reach c)
2) `gravElev(c)` ascending (lowest first within the same spill level)
3) `tieCoord(c)` ascending

#### Step D — Fill N units
Take the first `N` cells in sorted list `C` and set them to `WATER`.

If `N > |C|`, then the level has overflowed water capacity:
- MVP behavior: clamp to `|C|` and count overflow as failure only if level defines a water overflow lose rule. (By default, no such lose rule.)

### 5.4 Properties guaranteed
- **Even spread at low levels:** lowest reachable cells filled first.
- **Basins level:** within a basin, equal elevation fills are spread across all reachable cells at that elevation.
- **Spillover:** only occurs when required surface level (`req`) must rise to pass a ridge.
- **Deterministic:** fixed tie-break ordering.

---

## 6) Solid–Water Interaction (Displacement)
When a solid voxel moves into a water cell, water is displaced, not destroyed.

### 6.1 When displacement occurs
- On merge of active piece into grid (lock)
- During solid settling drops (component movement)

### 6.2 Displacement rule (exact)
If a solid voxel occupies a cell that currently contains `WATER`:
1. Remove water from that cell immediately.
2. Record a displaced water source at that cell (same coordinates).
3. Increment `displacedCount`.

After solids settle (before water settle), add displaced water units:
- `N += displacedCount`
- `S += displacedPositions`

Then run the water settle algorithm (§5).

This yields intuitive “squeezing out” behavior.

---

## 7) Drains (Exact)
Drains remove water units deterministically during resolve.

### 7.1 Drain definition
A drain tile has:
- `ratePerResolve` (integer units)
- `scope`: `SELF` | `ADJ6` | `ADJ26`

### 7.2 Drain order
Process drains in deterministic order:
- sort drains by `(gravElev(d), tieCoord(d))` ascending

### 7.3 Removal procedure
After water settle:
1. Enumerate all water cells in drain scope.
2. Sort candidates by `(gravElev(c), tieCoord(c))` ascending
3. Remove up to `ratePerResolve` units:
   - set cells to `EMPTY`
   - increment `WaterRemovedTotal`

### 7.4 Reflow after drain
After all drains processed:
- re-run water settle **once** (recommended for clarity), using remaining water cells as sources.

---

## 8) Freeze / Ice (Exact)
Freeze converts water into temporary support.

### 8.1 Freeze action
Freeze targets a set of water cells (selection method is UI/ability-specific):
- Each targeted `WATER` cell becomes `ICE`
- ICE duration: `T` resolves (integer), decremented after each resolve

### 8.2 ICE behavior
ICE:
- is support-capable (treated as SOLID for collision and support)
- is impassable and not occupiable by water
- does not move

### 8.3 Thaw
When a frozen cell’s timer expires:
- ICE converts back to WATER
- That cell is added to water sources
- Run water settle (§5)

---

## 9) Optional Extensions (Post-MVP)
These are not active in MVP but are compatible with the solver:

### 9.1 Leak-to-void
Add an “outside sink” node and treat boundary openings as escape paths.  
Then water can leave the field if there is a path to outside with `req` below the current water surface.

### 9.2 Fractional water volume
Store per-cell volume (0..1) and run a similar basin-level fill with partial cells, but visuals/complexity increase significantly.

### 9.3 Porous holds water
Allow POROUS to be occupiable; keep it passable. This turns porous into “sponges”/channels.

---

## 10) Test Cases (Must-Have)
1. **Single basin fill:** water fills lowest cells, flat surface.
2. **Two basins with ridge:** small N stays in low basin; large N spills into second basin.
3. **Displacement:** dropping a solid into water increases water elsewhere by exactly 1 unit.
4. **Drain:** removing k units reduces total water by k, then reflow produces expected distribution.
5. **Freeze:** frozen cells support solids; thaw reintroduces water and reflows deterministically.
6. **Gravity change:** rotate gravity and settle; results match golden hash for fixed seed.

---

*End of Water Algorithm Spec v0.2*


---

# Part V — Wind Hazard

*Engine:* Unity (C#)  
*Scope:* Exact wind event rules for High-Rise chapter.  
*Design intent:* Wind adds forcing without randomness. It must be predictable, telegraphed, and deterministic.

---

## 1) Wind Applies To (MVP Choice)
**Wind affects only the active falling piece**, not the settled structure.

Rationale:
- avoids surprising cascades,
- keeps gameplay skill-based (steer + plan),
- simplifies determinism and level tuning.

---

## 2) Wind Event Definition (Level JSON)
```json
{
  "type": "WIND_GUST",
  "enabled": true,
  "params": {
    "intervalSeconds": 10,
    "pushStrength": 1,
    "directionMode": "ALTERNATE_EW",
    "firstGustOffsetSeconds": 3
  }
}
```

### 2.1 Parameters
- `intervalSeconds` (float, converted to ticks deterministically)
- `firstGustOffsetSeconds` (optional; if absent, computed from seed)
- `pushStrength` (int): number of cells to attempt to shove (MVP: 1)
- `directionMode`:
  - `ALTERNATE_EW`: East, West, East, West…
  - `FIXED`: uses `fixedDirection`
  - `RANDOM_SEEDED`: uses level PRNG but deterministic

---

## 3) Tick Scheduling (Deterministic)
Convert seconds to ticks:
- `intervalTicks = round(intervalSeconds * 60)`
- `offsetTicks = round(firstGustOffsetSeconds * 60)` (or seeded)

Event fires at ticks:
- `t = offsetTicks + k * intervalTicks` for k = 0,1,2,...

If `intervalTicks < 1`, clamp to 1.

### 3.1 Seeded offset (if not provided)
If `firstGustOffsetSeconds` is missing:
- `offsetTicks = PRNG(seed).NextInt(0, intervalTicks)`  
(where PRNG is the same deterministic generator used for piece bag, with a separate stream or a jumped state.)

---

## 4) Wind Direction (Deterministic)
Define wind directions in **world XZ plane**:
- `EAST = (+1,0,0)`
- `WEST = (-1,0,0)`
- `NORTH = (0,0,-1)`
- `SOUTH = (0,0,+1)`

### 4.1 ALTERNATE_EW
- Gust #0: EAST
- Gust #1: WEST
- Gust #2: EAST
- …

### 4.2 RANDOM_SEEDED
- `dir = PRNG.NextChoice([EAST, WEST])` (or include N/S)
- Store in replay implicitly via seed+tick.

---

## 5) Wind Effect (Canonical Rule)
When a gust fires:
1. If no active piece exists, do nothing.
2. Attempt to translate the active piece origin by `pushStrength` cells in wind direction, one cell at a time:
   - For step i in 1..pushStrength:
     - `candidateOrigin = origin + dir`
     - If candidate placement is valid (all voxels in bounds and not colliding with SOLID/WALL/BEDROCK/ICE/DRAIN), accept and continue.
     - If invalid, stop immediately (no further steps).
3. Wind never rotates the piece.

### 5.1 Interaction with water
- Wind may move the active piece through/into `WATER` cells (allowed).
- Collisions with `WATER` are treated as empty for active piece movement (same as general collision rule).

---

## 6) Telegraphing (UX Must-Have)
- Display a wind icon and countdown timer to next gust.
- Display arrow indicating upcoming direction at least **1 second** before gust.
- When gust triggers, show a short screen-space nudge + whoosh SFX.

---

## 7) Tuning Guidance
- Early High-Rise: interval 12s, pushStrength 1, alternating.
- Mid High-Rise: interval 10s, pushStrength 1, alternating.
- Late High-Rise exam: interval 8–10s, pushStrength 1, optional N/S inclusion.

Avoid pushStrength > 1 in MVP.

---

*End of Wind Hazard Spec v0.2*


---

# Part VI — Materials, Anchoring, Stabilize

## 1) Materials (MVP)

Materials are per-voxel properties attached to each locked piece’s voxels.

| Material | Mass (for `STAY_UNDER_WEIGHT`) | Anchoring behavior | Wind interaction (active piece) | Notes |
|---|---:|---|---|---|
| `STANDARD` | 1 | Not anchored | Normal | Default solids |
| `HEAVY` | 2 | Not anchored | Wind push is applied, but **reduced** (see §3) | “High-rise” tuning lever |
| `REINFORCED` | 1 | **Anchors permanently on lock** | Wind push applied normally while falling | Level authors control scarcity |
| `WATER` | 0 | N/A | N/A | Always non-supporting |

### 1.1 Anchored flag (canonical)
A voxel may have `anchored=true`. Anchored voxels:
- never change cell position during **Solid Settling** (including after world rotations),
- still participate in support checks for other voxels/components,
- may be part of merges; a connected component that contains ≥1 anchored voxel is treated as immovable (fall distance = 0).

**How anchored voxels are created**
- `REINFORCED` voxels: become anchored **permanently** at lock time.
- **Stabilize** (ability): temporarily anchors a piece (see §2).

---

## 2) Stabilize ability (MVP)

**Design intent:** give the player a limited, skillful “insurance” tool against catastrophic structural shift during tilts/wind.

### 2.1 Resource model
- Levels can enable Stabilize with a configured number of `stabilizeCharges` (integer).
- If `stabilizeCharges=0`, the action is disabled/hidden.

### 2.2 Activation
- Player presses **Stabilize** (default binding: `V`) while a piece is active.
- If a charge is available, the active piece is marked `stabilizeArmed=true` (UI indicator).
- Arming can be canceled by pressing Stabilize again **before** lock (does not refund charge in MVP; optional refinement).

### 2.3 Effect on lock
When an armed piece locks:
- all its voxels gain `anchored=true` with a **temporary** duration:
  - `stabilizeAnchorRotations = 2` (default) successful world rotations after lock.

After each successful world rotation, decrement the duration counter; when it reaches 0, remove `anchored=true` from those voxels.

**Determinism requirement:** duration countdown is driven only by discrete “successful rotation” events, not wall-clock time.

### 2.4 Interaction rules
- Temporary anchored voxels behave identically to Reinforced anchoring while active.
- When temporary anchoring expires, the structure may shift on subsequent resolves/tilts.
- Reinforced anchoring never expires.

---

## 3) Wind + mass interaction (MVP)

Wind applies a deterministic lateral push to the **active piece** (Part V). For `HEAVY` material pieces:
- interpret wind “push strength” as **cells per gust per unit mass**,
- effective push steps = `floor(pushStrength / pieceMassFactor)` where:
  - `pieceMassFactor = 1` for Standard/Reinforced,
  - `pieceMassFactor = 2` for Heavy.

This preserves predictability while letting Heavy pieces be more wind-resistant.


---

# Part VII — Piece Library

*Engine:* Unity (C#)  
*Scope:* Canonical **piece IDs**, voxel definitions, pivot rules, and orientation generation for MVP.

> This document intentionally standardizes **piece IDs** used in level JSON (e.g., `I3`, `L3`, `T3`, `O2`).  
> Naming is “legacy-friendly” to match earlier templates; the IDs are the source of truth.

---

## 1) Representation

### 1.1 Piece definition (JSON)
Each piece is defined as:
- `pieceId`: string
- `voxels`: array of integer offsets relative to **pivot cell** `(0,0,0)`
- `pivot`: always `(0,0,0)` in v0.2 (the voxel at origin is part of the piece)
- `tags`: optional (e.g., `flat`, `3d`, `tutorial`)

```json
{
  "pieceId": "L3",
  "voxels": [[0,0,0],[1,0,0],[0,1,0]],
  "tags": ["flat"]
}
```

### 1.2 Pivot rule (hard requirement)
- Pivot is always an occupied voxel at `(0,0,0)`.
- All rotations are applied around pivot.
- After rotation, **no re-centering** is performed (offsets remain integers by construction).

This keeps rotation deterministic and simple.

---

## 2) Orientation Generation (Canonical)

### 2.1 Rotation group
- Use the 24 proper rotations of the cube (`SO(3)` with integer matrices).
- Apply each rotation matrix `R` to every voxel offset `v`:
  - `v' = R * v`
- Normalize the oriented shape by translating all voxel offsets so that:
  - the minimum `(x,y,z)` in the oriented set becomes `(0,0,0)` **ONLY for uniqueness testing**, not for placement.
- Deduplicate orientations by comparing normalized voxel sets.

### 2.2 Runtime orientation application
At runtime, when rotating a piece, use the integer matrix and apply directly to offsets:
- Offsets remain relative to pivot.
- If you use “wall-kick,” it shifts the **piece origin cell**, not the voxel offsets.

---

## 3) Rotation Kick Policy (MVP)
To reduce frustration near obstacles, MVP uses a **small deterministic kick set** for *piece rotations* (not world rotations).

When a piece rotation is requested:
1. Apply rotation to offsets.
2. Try to place at same origin.
3. If collision/out-of-bounds, try kicks in order:

Kick list in local axes (world grid axes):
1. `(0,0,0)`
2. `(+1,0,0)`, `(-1,0,0)`
3. `(0,0,+1)`, `(0,0,-1)`
4. `(0,+1,0)`  *(rare; helps near ledges)*
5. `(+1,0,+1)`, `(+1,0,-1)`, `(-1,0,+1)`, `(-1,0,-1)`

First valid placement wins. If none valid, rotation is rejected.

> This is intentionally simpler than modern Tetris SRS.

---

## 4) MVP Piece Set (12 pieces)

### 4.1 Notes
- Pieces are designed to be mostly “2.5D” early (flat), with a few true 3D polycubes to introduce depth later.
- All voxel offsets are small (fit within ~3×3×3 bounding boxes).

### 4.2 Definitions (canonical)

#### O2 — 2×2 flat square (4 voxels)
```json
{ "pieceId":"O2", "voxels":[[0,0,0],[1,0,0],[0,0,1],[1,0,1]], "tags":["flat","tutorial"] }
```

#### I3 — 3-long line (3 voxels)
```json
{ "pieceId":"I3", "voxels":[[0,0,0],[1,0,0],[2,0,0]], "tags":["tutorial"] }
```

#### I4 — 4-long line (4 voxels)
```json
{ "pieceId":"I4", "voxels":[[0,0,0],[1,0,0],[2,0,0],[3,0,0]] }
```

#### L3 — small L (3 voxels)
```json
{ "pieceId":"L3", "voxels":[[0,0,0],[1,0,0],[0,1,0]], "tags":["tutorial"] }
```

#### L4 — classic L (4 voxels)
```json
{ "pieceId":"L4", "voxels":[[0,0,0],[1,0,0],[2,0,0],[0,1,0]] }
```

#### J4 — mirrored L (4 voxels)
```json
{ "pieceId":"J4", "voxels":[[0,0,0],[1,0,0],[2,0,0],[2,1,0]] }
```

#### T3 — T shape (4 voxels)  *(legacy ID)*
```json
{ "pieceId":"T3", "voxels":[[0,0,0],[1,0,0],[2,0,0],[1,1,0]] }
```

#### S4 — S shape (4 voxels)
```json
{ "pieceId":"S4", "voxels":[[0,0,0],[1,0,0],[1,0,1],[2,0,1]] }
```

#### Z4 — Z shape (4 voxels)
```json
{ "pieceId":"Z4", "voxels":[[0,0,1],[1,0,1],[1,0,0],[2,0,0]] }
```

#### U5 — U shape (5 voxels)
```json
{ "pieceId":"U5", "voxels":[[0,0,0],[2,0,0],[0,0,1],[1,0,1],[2,0,1]] }
```

#### P5 — “P” / 2×2 plus tail (5 voxels)
```json
{ "pieceId":"P5", "voxels":[[0,0,0],[1,0,0],[0,1,0],[1,1,0],[2,0,0]] }
```

#### C3D5 — simple 3D corner (5 voxels) *(introduces depth)*
```json
{ "pieceId":"C3D5", "voxels":[[0,0,0],[1,0,0],[0,1,0],[0,0,1],[1,0,1]], "tags":["3d"] }
```

---

## 5) Piece Pack Progression (Recommended)
- **Chapter 1 (Foundation):** O2, I3, L3, T3, I4 (introduce)  
- **Chapter 2 (Floodplain):** add S4, Z4, L4, J4  
- **Chapter 3 (High-Rise):** add U5, P5, C3D5 (and bias heavy materials)

---

## 6) Validation Checklist
- All voxel offsets are integers.
- `(0,0,0)` is included for every piece.
- No duplicate voxel coordinates within a piece.
- Orientation generation deduplicates symmetric shapes.
- Kick set order is fixed and tested.

---

*End of Piece Library v0.2*


---

# Part VIII — 30-Level Campaign Plan

*Engine:* Unity (C#)  
*Scope:* A curated 30-level campaign that introduces mechanics in a controlled sequence.  
*Design intent:* Every level introduces **one new idea**, then mixes it with prior ideas. Difficulty emerges from **system interactions**, not raw speed.

---

## Overview
**Chapter 1 (Lv 1–10): Foundation**  
Teaches core block placement, world rotation, stability/collapse, plateau building, and rotation scarcity.

**Chapter 2 (Lv 11–20): Floodplain**  
Introduces water as a non-supporting fluid, drains, channel engineering, freeze tactics, and flood constraints.

**Chapter 3 (Lv 21–30): High-Rise**  
Introduces weight limits, footprint constraints, wind forcing, stabilization, and mixed hazards.

> Assumed defaults:
> - Snap 90° world rotation.
> - Deterministic resolve after each lock.
> - Water model: binary cells, basin leveling.
> - Campaign boards: mostly 10×10, some 8×8–9×9.
> - Stars: 1=win, 2=efficiency, 3=stability/mastery.

---

# Chapter 1 — Foundation (Lv 1–10)
**Teaching goal:** the player must internalize *gravity as a tool* and *support rules*.

### Level Table
| Lv | Title | New teach beat | Primary objective | Constraints / Star hooks |
|---:|---|---|---|---|
| 1 | First Stack | Basic move/rotate piece, lock | Place 8 pieces without overflow | No world rotation; ★2 ≤10 holes, ★3 no overhangs |
| 2 | First Tilt | World rotation (E/W) | Reach height 6 | Max rotations 3; ★3 shift voxels = 0 |
| 3 | Four Winds | Add N/S rotation | Reach height 8 | Cooldown 1.5s; ★2 ≤2 tilts |
| 4 | Unsupported | Overhangs fall during resolve | Survive 2 tilts with ≤5 voxels lost | Prebuilt overhang demo; ★3 loss ≤1 |
| 5 | Parking Lot | Flatness matters | Build plateau 3×3 at elev=2 | ★2 ≤10 pieces, ★3 plateau survives 2 tilts |
| 6 | Hold It | Introduce Hold | Reach height 10 | Hold on; ★2 hold used ≤2 |
| 7 | Reinforced Intro | Reinforced pieces for bridging | Build bridge gap length 3 | Bag includes reinforced; ★3 bridge survives 2 tilts |
| 8 | Tilt Budget | Rotation scarcity | Reach height 12 | Tilt budget 6; ★3 finish with ≥2 left |
| 9 | Cavity Bonus | Enclosure (air pocket) | Enclose 1 cavity | ★2 cavity + height 10, ★3 ≤12 pieces |
| 10 | Foundation Exam | Combine concepts | Height 14 AND plateau 3×3 | Tilt budget 8; ★3 shift voxels ≤6 |

### Notes for Chapter 1
- **Lv1–3:** rotation introduced with low penalty.
- **Lv4–6:** collapse becomes the “why” of the game.
- **Lv7–10:** scarcity + multi-objectives create planning.

---

# Chapter 2 — Floodplain (Lv 11–20)
**Teaching goal:** water is hostile, predictable, and controllable through engineering.

### Level Table
| Lv | Title | New teach beat | Primary objective | Constraints / Star hooks |
|---:|---|---|---|---|
| 11 | First Basin | Water exists; cannot support | Place 6 pieces; end stable | Starter pool under shelf; ★3 shift voxels 0 |
| 12 | Meet the Drain | Drain tile behavior | Drain 10 units | One drain preplaced; ★2 ≤12 pieces |
| 13 | Dig a Channel | Guide flow with terrain | Drain 18 units | Tilt allowed E/W; ★3 ≤4 tilts |
| 14 | Two Basins | Basins connect + level | Drain 20 units | Must connect basins; ★3 no water above elev=1 |
| 15 | Don’t Build on Water | Support rule enforcement | Reach height 10 | ★3 “no resting on water” at end-of-resolve |
| 16 | Freeze 101 | Freeze introduces temporary support | Drain 20 units | Freeze charges 1; ★3 freeze unused |
| 17 | Temporary Bridge | Freeze as tactical bridge | Create crossing then drain | Fixed piece seq; ★3 ≤1 tilt |
| 18 | Drain Placement | Player places drain block | Drain 25 units | Drain placement charges 1; ★2 ≤18 pieces |
| 19 | Forbidden Zone | Flood constraint | Drain 30 units | Lose if water elev≥3; ★3 ≤6 tilts |
| 20 | Floodplain Exam | Combine drain + freeze + planning | Drain 40 AND height 12 | Tilt budget 10; ★3 loss ≤8 & ≤22 pieces |

### Notes for Chapter 2
- Emphasize: **water is deterministic** and will always settle to lowest reachable cells.
- Drain scope visualization is essential (highlight radius).
- Freeze levels should teach “temporary support” and “controlled timing,” not spam.

---

# Chapter 3 — High-Rise (Lv 21–30)
**Teaching goal:** vertical engineering under constraints and external forcing.

### Level Table
| Lv | Title | New teach beat | Primary objective | Constraints / Star hooks |
|---:|---|---|---|---|
| 21 | Weight Limit | Total mass constraint | Reach height 18 | Max weight 250; ★3 finish ≤220 |
| 22 | Heavy Pieces | Heavy material tradeoff | Reach height 20 | Bag biased heavy; ★3 ≤1 major collapse |
| 23 | Skyline Footprint | Small base footprint | Reach height 18 | Field 8×8; ★3 plateau 3×3 also |
| 24 | First Gust | Wind forcing begins | Reach height 20 | Wind interval 12s; ★3 ≤4 tilts |
| 25 | Stabilize | Stabilize ability intro | Reach height 22 | Stabilize charges 1; ★3 no stabilize used |
| 26 | Cantilever | Overhang design under wind | Build overhang length 4 survives 2 tilts | Max weight 280; ★3 ≤14 pieces |
| 27 | Tight Budget | Scarce rotation under pressure | Reach height 24 | Tilt budget 6; ★3 finish with ≥2 left |
| 28 | Alternating Wind | Predictable wind patterns | Reach height 26 | Wind alternates E/W; ★3 shift voxels 0 |
| 29 | Wet Foundation | Water returns as foundation hazard | Reach height 26 under weight | Small base puddles; ★3 no resting on water at end |
| 30 | High-Rise Exam | Full mastery | Height 30 AND weight ≤400 | Wind stronger; tilt budget 10; ★3 ≤26 pieces & loss ≤10 |

### Notes for Chapter 3
- Wind must be **predictable** (patterned) and signposted (arrow + countdown).
- Stabilize is a clutch tool; design levels so it feels “earned,” not mandatory.

---

## Campaign Progression & Unlocks
### Unlock cadence (recommended)
- After Lv3: rotation N/S enabled generally (unless level restricts).
- After Lv6: Hold enabled by default (levels can disable).
- After Lv7: Reinforced material appears in bag.
- After Lv12: Drain UI appears; drain tile becomes common.
- After Lv16: Freeze introduced (limited charges).
- After Lv21: Weight UI and heavy material become common.
- After Lv24: Wind icon + timer UI appears.
- After Lv25: Stabilize introduced.

---

## Difficulty Tuning Checklist (Per Level)
- **Clarity:** is the new teach beat visible in the first 20 seconds?
- **Constraint fairness:** can a reasonable player recover after 1 mistake?
- **Rotation pressure:** does the level force rotation choices, not spam?
- **Water pressure:** does the level require basin/channel reasoning?
- **Stability:** do collapses have obvious causes (unsupported / resting on water)?
- **Stars:** ★2 requires efficiency, ★3 requires mastery (stability/constraints).

---

## Deliverable Notes (Implementation)
This plan is intended to be authored into actual JSON levels using the level template:
- Tutorial levels should use `bag.type = FIXED_SEQUENCE`.
- All levels should specify `meta.seed` for reproducibility.
- Each chapter should include 1–2 “exam” levels that combine prior concepts.

---

*End of 30-Level Campaign Plan v0.2*
