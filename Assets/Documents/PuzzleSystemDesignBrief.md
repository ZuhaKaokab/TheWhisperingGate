Here’s a ready-to-send markdown brief for Cursor. You can drop this straight into a new Cursor chat.

***

```markdown
# Puzzle Systems Design Brief
## The Whispering Gate – Grid Path & Rotating Symbol Puzzles

**Author:** [Your Name]  
**Engine:** Unity 2021.3+  
**Language:** C#  
**Architecture Style:** SOLID, event-driven, ScriptableObject-driven (must fit existing project architecture)

---

## 0. Context: Existing Architecture (Important)

Before designing anything, **read and understand** these existing systems in the project:

- `GameState` – central variable/flag store with condition evaluation
- `DialogueManager` – event-driven, ScriptableObject-based dialogue system
- `InventoryManager` – item storage, events, UI integration
- `Interaction System` – “Press E to interact” + generic interactables
- `Level / Segment Management` – scene transitions & gates triggered by flags
- `Architecture Reference` + `Project Overview` docs (already in repo)

**Goal:** New puzzle systems must:

1. Be **modular** and reusable (no puzzle-specific hardcoding).
2. Be **data-driven** through ScriptableObjects, similar to Dialogue/Inventory.
3. Communicate through **GameState, events, and commands**, not tight coupling.
4. Follow **SOLID**, with clean separation of config, runtime logic, and presentation.
5. Be friendly to designers (puzzle answers editable in Inspector).

Before coding, **research** (briefly, but properly):

- Grid-based puzzle patterns in Unity (data-driven design)
- Rotating ring / symbol puzzles (common adventure-game patterns)
- Using ScriptableObjects as configuration assets
- State machine & command pattern usage for mini-game controllers
- Good practices for modular puzzle frameworks in Unity

Summarize key findings and reflect them in architecture choices.

---

## 1. Puzzle System Overview

We need **two independent but stylistically consistent puzzle systems**:

1. **Grid Path Puzzle (5×5 “floor puzzle”)**
   - Location: Before entering the abandoned house.
   - Player walks on a 5×5 grid of tiles.
   - Only certain tiles, in a defined sequence, form the “correct path”.
   - Stepping on the correct next tile:
     - Visually pushes that tile down slightly.
     - Plays SFX (stone click / rumble).
   - Stepping on a wrong tile:
     - All tiles reset to default height/state.
     - Optional: minor penalty (e.g. small sanity loss via GameState).
   - Puzzle restarts until the full sequence is walked correctly.
   - When solved:
     - Set GameState flag(s) and/or variables (e.g. `flag:grid_path_solved`).
     - Trigger downstream effects (open path to house, camera focus, dialogue, etc.).

2. **Rotating Symbol Puzzle (portal ignition)**
   - Location: After leaving the house, in front of the portal.
   - 3–4 concentric stone rings with symbols.
   - Each ring can be rotated through N discrete symbol positions.
   - Goal: Align rings into a **specific combination** defined in data.
   - Hints come from journal pages / wall carvings / dialogue.
   - When all rings match the configured “answer”:
     - Rings lock with strong SFX.
     - Portal visual FX ignite.
     - Set GameState flag(s) (e.g. `flag:portal_ignited`, `flag:prologue_complete`).
     - Potentially trigger ending dialogue/cutscene.

The systems should **not know** about specific story beats. They should only:

- Manage puzzle states.
- Expose **events** and **commands** that other systems react to.

---

## 2. Shared Architectural Principles

Both puzzle types should follow a shared pattern:

1. **ScriptableObject Config Assets**  
   - Define puzzle parameters & answers.
   - Completely decouple design from code.

2. **Controller MonoBehaviours**  
   - Read config and manage runtime state.
   - Talk to GameState, InventoryManager, DialogueManager via:
     - Events
     - Command strings (reuse existing command pattern if possible)
     - Or minimal, well‑abstracted service interfaces.

3. **View/Presentation Layer**  
   - Handles visuals only (VFX, SFX, animations).
   - Subscribes to controller events.

4. **Event-Driven Integration**  
   - Using C# events or UnityEvents to notify:
     - OnPuzzleStarted
     - OnPuzzleProgressed
     - OnPuzzleFailed
     - OnPuzzleSolved

5. **SOLID**  
   - Config objects: SRP – just data.
   - Controllers: SRP – just puzzle logic.
   - View components: SRP – just visuals.
   - No hard-coded references to story, scenes, or dialogue assets.

Please choose and document **concrete patterns** (e.g. State Machine + Command pattern) where they add clarity.

---

## 3. Puzzle #1 – Grid Path Puzzle (5×5)

### 3.1 Design Goals

- Work for **any** grid size, but default 5×5.
- Support at least two modes:
  - **ExactSequence:** Player must step tiles in a specific ordered path.
  - **SafeZone:** Player can step on any “safe” tiles; unsafe tiles reset puzzle.
- Allow the “correct path” to be edited without code changes.
- Simple enough to re-skin for future levels (different meshes/materials).

### 3.2 Data Model – `GridPuzzleConfig` (ScriptableObject)

Proposed fields (feel free to refine, but keep intent):

```
public enum GridPuzzleMode
{
    ExactSequence,
    SafeZone
}

[CreateAssetMenu(menuName = "WhisperingGate/Puzzles/GridPuzzleConfig")]
public class GridPuzzleConfig : ScriptableObject
{
    public string puzzleId;

    public int rows = 5;
    public int cols = 5;
    public GridPuzzleMode mode;

    // For ExactSequence mode
    public List<Vector2Int> correctPath; // ordered sequence (x,y)

    // For SafeZone mode (optional)
    public List<Vector2Int> safeTiles;   // tiles that are allowed

    public Vector2Int startTile;
    public Vector2Int endTile;

    // Commands to run on solve/fail (reuse dialogue command pattern if possible)
    public List<string> onSolvedCommands;   // e.g. ["flag:grid_solved", "cam:focus_house_door"]
    public List<string> onFailedCommands;   // optional (e.g. ["var:insanity-5"])

    // Optional: audio/FX ids, tuning parameters, etc.
}
```

Requirements:

- Implement a **clean, inspector‑friendly** editor for assigning `correctPath` and `safeTiles`.
- Ideally allow recording a path by walking tiles in play mode and clicking a “Record Path” button.

### 3.3 Runtime Controller – `GridPuzzleController`

Responsibilities:

- Link to a `GridPuzzleConfig` asset.
- Maintain current puzzle state (index into path, which tiles are pressed).
- Listen for tile interaction events (from `GridTile` components or a central input handler).
- Validate steps according to `GridPuzzleMode`.
- Trigger reset / success flows.

Key behaviours:

- On player stepping on tile `(x,y)`:
  - **ExactSequence:**
    - Check if this equals `correctPath[currentIndex]`.
    - If yes:
      - Sink tile, play “correct” feedback.
      - Increment `currentIndex`.
      - If `currentIndex == correctPath.Count` → puzzle solved.
    - If no:
      - Play “fail” feedback.
      - Reset all tiles & `currentIndex = 0`.
      - Invoke `onFailed` events/commands.
  - **SafeZone:**
    - If tile not in `safeTiles` → fail & reset.
    - Else → sink tile; if at `endTile` with required conditions → solved.

Events to expose:

- `event Action OnPuzzleStarted;`
- `event Action<int> OnProgressChanged;` (e.g. currentIndex)
- `event Action OnPuzzleFailed;`
- `event Action OnPuzzleSolved;`

Integration:

- On solved:
  - Execute `onSolvedCommands` via existing command execution system (e.g. same as DialogueManager uses).
  - Set `flag:grid_path_solved` in GameState through a command like `flag:grid_path_solved`.

---

## 4. Puzzle #2 – Rotating Symbol Puzzle

### 4.1 Design Goals

- N rings (3–4) around a central portal.
- Each ring has M discrete symbols (e.g. 6–8).
- Correct solution: each ring at configured index or symbol ID.
- Designer can change:
  - Number of rings.
  - Available symbols per ring.
  - Correct combination.
- Support:
  - Lock-in feedback when puzzle is solved.
  - Optional per-ring feedback when ring is in correct position.

### 4.2 Data Model – `RotatingPuzzleConfig` (ScriptableObject)

Proposed fields:

```
[CreateAssetMenu(menuName = "WhisperingGate/Puzzles/RotatingPuzzleConfig")]
public class RotatingPuzzleConfig : ScriptableObject
{
    public string puzzleId;

    public List<RingConfig> rings;

    public List<string> onSolvedCommands;   // e.g. ["flag:portal_ignited", "cam:portal_closeup"]
    public List<string> onFailedCommands;   // optional, usually not needed

    [System.Serializable]
    public class RingConfig
    {
        public string ringId;
        public int totalPositions;        // e.g. 6 symbols around the ring
        public int correctIndex;         // 0..totalPositions-1
        // Optional: mapping to textures or material variants
        // public List<Sprite> symbolSprites;
    }
}
```

### 4.3 Runtime Controller – `RotatingPuzzleController`

Responsibilities:

- Owns the overall puzzle state (current index of each ring).
- Exposes ring‑wise operations (rotate left/right).
- Determines when all rings match their `correctIndex`.
- Talks to:
  - Visual ring components (for rotation & symbol swap).
  - GameState/Dialogue/Level via commands on solve.

Behaviour:

- Each ring is a child object with `RotatingRing` component:
  - Knows its current index.
  - Handles input when selected (e.g. E selects ring, A/D rotates).
  - Notifies controller when its index changes.

- Controller checks after each rotation:
  - If `currentIndex[i] == correctIndex[i]` for all rings → solved.

Events:

- `event Action<int, int> OnRingRotated;` (ringIndex, currentIndex)
- `event Action OnPuzzleSolved;`

On solved:

- Visually:
  - Lock rings (disable further rotation).
  - Trigger portal VFX / SFX.
- Logically:
  - Execute `onSolvedCommands` through shared command system.
  - Set `flag:portal_ignited`, maybe `flag:prologue_complete`.

---

## 5. Integration with Existing Systems

Both puzzles must integrate with:

### 5.1 GameState

- Use existing API to:
  - Set flags: `SetBool("grid_path_solved", true)` via commands.
  - Modify variables for failure penalties (optional).
- Optionally, allow conditions:
  - Example: Rotating puzzle only interactable if `flag:met_writer == true`.

### 5.2 Interaction System

- Puzzles should activate via existing interaction flow:
  - Player looks at puzzle object → Prompt “Press E to interact”.
  - On E:
    - Puzzle UI / control mode activates (disables normal movement if needed).
    - On exit/solve, return control to player.

### 5.3 Command System (from Dialogue)

- Reuse existing command parsing/execution to avoid code duplication:
  - `flag:xxx`, `var:xxx+N`, `item:xxx`, `cam:xxx`, `ending:xxx`.
- Puzzle configs just define string commands; an executor handles them.

### 5.4 Camera System

- Optionally:
  - On puzzle start: run `cam:focus_puzzle_x` command to move camera to scenic angle.
  - On puzzle end: run `cam:reset`.

---

## 6. Implementation Priorities & Expectations

1. **Architecture First:**
   - Before full code, propose and document:
     - Class diagrams (high-level).
     - Data flow for each puzzle.
     - Event flow for start/progress/fail/solve.
   - Keep patterns consistent with Dialogue & Inventory.

2. **Config & Extensibility:**
   - Support **multiple instances** of each puzzle type using different configs.
   - Zero code changes required to create a new puzzle: just create a new ScriptableObject, hook into scene, and define commands.

3. **Editor UX:**
   - If possible within time, add helper tooling:
     - For grid: “Record path” mode or click-to-define in editor.
     - For rotating puzzle: simple inspector for ring counts and positions.

4. **Testing:**
   - Provide simple test scenes:
     - `GridPuzzle_Test.unity`
     - `RotatingPuzzle_Test.unity`
   - Each scene should:
     - Spawn a temporary player.
     - Allow quick entering/exiting puzzles.
     - Log important events to Console.

5. **Code Quality:**
   - Follow our existing conventions and SOLID practices.
   - Document public classes/methods with XML comments.
   - Use events rather than hard references for cross-system communication.

---

## 7. Deliverables

- `GridPuzzleConfig.cs` (ScriptableObject)
- `GridPuzzleController.cs`
- `GridTile.cs` (per-tile component)
- Optional: editor helpers for path authoring

- `RotatingPuzzleConfig.cs` (ScriptableObject)
- `RotatingPuzzleController.cs`
- `RotatingRing.cs` (per-ring component)

- Test scenes and minimal sample configs:
  - `GridPuzzleConfig_Test.asset`
  - `RotatingPuzzleConfig_Test.asset`

- Short `PUZZLE-SYSTEMS-OVERVIEW.md` in the repo explaining:
  - How to create a new grid puzzle.
  - How to create a new rotating puzzle.
  - How to hook them into GameState/commands.

---

## 8. First Task for You (Cursor)

1. Confirm you understand the existing architecture (Dialogue, GameState, Inventory, Interaction).
2. Propose a brief architecture plan (classes, responsibilities) for both puzzles.
3. Then implement **Grid Path Puzzle** first (config + controller + tiles + test scene).
4. After validation, implement **Rotating Symbol Puzzle** in the same style.

Ask clarifying questions if anything seems ambiguous before coding.
```

---