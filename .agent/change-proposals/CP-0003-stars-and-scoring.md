# Change Proposal: Stars/score system (campaign + HUD)

## Problem statement
The campaign plan references star conditions and the Unity HUD backlog item includes "score/stars", but there is no canonical, machine-verifiable representation of star criteria.

Without this, autonomous content authoring and CI verification cannot validate star hooks or display consistent HUD info.

## Current behavior / architecture
- Objectives exist (win/lose) but no star/score model is defined in schemas or Core.
- Content pack star hooks include conditions that are not yet part of Core metrics (e.g., holes, no overhangs, major collapse).

## Proposed change
Define an MVP star model that is:
- deterministic
- based only on measurable Core metrics
- representable in level JSON and enforced by validator + CI

Recommended MVP support set:
- Star 1: win primary objectives
- Star 2/3: optional constraints expressible via a small set of typed conditions

### MVP semantics (accepted)
**Stars**
- Star 1: all primary objectives completed.
- Star 2/3: AND-list of typed conditions (if present).

**Supported star condition types (v0.2.3+):**
- `MAX_PIECES_USED` (params: `count`)
- `MAX_ROTATIONS_USED` (params: `count`)
- `MAX_SHIFT_VOXELS_TOTAL` (params: `count`)
- `MAX_LOST_VOXELS_TOTAL` (params: `count`)

**Score (optional, deterministic)**
- If a level declares `score.enabled=true`, compute:
  - `score = perPiece * PiecesUsed + perWaterRemoved * WaterRemovedTotal - penaltyShiftVoxel * ShiftVoxelsTotal - penaltyLostVoxel * LostVoxelsTotal - penaltyRotation * RotationsExecuted`
- All weights are integers; if `score` is omitted or `enabled=false`, score is not computed.

## Alternatives considered
1) Keep stars as prose-only (no machine enforcement) - low autonomy, inconsistent HUD.
2) Implement full set from Content Pack (holes/no-overhangs/major collapse) - higher spec and implementation cost.
3) MVP typed-star conditions + adjust content pack hooks to fit - best autonomy/effort tradeoff.

## Impact analysis
- API/contracts impact: new level JSON section for stars and score.
- Save data impact: none initially.
- Tooling/CI impact: validator + solution runner can assert star achievement.
- Risk: content pack may need minor edits to align with supported conditions.

## Migration / rollout plan
- Add a new spec section + schema v0.2.3 defining supported star condition types and score weights.
- Add backlog items to implement metrics and evaluation before Unity HUD depends on it.

## Test plan
- Unit tests for each star condition type.
- Integration test: solution runner asserts expected star count for at least one level.

## Decision
- Status: Accepted
- Rationale: improves autonomous content authoring and prevents UI/design drift with minimal implementation risk.

