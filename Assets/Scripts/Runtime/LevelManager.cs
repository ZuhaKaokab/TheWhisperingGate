using UnityEngine;
using System;
using System.Collections.Generic;
using WhisperingGate.Core;

namespace WhisperingGate.Gameplay
{
    /// <summary>
    /// Central manager for level/scene progression, tracking completed segments,
    /// managing checkpoints, and coordinating scene transitions.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Level Settings")]
        [SerializeField] private string currentLevelId = "prologue_jungle";
        [SerializeField] private bool debugMode = false;

        // Events
        public event Action<string> OnLevelChanged;
        public event Action<string> OnSegmentCompleted;
        public event Action<string> OnCheckpointReached;
        public event Action OnLevelComplete;

        // Internal state
        private readonly HashSet<string> completedSegments = new();
        private readonly HashSet<string> activatedCheckpoints = new();
        private string currentCheckpointId = string.Empty;

        public string CurrentLevelId => currentLevelId;
        public string CurrentCheckpointId => currentCheckpointId;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (debugMode)
            {
                Debug.Log($"[LevelManager] Initialized for level: {currentLevelId}");
            }
        }

        #region Segment Tracking

        /// <summary>
        /// Marks a dialogue segment as completed. Used to track progression through the narrative.
        /// </summary>
        public void CompleteSegment(string segmentId)
        {
            if (string.IsNullOrWhiteSpace(segmentId))
            {
                Debug.LogWarning("[LevelManager] Attempted to complete segment with empty ID");
                return;
            }

            if (completedSegments.Contains(segmentId))
            {
                if (debugMode)
                    Debug.Log($"[LevelManager] Segment '{segmentId}' already completed");
                return;
            }

            completedSegments.Add(segmentId);
            OnSegmentCompleted?.Invoke(segmentId);

            if (debugMode)
                Debug.Log($"[LevelManager] Segment completed: {segmentId}");

            // Optionally set a GameState flag
            if (GameState.Instance != null)
            {
                GameState.Instance.SetBool($"segment_{segmentId}_completed", true);
            }
        }

        /// <summary>
        /// Checks if a dialogue segment has been completed.
        /// </summary>
        public bool IsSegmentCompleted(string segmentId)
        {
            if (string.IsNullOrWhiteSpace(segmentId)) return false;
            return completedSegments.Contains(segmentId);
        }

        /// <summary>
        /// Gets all completed segments for the current level.
        /// </summary>
        public HashSet<string> GetCompletedSegments()
        {
            return new HashSet<string>(completedSegments);
        }

        /// <summary>
        /// Checks if all required segments are completed (for progression gates).
        /// </summary>
        public bool AreSegmentsCompleted(params string[] segmentIds)
        {
            if (segmentIds == null || segmentIds.Length == 0) return true;

            foreach (var segmentId in segmentIds)
            {
                if (!IsSegmentCompleted(segmentId))
                    return false;
            }

            return true;
        }

        #endregion

        #region Checkpoint System

        /// <summary>
        /// Sets a checkpoint at the current location. This saves the game state.
        /// </summary>
        public void SetCheckpoint(string checkpointId)
        {
            if (string.IsNullOrWhiteSpace(checkpointId))
            {
                Debug.LogWarning("[LevelManager] Attempted to set checkpoint with empty ID");
                return;
            }

            currentCheckpointId = checkpointId;
            activatedCheckpoints.Add(checkpointId);
            OnCheckpointReached?.Invoke(checkpointId);

            if (debugMode)
                Debug.Log($"[LevelManager] Checkpoint reached: {checkpointId}");

            // Save game state
            SaveCheckpoint(checkpointId);
        }

        /// <summary>
        /// Checks if a checkpoint has been activated.
        /// </summary>
        public bool IsCheckpointActivated(string checkpointId)
        {
            return !string.IsNullOrWhiteSpace(checkpointId) && activatedCheckpoints.Contains(checkpointId);
        }

        /// <summary>
        /// Loads a checkpoint, restoring game state to that point.
        /// </summary>
        public void LoadCheckpoint(string checkpointId)
        {
            if (string.IsNullOrWhiteSpace(checkpointId))
            {
                Debug.LogWarning("[LevelManager] Attempted to load checkpoint with empty ID");
                return;
            }

            if (!IsCheckpointActivated(checkpointId))
            {
                Debug.LogWarning($"[LevelManager] Checkpoint '{checkpointId}' has not been activated yet");
                return;
            }

            currentCheckpointId = checkpointId;
            RestoreCheckpoint(checkpointId);

            if (debugMode)
                Debug.Log($"[LevelManager] Checkpoint loaded: {checkpointId}");
        }

        private void SaveCheckpoint(string checkpointId)
        {
            // Save to PlayerPrefs (can be extended to use a proper save system)
            PlayerPrefs.SetString($"checkpoint_{checkpointId}_level", currentLevelId);
            PlayerPrefs.SetString($"checkpoint_{checkpointId}_segments", string.Join(",", completedSegments));
            PlayerPrefs.SetString($"checkpoint_{checkpointId}_id", checkpointId);
            PlayerPrefs.Save();
        }

        private void RestoreCheckpoint(string checkpointId)
        {
            // Restore from PlayerPrefs
            string savedLevel = PlayerPrefs.GetString($"checkpoint_{checkpointId}_level", currentLevelId);
            string savedSegments = PlayerPrefs.GetString($"checkpoint_{checkpointId}_segments", "");

            if (!string.IsNullOrEmpty(savedLevel))
            {
                currentLevelId = savedLevel;
                OnLevelChanged?.Invoke(currentLevelId);
            }

            if (!string.IsNullOrEmpty(savedSegments))
            {
                completedSegments.Clear();
                string[] segments = savedSegments.Split(',');
                foreach (var segment in segments)
                {
                    if (!string.IsNullOrWhiteSpace(segment))
                        completedSegments.Add(segment.Trim());
                }
            }
        }

        #endregion

        #region Level Management

        /// <summary>
        /// Changes to a new level/scene.
        /// </summary>
        public void ChangeLevel(string newLevelId)
        {
            if (string.IsNullOrWhiteSpace(newLevelId))
            {
                Debug.LogWarning("[LevelManager] Attempted to change to level with empty ID");
                return;
            }

            if (currentLevelId == newLevelId)
            {
                if (debugMode)
                    Debug.Log($"[LevelManager] Already in level: {newLevelId}");
                return;
            }

            string previousLevel = currentLevelId;
            currentLevelId = newLevelId;

            // Optionally clear segments when changing levels (or keep them persistent)
            // completedSegments.Clear();

            OnLevelChanged?.Invoke(newLevelId);

            if (debugMode)
                Debug.Log($"[LevelManager] Level changed: {previousLevel} -> {newLevelId}");
        }

        /// <summary>
        /// Marks the current level as complete.
        /// </summary>
        public void CompleteLevel()
        {
            OnLevelComplete?.Invoke();

            if (GameState.Instance != null)
            {
                GameState.Instance.SetBool($"level_{currentLevelId}_completed", true);
            }

            if (debugMode)
                Debug.Log($"[LevelManager] Level completed: {currentLevelId}");
        }

        #endregion

        #region Utility

        /// <summary>
        /// Resets all progression data for the current level (useful for testing).
        /// </summary>
        public void ResetLevelProgress()
        {
            completedSegments.Clear();
            activatedCheckpoints.Clear();
            currentCheckpointId = string.Empty;

            if (debugMode)
                Debug.Log($"[LevelManager] Progress reset for level: {currentLevelId}");
        }

        /// <summary>
        /// Gets debug information about current level state.
        /// </summary>
        public string GetDebugInfo()
        {
            return $"Level: {currentLevelId}\n" +
                   $"Checkpoint: {currentCheckpointId}\n" +
                   $"Completed Segments: {completedSegments.Count}\n" +
                   $"Activated Checkpoints: {activatedCheckpoints.Count}";
        }

        #endregion
    }
}


