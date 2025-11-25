# The Whispering Gate - 16-Day Implementation Roadmap
## Step-by-Step Execution Plan (Nov 25 - Dec 11)

**Target:** 15-min playable prologue + 3 polished systems  
**Defense Date:** December 11, 2025  
**Current Date:** November 25, 2025  
**Available Time:** 16 days

---

## PHASE 0: SETUP & FOUNDATION (Days 1-2)
**Duration:** Tuesday-Wednesday (Nov 25-26)  
**Owner:** You (solo dev)  
**Milestone:** All systems compile, GameState tracks variables

### Day 1 (Nov 25)

**MORNING (3-9am - 6 hours):**

1. **Project Setup**
   - [ ] Create new Unity 2021.3+ project
   - [ ] Create folder structure:
     ```
     Assets/
     ├── Scripts/
     │   ├── Data/
     │   ├── Runtime/
     │   ├── UI/
     │   ├── Interaction/
     │   ├── Gameplay/
     │   └── Editor/
     ├── Prefabs/
     ├── Scenes/
     └── Resources/
     ```
   - [ ] Create empty scene: `Prologue_JungleAwakens.unity`
   - **Time: 30 min**

2. **Copy Core Scripts**
   - [ ] Copy all C# scripts from Document 1 into correct folders
   - [ ] **DO NOT CREATE ASSETS YET** - just code files
   - **Time: 1 hour**

3. **First Compile Test**
   - [ ] Go to Unity, let it compile
   - [ ] Fix any namespace errors
   - [ ] Verify no red errors in Console
   - **Status:** ✅ If green, continue. ❌ If red, debug
   - **Time: 30 min**

**AFTERNOON (9am-6pm - 9 hours):**

4. **UI System Setup**
   - [ ] Create Canvas in scene
   - [ ] Add child elements for dialogue UI
   - [ ] Add DialogueUIPanel script
   - [ ] Assign UI elements in Inspector
   - **Time: 1.5 hours**

5. **Scene Setup - Managers**
   - [ ] Create empty GameObject: "GameManager"
   - [ ] Add GameState + DialogueManager + InventoryManager scripts
   - [ ] Mark as DontDestroyOnLoad
   - [ ] **First Play Test:** No errors, GameState initialized
   - **Time: 30 min**

6. **Create Character Assets**
   - [ ] Right-click → Create → Whispering Gate → Character Data
   - [ ] Create 4 characters: Protagonist, Alina, Writer, Portal Voice
   - [ ] Fill DisplayName and ID only
   - **Time: 30 min**

7. **Create First Dialogue Node**
   - [ ] Create test node: "Where... am I?"
   - [ ] No choices, IsEndNode: true
   - **Time: 15 min**

8. **Create Dialogue Tree**
   - [ ] Create: "Tree_JungleAwakens.asset"
   - [ ] Link to first node
   - **Time: 10 min**

9. **Manual Test**
   - [ ] Create test script to trigger dialogue with T key
   - [ ] Hit Play → Press T → Dialogue appears
   - **Status:** ✅ If works, continue. ❌ If doesn't, debug
   - **Time: 45 min**

**END OF DAY 1:**
- ✅ All systems compile
- ✅ GameState initializes
- ✅ DialogueManager can start dialogue
- ✅ UI displays text

---

### Day 2 (Nov 26)

**MORNING (6am-12pm - 6 hours):**

1. **Test Choices System**
   - [ ] Create 2 new nodes (2a, 2b)
   - [ ] Add choices to first node pointing to 2a/2b
   - [ ] Test: Press T → See 2 buttons → Click → See next node
   - **Time: 1.5 hours**

2. **Test Conditional Choices**
   - [ ] Set courage = 5
   - [ ] Create choice: "Only if courage >= 30"
   - [ ] Test: Choice hidden ✅
   - [ ] Set courage = 40
   - [ ] Test: Choice appears ✅
   - **Time: 1 hour**

3. **Test Commands System**
   - [ ] Add command: "item:journal"
   - [ ] Test: Item added to inventory ✅
   - [ ] Add command: "flag:journal_found"
   - [ ] Test: Flag set ✅
   - **Time: 1.5 hours**

4. **Code Review**
   - [ ] Check DialogueManager for typos
   - [ ] Check GameState logic
   - [ ] Run through console logs
   - **Time: 1 hour**

**AFTERNOON (12pm-8pm - 8 hours):**

5. **Character Controller Integration**
   - [ ] Copy PlayerController template
   - [ ] Add to scene
   - [ ] Test WASD movement
   - [ ] Connect pause/resume to DialogueManager events
   - **Time: 2 hours**

6. **Inventory UI**
   - [ ] Create InventoryPanel
   - [ ] Connect to InventoryManager.OnItemAdded
   - [ ] Test: Give journal via dialogue → Icon appears
   - **Time: 1.5 hours**

7. **Save Build State**
   - [ ] Commit to Git: "Milestone 1: Core systems working"
   - **Time: 30 min**

**END OF DAY 2:**
- ✅ All 3 systems integrated
- ✅ Choices modify state
- ✅ Conditional logic works
- ✅ Commands execute
- ✅ Player paused during dialogue

---

## PHASE 1: CHARACTER CONTROLLER (Days 3-5)
**Duration:** Thursday-Saturday (Nov 27-29)

### Day 3-5 Tasks:
- [ ] WASD movement (forward/backward/strafe)
- [ ] Mouse look (Y-axis)
- [ ] Sprint (Shift)
- [ ] Jump (Space)
- [ ] Interaction range detection
- [ ] Add Animator with states: Idle, Walk, Sprint, Jump
- [ ] Sync animations with movement
- [ ] Test all directions work smoothly

**Estimated Time:** 12-15 hours total

---

## PHASE 2: INVENTORY SYSTEM (Days 6-7)
**Duration:** Sunday-Monday (Nov 30 - Dec 1)

### Day 6-7 Tasks:
- [ ] Build InventoryManager fully
- [ ] Create ItemData ScriptableObject
- [ ] Create ~10 item assets (journal, key, photos, etc.)
- [ ] Build InventoryUI with grid display
- [ ] Add hover tooltips
- [ ] Add inspection panel (shows item details)
- [ ] Test full flow: dialogue → item received → visible in inventory

**Estimated Time:** 10-12 hours total

---

## PHASE 3: DIALOGUE CONTENT (Days 8-11)
**Duration:** Tuesday-Friday (Dec 2-5)

### Day 8-9: Create ~15 Dialogue Nodes for Scene 1
- [ ] You create nodes in Editor
- [ ] Test each one works in sequence
- [ ] Verify impacts apply
- [ ] Timeline: ~6 hours each day

### Day 10-11: Create Scene 2 & 3 Nodes
- [ ] ~12 nodes for Writer's House
- [ ] ~8 nodes for Portal Discovery
- [ ] Link all scenes
- [ ] Test full prologue (time it - should be ~15 min)
- [ ] Timeline: ~6 hours each day

---

## PHASE 4: 3D ASSETS & POLISH (Days 12-13)
**Duration:** Saturday-Sunday (Dec 6-7)

- [ ] Model jungle scene (twisted tree, dolls, ground)
- [ ] Model house interior
- [ ] Model portal chamber
- [ ] Lighting setup (red sky, shadows, torch lights)
- [ ] Audio ambience (jungle sounds, whispers, portal hum)

**Estimated Time:** 10 hours total

---

## PHASE 5: INTEGRATION & TESTING (Days 14-15)
**Duration:** Monday-Tuesday (Dec 8-9)

- [ ] Use Document 4 checklist
- [ ] Run all integration tests
- [ ] Fix critical bugs only
- [ ] Create final build
- [ ] Test on different machine

**Estimated Time:** 8-10 hours total

---

## PHASE 6: BUFFER & DEFENSE (Days 16+)
**Duration:** Wednesday onwards (Dec 10+)

- [ ] Day 16 (Dec 10): Final tweaks, re-test
- [ ] Day 17 (Dec 11 MORNING): Team demo rehearsal
- [ ] **DEFENSE:** Dec 11 PM

---

## CRITICAL SUCCESS METRICS

- ✅ By Nov 26: Systems compile, dialogue works
- ✅ By Dec 1: All 3 systems integrated
- ✅ By Dec 5: Full content created
- ✅ By Dec 9: Build tested, no critical bugs
- ✅ By Dec 11: 15-min demo playable, defense ready

---

## DAILY STANDUP TEMPLATE

Each day:
- [ ] GOAL: What you will accomplish today
- [ ] BLOCKERS: Any issues
- [ ] 8am: Start on X
- [ ] 12pm: Checkpoint - done with Y?
- [ ] 6pm: End of day - X working?

---

## IF YOU FALL BEHIND

**Priority 1 (MUST HAVE):** Dialogue system + 15-min scene

**Priority 2 (SHOULD HAVE):** Controller + Inventory

**Priority 3 (NICE TO HAVE):** Full 3D, audio, polish

Fallback: Show systems in simple test scene, focus on architecture quality.

---

**Next: Document 3 for team asset creation guide**
