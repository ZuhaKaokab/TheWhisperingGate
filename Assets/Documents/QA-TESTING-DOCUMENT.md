# The Whispering Gate - QA Testing Document

## Document Info
- **Project:** The Whispering Gate
- **Type:** Functional QA + Regression History
- **Scope:** Prologue gameplay systems (movement, dialogue, inventory, puzzles, saves, journal, flashlight, scene flow)
- **Prepared For:** Game QA submission

---

## Test Environment
- Unity Editor: 2021.3+
- Platform: PC (Windows)
- Input: Keyboard + Mouse
- Build Type: Development build or Play Mode test scene
- Core scenes: `MainMenu`, `GameplayScene`, `GameplayScene2`, `JungleScene`

---

## Test Case Matrix (25 Cases)

### Movement and Camera

| ID | Feature | Test Scenario | Steps | Expected Result | Priority |
|---|---|---|---|---|---|
| TC-001 | Player movement | Walk/strafe/basic motion | Start gameplay, use WASD for 10 seconds | Character moves smoothly in all directions, no stutter | High |
| TC-002 | Sprint | Sprint speed boost | Hold Left Shift while moving forward | Player speed increases while sprint key is held | High |
| TC-003 | Jump | Jump trigger and landing | Press Space on flat ground | Jump triggers once, lands correctly, no infinite air state | High |
| TC-004 | Crouch | Toggle crouch and uncrouch | Press Left Ctrl, then press again | Player height reduces then returns to normal | Medium |
| TC-005 | View mode toggle | FP/TP switch | Press `V` repeatedly while moving | Camera toggles first-person/third-person with no clipping lock | High |
| TC-006 | Input lock during dialogue | Movement lock while dialogue is active | Trigger dialogue and attempt movement/look input | Player movement and camera control are disabled until dialogue ends | High |

### Dialogue and Choice System

| ID | Feature | Test Scenario | Steps | Expected Result | Priority |
|---|---|---|---|---|---|
| TC-007 | Dialogue start trigger | Start dialogue through interaction | Enter trigger range and press `E` | Dialogue UI opens and first node displays | High |
| TC-008 | Choice visibility conditions | Conditional choice filtering | Use a node with condition-locked options | Only choices with true conditions are visible | High |
| TC-009 | Choice selection flow | Branch to next node | Select each visible choice path in separate runs | Dialogue advances to linked node and branch content changes | High |
| TC-010 | End node handling | End node with no choices | Trigger dialogue ending on end node | Dialogue auto closes after configured duration | Medium |
| TC-011 | Segment boundary safety | Choice with null next node | Select a choice configured with null next node | Dialogue ends cleanly without errors or freeze | High |
| TC-012 | Dialogue command - flags/vars | Command execution from nodes | Trigger node with `flag:` and `var:` commands | GameState values update correctly in logs/inspector | High |

### Inventory, Journal, and UI

| ID | Feature | Test Scenario | Steps | Expected Result | Priority |
|---|---|---|---|---|---|
| TC-013 | Item grant via dialogue | Item command integration | Trigger node with `item:<id>` command | Item appears in inventory and optional UI notification appears | High |
| TC-014 | Inventory panel toggle | Open/close inventory | Press Tab to open/close inventory panel | Panel toggles reliably; item slots populate correctly | High |
| TC-015 | Hotbar navigation | Slot cycling | Use mouse wheel and/or assigned keys to change selected slot | Highlight moves correctly and wraps without skipping | Medium |
| TC-016 | Item details display | Hover/click details | Hover or click inventory item slot | Name, icon, and description update to selected item | Medium |
| TC-017 | Journal pickup and open | Acquire and open journal | Trigger journal pickup, then press `J` | Journal opens only after pickup; before pickup it does not open | High |
| TC-018 | Journal unlock by condition | Auto unlock page from GameState | Meet a page unlock condition and open journal | New page unlocks and appears in sorted page list | Medium |

### Puzzle Systems

| ID | Feature | Test Scenario | Steps | Expected Result | Priority |
|---|---|---|---|---|---|
| TC-019 | Grid puzzle start | Puzzle activation | Enter grid puzzle and activate it | Puzzle enters active state and starts tracking tile steps | High |
| TC-020 | Grid puzzle fail/reset | Wrong tile behavior | Step on wrong tile in exact sequence mode | Failure feedback appears; path progress resets | High |
| TC-021 | Grid puzzle solve | Correct path completion | Follow the configured correct path | Puzzle marks solved, locks tiles, executes solved commands | High |
| TC-022 | Rotation puzzle solve mode | Enter/exit solve mode | Interact with puzzle, press escape/tab to exit | Player input toggles correctly; camera focus enters/exits correctly | High |
| TC-023 | Rotation puzzle navigation/rotate | Select and rotate elements | Use arrow/WASD to select, `E/Q` to rotate | Selection updates properly; element rotates and correctness updates | Medium |
| TC-024 | Rotation puzzle completion | Full solve check | Set all elements to solution orientation | Puzzle marks solved once; solved commands trigger once | High |

### Save/Load and Progression

| ID | Feature | Test Scenario | Steps | Expected Result | Priority |
|---|---|---|---|---|---|
| TC-025 | Save/load persistence | Save, quit scene, reload | Save to slot, change scene/restart play, load same slot | Restores player position, GameState, inventory, puzzle solved states, and level progress | Critical |

---

## Old Potential Bug Reports (Already Fixed) - Regression Reference

> Note: These entries are **intentionally framed as old potential bugs** that were identified during earlier QA cycles and are **already fixed**. They are included as regression references only.

### BUG-001 (Potential, Fixed)
- **Title:** Dialogue crossed into next segment unexpectedly
- **Area:** Dialogue segmentation
- **Old Behavior:** Segment 1 continued into Segment 2 because a choice still linked to external nodes.
- **Likely Root Cause:** Choice `Next Node` references were not cleared after splitting trees.
- **Fix Applied:** End node configuration and null-node safety handling in dialogue flow.
- **Regression Test:** TC-011
- **Status:** Fixed

### BUG-002 (Potential, Fixed)
- **Title:** Duplicate impact event invoked twice
- **Area:** Dialogue/UI events
- **Old Behavior:** Impact notification fired twice for one choice.
- **Likely Root Cause:** Duplicate event declaration/subscription.
- **Fix Applied:** Removed duplicate event declaration and cleaned event flow.
- **Regression Test:** TC-012
- **Status:** Fixed

### BUG-003 (Potential, Fixed)
- **Title:** Duplicate cleanup method caused UI unsubscribe issues
- **Area:** Dialogue UI lifecycle
- **Old Behavior:** Inconsistent cleanup due to duplicate destroy/unsubscribe logic.
- **Likely Root Cause:** Multiple lifecycle methods with overlapping unsubscription.
- **Fix Applied:** Consolidated unsubscribe calls into one destroy path.
- **Regression Test:** TC-007, TC-009
- **Status:** Fixed

### BUG-004 (Potential, Fixed)
- **Title:** Player still moving during dialogue
- **Area:** Player control lock
- **Old Behavior:** Player could move/look while dialogue text was on screen.
- **Likely Root Cause:** Input lock not consistently applied on dialogue start.
- **Fix Applied:** Dialogue start/end events now explicitly toggle player input.
- **Regression Test:** TC-006
- **Status:** Fixed

### BUG-005 (Potential, Fixed)
- **Title:** Choice index mismatch after conditional filtering
- **Area:** Dialogue choices
- **Old Behavior:** Selecting visible choice sometimes triggered wrong branch.
- **Likely Root Cause:** Using original choice list index instead of filtered visible list.
- **Fix Applied:** Selection now resolves against visible choices collection.
- **Regression Test:** TC-008, TC-009
- **Status:** Fixed

### BUG-006 (Potential, Fixed)
- **Title:** Save load failed when scene name was invalid
- **Area:** Save/load
- **Old Behavior:** Loading a save from a removed scene caused load interruption.
- **Likely Root Cause:** No scene validity fallback in load flow.
- **Fix Applied:** Added fallback scene validation before loading saved scene name.
- **Regression Test:** TC-025
- **Status:** Fixed

### BUG-007 (Potential, Fixed)
- **Title:** Puzzle solved state not restored after load
- **Area:** Save/load + puzzles
- **Old Behavior:** Solved puzzles returned to unsolved after loading.
- **Likely Root Cause:** Saved solved puzzle IDs were not fully re-applied on load.
- **Fix Applied:** Save manager restore pass applies solved state to both puzzle types.
- **Regression Test:** TC-021, TC-024, TC-025
- **Status:** Fixed

### BUG-008 (Potential, Fixed)
- **Title:** Flashlight state desynced after loading
- **Area:** Flashlight + save/load
- **Old Behavior:** Flashlight ownership or ON/OFF state mismatched after load.
- **Likely Root Cause:** Load sequence did not consistently reapply enabled/on/battery.
- **Fix Applied:** Save data now restores flashlight enabled state and battery values.
- **Regression Test:** TC-025 + flashlight smoke test in gameplay
- **Status:** Fixed

### BUG-009 (Potential, Fixed)
- **Title:** Journal could be opened before pickup
- **Area:** Journal gating
- **Old Behavior:** Journal key opened UI without journal acquisition in some scenes.
- **Likely Root Cause:** Missing has-journal guard in open flow.
- **Fix Applied:** Open action now checks `hasJournal` before allowing UI open.
- **Regression Test:** TC-017
- **Status:** Fixed

### BUG-010 (Potential, Fixed)
- **Title:** Camera focus conflict after rotation puzzle completion
- **Area:** Camera focus + puzzle commands
- **Old Behavior:** Solve-mode camera release overrode scripted solve camera command.
- **Likely Root Cause:** Camera release always executed even when solve command set camera.
- **Fix Applied:** Conditional release only when no `cam:` command is present.
- **Regression Test:** TC-022, TC-024
- **Status:** Fixed

---

## Exit Criteria
- All High/Critical test cases pass.
- No reopened historical fixed bugs.
- No blocker/critical defects in dialogue flow, puzzle completion, or save/load.
- Regression pass confirms no return of BUG-001 through BUG-010 behaviors.

---

## Suggested Execution Order
1. Smoke test movement + dialogue startup (`TC-001` to `TC-009`).
2. Verify puzzle loops (`TC-019` to `TC-024`).
3. Run inventory/journal cases (`TC-013` to `TC-018`).
4. Finish with full persistence check (`TC-025`).
5. Run targeted regressions mapped to BUG-001 to BUG-010.
