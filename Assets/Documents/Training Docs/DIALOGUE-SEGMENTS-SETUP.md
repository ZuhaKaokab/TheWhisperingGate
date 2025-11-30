# Dialogue Segments Setup Guide
## Complete Textual Data Organized by Segments

**Purpose:** Organize your existing dialogue tree into segments for the Scene Management System  
**Your Current Tree:** `Test_JungleAwakens` (16 nodes)  
**Recommended Organization:** 3 segments for 15-minute prologue

---

## RECOMMENDED SEGMENT STRUCTURE

### Option 1: Keep Single Tree, Add Segment IDs (EASIEST)
**Best for:** You already have the complete tree working  
**Approach:** Keep your existing tree, just add `DialogueSegmentTrigger` components with segment IDs

### Option 2: Split into Multiple Trees (MORE ORGANIZED)
**Best for:** Better organization, easier to manage  
**Approach:** Create separate trees for each segment, link them via prerequisites

---

## SEGMENT BREAKDOWN

### SEGMENT 1: "jungle_awakening"
**Location:** Jungle - Initial Awakening  
**Nodes:** 1-8 (jungle_start → jungle_scream)  
**Duration:** ~3-4 minutes  
**Prerequisites:** None (this is the starting segment)

**Dialogue Tree:** `Test_JungleAwakens` (use existing)  
**Segment ID:** `jungle_awakening`  
**Trigger Setup:** Use `DialogueSegmentTrigger` with segment ID `jungle_awakening`

---

### SEGMENT 2: "meet_writer"
**Location:** Jungle - Writer Encounter  
**Nodes:** 9-15 (jungle_writer_meeting → jungle_proof)  
**Duration:** ~4-5 minutes  
**Prerequisites:** `jungle_awakening` (must complete Segment 1 first)

**Dialogue Tree:** `Test_JungleAwakens` (same tree, different start node)  
**OR Create New Tree:** `Tree_WriterMeeting.asset`  
**Segment ID:** `meet_writer`  
**Trigger Setup:** Requires `jungle_awakening` segment completed

---

### SEGMENT 3: "prologue_ending"
**Location:** Jungle - Final Setup  
**Nodes:** 16 (jungle_ending_setup)  
**Duration:** ~1 minute  
**Prerequisites:** `meet_writer` (must complete Segment 2 first)

**Dialogue Tree:** `Test_JungleAwakens` (same tree)  
**OR Create New Tree:** `Tree_PrologueEnding.asset`  
**Segment ID:** `prologue_ending`  
**Trigger Setup:** Requires `meet_writer` segment completed

---

## COMPLETE TEXTUAL DATA BY SEGMENT

### SEGMENT 1: "jungle_awakening"

#### Node 1: Start Node (Jungle Awakening)
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

#### Node 2: Look Around
**Node ID:** `jungle_look`  
**Speaker:** Protagonist  
**Line Text:** `I scanned my surroundings. Twisted trees with gnarled branches. Straw dolls hanging from the limbs, their tiny clothes recently stitched. Something felt deeply wrong here.`  
**Is End Node:** No  
**Next Node If Auto:** `jungle_tree`

**Choices:** (None - auto-advances)

**Start Commands:**
- `flag:saw_dolls`

---

#### Node 3: Move Forward
**Node ID:** `jungle_move`  
**Speaker:** Protagonist  
**Line Text:** `I forced myself to stand. My legs felt weak, but I pushed forward through the unnatural jungle. The air itself seemed to whisper.`  
**Is End Node:** No  
**Next Node If Auto:** `jungle_tree`

**Choices:** (None - auto-advances)

**Impacts:**
- Variable: `sanity`, Value: `-5`

---

#### Node 4: The Tree
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

#### Node 5: Examine Doll
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

#### Node 6: Back Away
**Node ID:** `jungle_back_away`  
**Speaker:** Protagonist  
**Line Text:** `I took several steps back. My heart pounded. Whatever this place was, I didn't want to know more. But as I retreated, I heard it—a scream. Alina's voice, calling my name.`  
**Is End Node:** No  
**Next Node If Auto:** `jungle_scream`

**Choices:** (None - auto-advances)

**Start Commands:**
- `flag:heard_scream`

---

#### Node 7: Touch Tree
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

#### Node 8: The Scream
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

**SEGMENT 1 ENDS HERE**  
**Next Segment Requires:** Player moves to new location (use SceneTransition or manual trigger)

---

### SEGMENT 2: "meet_writer"

#### Node 9: Follow Scream
**Node ID:** `jungle_follow_scream`  
**Speaker:** Protagonist  
**Line Text:** `I didn't hesitate. I ran toward her voice, pushing through twisted branches and unnatural undergrowth. The path led deeper into the jungle, toward something I couldn't yet see.`  
**Is End Node:** No  
**Next Node If Auto:** `jungle_writer_meeting`

**Choices:** (None - auto-advances)

**Start Commands:**
- `flag:followed_scream`

---

#### Node 10: Wait and Observe
**Node ID:** `jungle_wait`  
**Speaker:** Protagonist  
**Line Text:** `I forced myself to stop. To think. The scream sounded real, but in this place, nothing was what it seemed. I watched the path, listening for any other sounds. After a moment, I saw movement—a figure in the shadows.`  
**Is End Node:** No  
**Next Node If Auto:** `jungle_writer_meeting`

**Choices:** (None - auto-advances)

**Start Commands:**
- `flag:waited_observed`

---

#### Node 11: Trap Paranoia
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

#### Node 12: Writer Meeting
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

#### Node 13: Trust Writer
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

#### Node 14: Doubt Writer
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

#### Node 15: Proof Request
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

**SEGMENT 2 ENDS HERE**  
**Next Segment Requires:** `meet_writer` segment completed

---

### SEGMENT 3: "prologue_ending"

#### Node 16: Ending Setup
**Node ID:** `jungle_ending_setup`  
**Speaker:** Protagonist  
**Line Text:** `I took the journal. The weight of it felt heavy in my hands—not just physical weight, but the weight of what I now knew. Alina was counting on me. The gate awaited. This was only the beginning.`  
**Is End Node:** Yes  
**Next Node If Auto:** (empty)

**Choices:** (None)

**Start Commands:**
- `flag:prologue_complete`

---

**SEGMENT 3 ENDS HERE**  
**Prologue Complete!**

---

## SETUP INSTRUCTIONS

### Option A: Keep Single Tree (EASIEST - NOW WORKING!)

**Step 1: Your tree is already created**
- Tree: `Test_JungleAwakens.asset`
- All 16 nodes exist
- Everything is linked correctly

**Step 2: Create 3 DialogueSegmentStarters**

**Starter 1: Segment "jungle_awakening"**
1. Create GameObject (no collider needed - uses distance check)
2. Add `DialogueSegmentStarter` component
3. Assign `Test_JungleAwakens` to Dialogue Tree
4. Set Start Node ID: `jungle_start`
5. Set Segment ID: `jungle_awakening`
6. Set Required Segments: (empty - no prerequisites)
7. Set Interaction Range: 3-5 units
8. Position at Location 1 (jungle start)

**Starter 2: Segment "meet_writer"**
1. Create GameObject
2. Add `DialogueSegmentStarter` component
3. Assign `Test_JungleAwakens` to Dialogue Tree
4. Set Start Node ID: `jungle_writer_meeting`
5. Set Segment ID: `meet_writer`
6. Set Required Segments: `jungle_awakening`
7. Set Interaction Range: 3-5 units
8. Position at Location 2 (writer encounter area)

**Starter 3: Segment "prologue_ending"**
1. Create GameObject
2. Add `DialogueSegmentStarter` component
3. Assign `Test_JungleAwakens` to Dialogue Tree
4. Set Start Node ID: `jungle_ending_setup`
5. Set Segment ID: `prologue_ending`
6. Set Required Segments: `meet_writer`
7. Set Interaction Range: 3-5 units
8. Position at Location 3 (ending area)

**How it works:**
- `DialogueSegmentStarter` uses `DialogueManager.StartDialogueAtNodeId()` to start at any node
- Player presses E when in range to trigger dialogue
- Segment is automatically marked complete when dialogue ends

---

### Option B: Split into Multiple Trees (RECOMMENDED)

**Step 1: Create Tree for Segment 1**
1. Create new Dialogue Tree: `Tree_JungleAwakening.asset`
2. Set Start Node: `jungle_start` (Node 1)
3. Include nodes: 1-8 (up to `jungle_scream`)
4. Make `jungle_scream` an End Node (or link to a transition node)

**Step 2: Create Tree for Segment 2**
1. Create new Dialogue Tree: `Tree_WriterMeeting.asset`
2. Set Start Node: `jungle_writer_meeting` (Node 12)
3. Include nodes: 9-15 (all writer-related nodes)
4. Make last node link to ending

**Step 3: Create Tree for Segment 3**
1. Create new Dialogue Tree: `Tree_PrologueEnding.asset`
2. Set Start Node: `jungle_ending_setup` (Node 16)
3. This is the final segment

**Step 4: Set up DialogueSegmentTriggers**

**Trigger 1:**
- Tree: `Tree_JungleAwakening`
- Segment ID: `jungle_awakening`
- Prerequisites: None

**Trigger 2:**
- Tree: `Tree_WriterMeeting`
- Segment ID: `meet_writer`
- Prerequisites: `jungle_awakening`

**Trigger 3:**
- Tree: `Tree_PrologueEnding`
- Segment ID: `prologue_ending`
- Prerequisites: `meet_writer`

---

## RECOMMENDED FOLDER STRUCTURE

```
Assets/
└── Scriptable Object Data/
    ├── Characters/
    │   ├── Character_Protagonist.asset ✅ (exists)
    │   ├── Character_Alina.asset ✅ (exists)
    │   └── Character_Writer.asset ✅ (exists)
    │
    ├── Trees/
    │   ├── Test_JungleAwakens.asset ✅ (exists - keep as backup)
    │   ├── Tree_JungleAwakening.asset (NEW - Segment 1)
    │   ├── Tree_WriterMeeting.asset (NEW - Segment 2)
    │   └── Tree_PrologueEnding.asset (NEW - Segment 3)
    │
    └── Nodes/
        ├── Segment1_Jungle/
        │   ├── Node_1_jungle_start.asset ✅ (exists)
        │   ├── Node_2_jungle_look.asset ✅ (exists)
        │   ├── Node_3_jungle_move.asset ✅ (exists)
        │   ├── Node_4_jungle_tree.asset ✅ (exists)
        │   ├── Node_5_jungle_doll_examine.asset ✅ (exists)
        │   ├── Node_6_jungle_back_away.asset ✅ (exists)
        │   ├── Node_7_jungle_touch_tree.asset ✅ (exists)
        │   └── Node_8_jungle_scream.asset ✅ (exists)
        │
        ├── Segment2_Writer/
        │   ├── Node_9_jungle_follow_scream.asset ✅ (exists)
        │   ├── Node_10_jungle_wait.asset ✅ (exists)
        │   ├── Node_11_jungle_trap.asset ✅ (exists)
        │   ├── Node_12_jungle_writer_meeting.asset ✅ (exists)
        │   ├── Node_13_jungle_trust_writer.asset ✅ (exists)
        │   ├── Node_14_jungle_doubt_writer.asset ✅ (exists)
        │   └── Node_15_jungle_proof.asset ✅ (exists)
        │
        └── Segment3_Ending/
            └── Node_16_jungle_ending_setup.asset ✅ (exists)
```

---

## QUICK SETUP CHECKLIST

### For Option A (Easiest - Keep Single Tree):

- [ ] Add `DialogueSegmentStarter` for Segment 1
  - [ ] Tree: `Test_JungleAwakens`
  - [ ] Start Node ID: `jungle_start`
  - [ ] Segment ID: `jungle_awakening`
  - [ ] No prerequisites

- [ ] Add `DialogueSegmentStarter` for Segment 2
  - [ ] Tree: `Test_JungleAwakens`
  - [ ] Start Node ID: `jungle_writer_meeting`
  - [ ] Segment ID: `meet_writer`
  - [ ] Required Segments: `jungle_awakening`

- [ ] Add `DialogueSegmentStarter` for Segment 3
  - [ ] Tree: `Test_JungleAwakens`
  - [ ] Start Node ID: `jungle_ending_setup`
  - [ ] Segment ID: `prologue_ending`
  - [ ] Required Segments: `meet_writer`

- [ ] Add LevelManager to scene
- [ ] Test the flow!

### For Option B (More Organized - Multiple Trees):

- [ ] Create `Tree_JungleAwakening.asset`
  - [ ] Set Start Node: `jungle_start`
  - [ ] Verify nodes 1-8 are linked correctly
  - [ ] Make `jungle_scream` end properly

- [ ] Create `Tree_WriterMeeting.asset`
  - [ ] Set Start Node: `jungle_writer_meeting`
  - [ ] Verify nodes 9-15 are linked correctly

- [ ] Create `Tree_PrologueEnding.asset`
  - [ ] Set Start Node: `jungle_ending_setup`
  - [ ] Mark as End Node

- [ ] Set up DialogueSegmentTrigger for Segment 1
  - [ ] Tree: `Tree_JungleAwakening`
  - [ ] Segment ID: `jungle_awakening`
  - [ ] No prerequisites

- [ ] Set up DialogueSegmentTrigger for Segment 2
  - [ ] Tree: `Tree_WriterMeeting`
  - [ ] Segment ID: `meet_writer`
  - [ ] Required Segments: `jungle_awakening`

- [ ] Set up DialogueSegmentTrigger for Segment 3
  - [ ] Tree: `Tree_PrologueEnding`
  - [ ] Segment ID: `prologue_ending`
  - [ ] Required Segments: `meet_writer`

- [ ] Add LevelManager to scene
- [ ] Test the flow!

---

## TESTING THE SEGMENTED FLOW

1. **Start Game** → Player at Location 1
2. **Enter Trigger 1** → Segment 1 dialogue plays
3. **Complete Segment 1** → `jungle_awakening` marked complete
4. **Move to Location 2** → Trigger 2 becomes active
5. **Enter Trigger 2** → Segment 2 dialogue plays (requires Segment 1)
6. **Complete Segment 2** → `meet_writer` marked complete
7. **Move to Location 3** → Trigger 3 becomes active
8. **Enter Trigger 3** → Segment 3 dialogue plays (requires Segment 2)
9. **Complete Segment 3** → Prologue complete!

---

## VARIABLES & FLAGS SUMMARY

**Variables Used:**
- `courage` (0-100)
- `trust_alina` (0-100)
- `trust_writer` (0-100)
- `sanity` (0-100, starts at 50)
- `investigation_level` (0-100)

**Flags Set:**
- `saw_dolls`
- `heard_scream`
- `examined_doll`
- `touched_tree`
- `followed_scream`
- `waited_observed`
- `paranoid_choice`
- `met_writer`
- `prologue_complete`

**Items Given:**
- `name_doll` (if examined)
- `ritual_journal` (always)
- `writer_notes` (if proof path)

---

**Ready to set up! Choose Option A (single tree with custom script) or Option B (multiple trees - recommended).**

