# Scene/Level Management System - Setup Guide

## Overview

The Scene/Level Management System enables you to create segmented dialogue flows where different dialogue sequences trigger at different locations based on player progression. This system tracks completed dialogue segments, manages checkpoints, and handles scene/level transitions.

## Core Components

### 1. LevelManager
**Location:** `Assets/Scripts/Runtime/LevelManager.cs`

Central manager that tracks:
- Completed dialogue segments
- Activated checkpoints
- Current level/scene state
- Level progression

**Setup:**
1. Create an empty GameObject in your scene (e.g., "LevelManager")
2. Add the `LevelManager` component
3. Set the `Current Level ID` (e.g., "prologue_jungle")
4. Enable `Debug Mode` for testing (optional)

**Key Features:**
- `CompleteSegment(string segmentId)` - Marks a dialogue segment as completed
- `IsSegmentCompleted(string segmentId)` - Checks if a segment is done
- `SetCheckpoint(string checkpointId)` - Saves game state at a checkpoint
- `ChangeLevel(string newLevelId)` - Transitions to a new level/area

---

### 2. DialogueSegmentTrigger
**Location:** `Assets/Scripts/Interaction/DialogueSegmentTrigger.cs`

Enhanced dialogue trigger that:
- Tracks dialogue segments automatically
- Checks prerequisites before allowing dialogue
- Supports segment-based progression gates

**Setup:**
1. Create a GameObject with a Collider (set as Trigger)
2. Add the `DialogueSegmentTrigger` component
3. Assign a `Dialogue Tree` asset
4. Set a unique `Segment ID` (e.g., "jungle_awakening")
5. Configure prerequisites (optional):
   - **Required Segments**: Comma-separated list (e.g., "segment1,segment2")
   - **Required Condition**: GameState condition (e.g., "courage >= 30")

**Example Workflow:**
```
Segment 1: "jungle_awakening" (no prerequisites)
Segment 2: "meet_writer" (requires: "jungle_awakening")
Segment 3: "portal_discovery" (requires: "meet_writer" AND "courage >= 20")
```

**Visual Feedback:**
- Optionally assign an `Interaction Prompt UI` GameObject
- It will show/hide based on prerequisites and player proximity

---

### 3. Checkpoint
**Location:** `Assets/Scripts/Gameplay/Checkpoint.cs`

Saves game state when player enters the trigger zone.

**Setup:**
1. Create a GameObject with a Collider (set as Trigger)
2. Add the `Checkpoint` component
3. Set a unique `Checkpoint ID` (e.g., "checkpoint_jungle_start")
4. Optionally assign a `Spawn Point` Transform (defaults to checkpoint position)
5. Enable `Save Player Position` to restore player location on load

**Visual Feedback:**
- Assign an `Activated Indicator` GameObject (e.g., glowing effect)
- It will activate when the checkpoint is reached

**Usage:**
- Checkpoints automatically activate when player enters the trigger
- Use `LevelManager.Instance.LoadCheckpoint(checkpointId)` to restore state

---

### 4. SceneTransition
**Location:** `Assets/Scripts/Gameplay/SceneTransition.cs`

Handles transitions between scenes/areas.

**Setup:**
1. Create a GameObject with a Collider (set as Trigger)
2. Add the `SceneTransition` component
3. Choose transition type:
   - **Load Scene**: Loads a new Unity scene
   - **Change Level**: Changes level ID (same scene, different area)
   - **Teleport Player**: Teleports player to a location (not yet implemented)
4. Configure prerequisites (same as DialogueSegmentTrigger)

**Example:**
- Player completes "jungle_awakening" segment
- Walks to a door trigger
- Door requires "jungle_awakening" segment
- Player presses E → Scene loads or level changes

---

## Complete Setup Example: Segmented Dialogue Flow

### Scenario: 15-Minute Prologue with 3 Locations

**Location 1: Jungle Awakening**
1. Create `DialogueSegmentTrigger` with:
   - Segment ID: "jungle_awakening"
   - Dialogue Tree: "JungleAwakening_Dialogue"
   - No prerequisites

2. Create `Checkpoint` with:
   - Checkpoint ID: "checkpoint_jungle_start"
   - Position at player spawn

**Location 2: Writer's House**
1. Create `DialogueSegmentTrigger` with:
   - Segment ID: "meet_writer"
   - Dialogue Tree: "MeetWriter_Dialogue"
   - Required Segments: "jungle_awakening"

2. Create `SceneTransition` (door to next area) with:
   - Required Segments: "meet_writer"
   - Transition Type: Change Level
   - Target Level ID: "prologue_portal_chamber"

**Location 3: Portal Chamber**
1. Create `DialogueSegmentTrigger` with:
   - Segment ID: "portal_discovery"
   - Dialogue Tree: "PortalDiscovery_Dialogue"
   - Required Segments: "meet_writer"
   - Required Condition: "courage >= 20"

---

## Testing the System

### Test Harness Script
Create a test script to verify functionality:

```csharp
using UnityEngine;
using WhisperingGate.Gameplay;

public class SceneManagementTest : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            // Complete a segment manually
            if (LevelManager.Instance != null)
                LevelManager.Instance.CompleteSegment("jungle_awakening");
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            // Check segment status
            if (LevelManager.Instance != null)
            {
                bool completed = LevelManager.Instance.IsSegmentCompleted("jungle_awakening");
                Debug.Log($"Segment completed: {completed}");
            }
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            // Set checkpoint
            if (LevelManager.Instance != null)
                LevelManager.Instance.SetCheckpoint("test_checkpoint");
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
            // Reset progress
            if (LevelManager.Instance != null)
                LevelManager.Instance.ResetLevelProgress();
        }
    }
}
```

### Testing Checklist
- [ ] LevelManager exists in scene and initializes correctly
- [ ] DialogueSegmentTrigger marks segments as completed
- [ ] Prerequisites block dialogue until conditions are met
- [ ] Checkpoints save and restore game state
- [ ] SceneTransition works with prerequisites
- [ ] Visual feedback (prompts, indicators) show/hide correctly

---

## Integration with Existing Systems

### GameState Integration
The LevelManager automatically sets GameState flags:
- `segment_{segmentId}_completed` - Set to true when segment completes
- `level_{levelId}_completed` - Set to true when level completes

You can use these in dialogue conditions:
- "segment_jungle_awakening_completed"
- "level_prologue_jungle_completed"

### DialogueManager Integration
DialogueSegmentTrigger automatically:
- Pauses player during dialogue
- Marks segment as completed when dialogue ends
- Restores player control after dialogue

### PlayerController Integration
Both DialogueSegmentTrigger and Checkpoint work with PlayerController:
- Automatically pause/resume player input
- No additional setup required

---

## Best Practices

1. **Naming Conventions:**
   - Segment IDs: `{location}_{event}` (e.g., "jungle_awakening", "house_meet_writer")
   - Checkpoint IDs: `checkpoint_{location}_{number}` (e.g., "checkpoint_jungle_01")
   - Level IDs: `{chapter}_{location}` (e.g., "prologue_jungle", "prologue_house")

2. **Prerequisites:**
   - Keep prerequisite chains simple (max 2-3 levels deep)
   - Use GameState conditions for dynamic gating (stats, flags)
   - Use segment prerequisites for narrative flow

3. **Checkpoints:**
   - Place checkpoints at major story beats
   - Don't place too many (every 2-3 minutes of gameplay)
   - Test checkpoint restoration thoroughly

4. **Performance:**
   - LevelManager uses DontDestroyOnLoad (persists across scenes)
   - Segment tracking is lightweight (HashSet lookups)
   - Prerequisites are checked periodically (not every frame)

---

## Troubleshooting

**Issue: Dialogue doesn't trigger**
- Check prerequisites are met (enable Debug Mode in LevelManager)
- Verify DialogueManager exists in scene
- Check DialogueSegmentTrigger has a valid Dialogue Tree assigned

**Issue: Segments not completing**
- Verify LevelManager exists in scene
- Check segment ID is not empty
- Enable Debug Mode to see completion logs

**Issue: Checkpoints not saving**
- Verify LevelManager exists
- Check checkpoint ID is unique
- Check PlayerPrefs are enabled (they should be by default)

**Issue: SceneTransition not working**
- Verify prerequisites are met
- Check target scene/level ID is correct
- Ensure SceneTransition has a Collider set as Trigger

---

## Next Steps

1. Set up LevelManager in your main scene
2. Create dialogue segments using DialogueSegmentTrigger
3. Place checkpoints at key locations
4. Test the segmented flow
5. Create the 15-minute prologue content using this system!

For questions or issues, refer to the code comments in each script or check the Unity Console for debug messages.


