# Floodline

Floodline is an in-development 3D voxel puzzle game where gravity is your main tool: stack blocks, rotate the world, and outsmart the flood.

Instead of merely spinning a piece, you **tilt the entire board** in crisp 90-degree snaps. Every rotation rewrites what "supported" means, turning safe platforms into freefalls, exposing new routes, and enabling satisfying, intentional chain reactions.

Your opponent is **water**: it hunts the lowest reachable spaces, levels out into readable basins, and **never supports blocks**. Build basins and channels, then deploy tools like **drains** and **freeze/ice** to stay one step ahead as later levels add hazards like **wind** and stabilizing anchors.

Key features:
- World rotation as a primary mechanic (snap 90-degree gravity shifts).
- Deterministic, puzzle-fair simulation (same inputs -> same outcome).
- Discrete water system: basin leveling, spillover, and displacement (no physics fuzz).
- A campaign focused on teachable mechanics and engineering-style solutions.

## Design & Specs

1. [`docs/GDD_Core_v0_2.md`](docs/GDD_Core_v0_2.md) - Core GDD (player-facing design)
2. [`docs/specs/Input_Feel_v0_2.md`](docs/specs/Input_Feel_v0_2.md) - Input sampling, lock rules, and feel constraints
3. [`docs/specs/Simulation_Rules_v0_2.md`](docs/specs/Simulation_Rules_v0_2.md) - Determinism + resolve order (simulation source of truth)
4. [`docs/specs/Water_Algorithm_v0_2.md`](docs/specs/Water_Algorithm_v0_2.md) - Deterministic water equilibrium solver + drains/freeze
5. [`docs/content/Content_Pack_v0_2.md`](docs/content/Content_Pack_v0_2.md) - Canonical pieces + campaign plan
6. [`.agent/contract-policy.md`](.agent/contract-policy.md) - Contract/versioning policy (schemas, replays, determinism)
7. [`.agent/change-proposals/`](.agent/change-proposals/) - Design/contract change proposals (CP-*.md)
8. [`.agent/backlog.json`](.agent/backlog.json) - Project backlog (work items and evidence)

## Repo Structure

- `.agent/` - change proposals, contract policy, and backlog
- `docs/` - game design + simulation specs
- `schemas/` - level/campaign JSON schemas (authoritative for validation)
- `levels/` - level fixtures (and later: campaign + solutions/replays)
- `scripts/` - canonical validation entrypoints (preflight/ci)
- `src/Floodline.Core/` - deterministic simulation (Unity-free)
- `src/Floodline.Cli/` - headless runner + validators
- `tests/Floodline.Core.Tests/` - Core unit + golden tests
- `tests/Floodline.Cli.Tests/` - CLI smoke tests (validation, record/replay)

Unity client is not checked in yet (planned under milestone M5).

## Build & Test

Preflight:

```powershell
powershell -File ./scripts/preflight.ps1
```

CI (local equivalent of what the repo expects in GitHub Actions):

```powershell
# early repo (before lock files exist)
powershell -File ./scripts/ci.ps1 -Scope Always -LockedRestore:$false

# M0: generate lock files (then verify locked restore)
powershell -File ./scripts/ci.ps1 -Scope M0 -UseLockFile

# M0+: include formatting once introduced
powershell -File ./scripts/ci.ps1 -Scope M0 -IncludeFormat

# M1+: default validations (restore/build/test)
powershell -File ./scripts/ci.ps1 -Scope M1
```

Note: `-Golden`, `-Replay`, `-ValidateLevels`, `-CampaignSolutions`, `-Unity` switches exist but some are placeholders until their corresponding tooling lands.

## Run The CLI

Run a level with an input script:

```powershell
dotnet run --project .\src\Floodline.Cli\Floodline.Cli.csproj -- --level .\levels\minimal_level.json --inputs .\levels\minimal_inputs.txt
```

Validate a level file:

```powershell
dotnet run --project .\src\Floodline.Cli\Floodline.Cli.csproj -- --level .\levels\minimal_level.json --validate
```
