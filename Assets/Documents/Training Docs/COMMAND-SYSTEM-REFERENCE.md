# Command System Reference

## Overview

The Whispering Gate uses a unified command system that allows different game systems to communicate through simple string commands. Commands can be triggered from:

- **Dialogue nodes** (in the Command field)
- **Puzzle completion** (OnSolvedCommands, OnFailedCommands)
- **Triggers and interactions**
- **Scripts** (via direct calls)

---

## Command Format

```
command_type:parameter
command_type:action:target
command_type:target:value
```

Commands are **case-insensitive** for the command type.

---

## Complete Command Reference

### 🎮 Game State Commands

| Command | Example | Description |
|---------|---------|-------------|
| `flag:name` | `flag:door_opened` | Sets a boolean flag to **true** |
| `unflag:name` | `unflag:puzzle_hint_shown` | Sets a boolean flag to **false** |
| `var:name+value` | `var:courage+10` | Adds to an integer variable |
| `var:name-value` | `var:sanity-5` | Subtracts from an integer variable |
| `var:name=value` | `var:chapter=2` | Sets an integer variable to exact value |

**Examples:**
```
flag:talked_to_writer
flag:journal_found
unflag:first_visit
var:courage+15
var:insanity-10
var:chapter=3
```

---

### 📦 Inventory Commands

| Command | Example | Description |
|---------|---------|-------------|
| `item:item_id` | `item:key_rusty` | Gives item to player |
| `removeitem:item_id` | `removeitem:key_rusty` | Removes item from inventory |

**Examples:**
```
item:flashlight
item:journal
item:mysterious_key
removeitem:used_potion
```

---

### 📖 Journal Commands

| Command | Example | Description |
|---------|---------|-------------|
| `journal:unlock:page_id` | `journal:unlock:forest_lore` | Unlocks a journal page |
| `journal:open` | `journal:open` | Opens the journal UI |
| `journal:goto:page_id` | `journal:goto:map_1` | Opens journal to specific page |
| `journal:pickup` | `journal:pickup` | Gives journal to player (if not using physical pickup) |

**Examples:**
```
journal:unlock:prologue_intro
journal:unlock:puzzle_hint_grid
journal:open
journal:goto:writer_notes
```

---

### 🎥 Camera Commands

| Command | Example | Description |
|---------|---------|-------------|
| `cam:focus_point_id` | `cam:tree_view` | Focus camera on a point (uses default duration) |
| `cam:focus_point_id:duration` | `cam:portal:4` | Focus camera with auto-return after duration |
| `cam:reset` | `cam:reset` | Release camera focus, return to player |
| `cam:release` | `cam:release` | Same as reset |

**Focus Point IDs** are defined by `CameraFocusPoint` components in the scene.

**Examples:**
```
cam:mysterious_tree:5
cam:portal_view:3
cam:puzzle_overview:2
cam:reset
```

---

### 🚪 Door Commands

| Command | Example | Description |
|---------|---------|-------------|
| `door:open:door_id` | `door:open:house_door` | Opens a door |
| `door:close:door_id` | `door:close:gate` | Closes a door |
| `door:toggle:door_id` | `door:toggle:secret_passage` | Toggles door open/closed |
| `door:lock:door_id` | `door:lock:vault` | Locks a door |
| `door:unlock:door_id` | `door:unlock:vault` | Unlocks a door |
| `door:door_id` | `door:main_gate` | Shorthand for open |

**Door IDs** are defined in the `Door` component's `doorId` field.

**Examples:**
```
door:open:abandoned_house
door:close:entrance
door:unlock:cellar_door
door:toggle:hidden_bookshelf
```

---

### ⚡ Activatable Object Commands

| Command | Example | Description |
|---------|---------|-------------|
| `activate:object_id` | `activate:portal` | Activates an object (enable, spawn, animate) |
| `deactivate:object_id` | `deactivate:portal` | Deactivates an object |

**Object IDs** are defined in the `ActivatableObject` component's `objectId` field.

**Examples:**
```
activate:portal
activate:light_puzzle_complete
activate:bridge_mechanism
deactivate:barrier
```

---

### 🔦 Flashlight Commands

| Command | Example | Description |
|---------|---------|-------------|
| `flashlight:on` | `flashlight:on` | Turns flashlight on |
| `flashlight:off` | `flashlight:off` | Turns flashlight off |
| `flashlight:toggle` | `flashlight:toggle` | Toggles flashlight |
| `flashlight:recharge` | `flashlight:recharge` | Refills battery to max |
| `flashlight:recharge:amount` | `flashlight:recharge:50` | Adds specific battery amount |
| `flashlight:enable` | `flashlight:enable` | Enables flashlight system |
| `flashlight:enable:on` | `flashlight:enable:on` | Enables and turns on |
| `flashlight:disable` | `flashlight:disable` | Disables flashlight system |

**Examples:**
```
flashlight:on
flashlight:toggle
flashlight:recharge:25
```

---

### 🏁 Ending Commands

| Command | Example | Description |
|---------|---------|-------------|
| `ending:ending_id` | `ending:true_ending` | Sets the current ending path |

**Examples:**
```
ending:bad_ending
ending:good_ending
ending:secret_ending
```

---

## Usage by System

### Dialogue System

In the **DialogueNode** inspector, add commands to the **Command** field:

```
Command: flag:talked_to_writer
Command: journal:unlock:forest_lore
Command: item:mysterious_map
```

Multiple commands can be separated by commas or placed in separate command fields.

---

### Puzzle System (Grid & Rotation)

In the **PuzzleConfig** ScriptableObject, add commands to:

- **On Solved Commands** - Execute when puzzle is completed
- **On Failed Commands** - Execute when puzzle fails (grid puzzle)

```
OnSolvedCommands:
  - door:open:house_door
  - cam:door_view:3
  - flag:grid_puzzle_solved
  - journal:unlock:next_clue
```

---

### Dialogue Segment Trigger

The `DialogueSegmentTrigger` can execute commands via its **On Pickup Commands** field.

---

## Conditional Execution

Commands are executed immediately when triggered. For conditional commands, use **GameState conditions** in:

- Dialogue choice conditions
- Trigger prerequisites
- Journal page unlock conditions

**Condition Examples:**
```
courage >= 30
journal_found
talked_to_writer && courage > 20
!puzzle_solved
```

---

## Creating New Commands

To add support for a new command type:

1. **DialogueManager.cs** - Add case in `ExecuteCommand()` switch
2. **GridPuzzleController.cs** - Add case in `ExecuteCommand()` switch
3. **RotationPuzzleController.cs** - Add case in `ExecuteCommand()` switch

```csharp
case "mycommand":
    HandleMyCommand(param);
    break;
```

---

## Quick Reference Card

```
┌─────────────────────────────────────────────────────────────┐
│                    COMMAND QUICK REFERENCE                  │
├─────────────────────────────────────────────────────────────┤
│ GAME STATE                                                  │
│   flag:name          unflag:name         var:name+/-/=value │
├─────────────────────────────────────────────────────────────┤
│ INVENTORY                                                   │
│   item:id            removeitem:id                          │
├─────────────────────────────────────────────────────────────┤
│ JOURNAL                                                     │
│   journal:unlock:id  journal:open        journal:goto:id    │
├─────────────────────────────────────────────────────────────┤
│ CAMERA                                                      │
│   cam:point_id       cam:point_id:dur    cam:reset          │
├─────────────────────────────────────────────────────────────┤
│ DOOR                                                        │
│   door:open:id       door:close:id       door:toggle:id     │
│   door:lock:id       door:unlock:id                         │
├─────────────────────────────────────────────────────────────┤
│ ACTIVATABLE                                                 │
│   activate:id        deactivate:id                          │
├─────────────────────────────────────────────────────────────┤
│ FLASHLIGHT                                                  │
│   flashlight:on      flashlight:off      flashlight:toggle  │
│   flashlight:recharge:amount                                │
├─────────────────────────────────────────────────────────────┤
│ ENDING                                                      │
│   ending:id                                                 │
└─────────────────────────────────────────────────────────────┘
```

---

## Troubleshooting

**Command not executing:**
- Check console for warnings (unknown command type)
- Verify the target ID exists (door ID, focus point ID, etc.)
- Ensure the relevant manager/controller is in the scene

**Camera not returning:**
- Use `cam:point:duration` format for auto-return
- Or manually call `cam:reset`

**Door not opening:**
- Check `doorId` matches the command
- Verify door isn't locked (`door:unlock:id` first)
- Check door component is enabled

**Journal page not showing:**
- Verify page is in `JournalConfig.allPages`
- Check page ID matches exactly
- Ensure page is unlocked

