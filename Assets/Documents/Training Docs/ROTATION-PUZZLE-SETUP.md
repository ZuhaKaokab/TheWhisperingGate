# Rotation Puzzle System Setup Guide

## Overview

A modular, grid-based rotation puzzle system where players rotate elements to match a solution pattern. Perfect for Uncharted-style symbol puzzles.

**Key Features:**
- Modular grid (any rows × columns)
- Lever interaction enters "Solve Mode"
- Q/E to rotate selected element
- Arrow keys or WASD to navigate between elements
- Escape or Tab to exit solve mode
- Visual feedback for selection and correct positions
- Camera focus during solve mode

---

## Quick Setup (10 Minutes)

### Step 1: Create Puzzle Config

1. **Right-click in Project** → Create → Whispering Gate → Puzzles → Rotation Puzzle Config
2. Name it `RotationPuzzle_Portal` (or similar)
3. Configure in Inspector:

```
Puzzle ID: portal_symbols
Rows: 1
Columns: 3
Element Spacing: 1.5
Rotation Steps: 4 (90° each)
Rotation Axis: Y (horizontal spin)
```

### Step 2: Set Up Solution

In the **Solution Grid** section:
- Click cells to cycle rotation values (left-click = increase, right-click = decrease)
- Each number represents a rotation index (0, 1, 2, 3 for 4-step)
- The angle shows the actual rotation (0°, 90°, 180°, 270°)

Example for 3 elements:
```
┌────┐  ┌────┐  ┌────┐
│ 2  │  │ 0  │  │ 3  │   ← Solution indices
│180°│  │ 0° │  │270°│   ← Actual angles
└────┘  └────┘  └────┘
```

### Step 3: Set Starting Positions

- **Randomize Start: ON** → Elements start at random rotations
- **Randomize Start: OFF** → Set specific starting positions manually

### Step 4: Add Commands

In "On Solved Commands":
```
flag:portal_unlocked
cam:portal_reveal:4
```

### Step 5: Scene Setup

Create this hierarchy:

```
RotationPuzzle_Portal/
├── PuzzleController (empty GameObject)
│   └── Add: RotationPuzzleController component
│   └── Assign: Your RotationPuzzleConfig
│
├── Elements/ (parent for cubes)
│   ├── Element_0_0 (Cube)
│   │   └── Add: RotatableElement component
│   ├── Element_0_1 (Cube)
│   │   └── Add: RotatableElement component
│   └── Element_0_2 (Cube)
│       └── Add: RotatableElement component
│
└── Lever (Cube or custom model)
    └── Add: RotationPuzzleLever component
    └── Add: BoxCollider (set as Trigger)
    └── Assign: PuzzleController reference
```

### Step 6: Configure Elements

For each RotatableElement:
1. Drag the cube's Renderer to "Element Renderer" field
2. Position cubes in a row/grid matching your config

**Alternative: Auto-Spawn**
- Leave "Pre-placed Elements" empty
- Controller will spawn cubes automatically based on config

### Step 7: Create Camera Focus Point

1. Create empty GameObject: `CamPoint_RotationPuzzle`
2. Position it to view all puzzle elements
3. Rotate to set the camera angle
4. Add `CameraFocusPoint` component
5. Set Point ID: `rotation_puzzle` (match config's cameraFocusPointId)

---

## How Solve Mode Works

```
Player approaches lever → Looks at it → Presses E
                              ↓
                     Enter SOLVE MODE
                              ↓
        Camera moves to puzzle focus point
        Player movement disabled
        First element selected (highlighted)
                              ↓
    ┌──────────────────────────────────────────┐
    │  CONTROLS:                               │
    │  ─────────                               │
    │  W/↑ : Select element above              │
    │  S/↓ : Select element below              │
    │  A/← : Select element left               │
    │  D/→ : Select element right              │
    │                                          │
    │  Q   : Rotate counter-clockwise          │
    │  E   : Rotate clockwise                  │
    │                                          │
    │  ESC/Tab : Exit solve mode               │
    └──────────────────────────────────────────┘
                              ↓
            All elements match solution?
                     YES ↓
               PUZZLE SOLVED!
               Commands execute
               Camera holds, then returns
               Player regains control
```

---

## Visual Feedback

| State | Color (Default) |
|-------|-----------------|
| Normal | Original material color |
| Selected | Yellow highlight |
| Correct | Green highlight |

Configure colors in the config's "Visual Settings" section.

---

## Grid Examples

### Single Row (3 elements)
```
Rows: 1, Columns: 3

┌───┐  ┌───┐  ┌───┐
│ 0 │  │ 1 │  │ 2 │
└───┘  └───┘  └───┘
```

### 2x2 Grid
```
Rows: 2, Columns: 2

┌───┐  ┌───┐
│ 2 │  │ 3 │  ← Row 1
└───┘  └───┘
┌───┐  ┌───┐
│ 0 │  │ 1 │  ← Row 0
└───┘  └───┘
```

### 3x3 Grid
```
Rows: 3, Columns: 3

┌───┐  ┌───┐  ┌───┐
│ 6 │  │ 7 │  │ 8 │
└───┘  └───┘  └───┘
┌───┐  ┌───┐  ┌───┐
│ 3 │  │ 4 │  │ 5 │
└───┘  └───┘  └───┘
┌───┐  ┌───┐  ┌───┐
│ 0 │  │ 1 │  │ 2 │
└───┘  └───┘  └───┘
```

---

## Rotation Steps Guide

| Steps | Angle | Use Case |
|-------|-------|----------|
| 2 | 180° | Simple flip (front/back) |
| 4 | 90° | Standard rotation (most common) |
| 6 | 60° | Hexagonal patterns |
| 8 | 45° | Fine rotation |

---

## Sample Config for Portal Puzzle

```
Puzzle ID: portal_symbols
Rows: 1
Columns: 3
Rotation Steps: 4
Rotation Axis: Y

Solution: [2, 0, 3]  (180°, 0°, 270°)
Randomize Start: true

Camera Focus Point: portal_puzzle_cam
Solved Camera Hold: 4

Commands:
- flag:portal_activated
- cam:portal_glow:3
```

---

## Commands Reference

| Command | Effect | Example |
|---------|--------|---------|
| `flag:name` | Set flag true | `flag:portal_unlocked` |
| `unflag:name` | Set flag false | `unflag:door_locked` |
| `var:name+5` | Add to variable | `var:progress+10` |
| `var:name-5` | Subtract from variable | `var:sanity-5` |
| `cam:point` | Focus camera | `cam:portal_view` |
| `cam:point:3` | Focus + auto-return | `cam:portal_view:3` |

---

## Troubleshooting

### Elements Not Responding
1. Check RotatableElement component is added
2. Verify Renderer is assigned
3. Ensure puzzle is in solve mode (entered via lever)

### Camera Not Moving
1. Check CameraFocusPoint exists with matching ID
2. Verify CameraFocusController is in scene
3. Check config's cameraFocusPointId field

### Selection Not Working
1. Make sure solve mode is active
2. Check grid dimensions match element count
3. Verify navigation keys aren't conflicting

### Puzzle Not Solving
1. Compare solution indices with current element rotations
2. Check console for "[RotationPuzzle] SOLVED!" message
3. Ensure all elements have correct rotation indices

---

## Tips

1. **Test with Gizmos**: Select controller to see grid preview
2. **Use Randomize**: Click "🎲 Randomize Solution" for quick testing
3. **Different Faces**: Use cubes with different colored faces for visual distinction
4. **Audio Feedback**: Add rotate/solve sounds for satisfaction
5. **Hint System**: Place a hint object showing the correct pattern nearby

---

## Integration with Story

```
// Check if puzzle solved in dialogue conditions
condition: flag:portal_activated

// Trigger dialogue after puzzle
OnPuzzleSolved event → Start next dialogue segment
```

---

Ready to create your puzzle! 🎯


