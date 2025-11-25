using UnityEngine;
using System.Collections.Generic;
using System;

namespace WhisperingGate.Gameplay
{
    /// <summary>
    /// Singleton manager that handles item storage, retrieval, and persistence.
    /// Integrates with DialogueManager to receive items via commands.
    /// </summary>
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }
        
        [System.Serializable]
        public class InventoryItem
        {
            public string itemId;
            public string itemName;
            public Sprite itemIcon;
            [TextArea(2, 4)]
            public string description;
        }
        
        [Header("Item Database")]
        [SerializeField] private List<InventoryItem> allItems = new();
        
        private List<string> playerInventory = new();
        
        public event Action<string> OnItemAdded;
        public event Action<string> OnItemRemoved;
        
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        /// <summary>
        /// Adds an item to the player's inventory by ID. Fires OnItemAdded event.
        /// </summary>
        /// <param name="itemId">The ID of the item to add. Must match an item in allItems list.</param>
        public void AddItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                Debug.LogWarning("[InventoryManager] Attempted to add item with null or empty ID");
                return;
            }
            
            if (!playerInventory.Contains(itemId))
            {
                playerInventory.Add(itemId);
                OnItemAdded?.Invoke(itemId);
                Debug.Log($"[Inventory] Added: {itemId}");
            }
            else
            {
                Debug.Log($"[Inventory] Item {itemId} already in inventory");
            }
        }
        
        /// <summary>
        /// Removes an item from the player's inventory by ID. Fires OnItemRemoved event.
        /// </summary>
        /// <param name="itemId">The ID of the item to remove.</param>
        public void RemoveItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                Debug.LogWarning("[InventoryManager] Attempted to remove item with null or empty ID");
                return;
            }
            
            if (playerInventory.Contains(itemId))
            {
                playerInventory.Remove(itemId);
                OnItemRemoved?.Invoke(itemId);
                Debug.Log($"[Inventory] Removed: {itemId}");
            }
        }
        
        /// <summary>
        /// Checks if the player has a specific item.
        /// </summary>
        /// <param name="itemId">The ID of the item to check.</param>
        /// <returns>True if the item is in inventory, false otherwise.</returns>
        public bool HasItem(string itemId)
        {
            return playerInventory.Contains(itemId);
        }
        
        /// <summary>
        /// Gets a copy of all item IDs in the player's inventory.
        /// </summary>
        /// <returns>List of item IDs.</returns>
        public List<string> GetAllItems()
        {
            return new List<string>(playerInventory);
        }
        
        /// <summary>
        /// Gets the metadata for an item by ID from the allItems database.
        /// </summary>
        /// <param name="itemId">The ID of the item to look up.</param>
        /// <returns>InventoryItem data if found, null otherwise.</returns>
        public InventoryItem GetItemData(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
                return null;
                
            return allItems.Find(i => i != null && i.itemId == itemId);
        }
        
        /// <summary>
        /// Gets the count of items in the player's inventory.
        /// </summary>
        public int ItemCount => playerInventory.Count;
        
        /// <summary>
        /// Clears all items from the inventory. Useful for testing or reset.
        /// </summary>
        public void ClearInventory()
        {
            playerInventory.Clear();
            Debug.Log("[InventoryManager] Inventory cleared");
        }
    }
}

