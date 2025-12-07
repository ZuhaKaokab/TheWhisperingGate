using UnityEngine;
using System;
using System.Collections.Generic;
using WhisperingGate.Core;
using WhisperingGate.Dialogue;

namespace WhisperingGate.Puzzles
{
    /// <summary>
    /// Controls a Grid Path Puzzle instance.
    /// Manages puzzle state, validates player steps, and triggers events.
    /// </summary>
    public class GridPuzzleController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private GridPuzzleConfig config;

        [Header("Grid Setup")]
        [Tooltip("Array of tile prefabs to randomly scatter across the grid")]
        [SerializeField] private GameObject[] tilePrefabs;
        
        [Tooltip("Legacy single prefab (used if tilePrefabs array is empty)")]
        [SerializeField] private GameObject tilePrefab;
        
        [Tooltip("Spacing between tiles")]
        [SerializeField] private float tileSpacing = 1.1f;
        
        [Tooltip("Parent transform for generated tiles")]
        [SerializeField] private Transform tilesParent;
        
        [Tooltip("Random seed for tile distribution (-1 for random each time)")]
        [SerializeField] private int randomSeed = -1;

        [Header("Trigger Zone Settings")]
        [Tooltip("Y offset for the trigger zone. Positive values move it UP. Use this to position the trigger at ground/walking level.")]
        [SerializeField] private float triggerHeightOffset = 0.5f;
        
        [Tooltip("Size of the trigger box (X, Y, Z). Y should be tall enough to catch the player.")]
        [SerializeField] private Vector3 triggerSize = new Vector3(1f, 1f, 1f);

        [Header("Runtime State")]
        [SerializeField] private bool isActive = false;
        [SerializeField] private bool isSolved = false;
        [SerializeField] private int currentPathIndex = 0;

        // Generated tiles
        private GridTile[,] tileGrid;
        private List<GridTile> allTiles = new List<GridTile>();

        // Events
        /// <summary>Fired when puzzle becomes active.</summary>
        public event Action OnPuzzleStarted;
        
        /// <summary>Fired when player makes progress (correct step).</summary>
        public event Action<int, int> OnProgressChanged; // (currentIndex, totalSteps)
        
        /// <summary>Fired when player makes a wrong step.</summary>
        public event Action OnPuzzleFailed;
        
        /// <summary>Fired when puzzle is completed.</summary>
        public event Action OnPuzzleSolved;

        /// <summary>Whether the puzzle is currently active.</summary>
        public bool IsActive => isActive;
        
        /// <summary>Whether the puzzle has been solved.</summary>
        public bool IsSolved => isSolved;
        
        /// <summary>Current progress in the path (ExactSequence mode).</summary>
        public int CurrentPathIndex => currentPathIndex;

        /// <summary>Puzzle configuration asset.</summary>
        public GridPuzzleConfig Config => config;

        private void Awake()
        {
            if (tilesParent == null)
                tilesParent = transform;
        }

        private void Start()
        {
            if (config != null && HasValidPrefabs())
            {
                GenerateGrid();
            }
            else
            {
                Debug.LogWarning($"[GridPuzzle] {gameObject.name}: Missing config or tile prefabs!");
            }
        }

        /// <summary>
        /// Check if we have at least one valid tile prefab.
        /// </summary>
        private bool HasValidPrefabs()
        {
            // Check array first
            if (tilePrefabs != null && tilePrefabs.Length > 0)
            {
                foreach (var prefab in tilePrefabs)
                {
                    if (prefab != null) return true;
                }
            }
            // Fall back to legacy single prefab
            return tilePrefab != null;
        }

        /// <summary>
        /// Get a random tile prefab from the available prefabs.
        /// </summary>
        private GameObject GetRandomTilePrefab(System.Random rng)
        {
            // Use array if available and has valid prefabs
            if (tilePrefabs != null && tilePrefabs.Length > 0)
            {
                // Filter out null entries
                var validPrefabs = new System.Collections.Generic.List<GameObject>();
                foreach (var prefab in tilePrefabs)
                {
                    if (prefab != null)
                        validPrefabs.Add(prefab);
                }

                if (validPrefabs.Count > 0)
                {
                    int index = rng.Next(validPrefabs.Count);
                    return validPrefabs[index];
                }
            }
            
            // Fall back to legacy single prefab
            return tilePrefab;
        }

        /// <summary>
        /// Generate the tile grid based on config.
        /// Uses multiple tile prefabs scattered randomly if available.
        /// </summary>
        [ContextMenu("Generate Grid")]
        public void GenerateGrid()
        {
            // Clear existing tiles
            ClearGrid();

            if (config == null || !HasValidPrefabs())
            {
                Debug.LogError("[GridPuzzle] Cannot generate grid: missing config or prefabs");
                return;
            }

            // Initialize random with seed (or use random seed if -1)
            System.Random rng;
            if (randomSeed >= 0)
            {
                rng = new System.Random(randomSeed);
            }
            else
            {
                rng = new System.Random();
            }

            tileGrid = new GridTile[config.cols, config.rows];

            // Calculate grid center offset
            float offsetX = (config.cols - 1) * tileSpacing * 0.5f;
            float offsetZ = (config.rows - 1) * tileSpacing * 0.5f;

            // Count prefab usage for debug logging
            var prefabUsageCount = new System.Collections.Generic.Dictionary<string, int>();

            for (int x = 0; x < config.cols; x++)
            {
                for (int z = 0; z < config.rows; z++)
                {
                    Vector3 position = tilesParent.position + new Vector3(
                        x * tileSpacing - offsetX,
                        0,
                        z * tileSpacing - offsetZ
                    );

                    // Get a random prefab for this tile
                    GameObject selectedPrefab = GetRandomTilePrefab(rng);
                    
                    GameObject tileObj = Instantiate(selectedPrefab, position, Quaternion.identity, tilesParent);
                    tileObj.name = $"Tile_{x}_{z}";

                    // Track prefab usage
                    string prefabName = selectedPrefab.name;
                    if (!prefabUsageCount.ContainsKey(prefabName))
                        prefabUsageCount[prefabName] = 0;
                    prefabUsageCount[prefabName]++;

                    GridTile tile = tileObj.GetComponent<GridTile>();
                    if (tile == null)
                        tile = tileObj.AddComponent<GridTile>();

                    tile.Initialize(new Vector2Int(x, z), config.tileSinkDepth, config.tileMoveSpeed, triggerHeightOffset, triggerSize);
                    
                    // Subscribe to tile events
                    tile.OnPlayerStep += HandlePlayerStep;
                    tile.OnPlayerExit += HandlePlayerExit;

                    tileGrid[x, z] = tile;
                    allTiles.Add(tile);
                }
            }

            // Log prefab distribution
            string distribution = "";
            foreach (var kvp in prefabUsageCount)
            {
                distribution += $"{kvp.Key}: {kvp.Value}, ";
            }
            Debug.Log($"[GridPuzzle] Generated {config.cols}x{config.rows} grid for '{config.puzzleId}'. Prefab distribution: {distribution.TrimEnd(',', ' ')}");
        }

        /// <summary>
        /// Clear all generated tiles.
        /// </summary>
        [ContextMenu("Clear Grid")]
        public void ClearGrid()
        {
            foreach (var tile in allTiles)
            {
                if (tile != null)
                {
                    tile.OnPlayerStep -= HandlePlayerStep;
                    tile.OnPlayerExit -= HandlePlayerExit;
                    
                    if (Application.isPlaying)
                        Destroy(tile.gameObject);
                    else
                        DestroyImmediate(tile.gameObject);
                }
            }

            allTiles.Clear();
            tileGrid = null;
        }

        /// <summary>
        /// Activate the puzzle (start listening for player steps).
        /// </summary>
        public void ActivatePuzzle()
        {
            if (isSolved)
            {
                Debug.Log($"[GridPuzzle] '{config.puzzleId}' already solved");
                return;
            }

            isActive = true;
            currentPathIndex = 0;
            OnPuzzleStarted?.Invoke();
            Debug.Log($"[GridPuzzle] '{config.puzzleId}' activated");
        }

        /// <summary>
        /// Deactivate the puzzle (stop listening).
        /// </summary>
        public void DeactivatePuzzle()
        {
            isActive = false;
            Debug.Log($"[GridPuzzle] '{config.puzzleId}' deactivated");
        }

        /// <summary>
        /// Reset puzzle to initial state.
        /// </summary>
        public void ResetPuzzle()
        {
            currentPathIndex = 0;
            
            foreach (var tile in allTiles)
            {
                tile.ResetTile();
                tile.Unlock();
            }

            isSolved = false;
            Debug.Log($"[GridPuzzle] '{config.puzzleId}' reset");
        }

        private void HandlePlayerStep(GridTile tile)
        {
            if (!isActive || isSolved) return;

            Vector2Int coord = tile.Coordinates;
            Debug.Log($"[GridPuzzle] Player stepped on tile ({coord.x}, {coord.y})");

            bool isCorrect = false;

            switch (config.mode)
            {
                case GridPuzzleMode.ExactSequence:
                    isCorrect = ValidateExactSequenceStep(tile);
                    break;

                case GridPuzzleMode.SafeZone:
                    isCorrect = ValidateSafeZoneStep(tile);
                    break;
            }

            if (isCorrect)
            {
                HandleCorrectStep(tile);
            }
            else
            {
                HandleWrongStep(tile);
            }
        }

        private void HandlePlayerExit(GridTile tile)
        {
            // Optional: Track when player leaves tiles
            // Could be used for more complex puzzle logic
        }

        private bool ValidateExactSequenceStep(GridTile tile)
        {
            if (currentPathIndex >= config.correctPath.Count)
                return false;

            Vector2Int expectedCoord = config.correctPath[currentPathIndex];
            return tile.Coordinates == expectedCoord;
        }

        private bool ValidateSafeZoneStep(GridTile tile)
        {
            return config.IsSafeTile(tile.Coordinates);
        }

        private void HandleCorrectStep(GridTile tile)
        {
            tile.Press();
            
            // Play correct step sound (integrate with audio system)
            // AudioManager.Instance?.PlaySFX(config.correctStepSFX);

            if (config.mode == GridPuzzleMode.ExactSequence)
            {
                currentPathIndex++;
                OnProgressChanged?.Invoke(currentPathIndex, config.correctPath.Count);

                Debug.Log($"[GridPuzzle] Correct! Progress: {currentPathIndex}/{config.correctPath.Count}");

                // Check if puzzle is complete
                if (currentPathIndex >= config.correctPath.Count)
                {
                    SolvePuzzle();
                }
            }
            else if (config.mode == GridPuzzleMode.SafeZone)
            {
                // In SafeZone mode, check if player reached the end tile
                if (tile.Coordinates == config.endTile)
                {
                    SolvePuzzle();
                }
            }
        }

        private void HandleWrongStep(GridTile tile)
        {
            tile.ShowWrongFeedback();
            
            // Play wrong step sound
            // AudioManager.Instance?.PlaySFX(config.wrongStepSFX);

            Debug.Log($"[GridPuzzle] Wrong step! Resetting puzzle...");

            OnPuzzleFailed?.Invoke();

            // Execute fail commands
            ExecuteCommands(config.onFailedCommands);

            // Reset all tiles after a brief delay
            Invoke(nameof(ResetAllTiles), 0.5f);
        }

        private void ResetAllTiles()
        {
            currentPathIndex = 0;
            
            foreach (var t in allTiles)
            {
                t.ResetTile();
            }

            // Play reset sound
            // AudioManager.Instance?.PlaySFX(config.puzzleResetSFX);
        }

        private void SolvePuzzle()
        {
            isSolved = true;
            isActive = false;

            // Lock all tiles
            foreach (var tile in allTiles)
            {
                tile.Lock();
            }

            // Play solved sound
            // AudioManager.Instance?.PlaySFX(config.puzzleSolvedSFX);

            Debug.Log($"[GridPuzzle] '{config.puzzleId}' SOLVED!");

            OnPuzzleSolved?.Invoke();

            // Execute solved commands
            ExecuteCommands(config.onSolvedCommands);
        }

        private void ExecuteCommands(List<string> commands)
        {
            if (commands == null || commands.Count == 0) return;

            foreach (string command in commands)
            {
                if (string.IsNullOrWhiteSpace(command)) continue;

                // Parse and execute command
                // Split only on first colon to preserve parameters like "cam:point:3"
                int colonIndex = command.IndexOf(':');
                if (colonIndex > 0)
                {
                    string type = command.Substring(0, colonIndex).ToLower().Trim();
                    string param = command.Substring(colonIndex + 1).Trim();
                    ExecuteCommand(type, param);
                }
                else if (!string.IsNullOrWhiteSpace(command))
                {
                    // Command with no parameter
                    ExecuteCommand(command.ToLower().Trim(), "");
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
                    Debug.Log($"[GridPuzzle] Set flag: {param}");
                    break;

                case "var":
                    ParseAndSetVariable(param);
                    break;

                case "cam":
                    if (WhisperingGate.Camera.CameraFocusController.Instance != null)
                    {
                        // Support format: cam:point_id or cam:point_id:duration
                        string[] camParts = param.Split(':');
                        string camTarget = camParts[0].Trim();
                        float camDuration = -1f;
                        
                        if (camParts.Length > 1 && float.TryParse(camParts[1].Trim(), out float parsedDuration))
                            camDuration = parsedDuration;

                        Debug.Log($"[GridPuzzle] Camera command - target: {camTarget}, duration: {camDuration}");

                        if (camTarget.Equals("reset", StringComparison.OrdinalIgnoreCase))
                            WhisperingGate.Camera.CameraFocusController.Instance.ReleaseFocus();
                        else
                            WhisperingGate.Camera.CameraFocusController.Instance.FocusOn(camTarget, camDuration);
                    }
                    break;

                case "door":
                    // Format: door:action:door_id or door:door_id (defaults to open)
                    {
                        string[] doorParts = param.Split(':');
                        string doorAction = doorParts.Length > 1 ? doorParts[0].Trim() : "open";
                        string doorId = doorParts.Length > 1 ? doorParts[1].Trim() : doorParts[0].Trim();
                        Interaction.Door.ExecuteCommand(doorAction, doorId);
                        Debug.Log($"[GridPuzzle] Door command: {doorAction} {doorId}");
                    }
                    break;

                case "activate":
                    Interaction.ActivatableObject.ExecuteCommand("activate", param);
                    Debug.Log($"[GridPuzzle] Activate: {param}");
                    break;

                case "deactivate":
                    Interaction.ActivatableObject.ExecuteCommand("deactivate", param);
                    Debug.Log($"[GridPuzzle] Deactivate: {param}");
                    break;

                default:
                    Debug.Log($"[GridPuzzle] Unknown command type: {type}");
                    break;
            }
        }

        private void ParseAndSetVariable(string param)
        {
            if (GameState.Instance == null) return;

            // Parse formats like "insanity+5" or "insanity-10"
            int plusIndex = param.IndexOf('+');
            int minusIndex = param.IndexOf('-');

            if (plusIndex > 0)
            {
                string varName = param.Substring(0, plusIndex);
                if (int.TryParse(param.Substring(plusIndex + 1), out int value))
                {
                    int current = GameState.Instance.GetInt(varName);
                    GameState.Instance.SetInt(varName, current + value);
                }
            }
            else if (minusIndex > 0)
            {
                string varName = param.Substring(0, minusIndex);
                if (int.TryParse(param.Substring(minusIndex + 1), out int value))
                {
                    int current = GameState.Instance.GetInt(varName);
                    GameState.Instance.SetInt(varName, current - value);
                }
            }
        }

        /// <summary>
        /// Get tile at specific coordinates.
        /// </summary>
        public GridTile GetTile(int x, int z)
        {
            if (tileGrid == null) return null;
            if (x < 0 || x >= config.cols || z < 0 || z >= config.rows) return null;
            return tileGrid[x, z];
        }

        /// <summary>
        /// Get tile at specific coordinates.
        /// </summary>
        public GridTile GetTile(Vector2Int coord)
        {
            return GetTile(coord.x, coord.y);
        }

        private void OnDrawGizmos()
        {
            if (config == null) return;

            // Draw grid outline
            float width = config.cols * tileSpacing;
            float depth = config.rows * tileSpacing;
            Vector3 center = transform.position;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(center, new Vector3(width, 0.1f, depth));

            // Draw start tile
            if (config.IsValidTile(config.startTile))
            {
                Gizmos.color = Color.green;
                Vector3 startPos = GetWorldPosition(config.startTile);
                Gizmos.DrawWireSphere(startPos + Vector3.up * 0.5f, 0.3f);
            }

            // Draw end tile
            if (config.IsValidTile(config.endTile))
            {
                Gizmos.color = Color.red;
                Vector3 endPos = GetWorldPosition(config.endTile);
                Gizmos.DrawWireSphere(endPos + Vector3.up * 0.5f, 0.3f);
            }

            // Draw correct path
            if (config.correctPath != null && config.correctPath.Count > 0)
            {
                Gizmos.color = Color.yellow;
                for (int i = 0; i < config.correctPath.Count - 1; i++)
                {
                    Vector3 from = GetWorldPosition(config.correctPath[i]) + Vector3.up * 0.2f;
                    Vector3 to = GetWorldPosition(config.correctPath[i + 1]) + Vector3.up * 0.2f;
                    Gizmos.DrawLine(from, to);
                }

                // Draw path points
                foreach (var coord in config.correctPath)
                {
                    Vector3 pos = GetWorldPosition(coord);
                    Gizmos.DrawWireCube(pos + Vector3.up * 0.2f, Vector3.one * 0.2f);
                }
            }
        }

        private Vector3 GetWorldPosition(Vector2Int coord)
        {
            float offsetX = (config.cols - 1) * tileSpacing * 0.5f;
            float offsetZ = (config.rows - 1) * tileSpacing * 0.5f;
            
            return transform.position + new Vector3(
                coord.x * tileSpacing - offsetX,
                0,
                coord.y * tileSpacing - offsetZ
            );
        }
    }
}

