# Dialogue Editor Window - Testing Guide
## How to Use and Test the Visual Dialogue Editor

**Status:** Ready for Testing  
**Location:** `Window → Whispering Gate → Dialogue Editor`

---

## QUICK START (5 Minutes)

### Step 1: Open the Editor
1. In Unity Editor, go to **Window → Whispering Gate → Dialogue Editor**
2. A new window opens with empty state

### Step 2: Create a New Dialogue Tree
1. Click **"New Tree"** button in toolbar
2. Save dialog appears → Name it `TestTree` and save in `Assets/Dialogue/Trees/`
3. Tree is now loaded in editor

### Step 3: Create Your First Node
1. **Right-click** in the graph canvas area
2. Select **"Create Node"** from context menu
3. Save dialog appears → Name it `Node_Start` and save in `Assets/Dialogue/Nodes/`
4. Node appears as a green box (start nodes are green)

### Step 4: Edit Node Properties
1. **Click** on the node to select it
2. Inspector panel (right side) shows node properties
3. Fill in:
   - **Node ID:** `start_1`
   - **Line Text:** `"I opened my eyes. The sky was bleeding red..."`
   - (Speaker can be set later after creating Character assets)

### Step 5: Add a Choice
1. In Inspector panel, scroll to **"Choices"** section
2. Click **"Add Choice"** button
3. Fill in:
   - **Text:** `"Follow the scream"`
   - **Next Node:** (leave empty for now, we'll create it next)
4. Click **"Add Impact"** under the choice
   - **Variable Name:** `courage`
   - **Value Change:** `10`

### Step 6: Create Second Node
1. Right-click canvas → **"Create Node"**
2. Save as `Node_2`
3. Edit properties:
   - **Node ID:** `follow_scream`
   - **Line Text:** `"I followed the sound through the twisted trees..."`
4. Go back to first node, select it
5. In the choice's **"Next Node"** field, drag `Node_2` from Project window

### Step 7: Set Start Node
1. Click empty space in graph (deselects node)
2. Inspector shows **"Tree Properties"**
3. Drag `Node_Start` into **"Start Node"** field

### Step 8: Test in Play Mode
1. Make sure your test scene has:
   - `GameState` GameObject
   - `DialogueManager` GameObject
   - `DialogueUIPanel` on Canvas
2. **Enter Play Mode** (Ctrl+P)
3. In Dialogue Editor window, click **"Preview"** button
4. Dialogue should play in-game!

---

## FEATURES TO TEST

### ✅ Node Creation & Editing
- [ ] Create multiple nodes via right-click context menu
- [ ] Click nodes to select and edit in inspector
- [ ] Drag nodes around canvas to reposition
- [ ] Edit node ID, text, speaker, voice clip
- [ ] Toggle "Is End Node" checkbox

### ✅ Choices & Connections
- [ ] Add multiple choices to a node
- [ ] Link choices to next nodes (drag from Project or use object field)
- [ ] Add conditions to choices ("Has Condition" toggle)
- [ ] Add impacts to choices (variable changes)
- [ ] Remove choices with "Remove Choice" button
- [ ] Visual connections appear as lines between nodes

### ✅ Visual Feedback
- [ ] Start nodes appear **green**
- [ ] End nodes appear **red**
- [ ] Selected nodes appear **yellow**
- [ ] Conditional choices show as **yellow lines** (vs white for always-show)
- [ ] Auto-advance connections show as **cyan dashed lines**

### ✅ Tree Management
- [ ] Create new tree via "New Tree" button
- [ ] Load existing tree via "Load Tree" button
- [ ] Drag tree asset into toolbar field
- [ ] Set start node in tree properties
- [ ] View tree statistics (node count, end nodes, etc.)

### ✅ Validation
- [ ] Click **"Validate"** button
- [ ] Should show warnings for:
  - Missing start node
  - Nodes with no ID
  - Nodes with no text
  - Nodes with no outgoing connections (unless end node)

### ✅ Preview System
- [ ] Enter Play Mode
- [ ] Click **"Preview"** button in editor
- [ ] Dialogue plays in-game UI
- [ ] Can click through choices
- [ ] Variables update (check Console logs)

### ✅ Node Deletion
- [ ] Select a node
- [ ] Click **"Delete Node"** button (red button at bottom)
- [ ] Confirm deletion
- [ ] Node removed, connections cleaned up

---

## COMMON WORKFLOWS

### Workflow 1: Create Simple 3-Node Dialogue
```
1. Create Tree → "TestTree"
2. Create Node_1 → Set as Start Node
   - Text: "Hello"
   - Add Choice: "Say Hi" → (will link to Node_2)
3. Create Node_2
   - Text: "You said hi!"
   - Add Choice: "Continue" → (will link to Node_3)
4. Create Node_3
   - Text: "The end"
   - Check "Is End Node"
5. Link choices:
   - Node_1 choice → Node_2
   - Node_2 choice → Node_3
6. Test in Preview
```

### Workflow 2: Add Conditional Choice
```
1. Select node with choice
2. Toggle "Has Condition" on the choice
3. Enter condition: "courage >= 30"
4. Visual: Connection line becomes yellow
5. Test: Set courage = 20 → Choice hidden
6. Test: Set courage = 50 → Choice shows
```

### Workflow 3: Add Commands
```
1. Select node
2. In Inspector, find "Start Commands" section
3. Click + to add command
4. Enter: "item:journal"
5. When node plays, journal added to inventory
```

---

## TROUBLESHOOTING

| Issue | Solution |
|-------|----------|
| Editor window is blank | Click "New Tree" or load existing tree |
| Can't create node | Make sure you right-click in graph canvas area |
| Changes not saving | Unity auto-saves, but check Console for errors |
| Preview doesn't work | Enter Play Mode first, ensure DialogueManager exists |
| Connections not showing | Make sure nodes are linked via "Next Node" field |
| Inspector fields grayed out | Select a node first (click on it in graph) |
| Validation shows errors | Fix missing IDs, text, or connections |

---

## NEXT STEPS

Once you've tested the editor:
1. Create Character Data assets (Protagonist, Alina, Writer)
2. Create full dialogue tree for Scene 1 (Jungle Awakening)
3. Test full flow: trigger → dialogue → choices → impacts
4. Build out remaining scenes (Writer's House, Portal Chamber)
5. Create 15-minute prologue content

---

**The editor is ready for content creation! Start building your dialogue trees.**






