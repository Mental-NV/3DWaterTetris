# Floodline — Milestones (Graph Plan)

**Source of truth:** `/docs/GDD_v0_2.md` (v0.2).  
This plan is aligned to the autonomy-first implementation sequence.

---

## 0) Dependency graph (high level)

```
[M0 Repo Baseline]
        |
        v
[M1 Core Sim + CLI Runner]
        |
        v
[M2 Golden Tests: Resolve + Water + Objectives]
        |
        v
[M3 Replay Format + Determinism Hash]
        |
        v
[M4 Level Schema + Validator + Campaign Validation]
        |
        v
[M5 Unity Client Shell: UI loop + camera + input feel]
```

**Rule:** A milestone is “done” only when its exit criteria are met and CI gates pass.

---

## M0 — Repo Baseline (strict .NET + CI)
**Goal:** make the repo deterministic, lint-gated, and agent-friendly before gameplay code starts.

**Deliverables**
- Solution structure + project skeletons
- Central Package Management + locked restore
- Strict analyzers, formatting, warnings-as-errors
- CI gates (Windows)

**Exit criteria**
- `dotnet restore` uses lock file and passes in locked mode
- `dotnet build -c Release` passes with zero warnings
- `dotnet test -c Release` passes
- `dotnet format --verify-no-changes` passes

---

## M1 — Core Sim Library + CLI Runner (no visuals)
**Goal:** headless, deterministic core that matches the rules bible.

**Deliverables**
- Core primitives: `Int3`, directions, gravity mapping, `worldHeight` vs `gravElev`, tie-break basis
- Grid state model with canonical occupancy types
- Piece library + rotations + deterministic kick logic
- Fixed tick loop + input application order + lock behavior
- Resolve Phase pipeline (merge -> settle solids -> settle water -> drains -> objectives)
- CLI runner: load level JSON + input script, run sim, print summary

**Exit criteria**
- CLI can run a minimal level end-to-end and produce a final state (even before golden tests)
- Core remains Unity-free (no UnityEngine references)

---

## M2 — Golden Tests (Resolve + Water + Objectives)
**Goal:** pin deterministic behavior to golden outputs to prevent drift.

**Deliverables**
- Water golden tests: basin fill, two-basin ridge spill, displacement, drain, freeze/thaw, gravity change
- Solid settling golden tests: support rules (water doesn’t support), component ordering, drop distance
- Objective golden tests: DRAIN_WATER, REACH_HEIGHT, BUILD_PLATEAU, STAY_UNDER_WEIGHT, SURVIVE_ROTATIONS
- Determinism fuzz test: repeated runs compare hashes across runs for same seed+inputs

**Exit criteria**
- Golden suite passes on Windows in Release
- At least one negative test per subsystem (water/solids/objectives)

---

## M3 — Replay Format + Determinism Hash
**Goal:** make deterministic reproduction an explicit contract (replays) and mechanically checkable (hash).

**Deliverables**
- Versioned replay format (header + per-tick inputs)
- Level hashing strategy (content hash of canonical level JSON)
- Determinism hash for:
  - end-of-run
  - (optionally) after each Resolve / tick for debugging
- CLI:
  - `--record-replay out.replay.json`
  - `--play-replay in.replay.json`
  - output determinism hash and compare to expected

**Exit criteria**
- A recorded replay replays to identical determinism hash in CI
- Replay versioning rules documented in `contract-policy.md`

---

## M4 — Level Schema + Validator + Campaign Validation
**Goal:** make content creation safe, validated, and deterministic.

**Deliverables**
- Versioned Level JSON schema (stored in repo)
- Level validator tool:
  - schema validation
  - semantic validation (piece IDs exist, objective fields coherent, gravity set legal, height rules consistent)
- Campaign validation:
  - validates all 30 campaign level JSONs
  - produces a report (pass/fail + list of issues)

**Exit criteria**
- Validator passes for all campaign levels in CI
- Validator errors are actionable (include file + JSON path + rule ID)

---

## M5 — Unity Client Shell (UI loop + camera + input feel)
**Goal:** wrap the already-correct sim with a readable, responsive Unity client.

**Deliverables**
- Unity project with strict separation:
  - Core packaged as DLL / asmdef reference
  - Unity layer for rendering/input/UI
- Visual loop:
  - render voxel grid
  - animate resolve (without changing outcomes)
- Input & camera:
  - Unity Input System bindings per spec
  - camera-stable horizon + 4 snap views
- Debug overlays:
  - gravity arrow + label
  - unsupported highlight
  - water `req[c]` heatmap
  - component IDs

**Exit criteria**
- A fixed seed + replay produces identical determinism hash between:
  - CLI runner
  - Unity client (in headless test mode if possible)
- Basic playability for early campaign levels
