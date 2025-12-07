using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using WhisperingGate.SaveSystem;
using WhisperingGate.Gameplay;

namespace WhisperingGate.UI
{
    /// <summary>
    /// In-game pause menu with save/load functionality.
    /// </summary>
    public class PauseMenuManager : MonoBehaviour
    {
        public static PauseMenuManager Instance { get; private set; }

        [Header("Scene Settings")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Header("Pause Menu Panel")]
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button saveGameButton;
        [SerializeField] private Button loadGameButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Save Game Panel")]
        [SerializeField] private GameObject saveGamePanel;
        [SerializeField] private Transform saveSlotContainer;
        [SerializeField] private GameObject saveSlotPrefab;
        [SerializeField] private Button saveBackButton;

        [Header("Load Game Panel")]
        [SerializeField] private GameObject loadGamePanel;
        [SerializeField] private Transform loadSlotContainer;
        [SerializeField] private Button loadBackButton;

        [Header("Options Panel")]
        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private Button optionsBackButton;

        [Header("Confirmation Dialog")]
        [SerializeField] private GameObject confirmDialog;
        [SerializeField] private TextMeshProUGUI confirmText;
        [SerializeField] private Button confirmYesButton;
        [SerializeField] private Button confirmNoButton;

        [Header("Settings")]
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
        [SerializeField] private bool canPauseDuringDialogue = false;

        private bool isPaused = false;
        private System.Action pendingConfirmAction;

        public bool IsPaused => isPaused;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            SetupButtons();
            HideAllPanels();
        }

        private void Update()
        {
            if (Input.GetKeyDown(pauseKey))
            {
                if (isPaused)
                {
                    // If in sub-menu, go back; otherwise unpause
                    if (IsInSubMenu())
                        ShowPauseMenu();
                    else
                        Resume();
                }
                else
                {
                    Pause();
                }
            }
        }

        private bool IsInSubMenu()
        {
            return (saveGamePanel != null && saveGamePanel.activeSelf) ||
                   (loadGamePanel != null && loadGamePanel.activeSelf) ||
                   (optionsPanel != null && optionsPanel.activeSelf) ||
                   (confirmDialog != null && confirmDialog.activeSelf);
        }

        private void SetupButtons()
        {
            // Pause Menu
            if (resumeButton != null)
                resumeButton.onClick.AddListener(Resume);
            
            if (saveGameButton != null)
                saveGameButton.onClick.AddListener(ShowSavePanel);
            
            if (loadGameButton != null)
                loadGameButton.onClick.AddListener(ShowLoadPanel);
            
            if (optionsButton != null)
                optionsButton.onClick.AddListener(ShowOptionsPanel);
            
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);

            // Save Panel
            if (saveBackButton != null)
                saveBackButton.onClick.AddListener(ShowPauseMenu);

            // Load Panel
            if (loadBackButton != null)
                loadBackButton.onClick.AddListener(ShowPauseMenu);

            // Options Panel
            if (optionsBackButton != null)
                optionsBackButton.onClick.AddListener(ShowPauseMenu);

            // Confirmation
            if (confirmYesButton != null)
                confirmYesButton.onClick.AddListener(OnConfirmYes);
            
            if (confirmNoButton != null)
                confirmNoButton.onClick.AddListener(OnConfirmNo);
        }

        #region Pause Control

        public void Pause()
        {
            // Check if we can pause
            if (!canPauseDuringDialogue)
            {
                if (Dialogue.DialogueManager.Instance != null && 
                    Dialogue.DialogueManager.Instance.IsDialogueActive)
                {
                    return;
                }
            }

            isPaused = true;
            Time.timeScale = 0f;
            ShowPauseMenu();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Disable player input
            if (PlayerController.Instance != null)
                PlayerController.Instance.SetInputEnabled(false);
        }

        public void Resume()
        {
            isPaused = false;
            Time.timeScale = 1f;
            HideAllPanels();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Enable player input
            if (PlayerController.Instance != null)
                PlayerController.Instance.SetInputEnabled(true);
        }

        #endregion

        #region Panel Management

        private void HideAllPanels()
        {
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if (saveGamePanel != null) saveGamePanel.SetActive(false);
            if (loadGamePanel != null) loadGamePanel.SetActive(false);
            if (optionsPanel != null) optionsPanel.SetActive(false);
            if (confirmDialog != null) confirmDialog.SetActive(false);
        }

        private void ShowPauseMenu()
        {
            HideAllPanels();
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        }

        private void ShowSavePanel()
        {
            HideAllPanels();
            if (saveGamePanel != null) saveGamePanel.SetActive(true);
            RefreshSaveSlots(saveSlotContainer, true);
        }

        private void ShowLoadPanel()
        {
            HideAllPanels();
            if (loadGamePanel != null) loadGamePanel.SetActive(true);
            RefreshSaveSlots(loadSlotContainer, false);
        }

        private void ShowOptionsPanel()
        {
            HideAllPanels();
            if (optionsPanel != null) optionsPanel.SetActive(true);
        }

        #endregion

        #region Save/Load Slots

        private void RefreshSaveSlots(Transform container, bool isSaveMode)
        {
            if (container == null || SaveManager.Instance == null) return;

            // Clear existing
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }

            // Create slots
            var slots = SaveManager.Instance.GetAllSlotInfos();
            for (int i = 0; i < slots.Count; i++)
            {
                CreateSlotUI(container, i, slots[i], isSaveMode);
            }
        }

        private void CreateSlotUI(Transform container, int index, SaveSlotInfo info, bool isSaveMode)
        {
            GameObject slotObj;
            
            if (saveSlotPrefab != null)
            {
                slotObj = Instantiate(saveSlotPrefab, container);
            }
            else
            {
                slotObj = new GameObject($"Slot_{index}");
                slotObj.transform.SetParent(container, false);
                
                var rect = slotObj.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(400, 60);
                
                var bg = slotObj.AddComponent<Image>();
                bg.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);
                
                var layout = slotObj.AddComponent<LayoutElement>();
                layout.minHeight = 60;
                layout.flexibleWidth = 1;
            }

            var slotUI = slotObj.GetComponent<MainMenuSaveSlot>();
            if (slotUI == null)
                slotUI = slotObj.AddComponent<MainMenuSaveSlot>();

            if (isSaveMode)
            {
                slotUI.Setup(index, info, OnSlotSave, OnSlotDelete);
            }
            else
            {
                slotUI.Setup(index, info, OnSlotLoad, OnSlotDelete);
            }
        }

        private void OnSlotSave(int slotIndex)
        {
            var info = SaveManager.Instance.GetSlotInfo(slotIndex);
            
            if (!info.isEmpty)
            {
                ShowConfirmation(
                    $"Overwrite save in Slot {slotIndex}?",
                    () => PerformSave(slotIndex)
                );
            }
            else
            {
                PerformSave(slotIndex);
            }
        }

        private void PerformSave(int slotIndex)
        {
            string saveName = slotIndex == 0 ? "Auto Save" : $"Save {slotIndex}";
            SaveManager.Instance.Save(slotIndex, saveName);
            RefreshSaveSlots(saveSlotContainer, true);
        }

        private void OnSlotLoad(int slotIndex)
        {
            if (!SaveManager.Instance.SaveExists(slotIndex))
            {
                Debug.LogWarning($"[PauseMenu] No save in slot {slotIndex}");
                return;
            }

            ShowConfirmation(
                "Load this save? Unsaved progress will be lost.",
                () => {
                    Resume();
                    SaveManager.Instance.Load(slotIndex);
                }
            );
        }

        private void OnSlotDelete(int slotIndex)
        {
            var info = SaveManager.Instance.GetSlotInfo(slotIndex);
            string saveName = info.isEmpty ? $"Slot {slotIndex}" : info.saveName;
            
            ShowConfirmation(
                $"Delete \"{saveName}\"?\nThis cannot be undone.",
                () => {
                    SaveManager.Instance.DeleteSave(slotIndex);
                    // Refresh the current panel
                    if (saveGamePanel != null && saveGamePanel.activeSelf)
                        RefreshSaveSlots(saveSlotContainer, true);
                    else if (loadGamePanel != null && loadGamePanel.activeSelf)
                        RefreshSaveSlots(loadSlotContainer, false);
                }
            );
        }

        #endregion

        #region Main Menu

        private void OnMainMenuClicked()
        {
            ShowConfirmation(
                "Return to main menu?\nUnsaved progress will be lost.",
                GoToMainMenu
            );
        }

        private void GoToMainMenu()
        {
            Time.timeScale = 1f;
            isPaused = false;
            SceneManager.LoadScene(mainMenuSceneName);
        }

        #endregion

        #region Confirmation Dialog

        private void ShowConfirmation(string message, System.Action onConfirm)
        {
            if (confirmDialog == null)
            {
                // No dialog, just execute
                onConfirm?.Invoke();
                return;
            }

            pendingConfirmAction = onConfirm;
            
            if (confirmText != null)
                confirmText.text = message;

            confirmDialog.SetActive(true);
        }

        private void OnConfirmYes()
        {
            if (confirmDialog != null)
                confirmDialog.SetActive(false);
            
            pendingConfirmAction?.Invoke();
            pendingConfirmAction = null;
        }

        private void OnConfirmNo()
        {
            if (confirmDialog != null)
                confirmDialog.SetActive(false);
            
            pendingConfirmAction = null;
        }

        #endregion
    }
}

