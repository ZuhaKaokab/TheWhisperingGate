# Asset Authoring Guide for Non-Programmers
## How Your Team Creates Dialogue in Unity Editor (NO CODING)

**For:** Narrative Designers, Content Creators  
**Time to Learn:** 30 minutes  
**Audience:** Your FYP team creating dialogue nodes

---

## QUICK START - CREATE YOUR FIRST DIALOGUE IN 30 MINUTES

### Step 1: Create a Character (5 min)
1. In Project panel, find `Assets/Dialogue/Characters` folder
2. Right-click → Create → Whispering Gate → Character Data
3. Name it: `Character_Protagonist.asset`
4. In Inspector:
   - Character ID: `protagonist`
   - Display Name: `You`
   - Portrait: (drag a face image)
5. Save

### Step 2: Create a Dialogue Node (5 min)
1. Right-click → Create → Whispering Gate → Dialogue Node
2. Name: `Node_Jungle_Intro.asset`
3. In Inspector:
   - Node ID: `jungle_intro_1`
   - Speaker: Character_Protagonist
   - Line Text: "Where... am I?"
   - Choices: 0 (leave empty)
   - IsEndNode: false
4. Save

### Step 3: Create Second Node (5 min)
1. Create another node: `Node_Jungle_Tree.asset`
2. Fill:
   - Node ID: `jungle_tree_1`
   - Speaker: Protagonist
   - Line Text: "That tree... something feels wrong."
3. Save

### Step 4: Link Nodes Together (5 min)
1. Open `Node_Jungle_Intro.asset`
2. Find "Next Node If Auto": drag `Node_Jungle_Tree.asset` here
3. Save

### Step 5: Create Dialogue Tree (5 min)
1. Right-click → Create → Whispering Gate → Dialogue Tree
2. Name: `Tree_JungleAwakens.asset`
3. In Inspector:
   - Tree ID: `jungle_awakens`
   - Tree Title: `Jungle Awakens`
   - Start Node: Drag `Node_Jungle_Intro.asset` here
4. Save

### Step 6: Test It (5-10 min)
1. In scene, create empty GameObject: "DialogueTest"
2. Add this temp script:
   ```csharp
   void Update() {
       if (Input.GetKeyDown(KeyCode.T)) {
           DialogueManager.Instance.StartDialogue(dialogueTree);
       }
   }
   ```
3. Drag `Tree_JungleAwakens` to dialogueTree field
4. Hit Play → Press T → See dialogue!

---

## COMPLETE REFERENCE GUIDE

### Variable Names You'll Use

```
courage (0-100)
trust_alina (0-100)
trust_writer (0-100)
sanity (0-100, starts 50)
investigation (0-100)

Flags:
journal_found (true/false)
saw_dolls (true/false)
heard_scream (true/false)
met_writer (true/false)
portal_discovered (true/false)
```

### Conditional Choice Example

**Scenario:** Show choice only if player has courage >= 30

1. Create a choice
2. Check: "Has Condition" = ON
3. In "Show Condition" type: `courage >= 30`
4. This button only shows if condition met!

### Choice Impact Example

**Scenario:** When player chooses "Be brave", add 10 courage

1. In a choice, find "Impacts"
2. Click + to add impact
3. Fill:
   - Variable Name: `courage`
   - Value Change: `10`
4. Done! Choosing this gives +10 courage

### Commands Example

**Scenario:** Give player journal when they read the desk

1. Create a node: "You find a journal"
2. In "Start Commands", add: `item:journal`
3. When this node plays, journal is added!

Other commands:
- `item:key` - Give item
- `flag:saw_dolls` - Set flag true
- `var:courage+10` - Add to variable
- `ending:good` - Mark ending path

---

## SCENE 1 EXAMPLE - COMPLETE WALKTHROUGH

### Scene: Jungle Clearing (First 5 minutes of game)

**You create these 5 nodes:**

**Node 1: Awakening**
```
ID: jungle_1
Speaker: Protagonist
Text: "I opened my eyes. The sky was bleeding red..."
Choices: 0
Next If Auto: Node 2
Start Commands: [flag:jungle_arrived]
```

**Node 2: The Tree**
```
ID: jungle_2
Speaker: Protagonist
Text: "The twisted tree above me hung with straw dolls..."
Choices: 2

Choice 1:
- Text: "These dolls are unnatural"
- Next: Node_2a
- Impacts: [courage -5, sanity -10]

Choice 2:
- Text: "I should examine them"
- Next: Node_2b
- Impacts: [investigation +10]
```

**Node 2A: Creepy**
```
ID: jungle_2a
Speaker: Protagonist
Text: "I looked away. Something in me didn't want to know."
Choices: 0
Next If Auto: Node 3
Start Commands: [flag:dolls_ignored]
```

**Node 2B: Investigate**
```
ID: jungle_2b
Speaker: Protagonist
Text: "Each doll wore tiny clothes. Recently stitched."
Choices: 0
Next If Auto: Node 3
Start Commands: [item:doll_sketch, flag:dolls_examined]
End Commands: [var:sanity-15]
```

**Node 3: Journal**
```
ID: jungle_3
Speaker: Protagonist
Text: "Beside me lay a burnt journal with my name..."
Choices: 1

Choice 1:
- Text: "Read the journal"
- Has Condition: YES
- Show Condition: investigation >= 15
- Next: Node 4
- Impacts: [investigation +20]
```

---

## COMMON MISTAKES & FIXES

| Mistake | Fix |
|---------|-----|
| Node ID has spaces | Use underscores: `node_1` not `node 1` |
| Variable name typo | Spell exactly: `courage` not `bravery` |
| Circular link | Node A → B → A | Make Node A an end |
| Missing next node | Choice has no "Next Node" | Always link to somewhere |
| Condition syntax wrong | `courage >= 30` has typo | Copy-paste from reference |
| Impact doesn't work | Variable name misspelled | Check variable name list |

---

## BEST PRACTICES

1. **Name nodes clearly:** `Node_Scene1_FollowScream` not `node1`
2. **Test as you create:** After each node, hit Play and test
3. **Use choices to branch:** Every choice should go somewhere different
4. **Make impacts meaningful:** If choice impacts courage, it should feel brave/cowardly
5. **Link logically:** Players should understand why they see certain choices

---

## WORKFLOW CHECKLIST

- [ ] Plan your scene on paper first (what happens, who says what)
- [ ] Create all character assets
- [ ] Create all dialogue nodes
- [ ] Add choices and impacts
- [ ] Create dialogue tree pointing to first node
- [ ] Test in game (hit Play, press T)
- [ ] Fix any errors
- [ ] Commit to Git

---

**You're ready. Start creating dialogue.**
