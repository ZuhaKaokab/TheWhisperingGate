# Camera Focus System Setup Guide

## Overview

The Camera Focus System moves the camera to predefined positions during dialogue, creating cinematic shots. The camera physically moves to focus points and adopts their view direction.

**Key Features:**
- Camera moves TO the focus point position (not just looks at it)
- Focus point rotation defines the view direction
- Smooth position and rotation transitions
- Optional limited player look while at focus point
- Auto-releases when dialogue ends

---

## Quick Setup (5 Minutes)

### Step 1: Add CameraFocusController to Scene

1. Create an empty GameObject: `CameraFocusController`
2. Add the `CameraFocusController` component
3. Assign the Main Camera to the `Target Camera` field (optional - auto-finds if empty)

**Recommended Settings:**
- Position Transition Speed: 5 (how fast camera moves to point)
- Rotation Transition Speed: 5 (how fast camera rotates to view direction)
- Return Speed: 8 (how fast camera returns to player)
- Default Hold Duration: 0 (0 = no auto-return, or set a default like 3 seconds)
- Allow Player Look: true (let player look around slightly)
- Allowed Pitch/Yaw Range: 15-20 degrees

### Step 2: Create Focus Points

1. Create empty GameObjects at camera positions
2. **Rotate them to set the view direction** (blue arrow shows where camera looks)
3. Add the `CameraFocusPoint` component
4. Set a unique **Point ID**

**Important:** The focus point's:
- **Position** = Where camera moves to
- **Rotation** = What direction camera looks

**Naming Convention:**
```
CamPoint_Sky         → Looking up at the sky
CamPoint_TreeWide    → Wide shot of the tree
CamPoint_DollClose   → Close-up of the doll
CamPoint_WriterFace  → Looking at writer's face
```

### Step 3: Use in Dialogue

Add camera commands to your dialogue nodes:

```
cam:sky          → Camera moves to sky viewpoint (stays until reset or dialogue ends)
cam:treewide:3   → Camera moves to tree wide shot, auto-returns after 3 seconds
cam:dollclose:5  → Camera moves to doll close-up, auto-returns after 5 seconds
cam:reset        → Return camera to player control immediately
```

**Duration Parameter:**
- Add `:seconds` after the point ID for auto-return
- `cam:point_id` = Stays focused until manually reset or dialogue ends
- `cam:point_id:3` = Auto-returns to player after 3 seconds
- Great for puzzle completion cutscenes or quick reactions

---

## Scene Hierarchy Example

```
Scene
├── Managers/
│   ├── GameState
│   ├── DialogueManager
│   ├── LevelManager
│   └── CameraFocusController    ← NEW
│
├── Player/
│   ├── Player (with PlayerController)
│   └── Main Camera
│
├── Environment/
│   └── Jungle/
│       ├── TwistedTree
│       ├── Dolls
│       └── WriterSpawnPoint
│
└── CameraFocusPoints/           ← NEW
    ├── CamPoint_Sky             (above player, rotated to look UP)
    ├── CamPoint_TreeWide        (positioned for wide shot, facing tree)
    ├── CamPoint_DollClose       (close to doll, facing it)
    └── CamPoint_WriterFace      (in front of writer, facing him)
```

## How Focus Points Work

```
                    ┌──────────────────────┐
                    │   Focus Point        │
                    │   Position: (5, 2, 3)│
                    │   Rotation: (0, 45, 0)│
                    └──────────────────────┘
                              │
                              ▼
        When cam:pointname is triggered:
                              │
                              ▼
        ┌─────────────────────────────────────┐
        │  Camera smoothly moves to (5, 2, 3) │
        │  Camera smoothly rotates to face    │
        │  the direction the point faces      │
        └─────────────────────────────────────┘
```

**Setting Up a Focus Point:**
1. Position the GameObject where you want the camera to BE
2. Rotate it to face what you want the camera to SEE
3. The blue arrow gizmo shows the view direction
4. The frustum preview shows what will be visible

---

## Dialogue Integration Examples

### Segment 1: Jungle Awakening

**Node: jungle_start**
```
Start Commands: cam:sky

Camera moves to CamPoint_Sky (above player, looking up)
→ Player sees the bleeding red sky as they wake
```

**Node: jungle_tree**
```
Start Commands: cam:treewide

Camera moves to CamPoint_TreeWide (wide shot position)
→ Dramatic reveal of the twisted tree with dolls
```

**Node: jungle_doll_examine**
```
Start Commands: cam:dollclose
End Commands: cam:reset

Camera moves to CamPoint_DollClose (close-up position)
→ Close-up of the doll with player's name
→ Returns to player control after node ends
```

### Segment 2: Meet Writer

**Node: jungle_writer_meeting**
```
Start Commands: cam:writerreveal

Camera moves to CamPoint_WriterReveal
→ The Writer emerges from shadows
```

### Example Focus Point Positions

| Point | Position | Rotation (Euler) | Purpose |
|-------|----------|------------------|---------|
| `sky` | (0, 5, 0) above player | (90, 0, 0) look up | Bleeding sky |
| `treewide` | (10, 2, 5) side view | Facing tree | Tree reveal |
| `dollclose` | (2, 1.5, 1) near doll | Facing doll | Name close-up |
| `writerreveal` | (0, 1.5, -5) | Facing spawn | Writer reveal |

---

## Command Reference

| Command | Effect |
|---------|--------|
| `cam:point_id` | Focus camera on the named focus point |
| `cam:reset` | Release focus, return to player camera |
| `cam:release` | Same as reset |
| `cam:free` | Same as reset |

---

## Player Look During Focus

While at a focus point, the player can still look around slightly:
- Default range: ±15° vertical, ±20° horizontal
- Mouse movement is slower than normal
- Look offset gradually returns to center when mouse is still
- Creates a "directed but not locked" feeling

To disable player look during focus:
- Uncheck `Allow Player Look` in CameraFocusController, OR
- Set `Allowed Pitch Range` and `Allowed Yaw Range` to 0

---

## Debug Features

### Gizmos

Focus points display:
- **Cyan wire sphere** - The focus point location
- **Direction ray** - The forward direction (useful for positioning)
- **Yellow solid sphere** - When selected in editor
- **Label** - Shows the Point ID above the point

### Console Logging

The system logs:
- When focus points are registered at startup
- When camera focuses on a point
- When focus is released
- Warnings for missing focus points

### Checking Focus Points at Runtime

In a test script or console:
```csharp
// Get all registered focus point IDs
string[] ids = CameraFocusController.Instance.GetAllFocusPointIds();
foreach (var id in ids)
    Debug.Log(id);

// Check if currently focusing
bool isFocusing = CameraFocusController.Instance.IsFocusing;

// Get current focus target
Transform target = CameraFocusController.Instance.CurrentFocusTarget;
```

---

## Troubleshooting

### Camera doesn't focus when command is triggered

1. **Check CameraFocusController exists** - Must be in scene
2. **Check Point ID spelling** - Case-insensitive but must match
3. **Check focus point has component** - Must have `CameraFocusPoint` component
4. **Check console for warnings** - System logs missing points

### Camera snaps instead of smooth transition

- Increase `Focus Transition Speed` value
- Check that `LateUpdate` is running (not paused)

### Focus point not found

- Call `CameraFocusController.Instance.RefreshFocusPoints()` after adding points dynamically
- Check the Point ID field is not empty on the `CameraFocusPoint` component

### Player can't look at all during focus

- Check `Allowed Pitch Range` and `Allowed Yaw Range` are > 0
- Check `Player Look Sensitivity` is > 0

---

## Best Practices

### Positioning Focus Points

1. **Position = Camera location** - Where do you want the camera to BE?
2. **Rotation = View direction** - What should the camera look AT?
3. **Use the gizmo** - Blue arrow shows view direction, frustum shows field of view
4. **Create multiple shots** - `tree_wide`, `tree_close`, `tree_side` for variety
5. **Test in Play mode** - Press Play and trigger dialogue to see the result

### When to Use Focus

✅ **Good uses:**
- Drawing attention to important story elements
- Character introductions
- Examining key objects (dolls, journal, portal)
- Environmental reveals (twisted tree, bleeding sky)

❌ **Avoid:**
- Every single dialogue node (gets repetitive)
- Long focus locks (player feels trapped)
- Focusing on things behind the player (jarring rotation)

### Timing Tips

- Use `cam:reset` in **End Commands** to release before next node
- Let auto-release handle focus at dialogue end
- Short focus moments (2-3 seconds) feel better than long locks

---

## Example: Full Jungle Awakening Setup

### Focus Points to Create

| Point ID | Position | Purpose |
|----------|----------|---------|
| `sky` | Above player, looking up | Opening shot - bleeding red sky |
| `tree` | At twisted tree | The ominous tree with dolls |
| `doll` | At specific doll | The doll with player's name |
| `path` | At hidden path entrance | Where scream comes from |

### Dialogue Commands

**jungle_start:**
```
Start Commands: cam:sky
```

**jungle_look:**
```
Start Commands: cam:tree
```

**jungle_tree:**
```
Start Commands: cam:tree
```

**jungle_doll_examine:**
```
Start Commands: cam:doll
End Commands: item:name_doll
```

**jungle_scream:**
```
Start Commands: cam:path
End Commands: cam:reset
```

---

## Integration Checklist

- [ ] Add `CameraFocusController` to scene
- [ ] Create focus point GameObjects with `CameraFocusPoint` components
- [ ] Set unique Point IDs for each focus point
- [ ] Add `cam:` commands to dialogue nodes
- [ ] Test focus transitions in Play mode
- [ ] Verify auto-release on dialogue end

---

**The camera focus system is now ready to use!**

