# Dialogue Segment Trigger - Troubleshooting Guide

## Quick Checklist

If your DialogueSegmentTrigger isn't working, check these in order:

### 1. Required Managers in Scene
- [ ] **DialogueManager** exists in scene (on a GameObject, usually "GameManager")
- [ ] **LevelManager** exists in scene (on a GameObject, usually "GameManager")
- [ ] **GameState** exists in scene (on a GameObject, usually "GameManager")
- [ ] All three are marked as `DontDestroyOnLoad` (optional but recommended)

**How to check:**
- Look in Hierarchy for a GameObject with these components
- Or check Console for errors like "DialogueManager.Instance is null"

### 2. Player Setup
- [ ] Player GameObject has **"Player" tag** (not "Untagged")
- [ ] Player has a **Collider component** (any type: BoxCollider, CapsuleCollider, etc.)
- [ ] Player Collider is **NOT** set as Trigger (only the dialogue trigger should be a trigger)

**How to check:**
- Select Player GameObject
- Check Tag dropdown (should say "Player")
- Check Inspector for Collider component
- Collider's "Is Trigger" should be **UNCHECKED**

### 3. DialogueSegmentTrigger Setup
- [ ] GameObject has a **Collider component** (BoxCollider, SphereCollider, etc.)
- [ ] Collider's **"Is Trigger" is CHECKED**
- [ ] **Dialogue Tree** is assigned in Inspector
- [ ] **Segment ID** is filled in (e.g., "jungle_awakening")
- [ ] For first segment: **Required Segments** field is **EMPTY**
- [ ] **Interaction Mode** is set correctly:
  - `OnEnter` = triggers automatically when player enters
  - `OnInteract` = requires pressing E key

**How to check:**
- Select the trigger GameObject
- In Inspector, check Collider → "Is Trigger" checkbox
- Check DialogueSegmentTrigger component fields

### 4. Prerequisites (For Segments 2+)
- [ ] **Required Segments** field contains the previous segment ID (e.g., "jungle_awakening")
- [ ] Previous segment has been completed (check LevelManager or use test harness)

**For Segment 1:**
- Required Segments should be **EMPTY**
- Required Condition should be **EMPTY**

**For Segment 2:**
- Required Segments: `jungle_awakening` (or whatever Segment 1's ID is)

**For Segment 3:**
- Required Segments: `meet_writer` (or whatever Segment 2's ID is)

### 5. Dialogue Tree Setup
- [ ] Dialogue Tree asset exists in Project
- [ ] Tree has a **Start Node** assigned
- [ ] Start Node is not null
- [ ] Tree is assigned to DialogueSegmentTrigger component

**How to check:**
- Select Dialogue Tree asset in Project
- Check Inspector → "Start Node" field (should have a node assigned)

---

## Common Issues & Solutions

### Issue: "DialogueManager.Instance is null"
**Solution:**
1. Create empty GameObject named "GameManager"
2. Add `DialogueManager` component
3. The component will automatically become the Instance

### Issue: "LevelManager.Instance is null"
**Solution:**
1. Create empty GameObject named "GameManager" (or use existing)
2. Add `LevelManager` component
3. Set Current Level ID (e.g., "prologue_jungle")

### Issue: "No Collider found"
**Solution:**
1. Select the trigger GameObject
2. Add Component → Physics → Box Collider (or any collider)
3. Check "Is Trigger" checkbox

### Issue: "Player entered trigger but nothing happened"
**Possible causes:**
1. **Prerequisites not met** - Check Console for "Prerequisites not met" message
2. **Interaction Mode is OnInteract** - You need to press E, not just walk in
3. **Dialogue Tree not assigned** - Check Inspector
4. **Single Use already triggered** - Uncheck "Single Use" or reset scene

### Issue: "Pressing E does nothing"
**Possible causes:**
1. **Player not in range** - Make sure you're inside the trigger collider
2. **Prerequisites not met** - Check Console logs
3. **Interaction Mode is OnEnter** - Change to OnInteract
4. **Player not tagged "Player"** - Check Player GameObject tag

### Issue: "Prerequisites not met" (for first segment)
**Solution:**
- Make sure "Required Segments" field is **EMPTY** for the first segment
- Make sure "Required Condition" field is **EMPTY** for the first segment

---

## Debug Tools

### 1. Use Diagnostic Script
1. Create empty GameObject
2. Add `DialogueSegmentTriggerDebug` component
3. Press Play
4. Check Console for diagnostic output
5. Press F6 to re-run diagnostics

### 2. Check Console Logs
The DialogueSegmentTrigger now logs detailed information:
- When player enters/exits trigger
- When prerequisites are checked
- When E key is pressed
- Why dialogue didn't trigger

**Look for these messages:**
- `[DialogueSegmentTrigger] Player entered trigger zone.` ✅ Good
- `[DialogueSegmentTrigger] Prerequisites not met` ❌ Check prerequisites
- `[DialogueSegmentTrigger] E key pressed but prerequisites not met` ❌ Fix prerequisites
- `[DialogueSegmentTrigger] No Collider found` ❌ Add collider

### 3. Test Prerequisites Manually
Use the SceneManagementTestHarness:
1. Add `SceneManagementTestHarness` to a GameObject
2. Press F1 to complete a segment manually
3. Check if next segment becomes available

---

## Step-by-Step Setup Verification

### Step 1: Verify Managers
```
1. Open scene
2. Check Hierarchy for "GameManager" (or similar)
3. Select it
4. In Inspector, verify these components exist:
   - DialogueManager ✅
   - LevelManager ✅
   - GameState ✅
```

### Step 2: Verify Player
```
1. Select Player GameObject
2. Check Tag = "Player" ✅
3. Check for Collider component ✅
4. Collider "Is Trigger" = UNCHECKED ✅
```

### Step 3: Verify Trigger (Segment 1)
```
1. Select trigger GameObject (e.g., "Segment1_Trigger")
2. Check Collider → "Is Trigger" = CHECKED ✅
3. Check DialogueSegmentTrigger component:
   - Dialogue Tree = Tree_JungleAwakening ✅
   - Segment ID = "jungle_awakening" ✅
   - Required Segments = (EMPTY) ✅
   - Required Condition = (EMPTY) ✅
   - Interaction Mode = OnInteract or OnEnter ✅
```

### Step 4: Test
```
1. Press Play
2. Move player into trigger zone
3. If OnInteract: Press E
4. If OnEnter: Dialogue should start automatically
5. Check Console for any error messages
```

---

## Still Not Working?

1. **Check Console** - Look for red error messages
2. **Use Diagnostic Script** - Add DialogueSegmentTriggerDebug component
3. **Verify all checkboxes above** - Go through each item
4. **Test with simple setup** - Create a new trigger with minimal setup:
   - Just a cube with BoxCollider (Is Trigger = true)
   - DialogueSegmentTrigger component
   - Assign a dialogue tree
   - No prerequisites
   - Interaction Mode = OnEnter
   - Walk into it

If the simple setup works, the issue is with your specific configuration (prerequisites, segment IDs, etc.).

---

## Quick Test Setup

Create this minimal test to verify everything works:

1. **Create Test Trigger:**
   - Create Cube GameObject
   - Add BoxCollider → Check "Is Trigger"
   - Add DialogueSegmentTrigger component
   - Assign any dialogue tree
   - Set Segment ID: "test_segment"
   - Leave Required Segments EMPTY
   - Set Interaction Mode: OnEnter

2. **Verify Player:**
   - Tag = "Player"
   - Has Collider (not trigger)

3. **Press Play:**
   - Walk into cube
   - Dialogue should start automatically

If this works, your system is set up correctly. The issue is with your specific segment configuration.


