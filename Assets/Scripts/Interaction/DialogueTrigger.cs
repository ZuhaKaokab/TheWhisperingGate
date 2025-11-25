using UnityEngine;
using WhisperingGate.Dialogue;

namespace WhisperingGate.Interaction
{
    /// <summary>
    /// Component that triggers dialogue when player enters a trigger zone or presses interact key.
    /// Supports both automatic (OnEnter) and manual (OnInteract) trigger modes.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class DialogueTrigger : MonoBehaviour
    {
        [Header("Dialogue Settings")]
        [SerializeField] private DialogueTree dialogueTree;
        [SerializeField] private InteractionMode interactionMode = InteractionMode.OnInteract;
        [SerializeField] private bool singleUse = false;
        [SerializeField] private bool pausePlayerDuringDialogue = true;
        
        private bool hasTriggered = false;
        private bool playerInRange = false;
        private Gameplay.PlayerController playerController;
        
        public enum InteractionMode { OnEnter, OnInteract }
        
        void Start()
        {
            // Find player controller (may not exist yet)
            playerController = FindObjectOfType<Gameplay.PlayerController>();
            
            // Ensure collider is set as trigger
            var collider = GetComponent<Collider>();
            if (collider != null)
                collider.isTrigger = true;
            else
                Debug.LogWarning($"[DialogueTrigger] No Collider found on {gameObject.name}. Adding BoxCollider.");
        }
        
        void Update()
        {
            // Handle manual interaction (E key)
            if (interactionMode == InteractionMode.OnInteract && 
                playerInRange && 
                Input.GetKeyDown(KeyCode.E))
            {
                TriggerDialogue();
            }
        }
        
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = true;
                if (interactionMode == InteractionMode.OnEnter)
                {
                    TriggerDialogue();
                }
            }
        }
        
        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
            }
        }
        
        private void TriggerDialogue()
        {
            // Check if already triggered (single use)
            if (singleUse && hasTriggered)
                return;
            
            // Validate dialogue tree
            if (dialogueTree == null)
            {
                Debug.LogError($"[DialogueTrigger] No dialogue tree assigned on {gameObject.name}");
                return;
            }
            
            // Validate DialogueManager
            if (DialogueManager.Instance == null)
            {
                Debug.LogError("[DialogueTrigger] DialogueManager.Instance is null. Make sure DialogueManager exists in scene.");
                return;
            }
            
            hasTriggered = true;
            
            // Pause player if needed
            if (pausePlayerDuringDialogue && playerController != null)
            {
                playerController.SetInputEnabled(false);
            }
            
            // Start dialogue
            DialogueManager.Instance.StartDialogue(dialogueTree);
            
            // Subscribe to dialogue end to restore player control
            if (pausePlayerDuringDialogue)
            {
                DialogueManager.Instance.OnDialogueEnded += OnDialogueEndedHandler;
            }
        }
        
        private void OnDialogueEndedHandler()
        {
            if (playerController != null)
                playerController.SetInputEnabled(true);
            
            // Unsubscribe to prevent memory leaks
            if (DialogueManager.Instance != null)
                DialogueManager.Instance.OnDialogueEnded -= OnDialogueEndedHandler;
        }
    }
}

