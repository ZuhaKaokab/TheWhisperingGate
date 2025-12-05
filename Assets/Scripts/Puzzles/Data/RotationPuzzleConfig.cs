using UnityEngine;
using System.Collections.Generic;

namespace WhisperingGate.Puzzles
{
    /// <summary>
    /// Configuration for a grid-based rotation puzzle.
    /// Supports any grid size (rows x columns) of rotatable elements.
    /// </summary>
    [CreateAssetMenu(fileName = "RotationPuzzle_New", menuName = "Whispering Gate/Puzzles/Rotation Puzzle Config")]
    public class RotationPuzzleConfig : ScriptableObject
    {
        [Header("Puzzle Identity")]
        [Tooltip("Unique identifier for this puzzle")]
        public string puzzleId = "rotation_puzzle";

        [Header("Grid Settings")]
        [Tooltip("Number of rows in the puzzle grid")]
        [Range(1, 10)]
        public int rows = 1;

        [Tooltip("Number of columns in the puzzle grid")]
        [Range(1, 10)]
        public int columns = 3;

        [Tooltip("Spacing between elements")]
        public float elementSpacing = 1.5f;

        [Header("Rotation Settings")]
        [Tooltip("Number of rotation positions (4 = 90° each, 6 = 60° each, 8 = 45° each)")]
        [Range(2, 12)]
        public int rotationSteps = 4;

        [Tooltip("Axis to rotate around")]
        public RotationAxis rotationAxis = RotationAxis.Y;

        [Tooltip("How fast elements rotate (degrees per second)")]
        public float rotationSpeed = 360f;

        [Header("Solution")]
        [Tooltip("Target rotation index for each element (row-major order). -1 means any rotation is valid.")]
        public List<int> solutionIndices = new List<int>();

        [Header("Starting State")]
        [Tooltip("Starting rotation index for each element. Leave empty to randomize.")]
        public List<int> startingIndices = new List<int>();

        [Tooltip("Randomize starting positions on puzzle activation")]
        public bool randomizeStart = true;

        [Header("Visual Settings")]
        [Tooltip("Material/color for selected element highlight")]
        public Color selectedHighlightColor = new Color(1f, 0.9f, 0.4f, 1f);

        [Tooltip("Material/color for correct element")]
        public Color correctHighlightColor = new Color(0.4f, 1f, 0.5f, 1f);

        [Header("Camera Focus")]
        [Tooltip("Camera focus point ID when entering solve mode")]
        public string cameraFocusPointId = "";

        [Tooltip("Duration to hold camera on puzzle after solving (0 = no auto-return)")]
        public float solvedCameraHoldDuration = 3f;

        [Header("Commands")]
        [Tooltip("Commands to execute when puzzle is solved")]
        public List<string> onSolvedCommands = new List<string>();

        /// <summary>
        /// Total number of elements in the grid.
        /// </summary>
        public int TotalElements => rows * columns;

        /// <summary>
        /// Angle per rotation step.
        /// </summary>
        public float AnglePerStep => 360f / rotationSteps;

        /// <summary>
        /// Get the solution index for a specific grid position.
        /// </summary>
        public int GetSolutionIndex(int row, int col)
        {
            int index = row * columns + col;
            if (index >= 0 && index < solutionIndices.Count)
                return solutionIndices[index];
            return 0;
        }

        /// <summary>
        /// Get the starting index for a specific grid position.
        /// Returns -1 if should be randomized.
        /// </summary>
        public int GetStartingIndex(int row, int col)
        {
            if (randomizeStart) return -1;
            
            int index = row * columns + col;
            if (index >= 0 && index < startingIndices.Count)
                return startingIndices[index];
            return -1;
        }

        /// <summary>
        /// Ensures solution and starting arrays match grid size.
        /// </summary>
        public void ValidateArraySizes()
        {
            int total = TotalElements;
            
            // Resize solution array
            while (solutionIndices.Count < total)
                solutionIndices.Add(0);
            while (solutionIndices.Count > total)
                solutionIndices.RemoveAt(solutionIndices.Count - 1);

            // Resize starting array
            while (startingIndices.Count < total)
                startingIndices.Add(0);
            while (startingIndices.Count > total)
                startingIndices.RemoveAt(startingIndices.Count - 1);

            // Clamp values
            for (int i = 0; i < solutionIndices.Count; i++)
                solutionIndices[i] = Mathf.Clamp(solutionIndices[i], 0, rotationSteps - 1);
            for (int i = 0; i < startingIndices.Count; i++)
                startingIndices[i] = Mathf.Clamp(startingIndices[i], 0, rotationSteps - 1);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ValidateArraySizes();
        }
#endif
    }

    public enum RotationAxis
    {
        X,  // Tilt forward/backward
        Y,  // Spin left/right (most common)
        Z   // Roll
    }
}


