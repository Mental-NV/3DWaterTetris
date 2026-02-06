# HUD Layout Reference

This document describes the layout of the Floodline HUD and provides guidance for implementation.

## Canvas Hierarchy

The HUD is built using Unity's UGUI (Canvas-based). The following is the canonical hierarchy:

```
HUD_Canvas (Canvas, CanvasScaler)
├── SafeArea (RectTransform - auto-positioned for device safe area)
│   ├── GravityIndicator (top-left)
│   │   ├── GravityArrow (Image)
│   │   └── GravityLabel (Text)
│   ├── ObjectivesPanel (top-center)
│   │   ├── ObjectiveTitle (Text)
│   │   └── ObjectiveProgressBar (Slider)
│   ├── NextHoldPanel (right side)
│   │   ├── NextQueueDisplay (Image array, 5 slots)
│   │   └── HoldSlotDisplay (Image)
│   ├── AbilityChargesPanel (bottom-right)
│   │   ├── FreezeCharges (Text)
│   │   ├── DrainCharges (Text)
│   │   └── StabilizeCharges (Text)
│   ├── RotationBudgetPanel (bottom-left)
│   │   ├── TiltBudgetLabel (Text)
│   │   └── CooldownIndicator (Image/Slider)
│   ├── WindGustIndicator (top-right)
│   │   ├── WindIcon (Image)
│   │   ├── CountdownTimer (Text)
│   │   └── DirectionArrow (Image)
│   └── ScoreStarsPanel (bottom-center)
│       ├── ScoreLabel (Text)
│       └── StarsDisplay (Image array, 3 stars)
```

## Positioning Guidelines

- **Safe Area**: Wrap all HUD elements in a SafeArea RectTransform that respects device notches and safe zones.
- **Top-Left (Gravity)**: Anchor: top-left; Offset: 20px from edge.
- **Top-Center (Objectives)**: Anchor: top-center; centered horizontally.
- **Right Side (Next/Hold)**: Anchor: right; Offset: 20px from right edge.
- **Bottom-Right (Abilities)**: Anchor: bottom-right; Offset: 20px from edge.
- **Bottom-Left (Rotation)**: Anchor: bottom-left; Offset: 20px from edge.
- **Top-Right (Wind)**: Anchor: top-right; Offset: 20px from right edge.
- **Bottom-Center (Score/Stars)**: Anchor: bottom-center; centered horizontally.

## Color Scheme

- **Text**: White (RGBA: 255, 255, 255, 255) for readability on dark backgrounds.
- **Accent Color**: Cyan (RGBA: 0, 200, 255, 255) for active states / indicators.
- **Warning Color**: Orange (RGBA: 255, 165, 0, 255) for danger/tilt budget low.
- **Success Color**: Green (RGBA: 0, 255, 100, 255) for objectives complete.

## Layout Validation

Test the HUD layout on the following aspect ratios:
- 16:9 (standard HD/Full HD) – the primary target
- 4:3 (legacy tablets/monitors)
- 21:9 (ultrawide)
- 19.5:9 (modern mobile with notch)

Ensure all text is readable (minimum font size 24pt) and no elements are cut off by the safe area.

## Update Frequency

The HUD updates once per game tick (60 Hz default). Data is pulled from the Core simulation state snapshot:
- Gravity direction (from state.CurrentGravity)
- Objective progress (from state.Objective.Progress)
- Next piece queue (from state.Bag state)
- Hold slot (from state.ActivePiece.HeldPiece)
- Ability charges (from state.AbilitiesState)
- Rotation budget (from state.RotationState)
- Wind gust countdown (from state.HazardState)
- Score/stars (from state.ObjectiveResult)

All updates are **read-only** from the Core perspective; the HUD never writes back to the simulation.

## Testing

Minimal HUD acceptance tests are found in `/unity/Assets/Editor/Tests/ConstraintPackTests.cs`:
- Prefab existence
- Component presence
- Folder structure

Manual testing is required for layout, colors, and readability.
