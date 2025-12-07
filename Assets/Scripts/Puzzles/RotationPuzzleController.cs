using UnityEngine;
using System;
using System.Collections.Generic;
using WhisperingGate.Core;
using WhisperingGate.Dialogue;

namespace WhisperingGate.Puzzles
{
    /// <summary>
    /// Controls a grid-based rotation puzzle.
    /// Handles solve mode, element selection, and completion checking.
    /// </summary>
    public class RotationPuzzleController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private RotationPuzzleConfig config;

        [Header("Grid Setup")]
        [Tooltip("Parent transform for spawned elements (optional)")]
        [SerializeField] private Transform elementsParent;

        [Tooltip("Prefab for rotatable elements (uses cube if empty)")]
        [SerializeField] private GameObject elementPrefab;

        [Tooltip("Pre-placed elements (if not spawning dynamically)")]
        [SerializeField] private List<RotatableElement> preplacedElements = new List<RotatableElement>();

        [Header("Solve Mode Settings")]
        [Tooltip("Key to exit solve mode")]
        [SerializeField] private KeyCode exitKey = KeyCode.Escape;

        [Tooltip("Alternative exit key")]
        [SerializeField] private KeyCode exitKeyAlt = KeyCode.Tab;

        [Tooltip("Key to rotate clockwise")]
        [SerializeField] private KeyCode rotateClockwiseKey = KeyCode.E;

        [Tooltip("Key to rotate counter-clockwise")]
        [SerializeField] private KeyCode rotateCounterClockwiseKey = KeyCode.Q;

        [Header("Selection Navigation")]
        [Tooltip("Use arrow keys for navigation")]
        [SerializeField] private bool useArrowKeys = true;

        [Tooltip("Use WASD for navigation")]
        [SerializeField] private bool useWASD = true;

        [Header("Audio (Optional)")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip rotateSound;
        [SerializeField] private AudioClip solveSound;
        [SerializeField] private AudioClip selectSound;

        [Header("Debug")]
        [Tooltip("Enable debug logging for puzzle state")]
        [SerializeField] private bool enableDebugLogs = true;

        // Runtime state
        private List<RotatableElement> elements = new List<RotatableElement>();
        private bool isInSolveMode = false;
        private bool isSolved = false;
        private int selectedRow = 0;
        private int selectedCol = 0;
        private RotatableElement selectedElement;

        // Events
        public event Action OnPuzzleSolved;
        public event Action OnSolveModeEntered;
        public event Action OnSolveModeExited;
        public event Action<RotatableElement> OnElementRotated;

        /// <summary>
        /// Is the puzzle currently in solve mode?
        /// </summary>
        public bool IsInSolveMode => isInSolveMode;

        /// <summary>
        /// Has the puzzle been solved?
        /// </summary>
        public bool IsSolved => isSolved;

        /// <summary>
        /// Current puzzle configuration.
        /// </summary>
        public RotationPuzzleConfig Config => config;

        /// <summary>
        /// Currently selected element.
        /// </summary>
        public RotatableElement SelectedElement => selectedElement;

        private void Start()
        {
            if (config == null)
            {
                Debug.LogError($"[RotationPuzzle] No config assigned to {gameObject.name}");
                return;
            }

            InitializeElements();
        }

        private void Update()
        {
            if (!isInSolveMode || isSolved) return;

            HandleSolveModeInput();
        }

        /// <summary>
        /// Initialize or spawn puzzle elements based on config.
        /// </summary>
        private void InitializeElements()
        {
            elements.Clear();

            // Use pre-placed elements if available
            if (preplacedElements.Count > 0)
            {
                elements.AddRange(preplacedElements);
                
                // Initialize each element
                for (int i = 0; i < elements.Count && i < config.TotalElements; i++)
                {
                    int row = i / config.columns;
                    int col = i % config.columns;
                    elements[i].Initialize(config, row, col);
                    elements[i].OnRotationComplete += OnElementRotationComplete;
                }
            }
            else
            {
                // Spawn elements dynamically
                SpawnElements();
            }

            if (enableDebugLogs) Debug.Log($"[RotationPuzzle] '{config.puzzleId}' initialized with {elements.Count} elements");
        }

        /// <summary>
        /// Spawn elements based on config grid size.
        /// </summary>
        private void SpawnElements()
        {
            Transform parent = elementsParent != null ? elementsParent : transform;

            for (int row = 0; row < config.rows; row++)
            {
                for (int col = 0; col < config.columns; col++)
                {
                    Vector3 localPos = new Vector3(
                        col * config.elementSpacing,
                        row * config.elementSpacing,
                        0
                    );

                    // Center the grid
                    localPos.x -= (config.columns - 1) * config.elementSpacing * 0.5f;
                    localPos.y -= (config.rows - 1) * config.elementSpacing * 0.5f;

                    GameObject obj;
                    if (elementPrefab != null)
                    {
                        obj = Instantiate(elementPrefab, parent);
                    }
                    else
                    {
                        // Create default cube
                        obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        obj.transform.SetParent(parent);
                    }

                    obj.transform.localPosition = localPos;
                    obj.transform.localScale = Vector3.one * (config.elementSpacing * 0.8f);
                    obj.name = $"Element_{row}_{col}";

                    // Add or get RotatableElement component
                    var element = obj.GetComponent<RotatableElement>();
                    if (element == null)
                        element = obj.AddComponent<RotatableElement>();

                    element.Initialize(config, row, col);
                    element.OnRotationComplete += OnElementRotationComplete;
                    elements.Add(element);
                }
            }
        }

        /// <summary>
        /// Enter solve mode - player can now rotate elements.
        /// </summary>
        public void EnterSolveMode()
        {
            if (isSolved)
            {
                if (enableDebugLogs) Debug.Log($"[RotationPuzzle] '{config.puzzleId}' already solved");
                return;
            }

            if (isInSolveMode) return;

            isInSolveMode = true;
            selectedRow = 0;
            selectedCol = 0;
            UpdateSelection();

            // Focus camera if configured
            if (!string.IsNullOrWhiteSpace(config.cameraFocusPointId))
            {
                if (Camera.CameraFocusController.Instance != null)
                {
                    Camera.CameraFocusController.Instance.FocusOn(config.cameraFocusPointId);
                }
            }

            // Pause player movement
            if (Gameplay.PlayerController.Instance != null)
            {
                Gameplay.PlayerController.Instance.SetInputEnabled(false);
            }

            // Lock and hide cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            OnSolveModeEntered?.Invoke();
            if (enableDebugLogs) Debug.Log($"[RotationPuzzle] Entered solve mode for '{config.puzzleId}'");
        }

        /// <summary>
        /// Exit solve mode - return to normal gameplay.
        /// </summary>
        public void ExitSolveMode()
        {
            if (!isInSolveMode) return;

            isInSolveMode = false;

            // Deselect current element
            if (selectedElement != null)
            {
                selectedElement.SetSelected(false);
                selectedElement = null;
            }

            // Release camera focus
            if (Camera.CameraFocusController.Instance != null)
            {
                Camera.CameraFocusController.Instance.ReleaseFocus();
            }

            // Resume player movement
            if (Gameplay.PlayerController.Instance != null)
            {
                Gameplay.PlayerController.Instance.SetInputEnabled(true);
            }

            OnSolveModeExited?.Invoke();
            if (enableDebugLogs) Debug.Log($"[RotationPuzzle] Exited solve mode for '{config.puzzleId}'");
        }

        /// <summary>
        /// Handle input during solve mode.
        /// </summary>
        private void HandleSolveModeInput()
        {
            // Exit solve mode
            if (Input.GetKeyDown(exitKey) || Input.GetKeyDown(exitKeyAlt))
            {
                ExitSolveMode();
                return;
            }

            // Navigation
            int rowDelta = 0;
            int colDelta = 0;

            if (useArrowKeys)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow)) rowDelta = 1;
                if (Input.GetKeyDown(KeyCode.DownArrow)) rowDelta = -1;
                if (Input.GetKeyDown(KeyCode.LeftArrow)) colDelta = -1;
                if (Input.GetKeyDown(KeyCode.RightArrow)) colDelta = 1;
            }

            if (useWASD)
            {
                if (Input.GetKeyDown(KeyCode.W)) rowDelta = 1;
                if (Input.GetKeyDown(KeyCode.S)) rowDelta = -1;
                if (Input.GetKeyDown(KeyCode.A)) colDelta = -1;
                if (Input.GetKeyDown(KeyCode.D)) colDelta = 1;
            }

            if (rowDelta != 0 || colDelta != 0)
            {
                NavigateSelection(rowDelta, colDelta);
            }

            // Rotation
            if (Input.GetKeyDown(rotateClockwiseKey))
            {
                RotateSelected(true);
            }
            else if (Input.GetKeyDown(rotateCounterClockwiseKey))
            {
                RotateSelected(false);
            }
        }

        /// <summary>
        /// Navigate element selection.
        /// </summary>
        private void NavigateSelection(int rowDelta, int colDelta)
        {
            int newRow = Mathf.Clamp(selectedRow + rowDelta, 0, config.rows - 1);
            int newCol = Mathf.Clamp(selectedCol + colDelta, 0, config.columns - 1);

            if (newRow != selectedRow || newCol != selectedCol)
            {
                selectedRow = newRow;
                selectedCol = newCol;
                UpdateSelection();
                PlaySound(selectSound);
            }
        }

        /// <summary>
        /// Update which element is visually selected.
        /// </summary>
        private void UpdateSelection()
        {
            // Deselect previous
            if (selectedElement != null)
            {
                selectedElement.SetSelected(false);
            }

            // Find and select new element
            selectedElement = GetElementAt(selectedRow, selectedCol);
            if (selectedElement != null)
            {
                selectedElement.SetSelected(true);
            }
        }

        /// <summary>
        /// Rotate the currently selected element.
        /// </summary>
        private void RotateSelected(bool clockwise)
        {
            if (selectedElement == null || selectedElement.IsRotating) return;

            if (clockwise)
                selectedElement.RotateNext();
            else
                selectedElement.RotatePrevious();

            PlaySound(rotateSound);
            OnElementRotated?.Invoke(selectedElement);
            
            // Debug log for rotation state
            if (enableDebugLogs)
            {
                int targetIndex = clockwise 
                    ? (selectedElement.CurrentRotationIndex + 1) % config.rotationSteps
                    : (selectedElement.CurrentRotationIndex - 1 + config.rotationSteps) % config.rotationSteps;
                    
                // Note: CurrentRotationIndex won't update until animation completes,
                // so we calculate what the target will be
                int solutionIndex = config.GetSolutionIndex(selectedElement.Row, selectedElement.Column);
                bool willBeCorrect = (targetIndex == solutionIndex) || (solutionIndex < 0);
                
                Debug.Log($"[RotationPuzzle] Element [{selectedElement.Row},{selectedElement.Column}] rotating to index {targetIndex} " +
                          $"(Solution: {solutionIndex}) → {(willBeCorrect ? "✓ CORRECT" : "✗ Wrong")}");
            }
        }

        /// <summary>
        /// Called when an element finishes rotating.
        /// </summary>
        private void OnElementRotationComplete(RotatableElement element)
        {
            element.UpdateCorrectState();
            
            // Debug log after rotation completes
            if (enableDebugLogs)
            {
                int solutionIndex = config.GetSolutionIndex(element.Row, element.Column);
                bool isCorrect = element.IsCorrect;
                Debug.Log($"[RotationPuzzle] Element [{element.Row},{element.Column}] now at index {element.CurrentRotationIndex} " +
                          $"(Solution: {solutionIndex}) → {(isCorrect ? "<color=green>✓ CORRECT</color>" : "<color=red>✗ Wrong</color>")}");
                
                // Show overall progress
                int correctCount = 0;
                foreach (var el in elements)
                {
                    if (el.IsCorrect) correctCount++;
                }
                Debug.Log($"[RotationPuzzle] Progress: {correctCount}/{elements.Count} elements correct");
            }
            
            CheckSolution();
        }

        /// <summary>
        /// Check if puzzle is solved.
        /// </summary>
        private void CheckSolution()
        {
            foreach (var element in elements)
            {
                if (!element.IsCorrect)
                    return;
            }

            // All elements correct - puzzle solved!
            SolvePuzzle();
        }

        /// <summary>
        /// Handle puzzle completion.
        /// </summary>
        private void SolvePuzzle()
        {
            if (isSolved) return;

            isSolved = true;
            isInSolveMode = false;

            // Deselect element
            if (selectedElement != null)
            {
                selectedElement.SetSelected(false);
                selectedElement = null;
            }

            PlaySound(solveSound);
            Debug.Log($"[RotationPuzzle] '{config.puzzleId}' SOLVED!");

            // Check if onSolvedCommands contains a camera command
            bool hasCameraCommand = false;
            if (config.onSolvedCommands != null)
            {
                foreach (string cmd in config.onSolvedCommands)
                {
                    if (!string.IsNullOrWhiteSpace(cmd) && cmd.Trim().StartsWith("cam:", System.StringComparison.OrdinalIgnoreCase))
                    {
                        hasCameraCommand = true;
                        break;
                    }
                }
            }

            // Execute commands (may include camera commands)
            ExecuteCommands(config.onSolvedCommands);

            // Only handle camera here if NO camera command was in onSolvedCommands
            // This prevents the solve-mode camera from overriding the command-based camera
            if (!hasCameraCommand)
            {
                // No cam command in onSolvedCommands, so release the solve-mode camera
                if (Camera.CameraFocusController.Instance != null)
                {
                    Camera.CameraFocusController.Instance.ReleaseFocus();
                }
            }
            // If there WAS a camera command, it already handled the camera - don't touch it

            // Resume player
            if (Gameplay.PlayerController.Instance != null)
            {
                Gameplay.PlayerController.Instance.SetInputEnabled(true);
            }

            OnPuzzleSolved?.Invoke();
        }

        /// <summary>
        /// Get element at specific grid position.
        /// </summary>
        public RotatableElement GetElementAt(int row, int col)
        {
            int index = row * config.columns + col;
            if (index >= 0 && index < elements.Count)
                return elements[index];
            return null;
        }

        /// <summary>
        /// Get all elements.
        /// </summary>
        public List<RotatableElement> GetAllElements()
        {
            return new List<RotatableElement>(elements);
        }

        /// <summary>
        /// Reset puzzle to initial state.
        /// </summary>
        public void ResetPuzzle()
        {
            isSolved = false;
            
            foreach (var element in elements)
            {
                int row = element.Row;
                int col = element.Column;
                
                int startIndex = config.GetStartingIndex(row, col);
                if (startIndex < 0)
                    startIndex = UnityEngine.Random.Range(0, config.rotationSteps);
                
                element.SetRotationIndex(startIndex);
                element.ResetVisuals();
            }

            if (enableDebugLogs) Debug.Log($"[RotationPuzzle] '{config.puzzleId}' reset");
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        /// <summary>
        /// Execute a list of commands.
        /// </summary>
        private void ExecuteCommands(List<string> commands)
        {
            if (commands == null) return;

            foreach (string command in commands)
            {
                if (string.IsNullOrWhiteSpace(command)) continue;

                int colonIndex = command.IndexOf(':');
                if (colonIndex > 0)
                {
                    string type = command.Substring(0, colonIndex).ToLower().Trim();
                    string param = command.Substring(colonIndex + 1).Trim();
                    ExecuteCommand(type, param);
                }
            }
        }

        private void ExecuteCommand(string type, string param)
        {
            switch (type)
            {
                case "flag":
                    if (GameState.Instance != null)
                        GameState.Instance.SetBool(param, true);
                    if (enableDebugLogs) Debug.Log($"[RotationPuzzle] Set flag: {param}");
                    break;

                case "unflag":
                    if (GameState.Instance != null)
                        GameState.Instance.SetBool(param, false);
                    if (enableDebugLogs) Debug.Log($"[RotationPuzzle] Cleared flag: {param}");
                    break;

                case "var":
                    ParseAndSetVariable(param);
                    break;

                case "cam":
                    if (Camera.CameraFocusController.Instance != null)
                    {
                        string[] camParts = param.Split(':');
                        string camTarget = camParts[0].Trim();
                        float camDuration = -1f;

                        if (camParts.Length > 1 && float.TryParse(camParts[1].Trim(), out float parsedDuration))
                            camDuration = parsedDuration;

                        if (camTarget.Equals("reset", System.StringComparison.OrdinalIgnoreCase))
                            Camera.CameraFocusController.Instance.ReleaseFocus();
                        else
                            Camera.CameraFocusController.Instance.FocusOn(camTarget, camDuration);
                    }
                    break;

                case "door":
                    // Format: door:action:door_id or door:door_id (defaults to open)
                    {
                        string[] doorParts = param.Split(':');
                        string doorAction = doorParts.Length > 1 ? doorParts[0].Trim() : "open";
                        string doorId = doorParts.Length > 1 ? doorParts[1].Trim() : doorParts[0].Trim();
                        Interaction.Door.ExecuteCommand(doorAction, doorId);
                        if (enableDebugLogs) Debug.Log($"[RotationPuzzle] Door command: {doorAction} {doorId}");
                    }
                    break;

                case "activate":
                    Interaction.ActivatableObject.ExecuteCommand("activate", param);
                    if (enableDebugLogs) Debug.Log($"[RotationPuzzle] Activate: {param}");
                    break;

                case "deactivate":
                    Interaction.ActivatableObject.ExecuteCommand("deactivate", param);
                    if (enableDebugLogs) Debug.Log($"[RotationPuzzle] Deactivate: {param}");
                    break;

                default:
                    if (enableDebugLogs) Debug.Log($"[RotationPuzzle] Unknown command: {type}");
                    break;
            }
        }

        private void ParseAndSetVariable(string param)
        {
            if (GameState.Instance == null) return;

            int plusIndex = param.IndexOf('+');
            int minusIndex = param.IndexOf('-');

            if (plusIndex > 0)
            {
                string varName = param.Substring(0, plusIndex);
                if (int.TryParse(param.Substring(plusIndex + 1), out int value))
                {
                    int current = GameState.Instance.GetInt(varName);
                    GameState.Instance.SetInt(varName, current + value);
                    if (enableDebugLogs) Debug.Log($"[RotationPuzzle] {varName} += {value}");
                }
            }
            else if (minusIndex > 0)
            {
                string varName = param.Substring(0, minusIndex);
                if (int.TryParse(param.Substring(minusIndex + 1), out int value))
                {
                    int current = GameState.Instance.GetInt(varName);
                    GameState.Instance.SetInt(varName, current - value);
                    if (enableDebugLogs) Debug.Log($"[RotationPuzzle] {varName} -= {value}");
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var element in elements)
            {
                if (element != null)
                    element.OnRotationComplete -= OnElementRotationComplete;
            }
        }

        /// <summary>
        /// Set the solved state directly (used by save system).
        /// </summary>
        public void SetSolvedState(bool solved)
        {
            isSolved = solved;
            isInSolveMode = false;

            if (solved && config != null)
            {
                // Set all elements to their correct/solved positions
                for (int i = 0; i < elements.Count && i < config.solutionIndices.Count; i++)
                {
                    if (elements[i] != null)
                    {
                        elements[i].SetRotationIndex(config.solutionIndices[i]);
                        elements[i].SetSelected(false);
                    }
                }
                Debug.Log($"[RotationPuzzle] '{config.puzzleId}' restored as solved");
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (config == null) return;

            // Draw grid preview
            Gizmos.color = Color.cyan;
            
            for (int row = 0; row < config.rows; row++)
            {
                for (int col = 0; col < config.columns; col++)
                {
                    Vector3 localPos = new Vector3(
                        col * config.elementSpacing,
                        row * config.elementSpacing,
                        0
                    );
                    localPos.x -= (config.columns - 1) * config.elementSpacing * 0.5f;
                    localPos.y -= (config.rows - 1) * config.elementSpacing * 0.5f;

                    Vector3 worldPos = transform.TransformPoint(localPos);
                    Gizmos.DrawWireCube(worldPos, Vector3.one * config.elementSpacing * 0.8f);
                }
            }
        }
#endif
    }
}

