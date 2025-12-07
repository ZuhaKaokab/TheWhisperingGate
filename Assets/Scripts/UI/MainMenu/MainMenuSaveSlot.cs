using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using WhisperingGate.SaveSystem;

namespace WhisperingGate.UI
{
    /// <summary>
    /// Individual save slot UI for the main menu load game panel.
    /// </summary>
    public class MainMenuSaveSlot : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI slotNameText;
        [SerializeField] private TextMeshProUGUI detailsText;
        [SerializeField] private TextMeshProUGUI timestampText;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private GameObject emptyStateObject;
        [SerializeField] private GameObject filledStateObject;
        [SerializeField] private Image slotBackground;

        [Header("Visual Settings")]
        [SerializeField] private Color emptySlotColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        [SerializeField] private Color filledSlotColor = new Color(0.15f, 0.15f, 0.2f, 0.95f);
        [SerializeField] private Color autoSaveColor = new Color(0.1f, 0.2f, 0.1f, 0.95f);

        private int slotIndex;
        private Action<int> onLoadCallback;
        private Action<int> onDeleteCallback;
        private bool isEmpty;

        /// <summary>
        /// Setup the save slot with data.
        /// </summary>
        public void Setup(int index, SaveSlotInfo info, Action<int> onLoad, Action<int> onDelete)
        {
            slotIndex = index;
            onLoadCallback = onLoad;
            onDeleteCallback = onDelete;
            isEmpty = info.isEmpty;

            // Setup buttons
            SetupButtons();

            if (info.isEmpty)
            {
                DisplayEmptySlot(index);
            }
            else
            {
                DisplayFilledSlot(index, info);
            }
        }

        private void SetupButtons()
        {
            if (loadButton != null)
            {
                loadButton.onClick.RemoveAllListeners();
                loadButton.onClick.AddListener(() => onLoadCallback?.Invoke(slotIndex));
            }

            if (deleteButton != null)
            {
                deleteButton.onClick.RemoveAllListeners();
                deleteButton.onClick.AddListener(() => onDeleteCallback?.Invoke(slotIndex));
            }
        }

        private void DisplayEmptySlot(int index)
        {
            // Show empty state
            if (emptyStateObject != null) emptyStateObject.SetActive(true);
            if (filledStateObject != null) filledStateObject.SetActive(false);

            // Set text
            string slotLabel = index == 0 ? "AUTO SAVE" : $"SLOT {index}";
            
            if (slotNameText != null)
            {
                slotNameText.text = slotLabel;
                slotNameText.color = new Color(0.5f, 0.5f, 0.5f);
            }

            if (detailsText != null)
            {
                detailsText.text = "- Empty -";
                detailsText.color = new Color(0.4f, 0.4f, 0.4f);
            }

            if (timestampText != null)
            {
                timestampText.text = "";
            }

            // Disable buttons
            if (loadButton != null) loadButton.interactable = false;
            if (deleteButton != null) deleteButton.gameObject.SetActive(false);

            // Background color
            if (slotBackground != null)
                slotBackground.color = emptySlotColor;
        }

        private void DisplayFilledSlot(int index, SaveSlotInfo info)
        {
            // Show filled state
            if (emptyStateObject != null) emptyStateObject.SetActive(false);
            if (filledStateObject != null) filledStateObject.SetActive(true);

            // Slot name
            string slotLabel = index == 0 ? "AUTO SAVE" : $"SLOT {index}";
            
            if (slotNameText != null)
            {
                slotNameText.text = $"{slotLabel}: {info.saveName}";
                slotNameText.color = index == 0 ? new Color(0.5f, 1f, 0.5f) : Color.white;
            }

            // Details
            if (detailsText != null)
            {
                string playTime = FormatPlayTime(info.playTime);
                string scene = string.IsNullOrEmpty(info.currentScene) ? "Unknown" : info.currentScene;
                detailsText.text = $"{scene}  •  {playTime}";
                detailsText.color = new Color(0.7f, 0.7f, 0.7f);
            }

            // Timestamp
            if (timestampText != null)
            {
                timestampText.text = FormatTimestamp(info.saveTimestamp);
                timestampText.color = new Color(0.5f, 0.5f, 0.5f);
            }

            // Enable buttons
            if (loadButton != null) loadButton.interactable = true;
            if (deleteButton != null) deleteButton.gameObject.SetActive(true);

            // Background color
            if (slotBackground != null)
                slotBackground.color = index == 0 ? autoSaveColor : filledSlotColor;
        }

        private string FormatPlayTime(float totalSeconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(totalSeconds);
            
            if (time.TotalHours >= 1)
                return $"{(int)time.TotalHours}h {time.Minutes}m";
            else if (time.TotalMinutes >= 1)
                return $"{time.Minutes}m {time.Seconds}s";
            else
                return $"{time.Seconds}s";
        }

        private string FormatTimestamp(DateTime timestamp)
        {
            TimeSpan diff = DateTime.Now - timestamp;

            if (diff.TotalMinutes < 1)
                return "Just now";
            else if (diff.TotalMinutes < 60)
                return $"{(int)diff.TotalMinutes} min ago";
            else if (diff.TotalHours < 24)
                return $"{(int)diff.TotalHours} hours ago";
            else if (diff.TotalDays < 7)
                return $"{(int)diff.TotalDays} days ago";
            else
                return timestamp.ToString("MMM dd, yyyy");
        }

        /// <summary>
        /// Create UI elements if not assigned (called automatically if needed).
        /// </summary>
        public void CreateDefaultUI()
        {
            // Only create if elements are missing
            if (slotNameText == null)
            {
                GameObject nameObj = new GameObject("SlotName");
                nameObj.transform.SetParent(transform, false);
                slotNameText = nameObj.AddComponent<TextMeshProUGUI>();
                slotNameText.fontSize = 18;
                slotNameText.fontStyle = FontStyles.Bold;
                
                var nameRect = slotNameText.rectTransform;
                nameRect.anchorMin = new Vector2(0, 0.5f);
                nameRect.anchorMax = new Vector2(0.6f, 1f);
                nameRect.offsetMin = new Vector2(15, 5);
                nameRect.offsetMax = new Vector2(-5, -5);
            }

            if (detailsText == null)
            {
                GameObject detailsObj = new GameObject("Details");
                detailsObj.transform.SetParent(transform, false);
                detailsText = detailsObj.AddComponent<TextMeshProUGUI>();
                detailsText.fontSize = 14;
                
                var detailsRect = detailsText.rectTransform;
                detailsRect.anchorMin = new Vector2(0, 0);
                detailsRect.anchorMax = new Vector2(0.6f, 0.5f);
                detailsRect.offsetMin = new Vector2(15, 5);
                detailsRect.offsetMax = new Vector2(-5, -5);
            }

            if (timestampText == null)
            {
                GameObject timeObj = new GameObject("Timestamp");
                timeObj.transform.SetParent(transform, false);
                timestampText = timeObj.AddComponent<TextMeshProUGUI>();
                timestampText.fontSize = 12;
                timestampText.alignment = TextAlignmentOptions.Right;
                
                var timeRect = timestampText.rectTransform;
                timeRect.anchorMin = new Vector2(0.6f, 0);
                timeRect.anchorMax = new Vector2(1f, 0.5f);
                timeRect.offsetMin = new Vector2(5, 5);
                timeRect.offsetMax = new Vector2(-100, -5);
            }

            if (loadButton == null)
            {
                GameObject loadObj = new GameObject("LoadButton");
                loadObj.transform.SetParent(transform, false);
                
                var loadRect = loadObj.AddComponent<RectTransform>();
                loadRect.anchorMin = new Vector2(1, 0.5f);
                loadRect.anchorMax = new Vector2(1, 0.5f);
                loadRect.pivot = new Vector2(1, 0.5f);
                loadRect.anchoredPosition = new Vector2(-60, 0);
                loadRect.sizeDelta = new Vector2(50, 30);

                var loadBg = loadObj.AddComponent<Image>();
                loadBg.color = new Color(0.2f, 0.5f, 0.2f);

                loadButton = loadObj.AddComponent<Button>();

                GameObject loadTextObj = new GameObject("Text");
                loadTextObj.transform.SetParent(loadObj.transform, false);
                var loadText = loadTextObj.AddComponent<TextMeshProUGUI>();
                loadText.text = "LOAD";
                loadText.fontSize = 12;
                loadText.alignment = TextAlignmentOptions.Center;
                loadText.rectTransform.anchorMin = Vector2.zero;
                loadText.rectTransform.anchorMax = Vector2.one;
                loadText.rectTransform.offsetMin = Vector2.zero;
                loadText.rectTransform.offsetMax = Vector2.zero;
            }

            if (deleteButton == null)
            {
                GameObject deleteObj = new GameObject("DeleteButton");
                deleteObj.transform.SetParent(transform, false);
                
                var deleteRect = deleteObj.AddComponent<RectTransform>();
                deleteRect.anchorMin = new Vector2(1, 0.5f);
                deleteRect.anchorMax = new Vector2(1, 0.5f);
                deleteRect.pivot = new Vector2(1, 0.5f);
                deleteRect.anchoredPosition = new Vector2(-5, 0);
                deleteRect.sizeDelta = new Vector2(50, 30);

                var deleteBg = deleteObj.AddComponent<Image>();
                deleteBg.color = new Color(0.5f, 0.2f, 0.2f);

                deleteButton = deleteObj.AddComponent<Button>();

                GameObject deleteTextObj = new GameObject("Text");
                deleteTextObj.transform.SetParent(deleteObj.transform, false);
                var deleteText = deleteTextObj.AddComponent<TextMeshProUGUI>();
                deleteText.text = "DEL";
                deleteText.fontSize = 12;
                deleteText.alignment = TextAlignmentOptions.Center;
                deleteText.rectTransform.anchorMin = Vector2.zero;
                deleteText.rectTransform.anchorMax = Vector2.one;
                deleteText.rectTransform.offsetMin = Vector2.zero;
                deleteText.rectTransform.offsetMax = Vector2.zero;
            }

            // Add background if missing
            if (slotBackground == null)
            {
                slotBackground = GetComponent<Image>();
                if (slotBackground == null)
                    slotBackground = gameObject.AddComponent<Image>();
            }
        }

        private void Awake()
        {
            // Auto-create UI if elements are not assigned
            if (slotNameText == null || loadButton == null)
            {
                CreateDefaultUI();
            }
        }
    }
}

