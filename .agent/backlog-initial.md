# Floodline — Initial Backlog (Autonomy-First)

**Ordering:** matches the autonomy-maximizing implementation sequence:
1) Core sim library + CLI runner
2) Golden tests (resolve + water + objectives)
3) Replay format + determinism hash
4) Level validator + campaign validation
5) Unity client shell + camera + input feel

**Task format:** each task must use `/.agent/task-template.md` and meet `/.agent/definition-of-done.md`.

---

## Epic E0 — Repo Baseline (strict .NET + central packages)

### FL-0001 — Create solution + project skeletons
**Goal:** establish clean module boundaries before code exists.

**Deliverables**
- `Floodline.Core` (multi-target `netstandard2.1;net8.0`)
- `Floodline.Core.Tests` (`net8.0`)
- `Floodline.Cli` (`net8.0`)
- `Floodline.Levels` (optional: shared level schema/helpers)

**Validation**
- `dotnet restore`
- `dotnet build -c Release`
- `dotnet test -c Release`

---

### FL-0002 — Enable Central Package Management + locked restore
**Goal:** deterministic dependencies and easy upgrades.

**Deliverables**
- `Directory.Packages.props` with pinned versions
- `Directory.Build.props` sets:
  - `ManagePackageVersionsCentrally=true`
  - lock file enabled and CI uses locked mode

**Validation**
- `dotnet restore --use-lock-file`
- `dotnet restore --locked-mode`
- `dotnet build -c Release`

---

### FL-0003 — Strict analyzers + formatting gates (warnings-as-errors)
**Goal:** maximal static correctness to support autonomous refactors.

**Deliverables**
- `.editorconfig` with strict rules (nullable, style, analyzers)
- `TreatWarningsAsErrors=true` + `EnforceCodeStyleInBuild=true`
- Analyzers packages pinned centrally (e.g., .NET analyzers + StyleCop + one quality analyzer)

**Validation**
- `dotnet build -c Release`
- `dotnet format --verify-no-changes`
- `dotnet test -c Release`

---

## Epic E1 — Core Sim Library + CLI Runner (headless)

### FL-0101 — Core primitives: coords, gravity, ordering
**Goal:** implement the deterministic ordering foundation.

**Acceptance criteria**
- `worldHeight(c)=y`
- `gravElev(c)=dot(c,u)` with `u=-g`
- `tieCoord(c)` computed via the gravity-specific right-axis table
- Canonical ordering helper: compares by `(gravElev, tieCoord)`

**Validation**
- `dotnet test -c Release` (unit tests for ordering + table mapping)

---

### FL-0102 — Grid model + occupancy types
**Goal:** represent the full cell type set used by rules.

**Acceptance criteria**
- Exact occupancy enum set includes: EMPTY, SOLID(material), WALL/BEDROCK, WATER, ICE, POROUS, DRAIN
- Single occupancy per cell enforced

**Validation**
- `dotnet test -c Release`

---

### FL-0103 — Deterministic PRNG
**Goal:** single seeded PRNG for gameplay randomness.

**Acceptance criteria**
- Deterministic across runtimes
- No use of UnityEngine.Random in Core

**Validation**
- `dotnet test -c Release` (golden sequence test for PRNG outputs)

---

### FL-0104 — Piece library + rotations + kick rules
**Goal:** implement canonical piece IDs and deterministic rotation behavior.

**Acceptance criteria**
- Piece IDs match GDD list (O2, I3, I4, L3, L4, J4, T3, S4, Z4, U5, P5, C3D5)
- Kick list order is fixed and tested

**Validation**
- `dotnet test -c Release`

---

### FL-0105 — Tick loop + input application order + locking
**Goal:** implement the per-tick action pipeline and lock behavior.

**Acceptance criteria**
- Input order: world rotate -> stabilize -> rotate -> move -> soft drop -> hard drop
- Lock delay = 12 ticks; max lock resets = 4; hard drop locks immediately

**Validation**
- `dotnet test -c Release` (tick-by-tick unit tests)

---

### FL-0106 — Resolve Phase skeleton + invariants
**Goal:** enforce canonical resolve ordering (even with stub solvers at first).

**Acceptance criteria**
- Resolve order exactly matches spec
- Loop cap implemented (2 solid-water cycles)

**Validation**
- `dotnet test -c Release`

---

### FL-0107 — Solid settling solver (components, deterministic ordering)
**Goal:** implement stability and collapse in integers.

**Acceptance criteria**
- Support rules: WATER does not support
- Component-based drop with deterministic component ordering
- Displacement events emitted when solids enter water

**Validation**
- `dotnet test -c Release`

---

### FL-0108 — Water equilibrium solver + drains + freeze
**Goal:** implement the minimax-Dijkstra water settle and modifiers.

**Acceptance criteria**
- Passability/occupiability rules match spec (POROUS passable but not occupiable)
- `req[c]` minimax path computed deterministically
- Fill order uses `(req, gravElev, tieCoord)`
- Drains and freeze/thaw behave exactly as specified

**Validation**
- `dotnet test -c Release`

---

### FL-0109 — Objective evaluator + fail states (MVP set)
**Goal:** compute win/lose and progression metrics.

**Acceptance criteria**
- Implement objectives: DRAIN_WATER, REACH_HEIGHT, BUILD_PLATEAU, STAY_UNDER_WEIGHT, SURVIVE_ROTATIONS
- Implement fail: OVERFLOW, WEIGHT_EXCEEDED, WATER_FORBIDDEN, (optional) no-resting-on-water

**Validation**
- `dotnet test -c Release`

---

### FL-0110 — CLI runner (level + scripted inputs)
**Goal:** runnable headless sim for agent-driven testing.

**Acceptance criteria**
- Loads a level JSON + seed
- Loads scripted inputs (per tick)
- Runs until win/lose or max ticks
- Prints summary metrics

**Validation**
- `dotnet run --project Floodline.Cli -- --help`
- `dotnet run --project Floodline.Cli -- --level <path> --inputs <path>`

---

## Epic E2 — Golden tests (Resolve + Water + Objectives)

### FL-0201 — Water golden tests (6 canonical cases)
**Goal:** lock water behavior.

**Acceptance criteria**
- Golden tests cover: single basin, ridge spill, displacement, drain, freeze, gravity change
- Each test asserts exact final water set (or a stable hash)

**Validation**
- `dotnet test -c Release`

---

### FL-0202 — Solid settling golden tests
**Goal:** lock collapse behavior and ordering.

**Acceptance criteria**
- Components drop deterministically
- WATER never supports (explicit regression)
- Shift/Lost metrics computed consistently

**Validation**
- `dotnet test -c Release`

---

### FL-0203 — Objective golden tests
**Goal:** lock objective semantics and fail checks.

**Acceptance criteria**
- Each objective has at least:
  - one positive case
  - one negative/edge case

**Validation**
- `dotnet test -c Release`

---

## Epic E3 — Replay format + determinism hash

### FL-0301 — Define replay format + level hashing
**Goal:** replays are stable artifacts.

**Acceptance criteria**
- Replay header includes: replayVersion, rulesVersion, levelId+levelHash, seed, tickRate
- Canonical level hash computation implemented

**Validation**
- `dotnet test -c Release`

---

### FL-0302 — Determinism hash v0.2.0
**Goal:** stable “same outcome” proof.

**Acceptance criteria**
- Hash includes grid + gravity + PRNG + objective counters (+ active piece state if mid-run)
- Hash stable across machines for identical inputs

**Validation**
- `dotnet test -c Release` (golden hash fixtures)

---

### FL-0303 — CLI record/playback
**Goal:** record and replay are identical.

**Acceptance criteria**
- `--record` produces a replay file
- `--play` reproduces the same determinism hash

**Validation**
- CLI smoke tests in CI + unit tests

---

## Epic E4 — Level schema + validator + campaign validation

### FL-0401 — Level JSON schema v0.2.0
**Goal:** validate content before runtime.

**Acceptance criteria**
- Schema stored in repo
- CI validates all levels

**Validation**
- `dotnet run --project tools/LevelValidator -- --schema <schema> --level <level>`

---

### FL-0402 — Semantic validator rules
**Goal:** prevent authoring errors.

**Acceptance criteria**
- Checks: piece IDs, board dims, allowed gravity set, objective fields, height/fail constraints

**Validation**
- `dotnet test -c Release`
- validator run on sample levels

---

### FL-0403 — Campaign validation pipeline
**Goal:** validate all 30 campaign levels.

**Acceptance criteria**
- Validator runs on all campaign levels, produces report, fails CI on errors

**Validation**
- `dotnet run --project tools/LevelValidator -- --campaign ./Content/Campaign`

---

## Epic E5 — Unity client shell (after everything above is green)

### FL-0501 — Unity project bootstrap + core integration
**Goal:** Unity consumes Core without altering determinism.

**Acceptance criteria**
- Core referenced via asmdef / DLL
- Unity layer contains no gameplay logic beyond orchestration/visualization

**Validation**
- Unity batchmode EditMode tests
- CLI vs Unity determinism parity on a replay
