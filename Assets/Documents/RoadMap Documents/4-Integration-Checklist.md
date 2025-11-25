# Integration Testing & Validation Checklist
## How All 3 Systems Work Together

**For:** Development Team  
**Purpose:** Ensure Dialogue, Character Controller, and Inventory work seamlessly

---

## THE THREE SYSTEMS & HOW THEY INTERACT

**System 1: Character Controller**
- Lets player move around in 3D world
- Pauses during dialogue, resumes after

**System 2: Dialogue System**
- Plays conversations with branching choices
- Tracks variables (courage, trust, sanity)
- Executes commands (give items, set flags)

**System 3: Inventory System**
- Stores items player receives
- Shows items in UI
- Persists across scenes

---

## CRITICAL CONNECTION POINTS

### Connection 1: Dialogue → Player Control

**When:** Dialogue starts  
**What should happen:**
1. DialogueManager emits OnNodeDisplayed event
2. PlayerController receives event → disables input
3. Player cannot move ✓
4. Player clicks through dialogue
5. DialogueManager emits OnDialogueEnded
6. PlayerController re-enables input
7. Player can move again ✓

**Test:**
- [ ] Walk to dialogue trigger
- [ ] Trigger dialogue
- [ ] Try to move → should NOT move
- [ ] Click through
- [ ] Try to move → should move

---

### Connection 2: Dialogue → Inventory

**When:** Node has command `item:journal`  
**What should happen:**
1. DialogueManager.ExecuteCommand("item:journal")
2. InventoryManager.AddItem("journal")
3. InventoryUI updates
4. Icon appears ✓

**Test:**
- [ ] Create node with: Start Commands: ["item:journal"]
- [ ] Trigger dialogue
- [ ] Check Console: "Added: journal" prints ✓
- [ ] Check UI: Icon appears ✓

---

### Connection 3: Dialogue → Game State

**When:** Player makes choice with impact  
**What should happen:**
1. Player clicks: "Be brave"
2. Choice has impact: courage +10
3. GameState.AddInt("courage", 10)
4. Console prints: "[GameState] courage = 10"
5. Variable updated ✓

**Test:**
- [ ] Create choice with impact
- [ ] Make choice
- [ ] Check Console for variable update
- [ ] Verify number increased/decreased ✓

---

### Connection 4: Conditional Choices

**When:** Choice should only appear if condition met  
**What should happen:**
1. courage = 5
2. Choice: "Only if courage >= 30"
3. EvaluateCondition("courage >= 30") returns false
4. Button hidden ✓
5. Set courage = 40
6. Button appears ✓

**Test:**
- [ ] Set courage = 5
- [ ] Create conditional choice
- [ ] Trigger dialogue
- [ ] Choice hidden ✓
- [ ] Set courage = 40
- [ ] Choice appears ✓

---

## SCENE SETUP CHECKLIST

**Hierarchy:**
- [ ] GameManager (with GameState + DialogueManager + InventoryManager scripts)
- [ ] Player (with PlayerController script)
- [ ] Canvas (with DialogueUIPanel script)
- [ ] InventoryUI (displays items)
- [ ] DialogueTrigger (in world for first dialogue)

**In Inspector - GameManager:**
- [ ] GameState script assigned
- [ ] DialogueManager script assigned
- [ ] InventoryManager script assigned
- [ ] All marked DontDestroyOnLoad ✓

**In Inspector - Player:**
- [ ] PlayerController script assigned
- [ ] Rigidbody configured
- [ ] Can move with WASD ✓
- [ ] Can look with mouse ✓

**In Inspector - Canvas:**
- [ ] DialogueUIPanel script assigned
- [ ] All UI elements assigned (portrait, text, choices)
- [ ] Canvas visible ✓

---

## COMPLETE INTEGRATION TEST SEQUENCE

### Test 1: GameState Initialization
```
Expected: GameState exists with default values
1. Hit Play
2. Check Console: "[GameState] courage = 0"
3. Check Console: "[GameState] sanity = 50"
4. ✅ PASS if all default values initialized
```

### Test 2: Dialogue Trigger
```
Expected: Walking to trigger and pressing E starts dialogue
1. Walk to dialogue trigger area
2. Press E
3. ✅ PASS if UI panel fades in and text appears
```

### Test 3: Player Pause
```
Expected: Player cannot move during dialogue
1. Before dialogue: Move with WASD works
2. Start dialogue
3. Try WASD: No movement ✅
4. Close dialogue
5. Try WASD: Movement works ✅
```

### Test 4: Choices Display
```
Expected: Dialogue choices appear as buttons
1. Trigger dialogue with 2+ choices
2. ✅ PASS if 2 buttons appear with choice text
3. Hover over button: Highlight works ✓
4. Click button: Next dialogue plays ✓
```

### Test 5: Variable Updates
```
Expected: Choosing option updates variables
1. Before: Check Console for current courage value
2. Make choice with impact: courage +10
3. After: Check Console for new value (should be +10)
4. ✅ PASS if variable incremented correctly
```

### Test 6: Conditional Choice
```
Expected: Choices hide/show based on conditions
1. Set courage = 5
2. Trigger dialogue with conditional choice
3. ✅ PASS if choice is hidden
4. Set courage = 50
5. Trigger dialogue again
6. ✅ PASS if choice is now visible
```

### Test 7: Item Giving
```
Expected: Commands give items to player
1. Create dialogue node with: "item:journal"
2. Trigger node
3. Check Console: "Added: journal"
4. Check InventoryUI: Icon appears ✅
```

### Test 8: End-to-End Flow
```
Expected: Full dialogue flow with multiple systems
1. Walk to trigger → P dialog starts → Player paused
2. See choices → Choice hidden due to low courage
3. Choose brave option → Courage +10
4. Receive item → Icon appears in inventory
5. Dialogue ends → Player can move
6. ✅ PASS if all systems work together
```

---

## PERFORMANCE CHECKLIST

- [ ] FPS: Maintain 60+ FPS (check Game window)
- [ ] No lag when selecting dialogue options
- [ ] Dialogue text appears instantly
- [ ] UI transitions are smooth
- [ ] Memory: Under 512 MB (Profiler → Memory)
- [ ] No warnings in Console (only logs)

---

## BUILD & DEPLOYMENT

**Creating Final Build:**
1. File → Build Settings
2. Platform: Windows
3. Add scene: Prologue_JungleAwakens
4. Build to: "Builds/FYP_Demo"

**Test Build:**
1. Run .exe from Builds folder
2. Play full prologue (~15 min)
3. Verify all systems work
4. Test on different computer

**Backup:**
1. Copy Builds/ folder to USB
2. Copy entire project to USB
3. Upload to Google Drive/OneDrive

---

## TROUBLESHOOTING

| Issue | Solution |
|-------|----------|
| Dialogue Manager is null | Create GameManager with DialogueManager script |
| Choices don't appear | Check DialogueUIPanel is subscribed to OnNodeDisplayed |
| Player can move during dialogue | Check PlayerController.SetInputEnabled() is called |
| Variables don't update | Check variable name spelling (case-sensitive) |
| Items not received | Check InventoryManager exists and command syntax |
| Choices always visible | Check conditional logic syntax |
| Game stutters | Check Profiler for expensive operations |

---

## PRE-DEFENSE VALIDATION

- [ ] All systems integrated and tested
- [ ] 15-minute demo playable
- [ ] No critical bugs
- [ ] Build works on clean machine
- [ ] Backup copies created
- [ ] Team briefed on demo flow

---

**All systems connected. Ready for defense.**
