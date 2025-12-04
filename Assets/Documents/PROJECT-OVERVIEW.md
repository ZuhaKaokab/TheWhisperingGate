# The Whispering Gate - Project Overview

**Document Version:** 1.0  
**Last Updated:** December 4, 2025  
**Engine:** Unity 2021.3+  
**Genre:** Psychological Horror Thriller with Choice-Based Narrative

---

## 📋 Table of Contents

1. [Project Vision](#project-vision)
2. [Systems Overview](#systems-overview)
3. [Core Systems Breakdown](#core-systems-breakdown)
4. [Architecture Patterns](#architecture-patterns)
5. [File Structure](#file-structure)
6. [Integration Map](#integration-map)
7. [What's Working](#whats-working)
8. [Remaining Tasks](#remaining-tasks)

---

## 🎯 Project Vision

**The Whispering Gate** is a psychological horror thriller featuring:
- Choice-driven narrative with meaningful consequences
- Life is Strange / TWD-style impact notifications
- Exploration and environmental storytelling
- Character relationships affected by player choices
- Multiple story branches and endings

**Target:** 15-minute playable prologue set in a nightmare jungle

---

## 🔧 Systems Overview

| System | Status | Description |
|--------|--------|-------------|
| **Dialogue System** | ✅ Complete | Branching dialogue with choices, conditions, and commands |
| **GameState** | ✅ Complete | Flags, variables, conditions, and player stats |
| **Player Controller** | ✅ Complete | Hybrid FP/TP, movement, jump, crouch |
| **Animation System** | ✅ Complete | Animator integration with controller |
| **Inventory System** | ✅ Complete | Items, hotbar, grid view, details panel |
| **UI System** | ✅ Complete | Dialogue, inventory, impacts, stats |
| **Interaction System** | ✅ Complete | Interactable objects, examine, pickup |
| **Level Management** | ✅ Complete | Scene transitions, segment triggers |
| **Camera Focus** | ✅ Complete | Cinematic camera positions during dialogue |

---

## 🏗️ Core Systems Breakdown

### 1. Dialogue System

**Purpose:** Handles all story dialogue, branching conversations, and narrative choices.

**Components:**
- `DialogueManager.cs` - Core singleton managing dialogue flow
- `DialogueTree.cs` - ScriptableObject container for dialogue nodes
- `DialogueNode.cs` - ScriptableObject representing a single dialogue moment
- `DialogueChoice.cs` - ScriptableObject for player choices
- `ChoiceImpact.cs` - ScriptableObject defining consequences

**Features:**
- Branching conversations with multiple paths
- Conditional choices (show/hide based on GameState)
- Start/End commands for triggering game events
- Character portraits and speaker names
- Auto-advance for narration vs. wait for input
- Support for starting dialogue at any node

**Commands Supported:**
```
item:key_rusty         → Add item to inventory
flag:met_writer        → Set a boolean flag
var:trust:+10          → Modify a variable
ending:bad             → Set ending state
cam:pointname          → Move camera to focus point
cam:reset              → Return camera to player
```

**Key Events:**
- `OnDialogueStarted` - Fires when dialogue begins
- `OnDialogueEnded` - Fires when dialogue completes
- `OnNodeDisplayed` - Fires for each dialogue node
- `OnChoiceSelected` - Fires when player makes a choice
- `OnImpactApplied` - Fires when a choice has consequences

---

### 2. GameState System

**Purpose:** Central state management for all game data - flags, variables, player stats, and condition evaluation.

**Components:**
- `GameState.cs` - Singleton managing all game state

**Features:**
- **Flags:** Boolean states (met_writer, found_key, etc.)
- **Variables:** Integer values (trust, courage, insanity, etc.)
- **Conditions:** String-based condition evaluation
- **Stats:** Player attributes affecting story outcomes

**Condition Syntax:**
```
flag:met_writer                    → Check if flag is true
!flag:met_writer                   → Check if flag is false
var:trust>=50                      → Variable comparison
var:courage>0 && flag:explored     → Compound conditions
```

**Player Stats Tracked:**
- Trust (relationship with NPCs)
- Courage (bravery in face of horror)
- Insanity (mental stability)
- Custom variables as needed

---

### 3. Player Controller

**Purpose:** Handles all player movement, camera control, and input.

**Components:**
- `PlayerController.cs` - Main movement and camera controller
- `PlayerAnimationController.cs` - Animator parameter bridge

**Features:**
- **Hybrid View:** Toggle between First Person and Third Person
- **Movement:** Walk, sprint, crouch, strafe
- **Jump:** Animation-event triggered for precise timing
- **Camera:** Smooth follow in TP, direct control in FP
- **Crouch:** Toggle with height change and speed reduction
- **Coyote Time:** Forgiving jump window after leaving ground

**Input Scheme:**
| Input | Action |
|-------|--------|
| WASD | Movement |
| Space | Jump |
| Left Shift | Sprint |
| Left Ctrl | Toggle Crouch |
| V | Toggle FP/TP View |
| Mouse | Look around |

**Integration:**
- Pauses input during dialogue (via DialogueManager)
- Yields camera control to CameraFocusController when needed

---

### 4. Animation System

**Purpose:** Bridges PlayerController state to Animator parameters.

**Components:**
- `PlayerAnimationController.cs` - Reads controller state, sets animator params

**Animator Parameters:**
| Parameter | Type | Purpose |
|-----------|------|---------|
| Speed | Float | Movement speed (0-1) |
| IsGrounded | Bool | On ground state |
| IsCrouched | Bool | Crouch state |
| Jump | Trigger | Fire jump animation |

**Supported Animations:**
- Idle
- Walk
- Run
- Jump
- Crouch Idle
- Crouch Walk

---

### 5. Inventory System

**Purpose:** Manages collected items with hotbar quick-access and detailed grid view.

**Components:**
- `InventoryManager.cs` - Singleton managing item collection
- `InventoryUIController.cs` - UI controller for hotbar and grid
- `InventorySlotUI.cs` - Individual slot behavior
- `ItemData.cs` - ScriptableObject defining item properties

**Features:**
- **Hotbar:** 3-4 quick-access slots, scroll wheel selection
- **Grid View:** Press Tab for full inventory
- **Detail Panel:** Hover over items to see name, description, properties
- **Categories:** Key items, consumables, documents, etc.

**Item Properties:**
- Name and description
- Icon sprite
- Category
- Stackable flag
- Custom properties

---

### 6. UI System

**Purpose:** All user interface elements for gameplay.

**Components:**
- `DialogueUIPanel.cs` - Dialogue display and choices
- `ImpactNotificationUI.cs` - "X will remember that" notifications
- `StatsDisplayUI.cs` - Player stats visualization
- `InventoryUIController.cs` - Inventory interface

**Dialogue UI Features:**
- Speaker portrait and name
- Typewriter text effect (optional)
- Choice buttons with dynamic visibility
- Continue indicator for click-to-advance

**Impact Notifications:**
- Slide-in animation from screen edge
- Variable display time
- Stacking for multiple rapid impacts
- Categories: Relationship, Stat Change, Discovery, Story

**Stats Display:**
- Real-time stat bars
- Located in inventory or always-visible HUD
- Color-coded by stat type

---

### 7. Interaction System

**Purpose:** Handles player interaction with world objects.

**Components:**
- `InteractionManager.cs` - Detects and processes interactions
- `Interactable.cs` - Base class for interactable objects
- `ItemPickup.cs` - Collectible items
- `DialogueTrigger.cs` - NPC/object dialogue triggers

**Features:**
- Raycast-based detection
- Interaction prompt UI
- E key to interact
- Different interaction types (Examine, Pickup, Talk, Use)

---

### 8. Level Management System

**Purpose:** Manages scene flow and dialogue segmentation across locations.

**Components:**
- `LevelManager.cs` - Singleton tracking scene/segment state
- `DialogueSegmentTrigger.cs` - Location-based dialogue triggers
- `SceneTransition.cs` - Handles scene loading

**Features:**
- **Segments:** Break dialogue into location-specific chunks
- **Prerequisites:** Require previous segments before triggering
- **Completion Tracking:** Mark segments as done
- **Scene Transitions:** Load new scenes with data persistence

**Segment Flow Example:**
```
Segment 1: jungle_awakening (at spawn)
    ↓ Player walks to tree
Segment 2: jungle_tree (near twisted tree)
    ↓ Player walks to portal
Segment 3: jungle_portal (at gate)
```

---

### 9. Camera Focus System

**Purpose:** Cinematic camera control during dialogue sequences.

**Components:**
- `CameraFocusController.cs` - Manages camera movement to focus points
- `CameraFocusPoint.cs` - Marker for camera positions

**Features:**
- Camera physically moves to focus point position
- Focus point rotation defines view direction
- Smooth position and rotation transitions
- Optional limited player look while focused
- Auto-releases when dialogue ends
- Triggered via dialogue commands (`cam:pointname`)

**Usage:**
1. Place empty GameObjects at desired camera positions
2. Rotate to set view direction (blue gizmo shows direction)
3. Add CameraFocusPoint component with unique ID
4. Use `cam:id` in dialogue commands

---

## 🏛️ Architecture Patterns

### Singleton Pattern
Used for global managers that need single instance access:
- `GameState.Instance`
- `DialogueManager.Instance`
- `InventoryManager.Instance`
- `LevelManager.Instance`
- `CameraFocusController.Instance`

### ScriptableObject Data
All game content defined as ScriptableObjects:
- `DialogueTree` - Contains dialogue flows
- `DialogueNode` - Individual dialogue moments
- `DialogueChoice` - Player options
- `ChoiceImpact` - Consequences
- `ItemData` - Item definitions
- `CharacterData` - Character info and portraits

### Event-Driven Communication
Systems communicate via C# events for loose coupling:
```csharp
// Publisher
public event Action OnDialogueEnded;

// Subscriber
DialogueManager.Instance.OnDialogueEnded += HandleDialogueEnd;
```

### Command Pattern
Dialogue nodes execute commands as strings:
```
item:key → Parsed and executed by DialogueManager
flag:found_secret → Sets GameState flag
cam:dramatic_angle → Triggers camera focus
```

---

## 📁 File Structure

```
Assets/
├── Documents/
│   ├── Training Docs/
│   │   ├── SYSTEMS-SETUP.md
│   │   ├── DIALOGUE-SEGMENTS-SETUP.md
│   │   ├── CAMERA-FOCUS-SETUP.md
│   │   └── TEST-DATA.md
│   └── PROJECT-OVERVIEW.md (this file)
│
├── Scripts/
│   ├── Camera/
│   │   ├── CameraFocusController.cs
│   │   └── CameraFocusPoint.cs
│   │
│   ├── Data/
│   │   ├── CharacterData.cs
│   │   ├── DialogueTree.cs
│   │   ├── DialogueNode.cs
│   │   ├── DialogueChoice.cs
│   │   ├── ChoiceImpact.cs
│   │   └── ItemData.cs
│   │
│   ├── Gameplay/
│   │   ├── PlayerController.cs
│   │   └── PlayerAnimationController.cs
│   │
│   ├── Interaction/
│   │   ├── InteractionManager.cs
│   │   ├── Interactable.cs
│   │   ├── ItemPickup.cs
│   │   ├── DialogueTrigger.cs
│   │   └── DialogueSegmentTrigger.cs
│   │
│   ├── Runtime/
│   │   ├── GameState.cs
│   │   ├── DialogueManager.cs
│   │   ├── InventoryManager.cs
│   │   └── LevelManager.cs
│   │
│   └── UI/
│       ├── DialogueUIPanel.cs
│       ├── ImpactNotificationUI.cs
│       ├── StatsDisplayUI.cs
│       ├── InventoryUIController.cs
│       └── InventorySlotUI.cs
│
├── ScriptableObjects/
│   ├── Characters/
│   ├── Dialogues/
│   │   ├── Trees/
│   │   ├── Nodes/
│   │   └── Choices/
│   └── Items/
│
├── Prefabs/
│   ├── UI/
│   └── Interactables/
│
└── Scenes/
    └── TestScene.unity
```

---

## 🔗 Integration Map

```
┌─────────────────────────────────────────────────────────────────┐
│                         PLAYER INPUT                            │
└─────────────────────────────────────────────────────────────────┘
                              │
          ┌───────────────────┼───────────────────┐
          ▼                   ▼                   ▼
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│ PlayerController │  │ InteractionMgr  │  │  Inventory UI   │
│  (Movement)      │  │  (E to interact)│  │  (Tab to open)  │
└────────┬────────┘  └────────┬────────┘  └────────┬────────┘
         │                    │                    │
         ▼                    ▼                    ▼
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│ AnimController   │  │ DialogueTrigger │  │ InventoryMgr    │
│ (Animations)     │  │ SegmentTrigger  │  │ (Item storage)  │
└─────────────────┘  └────────┬────────┘  └─────────────────┘
                              │
                              ▼
                    ┌─────────────────┐
                    │ DialogueManager │◄────────┐
                    │ (Dialogue flow) │         │
                    └────────┬────────┘         │
                             │                  │
         ┌───────────────────┼───────────────────┐
         ▼                   ▼                   ▼
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│ DialogueUIPanel │  │   GameState     │  │ CameraFocusCtr  │
│ (Display text)  │  │ (Flags/Vars)    │  │ (Camera moves)  │
└────────┬────────┘  └────────┬────────┘  └─────────────────┘
         │                    │
         ▼                    ▼
┌─────────────────┐  ┌─────────────────┐
│ ImpactNotify UI │  │ StatsDisplay UI │
│ ("X remembers") │  │ (Trust, etc.)   │
└─────────────────┘  └─────────────────┘
```

---

## ✅ What's Working

### Fully Functional:
- [x] Complete dialogue flow with branching
- [x] Conditional choices based on game state
- [x] Dialogue commands (items, flags, variables, camera)
- [x] Player movement (walk, run, crouch, jump)
- [x] First person / Third person toggle
- [x] Animation integration
- [x] Inventory hotbar and grid view
- [x] Item hover details
- [x] Impact notifications ("X will remember that")
- [x] Stats display (trust, courage, insanity)
- [x] Scene/segment management
- [x] Dialogue triggers at specific locations
- [x] Cinematic camera focus points
- [x] Interaction system (E to interact)

### Tested Scenarios:
- [x] Full dialogue tree playthrough
- [x] Multiple choice branches
- [x] Segment transitions
- [x] Item pickup and inventory display
- [x] Camera focus during dialogue
- [x] Stats modification via choices

---

## 📝 Remaining Tasks

### Content Creation:
- [ ] Write full prologue dialogue (15 minutes)
- [ ] Create character portraits
- [ ] Design jungle environment
- [ ] Place all dialogue triggers
- [ ] Configure camera focus points for key moments

### Polish:
- [ ] Add sound effects
- [ ] Add music/ambience
- [ ] Screen transitions/fades
- [ ] Loading screens
- [ ] Main menu

### Optional Enhancements:
- [ ] Save/Load system
- [ ] Settings menu
- [ ] Dialogue history/log
- [ ] Achievement system
- [ ] Multiple endings tracking

---

## 🎮 Quick Test Guide

1. **Open TestScene**
2. **Play the game**
3. **Walk to dialogue triggers** (cubes/NPCs)
4. **Press E** to start dialogue
5. **Click choices** to progress
6. **Press Tab** to open inventory
7. **Observe impact notifications** when making choices

---

## 📚 Related Documentation

- `SYSTEMS-SETUP.md` - Detailed setup guide for each system
- `DIALOGUE-SEGMENTS-SETUP.md` - How to create segmented dialogue flows
- `CAMERA-FOCUS-SETUP.md` - Cinematic camera system guide
- `TEST-DATA.md` - Sample dialogue content for testing

---

*This document tracks the development progress of The Whispering Gate. Update as new systems are added.*



