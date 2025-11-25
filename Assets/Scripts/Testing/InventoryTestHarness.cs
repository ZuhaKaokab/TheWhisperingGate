using UnityEngine;
using WhisperingGate.Gameplay;

namespace WhisperingGate.Testing
{
    /// <summary>
    /// Helper MonoBehaviour that adds/removes items from InventoryManager via hotkeys.
    /// Attach to any object to verify item events and UI reactions.
    /// </summary>
    public class InventoryTestHarness : MonoBehaviour
    {
        [Header("Add Item")]
        [SerializeField] private string addItemId = "journal";
        [SerializeField] private KeyCode addItemKey = KeyCode.Alpha4;

        [Header("Remove Item")]
        [SerializeField] private string removeItemId = "journal";
        [SerializeField] private KeyCode removeItemKey = KeyCode.Alpha5;

        [Header("Query Item")]
        [SerializeField] private string queryItemId = "journal";
        [SerializeField] private KeyCode queryItemKey = KeyCode.Alpha6;

        private void OnEnable()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnItemAdded += HandleItemAdded;
                InventoryManager.Instance.OnItemRemoved += HandleItemRemoved;
            }
        }

        private void OnDisable()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnItemAdded -= HandleItemAdded;
                InventoryManager.Instance.OnItemRemoved -= HandleItemRemoved;
            }
        }

        private void Update()
        {
            var inventory = InventoryManager.Instance;
            if (inventory == null)
                return;

            if (Input.GetKeyDown(addItemKey))
            {
                inventory.AddItem(addItemId);
            }

            if (Input.GetKeyDown(removeItemKey))
            {
                inventory.RemoveItem(removeItemId);
            }

            if (Input.GetKeyDown(queryItemKey))
            {
                bool hasItem = inventory.HasItem(queryItemId);
                Debug.Log($"[InventoryTest] Has '{queryItemId}'? {hasItem}");
            }
        }

        private void HandleItemAdded(string itemId)
        {
            Debug.Log($"[InventoryTest] OnItemAdded fired for '{itemId}'.");
        }

        private void HandleItemRemoved(string itemId)
        {
            Debug.Log($"[InventoryTest] OnItemRemoved fired for '{itemId}'.");
        }
    }
}

