using UnityEngine;
using WhisperingGate.Dialogue;
using WhisperingGate.Gameplay;

namespace WhisperingGate.Interaction
{
    /// <summary>
    /// Enhanced dialogue trigger that supports segment tracking and prerequisite checking.
    /// Allows dialogue to be gated behind completed segments or game state conditions.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class DialogueSegmentTrigger : MonoBehaviour
    {
        [Header("Dialogue Settings")]
        [SerializeField] private DialogueTree dialogueTree;
        [SerializeField] private InteractionMode interactionMode = InteractionMode.OnInteract;
        [SerializeField] private bool singleUse = false;
        [SerializeField] private bool pausePlayerDuringDialogue = true;

        [Header("Segment Tracking")]
        [Tooltip("Unique ID for this dialogue segment. Will be marked as completed after dialogue ends.")]
        [SerializeField] private string segmentId = "";
        
        [Tooltip("End dialogue when reaching a node with no next node (segment boundary). Prevents dialogue from continuing into next segment.")]
        [SerializeField] private bool endOnSegmentBoundary = true;

        [Header("Prerequisites")]
        [Tooltip("Required completed segments (comma-separated). Dialogue won't trigger until these are done.")]
        [SerializeField] private string requiredSegments = "";

        [Tooltip("GameState condition that must be true (e.g., 'courage >= 30' or 'journal_found').")]
        [SerializeField] private string requiredCondition = "";

        [Header("Visual Feedback")]
        [Tooltip("Show interaction prompt when player is in range.")]
        [SerializeField] private bool showInteractionPrompt = true;
        [SerializeField] private GameObject interactionPromptUI;

        [Header("Debug")]
        [Tooltip("Enable debug logging")]
        [SerializeField] private bool enableDebugLogs = false;

        private bool hasTriggered = false;
        private bool playerInRange = false;
        private Gameplay.PlayerController playerController;
        private bool prerequisitesMet = false;

        public enum InteractionMode { OnEnter, OnInteract }

        void Start()
        {
            playerController = FindObjectOfType<Gameplay.PlayerController>();

            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
                if (enableDebugLogs) Debug.Log($"[DialogueSegmentTrigger] {gameObject.name}: Collider set as trigger.");
            }
            else
            {
                Debug.LogError($"[DialogueSegmentTrigger] {gameObject.name}: No Collider found! Add a Collider component.");
            }

            // Check if DialogueManager exists
            if (DialogueManager.Instance == null)
            {
                Debug.LogError($"[DialogueSegmentTrigger] {gameObject.name}: DialogueManager.Instance is null! Create a GameObject with DialogueManager component.");
            }

            // Check if LevelManager exists (only warn if prerequisites are set)
            if (LevelManager.Instance == null && !string.IsNullOrWhiteSpace(requiredSegments))
            {
                Debug.LogWarning($"[DialogueSegmentTrigger] {gameObject.name}: LevelManager.Instance is null but required segments are set. Create a GameObject with LevelManager component.");
            }

            UpdatePrerequisites();
            UpdateVisualState();
        }

        void Update()
        {
            // Check prerequisites periodically (in case game state changes)
            bool previousPrerequisitesMet = prerequisitesMet;
            UpdatePrerequisites();

            if (prerequisitesMet != previousPrerequisitesMet)
            {
                UpdateVisualState();
            }

            // Handle manual interaction
            if (interactionMode == InteractionMode.OnInteract &&
                playerInRange &&
                prerequisitesMet &&
                Input.GetKeyDown(KeyCode.E))
            {
                if (enableDebugLogs) Debug.Log($"[DialogueSegmentTrigger] {gameObject.name}: E key pressed, triggering dialogue.");
                TriggerDialogue();
            }
            else if (interactionMode == InteractionMode.OnInteract && playerInRange && Input.GetKeyDown(KeyCode.E))
            {
                if (enableDebugLogs) Debug.Log($"[DialogueSegmentTrigger] {gameObject.name}: E key pressed but prerequisites not met. Prerequisites: {prerequisitesMet}, Player in range: {playerInRange}");
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (enableDebugLogs) Debug.Log($"[DialogueSegmentTrigger] {gameObject.name}: Player entered trigger zone.");
                playerInRange = true;
                UpdateVisualState();

                if (interactionMode == InteractionMode.OnEnter && prerequisitesMet)
                {
                    if (enableDebugLogs) Debug.Log($"[DialogueSegmentTrigger] {gameObject.name}: Triggering dialogue (OnEnter mode).");
                    TriggerDialogue();
                }
                else if (interactionMode == InteractionMode.OnEnter && !prerequisitesMet)
                {
                    if (enableDebugLogs) Debug.Log($"[DialogueSegmentTrigger] {gameObject.name}: Prerequisites not met, cannot trigger (OnEnter mode).");
                }
            }
            else
            {
                if (enableDebugLogs) Debug.Log($"[DialogueSegmentTrigger] {gameObject.name}: Object entered trigger but is not tagged 'Player'. Tag: {other.tag}");
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
                UpdateVisualState();
            }
        }

        private void UpdatePrerequisites()
        {
            prerequisitesMet = true;

            // Check required segments
            if (!string.IsNullOrWhiteSpace(requiredSegments))
            {
                if (LevelManager.Instance == null)
                {
                    Debug.LogWarning($"[DialogueSegmentTrigger] {gameObject.name}: LevelManager.Instance is null. Prerequisites cannot be checked. Make sure LevelManager exists in scene.");
                    prerequisitesMet = false;
                    return;
                }

                string[] segments = requiredSegments.Split(',');
                foreach (var segment in segments)
                {
                    string trimmedSegment = segment.Trim();
                    if (!string.IsNullOrEmpty(trimmedSegment))
                    {
                        bool segmentCompleted = LevelManager.Instance.IsSegmentCompleted(trimmedSegment);
                        if (!segmentCompleted)
                        {
                            prerequisitesMet = false;
                            if (enableDebugLogs) Debug.Log($"[DialogueSegmentTrigger] {gameObject.name}: Required segment '{trimmedSegment}' not completed yet.");
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
                    Debug.LogWarning($"[DialogueSegmentTrigger] {gameObject.name}: GameState.Instance is null. Condition cannot be evaluated.");
                    prerequisitesMet = false;
                }
                else
                {
                    prerequisitesMet = Core.GameState.Instance.EvaluateCondition(requiredCondition);
                    if (!prerequisitesMet)
                    {
                        if (enableDebugLogs) Debug.Log($"[DialogueSegmentTrigger] {gameObject.name}: Condition '{requiredCondition}' not met.");
                    }
                }
            }

            // Note: Removed spammy per-frame log for "Prerequisites met (no requirements)"
            // This was being called every frame in Update()
        }

        private void UpdateVisualState()
        {
            if (interactionPromptUI != null)
            {
                interactionPromptUI.SetActive(showInteractionPrompt && playerInRange && prerequisitesMet && !hasTriggered);
            }
        }

        private void TriggerDialogue()
        {
            // Check prerequisites again
            if (!prerequisitesMet)
            {
                if (enableDebugLogs) Debug.Log($"[DialogueSegmentTrigger] Prerequisites not met for {gameObject.name}");
                return;
            }

            // Check if already triggered (single use)
            if (singleUse && hasTriggered)
                return;

            // Validate dialogue tree
            if (dialogueTree == null)
            {
                Debug.LogError($"[DialogueSegmentTrigger] No dialogue tree assigned on {gameObject.name}");
                return;
            }

            // Validate DialogueManager
            if (DialogueManager.Instance == null)
            {
                Debug.LogError("[DialogueSegmentTrigger] DialogueManager.Instance is null. Make sure DialogueManager exists in scene.");
                return;
            }

            hasTriggered = true;
            UpdateVisualState();

            // Pause player if needed
            if (pausePlayerDuringDialogue && playerController != null)
            {
                playerController.SetInputEnabled(false);
            }

            // Start dialogue
            DialogueManager.Instance.StartDialogue(dialogueTree);

            // Subscribe to dialogue end
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnDialogueEnded += OnDialogueEndedHandler;
            }
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
            {
                DialogueManager.Instance.OnDialogueEnded -= OnDialogueEndedHandler;
            }
        }
    }
}

