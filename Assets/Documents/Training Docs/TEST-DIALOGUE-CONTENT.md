# Test Dialogue Content - Ready to Use
## Complete Dialogue Tree Data for Testing the System

**Purpose:** Copy-paste ready dialogue content to test all system features  
**Estimated Play Time:** ~5-7 minutes  
**Scenes:** 1 (Jungle Awakening - Simplified)

---

## SETUP INSTRUCTIONS

### Step 1: Create Character Assets
1. Right-click in Project → `Create/Whispering Gate/Character Data`
2. Create these 3 characters:

**Character_Protagonist.asset**
- Character ID: `protagonist`
- Display Name: `You`
- Portrait: (optional, can add later)
- Description: `The player character`

**Character_Alina.asset**
- Character ID: `alina`
- Display Name: `Alina`
- Portrait: (optional)
- Description: `Your sister, cursed and in danger`

**Character_Writer.asset**
- Character ID: `writer`
- Display Name: `The Writer`
- Portrait: (optional)
- Description: `Mysterious figure who knows about the curse`

---

## DIALOGUE TREE: "Test_JungleAwakens"

### Node 1: Start Node (Jungle Awakening)
**Node ID:** `jungle_start`  
**Speaker:** Protagonist  
**Line Text:** `I opened my eyes. The sky above me was bleeding red, and rain fell upward into the void. Where... where am I?`  
**Is End Node:** No  
**Next Node If Auto:** (empty)

**Choices:**
1. **Text:** `"Look around carefully"`  
   **Next Node:** `jungle_look`  
   **Has Condition:** No  
   **Impacts:**
   - Variable: `investigation_level`, Value: `+10`

2. **Text:** `"Get up and move forward"`  
   **Next Node:** `jungle_move`  
   **Has Condition:** No  
   **Impacts:**
   - Variable: `courage`, Value: `+5`

---

### Node 2: Look Around
**Node ID:** `jungle_look`  
**Speaker:** Protagonist  
**Line Text:** `I scanned my surroundings. Twisted trees with gnarled branches. Straw dolls hanging from the limbs, their tiny clothes recently stitched. Something felt deeply wrong here.`  
**Is End Node:** No  
**Next Node If Auto:** `jungle_tree`

**Choices:** (None - auto-advances)

**Start Commands:**
- `flag:saw_dolls`

---

### Node 3: Move Forward
**Node ID:** `jungle_move`  
**Speaker:** Protagonist  
**Line Text:** `I forced myself to stand. My legs felt weak, but I pushed forward through the unnatural jungle. The air itself seemed to whisper.`  
**Is End Node:** No  
**Next Node If Auto:** `jungle_tree`

**Choices:** (None - auto-advances)

**Impacts:**
- Variable: `sanity`, Value: `-5`

---

### Node 4: The Tree
**Node ID:** `jungle_tree`  
**Speaker:** Protagonist  
**Line Text:** `Before me stood a massive, twisted tree. Its branches reached like skeletal fingers, and from them hung dozens of straw dolls. Each one wore tiny, hand-sewn clothes. I could see my name stitched into one of them.`  
**Is End Node:** No  
**Next Node If Auto:** (empty)

**Choices:**
1. **Text:** `"Examine the doll with my name"`  
   **Next Node:** `jungle_doll_examine`  
   **Has Condition:** Yes  
   **Show Condition:** `investigation_level >= 10`  
   **Impacts:**
   - Variable: `sanity`, Value: `-10`
   - Variable: `investigation_level`, Value: `+15`

2. **Text:** `"Back away from the tree"`  
   **Next Node:** `jungle_back_away`  
   **Has Condition:** No  
   **Impacts:**
   - Variable: `courage`, Value: `-5`

3. **Text:** `"Touch the tree"`  
   **Next Node:** `jungle_touch_tree`  
   **Has Condition:** Yes  
   **Show Condition:** `courage >= 10`  
   **Impacts:**
   - Variable: `courage`, Value: `+10`
   - Variable: `sanity`, Value: `-15`

---

### Node 5: Examine Doll
**Node ID:** `jungle_doll_examine`  
**Speaker:** Protagonist  
**Line Text:** `I reached up and carefully took down the doll. It was unnervingly detailed. The clothes were real fabric, recently made. Stitched into its chest was my full name, and below it... a date. Today's date.`  
**Is End Node:** No  
**Next Node If Auto:** `jungle_scream`

**Choices:** (None - auto-advances)

**Start Commands:**
- `item:name_doll`
- `flag:examined_doll`

---

### Node 6: Back Away
**Node ID:** `jungle_back_away`  
**Speaker:** Protagonist  
**Line Text:** `I took several steps back. My heart pounded. Whatever this place was, I didn't want to know more. But as I retreated, I heard it—a scream. Alina's voice, calling my name.`  
**Is End Node:** No  
**Next Node If Auto:** `jungle_scream`

**Choices:** (None - auto-advances)

**Start Commands:**
- `flag:heard_scream`

---

### Node 7: Touch Tree
**Node ID:** `jungle_touch_tree`  
**Speaker:** Protagonist  
**Line Text:** `I placed my hand on the gnarled bark. The moment I touched it, images flooded my mind—Alina screaming, a ritual circle, a gate that shouldn't exist. The tree was showing me something. Showing me the way.`  
**Is End Node:** No  
**Next Node If Auto:** `jungle_scream`

**Choices:** (None - auto-advances)

**Start Commands:**
- `flag:touched_tree`
- `var:investigation_level+20`

---

### Node 8: The Scream
**Node ID:** `jungle_scream`  
**Speaker:** Protagonist  
**Line Text:** `"Help me!" The voice was unmistakable. Alina. She was here, somewhere in this nightmare. The scream came from deeper in the jungle, toward a path I hadn't noticed before.`  
**Is End Node:** No  
**Next Node If Auto:** (empty)

**Choices:**
1. **Text:** `"Run toward the scream"`  
   **Next Node:** `jungle_follow_scream`  
   **Has Condition:** No  
   **Impacts:**
   - Variable: `courage`, Value: `+15`
   - Variable: `trust_alina`, Value: `+10`

2. **Text:** `"Wait and observe first"`  
   **Next Node:** `jungle_wait`  
   **Has Condition:** No  
   **Impacts:**
   - Variable: `investigation_level`, Value: `+10`
   - Variable: `courage`, Value: `-5`

3. **Text:** `"This might be a trap"`  
   **Next Node:** `jungle_trap`  
   **Has Condition:** Yes  
   **Show Condition:** `sanity < 40`  
   **Impacts:**
   - Variable: `sanity`, Value: `-5`
   - Variable: `trust_alina`, Value: `-10`

---

### Node 9: Follow Scream
**Node ID:** `jungle_follow_scream`  
**Speaker:** Protagonist  
**Line Text:** `I didn't hesitate. I ran toward her voice, pushing through twisted branches and unnatural undergrowth. The path led deeper into the jungle, toward something I couldn't yet see.`  
**Is End Node:** No  
**Next Node If Auto:** `jungle_writer_meeting`

**Choices:** (None - auto-advances)

**Start Commands:**
- `flag:followed_scream`

---

### Node 10: Wait and Observe
**Node ID:** `jungle_wait`  
**Speaker:** Protagonist  
**Line Text:** `I forced myself to stop. To think. The scream sounded real, but in this place, nothing was what it seemed. I watched the path, listening for any other sounds. After a moment, I saw movement—a figure in the shadows.`  
**Is End Node:** No  
**Next Node If Auto:** `jungle_writer_meeting`

**Choices:** (None - auto-advances)

**Start Commands:**
- `flag:waited_observed`

---

### Node 11: Trap Paranoia
**Node ID:** `jungle_trap`  
**Speaker:** Protagonist  
**Line Text:** `No. This had to be a trap. Alina couldn't be here. This whole place was designed to lure me in, to make me desperate. I stayed where I was, my mind racing with paranoia.`  
**Is End Node:** No  
**Next Node If Auto:** `jungle_writer_meeting`

**Choices:** (None - auto-advances)

**Start Commands:**
- `flag:paranoid_choice`
- `var:sanity-10`

---

### Node 12: Writer Meeting
**Node ID:** `jungle_writer_meeting`  
**Speaker:** The Writer  
**Line Text:** `"You're here. Good." A figure emerged from the shadows—a man with tired eyes, holding a journal. "I've been waiting. The curse is real, and your sister doesn't have much time. I can help you, but you need to trust me."`  
**Is End Node:** No  
**Next Node If Auto:** (empty)

**Choices:**
1. **Text:** `"I trust you. Tell me everything."`  
   **Next Node:** `jungle_trust_writer`  
   **Has Condition:** No  
   **Impacts:**
   - Variable: `trust_writer`, Value: `+20`
   - Variable: `investigation_level`, Value: `+15`

2. **Text:** `"Why should I believe you?"`  
   **Next Node:** `jungle_doubt_writer`  
   **Has Condition:** No  
   **Impacts:**
   - Variable: `trust_writer`, Value: `-10`
   - Variable: `courage`, Value: `+5`

3. **Text:** `"Show me proof first"`  
   **Next Node:** `jungle_proof`  
   **Has Condition:** Yes  
   **Show Condition:** `investigation_level >= 20`  
   **Impacts:**
   - Variable: `trust_writer`, Value: `+10`
   - Variable: `investigation_level`, Value: `+5`

---

### Node 13: Trust Writer
**Node ID:** `jungle_trust_writer`  
**Speaker:** The Writer  
**Line Text:** `"Good. The curse can be broken, but it requires a ritual at the Whispering Gate—a portal between worlds. Your sister is trapped on the other side. You must enter, complete the ritual, and bring her back. But be warned: the gate will test your mind, your courage, and your sanity."`  
**Is End Node:** No  
**Next Node If Auto:** `jungle_ending_setup`

**Choices:** (None - auto-advances)

**Start Commands:**
- `item:ritual_journal`
- `flag:met_writer`
- `ending:trust_path`

---

### Node 14: Doubt Writer
**Node ID:** `jungle_doubt_writer`  
**Speaker:** The Writer  
**Line Text:** `"I understand your caution. But time is running out. Every moment you delay, the curse grows stronger. I'll give you the journal anyway—read it, then decide. But don't wait too long."`  
**Is End Node:** No  
**Next Node If Auto:** `jungle_ending_setup`

**Choices:** (None - auto-advances)

**Start Commands:**
- `item:ritual_journal`
- `flag:met_writer`

---

### Node 15: Proof Request
**Node ID:** `jungle_proof`  
**Speaker:** The Writer  
**Line Text:** `"Smart. Here—this journal contains everything I've learned. See for yourself. The ritual, the gate, the consequences. Once you've read it, you'll understand why I'm here, and why you must act quickly."`  
**Is End Node:** No  
**Next Node If Auto:** `jungle_ending_setup`

**Choices:** (None - auto-advances)

**Start Commands:**
- `item:ritual_journal`
- `item:writer_notes`
- `flag:met_writer`
- `var:investigation_level+10`

---

### Node 16: Ending Setup
**Node ID:** `jungle_ending_setup`  
**Speaker:** Protagonist  
**Line Text:** `I took the journal. The weight of it felt heavy in my hands—not just physical weight, but the weight of what I now knew. Alina was counting on me. The gate awaited. This was only the beginning.`  
**Is End Node:** Yes  
**Next Node If Auto:** (empty)

**Choices:** (None)

**Start Commands:**
- `flag:prologue_complete`

---

## VARIABLE TRACKING

**Variables Used:**
- `courage` (0-100) - Affects brave choices visibility
- `trust_alina` (0-100) - Relationship with sister
- `trust_writer` (0-100) - Relationship with writer
- `sanity` (0-100, starts at 50) - Mental state
- `investigation_level` (0-100) - How much player has learned

**Flags Used:**
- `saw_dolls` - Saw the hanging dolls
- `heard_scream` - Heard Alina's scream
- `examined_doll` - Examined the name doll
- `touched_tree` - Touched the twisted tree
- `followed_scream` - Ran toward the scream
- `waited_observed` - Waited and observed
- `paranoid_choice` - Chose trap option
- `met_writer` - Met the writer
- `prologue_complete` - Finished prologue

**Items Given:**
- `name_doll` - Doll with player's name
- `ritual_journal` - Journal with ritual instructions
- `writer_notes` - Additional notes (if proof path taken)

---

## TESTING CHECKLIST

### Basic Flow Test
- [ ] Create all nodes in editor
- [ ] Link choices to next nodes
- [ ] Set Node 1 as Start Node
- [ ] Test Preview in Play Mode
- [ ] Verify dialogue plays from start to end

### Conditional Choices Test
- [ ] Node 4: "Examine doll" only shows if `investigation_level >= 10`
- [ ] Node 4: "Touch tree" only shows if `courage >= 10`
- [ ] Node 8: "This might be a trap" only shows if `sanity < 40`
- [ ] Node 12: "Show me proof" only shows if `investigation_level >= 20`

### Impact Test
- [ ] Choices modify variables correctly (check Console logs)
- [ ] Items are added to inventory when commands execute
- [ ] Flags are set when commands execute

### Branching Test
- [ ] Follow "Look around" path → Should get investigation boost
- [ ] Follow "Move forward" path → Should get courage but lose sanity
- [ ] Test all three paths from Node 8 (scream)
- [ ] Test all three paths from Node 12 (writer)

### End-to-End Test
- [ ] Play full dialogue tree from start to end
- [ ] Verify all variables update correctly
- [ ] Verify all items are received
- [ ] Verify all flags are set
- [ ] Check ending path is recorded

---

## QUICK COPY-PASTE GUIDE

1. **Open Dialogue Editor** (Window → Whispering Gate → Dialogue Editor)
2. **Create New Tree** → Name it `Test_JungleAwakens`
3. **For each node above:**
   - Right-click canvas → Create Node
   - Fill in Node ID, Speaker, Line Text from this document
   - Add choices with Next Node links
   - Add impacts and conditions
   - Add commands
4. **Set Start Node** → Node 1 (`jungle_start`)
5. **Validate** → Click Validate button
6. **Test** → Enter Play Mode → Click Preview

---

## EXPECTED RESULTS

After completing this dialogue tree:
- **Play Time:** ~5-7 minutes
- **Total Nodes:** 16
- **Total Choices:** 12
- **Branching Paths:** 6+ distinct routes
- **Variables Affected:** 5 (courage, trust_alina, trust_writer, sanity, investigation_level)
- **Items Given:** 2-3 (depending on path)
- **Flags Set:** 8+

**This content tests:**
✅ Basic dialogue flow  
✅ Conditional choices  
✅ Variable impacts  
✅ Item commands  
✅ Flag commands  
✅ Multiple branching paths  
✅ Auto-advance nodes  
✅ End nodes  

---

**Ready to test! Copy the content above into your Dialogue Editor and start building.**







