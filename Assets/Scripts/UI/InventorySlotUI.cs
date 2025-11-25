using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WhisperingGate.Gameplay;

namespace WhisperingGate.UI
{
    /// <summary>
    /// UI element representing a single inventory item slot.
    /// Displays icon/text and notifies when clicked or hovered.
    /// </summary>
    public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text itemNameText;
        [SerializeField] private Button button;
        [SerializeField] private GameObject selectionHighlight;

        private string itemId;
        private Action<string> onClicked;
        private Action<string> onHovered;
        private Action onHoverExit;

        /// <summary>
        /// Initializes the slot visuals and click callback.
        /// </summary>
        public void Initialize(InventoryManager.InventoryItem itemData, Action<string> clickedCallback, Action<string> hoveredCallback = null, Action hoverExitCallback = null)
        {
            if (itemData == null)
            {
                Debug.LogError("[InventorySlotUI] Initialize called with null itemData");
                return;
            }

            itemId = itemData.itemId;
            onClicked = clickedCallback;
            onHovered = hoveredCallback;
            onHoverExit = hoverExitCallback;

            if (iconImage != null)
            {
                iconImage.sprite = itemData.itemIcon;
                iconImage.enabled = itemData.itemIcon != null;
            }

            if (itemNameText != null)
                itemNameText.text = string.IsNullOrEmpty(itemData.itemName) ? itemData.itemId : itemData.itemName;

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(HandleClicked);
            }
        }

        public void SetSelected(bool selected)
        {
            if (selectionHighlight != null)
                selectionHighlight.SetActive(selected);
        }

        private void HandleClicked()
        {
            onClicked?.Invoke(itemId);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!string.IsNullOrEmpty(itemId))
            {
                onHovered?.Invoke(itemId);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            onHoverExit?.Invoke();
        }
    }
}

