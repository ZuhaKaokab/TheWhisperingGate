# The Whispering Gate - Technical Design Document (TDD)

## 1. Document Control
- **Project:** The Whispering Gate
- **Engine:** Unity 2021.3+
- **Language:** C#
- **Genre:** Psychological horror narrative adventure
- **Current Scope:** Prologue-focused vertical slice with branching dialogue, puzzle interactions, and persistence
- **Audience:** Developers, technical reviewers, QA, future contributors

---

## 2. Technical Vision
The codebase is designed around a modular runtime architecture where gameplay systems are separated into manager-style subsystems (dialogue, state, inventory, level flow, save/load, puzzles, journal, environment). Narrative progression is driven by data assets (ScriptableObjects) and command-based triggers, allowing designers to iterate on content without rewriting core logic.

Primary goals:
- Decouple content authoring from code.
- Keep runtime systems event-driven and testable.
- Preserve player progression across scenes and sessions.
- Support scalable narrative branching and puzzle integration.

---

## 3. Technology Stack
- **Core runtime:** Unity (MonoBehaviour lifecycle, SceneManager, CharacterController, ScriptableObjects)
- **Programming:** C# with namespaced modules (e.g., `WhisperingGate.Dialogue`, `WhisperingGate.Gameplay`)
- **UI:** Unity UI + TextMeshPro
- **Persistence:** JSON serialization via `JsonUtility`, local filesystem storage in `Application.persistentDataPath`
- **Data containers:** ScriptableObject assets for dialogue, characters, items, puzzle configs, and journal pages
- **Input model:** Unity classic input bindings (`Input.GetAxis`, `Input.GetKeyDown`, etc.)

---

## 4. High-Level Architecture

### 4.1 Architectural Style
Hybrid architecture combining:
- **Manager singletons** for globally accessible runtime services.
- **Event-driven communication** for UI/system reactions.
- **Command parsing** for content-driven side effects from dialogue/puzzle nodes.
- **Data-driven content** using ScriptableObjects.

### 4.2 Core Runtime Layers
- **Presentation/UI Layer**
  - Dialogue panel, inventory display, notifications, journal UI.
- **Gameplay Logic Layer**
  - Player control, interaction triggers, puzzle controllers, level progression.
- **Narrative/State Layer**
  - Dialogue orchestration, GameState flags/variables/conditions.
- **Persistence Layer**
  - SaveManager gather/apply workflow.
- **Content Layer**
  - Dialogue nodes/trees, choices, items, puzzle configs, journal config/pages.

---

## 5. Source Structure Overview
Primary folders:
- `Assets/Scripts/Runtime`
  - Shared managers (`DialogueManager`, `GameState`, `LevelManager`)
- `Assets/Scripts/Gameplay`
  - Player, inventory, scene transitions, checkpoints
- `Assets/Scripts/Interaction`
  - Triggers, doors, activatable world objects
- `Assets/Scripts/Puzzles`
  - Grid and rotation puzzle systems + data configs
- `Assets/Scripts/Items`
  - Flashlight controllers/pickups
- `Assets/Scripts/Journal`
  - Journal state and UI integration
- `Assets/Scripts/SaveSystem`
  - Save/load orchestration and slot data
- `Assets/Scripts/UI`
  - Dialogue/inventory/notification presentation
- `Assets/Scripts/Camera`
  - Camera focus point runtime control
- `Assets/Scripts/Data`
  - Narrative and character ScriptableObject models

Supporting content:
- `Assets/Scenes` (main menu/gameplay scenes)
- `Assets/Documents` (setup guides, QA, design docs)

---

## 6. Core Systems Design

## 6.1 GameState System
**Responsibility:** Central source of truth for runtime flags, integers, and strings.

Key capabilities:
- Track boolean progress flags (e.g., encounters, unlocks).
- Track narrative stats and counters (trust/courage/etc.).
- Evaluate condition expressions consumed by dialogue and journal.
- Broadcast change events consumed by UI and progression logic.

Design intent:
- Avoid hard-coding narrative state into scene objects.
- Allow content-level conditions to stay declarative.

---

## 6.2 Dialogue System
**Primary component:** `DialogueManager`

Flow:
1. Start tree (or start at node / node ID).
2. Show node and execute start commands.
3. Filter visible choices by GameState conditions.
4. On player selection: apply impacts, execute end commands, advance.
5. End dialogue when reaching terminal condition or null next node.

Supported command categories:
- `item`, `flag`, `unflag`, `var`, `ending`
- `cam` (camera focus/release)
- `journal` (unlock/open/goto/pickup)
- `door` (open/close/toggle)
- `sky`, `activate`, `deactivate`, `flashlight`

Key events:
- `OnNodeDisplayed`, `OnChoicesUpdated`, `OnChoiceSelected`, `OnImpactApplied`, `OnItemGiven`, `OnDialogueEnded`

Design notes:
- Content-driven execution allows narrative designers to chain gameplay effects without bespoke code per node.
- Segment-boundary safety is implemented by ending dialogue if selected choice points to null.

---

## 6.3 Player and Camera Control
**Primary component:** `PlayerController`

Implemented behavior:
- Hybrid First Person / Third Person view switching.
- CharacterController-based walk, sprint, crouch, jump.
- Coyote-time style grounded grace for jump responsiveness.
- Input gating when dialogue or other systems require control lock.

Camera model:
- Uses first-person and third-person anchors.
- Smooth follow and look interpolation.
- Defers control while cinematic focus is active (`CameraFocusController`).

---

## 6.4 Interaction Model
Interaction scripts (`DialogueTrigger`, segment triggers, doors, activatables) expose gameplay actions at world points.

Common pattern:
- Proximity/collider detection.
- Contextual player action (typically `E`).
- Trigger runtime managers (dialogue, door logic, puzzle mode, etc.).

This keeps world logic thin and delegates business rules to managers.

---

## 6.5 Inventory System
**Responsibility:** Item ownership state + UI reflection.

Capabilities:
- Add/remove/query items by ID.
- Sync quick-access hotbar and full inventory panel.
- Display item details and metadata.
- Integrate with dialogue commands (`item:<id>`).

Design choice:
- IDs and ScriptableObject definitions keep item content scalable.

---

## 6.6 Journal System
**Primary component:** `JournalManager`

Capabilities:
- Tracks whether player owns journal.
- Unlocks pages by default, by direct command, or by GameState condition.
- Records viewed/unviewed pages.
- Opens/closes UI and pauses player input while reading.

Command integration:
- `journal:unlock:<page>`
- `journal:open`
- `journal:goto:<page>`
- `journal:pickup`

---

## 6.7 Puzzle Systems

### Grid Puzzle
- Configurable grid dimensions and correct path data.
- Supports exact-sequence and safe-zone modes.
- Wrong step -> fail feedback + reset flow.
- Solved state triggers command list and lock behavior.

### Rotation Puzzle
- Grid of rotatable elements with configurable target orientations.
- Solve mode with navigation and rotation controls.
- Camera focus + player input lock while solving.
- Completion validates all elements and executes solved commands.

Shared puzzle design pattern:
- Config asset defines puzzle-specific data.
- Controller owns runtime state and emits progression events.
- Command execution hooks puzzle outcomes into broader narrative/gameplay flow.

---

## 6.8 Level and Segment Progression
**Primary component:** `LevelManager`

Responsibilities:
- Track completed segments.
- Manage checkpoint activation and restoration.
- Publish level/segment/checkpoint events.
- Support saved progress restore hooks.

Design intent:
- Narrative segmentation allows scene-local progression gates while preserving global state.

---

## 6.9 Save/Load System
**Primary component:** `SaveManager`

Persistence model:
- Multi-slot JSON saves with metadata and optional obfuscation.
- Auto-save interval and scene-change auto-save.
- Quick save/load hotkeys.
- Slot summary cache for menu display.

Saved domains:
- Player position/rotation and flashlight state
- GameState flags/ints/strings
- Inventory items/counts
- Level segment/checkpoint progress
- Puzzle solved IDs
- Environment mood data
- Current scene + playtime metadata

Load strategy:
1. Validate target scene.
2. Load scene when needed.
3. Apply aggregated state to runtime managers.
4. Emit load completion events.

---

## 7. Data Model Design

### 7.1 Narrative Data Assets
- `DialogueTree`: entry point and tree identity
- `DialogueNode`: speaker, line, commands, transition links
- `DialogueChoice`: text, condition, impacts, next node
- `CharacterData`: display metadata/portrait data

### 7.2 Gameplay Data Assets
- Item definitions (ID, icon, description, category)
- Puzzle configs (`GridPuzzleConfig`, `RotationPuzzleConfig`)
- Journal config/page assets (order, unlock conditions, flags)

Benefits:
- Non-programmers can author and tune content.
- Runtime logic remains generic and reusable.

---

## 8. Event and Command Orchestration

### 8.1 Event-driven Reactions
Managers emit events consumed by UI and other systems to avoid direct coupling.
Examples:
- Dialogue impacts -> impact notification UI
- GameState changes -> journal auto-unlock checks
- Puzzle solved -> progression/activation effects

### 8.2 Command Bus Pattern (String Commands)
Commands are parsed from content strings and routed to concrete handlers.

Advantages:
- Fast iteration for narrative scripting.
- Supports mixed gameplay effects from one dialogue/puzzle node.

Trade-offs:
- String-based commands are typo-prone; require validation/test pass.
- Strong documentation and content QA are essential.

---

## 9. Scene and Runtime Lifecycle

### 9.1 Scene Responsibilities
- `MainMenu`: entry, slot selection, bootstrapping
- Gameplay scenes: world traversal, triggers, dialogue segments, puzzles

### 9.2 Persistent Runtime Managers
Common managers are singleton + `DontDestroyOnLoad` to keep continuity.
Examples include dialogue/state/level/save managers.

### 9.3 Initialization Considerations
- Manager startup order should guarantee state availability before content triggers fire.
- UI scripts should guard against null manager refs during scene transitions.

---

## 10. Coding Patterns and Standards
- Namespace by domain (`WhisperingGate.<System>`).
- Keep per-system single responsibility where possible.
- Prefer events over direct cross-system references.
- Keep world trigger components thin.
- Expose debug logs as serialized toggles for development diagnostics.

Recommended improvements:
- Introduce command validation/editor tooling.
- Add centralized service bootstrap to reduce ad hoc initialization order risks.
- Add automated playmode regression tests for dialogue and save/load.

---

## 11. Performance and Scalability Notes
- Dialogue choice filtering and condition evaluation are lightweight for current scope.
- Save/load uses full-state snapshots; suitable for current project scale.
- Puzzle restoration scans scene objects (`FindObjectsOfType`) during load; acceptable for prologue scope, may need optimization for larger worlds.
- Frequent debug logging should be disabled in production builds.

---

## 12. Error Handling and Reliability
Current approach:
- Defensive null checks across manager and command flows.
- Warnings/errors logged when required references are missing.
- Fallback scene logic during load when saved scene is unavailable.
- Segment boundary safety when dialogue choice next node is null.

Future hardening:
- Add structured command parse errors with source asset reference.
- Add save file schema versioning/migration path.
- Add corruption fallback/recovery for malformed save files.

---

## 13. Security and Data Integrity
- Save files are local JSON (optional XOR obfuscation for light tamper resistance).
- No sensitive user data is stored in current design.
- For stronger tamper detection, add checksums/signatures to save payloads.

---

## 14. QA and Testability Strategy
- System-level harnesses exist for dialogue, inventory, GameState, and scene management.
- QA matrix and historical fixed bug regressions are maintained in:
  - `Assets/Documents/QA-TESTING-DOCUMENT.md`

Recommended automated targets:
- Condition parser unit tests.
- Dialogue branch traversal tests.
- Save/load roundtrip tests across scene boundaries.
- Puzzle solved-state persistence tests.

---

## 15. Extensibility Roadmap (Technical)
Designed extension points:
- Add new command handlers in dialogue/puzzle controllers.
- Add new ScriptableObject data types for content systems.
- Add additional puzzle controller variants using same command/event conventions.
- Extend save data schema to include audio/settings/objective logs.

Medium-term architectural opportunities:
- Move input to Unity Input System package abstraction.
- Introduce command registry instead of switch-heavy handlers.
- Replace singleton hard refs with dependency injection/service locator.

---

## 16. Technical Risks and Mitigations

| Risk | Impact | Mitigation |
|---|---|---|
| String command typo in content | Broken runtime event chain | Add editor validator + preflight content scan |
| Scene setup inconsistency | Null refs, missing manager behavior | Add scene setup checklist and bootstrap prefab |
| Save schema drift | Old saves fail after updates | Add version field + migration logic |
| Event subscription leaks | Duplicate behavior / memory issues | Enforce subscribe/unsubscribe lifecycle review |
| Manual data wiring load | Authoring mistakes | Build custom inspectors and validation tools |

---

## 17. Conclusion
The Whispering Gate codebase currently implements a strong vertical-slice architecture: data-driven narrative, event-based UI reactions, modular puzzle systems, and comprehensive persistence hooks. The design is suitable for continued prologue expansion and can scale with additional tooling, validation, and automated regression coverage.

