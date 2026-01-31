# Floodline — Core Game Design v0.2
*Location:* `/docs/GDD_Core_v0_2.md`  
*Date:* 2026-01-31  
*Target platform:* PC (Windows)  
*Engine:* Unity (C#)  
*Status:* MVP-focused

## How to read this document set
This repository splits the former unified GDD into:
- **Core GDD (this file):** player-facing design, modes, UX/readability, art/audio direction, production framing.
- **On-demand specs:** open only when a backlog item’s `requirementRef` links to them:
  - Simulation Rules Bible: [`specs/Simulation_Rules_v0_2.md`](specs/Simulation_Rules_v0_2.md)
  - Input & Feel: [`specs/Input_Feel_v0_2.md`](specs/Input_Feel_v0_2.md)
  - Water Algorithm: [`specs/Water_Algorithm_v0_2.md`](specs/Water_Algorithm_v0_2.md)
  - Content Pack (pieces + campaign): [`content/Content_Pack_v0_2.md`](content/Content_Pack_v0_2.md)

**Source-of-truth rule:** Simulation outcomes are defined by the *Simulation Rules* + *Water Algorithm* specs (not Unity physics).

---

## Player-Facing Design

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

(Design reference for teaching beats: [`content/Content_Pack_v0_2.md#campaign-plan-30`](content/Content_Pack_v0_2.md#campaign-plan-30).)

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

## 13. Controls & Bindings

### Controls & bindings
Controls are defined once in **Input & Feel Spec v0.2**: [`specs/Input_Feel_v0_2.md#input-defaults-pc`](specs/Input_Feel_v0_2.md#input-defaults-pc). Rebinding is supported via Unity Input System.

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
- Rules & Algorithms: [`specs/Simulation_Rules_v0_2.md`](specs/Simulation_Rules_v0_2.md)
- Level JSON template (schema + examples): (to be introduced in M4)
- 30-level campaign plan: [`content/Content_Pack_v0_2.md#campaign-plan-30`](content/Content_Pack_v0_2.md#campaign-plan-30)

---

*End of Part I — Player-Facing Design*


---

