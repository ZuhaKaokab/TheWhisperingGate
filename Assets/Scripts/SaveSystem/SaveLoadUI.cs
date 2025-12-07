using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace WhisperingGate.SaveSystem
{
    /// <summary>
    /// Simple UI for save/load functionality.
    /// Can be customized or replaced with a more elaborate UI.
    /// </summary>
    public class SaveLoadUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Transform slotsContainer;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI titleText;

        [Header("Settings")]
        [SerializeField] private KeyCode toggleKey = KeyCode.Escape;
        [SerializeField] private bool pauseGameWhenOpen = true;

        private bool isSaveMode = true; // true = save, false = load
        private SaveSlotUI[] slotUIs;

        private void Start()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (panel != null)
                panel.SetActive(false);

            // Subscribe to save events for feedback
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnSaveCompleted += OnSaveCompleted;
                SaveManager.Instance.OnLoadCompleted += OnLoadCompleted;
            }
        }

        private void OnDestroy()
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnSaveCompleted -= OnSaveCompleted;
                SaveManager.Instance.OnLoadCompleted -= OnLoadCompleted;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                if (panel != null && panel.activeSelf)
                    Hide();
                else
                    ShowLoadMenu(); // Default to load menu on Escape
            }
        }

        /// <summary>
        /// Show the save menu.
        /// </summary>
        public void ShowSaveMenu()
        {
            isSaveMode = true;
            Show();
        }

        /// <summary>
        /// Show the load menu.
        /// </summary>
        public void ShowLoadMenu()
        {
            isSaveMode = false;
            Show();
        }

        private void Show()
        {
            if (panel == null) return;

            panel.SetActive(true);
            
            if (titleText != null)
                titleText.text = isSaveMode ? "SAVE GAME" : "LOAD GAME";

            RefreshSlots();

            if (pauseGameWhenOpen)
                Time.timeScale = 0f;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        /// <summary>
        /// Hide the save/load menu.
        /// </summary>
        public void Hide()
        {
            if (panel == null) return;

            panel.SetActive(false);

            if (pauseGameWhenOpen)
                Time.timeScale = 1f;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void RefreshSlots()
        {
            if (SaveManager.Instance == null || slotsContainer == null) return;

            // Clear existing slots
            foreach (Transform child in slotsContainer)
            {
                Destroy(child.gameObject);
            }

            var slotInfos = SaveManager.Instance.GetAllSlotInfos();
            slotUIs = new SaveSlotUI[slotInfos.Count];

            for (int i = 0; i < slotInfos.Count; i++)
            {
                int slotIndex = i; // Capture for closure
                var info = slotInfos[i];

                GameObject slotObj = slotPrefab != null 
                    ? Instantiate(slotPrefab, slotsContainer) 
                    : CreateDefaultSlot(slotsContainer);

                var slotUI = slotObj.GetComponent<SaveSlotUI>();
                if (slotUI == null)
                    slotUI = slotObj.AddComponent<SaveSlotUI>();

                slotUI.Setup(slotIndex, info, isSaveMode, OnSlotClicked);
                slotUIs[i] = slotUI;
            }
        }

        private GameObject CreateDefaultSlot(Transform parent)
        {
            // Create a simple default slot if no prefab provided
            GameObject slot = new GameObject("SaveSlot");
            slot.transform.SetParent(parent, false);

            var layout = slot.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10;
            layout.padding = new RectOffset(10, 10, 5, 5);

            var bg = slot.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

            var button = slot.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f);
            button.colors = colors;

            // Add layout element
            var layoutElement = slot.AddComponent<LayoutElement>();
            layoutElement.minHeight = 60;
            layoutElement.preferredHeight = 60;

            return slot;
        }

        private void OnSlotClicked(int slotIndex)
        {
            if (SaveManager.Instance == null) return;

            if (isSaveMode)
            {
                SaveManager.Instance.Save(slotIndex);
            }
            else
            {
                if (SaveManager.Instance.SaveExists(slotIndex))
                {
                    SaveManager.Instance.Load(slotIndex);
                    Hide();
                }
            }
        }

        private void OnSaveCompleted(int slot, bool success)
        {
            if (success)
            {
                RefreshSlots();
                Debug.Log($"[SaveLoadUI] Saved to slot {slot}");
            }
        }

        private void OnLoadCompleted(int slot, bool success)
        {
            if (success)
            {
                Debug.Log($"[SaveLoadUI] Loaded from slot {slot}");
            }
        }
    }

    /// <summary>
    /// Individual save slot UI component.
    /// </summary>
    public class SaveSlotUI : MonoBehaviour
    {
        private TextMeshProUGUI slotText;
        private Button button;
        private int slotIndex;
        private Action<int> onClickCallback;

        public void Setup(int index, SaveSlotInfo info, bool isSaveMode, Action<int> onClick)
        {
            slotIndex = index;
            onClickCallback = onClick;

            // Find or create text
            slotText = GetComponentInChildren<TextMeshProUGUI>();
            if (slotText == null)
            {
                GameObject textObj = new GameObject("SlotText");
                textObj.transform.SetParent(transform, false);
                slotText = textObj.AddComponent<TextMeshProUGUI>();
                slotText.fontSize = 16;
                slotText.alignment = TextAlignmentOptions.Left;
                
                var rect = slotText.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = new Vector2(10, 5);
                rect.offsetMax = new Vector2(-10, -5);
            }

            // Setup button
            button = GetComponent<Button>();
            if (button == null)
                button = gameObject.AddComponent<Button>();

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClickCallback?.Invoke(slotIndex));

            // Update display
            if (info.isEmpty)
            {
                slotText.text = $"Slot {index + 1}: <color=#666>Empty</color>";
                button.interactable = isSaveMode; // Can only click empty slots in save mode
            }
            else
            {
                string timeAgo = GetTimeAgo(info.saveTimestamp);
                string playTime = FormatPlayTime(info.playTime);
                slotText.text = $"Slot {index + 1}: {info.saveName}\n" +
                               $"<size=12><color=#888>{info.currentScene} | {playTime} | {timeAgo}</color></size>";
                button.interactable = true;
            }

            // Auto-save slot styling
            if (index == 0)
            {
                slotText.text = slotText.text.Replace($"Slot {index + 1}", "<color=#55ff55>AUTO</color>");
            }
        }

        private string GetTimeAgo(DateTime timestamp)
        {
            TimeSpan diff = DateTime.Now - timestamp;

            if (diff.TotalMinutes < 1)
                return "Just now";
            if (diff.TotalHours < 1)
                return $"{(int)diff.TotalMinutes}m ago";
            if (diff.TotalDays < 1)
                return $"{(int)diff.TotalHours}h ago";
            if (diff.TotalDays < 7)
                return $"{(int)diff.TotalDays}d ago";
            
            return timestamp.ToString("MMM dd");
        }

        private string FormatPlayTime(float seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            if (time.TotalHours >= 1)
                return $"{(int)time.TotalHours}h {time.Minutes}m";
            return $"{time.Minutes}m {time.Seconds}s";
        }
    }
}

