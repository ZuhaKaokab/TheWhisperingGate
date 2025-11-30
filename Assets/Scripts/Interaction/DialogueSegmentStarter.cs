using UnityEngine;
using WhisperingGate.Dialogue;
using WhisperingGate.Gameplay;

namespace WhisperingGate.Interaction
{
    /// <summary>
    /// Helper component that allows starting dialogue at a specific node within a tree.
    /// Useful for segmented dialogue flows where you want to keep one tree but start at different nodes.
    /// </summary>
    public class DialogueSegmentStarter : MonoBehaviour
    {
        [Header("Dialogue Settings")]
        [SerializeField] private DialogueTree dialogueTree;
        [SerializeField] private string startNodeId = "";
        [SerializeField] private bool pausePlayerDuringDialogue = true;

        [Header("Segment Tracking")]
        [Tooltip("Unique ID for this dialogue segment. Will be marked as completed after dialogue ends.")]
        [SerializeField] private string segmentId = "";

        [Header("Prerequisites")]
        [Tooltip("Required completed segments (comma-separated). Dialogue won't trigger until these are done.")]
        [SerializeField] private string requiredSegments = "";

        [Tooltip("GameState condition that must be true (e.g., 'courage >= 30' or 'journal_found').")]
        [SerializeField] private string requiredCondition = "";

        [Header("Interaction")]
        [SerializeField] private bool requireInteraction = true;
        [SerializeField] private KeyCode interactionKey = KeyCode.E;
        [SerializeField] private float interactionRange = 3f;

        [Header("Visual Feedback")]
        [SerializeField] private GameObject interactionPromptUI;
        [SerializeField] private bool showDebugInfo = false;

        private bool prerequisitesMet = false;
        private bool playerInRange = false;
        private Gameplay.PlayerController playerController;
        private Transform playerTransform;

        void Start()
        {
            playerController = FindObjectOfType<Gameplay.PlayerController>();
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;

            UpdatePrerequisites();
            UpdateVisualState();
        }

        void Update()
        {
            // Check prerequisites periodically
            bool previousPrerequisitesMet = prerequisitesMet;
            UpdatePrerequisites();

            if (prerequisitesMet != previousPrerequisitesMet)
            {
                UpdateVisualState();
            }

            // Check player distance
            if (playerTransform != null)
            {
                float distance = Vector3.Distance(transform.position, playerTransform.position);
                bool wasInRange = playerInRange;
                playerInRange = distance <= interactionRange;

                if (wasInRange != playerInRange)
                {
                    UpdateVisualState();
                }
            }

            // Handle interaction
            if (requireInteraction && playerInRange && prerequisitesMet && Input.GetKeyDown(interactionKey))
            {
                StartDialogueSegment();
            }
        }

        private void UpdatePrerequisites()
        {
            prerequisitesMet = true;

            // Check required segments
            if (!string.IsNullOrWhiteSpace(requiredSegments))
            {
                string[] segments = requiredSegments.Split(',');
                foreach (var segment in segments)
                {
                    string trimmedSegment = segment.Trim();
                    if (!string.IsNullOrEmpty(trimmedSegment))
                    {
                        if (LevelManager.Instance == null || !LevelManager.Instance.IsSegmentCompleted(trimmedSegment))
                        {
                            prerequisitesMet = false;
                            break;
                        }
                    }
                }
            }

            // Check game state condition
            if (prerequisitesMet && !string.IsNullOrWhiteSpace(requiredCondition))
            {
                if (Core.GameState.Instance == null)
                {
                    prerequisitesMet = false;
                }
                else
                {
                    prerequisitesMet = Core.GameState.Instance.EvaluateCondition(requiredCondition);
                }
            }
        }

        private void UpdateVisualState()
        {
            if (interactionPromptUI != null)
            {
                interactionPromptUI.SetActive(playerInRange && prerequisitesMet);
            }
        }

        /// <summary>
        /// Starts the dialogue segment at the specified node.
        /// </summary>
        public void StartDialogueSegment()
        {
            if (!prerequisitesMet)
            {
                if (showDebugInfo)
                    Debug.Log($"[DialogueSegmentStarter] Prerequisites not met for {gameObject.name}");
                return;
            }

            if (dialogueTree == null)
            {
                Debug.LogError($"[DialogueSegmentStarter] No dialogue tree assigned on {gameObject.name}");
                return;
            }

            if (DialogueManager.Instance == null)
            {
                Debug.LogError("[DialogueSegmentStarter] DialogueManager.Instance is null. Make sure DialogueManager exists in scene.");
                return;
            }

            // Pause player if needed
            if (pausePlayerDuringDialogue && playerController != null)
            {
                playerController.SetInputEnabled(false);
            }

            // Start dialogue at specific node
            if (!string.IsNullOrWhiteSpace(startNodeId))
            {
                StartDialogueAtNode(startNodeId);
            }
            else
            {
                // Fallback to normal start
                DialogueManager.Instance.StartDialogue(dialogueTree);
            }

            // Subscribe to dialogue end
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnDialogueEnded += OnDialogueEndedHandler;
            }
        }

        private void StartDialogueAtNode(string nodeId)
        {
            // Use the new DialogueManager method to start at a specific node
            DialogueManager.Instance.StartDialogueAtNodeId(dialogueTree, nodeId);

            if (showDebugInfo)
                Debug.Log($"[DialogueSegmentStarter] Starting dialogue at node: {nodeId}");
        }

        private void OnDialogueEndedHandler()
        {
            // Restore player control
            if (playerController != null)
                playerController.SetInputEnabled(true);

            // Mark segment as completed
            if (!string.IsNullOrWhiteSpace(segmentId) && LevelManager.Instance != null)
            {
                LevelManager.Instance.CompleteSegment(segmentId);
            }

            // Unsubscribe
            if (DialogueManager.Instance != null)
                DialogueManager.Instance.OnDialogueEnded -= OnDialogueEndedHandler;
        }

        void OnDestroy()
        {
            // Cleanup subscriptions
            if (DialogueManager.Instance != null)
                DialogueManager.Instance.OnDialogueEnded -= OnDialogueEndedHandler;
        }

        void OnDrawGizmosSelected()
        {
            // Draw interaction range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}

