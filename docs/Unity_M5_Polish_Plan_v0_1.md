# M5 Unity Polish & Testing Plan

## Overview
M5 has completed all major feature implementation (camera, HUD, audio/music, materials/lighting). This polish pass focuses on:
- **Performance**: GridRenderer optimization, object pooling, profiling
- **Visuals**: Material color refinement, lighting tweaks, UI layout
- **Input/Feel**: Comprehensive input test scenarios, DAS/ARR validation
- **Audio**: Volume balancing, cue timing validation
- **Documentation**: Scene setup guide, performance characteristics, known limitations

## 1. Performance Optimization

### 1.1 GridRenderer Object Pooling
**Issue**: Current implementation destroys and recreates every voxel cube every frame.
- **Symptom**: High GC allocation, frame stutters on large grids (30×30×30)
- **Solution**: Implement object pool for cube meshes; reuse transforms instead of destroying
- **Expected Benefit**: 10–50% frame time reduction on 30-level campaign scenarios
- **Implementation** (FU-0501):
  - VoxelPool: LRU cache of up to 500 cube GameObjects
  - Track active/inactive positions per frame
  - Reuse inactive cubes on next frame

### 1.2 Material Batch Optimization
**Issue**: Each voxel may reference a unique material instance.
- **Solution**: Use Material.SetColor() on a shared material template per type, not material swaps
- **Expected Benefit**: Fewer draw calls, better batching

### 1.3 Profiling Baseline
**Validation**:
- Run L01 (minimal) → L10 (foundation exam) with profiler
- Record frame time, GC allocation, draw calls
- Document baseline before/after pooling

## 2. Visual Polish

### 2.1 Material Colors Refinement
**Current Palette** (RGB):
- STANDARD: 220, 220, 220 (very light gray)
- HEAVY: 64, 64, 64 (very dark, hard to see against bedrock)
- REINFORCED: 180, 180, 180 (too similar to standard)
- BEDROCK: 50, 50, 50 (very dark)
- WATER: 100, 200, 255, 0.5 alpha (okay)
- ICE: 200, 240, 255 (good)
- DRAIN: 255, 165, 0 (good orange)

**Improvements**:
- STANDARD → RGB(210, 210, 200) - slightly warmer off-white
- HEAVY → RGB(100, 30, 30) - dark red tint to suggest weight/hazard
- REINFORCED → RGB(180, 180, 255) - subtle blue tint to suggest metal/tech
- BEDROCK → RGB(80, 70, 70) - slightly brown/warm to contrast with STANDARD
- WATER → RGB(100, 180, 255) with 0.6 alpha - brighter, more saturated
- Result: Better visual distinction while remaining readable

**Implementation** (FU-0502):
- Add Material Color Palette.md guide with RGB hex values
- Update MaterialPalette UI to show color swatches
- Test on L01-L05 to ensure readability

### 2.2 Lighting Fine-Tuning
**Current Setup**: Fixed 45° NE light, 0.75 intensity, soft shadows
**Tweaks**:
- Optional: Reduce shadow softness for crisper edges on small voxels
- Optional: Add very subtle rim light (0.1 intensity, white, behind camera)
- Test: Compare with/without on L01

**Implementation** (FU-0503):
- Document lighting settings in LightingSetup.cs comments
- Add preset toggles (e.g., "Realistic" vs "Stylized")

### 2.3 HUD Layout Refinement
**Current State**: Gravity arrow, objectives, next/hold, wind timer, score/stars
**Improvements**:
- Add background panel on HUD root to unify visual space (dark semi-transparent)
- Increase font sizes on key readouts (rotation budget, wind countdown)
- Add subtle animations: number tickers (score), glow on star earned
- Test: Verify HUD readable at 1080p + 4K resolutions

**Implementation** (FU-0504):
- Update HUDManager to add canvas background
- Refine ObjectivesDisplay spacing/alignment

## 3. Input & Feel Testing

### 3.1 Comprehensive Input Scenarios
**Test Cases**:
1. **DAS Validation** (L01 flat grid): Hold right, verify 1 step initially, then repeats every 2 ticks
2. **ARR Validation** (L01): Rotate held, verify independent ARR (initial delay, then repeat)
3. **Direction Changes** (L02 narrow): Left→Right→Left in quick succession; verify buffer clears
4. **Soft Drop** (L03): Soft drop vs normal gravity; measure frame advancement
5. **Hard Drop** (L04): Verify immediate lock, no delay
6. **Hold Swap** (L06): Hold enable/disable per level; verify swap prevents re-hold
7. **Rotation + Hold** (L07): Rotate then hold; verify hold during rotation
8. **Wind Interaction** (L24): Wind push during falling; verify deterministic path

**Fixture Files**:
- `/tests/Floodline.Client.Tests/InputScenarios/` → JSON files with input sequences
- Each scenario includes expected tick outcomes

**Implementation** (FU-0505):
- Create InputScenarioTests.cs with 8 parameterized test cases
- Use scripted input source + replay parity check

### 3.2 Round-Trip Determinism (Input→Sim→Output)
**Test**: Record input sequence via InputManager → Tick simulation → Compare vs CLI replay
- Runs on L01, L06 (hold), L24 (wind)
- Fails if any command differs

**Implementation** (FU-0506):
- PlayMode test: open L01 replay, feed commands via scripted input, recompute hash

## 4. Audio & Music Validation

### 4.1 SFX Volume Levels
**Current**: All audiosources at default 1.0 volume
**Adjustments**:
- SFX base: 0.7 (not overwhelming)
- Wind whoosh: 0.5 (telegraphic, not startling)
- Lock: 0.6 (satisfying click)
- Collapse: 0.8 (dramatic)

**Implementation** (FU-0507):
- Add AudioManager volume knobs (serialized fields)
- Document recommended levels in guide

### 4.2 Music State Timing
**Test**: Verify MusicController transitions at correct danger thresholds
- Simulate danger ramp via scripted metrics
- Assert state changes occur at exact thresholds (Calm→Tension at 0.3, etc.)

**Implementation** (FU-0508):
- MusicControllerIntegrationTests.cs: Tick simulation to specific danger levels

## 5. Documentation & Guidelines

### 5.1 M5 Scene Setup Best Practices
**Document** (`/docs/Unity_M5_Scene_Setup_v0_1.md`):
- GameLoader configuration checklist
- Camera preset hotkeys (F1-F4)
- Expected folder structure
- Level loading from JSON
- Headless/batchmode execution for CI

### 5.2 Performance Characteristics
**Document** (`/docs/Unity_M5_Performance_v0_1.md`):
- GridRenderer: ~50K cubes @ 30fps (L30 with full grid)
- Frame budget breakdown (render 60%, logic 30%, GC 10%)
- Known GC spikes (voxel destruction every frame → pool fix)
- Optimization roadmap (GPU instancing, LOD, mesh baking)

### 5.3 Known Limitations & Future Work
**Document** (`/docs/Unity_M5_Limitations_v0_1.md`):
- Audio: Placeholder clips (no recorded voiceovers)
- Materials: No custom shaders (using Standard shader)
- HUD: No animation on state changes (static text)
- Wind: Placeholder 60-tick gust schedule (not synced to actual hazard scheduler)
- Lighting: No colored lights or advanced effects

## 6. Testing Checklist

### 6.1 PlayMode Tests
- [ ] Input scenario coverage (8 test cases, FU-0505)
- [ ] HUD read-only validation (never modifies Core)
- [ ] Audio SFX triggering on specific events
- [ ] Music state machine transitions
- [ ] Round-trip input→sim→hash determinism (FU-0506)

### 6.2 Manual Testing
- [ ] Load L01–L05 in Editor, verify visuals readable
- [ ] Test camera snaps (F1-F4) on each level
- [ ] Verify HUD updates correctly during play
- [ ] Test wind gust feedback (L24)
- [ ] Verify hold mechanics (L06+)
- [ ] Record small input sequence, replay, check hash match

### 6.3 Performance Profiling
- [ ] Baseline frame time: L01 (minimal) vs L30 (full)
- [ ] Measure GC allocations (before/after pooling)
- [ ] Document draw call count
- [ ] Verify no frame drops on campaign sequence playback

### 6.4 CI Integration
- [ ] Unity input scenario tests run in batchmode
- [ ] Replay parity gate passes
- [ ] Performance metrics logged to artifact
- [ ] m5 –testCategory polish produces report

## 7. Implementation Timeline

| Task | PR | Status |
|------|----|----|
| FU-0501: GridRenderer pooling | TBD | Planned |
| FU-0502: MaterialColors refinement | TBD | Planned |
| FU-0503: Lighting tweaks | TBD | Planned |
| FU-0504: HUD layout refinement | TBD | Planned |
| FU-0505: Input scenario tests | TBD | Planned |
| FU-0506: Input↔Sim round-trip tests | TBD | Planned |
| FU-0507: Audio volume tuning | TBD | Planned |
| FU-0508: Music state timing tests | TBD | Planned |
| Docs: Scene setup guide | TBD | Planned |
| Docs: Performance characteristics | TBD | Planned |
| Docs: Known limitations | TBD | Planned |

## 8. Success Criteria

- [ ] GridRenderer achieves ≥20% frame time reduction via pooling
- [ ] All 8 input test scenarios pass in batchmode
- [ ] HUD updates correctly for 100% of level loads
- [ ] Audio/music feedback felt snappy and balanced
- [ ] Documentation covers setup, performance, and known limits
- [ ] No regression in replay parity or determinism
- [ ] All 215 Core tests still pass
