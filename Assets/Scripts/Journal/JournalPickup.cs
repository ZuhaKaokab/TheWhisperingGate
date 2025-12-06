using UnityEngine;
using WhisperingGate.Core;
using WhisperingGate.Gameplay;

namespace WhisperingGate.Journal
{
    /// <summary>
    /// Attach to the physical journal object in the world.
    /// Allows player to pick up the journal and add it to their inventory.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class JournalPickup : MonoBehaviour
    {
        [Header("Interaction")]
        [Tooltip("Require player to press key to pick up")]
        [SerializeField] private bool requireInteraction = true;
        
        [Tooltip("Key to pick up the journal")]
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        
        [Tooltip("Maximum distance for interaction")]
        [SerializeField] private float interactDistance = 2f;

        [Header("Visual Feedback")]
        [Tooltip("UI prompt to show when player is nearby")]
        [SerializeField] private GameObject interactionPrompt;
        
        [Tooltip("Highlight effect when player is nearby")]
        [SerializeField] private GameObject highlightEffect;

        [Header("Inventory Integration")]
        [Tooltip("Add journal to inventory when picked up")]
        [SerializeField] private bool addToInventory = true;
        
        [Tooltip("Item ID for the journal in inventory system")]
        [SerializeField] private string inventoryItemId = "journal";

        [Header("Pickup Effects")]
        [Tooltip("Sound to play on pickup")]
        [SerializeField] private AudioClip pickupSound;
        
        [Tooltip("Particle effect on pickup")]
        [SerializeField] private ParticleSystem pickupParticles;
        
        [Tooltip("Destroy object after pickup (false = just disable)")]
        [SerializeField] private bool destroyOnPickup = false;

        [Header("Auto Unlock Pages")]
        [Tooltip("Page IDs to unlock when journal is picked up")]
        [SerializeField] private string[] autoUnlockPages;

        [Header("Commands on Pickup")]
        [Tooltip("Commands to execute when picked up (e.g., 'flag:journal_found')")]
        [SerializeField] private string[] onPickupCommands;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        // Runtime
        private bool isPickedUp = false;
        private bool playerInRange = false;
        private Transform playerTransform;

        private void Start()
        {
            // Ensure collider is trigger
            var col = GetComponent<Collider>();
            if (col != null && !col.isTrigger)
            {
                col.isTrigger = true;
            }

            // Hide prompt initially
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
            
            if (highlightEffect != null)
                highlightEffect.SetActive(false);

            // Check if already picked up (from saved state)
            if (GameState.Instance != null && GameState.Instance.GetBool("has_journal"))
            {
                // Already have journal, hide this pickup
                gameObject.SetActive(false);
                isPickedUp = true;
            }
        }

        private void Update()
        {
            if (isPickedUp) return;

            // Check for interaction
            if (playerInRange && requireInteraction && Input.GetKeyDown(interactKey))
            {
                // Additional distance check for precision
                if (playerTransform != null)
                {
                    float distance = Vector3.Distance(transform.position, playerTransform.position);
                    if (distance <= interactDistance)
                    {
                        PickUp();
                    }
                }
                else
                {
                    PickUp();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isPickedUp) return;

            if (other.CompareTag("Player"))
            {
                playerInRange = true;
                playerTransform = other.transform;
                
                if (enableDebugLogs) Debug.Log("[JournalPickup] Player in range");

                if (requireInteraction)
                {
                    // Show prompt
                    if (interactionPrompt != null)
                        interactionPrompt.SetActive(true);
                    
                    if (highlightEffect != null)
                        highlightEffect.SetActive(true);
                }
                else
                {
                    // Auto pickup on touch
                    PickUp();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
                playerTransform = null;

                // Hide prompt
                if (interactionPrompt != null)
                    interactionPrompt.SetActive(false);
                
                if (highlightEffect != null)
                    highlightEffect.SetActive(false);
            }
        }

        /// <summary>
        /// Pick up the journal.
        /// </summary>
        public void PickUp()
        {
            if (isPickedUp) return;
            isPickedUp = true;

            if (enableDebugLogs) Debug.Log("[JournalPickup] Journal picked up!");

            // Notify journal manager
            if (JournalManager.Instance != null)
            {
                JournalManager.Instance.PickUpJournal();

                // Unlock starting pages
                if (autoUnlockPages != null)
                {
                    foreach (string pageId in autoUnlockPages)
                    {
                        if (!string.IsNullOrWhiteSpace(pageId))
                        {
                            JournalManager.Instance.UnlockPage(pageId.Trim());
                        }
                    }
                }
            }

            // Add to inventory
            if (addToInventory && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem(inventoryItemId);
                if (enableDebugLogs) Debug.Log($"[JournalPickup] Added '{inventoryItemId}' to inventory");
            }

            // Execute pickup commands
            ExecutePickupCommands();

            // Play sound
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            // Play particles
            if (pickupParticles != null)
            {
                pickupParticles.transform.SetParent(null);
                pickupParticles.Play();
                Destroy(pickupParticles.gameObject, pickupParticles.main.duration + 1f);
            }

            // Hide prompts
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
            
            if (highlightEffect != null)
                highlightEffect.SetActive(false);

            // Remove object
            if (destroyOnPickup)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Execute commands specified for pickup.
        /// </summary>
        private void ExecutePickupCommands()
        {
            if (onPickupCommands == null || GameState.Instance == null) return;

            foreach (string command in onPickupCommands)
            {
                if (string.IsNullOrWhiteSpace(command)) continue;

                string trimmed = command.Trim();
                int colonIndex = trimmed.IndexOf(':');
                
                if (colonIndex > 0)
                {
                    string type = trimmed.Substring(0, colonIndex).ToLower();
                    string param = trimmed.Substring(colonIndex + 1);

                    switch (type)
                    {
                        case "flag":
                            GameState.Instance.SetBool(param.Trim(), true);
                            if (enableDebugLogs) Debug.Log($"[JournalPickup] Set flag: {param}");
                            break;
                            
                        case "unflag":
                            GameState.Instance.SetBool(param.Trim(), false);
                            break;
                            
                        case "var":
                            // Parse var:name=value
                            int eqIndex = param.IndexOf('=');
                            if (eqIndex > 0)
                            {
                                string varName = param.Substring(0, eqIndex).Trim();
                                string varValue = param.Substring(eqIndex + 1).Trim();
                                if (int.TryParse(varValue, out int intVal))
                                {
                                    GameState.Instance.SetInt(varName, intVal);
                                }
                            }
                            break;
                    }
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Draw interaction range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactDistance);
        }
#endif
    }
}

