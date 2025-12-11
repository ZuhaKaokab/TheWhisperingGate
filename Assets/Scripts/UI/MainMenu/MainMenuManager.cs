using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using WhisperingGate.SaveSystem;

namespace WhisperingGate.UI
{
    /// <summary>
    /// Main Menu controller handling all menu navigation and actions.
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Scene Settings")]
        [SerializeField] private string gameplaySceneName = "TestScene";
        [SerializeField] private string newGameStartScene = "TestScene";

        [Header("Main Menu Panel")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button loadGameButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button exitButton;

        [Header("Load Game Panel")]
        [SerializeField] private GameObject loadGamePanel;
        [SerializeField] private Transform saveSlotContainer;
        [SerializeField] private GameObject saveSlotPrefab;
        [SerializeField] private Button loadBackButton;

        [Header("Options Panel")]
        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private Button optionsBackButton;

        [Header("Confirmation Dialog")]
        [SerializeField] private GameObject confirmDialog;
        [SerializeField] private TextMeshProUGUI confirmText;
        [SerializeField] private Button confirmYesButton;
        [SerializeField] private Button confirmNoButton;

        [Header("New Game Warning")]
        [SerializeField] private bool warnIfSaveExists = true;

        [Header("Visual Settings")]
        [SerializeField] private Color disabledButtonColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        // --- Added for Instructions Panel ---
        [Header("Instructions Panel")]
        [SerializeField] private GameObject instructionsPanel;
        [SerializeField] private Button instructionsButton;
        [SerializeField] private Button instructionsCloseButton;

        private System.Action pendingConfirmAction;

        private void Start()
        {
            // Ensure SaveManager exists
            SaveManager.GetOrCreate();

            // Setup button listeners
            SetupButtons();

            // Show main menu
            ShowMainMenu();

            // Update Resume button state
            UpdateResumeButton();

            // Ensure cursor is visible
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Ensure time is running
            Time.timeScale = 1f;
        }

        private void SetupButtons()
        {
            // Main Menu
            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumeClicked);

            if (newGameButton != null)
                newGameButton.onClick.AddListener(OnNewGameClicked);

            if (loadGameButton != null)
                loadGameButton.onClick.AddListener(OnLoadGameClicked);

            if (optionsButton != null)
                optionsButton.onClick.AddListener(OnOptionsClicked);

            if (exitButton != null)
                exitButton.onClick.AddListener(OnExitClicked);

            // Instructions Button
            if (instructionsButton != null)
                instructionsButton.onClick.AddListener(OnInstructionsClicked);

            // Load Game Panel
            if (loadBackButton != null)
                loadBackButton.onClick.AddListener(ShowMainMenu);

            // Options Panel
            if (optionsBackButton != null)
                optionsBackButton.onClick.AddListener(ShowMainMenu);

            // Confirmation Dialog
            if (confirmYesButton != null)
                confirmYesButton.onClick.AddListener(OnConfirmYes);

            if (confirmNoButton != null)
                confirmNoButton.onClick.AddListener(OnConfirmNo);

            // Instructions Panel Close Button
            if (instructionsCloseButton != null)
                instructionsCloseButton.onClick.AddListener(OnInstructionsCloseClicked);
        }

        private void UpdateResumeButton()
        {
            if (resumeButton == null) return;

            bool hasSave = SaveManager.Instance != null && SaveManager.Instance.HasAnySave();

            resumeButton.interactable = hasSave;

            // Visual feedback for disabled state
            var colors = resumeButton.colors;
            colors.disabledColor = disabledButtonColor;
            resumeButton.colors = colors;

            // Optional: change text color
            var text = resumeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.color = hasSave ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            }
        }

        #region Panel Management
        private void ShowMainMenu()
        {
            SetActivePanel(mainMenuPanel);
            UpdateResumeButton();
        }

        private void ShowLoadGamePanel()
        {
            SetActivePanel(loadGamePanel);
            RefreshSaveSlots();
        }

        private void ShowOptionsPanel()
        {
            SetActivePanel(optionsPanel);
        }

        private void SetActivePanel(GameObject panel)
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(panel == mainMenuPanel);
            if (loadGamePanel != null) loadGamePanel.SetActive(panel == loadGamePanel);
            if (optionsPanel != null) optionsPanel.SetActive(panel == optionsPanel);
            if (confirmDialog != null) confirmDialog.SetActive(false);

            // Also ensure instructions panel is hidden when switching main panels
            if (instructionsPanel != null && panel != instructionsPanel)
            {
                instructionsPanel.SetActive(false);
            }
        }
        #endregion

        #region Button Handlers
        private void OnResumeClicked()
        {
            if (SaveManager.Instance == null)
            {
                Debug.LogWarning("[MainMenu] Resume failed: SaveManager.Instance is null");
                return;
            }

            int mostRecent = SaveManager.Instance.GetMostRecentSaveSlot();
            Debug.Log($"[MainMenu] Resume clicked - Most recent slot: {mostRecent}");

            if (mostRecent >= 0)
            {
                LoadGame(mostRecent);
            }
            else
            {
                Debug.LogWarning("[MainMenu] Resume failed: No save slot found");
            }
        }

        private void OnNewGameClicked()
        {
            if (warnIfSaveExists && SaveManager.Instance != null && SaveManager.Instance.HasAnySave())
            {
                ShowConfirmation(
                    "Starting a new game will not delete your saves.\nAre you sure you want to start fresh?",
                    StartNewGame
                );
            }
            else
            {
                StartNewGame();
            }
        }

        private void OnLoadGameClicked()
        {
            ShowLoadGamePanel();
        }

        private void OnOptionsClicked()
        {
            ShowOptionsPanel();
        }

        private void OnExitClicked()
        {
            ShowConfirmation("Are you sure you want to exit?", ExitGame);
        }
        #endregion

        #region Game Actions
        private void StartNewGame()
        {
            Debug.Log("[MainMenu] Starting new game...");

            // Clear any existing game state
            if (Core.GameState.Instance != null)
            {
                Core.GameState.Instance.ClearAll();
            }

            // Load the starting scene
            SceneManager.LoadScene(newGameStartScene);
        }

        private void LoadGame(int slot)
        {
            Debug.Log($"[MainMenu] Loading game from slot {slot}...");

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.Load(slot);
            }
        }

        private void ExitGame()
        {
            Debug.Log("[MainMenu] Exiting game...");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        #endregion

        #region Save Slots
        private void RefreshSaveSlots()
        {
            if (saveSlotContainer == null || SaveManager.Instance == null) return;

            // Clear existing slots
            foreach (Transform child in saveSlotContainer)
            {
                Destroy(child.gameObject);
            }

            // Create slot UIs
            var slots = SaveManager.Instance.GetAllSlotInfos();
            for (int i = 0; i < slots.Count; i++)
            {
                CreateSaveSlotUI(i, slots[i]);
            }
        }

        private void CreateSaveSlotUI(int slotIndex, SaveSlotInfo info)
        {
            GameObject slotObj;

            if (saveSlotPrefab != null)
            {
                slotObj = Instantiate(saveSlotPrefab, saveSlotContainer);
            }
            else
            {
                slotObj = CreateDefaultSlotUI();
                slotObj.transform.SetParent(saveSlotContainer, false);
            }

            var slotUI = slotObj.GetComponent<MainMenuSaveSlot>();
            if (slotUI == null)
                slotUI = slotObj.AddComponent<MainMenuSaveSlot>();

            slotUI.Setup(slotIndex, info, OnSlotLoad, OnSlotDelete);
        }

        private GameObject CreateDefaultSlotUI()
        {
            // Create a simple default slot
            GameObject slot = new GameObject("SaveSlot");

            var rect = slot.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400, 80);

            var layout = slot.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10;
            layout.padding = new RectOffset(15, 15, 10, 10);
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandWidth = false;

            var bg = slot.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);

            var layoutElement = slot.AddComponent<LayoutElement>();
            layoutElement.minHeight = 80;
            layoutElement.preferredHeight = 80;
            layoutElement.flexibleWidth = 1;

            return slot;
        }

        private void OnSlotLoad(int slotIndex)
        {
            if (!SaveManager.Instance.SaveExists(slotIndex))
            {
                Debug.LogWarning($"[MainMenu] No save in slot {slotIndex}");
                return;
            }

            LoadGame(slotIndex);
        }

        private void OnSlotDelete(int slotIndex)
        {
            var info = SaveManager.Instance.GetSlotInfo(slotIndex);
            string saveName = info.isEmpty ? $"Slot {slotIndex + 1}" : info.saveName;

            ShowConfirmation(
                $"Delete save \"{saveName}\"?\nThis cannot be undone.",
                () =>
                {
                    SaveManager.Instance.DeleteSave(slotIndex);
                    RefreshSaveSlots();
                    UpdateResumeButton();
                }
            );
        }
        #endregion

        #region Confirmation Dialog
        private void ShowConfirmation(string message, System.Action onConfirm)
        {
            if (confirmDialog == null) return;

            pendingConfirmAction = onConfirm;

            if (confirmText != null)
                confirmText.text = message;

            confirmDialog.SetActive(true);
        }

        private void OnConfirmYes()
        {
            confirmDialog?.SetActive(false);
            pendingConfirmAction?.Invoke();
            pendingConfirmAction = null;
        }

        private void OnConfirmNo()
        {
            confirmDialog?.SetActive(false);
            pendingConfirmAction = null;
        }
        #endregion

        #region Instructions Panel Handlers
        private void OnInstructionsClicked()
        {
            if (instructionsPanel != null)
            {
                instructionsPanel.SetActive(true);
            }
        }

        private void OnInstructionsCloseClicked()
        {
            if (instructionsPanel != null)
            {
                instructionsPanel.SetActive(false);
            }
        }
        #endregion

        #region Keyboard Navigation
        private void Update()
        {
            // ESC to go back
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (confirmDialog != null && confirmDialog.activeSelf)
                {
                    OnConfirmNo();
                }
                else if (loadGamePanel != null && loadGamePanel.activeSelf)
                {
                    ShowMainMenu();
                }
                else if (optionsPanel != null && optionsPanel.activeSelf)
                {
                    ShowMainMenu();
                }
                else if (instructionsPanel != null && instructionsPanel.activeSelf)
                {
                    // If instructions panel is open, close it on ESC
                    OnInstructionsCloseClicked();
                }
            }
        }
        #endregion
    }
}
