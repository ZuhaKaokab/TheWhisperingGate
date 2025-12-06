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

        [Header("Selection Outline")]
        [Tooltip("Custom outline prefab (optional - will create default if empty)")]
        [SerializeField] private GameObject outlinePrefab;
        
        [Tooltip("Outline color when selected")]
        [SerializeField] private Color outlineColor = new Color(1f, 0.8f, 0.2f, 1f); // Golden yellow
        
        [Tooltip("Scale multiplier for the outline (1.1 = 10% larger than element)")]
        [SerializeField] private float outlineScale = 1.15f;
        
        [Tooltip("Outline pulse speed (0 = no pulse)")]
        [SerializeField] private float outlinePulseSpeed = 2f;
        
        [Tooltip("Outline pulse intensity (how much it scales during pulse)")]
        [SerializeField] private float outlinePulseIntensity = 0.05f;

        // Runtime state
        private RotationPuzzleConfig config;
        private bool isRotating = false;
        private bool isSelected = false;
        private bool isCorrect = false;
        private Color originalColor;
        private Material highlightMaterial;
        private Quaternion targetRotation;
        
        // Outline runtime
        private GameObject outlineInstance;
        private Renderer outlineRenderer;
        private Material outlineMaterial;
        private float outlinePulseTimer = 0f;
        private Vector3 baseOutlineScale;

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
            
            // Create outline
            CreateOutline();
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
            
            // Animate outline pulse when selected
            if (isSelected && outlineInstance != null && outlinePulseSpeed > 0)
            {
                outlinePulseTimer += Time.deltaTime * outlinePulseSpeed;
                float pulse = 1f + Mathf.Sin(outlinePulseTimer * Mathf.PI * 2f) * outlinePulseIntensity;
                
                // Use the cube transform if available (bounds-based outline), otherwise use instance
                Transform scaleTarget = outlineCubeTransform != null ? outlineCubeTransform : outlineInstance.transform;
                scaleTarget.localScale = baseOutlineScale * pulse;
            }
        }
        
        /// <summary>
        /// Creates the selection outline object.
        /// </summary>
        private void CreateOutline()
        {
            if (outlinePrefab != null)
            {
                // Use custom prefab
                outlineInstance = Instantiate(outlinePrefab, transform);
                outlineInstance.transform.localPosition = Vector3.zero;
                outlineInstance.transform.localRotation = Quaternion.identity;
                
                outlineRenderer = outlineInstance.GetComponent<Renderer>();
                if (outlineRenderer == null)
                    outlineRenderer = outlineInstance.GetComponentInChildren<Renderer>();
                    
                if (outlineRenderer != null)
                {
                    outlineMaterial = new Material(Shader.Find("Sprites/Default"));
                    outlineMaterial.color = outlineColor;
                    outlineRenderer.material = outlineMaterial;
                    outlineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    outlineRenderer.receiveShadows = false;
                }
                
                baseOutlineScale = outlineInstance.transform.localScale * outlineScale;
            }
            else
            {
                // Create bounds-based outline that works with any model scale/hierarchy
                CreateBoundsBasedOutline();
            }
            
            // Start hidden
            if (outlineInstance != null)
            {
                outlineInstance.SetActive(false);
            }
        }
        
        /// <summary>
        /// Creates an outline based on the actual rendered bounds of the element.
        /// This works regardless of model scale, hierarchy, or Blender export issues.
        /// </summary>
        private void CreateBoundsBasedOutline()
        {
            // Get the actual rendered bounds
            Renderer targetRenderer = GetComponent<Renderer>();
            if (targetRenderer == null)
                targetRenderer = GetComponentInChildren<Renderer>();
            
            if (targetRenderer == null)
            {
                Debug.LogWarning($"[RotatableElement] No renderer found on {gameObject.name} for outline");
                return;
            }
            
            // Get world-space bounds
            Bounds worldBounds = targetRenderer.bounds;
            
            // Create outline container as sibling (not child) to avoid transform issues
            // Actually, let's make it a child but calculate size in world space
            outlineInstance = new GameObject("SelectionOutline");
            outlineInstance.transform.SetParent(transform);
            
            // Create the outline cube
            GameObject outlineCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            outlineCube.name = "OutlineCube";
            outlineCube.transform.SetParent(outlineInstance.transform);
            
            // Remove the collider
            Collider col = outlineCube.GetComponent<Collider>();
            if (col != null) 
                DestroyImmediate(col);
            
            // Calculate the size needed in local space
            // We need to account for the parent's scale
            Vector3 parentLossyScale = transform.lossyScale;
            Vector3 localSize = new Vector3(
                worldBounds.size.x / Mathf.Abs(parentLossyScale.x),
                worldBounds.size.y / Mathf.Abs(parentLossyScale.y),
                worldBounds.size.z / Mathf.Abs(parentLossyScale.z)
            );
            
            // Calculate local center offset
            Vector3 worldCenter = worldBounds.center;
            Vector3 localCenter = transform.InverseTransformPoint(worldCenter);
            
            // Position and scale the outline
            outlineInstance.transform.localPosition = localCenter;
            outlineInstance.transform.localRotation = Quaternion.identity;
            outlineCube.transform.localPosition = Vector3.zero;
            outlineCube.transform.localRotation = Quaternion.identity;
            outlineCube.transform.localScale = localSize;
            
            // Store base scale for pulse animation
            baseOutlineScale = localSize * outlineScale;
            outlineCube.transform.localScale = baseOutlineScale;
            
            // Setup material
            outlineRenderer = outlineCube.GetComponent<Renderer>();
            outlineMaterial = new Material(Shader.Find("Sprites/Default"));
            outlineMaterial.color = outlineColor;
            outlineRenderer.material = outlineMaterial;
            outlineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            outlineRenderer.receiveShadows = false;
            
            // Store reference to the actual cube for scaling
            outlineCubeTransform = outlineCube.transform;
        }
        
        // Reference to the actual cube inside the outline instance (for proper scaling)
        private Transform outlineCubeTransform;

        /// <summary>
        /// Initialize this element with puzzle config and grid position.
        /// </summary>
        public void Initialize(RotationPuzzleConfig puzzleConfig, int gridRow, int gridCol)
        {
            config = puzzleConfig;
            row = gridRow;
            column = gridCol;
            
            // Apply outline settings from config
            ApplyOutlineSettings();

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
        /// Apply outline settings from the puzzle config.
        /// </summary>
        private void ApplyOutlineSettings()
        {
            if (config == null) return;
            
            // Apply config settings
            outlineColor = config.selectionOutlineColor;
            outlinePulseSpeed = config.outlinePulseSpeed;
            outlinePulseIntensity = config.outlinePulseIntensity;
            
            // Only update outline scale if it changed
            if (Mathf.Abs(outlineScale - config.outlineScale) > 0.001f)
            {
                outlineScale = config.outlineScale;
                
                // Recalculate bounds-based outline if needed
                if (outlineInstance != null && outlineCubeTransform != null)
                {
                    // For bounds-based outline, we need to recalculate based on current bounds
                    RecalculateOutlineBounds();
                }
            }
            
            // Update material color
            if (outlineMaterial != null)
            {
                outlineMaterial.color = outlineColor;
            }
        }
        
        /// <summary>
        /// Recalculate the outline bounds (useful if model changes or scale updates).
        /// </summary>
        public void RecalculateOutlineBounds()
        {
            if (outlineCubeTransform == null) return;
            
            Renderer targetRenderer = GetComponent<Renderer>();
            if (targetRenderer == null)
                targetRenderer = GetComponentInChildren<Renderer>();
            
            if (targetRenderer == null) return;
            
            Bounds worldBounds = targetRenderer.bounds;
            Vector3 parentLossyScale = transform.lossyScale;
            
            Vector3 localSize = new Vector3(
                worldBounds.size.x / Mathf.Abs(parentLossyScale.x),
                worldBounds.size.y / Mathf.Abs(parentLossyScale.y),
                worldBounds.size.z / Mathf.Abs(parentLossyScale.z)
            );
            
            baseOutlineScale = localSize * outlineScale;
            outlineCubeTransform.localScale = baseOutlineScale;
            
            // Update position too
            Vector3 worldCenter = worldBounds.center;
            Vector3 localCenter = transform.InverseTransformPoint(worldCenter);
            outlineInstance.transform.localPosition = localCenter;
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
            UpdateOutline();
        }

        private void UpdateVisuals()
        {
            // Skip material color changes - we use outline instead
            // But keep this for backwards compatibility if needed
            if (highlightMaterial == null || config == null) return;

            // Only show correct color when NOT selected (don't give away solution)
            // Actually, per user request, we don't show correct state at all
            // Just use original color always for the element itself
            highlightMaterial.color = originalColor;
        }
        
        /// <summary>
        /// Update outline visibility and appearance.
        /// </summary>
        private void UpdateOutline()
        {
            if (outlineInstance == null) return;
            
            // Show outline only when selected
            outlineInstance.SetActive(isSelected);
            
            if (isSelected)
            {
                // Reset pulse timer for consistent animation start
                outlinePulseTimer = 0f;
                
                // Use the cube transform if available (bounds-based outline), otherwise use instance
                Transform scaleTarget = outlineCubeTransform != null ? outlineCubeTransform : outlineInstance.transform;
                scaleTarget.localScale = baseOutlineScale;
                
                // Update outline color
                if (outlineMaterial != null)
                {
                    outlineMaterial.color = outlineColor;
                }
            }
        }
        
        /// <summary>
        /// Set custom outline color at runtime.
        /// </summary>
        public void SetOutlineColor(Color color)
        {
            outlineColor = color;
            if (outlineMaterial != null && isSelected)
            {
                outlineMaterial.color = color;
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
            
            // Hide outline
            if (outlineInstance != null)
            {
                outlineInstance.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            // Cleanup dynamically created material
            if (outlineMaterial != null)
            {
                Destroy(outlineMaterial);
            }
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





