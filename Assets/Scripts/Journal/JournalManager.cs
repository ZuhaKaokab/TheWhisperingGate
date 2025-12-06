using UnityEngine;
using System;
using System.Collections.Generic;
using WhisperingGate.Core;
using WhisperingGate.Gameplay;

namespace WhisperingGate.Journal
{
    /// <summary>
    /// Manages the journal system - tracking unlocked pages, handling open/close,
    /// and integrating with GameState.
    /// </summary>
    public class JournalManager : MonoBehaviour
    {
        public static JournalManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private JournalConfig config;

        [Header("UI Reference")]
        [SerializeField] private JournalUI journalUI;

        [Header("Settings")]
        [Tooltip("Has the player picked up the journal?")]
        [SerializeField] private bool hasJournal = false;
        
        [Tooltip("Key to open journal (when in inventory)")]
        [SerializeField] private KeyCode openKey = KeyCode.J;
        
        [Tooltip("Enable debug logging")]
        [SerializeField] private bool enableDebugLogs = true;

        // Runtime state
        private HashSet<string> unlockedPageIds = new HashSet<string>();
        private HashSet<string> viewedPageIds = new HashSet<string>();
        private bool isJournalOpen = false;

        // Events
        public event Action OnJournalOpened;
        public event Action OnJournalClosed;
        public event Action<JournalPage> OnPageUnlocked;
        public event Action<JournalPage> OnPageViewed;
        public event Action OnJournalPickedUp;

        /// <summary>
        /// Does the player have the journal?
        /// </summary>
        public bool HasJournal => hasJournal;

        /// <summary>
        /// Is the journal currently open?
        /// </summary>
        public bool IsJournalOpen => isJournalOpen;

        /// <summary>
        /// Current journal configuration.
        /// </summary>
        public JournalConfig Config => config;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Initialize default unlocked pages
            InitializeDefaultPages();
        }

        private void Start()
        {
            // Find UI if not assigned
            if (journalUI == null)
            {
                journalUI = FindObjectOfType<JournalUI>(true);
            }

            // Check for pages that should auto-unlock based on current GameState
            CheckAutoUnlocks();
            
            // Subscribe to GameState changes (bool changes are most common for unlock conditions)
            if (GameState.Instance != null)
            {
                GameState.Instance.OnBoolChanged += OnGameStateBoolChanged;
                GameState.Instance.OnIntChanged += OnGameStateIntChanged;
            }
        }

        private void Update()
        {
            // Open journal with key (only if player has it and it's not already open)
            if (hasJournal && !isJournalOpen && Input.GetKeyDown(openKey))
            {
                OpenJournal();
            }
        }

        private void OnDestroy()
        {
            if (GameState.Instance != null)
            {
                GameState.Instance.OnBoolChanged -= OnGameStateBoolChanged;
                GameState.Instance.OnIntChanged -= OnGameStateIntChanged;
            }
        }

        /// <summary>
        /// Initialize pages that are unlocked by default.
        /// </summary>
        private void InitializeDefaultPages()
        {
            if (config == null) return;

            foreach (var page in config.allPages)
            {
                if (page != null && page.unlockedByDefault)
                {
                    unlockedPageIds.Add(page.pageId);
                    if (enableDebugLogs) Debug.Log($"[Journal] Default page unlocked: {page.pageId}");
                }
            }
        }

        /// <summary>
        /// Check all pages for auto-unlock conditions.
        /// </summary>
        private void CheckAutoUnlocks()
        {
            if (config == null || GameState.Instance == null) return;

            foreach (var page in config.allPages)
            {
                if (page == null || unlockedPageIds.Contains(page.pageId)) continue;
                
                if (!string.IsNullOrWhiteSpace(page.unlockCondition))
                {
                    if (GameState.Instance.EvaluateCondition(page.unlockCondition))
                    {
                        UnlockPage(page.pageId, silent: true);
                    }
                }
            }
        }

        /// <summary>
        /// Called when a GameState bool changes - check for new unlocks.
        /// </summary>
        private void OnGameStateBoolChanged(string key, bool value)
        {
            CheckAutoUnlocks();
        }

        /// <summary>
        /// Called when a GameState int changes - check for new unlocks.
        /// </summary>
        private void OnGameStateIntChanged(string key, int value)
        {
            CheckAutoUnlocks();
        }

        /// <summary>
        /// Player picks up the physical journal object.
        /// </summary>
        public void PickUpJournal()
        {
            if (hasJournal) return;

            hasJournal = true;
            
            // Set GameState flag
            if (GameState.Instance != null)
            {
                GameState.Instance.SetBool("has_journal", true);
                GameState.Instance.SetBool("journal_found", true);
            }

            if (enableDebugLogs) Debug.Log("[Journal] Journal picked up!");
            OnJournalPickedUp?.Invoke();
        }

        /// <summary>
        /// Open the journal UI.
        /// </summary>
        public void OpenJournal(string gotoPageId = null)
        {
            if (!hasJournal)
            {
                if (enableDebugLogs) Debug.Log("[Journal] Cannot open - player doesn't have journal");
                return;
            }

            if (isJournalOpen) return;

            isJournalOpen = true;

            // Pause player
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.SetInputEnabled(false);
            }

            // Show cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Open UI
            if (journalUI != null)
            {
                journalUI.Open(gotoPageId);
            }

            // Play sound
            PlaySound(config?.openSound);

            if (enableDebugLogs) Debug.Log("[Journal] Opened");
            OnJournalOpened?.Invoke();
        }

        /// <summary>
        /// Close the journal UI.
        /// </summary>
        public void CloseJournal()
        {
            if (!isJournalOpen) return;

            isJournalOpen = false;

            // Close UI
            if (journalUI != null)
            {
                journalUI.Close();
            }

            // Resume player
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.SetInputEnabled(true);
            }

            // Lock cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Play sound
            PlaySound(config?.closeSound);

            if (enableDebugLogs) Debug.Log("[Journal] Closed");
            OnJournalClosed?.Invoke();
        }

        /// <summary>
        /// Unlock a page by ID.
        /// </summary>
        public void UnlockPage(string pageId, bool silent = false)
        {
            if (string.IsNullOrWhiteSpace(pageId)) return;
            if (unlockedPageIds.Contains(pageId)) return;

            JournalPage page = config?.GetPageById(pageId);
            if (page == null)
            {
                Debug.LogWarning($"[Journal] Page not found: {pageId}");
                return;
            }

            unlockedPageIds.Add(pageId);

            // Set unlock flag if specified
            if (!string.IsNullOrWhiteSpace(page.unlockFlag) && GameState.Instance != null)
            {
                GameState.Instance.SetBool(page.unlockFlag, true);
            }

            if (!silent)
            {
                PlaySound(config?.unlockSound);
                // TODO: Show "New journal entry" notification
            }

            if (enableDebugLogs) Debug.Log($"[Journal] Page unlocked: {pageId}");
            OnPageUnlocked?.Invoke(page);
        }

        /// <summary>
        /// Mark a page as viewed.
        /// </summary>
        public void MarkPageViewed(string pageId)
        {
            if (viewedPageIds.Contains(pageId)) return;

            viewedPageIds.Add(pageId);

            JournalPage page = config?.GetPageById(pageId);
            if (page != null)
            {
                // Play first view sound
                if (page.firstViewSound != null)
                {
                    PlaySound(page.firstViewSound);
                }

                OnPageViewed?.Invoke(page);
            }
        }

        /// <summary>
        /// Check if a page is unlocked.
        /// </summary>
        public bool IsPageUnlocked(string pageId)
        {
            return unlockedPageIds.Contains(pageId);
        }

        /// <summary>
        /// Check if a page has been viewed.
        /// </summary>
        public bool IsPageViewed(string pageId)
        {
            return viewedPageIds.Contains(pageId);
        }

        /// <summary>
        /// Get all unlocked pages, sorted.
        /// </summary>
        public List<JournalPage> GetUnlockedPages()
        {
            List<JournalPage> pages = new List<JournalPage>();
            
            if (config == null) return pages;

            foreach (var page in config.GetSortedPages())
            {
                if (page != null && unlockedPageIds.Contains(page.pageId))
                {
                    pages.Add(page);
                }
            }

            return pages;
        }

        /// <summary>
        /// Get count of unviewed (new) pages.
        /// </summary>
        public int GetNewPageCount()
        {
            int count = 0;
            foreach (var pageId in unlockedPageIds)
            {
                if (!viewedPageIds.Contains(pageId))
                    count++;
            }
            return count;
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, UnityEngine.Camera.main.transform.position);
            }
        }

        /// <summary>
        /// Execute a journal command (called from command system).
        /// Format: journal:action:param
        /// Actions: unlock, open, goto, pickup
        /// </summary>
        public void ExecuteCommand(string action, string param)
        {
            switch (action.ToLower())
            {
                case "unlock":
                    UnlockPage(param);
                    break;
                    
                case "open":
                    OpenJournal();
                    break;
                    
                case "goto":
                    OpenJournal(param);
                    break;
                    
                case "pickup":
                    PickUpJournal();
                    break;
                    
                default:
                    Debug.LogWarning($"[Journal] Unknown command action: {action}");
                    break;
            }
        }
    }
}

