using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using WhisperingGate.Core;
using WhisperingGate.Gameplay;
using WhisperingGate.Items;
using WhisperingGate.Environment;

namespace WhisperingGate.SaveSystem
{
    /// <summary>
    /// Central manager for saving and loading game state.
    /// Supports multiple save slots, auto-save, and extensible data.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        [Header("Save Settings")]
        [SerializeField] private int maxSaveSlots = 5;
        [SerializeField] private string saveFilePrefix = "save_";
        [SerializeField] private string saveFileExtension = ".json";
        [SerializeField] private bool useEncryption = false;
        [SerializeField] private bool prettyPrintJson = true;

        [Header("Scene Settings")]
        [Tooltip("Fallback scene to load if saved scene doesn't exist")]
        [SerializeField] private string fallbackGameplayScene = "GameplayScene";

        [Header("Auto-Save")]
        [SerializeField] private bool enableAutoSave = true;
        [SerializeField] private float autoSaveInterval = 300f; // 5 minutes
        [SerializeField] private bool autoSaveOnSceneChange = true;
        [SerializeField] private int autoSaveSlot = 0; // Slot 0 reserved for auto-save

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        // Runtime
        private float playTimeThisSession = 0f;
        private float lastAutoSaveTime = 0f;
        private float totalPlayTime = 0f;
        private bool isLoading = false;

        // Events
        public event Action<int> OnSaveStarted;
        public event Action<int, bool> OnSaveCompleted; // slot, success
        public event Action<int> OnLoadStarted;
        public event Action<int, bool> OnLoadCompleted; // slot, success
        public event Action OnAutoSave;

        /// <summary>
        /// Directory where save files are stored.
        /// </summary>
        public string SaveDirectory => Path.Combine(Application.persistentDataPath, "Saves");

        /// <summary>
        /// Get or create SaveManager instance (lazy initialization).
        /// Call this from Main Menu to ensure SaveManager exists.
        /// </summary>
        public static SaveManager GetOrCreate()
        {
            if (Instance == null)
            {
                GameObject go = new GameObject("SaveManager");
                Instance = go.AddComponent<SaveManager>();
                DontDestroyOnLoad(go);
            }
            return Instance;
        }

        /// <summary>
        /// Whether a load operation is in progress.
        /// </summary>
        public bool IsLoading => isLoading;

        /// <summary>
        /// Total play time across all sessions.
        /// </summary>
        public float TotalPlayTime => totalPlayTime + playTimeThisSession;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Create save directory if it doesn't exist
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
                if (enableDebugLogs) Debug.Log($"[SaveManager] Created save directory: {SaveDirectory}");
            }

            // Load total play time from PlayerPrefs
            totalPlayTime = PlayerPrefs.GetFloat("TotalPlayTime", 0f);
        }

        private void Start()
        {
            if (autoSaveOnSceneChange)
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
        }

        private void OnDestroy()
        {
            if (autoSaveOnSceneChange)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }

            // Save total play time
            PlayerPrefs.SetFloat("TotalPlayTime", totalPlayTime + playTimeThisSession);
            PlayerPrefs.Save();
        }

        private void Update()
        {
            playTimeThisSession += Time.deltaTime;

            // Auto-save check
            if (enableAutoSave && !isLoading)
            {
                if (Time.time - lastAutoSaveTime >= autoSaveInterval)
                {
                    AutoSave();
                    lastAutoSaveTime = Time.time;
                }
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (enableAutoSave && !isLoading && scene.name != "MainMenu")
            {
                // Delay auto-save slightly to let scene initialize
                Invoke(nameof(AutoSave), 1f);
            }
        }

        #region Public API

        /// <summary>
        /// Save game to specified slot.
        /// </summary>
        public bool Save(int slot, string saveName = null)
        {
            if (slot < 0 || slot >= maxSaveSlots)
            {
                Debug.LogError($"[SaveManager] Invalid save slot: {slot}");
                return false;
            }

            OnSaveStarted?.Invoke(slot);

            try
            {
                SaveData data = GatherSaveData(slot, saveName);
                string json = prettyPrintJson 
                    ? JsonUtility.ToJson(data, true) 
                    : JsonUtility.ToJson(data);

                if (useEncryption)
                {
                    json = EncryptString(json);
                }

                string filePath = GetSaveFilePath(slot);
                File.WriteAllText(filePath, json);

                // Update slot info in PlayerPrefs for quick menu access
                SaveSlotInfo slotInfo = new SaveSlotInfo
                {
                    saveId = data.saveId,
                    saveName = data.saveName,
                    saveTimestamp = data.saveTimestamp,
                    playTime = data.playTime,
                    currentScene = data.currentScene,
                    isEmpty = false
                };
                PlayerPrefs.SetString($"SaveSlot_{slot}", JsonUtility.ToJson(slotInfo));
                PlayerPrefs.Save();

                if (enableDebugLogs) Debug.Log($"[SaveManager] Saved to slot {slot}: {filePath}");

                OnSaveCompleted?.Invoke(slot, true);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Save failed: {e.Message}");
                OnSaveCompleted?.Invoke(slot, false);
                return false;
            }
        }

        /// <summary>
        /// Load game from specified slot.
        /// </summary>
        public bool Load(int slot)
        {
            if (slot < 0 || slot >= maxSaveSlots)
            {
                Debug.LogError($"[SaveManager] Invalid load slot: {slot}");
                return false;
            }

            string filePath = GetSaveFilePath(slot);
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[SaveManager] No save file found at slot {slot}");
                return false;
            }

            OnLoadStarted?.Invoke(slot);
            isLoading = true;

            try
            {
                string json = File.ReadAllText(filePath);

                if (useEncryption)
                {
                    json = DecryptString(json);
                }

                SaveData data = JsonUtility.FromJson<SaveData>(json);
                
                // Load scene first if different
                string currentScene = SceneManager.GetActiveScene().name;
                string targetScene = GetValidSceneName(data.currentScene);
                
                if (!string.IsNullOrEmpty(targetScene) && targetScene != currentScene)
                {
                    // Store data for after scene load
                    pendingLoadData = data;
                    pendingLoadSlot = slot;
                    SceneManager.sceneLoaded += OnSceneLoadedForLoad;
                    SceneManager.LoadScene(targetScene);
                    return true;
                }
                else
                {
                    ApplySaveData(data);
                    isLoading = false;
                    OnLoadCompleted?.Invoke(slot, true);
                    if (enableDebugLogs) Debug.Log($"[SaveManager] Loaded from slot {slot}");
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Load failed: {e.Message}");
                isLoading = false;
                OnLoadCompleted?.Invoke(slot, false);
                return false;
            }
        }

        private SaveData pendingLoadData;
        private int pendingLoadSlot;

        private void OnSceneLoadedForLoad(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnSceneLoadedForLoad;
            
            // Delay to let scene initialize
            Invoke(nameof(ApplyPendingLoad), 0.1f);
        }

        private void ApplyPendingLoad()
        {
            if (pendingLoadData != null)
            {
                ApplySaveData(pendingLoadData);
                isLoading = false;
                OnLoadCompleted?.Invoke(pendingLoadSlot, true);
                if (enableDebugLogs) Debug.Log($"[SaveManager] Loaded from slot {pendingLoadSlot} (after scene change)");
                pendingLoadData = null;
            }
        }

        /// <summary>
        /// Trigger auto-save to slot 0.
        /// </summary>
        public void AutoSave()
        {
            if (isLoading) return;
            
            OnAutoSave?.Invoke();
            Save(autoSaveSlot, "Auto Save");
            if (enableDebugLogs) Debug.Log("[SaveManager] Auto-saved");
        }

        /// <summary>
        /// Quick save to last used slot or slot 1.
        /// </summary>
        public void QuickSave()
        {
            int lastSlot = PlayerPrefs.GetInt("LastSaveSlot", 1);
            Save(lastSlot, "Quick Save");
            PlayerPrefs.SetInt("LastSaveSlot", lastSlot);
        }

        /// <summary>
        /// Quick load from last used slot.
        /// </summary>
        public void QuickLoad()
        {
            int lastSlot = PlayerPrefs.GetInt("LastSaveSlot", 1);
            Load(lastSlot);
        }

        /// <summary>
        /// Delete save at specified slot.
        /// </summary>
        public bool DeleteSave(int slot)
        {
            string filePath = GetSaveFilePath(slot);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                PlayerPrefs.DeleteKey($"SaveSlot_{slot}");
                PlayerPrefs.Save();
                if (enableDebugLogs) Debug.Log($"[SaveManager] Deleted save at slot {slot}");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if a save exists at specified slot.
        /// </summary>
        public bool SaveExists(int slot)
        {
            return File.Exists(GetSaveFilePath(slot));
        }

        /// <summary>
        /// Get save slot info for menu display.
        /// </summary>
        public SaveSlotInfo GetSlotInfo(int slot)
        {
            string json = PlayerPrefs.GetString($"SaveSlot_{slot}", "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    return JsonUtility.FromJson<SaveSlotInfo>(json);
                }
                catch
                {
                    return new SaveSlotInfo { isEmpty = true };
                }
            }
            return new SaveSlotInfo { isEmpty = true };
        }

        /// <summary>
        /// Get all save slot infos.
        /// </summary>
        public List<SaveSlotInfo> GetAllSlotInfos()
        {
            var slots = new List<SaveSlotInfo>();
            for (int i = 0; i < maxSaveSlots; i++)
            {
                slots.Add(GetSlotInfo(i));
            }
            return slots;
        }

        /// <summary>
        /// Check if any save exists (for Resume button).
        /// </summary>
        public bool HasAnySave()
        {
            for (int i = 0; i < maxSaveSlots; i++)
            {
                if (SaveExists(i))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get the most recent save slot (for Resume).
        /// By default excludes auto-save slot (slot 0).
        /// Returns -1 if no saves exist.
        /// </summary>
        /// <param name="excludeAutoSave">If true, skips slot 0 (auto-save). Default is true.</param>
        public int GetMostRecentSaveSlot(bool excludeAutoSave = true)
        {
            int mostRecentSlot = -1;
            DateTime mostRecentTime = DateTime.MinValue;
            int startSlot = excludeAutoSave ? 1 : 0; // Skip slot 0 if excluding auto-save

            for (int i = startSlot; i < maxSaveSlots; i++)
            {
                // First check if file actually exists
                if (!SaveExists(i)) continue;

                var info = GetSlotInfo(i);
                
                // If PlayerPrefs info exists and has valid timestamp
                if (!info.isEmpty && info.saveTimestamp > mostRecentTime)
                {
                    mostRecentTime = info.saveTimestamp;
                    mostRecentSlot = i;
                }
                // If file exists but no PlayerPrefs info, use file modification time
                else if (info.isEmpty)
                {
                    string filePath = GetSaveFilePath(i);
                    try
                    {
                        DateTime fileTime = File.GetLastWriteTime(filePath);
                        if (fileTime > mostRecentTime)
                        {
                            mostRecentTime = fileTime;
                            mostRecentSlot = i;
                        }
                    }
                    catch { }
                }
            }

            // Fallback: if no manual save found but excluding auto-save, try auto-save as last resort
            if (mostRecentSlot == -1 && excludeAutoSave && SaveExists(autoSaveSlot))
            {
                mostRecentSlot = autoSaveSlot;
                if (enableDebugLogs) Debug.Log("[SaveManager] No manual saves found, falling back to auto-save");
            }

            // Final fallback: return first existing save
            if (mostRecentSlot == -1)
            {
                for (int i = startSlot; i < maxSaveSlots; i++)
                {
                    if (SaveExists(i))
                    {
                        mostRecentSlot = i;
                        break;
                    }
                }
            }

            return mostRecentSlot;
        }

        /// <summary>
        /// Get the max number of save slots.
        /// </summary>
        public int MaxSaveSlots => maxSaveSlots;

        #endregion

        #region Data Gathering

        private SaveData GatherSaveData(int slot, string saveName)
        {
            SaveData data = new SaveData
            {
                saveId = Guid.NewGuid().ToString(),
                saveName = saveName ?? $"Save {slot}",
                saveTimestamp = DateTime.Now,
                playTime = totalPlayTime + playTimeThisSession,
                currentScene = SceneManager.GetActiveScene().name
            };

            // Player
            GatherPlayerData(data.player);

            // GameState
            GatherGameStateData(data.gameState);

            // Inventory
            GatherInventoryData(data.inventory);

            // Level/Segments
            GatherLevelData(data.level);

            // Puzzles
            GatherPuzzleData(data.puzzles);

            // Environment
            GatherEnvironmentData(data.environment);

            return data;
        }

        private void GatherPlayerData(PlayerSaveData data)
        {
            if (PlayerController.Instance != null)
            {
                var pos = PlayerController.Instance.transform.position;
                data.positionX = pos.x;
                data.positionY = pos.y;
                data.positionZ = pos.z;
                data.rotationY = PlayerController.Instance.transform.eulerAngles.y;
            }

            if (FlashlightController.Instance != null)
            {
                data.hasFlashlight = FlashlightController.Instance.HasFlashlight;
                data.flashlightOn = FlashlightController.Instance.IsOn;
                data.flashlightBattery = FlashlightController.Instance.CurrentBattery;
            }
        }

        private void GatherGameStateData(GameStateSaveData data)
        {
            if (GameState.Instance == null) return;

            // Get all flags and variables from GameState
            // Note: You may need to expose these from GameState
            var flags = GameState.Instance.GetAllFlags();
            var ints = GameState.Instance.GetAllInts();
            var strings = GameState.Instance.GetAllStrings();

            foreach (var flag in flags)
            {
                if (flag.Value)
                    data.trueFlags.Add(flag.Key);
            }

            foreach (var intVar in ints)
            {
                data.intVariables.Add(new GameStateSaveData.IntVariable 
                { 
                    key = intVar.Key, 
                    value = intVar.Value 
                });
            }

            foreach (var strVar in strings)
            {
                data.stringVariables.Add(new GameStateSaveData.StringVariable 
                { 
                    key = strVar.Key, 
                    value = strVar.Value 
                });
            }
        }

        private void GatherInventoryData(InventorySaveData data)
        {
            if (InventoryManager.Instance == null) return;

            var items = InventoryManager.Instance.GetAllItemsForSave();
            foreach (var item in items)
            {
                data.itemIds.Add(item.itemId);
                data.itemCounts.Add(item.count);
            }
        }

        private void GatherLevelData(LevelSaveData data)
        {
            if (LevelManager.Instance == null) return;

            data.currentSegmentId = LevelManager.Instance.CurrentSegmentId;
            // Convert HashSet to List
            var completedSet = LevelManager.Instance.GetCompletedSegments();
            data.completedSegments = new List<string>(completedSet);
        }

        private void GatherPuzzleData(PuzzleSaveData data)
        {
            // Find all puzzle controllers and check their solved state
            var gridPuzzles = FindObjectsOfType<Puzzles.GridPuzzleController>();
            foreach (var puzzle in gridPuzzles)
            {
                if (puzzle.IsSolved && puzzle.Config != null)
                {
                    data.solvedPuzzleIds.Add(puzzle.Config.puzzleId);
                }
            }

            var rotationPuzzles = FindObjectsOfType<Puzzles.RotationPuzzleController>();
            foreach (var puzzle in rotationPuzzles)
            {
                if (puzzle.IsSolved && puzzle.Config != null)
                {
                    data.solvedPuzzleIds.Add(puzzle.Config.puzzleId);
                }
            }
        }

        private void GatherEnvironmentData(EnvironmentSaveData data)
        {
            if (HorrorSkyboxController.Instance != null)
            {
                data.skyboxMood = HorrorSkyboxController.Instance.CurrentMood;
            }
        }

        #endregion

        #region Data Application

        private void ApplySaveData(SaveData data)
        {
            // Restore total play time
            totalPlayTime = data.playTime;
            playTimeThisSession = 0f;

            // Player
            ApplyPlayerData(data.player);

            // GameState
            ApplyGameStateData(data.gameState);

            // Inventory
            ApplyInventoryData(data.inventory);

            // Level/Segments
            ApplyLevelData(data.level);

            // Puzzles
            ApplyPuzzleData(data.puzzles);

            // Environment
            ApplyEnvironmentData(data.environment);
        }

        private void ApplyPlayerData(PlayerSaveData data)
        {
            if (PlayerController.Instance != null)
            {
                // Disable controller temporarily to set position
                var cc = PlayerController.Instance.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;

                PlayerController.Instance.transform.position = new Vector3(
                    data.positionX, 
                    data.positionY, 
                    data.positionZ
                );
                PlayerController.Instance.transform.rotation = Quaternion.Euler(0, data.rotationY, 0);

                if (cc != null) cc.enabled = true;
            }

            if (FlashlightController.Instance != null)
            {
                if (data.hasFlashlight)
                {
                    FlashlightController.Instance.EnableFlashlight(data.flashlightOn);
                    FlashlightController.Instance.AddBattery(data.flashlightBattery - FlashlightController.Instance.CurrentBattery);
                }
            }
        }

        private void ApplyGameStateData(GameStateSaveData data)
        {
            if (GameState.Instance == null) return;

            // Clear and restore
            GameState.Instance.ClearAll();

            foreach (var flag in data.trueFlags)
            {
                GameState.Instance.SetBool(flag, true);
            }

            foreach (var intVar in data.intVariables)
            {
                GameState.Instance.SetInt(intVar.key, intVar.value);
            }

            foreach (var strVar in data.stringVariables)
            {
                GameState.Instance.SetString(strVar.key, strVar.value);
            }
        }

        private void ApplyInventoryData(InventorySaveData data)
        {
            if (InventoryManager.Instance == null) return;

            InventoryManager.Instance.ClearInventory();

            for (int i = 0; i < data.itemIds.Count; i++)
            {
                string itemId = data.itemIds[i];
                int count = i < data.itemCounts.Count ? data.itemCounts[i] : 1;
                
                for (int j = 0; j < count; j++)
                {
                    InventoryManager.Instance.AddItemById(itemId);
                }
            }
        }

        private void ApplyLevelData(LevelSaveData data)
        {
            if (LevelManager.Instance == null) return;

            LevelManager.Instance.RestoreProgress(data.currentSegmentId, data.completedSegments);
        }

        private void ApplyPuzzleData(PuzzleSaveData data)
        {
            // Mark puzzles as solved based on saved data
            var gridPuzzles = FindObjectsOfType<Puzzles.GridPuzzleController>();
            foreach (var puzzle in gridPuzzles)
            {
                if (puzzle.Config != null && data.solvedPuzzleIds.Contains(puzzle.Config.puzzleId))
                {
                    puzzle.SetSolvedState(true);
                }
            }

            var rotationPuzzles = FindObjectsOfType<Puzzles.RotationPuzzleController>();
            foreach (var puzzle in rotationPuzzles)
            {
                if (puzzle.Config != null && data.solvedPuzzleIds.Contains(puzzle.Config.puzzleId))
                {
                    puzzle.SetSolvedState(true);
                }
            }
        }

        private void ApplyEnvironmentData(EnvironmentSaveData data)
        {
            if (HorrorSkyboxController.Instance != null)
            {
                HorrorSkyboxController.Instance.SetMood(data.skyboxMood);
            }
        }

        #endregion

        #region Utilities

        private string GetSaveFilePath(int slot)
        {
            return Path.Combine(SaveDirectory, $"{saveFilePrefix}{slot}{saveFileExtension}");
        }

        /// <summary>
        /// Validate scene name and return fallback if invalid.
        /// </summary>
        private string GetValidSceneName(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
                return fallbackGameplayScene;

            // Check if scene is in build settings
            if (IsSceneInBuildSettings(sceneName))
                return sceneName;

            // Scene not found, use fallback
            Debug.LogWarning($"[SaveManager] Scene '{sceneName}' not found in build settings. Using fallback: {fallbackGameplayScene}");
            return fallbackGameplayScene;
        }

        /// <summary>
        /// Check if a scene exists in build settings.
        /// </summary>
        private bool IsSceneInBuildSettings(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                if (sceneNameFromPath == sceneName)
                    return true;
            }
            return false;
        }

        // Simple XOR encryption (for basic obfuscation, not security)
        private string EncryptString(string text)
        {
            string key = "WhisperingGate2025";
            char[] result = new char[text.Length];
            for (int i = 0; i < text.Length; i++)
            {
                result[i] = (char)(text[i] ^ key[i % key.Length]);
            }
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(new string(result)));
        }

        private string DecryptString(string encrypted)
        {
            string key = "WhisperingGate2025";
            byte[] bytes = Convert.FromBase64String(encrypted);
            string text = System.Text.Encoding.UTF8.GetString(bytes);
            char[] result = new char[text.Length];
            for (int i = 0; i < text.Length; i++)
            {
                result[i] = (char)(text[i] ^ key[i % key.Length]);
            }
            return new string(result);
        }

        #endregion

        #region Input Shortcuts

        [Header("Keyboard Shortcuts")]
        [SerializeField] private KeyCode quickSaveKey = KeyCode.F5;
        [SerializeField] private KeyCode quickLoadKey = KeyCode.F9;

        private void LateUpdate()
        {
            if (Input.GetKeyDown(quickSaveKey))
            {
                QuickSave();
            }
            
            if (Input.GetKeyDown(quickLoadKey))
            {
                QuickLoad();
            }
        }

        #endregion
    }
}

