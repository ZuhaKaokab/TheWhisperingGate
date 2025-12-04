using UnityEngine;
using System;

namespace WhisperingGate.Puzzles
{
    /// <summary>
    /// Represents a single tile in the Grid Path Puzzle.
    /// Handles player detection and visual state changes.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class GridTile : MonoBehaviour
    {
        [Header("Tile Identity")]
        [SerializeField] private Vector2Int coordinates;
        
        [Header("State")]
        [SerializeField] private bool isPressed = false;
        [SerializeField] private bool isLocked = false; // Locked when puzzle is solved

        [Header("Visual References")]
        [SerializeField] private Transform tileVisual; // The part that moves down
        [SerializeField] private Renderer tileRenderer;

        [Header("Materials (Optional)")]
        [SerializeField] private Material defaultMaterial;
        [SerializeField] private Material pressedMaterial;
        [SerializeField] private Material wrongMaterial;

        // Movement
        private Vector3 defaultPosition;
        private Vector3 pressedPosition;
        private float sinkDepth = 0.15f;
        private float moveSpeed = 5f;
        private Vector3 targetPosition;

        // Wrong step flash
        private float wrongFlashTimer = 0f;
        private const float WRONG_FLASH_DURATION = 0.3f;

        /// <summary>
        /// The grid coordinates of this tile.
        /// </summary>
        public Vector2Int Coordinates => coordinates;

        /// <summary>
        /// Whether this tile is currently pressed down.
        /// </summary>
        public bool IsPressed => isPressed;

        /// <summary>
        /// Event fired when player steps on this tile.
        /// </summary>
        public event Action<GridTile> OnPlayerStep;

        /// <summary>
        /// Event fired when player leaves this tile.
        /// </summary>
        public event Action<GridTile> OnPlayerExit;

        /// <summary>
        /// Initialize tile with coordinates and settings.
        /// Called by GridPuzzleController during setup.
        /// </summary>
        public void Initialize(Vector2Int coords, float sinkAmount, float speed)
        {
            coordinates = coords;
            sinkDepth = sinkAmount;
            moveSpeed = speed;

            if (tileVisual == null)
                tileVisual = transform;

            defaultPosition = tileVisual.localPosition;
            pressedPosition = defaultPosition + Vector3.down * sinkDepth;
            targetPosition = defaultPosition;

            if (tileRenderer == null)
                tileRenderer = GetComponentInChildren<Renderer>();

            if (defaultMaterial == null && tileRenderer != null)
                defaultMaterial = tileRenderer.material;
        }

        /// <summary>
        /// Set coordinates manually (for editor setup).
        /// </summary>
        public void SetCoordinates(Vector2Int coords)
        {
            coordinates = coords;
        }

        private void Update()
        {
            // Smooth tile movement
            if (tileVisual != null && Vector3.Distance(tileVisual.localPosition, targetPosition) > 0.001f)
            {
                tileVisual.localPosition = Vector3.Lerp(
                    tileVisual.localPosition, 
                    targetPosition, 
                    moveSpeed * Time.deltaTime
                );
            }

            // Wrong step flash timer
            if (wrongFlashTimer > 0f)
            {
                wrongFlashTimer -= Time.deltaTime;
                if (wrongFlashTimer <= 0f)
                {
                    SetMaterial(isPressed ? pressedMaterial : defaultMaterial);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isLocked) return;

            if (other.CompareTag("Player"))
            {
                OnPlayerStep?.Invoke(this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (isLocked) return;

            if (other.CompareTag("Player"))
            {
                OnPlayerExit?.Invoke(this);
            }
        }

        /// <summary>
        /// Press the tile down (correct step).
        /// </summary>
        public void Press()
        {
            if (isLocked) return;

            isPressed = true;
            targetPosition = pressedPosition;
            SetMaterial(pressedMaterial);
        }

        /// <summary>
        /// Reset tile to default state.
        /// </summary>
        public void ResetTile()
        {
            if (isLocked) return;

            isPressed = false;
            targetPosition = defaultPosition;
            SetMaterial(defaultMaterial);
        }

        /// <summary>
        /// Show wrong step feedback (brief flash).
        /// </summary>
        public void ShowWrongFeedback()
        {
            wrongFlashTimer = WRONG_FLASH_DURATION;
            SetMaterial(wrongMaterial);
        }

        /// <summary>
        /// Lock tile in current state (puzzle solved).
        /// </summary>
        public void Lock()
        {
            isLocked = true;
        }

        /// <summary>
        /// Unlock tile (for puzzle reset).
        /// </summary>
        public void Unlock()
        {
            isLocked = false;
        }

        private void SetMaterial(Material mat)
        {
            if (tileRenderer != null && mat != null)
            {
                tileRenderer.material = mat;
            }
        }

        private void OnDrawGizmos()
        {
            // Show tile coordinates in editor
            Gizmos.color = isPressed ? Color.green : Color.white;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.1f, new Vector3(0.9f, 0.1f, 0.9f));
        }

        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            // Show coordinates label
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 0.5f,
                $"({coordinates.x}, {coordinates.y})",
                new GUIStyle { 
                    normal = { textColor = Color.yellow },
                    fontStyle = FontStyle.Bold,
                    fontSize = 14,
                    alignment = TextAnchor.MiddleCenter
                }
            );
#endif
        }
    }
}



