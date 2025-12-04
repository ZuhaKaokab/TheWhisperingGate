using UnityEngine;
using WhisperingGate.Core;

namespace WhisperingGate.Puzzles
{
    /// <summary>
    /// Trigger zone that activates a Grid Puzzle when player enters.
    /// Place this at the entrance of the puzzle area.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class GridPuzzleTrigger : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GridPuzzleController puzzleController;

        [Header("Activation Settings")]
        [Tooltip("Activate puzzle when player enters trigger")]
        [SerializeField] private bool activateOnEnter = true;
        
        [Tooltip("Deactivate puzzle when player exits trigger")]
        [SerializeField] private bool deactivateOnExit = false;

        [Header("Conditions (Optional)")]
        [Tooltip("GameState flag that must be true to activate")]
        [SerializeField] private string requiredFlag = "";
        
        [Tooltip("Only activate once")]
        [SerializeField] private bool oneTimeActivation = false;

        private bool hasActivated = false;

        private void Awake()
        {
            // Ensure trigger is set
            var col = GetComponent<Collider>();
            if (col != null)
                col.isTrigger = true;
        }

        private void Start()
        {
            if (puzzleController == null)
            {
                puzzleController = GetComponentInParent<GridPuzzleController>();
                if (puzzleController == null)
                {
                    Debug.LogWarning($"[GridPuzzleTrigger] No GridPuzzleController found for {gameObject.name}");
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!activateOnEnter) return;
            if (!other.CompareTag("Player")) return;
            if (puzzleController == null) return;
            if (puzzleController.IsSolved) return;
            if (oneTimeActivation && hasActivated) return;

            // Check required flag
            if (!string.IsNullOrWhiteSpace(requiredFlag))
            {
                if (GameState.Instance == null || !GameState.Instance.GetBool(requiredFlag))
                {
                    Debug.Log($"[GridPuzzleTrigger] Required flag '{requiredFlag}' not set");
                    return;
                }
            }

            hasActivated = true;
            puzzleController.ActivatePuzzle();
            Debug.Log($"[GridPuzzleTrigger] Puzzle activated");
        }

        private void OnTriggerExit(Collider other)
        {
            if (!deactivateOnExit) return;
            if (!other.CompareTag("Player")) return;
            if (puzzleController == null) return;

            puzzleController.DeactivatePuzzle();
            Debug.Log($"[GridPuzzleTrigger] Puzzle deactivated");
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0f, 1f, 0.5f, 0.3f);
            
            var col = GetComponent<BoxCollider>();
            if (col != null)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(col.center, col.size);
                Gizmos.DrawWireCube(col.center, col.size);
            }
        }
    }
}

