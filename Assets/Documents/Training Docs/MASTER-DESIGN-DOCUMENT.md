# THE WHISPERING GATE - MASTER DESIGN DOCUMENT
## Complete Game Vision, Technical Requirements & System Architecture

**For:** Cursor AI Training & Development Team  
**Version:** 1.0 Final  
**Date:** November 25, 2025  
**Status:** Ready for Implementation

---

## PART 1: GAME OVERVIEW & VISION

### 1.1 Core Concept

**Title:** The Whispering Gate  
**Genre:** Psychological Horror Thriller / Choice-Based Adventure / Interactive Narrative  
**Platform:** PC (Windows)  
**Engine:** Unity 2021.3+  
**Perspective:** 3rd-Person with First-Person Camera (Immersive)  
**Target Audience:** Adults (18+), Horror/Mystery enthusiasts

**Tagline:** "Your sister is cursed. The only way to save her leads through a gate that shouldn't exist."

---

### 1.2 Story Summary

**Premise:**
The protagonist's sister, Alina, has been afflicted with an ancient black magic curse that modern medicine cannot cure. Desperate, the protagonist discovers a cryptic journal belonging to a journalist/writer who investigated the same curse years ago. This leads him to a mysterious ritual to break the curse, but the path requires him to navigate a supernatural portal known as "The Whispering Gate"—a threshold between the physical world and a cursed parallel realm.

**Setting:**
- **Primary World:** Rural jungle location (Pakistan/South Asian inspired), abandoned house, supernatural portal chamber
- **Parallel Realm:** Distorted, nightmarish version of familiar locations
- **Time Period:** Modern day with supernatural/mystical elements

**Core Conflict:**
- The curse grows stronger each day
- The ritual to break it is dangerous and may not work
- The protagonist's choices affect his mental state (sanity), relationships (trust), and bravery (courage)
- Multiple possible endings based on accumulated decisions

**Themes:**
- Sacrifice vs. Self-Preservation
- Trust in the Unknown
- The Cost of Knowledge
- Sanity vs. Reality
- Love and Desperation

---

### 1.3 Game Philosophy

**Narrative-First Approach:**
- Story, characters, and choices are central to the experience
- Every gameplay mechanic serves the narrative
- Player agency through meaningful choices that genuinely affect outcome

**Branching Narrative:**
- Non-linear storytelling with multiple paths
- Player choices accumulate to determine ending
- Variable-driven consequences (courage, trust, sanity)
- 3+ distinct ending outcomes visible in 15-minute prologue

**Atmospheric Horror:**
- Environmental storytelling (dolls, rituals, supernatural signs)
- Psychological tension through choice consequences
- Immersive first-person perspective for player agency
- Sound design and visual effects to build dread

---

## PART 2: CORE SYSTEMS

### 2.1 Dialogue System

**Purpose:** Deliver branching narrative with player agency through choice-driven conversations

**Key Features:**
- **Non-Linear Branching:** Choices lead to different dialogue paths
- **State Tracking:** Variables (courage, trust, sanity) persist and affect dialogue
- **Conditional Choices:** Show/hide options based on player's accumulated decisions
- **Character Relationships:** Different characters react differently based on trust levels
- **Impactful Consequences:** Each choice modifies game state meaningfully

**Technical Approach:**
- ScriptableObject-based node system (NOT Yarn/Ink - pure C# architecture)
- Allows non-programmers to create dialogue in Unity Inspector (WYSIWYG)
- Serializable and editor-friendly
- No external dependencies

**Data Structure:**
```
DialogueTree (container)
    ├─ DialogueNode (single line + speaker)
    │   ├─ CharacterData (speaker info, portrait)
    │   ├─ DialogueChoice (player option)
    │   │   ├─ NextNode (where this choice leads)
    │   │   ├─ ChoiceImpact (variable changes)
    │   │   └─ Conditions (when to show this choice)
    │   └─ Commands (consequences: give items, set flags)
```

**Variable System:**
```
courage (0-100)
├─ Increases: Brave choices, facing fears
├─ Decreases: Cowardly choices, fleeing
└─ Affects: Available dialogue options, ending determination

trust_alina (0-100)
├─ Increases: Helping sister, asking about her
├─ Decreases: Ignoring her, prioritizing yourself
└─ Affects: Sister's responses, special endings

trust_writer (0-100)
├─ Increases: Believing writer, asking about ritual
├─ Decreases: Doubting writer, refusing help
└─ Affects: Writer's cooperation, ritual difficulty

sanity (0-100, starts 50)
├─ Increases: Safe choices, normalcy, rest
├─ Decreases: Supernatural encounters, horror elements
└─ Affects: dialogue tone, ending path

investigation (0-100)
├─ Increases: Reading journal, examining clues, asking questions
├─ Decreases: Ignoring evidence, moving forward blindly
└─ Affects: dialogue detail, hidden lore availability
```

**Ending Determination (Example Logic):**
```
if sanity < 30
    → BAD_INSANE ending (player lost mind)

else if courage >= 70 && trust_alina >= 60
    → GOOD_SAVED ending (save sister successfully)

else if courage < 30
    → BAD_FLEE ending (player abandons sister)

else if trust_writer >= 80
    → GOOD_CONTROL ending (player masters ritual)

else
    → MIXED_UNCERTAIN ending (ambiguous fate)
```

---

### 2.2 Character Controller System

**Purpose:** Provide immersive exploration and interaction mechanics

**Key Features:**
- **3D First-Person Movement:** WASD for direction, Mouse for camera
- **Contextual Interaction:** E key to trigger world interactions (dialogues, items)
- **Sprint Mechanic:** Shift for faster movement
- **Jump Mechanic:** Space for vertical navigation
- **Pause During Dialogue:** Automatically disable input when dialogue active
- **Resume After Dialogue:** Restore player control seamlessly

**Technical Requirements:**
- Rigidbody-based movement (physics-aware)
- Smooth camera rotation with framerate independence
- Animation integration (Idle, Walk, Sprint, Jump states)
- Interaction range detection (raycast or sphere cast)
- Event-based pause/resume with DialogueManager

**Movement Parameters:**
```
Walk Speed: 5 units/sec
Sprint Speed: 10 units/sec
Mouse Sensitivity: 2.0
Jump Force: 5 units
Jump Cooldown: 0.5 seconds
Interaction Range: 3 units (adjustable)
```

---

### 2.3 Inventory System

**Purpose:** Track items collected and make them visible to player

**Key Features:**
- **Item Storage:** Collect items from dialogue consequences
- **Item Persistence:** Items survive scene changes
- **Visual Representation:** Icon grid showing collected items
- **Item Metadata:** Name, description, icon, usage notes
- **Dialogue Integration:** Dialogue can check "Do you have X?"

**Technical Requirements:**
- ScriptableObject ItemData for each item
- Dictionary-based inventory (fast lookup)
- UI grid display with hover tooltips
- Serialization for save/load
- Event system for UI updates

**Typical Items:**
- journal (burnt leather journal with clues)
- key (to unlock locations)
- photo (evidence or memory)
- ritual_scroll (instructions for ritual)
- charm (protection against supernatural)
- potion (mental clarity / sanity boost)

---

## PART 3: STORY BREAKDOWN - PROLOGUE (15 minutes)

### 3.1 Scene 1: Jungle Awakening (5 minutes)

**Player Experience:**
1. **Opening Cutscene (1 min):**
   - Red bleeding sky
   - Rain that vanishes before hitting ground
   - Twisted tree with hanging dolls
   - Camera focus on protagonist's face waking up
   - Flashback montage of Alina screaming

2. **Player Control (2 min):**
   - Player wakes under twisted tree
   - Environment: Jungle clearing, dolls hanging, ominous silence
   - Interactive: Examine dolls (sanity -10), read journal (investigation +10)
   - Dialogue trigger: Writer's voice whispers a warning

3. **Dialogue Sequence (2 min):**
   - 3-5 dialogue nodes about waking up, disorientation, hearing Alina's scream
   - Choices: "Follow the scream" (courage +10) or "Investigate first" (investigation +10)
   - First variable impact test

**Story Beats:**
- Establish mystery: Where am I? Why is the sky red?
- Introduce curse: Hear Alina's terrified scream
- Introduce journal: Find clues about what's happening
- Introduce dread: Dolls suggest ritual/curse

**Technical Implementation:**
- Scene: `Prologue_JungleAwakens.unity`
- Nodes: ~8 dialogue nodes
- Variables Affected: courage, sanity, investigation
- Commands: item:journal_excerpt, flag:jungle_arrived

---

### 3.2 Scene 2: Writer's House (5 minutes)

**Player Experience:**
1. **Exploration (2 min):**
   - Journey through jungle to house entrance
   - Atmospheric exploration of decaying house
   - Examine ritual markings, candles, scattered notes
   - Collect items: ritual_scroll, photograph, charm

2. **Encounter with Writer (2 min):**
   - Mysterious figure appears or is discovered
   - Dialogue about the curse and ritual
   - Choices determine trust level
   - Writer gives mission: Reach the portal and complete ritual

3. **Preparation (1 min):**
   - Writer provides final warnings
   - Player can ask questions (investigation affects detail)
   - Sanity warning: "The portal will test your mind"

**Story Beats:**
- Reveal ritual method to break curse
- Introduce writer character and his knowledge
- Establish stakes: Must reach portal before curse consumes Alina
- Choice: Trust writer or doubt him?

**Technical Implementation:**
- Scene: `Prologue_WritersHouse.unity` (can be same as Scene 1 or separate)
- Nodes: ~12 dialogue nodes (branching based on Scene 1 choices)
- Variables Affected: trust_writer, courage, sanity
- Commands: item:ritual_scroll, item:charm, flag:met_writer

---

### 3.3 Scene 3: Portal Chamber (5 minutes)

**Player Experience:**
1. **Discovery (1 min):**
   - Hidden passage or journey to underground chamber
   - Ritual chamber discovered: Glowing portal, candles, symbols
   - Environmental storytelling: Previous failed attempts visible

2. **Moment of Decision (2 min):**
   - Critical dialogue about consequences
   - Dialogue tree heavily influenced by accumulated variables
   - If courage < 30: Option to flee (BAD_FLEE)
   - If sanity < 40: Dialogue becomes unreliable (player seeing things?)
   - If trust_writer >= 70: Writer encourages entry
   - If investigation >= 60: Player understands ritual better

3. **Cliffhanger Ending (2 min):**
   - Cinematic: Player makes final choice - enter portal or refuse
   - If enters: Portal opens dramatically, Alina's voice calls out
   - If refuses: Writer confronts player, moral dilemma
   - Fade to black with ending title card

**Story Beats:**
- Point of no return: This is the actual ritual
- Consequence preview: Dialogue hints at what happens inside portal
- Character test: Has player earned writer's respect? Alina's faith?
- Multiple outcome teasers: "If you fail...", "She's counting on you...", "There's no coming back..."

**Technical Implementation:**
- Scene: `Prologue_PortalChamber.unity`
- Nodes: ~15 dialogue nodes (heavily conditional based on all variables)
- Variables Affected: sanity, courage, ending_path
- Commands: ending:good_saved, ending:mixed_uncertain, ending:bad_flee

---

## PART 4: DIALOGUE CONTENT SPECIFICATION

### 4.1 Required Dialogue Nodes (Prologue)

**Scene 1 (Jungle):**
```
Node_Jungle_1: "I opened my eyes..." (intro, no choice)
Node_Jungle_2: "The tree above me..." (choice: investigate vs flee)
Node_Jungle_2a: "Something felt wrong..." (investigate path)
Node_Jungle_2b: "I turned away..." (flee path)
Node_Jungle_3: "I heard it clearly..." (Alina's scream, no choice)
Node_Jungle_4: "The journal beside me..." (discovery, no choice)
Node_Jungle_5: "A voice in the wind..." (writer's warning, choice: listen vs ignore)
Node_Jungle_6: "The voice faded..." (closing, leads to Scene 2)
```

**Scene 2 (House):**
```
Node_House_1: "The house loomed before me..." (arrival, no choice)
Node_House_2: "Inside, evidence of ritual..." (exploration)
Node_House_3: "A figure emerged from shadows..." (writer appears)
Node_House_3a: (if courage >= 50) "You seem brave..."
Node_House_3b: (if courage < 50) "You seem uncertain..."
Node_House_4: "The curse is no ordinary curse..." (exposition)
Node_House_5: "You must reach the portal..." (mission briefing)
Node_House_6: "The portal will change you..." (final warning, choice: proceed vs hesitate)
Node_House_7: (if trust_writer >= 70) "I believe in you."
Node_House_7b: (if trust_writer < 70) "I hope you're prepared."
Node_House_End: "The chamber is hidden beneath..." (direction to portal)
```

**Scene 3 (Portal):**
```
Node_Portal_1: "The chamber took my breath away..." (discovery)
Node_Portal_2: "Before me hung the gate..." (portal description)
Node_Portal_3: (conditional on variables) "This is the moment..." (choices multiply based on path)
Node_Portal_Good: (if all good choices) "Alina needs you. You won't fail."
Node_Portal_Mixed: (if mixed) "The outcome is uncertain now."
Node_Portal_Bad: (if bad choices) "There's still a way... but it costs."
Node_Portal_Final: "Will you enter?" (final choice: enter vs refuse)
Node_Ending_A: "The portal opens..." (GOOD ending)
Node_Ending_B: "She looks at you with betrayal..." (BAD ending)
Node_Ending_C: "The light swallows everything..." (UNCERTAIN ending)
```

---

## PART 5: TECHNICAL ARCHITECTURE

### 5.1 System Dependencies

```
┌─────────────────────────────────────────────┐
│          DIALOGUE SYSTEM (Core)              │
│         Orchestrates all interactions        │
└────────────┬────────────────┬────────────────┘
             │                │
             ↓                ↓
        ┌─────────┐      ┌──────────┐
        │ GameState   │      │ PlayerController│
        │ (Variables) │      │ (Input/Movement)│
        └────┬────┘      └───┬──────┘
             │                │
             ↓                ↓
        ┌─────────┐      ┌──────────┐
        │InventoryUI │      │ Dialogue UI │
        │(Display)  │      │(Display)   │
        └─────────┘      └──────────┘
```

### 5.2 Class Structure

**Core Classes:**
- `GameState` - Variable tracking (courage, trust, sanity, etc.)
- `DialogueManager` - Dialogue orchestration and flow control
- `DialogueUIPanel` - UI display and player interaction
- `PlayerController` - Input handling and character movement
- `InventoryManager` - Item storage and management
- `DialogueTrigger` - World-based dialogue trigger points

**Data Classes:**
- `CharacterData` - Character metadata (portrait, name, theme audio)
- `DialogueNode` - Single dialogue line with choices
- `DialogueTree` - Container of nodes, entry point for dialogue
- `DialogueChoice` - Player choice option
- `ChoiceImpact` - Variable modification from choice

### 5.3 Communication Pattern

**Event-Driven Design:**
```
DialogueManager
├─ OnNodeDisplayed(DialogueNode)
│  ├─→ DialogueUIPanel.DisplayNode()
│  ├─→ PlayerController.SetInputEnabled(false)
│  └─→ SoundManager.PlayNarration()
├─ OnDialogueEnded()
│  ├─→ PlayerController.SetInputEnabled(true)
│  └─→ DialogueUIPanel.Hide()
└─ OnChoiceSelected(Choice)
   ├─→ GameState.ApplyImpacts()
   └─→ DialogueManager.ShowNode(nextNode)
```

---

## PART 6: DEVELOPMENT PHASES

### Phase 0: Foundation (Days 1-2)
- Create all base classes
- Compile and test individually
- Create test scene with single dialogue flow

### Phase 1: Character Controller (Days 3-5)
- Implement movement (WASD)
- Implement camera look (Mouse)
- Add animation support
- Integrate pause/resume with DialogueManager

### Phase 2: Inventory System (Days 6-7)
- Implement InventoryManager
- Create UI grid display
- Connect to DialogueManager item commands

### Phase 3: Content Creation (Days 8-11)
- Team creates dialogue nodes in Editor
- Build all 30+ nodes for prologue
- Test branching and conditional logic

### Phase 4: 3D Assets (Days 12-13)
- Model jungle environment
- Model house interior
- Model portal chamber
- Setup lighting and atmosphere

### Phase 5: Integration & Testing (Days 14-15)
- Full system integration test
- Debug any cross-system issues
- Create final build

### Phase 6: Polish & Defense (Days 16+)
- Performance optimization
- Bug fixes
- Demo rehearsal

---

## PART 7: SUCCESS CRITERIA

### Technical Criteria
- ✅ All systems compile without errors
- ✅ 60+ FPS maintained consistently
- ✅ Dialogue branching functional with 3+ paths
- ✅ Variables persist and affect outcomes
- ✅ Conditional choices working correctly
- ✅ UI responsive and polished

### Content Criteria
- ✅ 30+ dialogue nodes created
- ✅ 3 distinct scenes with unique environments
- ✅ Multiple endings visible in 15-minute demo
- ✅ Choices meaningfully affect story
- ✅ Atmosphere consistent throughout

### Presentation Criteria
- ✅ 15-minute playable prologue
- ✅ Professional code explaining capability
- ✅ Clear demonstration of SOLID principles
- ✅ Team ready to explain architecture
- ✅ Build works on different machines

---

## PART 8: IMPLEMENTATION NOTES FOR CURSOR AI

### Dialogue System Window Design

**Editor Window Features:**
1. **Graph View:**
   - Visual representation of all nodes
   - Connections between nodes show choice flow
   - Color coding by scene

2. **Node Inspector:**
   - Edit dialogue text inline
   - Add/remove choices
   - Assign characters and audio
   - View and edit impacts

3. **Tree Manager:**
   - Browse all dialogue trees
   - Create new trees
   - Link start nodes
   - Preview dialogue flow

4. **Variable Inspector:**
   - View current variable values (in play mode)
   - Real-time impact tracking
   - Condition testing

**Implementation Approach:**
- Use Unity Editor Window API
- Scriptable Objects as data backend
- Graph visualization using Editor drawing
- Gizmos for visual feedback

---

**This document combines full GDD + TDD into actionable specifications for development. Use this to train Cursor AI on the complete vision.**
