# Grid Path Puzzle - Setup Guide

## Overview

The Grid Path Puzzle is a floor-based puzzle where players must walk across tiles in a specific sequence. Wrong steps reset the puzzle.

**Features:**
- Data-driven via ScriptableObject configuration
- Visual path editor in Inspector
- Tile sink feedback on correct steps
- Integration with GameState commands
- Optional UI for progress tracking

---

## Quick Setup (10 Minutes)

### Step 1: Create the Puzzle Config

1. **Right-click in Project** → Create → Whispering Gate → Puzzles → Grid Puzzle Config
2. Name it `GridPuzzle_House` (or your choice)
3. Configure settings:

| Setting | Value | Description |
|---------|-------|-------------|
| Puzzle ID | `house_entrance` | Unique identifier |
| Rows | 5 | Grid depth (Z axis) |
| Cols | 5 | Grid width (X axis) |
| Mode | ExactSequence | Must follow exact path |
| Tile Sink Depth | 0.15 | How far tiles sink |
| Tile Move Speed | 5 | Animation speed |

### Step 2: Define the Correct Path

**Option A: Visual Editor (Recommended)**
1. In the Inspector, scroll to "Grid Preview"
2. Click tiles to add/remove from path
3. Numbers show the order

**Option B: Quick Templates**
- Click "Straight Line", "Diagonal", "Zigzag", or "Snake"
- Modify as needed

**Option C: Manual Entry**
- Expand "Correct Path" list
- Add coordinates: (0,0), (1,0), (1,1), etc.

### Step 3: Set Start/End Tiles

- **Start Tile:** Where player enters (usually bottom edge)
- **End Tile:** Final tile to reach (usually top edge)

### Step 4: Add Commands

In "On Solved Commands":
```
flag:grid_puzzle_solved
cam:house_door:3
```

> **Tip:** `cam:house_door:3` means focus on `house_door` camera point for 3 seconds, then auto-return to player.

In "On Failed Commands" (optional):
```
var:insanity-5
```

---

## Scene Setup

### Step 5: Create Tile Prefab

1. Create a Cube: GameObject → 3D Object → Cube
2. Scale it: (1, 0.2, 1)
3. Add `BoxCollider` (should already exist)
4. Set collider as **Trigger** ✓
5. Add `GridTile` component
6. **Save as Prefab:** Drag to Project folder

**Optional Materials:**
- Assign Default Material (gray)
- Assign Pressed Material (green tint)
- Assign Wrong Material (red tint)

### Step 6: Create Puzzle Controller

1. Create empty GameObject: `GridPuzzle_House`
2. Add `GridPuzzleController` component
3. Assign:
   - **Config:** Your GridPuzzleConfig asset
   - **Tile Prefab:** Your cube prefab
   - **Tile Spacing:** 1.1 (slightly larger than tile)

### Step 7: Generate the Grid

1. With the controller selected
2. Click **Context Menu (⋮)** → Generate Grid
3. Tiles will spawn as children

### Step 8: Add Activation Trigger

1. Create empty child: `PuzzleTrigger`
2. Add `BoxCollider` (set as Trigger)
3. Size it to cover puzzle entrance area
4. Add `GridPuzzleTrigger` component
5. Assign the `GridPuzzleController` reference

---

## Hierarchy Example

```
GridPuzzle_House
├── GridPuzzleController (component)
├── PuzzleTrigger
│   └── BoxCollider (trigger)
│   └── GridPuzzleTrigger (component)
└── Tiles (auto-generated)
    ├── Tile_0_0
    ├── Tile_0_1
    ├── Tile_1_0
    └── ... (25 tiles for 5x5)
```

---

## Path Design Tips

### Good Path Patterns

**The L-Shape:**
```
□ □ □ □ ■    ■ = Correct path
□ □ □ □ ■    □ = Wrong (reset)
□ □ □ □ ■    
□ □ □ □ ■    Player starts bottom-left
■ ■ ■ ■ ■    Ends top-right
```

**The Zigzag:**
```
■ □ □ □ □
■ ■ ■ □ □
□ □ ■ □ □
□ □ ■ ■ ■
□ □ □ □ ■
```

**The Spiral:**
```
■ ■ ■ ■ ■
□ □ □ □ ■
■ ■ ■ □ ■
■ □ □ □ ■
■ ■ ■ ■ ■
```

### Path Design Guidelines

1. **Make it fair** - Path should be solvable with the available hints
2. **Start simple** - First few steps should be obvious
3. **Create landmarks** - Memorable turns or patterns
4. **Consider hint placement** - What part of the path does each hint reveal?

---

## Hint Integration

### Wall Carving (Partial Hint)

Create a readable texture or decal near the puzzle showing:
```
"Follow the serpent's spine:
 Start at the setting sun (west),
 Turn at the third stone..."
```

### Paper Item (Full Solution)

Create an `ItemData` for a torn map or note:
- Found inside the house
- Shows complete path diagram
- Player must remember or refer back

### Environmental Hint

- Dead bodies on wrong tiles (dark!)
- Scorch marks on incorrect tiles
- Moss/growth only on safe tiles

---

## Integration with Game Systems

### GameState Flags

The puzzle automatically sets flags via commands:

```csharp
// In config, add these commands:
onSolvedCommands = ["flag:grid_puzzle_solved"]

// Later, check in dialogue conditions:
// condition: "flag:grid_puzzle_solved"
```

### Camera Focus

Add camera commands for dramatic shots:

```
onSolvedCommands:
  flag:grid_puzzle_solved
  cam:house_door_reveal:4
```

> Camera moves to `house_door_reveal` focus point for 4 seconds, then returns to player.

### Dialogue Trigger

After puzzle is solved, trigger next story beat:

```csharp
// On DialogueSegmentTrigger, set prerequisite:
prerequisiteSegments = ["grid_puzzle_solved"]
```

---

## Testing Checklist

- [ ] Grid generates correctly
- [ ] Tiles have colliders set as triggers
- [ ] Player has "Player" tag
- [ ] Stepping on correct tile sinks it
- [ ] Stepping on wrong tile resets all
- [ ] Completing path triggers solved event
- [ ] Commands execute (check Console for flag/var changes)
- [ ] UI shows progress (if using GridPuzzleUI)

---

## Troubleshooting

### Tiles Not Detecting Player

1. Check player has **"Player" tag**
2. Check tile colliders are **Is Trigger = true**
3. Check player has a **Collider** (CharacterController counts)

### Grid Not Generating

1. Check **Config** is assigned
2. Check **Tile Prefab** is assigned
3. Try right-click → Generate Grid

### Wrong Tile Sinking

1. Verify correct path coordinates in config
2. Use the Grid Preview to visualize
3. Check coordinate system (X = columns, Y = rows from front)

### Commands Not Executing

1. Check GameState.Instance exists in scene
2. Check command syntax: `flag:name` not `flag: name`
3. Check Console for execution logs

---

## Sample Config Values

### Easy Puzzle (Tutorial)
```
Rows: 3, Cols: 3
Path: (1,0) → (1,1) → (1,2)
Just a straight line up the middle
```

### Medium Puzzle (House Entrance)
```
Rows: 5, Cols: 5
Path: 7-10 steps with 2-3 turns
Hint: Wall carving shows first half
```

### Hard Puzzle (Optional Challenge)
```
Rows: 5, Cols: 5
Path: All 25 tiles in specific order (snake)
Hint: Must find multiple clue pieces
```

---

## Code Reference

### Key Classes

| Class | Purpose |
|-------|---------|
| `GridPuzzleConfig` | ScriptableObject with puzzle data |
| `GridPuzzleController` | Main puzzle logic and state |
| `GridTile` | Individual tile behavior |
| `GridPuzzleTrigger` | Activation zone |
| `GridPuzzleUI` | Optional progress display |

### Events You Can Subscribe To

```csharp
puzzleController.OnPuzzleStarted += () => { /* Puzzle activated */ };
puzzleController.OnProgressChanged += (current, total) => { /* Step taken */ };
puzzleController.OnPuzzleFailed += () => { /* Wrong step */ };
puzzleController.OnPuzzleSolved += () => { /* Puzzle complete! */ };
```

---

## Next Steps

After setting up the Grid Puzzle:

1. **Place in scene** before the abandoned house entrance
2. **Create hint items** (wall carving, paper in house)
3. **Test the flow**: Jungle → Find hint → Solve puzzle → Enter house
4. **Polish**: Add sound effects, particle effects on solve

---

*Grid Puzzle system ready for The Whispering Gate prologue!*

