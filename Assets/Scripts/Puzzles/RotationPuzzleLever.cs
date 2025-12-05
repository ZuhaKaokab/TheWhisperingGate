using UnityEngine;
using WhisperingGate.Core;

namespace WhisperingGate.Puzzles
{
    /// <summary>
    /// Interaction point (lever/button) that activates a rotation puzzle's solve mode.
    /// Player interacts with this to start solving the puzzle.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class RotationPuzzleLever : MonoBehaviour
    {
        [Header("Puzzle Reference")]
        [SerializeField] private RotationPuzzleController puzzleController;

        [Header("Interaction Settings")]
        [Tooltip("Key to interact with lever")]
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        [Tooltip("If true, player must look at lever. If false, just being in trigger range is enough.")]
        [SerializeField] private bool requireLookAt = false;

        [Tooltip("Maximum distance for look-at check (only used if requireLookAt is true)")]
        [SerializeField] private float interactDistance = 2.5f;

        [Header("Prerequisites")]
        [Tooltip("GameState flag that must be true to use this lever")]
        [SerializeField] private string requiredFlag = "";

        [Tooltip("GameState flag that prevents using this lever if true")]
        [SerializeField] private string blockingFlag = "";

        [Header("UI Prompt")]
        [Tooltip("Text to show when player can interact")]
        [SerializeField] private string interactPrompt = "Press E to interact";

        [Tooltip("Text to show when puzzle is already solved")]
        [SerializeField] private string solvedPrompt = "Puzzle already solved";

        [Tooltip("UI element to show/hide prompt (optional)")]
        [SerializeField] private GameObject promptUI;

        [Tooltip("Text component for prompt (optional)")]
        [SerializeField] private TMPro.TextMeshProUGUI promptText;

        [Header("Visual Feedback")]
        [Tooltip("Animator for lever animation (optional)")]
        [SerializeField] private Animator leverAnimator;

        [Tooltip("Animation trigger name for activation")]
        [SerializeField] private string activateTrigger = "Activate";

        // Runtime state
        private bool playerInRange = false;
        private Transform playerTransform;
        private bool isLooking = false;

        private void Start()
        {
            if (puzzleController == null)
            {
                puzzleController = GetComponentInParent<RotationPuzzleController>();
            }

            if (puzzleController == null)
            {
                Debug.LogError($"[RotationPuzzleLever] No puzzle controller assigned or found on {gameObject.name}");
            }

            // Ensure collider is trigger
            var collider = GetComponent<Collider>();
            if (collider != null)
                collider.isTrigger = true;

            // Hide prompt initially
            SetPromptVisible(false);
        }

        private void Update()
        {
            if (!playerInRange || puzzleController == null) return;

            if (requireLookAt)
            {
                // Check if player is looking at lever
                CheckPlayerLook();

                // Handle interaction only if looking
                if (isLooking && Input.GetKeyDown(interactKey))
                {
                    TryActivate();
                }
            }
            else
            {
                // No look requirement - just show prompt and allow interaction when in range
                UpdatePrompt();
                SetPromptVisible(true);
                
                if (Input.GetKeyDown(interactKey))
                {
                    TryActivate();
                }
            }
        }

        private void CheckPlayerLook()
        {
            if (playerTransform == null)
            {
                isLooking = false;
                SetPromptVisible(false);
                return;
            }

            // Raycast from player camera
            UnityEngine.Camera playerCam = UnityEngine.Camera.main;
            if (playerCam == null)
            {
                isLooking = false;
                return;
            }

            Ray ray = new Ray(playerCam.transform.position, playerCam.transform.forward);
            
            // Use QueryTriggerInteraction.Collide to hit trigger colliders
            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, ~0, QueryTriggerInteraction.Collide))
            {
                // Check if hit this lever
                if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
                {
                    isLooking = true;
                    UpdatePrompt();
                    return;
                }
            }

            isLooking = false;
            SetPromptVisible(false);
        }

        private void UpdatePrompt()
        {
            if (!CanActivate(out string reason))
            {
                // Show why can't activate
                SetPromptText(reason);
                SetPromptVisible(true);
            }
            else
            {
                SetPromptText(interactPrompt);
                SetPromptVisible(true);
            }
        }

        private bool CanActivate(out string reason)
        {
            reason = "";

            // Check if already in solve mode
            if (puzzleController.IsInSolveMode)
            {
                reason = "Already solving puzzle";
                return false;
            }

            // Check if already solved
            if (puzzleController.IsSolved)
            {
                reason = solvedPrompt;
                return false;
            }

            // Check required flag
            if (!string.IsNullOrWhiteSpace(requiredFlag))
            {
                if (GameState.Instance == null || !GameState.Instance.GetBool(requiredFlag))
                {
                    reason = "Cannot activate yet";
                    return false;
                }
            }

            // Check blocking flag
            if (!string.IsNullOrWhiteSpace(blockingFlag))
            {
                if (GameState.Instance != null && GameState.Instance.GetBool(blockingFlag))
                {
                    reason = "Cannot activate";
                    return false;
                }
            }

            return true;
        }

        private void TryActivate()
        {
            if (!CanActivate(out string reason))
            {
                Debug.Log($"[RotationPuzzleLever] Cannot activate: {reason}");
                return;
            }

            Activate();
        }

        /// <summary>
        /// Activate the puzzle's solve mode.
        /// </summary>
        public void Activate()
        {
            if (puzzleController == null) return;

            // Play lever animation
            if (leverAnimator != null && !string.IsNullOrWhiteSpace(activateTrigger))
            {
                leverAnimator.SetTrigger(activateTrigger);
            }

            // Hide prompt
            SetPromptVisible(false);

            // Enter solve mode
            puzzleController.EnterSolveMode();

            Debug.Log($"[RotationPuzzleLever] Activated puzzle '{puzzleController.Config?.puzzleId}'");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = true;
                playerTransform = other.transform;
                Debug.Log("[RotationPuzzleLever] Player in range");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
                playerTransform = null;
                isLooking = false;
                SetPromptVisible(false);
                Debug.Log("[RotationPuzzleLever] Player left range");
            }
        }

        private void SetPromptVisible(bool visible)
        {
            if (promptUI != null)
                promptUI.SetActive(visible);
        }

        private void SetPromptText(string text)
        {
            if (promptText != null)
                promptText.text = text;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Draw interaction range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactDistance);

            // Draw line to puzzle controller
            if (puzzleController != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, puzzleController.transform.position);
            }
        }
#endif
    }
}

