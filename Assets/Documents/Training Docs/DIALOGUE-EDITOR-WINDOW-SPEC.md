# DIALOGUE SYSTEM EDITOR WINDOW SPECIFICATION
## Professional Development Workflow for Content Creation

**For:** Cursor AI Code Generation  
**Purpose:** Design a dedicated editor window for creating dialogue without coding  
**Status:** Specification Ready

---

## OVERVIEW

Instead of creating dialogue in raw ScriptableObjects, create a professional editor window that allows non-programmers to build dialogue trees graphically, similar to:
- Unreal's Dialogue System
- Narrative Design tools
- Node-based editors

**Target Workflow:**
1. Open editor window: Window → Whispering Gate → Dialogue System
2. Create new Dialogue Tree (container)
3. Add nodes visually
4. Connect nodes by dragging
5. Edit node properties in inspector panel
6. Test dialogue in real-time (play mode)
7. Export/save as ScriptableObjects

---

## EDITOR WINDOW LAYOUT

### Main Viewport (Left 70%)
- **Graph Canvas:** Shows all dialogue nodes as boxes
- **Connections:** Lines connecting nodes show choice flow
- **Color Coding:** Different colors for scenes/character types
- **Grid Background:** Optional grid for alignment
- **Zoom Controls:** Scroll to zoom, middle-mouse to pan
- **Selection:** Click node to select, highlight in orange

**Context Menu (Right-click):**
```
└─ Create Node
   ├─ Regular Node (text + choices)
   ├─ Start Node (entry point)
   └─ End Node (conclusion)

└─ Delete Node
└─ Duplicate Node
└─ Copy Subtree
└─ Convert to Trigger
└─ Search Nodes
```

### Inspector Panel (Right 30%)
- **Node Properties** (when node selected):
  ```
  Node ID: [Text field]
  Speaker: [Character dropdown]
  Line Text: [Text area - multiline]
  Voice Clip: [Audio file selector]
  
  CHOICES:
  ├─ Choice 1: [Text] → [Node selector]
  │  ├─ Condition: [if conditions/always]
  │  └─ Impacts: [courage +10] [var:investigation+5]
  ├─ Choice 2: [Text] → [Node selector]
  │  ├─ Condition: [...]
  │  └─ Impacts: [...]
  └─ [+ Add Choice]
  
  COMMANDS:
  ├─ On Start: [item:journal] [flag:saw_dolls]
  ├─ On End: [var:courage+5]
  └─ [+ Add Command]
  
  GENERAL:
  ├─ Is End Node: [Toggle]
  ├─ Auto Advance: [Toggle]
  ├─ Display Duration: [0.0 = wait for input]
  └─ [Preview] [Delete]
  ```

- **Tree Properties** (when no node selected):
  ```
  Tree ID: [Text]
  Tree Title: [Text]
  Start Node: [Node selector / shows current]
  Typewriter Speed: [Slider 0.01 - 0.1]
  Auto Advance Conditions: [Dropdown options]
  
  STATISTICS:
  ├─ Total Nodes: 47
  ├─ End Nodes: 5
  ├─ Branching Paths: 12
  └─ Average Choice Depth: 4.2
  ```

### Bottom Panel (Status Bar)
```
├─ Search: [🔍 Search nodes...]
├─ Variable Viewer: [Expand to show all variables]
├─ Condition Tester: [Test "courage >= 30" returns TRUE]
├─ Preview: [Play Dialogue] [Reset Variables] [Close Preview]
└─ Info: [47 nodes | 5 endings | Unsaved Changes]
```

---

## CORE FEATURES

### Feature 1: Node Creation & Editing

**Creating Node:**
1. Right-click canvas → Create Node
2. Popup appears with defaults
3. Click on canvas, node created at mouse position
4. Automatically selects node, focus to ID field

**Editing Node Properties:**
1. Click node in graph
2. Properties appear in right panel
3. Edit any field
4. Changes apply immediately (visual feedback)

**Deleting Node:**
1. Select node
2. Press Delete key OR right-click → Delete
3. Confirms: "Delete this node and its 3 outgoing connections?"

---

### Feature 2: Connection Management

**Creating Connection (Choice):**
1. Click node A
2. In Inspector → Choices section → [+Add Choice]
3. A new choice row appears
4. In "→ Next Node": drag-select node B
5. Connection visualized in graph as line A→B

**Editing Connection:**
1. Click choice in Inspector panel
2. Edit: Choice Text, Condition, Impacts
3. Impacts shown as tags: [courage +10] [item:journal]

**Deleting Connection:**
1. In Choice row, click [X] button
2. Connection removed from graph

**Visual Feedback:**
- Selected nodes: Orange outline
- Hovered connections: Highlight green
- Conditional choices: Dashed line (vs solid for always-show)
- Start node: Green background
- End nodes: Red background

---

### Feature 3: Variable & Condition System

**Variable Viewer Panel:**
```
VARIABLES IN THIS TREE:
├─ courage: [Min:0] [Max:100] [Default:0] [Usage:12]
├─ sanity: [Min:0] [Max:100] [Default:50] [Usage:8]
├─ investigation: [Min:0] [Max:100] [Default:0] [Usage:5]
├─ trust_writer: [Min:0] [Max:100] [Default:0] [Usage:4]
└─ [+ Add New Variable]

FLAGS IN THIS TREE:
├─ journal_found: [Usage:3]
├─ saw_dolls: [Usage:2]
└─ [+ Add New Flag]
```

**Condition Testing Panel:**
```
Test Condition:
[courage >= 30] [Test] → Result: TRUE ✓

(In play mode, shows actual values:
 courage = 45, so 45 >= 30 = TRUE)
```

---

### Feature 4: Search & Navigation

**Search Box (Ctrl+F or click search):**
```
[🔍 Search... (47 results)]

Results:
├─ Node: jungle_1 "I opened my eyes..."
├─ Node: jungle_2 "The twisted tree..."
├─ Choice: "Follow the scream" (in jungle_1)
├─ Command: item:journal (in jungle_3)
└─ Variable: courage (used in 12 nodes)

Click result → Jump to that node
```

**Go To Node (Ctrl+G):**
```
[Go To Node ID: jungle_1] [Go]
→ Zooms to jungle_1 and selects it
```

---

### Feature 5: Tree Statistics & Analytics

**Automatic Analysis:**
```
TREE STATS:
├─ Total Nodes: 47
├─ Start Nodes: 1
├─ End Nodes: 5
├─ Total Choices: 89
├─ Branching Factor: 1.9
├─ Max Depth: 8
├─ Unreachable Nodes: 0 ✓

ENDING PATHS:
├─ Path 1: [courage >= 70] "GOOD_SAVED"
├─ Path 2: [sanity < 30] "BAD_INSANE"
├─ Path 3: [courage < 30] "BAD_FLEE"
├─ Path 4: [else] "MIXED_UNCERTAIN"
└─ Path 5: [courage >= 50 && trust_writer >= 80] "GOOD_CONTROL"

VARIABLE USAGE:
├─ courage: 12 nodes (impacts), 8 conditions (choices)
├─ sanity: 8 nodes, 6 conditions
├─ investigation: 5 nodes, 3 conditions
└─ trust_writer: 4 nodes, 5 conditions

POTENTIAL ISSUES:
├─ ⚠ 2 nodes have no outgoing connections (might be intentional ends)
├─ ✓ All nodes reachable from start
├─ ✓ No circular paths
└─ ✓ All conditions parseable
```

---

### Feature 6: Real-Time Preview

**Play Dialogue Preview (in Editor play mode):**
1. Select tree
2. Click [Preview] button
3. Dialogue UI shows in center of viewport
4. Can click through dialogue
5. Variables track in real-time: "courage 50 → 60"
6. Conditions show as enabled/disabled: "[courage >= 30] ✓"
7. Click [Reset] to start over

**Testing Workflow:**
1. Build dialogue tree
2. Hit Play
3. Open Dialogue editor
4. Preview dialogue
5. Make notes: "Choice hidden when it should show"
6. Stop Play, edit condition
7. Play again, verify fix

---

## IMPLEMENTATION ARCHITECTURE

### Core Components

**DialogueEditorWindow.cs** (Editor script)
- Main window class
- Layout management (viewport + inspector + bottom panels)
- Event handling (mouse clicks, drag operations)
- GUI drawing

**DialogueGraphView.cs** (Editor helper)
- Graph rendering (nodes, connections)
- Node positioning (graph layout algorithm)
- Zoom/pan handling
- Selection highlighting

**DialogueInspectorPanel.cs** (Editor helper)
- Properties display for selected node
- Field editors (text, dropdowns, sliders)
- Impacts display and editor

**DialoguePreviewSystem.cs** (Runtime + Editor)
- Simulates dialogue playback in editor play mode
- Shows variable changes in real-time
- Tests conditions as they're evaluated

### Data Flow

```
User Action (click, drag)
    ↓
DialogueEditorWindow receives input
    ↓
Updates selected node/connection
    ↓
Modifies DialogueTree ScriptableObject
    ↓
Unity serializes changes
    ↓
Graph redraws to show changes
    ↓
Inspector panel updates to show properties
```

---

## USER WORKFLOWS

### Workflow 1: Creating a Scene (Example: Jungle Scene)

```
1. Open editor: Window → Whispering Gate → Dialogue Editor
2. Create new Tree: File → New Tree
3. Name: "Tree_JungleAwakens"
4. Right-click canvas → Create Start Node
5. Edit node:
   - ID: jungle_intro
   - Speaker: Protagonist
   - Text: "I opened my eyes. The sky was bleeding red..."
6. [Add Choice] "Follow the scream"
   - Next Node: [create new] jungle_2
7. [Add Choice] "Wait and observe"
   - Next Node: [create new] jungle_2b
   - Impacts: investigation +10
8. Create jungle_2 node, add more choices
9. Connect to jungle_3, jungle_4, etc.
10. Test in Preview
11. Save when complete
```

### Workflow 2: Adding Conditional Choice

```
1. Select node in graph
2. In Inspector, scroll to Choices
3. Find "Follow the scream" choice
4. Click on the choice row
5. In "Condition" field: "courage >= 30"
6. Visual feedback: Connection line becomes dashed
7. Test in Preview:
   - Set courage = 20 → Choice hidden ✓
   - Set courage = 50 → Choice shows ✓
```

### Workflow 3: Debugging Unreachable Node

```
1. In Tree Stats: See "2 Unreachable Nodes"
2. Click on warning
3. Editor highlights disconnected node
4. Can drag it to connect, or delete if intentional
```

---

## TECHNICAL IMPLEMENTATION NOTES FOR CURSOR AI

### When Generating DialogueEditorWindow Code:

**Requirements:**
1. Inherits from EditorWindow
2. Uses OnGUI() for drawing
3. Stores SelectedNode and SelectedTree references
4. Handles mouse input (click, drag)
5. Communicates with DialogueTreeScriptable Objects
6. Saves changes automatically to assets

**Key Methods Needed:**
```csharp
public class DialogueEditorWindow : EditorWindow
{
    private void DrawGraphViewport()      // Render nodes/lines
    private void DrawInspectorPanel()     // Show properties
    private void DrawStatusBar()           // Show stats
    private void HandleMouseInput()        // Click/drag events
    private void SerializeChanges()        // Save to ScriptableObject
    private void OnPlayModeStateChanged()  // Handle play/stop
}
```

**Dependencies:**
- DialogueTree (ScriptableObject to edit)
- DialogueNode (data class)
- DialogueChoice (data class)

---

## SUCCESS CRITERIA

✅ Non-programmers can create full dialogue trees without coding  
✅ Visual representation clear (nodes, connections, colors)  
✅ Changes save automatically to ScriptableObjects  
✅ Real-time preview shows what players see  
✅ Variable tracking built-in  
✅ Conditions testable within editor  
✅ Search and navigation fast and easy  
✅ Professional appearance (similar to industry tools)  

---

**This specification allows Cursor AI to generate a production-ready dialogue editor window. Provide this to Cursor when requesting the editor code.**
