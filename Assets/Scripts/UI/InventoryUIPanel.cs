using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WhisperingGate.Gameplay;

namespace WhisperingGate.UI
{
    /// <summary>
    /// Inventory UI controller that listens to InventoryManager events and populates slots & detail view.
    /// </summary>
    public class InventoryUIPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Transform slotsParent;
        [SerializeField] private InventorySlotUI slotPrefab;
        [SerializeField] private Transform hotbarParent;
        [SerializeField] private InventorySlotUI hotbarSlotPrefab;
        [SerializeField] private Image detailIcon;
        [SerializeField] private TMP_Text detailNameText;
        [SerializeField] private TMP_Text detailDescriptionText;

        [Header("Behavior")]
        [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
        [SerializeField] private bool openOnStart = false;
        [SerializeField] private KeyCode hotbarPrevKey = KeyCode.Q;
        [SerializeField] private KeyCode hotbarNextKey = KeyCode.E;
        [SerializeField, Range(3, 10)] private int hotbarSize = 4;

        private readonly Dictionary<string, InventorySlotUI> slotLookup = new();
        private readonly List<string> hotbarItems = new();
        private readonly List<InventorySlotUI> hotbarSlots = new();
        private int hotbarIndex;
        private string selectedItemId;

        private void Start()
        {
            if (InventoryManager.Instance == null)
            {
                Debug.LogWarning("[InventoryUIPanel] InventoryManager.Instance not found in scene.");
                enabled = false;
                return;
            }

            InventoryManager.Instance.OnItemAdded += HandleItemAdded;
            InventoryManager.Instance.OnItemRemoved += HandleItemRemoved;

            RefreshAllSlots();
            BuildHotbar();
            panelRoot?.SetActive(openOnStart);

            if (!openOnStart)
            {
                ClearDetails();
            }
        }

        private void OnDestroy()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnItemAdded -= HandleItemAdded;
                InventoryManager.Instance.OnItemRemoved -= HandleItemRemoved;
            }
        }

        private void Update()
        {
            if (panelRoot != null && Input.GetKeyDown(toggleKey))
            {
                bool isOpening = !panelRoot.activeSelf;
                panelRoot.SetActive(isOpening);
                
                // Unlock cursor when opening, lock when closing
                if (isOpening)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    ClearDetails();
                }
            }

            HandleHotbarInput();
        }

        private void HandleHotbarInput()
        {
            if (hotbarSlots.Count == 0)
                return;

            if (Input.GetAxis("Mouse ScrollWheel") > 0f || Input.GetKeyDown(hotbarNextKey))
            {
                CycleHotbar(1);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f || Input.GetKeyDown(hotbarPrevKey))
            {
                CycleHotbar(-1);
            }
        }

        private void RefreshAllSlots()
        {
            foreach (Transform child in slotsParent)
            {
                Destroy(child.gameObject);
            }
            slotLookup.Clear();
            hotbarItems.Clear();

            var inventory = InventoryManager.Instance;
            foreach (var itemId in inventory.GetAllItems())
            {
                AddSlot(itemId);
            }
        }

        private void BuildHotbar()
        {
            foreach (Transform child in hotbarParent)
            {
                Destroy(child.gameObject);
            }
            hotbarSlots.Clear();

            for (int i = 0; i < hotbarSize; i++)
            {
                var slot = Instantiate(hotbarSlotPrefab, hotbarParent);
                slot.SetSelected(i == hotbarIndex);
                hotbarSlots.Add(slot);
            }

            UpdateHotbarVisuals();
        }

        private void HandleItemAdded(string itemId)
        {
            AddSlot(itemId);
            UpdateHotbarVisuals();
        }

        private void HandleItemRemoved(string itemId)
        {
            if (slotLookup.TryGetValue(itemId, out var slot))
            {
                Destroy(slot.gameObject);
                slotLookup.Remove(itemId);
            }

            if (selectedItemId == itemId)
            {
                selectedItemId = null;
                ClearDetails();
            }

            hotbarItems.Remove(itemId);
            UpdateHotbarVisuals();
        }

        private void AddSlot(string itemId)
        {
            if (slotLookup.ContainsKey(itemId))
                return;

            var itemData = InventoryManager.Instance.GetItemData(itemId);
            if (itemData == null)
                return;

            var slot = Instantiate(slotPrefab, slotsParent);
            slot.Initialize(itemData, HandleSlotClicked, HandleSlotHovered, HandleSlotHoverExit);
            slot.SetSelected(false);
            slotLookup.Add(itemId, slot);

            if (!hotbarItems.Contains(itemId))
                hotbarItems.Add(itemId);
        }

        private void HandleSlotClicked(string itemId)
        {
            selectedItemId = itemId;

            foreach (var kv in slotLookup)
            {
                kv.Value.SetSelected(kv.Key == itemId);
            }

            ShowItemDetails(itemId);
        }

        private void HandleSlotHovered(string itemId)
        {
            if (panelRoot == null || !panelRoot.activeSelf)
                return;

            if (string.IsNullOrEmpty(itemId))
            {
                Debug.LogWarning("[InventoryUIPanel] HandleSlotHovered called with null/empty itemId");
                return;
            }

            ShowItemDetails(itemId);
        }

        private void HandleSlotHoverExit()
        {
            // Optionally clear details when mouse leaves, or keep last hovered item visible
            // For now, we'll keep the details visible until another item is hovered or panel closes
        }

        private void ShowItemDetails(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                ClearDetails();
                return;
            }

            var data = InventoryManager.Instance?.GetItemData(itemId);
            if (data == null)
            {
                Debug.LogWarning($"[InventoryUIPanel] No item data found for ID: {itemId}");
                ClearDetails();
                return;
            }

            // Update detail panel
            if (detailIcon != null)
            {
                detailIcon.sprite = data.itemIcon;
                detailIcon.enabled = data.itemIcon != null;
            }
            
            if (detailNameText != null)
            {
                detailNameText.text = string.IsNullOrEmpty(data.itemName) ? "Unknown Item" : data.itemName;
            }
            
            if (detailDescriptionText != null)
            {
                detailDescriptionText.text = string.IsNullOrEmpty(data.description) ? "No description available." : data.description;
            }

            // Update hotbar selection if item is in hotbar
            int hotbarPos = hotbarItems.IndexOf(itemId);
            if (hotbarPos >= 0)
            {
                hotbarIndex = hotbarPos;
                UpdateHotbarVisuals();
            }
        }

        private void ClearDetails()
        {
            if (detailIcon != null)
                detailIcon.sprite = null;
            if (detailNameText != null)
                detailNameText.text = "";
            if (detailDescriptionText != null)
                detailDescriptionText.text = "";

            foreach (var slot in slotLookup.Values)
            {
                slot.SetSelected(false);
            }
        }

        private void UpdateHotbarVisuals()
        {
            if (hotbarSlots.Count == 0)
                return;

            for (int i = 0; i < hotbarSlots.Count; i++)
            {
                var slot = hotbarSlots[i];
                if (i < hotbarItems.Count)
                {
                    var data = InventoryManager.Instance.GetItemData(hotbarItems[i]);
                    if (data != null)
                    {
                        slot.gameObject.SetActive(true);
                        slot.Initialize(data, HandleSlotClicked);
                    }
                    else
                    {
                        slot.gameObject.SetActive(false);
                    }
                }
                else
                {
                    slot.gameObject.SetActive(false);
                }

                slot.SetSelected(i == hotbarIndex);
            }
        }

        private void CycleHotbar(int direction)
        {
            if (hotbarItems.Count == 0)
                return;

            hotbarIndex = (hotbarIndex + direction) % hotbarItems.Count;
            if (hotbarIndex < 0)
                hotbarIndex += hotbarItems.Count;

            UpdateHotbarVisuals();
        }
    }
}

