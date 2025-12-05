using UnityEngine;
using System;

namespace WhisperingGate.Puzzles
{
    /// <summary>
    /// A single rotatable element in a rotation puzzle.
    /// Handles rotation animation and visual feedback.
    /// </summary>
    public class RotatableElement : MonoBehaviour
    {
        [Header("Grid Position")]
        [SerializeField] private int row;
        [SerializeField] private int column;

        [Header("State")]
        [SerializeField] private int currentRotationIndex = 0;
        [SerializeField] private int targetRotationIndex = 0;

        [Header("Visual Feedback")]
        [SerializeField] private Renderer elementRenderer;
        [SerializeField] private int highlightMaterialIndex = 0;

        // Runtime state
        private RotationPuzzleConfig config;
        private bool isRotating = false;
        private bool isSelected = false;
        private bool isCorrect = false;
        private Color originalColor;
        private Material highlightMaterial;
        private Quaternion targetRotation;

        // Events
        public event Action<RotatableElement> OnRotationComplete;

        /// <summary>
        /// Grid row position.
        /// </summary>
        public int Row => row;

        /// <summary>
        /// Grid column position.
        /// </summary>
        public int Column => column;

        /// <summary>
        /// Current rotation index (0 to rotationSteps-1).
        /// </summary>
        public int CurrentRotationIndex => currentRotationIndex;

        /// <summary>
        /// Is this element currently animating?
        /// </summary>
        public bool IsRotating => isRotating;

        /// <summary>
        /// Is this element selected for rotation?
        /// </summary>
        public bool IsSelected => isSelected;

        /// <summary>
        /// Is this element in the correct position?
        /// </summary>
        public bool IsCorrect => isCorrect;

        private void Awake()
        {
            if (elementRenderer == null)
                elementRenderer = GetComponent<Renderer>();

            if (elementRenderer != null && elementRenderer.materials.Length > highlightMaterialIndex)
            {
                highlightMaterial = elementRenderer.materials[highlightMaterialIndex];
                originalColor = highlightMaterial.color;
            }
        }

        private void Update()
        {
            if (isRotating && config != null)
            {
                // Smoothly rotate towards target
                transform.localRotation = Quaternion.RotateTowards(
                    transform.localRotation,
                    targetRotation,
                    config.rotationSpeed * Time.deltaTime
                );

                // Check if rotation complete
                if (Quaternion.Angle(transform.localRotation, targetRotation) < 0.5f)
                {
                    transform.localRotation = targetRotation;
                    currentRotationIndex = targetRotationIndex;
                    isRotating = false;
                    OnRotationComplete?.Invoke(this);
                }
            }
        }

        /// <summary>
        /// Initialize this element with puzzle config and grid position.
        /// </summary>
        public void Initialize(RotationPuzzleConfig puzzleConfig, int gridRow, int gridCol)
        {
            config = puzzleConfig;
            row = gridRow;
            column = gridCol;

            // Set starting rotation
            int startIndex = config.GetStartingIndex(row, column);
            if (startIndex < 0)
            {
                // Randomize
                currentRotationIndex = UnityEngine.Random.Range(0, config.rotationSteps);
            }
            else
            {
                currentRotationIndex = startIndex;
            }

            targetRotationIndex = currentRotationIndex;
            ApplyRotationImmediate(currentRotationIndex);
            UpdateCorrectState();
        }

        /// <summary>
        /// Rotate to the next position (clockwise).
        /// </summary>
        public void RotateNext()
        {
            if (isRotating || config == null) return;

            targetRotationIndex = (currentRotationIndex + 1) % config.rotationSteps;
            StartRotation();
        }

        /// <summary>
        /// Rotate to the previous position (counter-clockwise).
        /// </summary>
        public void RotatePrevious()
        {
            if (isRotating || config == null) return;

            targetRotationIndex = (currentRotationIndex - 1 + config.rotationSteps) % config.rotationSteps;
            StartRotation();
        }

        /// <summary>
        /// Set rotation to a specific index immediately (no animation).
        /// </summary>
        public void SetRotationIndex(int index)
        {
            if (config == null) return;

            currentRotationIndex = index % config.rotationSteps;
            targetRotationIndex = currentRotationIndex;
            ApplyRotationImmediate(currentRotationIndex);
            UpdateCorrectState();
        }

        private void StartRotation()
        {
            targetRotation = GetRotationForIndex(targetRotationIndex);
            isRotating = true;
        }

        private void ApplyRotationImmediate(int index)
        {
            transform.localRotation = GetRotationForIndex(index);
        }

        private Quaternion GetRotationForIndex(int index)
        {
            if (config == null) return Quaternion.identity;

            float angle = index * config.AnglePerStep;
            
            return config.rotationAxis switch
            {
                RotationAxis.X => Quaternion.Euler(angle, 0, 0),
                RotationAxis.Y => Quaternion.Euler(0, angle, 0),
                RotationAxis.Z => Quaternion.Euler(0, 0, angle),
                _ => Quaternion.identity
            };
        }

        /// <summary>
        /// Update whether this element is in the correct position.
        /// </summary>
        public void UpdateCorrectState()
        {
            if (config == null) return;

            int solutionIndex = config.GetSolutionIndex(row, column);
            
            // -1 means any rotation is valid
            if (solutionIndex < 0)
            {
                isCorrect = true;
            }
            else
            {
                isCorrect = (currentRotationIndex == solutionIndex);
            }

            UpdateVisuals();
        }

        /// <summary>
        /// Set whether this element is selected.
        /// </summary>
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (highlightMaterial == null || config == null) return;

            if (isSelected)
            {
                highlightMaterial.color = config.selectedHighlightColor;
            }
            else if (isCorrect)
            {
                highlightMaterial.color = config.correctHighlightColor;
            }
            else
            {
                highlightMaterial.color = originalColor;
            }
        }

        /// <summary>
        /// Reset visuals to original state.
        /// </summary>
        public void ResetVisuals()
        {
            if (highlightMaterial != null)
            {
                highlightMaterial.color = originalColor;
            }
            isSelected = false;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor helper to visualize rotation state.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            // Draw direction indicator
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward * 0.5f);
            
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, transform.up * 0.3f);
        }
#endif
    }
}


