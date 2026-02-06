# Floodline Unity MVP Constraints Pack

**Purpose:** Define deterministic, testable constraints for Floodline M5 (Unity Client Shell) so autonomous development can proceed without waiting for subjective asset creation.

**Key Principle:** Unity M5 work is asset-heavy and design-subjective. By establishing explicit constraints, placeholder strategy, and automated tests, we keep the backlog deterministic and unblocked by missing assets.

---

## 1) Style Constraints

### 1.1 Voxel Rendering
- **Material palette mapping** (Core occupancy type → Unity material):
  - `SOLID` (standard block) → `Mat_Solid_Standard`
  - `HEAVY` (heavyweight block) → `Mat_Solid_Heavy` (darker, with hazard stripes optional)
  - `REINFORCED` (with steel frame) → `Mat_Solid_Reinforced` (metallic sheen)
  - `WATER` → `Mat_Water_Surface` (translucent with strong surface line)
  - `ICE` → `Mat_Ice_Crystalline` (bright, crystalline appearance)
  - `WALL` / `BEDROCK` (immovable) → `Mat_Wall_Bedrock` (darker, immobile marker)
  - `DRAIN` (hole) → `Mat_Drain_Hole` (dark, visual sink indicator)
  - `POROUS` (optional support) → `Mat_Porous_Support` (lighter tint, permeable visual)

### 1.2 Lighting
- Non-distracting environment lighting to preserve silhouette readability.
- No heavy post-processing required for MVP.
- Gravity direction (gravity arrow icon) is the primary visual indicator of world state.

### 1.3 UI / HUD
- Canvas-based layout (UGUI).
- Safe area margins for readability on different aspect ratios.
- Color scheme: high contrast for readability (test to ensure WCAG AA minimum).

---

## 2) Placeholder Asset Strategy

### 2.1 Stub Materials
All materials under `/unity/Assets/Materials/` are procedural or single-color placeholders:
- `Mat_Solid_Standard.mat` → solid white
- `Mat_Solid_Heavy.mat` → dark gray with optional diagonal stripe pattern
- `Mat_Solid_Reinforced.mat` → metal-gray with metallic shader (Unity Standard)
- `Mat_Water_Surface.mat` → light cyan translucent
- `Mat_Ice_Crystalline.mat` → bright cyan with transparency
- `Mat_Wall_Bedrock.mat` → dark brown
- `Mat_Drain_Hole.mat` → black
- `Mat_Porous_Support.mat` → light gray

### 2.2 Stub UI Assets
- `/unity/Assets/UI/Sprites/gravity-arrow.png` → simple arrow icon
- `/unity/Assets/UI/Sprites/heart-icon.png` → simple filled circle
- `/unity/Assets/UI/Sprites/wind-icon.png` → simple directional arrow
- `/unity/Assets/UI/Sprites/target-icon.png` → simple crosshair or bullseye

### 2.3 Stub Audio
All audio clips under `/unity/Assets/Audio/` are optional sine-wave WAV files (mono, 48kHz, 200ms):
- `sfx_lock.wav` → click tone (1000 Hz)
- `sfx_drop.wav` → whoosh tone (descending 500→200 Hz)
- `sfx_rotate.wav` → chirp tone (ascending 800→1200 Hz)
- `sfx_collapse.wav` → rumble tone (100 Hz, 300ms)
- `sfx_water_flow.wav` → stream tone (600 Hz, ripple modulation)
- `sfx_drain_tick.wav` → tick tone (2000 Hz, 100ms)
- `sfx_freeze.wav` → shimmer tone (1500 Hz + harmonics)
- `sfx_thaw.wav` → pop tone (1200 Hz)
- `sfx_wind_gust.wav` → whoosh tone (200→1000 Hz, 500ms)
- `mus_calm.wav` → ambient pad (simple sine, 200 Hz, loopable)
- `mus_tension.wav` → higher-pitch pad (500 Hz, loopable)
- `mus_danger.wav` → dissonant chord (440 Hz + 660 Hz, loopable)

### 2.4 Placeholder Generation (Scripting)
A utility script (`/unity/Assets/Editor/PlaceholderAssetGenerator.cs`) creates missing stubs at build time if the asset does not already exist. This ensures CI never fails due to missing placeholder graphics.

---

## 3) HUD Layout Rules

### 3.1 Required UI Sections
The HUD must include (not all required for MVP, but layout must support):
- **Gravity Indicator** (top-left): large arrow + text label
- **Objectives Panel** (top-center): primary objective + status bar
- **Next/Hold Piece Preview** (right side): next queue (3–5) + hold slot
- **Ability Charges** (bottom-right): charge counts for Freeze, Drain, Stabilize (when enabled)
- **Rotation Budget** (bottom-left): tilt budget counter and cooldown indicator
- **Wind Gust Indicator** (top-right): wind icon + countdown timer + direction arrow
- **Score & Stars** (bottom-center): current score / earned star status

### 3.2 Layout Anchoring
- Anchors: Use safe-area-aware positioning; never hardcode screen-space pixel positions.
- Resolution independence: test on 16:9, 4:3, and ultrawide (21:9) aspect ratios.

---

## 4) Audio Stub Policy

### 4.1 SFX Playback Rules
- All SFX are fireandforget; no complex spatial audio required for MVP.
- Volume: SFX at 0.5 relative to master, Music at 0.6.
- Muting: a global AudioManager can mute/unmute all categories.

### 4.2 Music State Machine
- States: `Calm` → `Tension` → `Danger` (one-way escalation or reset on relief).
- Music crossfade: 1 second linear fade between layers/states.
- No dynamic generation; use fixed loopable stub clips.

### 4.3 Wind Gust Audio
- Wind SFX paired with a screen-space nudge animation (presentation-only; no gameplay feedback).

---

## 5) Acceptance Tests

### 5.1 EditMode Tests
Path: `/unity/Assets/Editor/Tests/ConstraintPackTests.cs`

**Test Cases:**
1. `Assets_Exist_For_All_Materials` – verify all 8 material names exist under `Assets/Materials/`.
2. `Assets_Exist_For_All_UI_Sprites` – verify all 4 UI sprite names exist under `Assets/UI/Sprites/`.
3. `Assets_Exist_For_All_Audio_Clips` – verify all 11 audio clip names exist under `Assets/Audio/SFX/` and `Assets/Audio/Music/`.
4. `HUD_Canvas_Prefab_Exists` – verify a HUD root prefab exists at `Assets/Prefabs/HUD_Canvas.prefab`.
5. `HUD_Canvas_Has_Required_Components` – verify HUD Canvas has CanvasScaler, GraphicRaycaster, and required child panels.

### 5.2 Assertions
```csharp
foreach (var materialName in new[] { "Mat_Solid_Standard", "Mat_Solid_Heavy", ... })
{
    Assert.IsNotNull(
        Resources.Load<Material>($"Materials/{materialName}"),
        $"Required material {materialName} not found. Run PlaceholderAssetGenerator."
    );
}
```

### 5.3 Running the Tests
```bash
# In Unity Editor:
Window > TextMesh Pro > Import TMP Essential Resources  # if needed
Window > General > Test Runner
Tests > Play Mode > (select ConstraintPackTests)
Run All

# Or via command-line (CI):
/path/to/Unity -projectPath /path/to/unity -runTests -testResults results.xml -testCategory EditMode
```

---

## 6) Constraint Enforcement

### 6.1 NO Subjective Design Iteration
- Style constraints are *fixed* for M5. No hand-authored art, custom shaders, or post-processing w/o explicit scope change.
- Placeholder assets are sufficient to unblock: input (FL-0501), visualization (FL-0502), HUD (FL-0504), parity testing (FL-0503).

### 6.2 NO Direct Core Dependency
- Unity never writes to Core state; all gameplay is read-only from the Core snapshot.
- Audio/music never influence simulation inputs.

### 6.3 NO Floating-Point Gameplay
- All timers, counts, and input sequences are integer-based (tick units, not seconds).
- Audio/animation can use floats for presentation only.

---

## 7) Placeholder Auto-Generation (Internal Use)

The `PlaceholderAssetGenerator` utility (run at Editor startup or explicitly via menu) ensures the following:

1. **Materials**: Create simple single-color materials if missing.
2. **UI Sprites**: Generate minimal 64×64 PNG images (white square) if missing.
3. **Audio Clips**: Generate minimal sine-wave WAV stubs if missing (or fatal error if AudioImporter unavailable).
4. **Prefabs**: Create minimal HUD Canvas prefab with default hierarchy if missing.

**Usage:**
```csharp
// In the Editor:
Assets > Floodline > Generate Placeholder Assets
```

Or automatically on first load:
```csharp
[InitializeOnLoad]
public static class PlaceholderAssetLoader
{
    static PlaceholderAssetLoader()
    {
        PlaceholderAssetGenerator.EnsureAllAssetsExist();
    }
}
```

---

## Document Metadata

- **Version**: 0.1.0
- **Status**: Foundation for M5 autonomous work
- **Next Review**: After FL-0508 acceptance tests pass in CI
- **Supersedes**: None (new document)
