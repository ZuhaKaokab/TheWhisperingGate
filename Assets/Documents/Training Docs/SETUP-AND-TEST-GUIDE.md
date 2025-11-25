## System Setup & Verification Guide

This checklist walks through creating the required scene objects, wiring references, and validating each system in isolation before integrating everything. Follow the steps in order; each section builds on the previous one.

---

### 1. Project Preparation
1. Open the Unity project (`TheWhisperingGate`).
2. Create (or open) a test scene named `Systems_Test.unity`.
3. In `Project` view, ensure the following folders exist (create if missing):
   - `Assets/Scripts/Data`
   - `Assets/Scripts/Runtime`
   - `Assets/Scripts/UI`
   - `Assets/Scripts/Interaction`
   - `Assets/Scripts/Gameplay`

---

### 2. GameState Setup & Test
1. Create an empty GameObject named `GameState`.
2. Add the `GameState` component (`Assets/Scripts/Runtime/GameState.cs`).
3. Press Play. Observe Console logs for default variable initialization:
   - Should see entries like `[GameState] courage = 0`.
4. In the Inspector during Play mode:
   - Expand `Default Int Variables` / `Default Bool Variables` to adjust base values (e.g., set `sanity` to 60).
   - Stop Play mode; confirm values persist via serialized lists.
5. (Optional) drop in `GameStateTestHarness` (`Assets/Scripts/Testing/GameStateTestHarness.cs`) on an empty object:
   - Press `1` → adds `intDelta` to `intVariableName`.
   - Press `2` → toggles the bool variable.
   - Press `3` → evaluates `conditionToEvaluate` and logs the result.
6. ✅ Success criteria: No errors in Console, hotkeys update variables and conditions correctly.

---

### 3. DialogueManager Setup
1. Create an empty GameObject named `DialogueManager`.
2. Add the `DialogueManager` component (`Assets/Scripts/Runtime/DialogueManager.cs`).
3. Drag `GameState` GameObject into the hierarchy above `DialogueManager` to ensure initialization order (optional but cleaner).
4. Create simple ScriptableObjects to test:
   - Right-click → `Create/Whispering Gate/Character Data` → set ID = `protagonist`, Display Name = `You`.
   - Right-click → `Create/Whispering Gate/Dialogue Node` (Node_A).
     - Assign Speaker = `Character_Protagonist`.
     - Line Text = `"Test node line"`.
     - `Is End Node` = true.
   - Right-click → `Create/Whispering Gate/Dialogue Tree`.
     - Tree ID = `test_tree`.
     - Start Node = `Node_A`.
5. Attach `DialogueTestHarness` (`Assets/Scripts/Testing/DialogueTestHarness.cs`) to an empty GameObject and assign the test tree.
   - Press Play, hit `T` (default `triggerKey`) to start the dialogue.
6. ✅ Success criteria: Console shows `[DialogueManager] Showing node: Node_A` and no errors; repeated presses re-run the flow.

---

### 4. Dialogue UI Panel
1. Create a Canvas (UI → Canvas) named `DialogueCanvas`.
   - Canvas: Screen Space - Overlay.
   - Add `CanvasScaler` (UI Scale Mode: Scale With Screen Size).
2. Import TextMeshPro (Window → TextMeshPro → Import TMP Essential Resources) if not already done.
3. Inside Canvas, create hierarchy:
   ```
   DialoguePanel (Image & CanvasGroup)
     ├─ Portrait (Image)
     ├─ SpeakerName (TextMeshPro - Text)
     ├─ DialogueText (TextMeshPro - Text)
     ├─ ChoicesContainer (Vertical Layout Group)
     └─ SkipButton (Button → child TextMeshPro - Text = "Skip")
   ```
4. Add `DialogueUIPanel` component to `DialoguePanel` object.
5. Assign serialized fields:
   - `Portrait Image` → Portrait
   - `Speaker Name Text` → SpeakerName
   - `Dialogue Text` → DialogueText
   - `Skip Button` → SkipButton
   - `Choices Container` → ChoicesContainer
   - `Choice Button Prefab` → Create a prefab (`UI/ .prefab`):
     - Button with child TextMeshPro component for the label.
     - Drag prefab into script field.
   - `Panel Canvas Group` → CanvasGroup on DialoguePanel.
6. Press Play, trigger dialogue (key `T`). Verify:
   - Panel fades in.
   - Text typewrites; Skip button fills instantly.
   - Choices appear (if node has choices).
   - Panel fades out on dialogue end.
7. ✅ Success criteria: UI updates, no null-reference errors, choices clickable (if present).

---

### 5. Interaction Layer (DialogueTrigger)
1. In the scene, create a `Cube` or placeholder object to serve as trigger.
2. Add `DialogueTrigger` component.
3. Assign:
   - `Dialogue Tree` → The test tree created earlier.
   - `Interaction Mode` → `OnInteract`.
   - `Single Use` → toggle per need.
4. Ensure the object has a Collider (BoxCollider) with `Is Trigger` checked.
5. Tag your player object as `Player`.
6. Press Play:
   - Move to trigger, press `E`, confirm dialogue starts.
   - If `Pause Player During Dialogue` is true, PlayerController should pause automatically (set up in next section).
7. ✅ Success criteria: Dialogue only triggers when in range; respects single-use toggle.

---

### 6. InventoryManager + UI
1. On `GameManager` object (or a new empty), add `InventoryManager` component.
2. Populate `All Items` list with sample data:
   - Size = 2 (e.g., `journal`, `key`).
   - Provide name, icon, description (icons optional but recommended).
3. Build inventory UI:
   - Create Canvas (can reuse Dialogue canvas or make new `InventoryCanvas`).
   - Panel hierarchy example:
     ```
     InventoryPanel (panelRoot)
       ├─ ScrollView/Content (assign to slotsParent, add GridLayoutGroup)
       ├─ DetailPanel
       │    ├─ DetailIcon (Image)
       │    ├─ DetailName (TMP Text)
       │    └─ DetailDescription (TMP Text)
      HotbarRoot (separate horizontal layout at bottom)
        ├─ HotbarSlot (repeat 4 times) – assign as children of `hotbarParent`
     ```
   - Create slot prefab (`UI/InventorySlot.prefab`):
     - Button → child with Image for icon + TMP Text for item name + optional highlight object.
     - Add `InventorySlotUI` component, assign icon/name/highlight references.
    - Create hotbar slot prefab (can reuse same prefab or simplified version) and assign to `hotbarSlotPrefab`.
   - Add `InventoryUIPanel` component to `InventoryPanel`.
     - Assign `panelRoot`, `slotsParent`, `slotPrefab`, detail UI references, `hotbarParent`, `hotbarSlotPrefab`.
     - Set `toggleKey` (defaults to `Tab`), and optional prev/next hotbar keys (default `Q`/`E`).
4. Add `InventoryTestHarness` (`Assets/Scripts/Testing/InventoryTestHarness.cs`) to any object:
   - `4` adds `addItemId`.
   - `5` removes `removeItemId`.
   - `6` queries inventory for `queryItemId`.
   - Harness auto-subscribes to add/remove events and logs confirmations.
5. Press Play:
   - Use harness keys to add/remove items, confirm slots appear/disappear.
   - Scroll mouse wheel or press `Q/E` to move hotbar highlight; click slots to preview details.
   - Hover over grid items (when panel open) to see detail panel update instantly.
   - Press `Tab` to toggle the full grid panel; hotbar stays visible outside the panel.
6. ✅ Success criteria: Grid shows all items, hotbar mirrors first few items with highlight cycling via scroll/keys, detail info updates on hover/click, UI toggles cleanly.

---

### 7. PlayerController (Hybrid FP/TP)
1. Create a `Player` GameObject:
   - Add components: `CapsuleCollider`, `CharacterController`, `PlayerController`.
   - Remove Rigidbody if it was previously added; CharacterController handles movement now.
2. Create child anchors for camera:
   - Add child `FirstPersonAnchor` (position near eyes / inside head).
   - Add child `ThirdPersonAnchor` (offset behind & above player, e.g., (0, 1.6, -3)).
   - Assign these transforms to the corresponding fields on `PlayerController`.
3. Main Camera:
   - Drag the scene camera into the `playerCamera` field (script will unparent it automatically).
4. Configure controller fields:
   - Adjust walk/sprint speeds as desired.
   - `Toggle View Key` defaults to `V`.
5. Tag Player object as `Player`.
6. Press Play:
   - Move (WASD), sprint (Shift), jump (Space).
   - Move mouse to look around; press `V` to swap between FP and TP views.
   - Camera should smoothly follow anchors and interpolate rotations.
7. Trigger dialogue via `DialogueTrigger` to confirm auto pause/resume still works.
8. ✅ Success criteria: Smooth camera follow, view toggle responsive, movement solid in both modes.

---

### 8. End-to-End Flow Test
1. Combine all pieces in `Systems_Test` scene:
   - `GameState`, `DialogueManager`, `InventoryManager`
   - `Player` with `PlayerController`
   - `DialogueCanvas` with `DialogueUIPanel`
   - `DialogueTrigger` in environment
2. Create a dialogue node with:
   - `Start Commands`: `item:journal`
   - Choice with condition `courage >= 10`.
3. In Play mode:
   - Trigger dialogue, pick choices, observe GameState logs for impacts.
   - Check `InventoryManager` log for `journal`.
   - Confirm conditional choice hides/shows based on `courage`.
4. ✅ Success criteria: All systems interact (GameState, Dialogue, UI, Inventory, Player pause).

---

### 9. Troubleshooting Tips
| Issue | Fix |
|-------|-----|
| `DialogueManager.Instance` null | Ensure DialogueManager prefab is in scene before Play |
| Choices not appearing | Check `Choice Button Prefab` assignment, condition syntax |
| Player still moves during dialogue | Confirm PlayerController subscribed (look for warnings in Console) |
| Inventory not updated | Ensure InventoryManager exists & `item:` command spelled correctly |
| Null reference on UI | Verify all serialized fields assigned (use inspector warnings) |

---

### 10. Suggested Next Steps
1. Duplicate this test scene as a sandbox.
2. Begin authoring real dialogue nodes & trees per design docs.
3. Build editor tooling (Dialogue Editor Window) once runtime is stable.
4. Follow Implementation Roadmap (Document 2) for daily goals.

Keep this guide handy while iterating; check off each section as you validate the corresponding system. Once everything passes, proceed to integrate story content and production assets.

