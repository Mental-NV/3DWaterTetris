# Material Color Refinement Guide (FL-0507 Polish)

## Overview

This guide documents the color palette for Floodline's 9 material types and provides tuning recommendations for visual clarity and gameplay feel. Colors are defined in [MaterialPalette.cs](../unity/Assets/Materials/MaterialPalette.cs) and applied via [VoxelMaterialMapper.cs](../unity/Assets/Materials/VoxelMaterialMapper.cs).

## Current Palette (MVP Baseline)

| Material Type | RGB (Hex) | Description | Purpose |
|---|---|---|---|
| **STANDARD** | (0.8, 0.8, 0.8) #CCCCCC | Light gray | Default solid; earth/concrete |
| **HEAVY** | (0.5, 0.5, 0.5) #808080 | Dark gray | Heavy blocks; increased visual weight |
| **REINFORCED** | (0.6, 0.6, 0.8) #9999CC | Blue-gray | Structural; metal reinforcement |
| **WATER** | (0.3, 0.6, 0.9) #4D99E6 | Light blue | Water; transparent at 50% alpha |
| **ICE** | (0.7, 0.95, 1.0) #B3F2FF | Cyan | Frozen; icy appearance |
| **DRAIN** | (0.3, 0.3, 0.3) #4D4D4D | Very dark gray | Drain holes; warning |
| **BEDROCK** | (0.3, 0.2, 0.1) #4D3319 | Brown | Immovable base; earth tone |
| **WALL** | (0.4, 0.4, 0.5) #666680 | Dark blue-gray | Boundary walls |
| **POROUS** | (0.75, 0.65, 0.5) #BFA680 | Tan | Permeable; sand tone |

## Refinement Candidates for Polish Phase

### 1. HEAVY → DARK SLATE
**Current**: (0.5, 0.5, 0.5) - neutral dark gray  
**Proposal**: (0.35, 0.42, 0.48) - slate blue undertone  
**Rationale**: 
- Adds visual distinction from STANDARD
- Cooler tone suggests inertia/weight
- Improves readability in dark scenes
- Reference: Blueprints, industrial materials

**Test Scenario**: Place heavy block adjacent to standard block; verify visual hierarchy

### 2. REINFORCED → STEEL BLUE
**Current**: (0.6, 0.6, 0.8) - light blue-gray  
**Proposal**: (0.5, 0.6, 0.9) - saturated steel blue  
**Rationale**:
- Increase saturation to suggest strength/structure
- Reduce gray undertone for clarity
- More distinct from HEAVY
- Reference: Steel, metal structures

**Test Scenario**: Reinforced tower vs. heavy column; verify strength perception

### 3. ICE → CRYSTALLINE CYAN
**Current**: (0.7, 0.95, 1.0) - pale cyan  
**Proposal**: (0.5, 0.9, 1.0) - bright crystalline  
**Rationale**:
- Increase saturation for visual "pop"
- Reduce lightness to improve shadow definition
- More clearly distinguishable from WATER
- Reference: Ice crystals, translucent clarity

**Test Scenario**: Ice structure under directional light; verify crystal-like appearance

### 4. BEDROCK → OCHRE BROWN
**Current**: (0.3, 0.2, 0.1) - very dark brown  
**Proposal**: (0.5, 0.38, 0.25) - warm ochre  
**Rationale**:
- Increase brightness for readability
- Add warmth to show immovability (earth)
- Better contrast in underground levels
- Reference: Clay, stone, earth tones

**Test Scenario**: Bedrock foundation level; verify visibility and stability perception

### 5. POROUS → LIGHT SAND
**Current**: (0.75, 0.65, 0.5) - muted tan  
**Proposal**: (0.85, 0.75, 0.6) - light sandy  
**Rationale**:
- Increase brightness for better contrast
- Maintain warm tone to suggest permeability
- Distinguish from BEDROCK (darker earth)
- Reference: Sand, permeable materials

**Test Scenario**: Porous blocks in basin; verify water interaction visibility

## Validation Protocol

### Visual Clarity Test
1. Render test level with all 9 material types
2. Verify each type distinguishable from adjacent materials
3. Test under multiple lighting scenarios:
   - Directional light (main light at 45° elevation)
   - Ambient light only (0.3 intensity)
   - Fill light enabled (warm fill at 45° opposite)

### Gameplay Feel Test
1. **Weight Perception**: Heavy/Reinforced should feel "heavier" (darker)
2. **Safety Perception**: Water/Ice should feel "hazardous" (saturated, distinct)
3. **Structure Perception**: Bedrock should feel "immovable" (earth tone)
4. **Stability vs. Flexibility**: Standard vs. Porous should suggest material differences

### Performance Validation
- [ ] No material lookup overhead increase (VoxelMaterialMapper cache working)
- [ ] Frame time stable: <16ms per frame (60 fps)
- [ ] No GC spikes during material changes
- [ ] Object pool activation/deactivation smooth

## Implementation Steps

1. **Phase 1 - Testing (5 min)**
   - Create test scene with 3×3×3 grid of each material type
   - Set up 4 lighting configurations
   - Visually compare current vs. proposed colors side-by-side

2. **Phase 2 - Refinement (10 min)**
   - Adjust HSV values based on test results
   - Create variants (Light, Medium, Dark) of 2-3 key materials
   - AB test with playtesters if available

3. **Phase 3 - Integration (5 min)**
   - Update MaterialPalette.cs with final RGB values
   - Verify all levels still render correctly
   - Run full CI to ensure no regressions

## Lighting Interaction Recommendations

### Directional Light (Current: Cool 0.9, 0.95, 1.0)
- **Purpose**: Main illumination; defines form and shadow
- **Recommended adjustment**: Slightly warm (0.95, 0.92, 0.88) for earth-tone compatibility
- **Impact**: Makes BEDROCK/POROUS warmer; better color harmony

### Ambient Light (Current: Neutral 1.0, 1.0, 1.0)
- **Purpose**: Fill shadow regions; prevent pure black
- **Recommended adjustment**: Slight blue tint (0.95, 0.97, 1.0) for cool shadows
- **Impact**: Cooler shadows; water/ice materials pop more

### Fill Light (Optional)
- **Purpose**: 45° offset fill; adds plastic/stage lighting
- **Recommended**: Warm tone (1.0, 0.85, 0.7) from opposite of directional
- **Impact**: Adds depth; useful for cave/underground levels

## Historical Notes

- **FL-0507**: Baseline MaterialPalette established with 9 material types
- **FL-0502**: Isometric camera + light setup; influenced color choices
- **GDD v0.2**: Material types defined; no specific color recommendations

## Future Work

- [ ] Material glossiness/smoothness tuning per type
- [ ] Normal map support for tactile appearance
- [ ] Emissive materials for special effects
- [ ] Environment-specific color variants (dark level, bright level, etc.)
- [ ] Colorblind mode: alternative palettes for accessibility
- [ ] Shader-based color grading for post-processing

## References

- [MaterialPalette.cs](../unity/Assets/Materials/MaterialPalette.cs)
- [VoxelMaterialMapper.cs](../unity/Assets/Materials/VoxelMaterialMapper.cs)
- [LightingSetup.cs](../unity/Assets/Lighting/LightingSetup.cs)
- [GDD v0.2](./GDD_Core_v0_2.md)
