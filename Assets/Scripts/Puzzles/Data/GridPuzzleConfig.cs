using UnityEngine;
using System.Collections.Generic;

namespace WhisperingGate.Puzzles
{
    /// <summary>
    /// Defines how the grid puzzle validates player movement.
    /// </summary>
    public enum GridPuzzleMode
    {
        /// <summary>Player must step tiles in exact order.</summary>
        ExactSequence,
        /// <summary>Player can step on any safe tile, just avoid forbidden ones.</summary>
        SafeZone
    }

    /// <summary>
    /// ScriptableObject configuration for a Grid Path Puzzle.
    /// Defines grid size, correct path, and integration commands.
    /// </summary>
    [CreateAssetMenu(fileName = "GridPuzzle_New", menuName = "Whispering Gate/Puzzles/Grid Puzzle Config")]
    public class GridPuzzleConfig : ScriptableObject
    {
        [Header("Identification")]
        [Tooltip("Unique ID for this puzzle instance")]
        public string puzzleId = "grid_puzzle_01";

        [Header("Grid Settings")]
        [Tooltip("Number of rows (Z axis)")]
        [Range(3, 10)] public int rows = 5;
        
        [Tooltip("Number of columns (X axis)")]
        [Range(3, 10)] public int cols = 5;
        
        [Tooltip("How the puzzle validates player steps")]
        public GridPuzzleMode mode = GridPuzzleMode.ExactSequence;

        [Header("Path Definition")]
        [Tooltip("For ExactSequence: The ordered list of tile coordinates player must follow")]
        public List<Vector2Int> correctPath = new List<Vector2Int>();

        [Tooltip("For SafeZone: All tiles that are safe to step on")]
        public List<Vector2Int> safeTiles = new List<Vector2Int>();

        [Tooltip("Starting tile (player enters puzzle here)")]
        public Vector2Int startTile = new Vector2Int(2, 0);

        [Tooltip("End tile (reaching this with correct path = solved)")]
        public Vector2Int endTile = new Vector2Int(2, 4);

        [Header("Visuals")]
        [Tooltip("How far tiles sink when stepped on correctly")]
        [Range(0.05f, 0.5f)] public float tileSinkDepth = 0.15f;
        
        [Tooltip("How fast tiles sink/rise")]
        [Range(1f, 10f)] public float tileMoveSpeed = 5f;

        [Header("Commands (executed via existing command system)")]
        [Tooltip("Commands to execute when puzzle is solved")]
        public List<string> onSolvedCommands = new List<string>() { "flag:grid_puzzle_solved" };

        [Tooltip("Commands to execute when player makes a wrong step")]
        public List<string> onFailedCommands = new List<string>();

        [Header("Audio IDs (for audio system integration)")]
        public string correctStepSFX = "puzzle_step_correct";
        public string wrongStepSFX = "puzzle_step_wrong";
        public string puzzleSolvedSFX = "puzzle_solved";
        public string puzzleResetSFX = "puzzle_reset";

        /// <summary>
        /// Check if a tile coordinate is within grid bounds.
        /// </summary>
        public bool IsValidTile(Vector2Int coord)
        {
            return coord.x >= 0 && coord.x < cols && coord.y >= 0 && coord.y < rows;
        }

        /// <summary>
        /// Check if a tile is part of the correct path (for ExactSequence mode).
        /// </summary>
        public bool IsOnCorrectPath(Vector2Int coord)
        {
            return correctPath.Contains(coord);
        }

        /// <summary>
        /// Check if a tile is safe (for SafeZone mode).
        /// </summary>
        public bool IsSafeTile(Vector2Int coord)
        {
            return safeTiles.Contains(coord);
        }

        /// <summary>
        /// Get the index of a tile in the correct path (-1 if not found).
        /// </summary>
        public int GetPathIndex(Vector2Int coord)
        {
            return correctPath.IndexOf(coord);
        }

        private void OnValidate()
        {
            // Ensure start and end tiles are in bounds
            startTile.x = Mathf.Clamp(startTile.x, 0, cols - 1);
            startTile.y = Mathf.Clamp(startTile.y, 0, rows - 1);
            endTile.x = Mathf.Clamp(endTile.x, 0, cols - 1);
            endTile.y = Mathf.Clamp(endTile.y, 0, rows - 1);

            // Clamp all path coordinates
            for (int i = 0; i < correctPath.Count; i++)
            {
                var coord = correctPath[i];
                coord.x = Mathf.Clamp(coord.x, 0, cols - 1);
                coord.y = Mathf.Clamp(coord.y, 0, rows - 1);
                correctPath[i] = coord;
            }

            for (int i = 0; i < safeTiles.Count; i++)
            {
                var coord = safeTiles[i];
                coord.x = Mathf.Clamp(coord.x, 0, cols - 1);
                coord.y = Mathf.Clamp(coord.y, 0, rows - 1);
                safeTiles[i] = coord;
            }
        }
    }
}

