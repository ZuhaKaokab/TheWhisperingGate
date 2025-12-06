using UnityEngine;
using WhisperingGate.Core;
using WhisperingGate.Gameplay;

namespace WhisperingGate.Items
{
    /// <summary>
    /// Pickup for a flashlight object in the world.
    /// When picked up, adds flashlight to inventory and enables flashlight controller.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class FlashlightPickup : MonoBehaviour
    {
        [Header("Interaction")]
        [SerializeField] private bool requireInteraction = true;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private float interactDistance = 2f;

        [Header("Inventory")]
        [SerializeField] private string flashlightItemId = "flashlight";
        [SerializeField] private bool addToInventory = true;

        [Header("Flashlight State")]
        [Tooltip("Is this flashlight already on when picked up?")]
        [SerializeField] private bool startsOn = false;

        [Header("Visual Feedback")]
        [SerializeField] private GameObject interactionPrompt;
        [SerializeField] private GameObject highlightEffect;
        
        [Header("Flashlight Visual (on this object)")]
        [SerializeField] private Light flashlightLight;

        [Header("Audio")]
        [SerializeField] private AudioClip pickupSound;

        [Header("Pickup Effects")]
        [SerializeField] private bool destroyOnPickup = true;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        // Runtime
        private bool isPickedUp = false;
        private bool playerInRange = false;
        private Transform playerTransform;

        private void Start()
        {
            var col = GetComponent<Collider>();
            if (col != null && !col.isTrigger)
            {
                col.isTrigger = true;
            }

            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
            
            if (highlightEffect != null)
                highlightEffect.SetActive(false);

            // Check if already have flashlight
            if (GameState.Instance != null && GameState.Instance.GetBool("has_flashlight"))
            {
                gameObject.SetActive(false);
                isPickedUp = true;
            }

            // Set initial on/off state
            SetOnVisual(startsOn);
        }

        private void Update()
        {
            if (isPickedUp) return;

            if (playerInRange && requireInteraction && Input.GetKeyDown(interactKey))
            {
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

                if (enableDebugLogs) Debug.Log("[FlashlightPickup] Player in range");

                if (requireInteraction)
                {
                    if (interactionPrompt != null)
                        interactionPrompt.SetActive(true);
                    
                    if (highlightEffect != null)
                        highlightEffect.SetActive(true);
                }
                else
                {
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

                if (interactionPrompt != null)
                    interactionPrompt.SetActive(false);
                
                if (highlightEffect != null)
                    highlightEffect.SetActive(false);
            }
        }

        public void PickUp()
        {
            if (isPickedUp) return;
            isPickedUp = true;

            if (enableDebugLogs) Debug.Log("[FlashlightPickup] Flashlight picked up!");

            // Set game state
            if (GameState.Instance != null)
            {
                GameState.Instance.SetBool("has_flashlight", true);
            }

            // Add to inventory
            if (addToInventory && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem(flashlightItemId);
                if (enableDebugLogs) Debug.Log($"[FlashlightPickup] Added '{flashlightItemId}' to inventory");
            }

            // Enable flashlight controller on player
            if (FlashlightController.Instance != null)
            {
                FlashlightController.Instance.EnableFlashlight(startsOn);
                if (enableDebugLogs) Debug.Log($"[FlashlightPickup] Flashlight controller enabled, on: {startsOn}");
            }

            // Play sound
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
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

        private void SetOnVisual(bool on)
        {
            if (flashlightLight != null)
                flashlightLight.enabled = on;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactDistance);
        }
#endif
    }
}

