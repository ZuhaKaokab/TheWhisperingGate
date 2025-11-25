# How Nodes Work in Dialogue Trees
## Understanding the Node-Tree Relationship

**Important Concept:** Nodes are **separate assets** that get **linked together** to form a tree.

---

## HOW IT WORKS

### Nodes Are Independent Assets
- Each `DialogueNode` is a **separate ScriptableObject file** (like `Node_1.asset`, `Node_2.asset`)
- Nodes exist **independently** in your project
- They can be used in **multiple trees** if needed

### Trees Connect Nodes Together
- A `DialogueTree` is a **container** that references nodes
- It has a **Start Node** (the entry point)
- Nodes link to each other via:
  - **Choices** → "Next Node" field
  - **Auto-Advance** → "Next Node If Auto" field

### Visual Representation
- **By Default:** Editor only shows nodes **connected** to the start node (reachable path)
- **"Show All Nodes" Toggle:** Shows ALL nodes in project, even unlinked ones (grayed out)

---

## WORKFLOW: Creating Multiple Nodes in One Tree

### Step 1: Create the Tree
1. Click **"New Tree"** → Save as `MyDialogueTree.asset`
2. Tree is now loaded (but empty)

### Step 2: Create Nodes
1. **Right-click canvas** → "Create Node"
2. Save as `Node_1.asset` (or any name)
3. **Repeat** to create `Node_2.asset`, `Node_3.asset`, etc.
4. Each node is saved as a **separate file** in your project

### Step 3: Link Nodes Together
**Option A: Set Start Node**
- Click empty space (deselect any node)
- In Inspector → "Tree Properties"
- Drag `Node_1` into **"Start Node"** field
- Now `Node_1` appears in graph (green = start node)

**Option B: Link via Choices**
- Select `Node_1`
- In Inspector → "Choices" section
- Click **"Add Choice"**
- In **"Next Node"** field, drag `Node_2` from Project window
- Now `Node_2` appears in graph (connected via line)

**Option C: Link via Auto-Advance**
- Select `Node_1`
- In Inspector → "Next Node (Auto)" field
- Drag `Node_2` from Project window
- Now `Node_2` appears in graph (cyan dashed line)

### Step 4: See All Nodes
- **Default:** Only connected nodes show (reachable from start)
- **Toggle "Show All Nodes"** in toolbar → See ALL nodes (unlinked ones grayed out)
- Unlinked nodes show `[Unlinked]` label

---

## EXAMPLE: Building a 3-Node Tree

```
1. Create Tree → "TestTree"
2. Create Node_1 → Set as Start Node
3. Create Node_2 → (not visible yet - not linked)
4. Create Node_3 → (not visible yet - not linked)

5. Select Node_1 → Add Choice → Link to Node_2
   → Node_2 now appears! (connected)

6. Select Node_2 → Add Choice → Link to Node_3
   → Node_3 now appears! (connected)

Result: All 3 nodes visible, connected in sequence
```

---

## WHY NODES APPEAR "SEPARATELY"

### The Issue You're Experiencing:
- You create Node_1 → It appears
- You create Node_2 → It doesn't appear (because it's not linked yet)
- You create Node_3 → It doesn't appear (because it's not linked yet)

### The Solution:
1. **Link them!** Set "Next Node" in choices or auto-advance
2. **OR** Toggle **"Show All Nodes"** to see unlinked nodes

---

## TIPS

### Tip 1: Use "Show All Nodes" Toggle
- **When creating:** Toggle ON to see all nodes while building
- **When testing:** Toggle OFF to see only the connected flow

### Tip 2: Link as You Go
- After creating a node, immediately link it to the previous one
- This keeps your graph organized and visible

### Tip 3: Start Node First
- Always set the Start Node first
- Then build outward from there

### Tip 4: Visual Indicators
- **Green node** = Start Node
- **Red node** = End Node
- **Yellow node** = Selected
- **Grayed out** = Unlinked (when "Show All Nodes" is ON)
- **White line** = Always-show choice
- **Yellow line** = Conditional choice
- **Cyan dashed line** = Auto-advance

---

## COMMON QUESTIONS

**Q: Can I use one node in multiple trees?**  
A: Yes! Nodes are independent. You can reference the same node from different trees.

**Q: Why don't my new nodes show up?**  
A: They're not linked yet. Either:
- Link them via choices/auto-advance, OR
- Toggle "Show All Nodes" to see unlinked nodes

**Q: How do I know which nodes are in my tree?**  
A: 
- **Connected view:** Only nodes reachable from start node
- **All nodes view:** All nodes in project (toggle "Show All Nodes")

**Q: Can I delete a node that's used in multiple trees?**  
A: Yes, but it will break references in all trees. Unity will show missing references.

---

## QUICK REFERENCE

| Action | Result |
|--------|--------|
| Create Node | Node saved as separate asset |
| Set as Start Node | Node appears in graph (green) |
| Link via Choice | Target node appears (connected) |
| Link via Auto-Advance | Target node appears (cyan line) |
| Toggle "Show All Nodes" | See all nodes, even unlinked |
| Unlink a node | Node disappears from graph (unless "Show All" is ON) |

---

**Remember: Nodes are separate files. Trees connect them. Link nodes to see them in the graph!**


