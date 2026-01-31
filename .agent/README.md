# Agent Operating Pack

This folder defines the "constitution" and execution loop for coding AI agents working in this repo.

## Golden rule
The agent must prefer **verifiable artifacts** over prose:

**contracts / schemas > automated tests (incl. golden) > code > docs text**

## Canonical backlog
**Source of truth:** `/.agent/backlog.json`  
`/.agent/backlog.md` is a human-readable view; the agent MUST update JSON.

**Status enum:** `New` → `InProgress` → `Done`  
**WIP limit:** 1 (at most one `InProgress` at a time; `0` allowed only between items).
- If any eligible `New` item exists (all `dependsOn` Done), immediately set NEXT to `InProgress`
  before making other repo changes.

## Execution loop (per backlog item)

### A) Plan window (allowed backlog edits)
Before starting a new item, the agent MUST:
1) Read `/.agent/backlog.json`
2) Identify:
   - Done items
   - Current item (the single InProgress item, if any)
   - Next item (lowest ID New item whose dependencies are Done)
3) Confirm the next item is **appropriately sized**. If it is too large, the agent MUST split it into smaller items (see `rules.md`).

During this window the agent MAY:
- split an item into smaller items,
- add an enabler task (CI, scripts, schema tools, fixtures),
- add a bugfix task discovered by tests,
- add a Change Proposal task if design/architecture must change.

Outside this window: do not re-plan.

### B) Start work (mandatory)
4) Set the chosen item `status` to **InProgress** and set `startedAt` (ISO 8601 UTC).
   - If any other item is InProgress, STOP and resolve the inconsistency first.
   - Immediately commit this backlog-only change (no code yet) to keep the working tree clean.
     - Commit message: `FL-XXXX: start <short title>`

### C) Implement + verify
5) Restate the goal and cite the requirement (GDD section / rule / test).
6) List touched artifacts (contracts/tests/docs/code).
7) Implement minimal diff.
8) Run the validation commands listed in the backlog item.
9) Update docs/ADR if behavior or architecture changed.
10) Commit with evidence (commands + results).

### D) Finish
11) If DoD is satisfied, set `status` to **Done** and set `doneAt` (ISO 8601 UTC).
12) Append evidence to the backlog item (`evidence.commandsRun`).
13) Move to the next eligible **New** item.

## When the agent is blocked
- Do NOT guess silently.
- Keep the item as **InProgress**.
- Record a Blocking Note in `evidence.notes`:
  - what failed
  - 2–3 options
  - recommended option + why
  - what info is needed

## Key references
- design: `/docs/GDD_v0_2.md`
- context: `/.agent/context.md`
- milestones: `/.agent/milestones.md`
- backlog: `/.agent/backlog.json` (canonical), `/.agent/backlog.md` (view)
- contract policy: `/.agent/contract-policy.md`
- rules: `/.agent/rules.md`
- roles: `/.agent/roles.md`
- DoD: `/.agent/definition-of-done.md`
- commands: `/.agent/commands-windows.md`
- change proposals: `/.agent/change-proposal-template.md`
- CI gates: `/.agent/ci-gates.md`
