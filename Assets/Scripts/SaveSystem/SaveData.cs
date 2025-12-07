using System;
using System.Collections.Generic;
using UnityEngine;

namespace WhisperingGate.SaveSystem
{
    /// <summary>
    /// Container for all saveable game data.
    /// Serialized to JSON for saving/loading.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        // Metadata
        public string saveId;
        public string saveName;
        public DateTime saveTimestamp;
        public float playTime; // Total play time in seconds
        public string currentScene;
        public string screenshotBase64; // Optional thumbnail

        // Player
        public PlayerSaveData player = new PlayerSaveData();

        // Game State (flags & variables)
        public GameStateSaveData gameState = new GameStateSaveData();

        // Inventory
        public InventorySaveData inventory = new InventorySaveData();

        // Level/Segment Progress
        public LevelSaveData level = new LevelSaveData();

        // Puzzles
        public PuzzleSaveData puzzles = new PuzzleSaveData();

        // Environment
        public EnvironmentSaveData environment = new EnvironmentSaveData();

        // Custom data from other systems (extensible)
        public Dictionary<string, string> customData = new Dictionary<string, string>();
    }

    [Serializable]
    public class PlayerSaveData
    {
        public float positionX;
        public float positionY;
        public float positionZ;
        public float rotationY;
        public float cameraPitch;
        public bool hasFlashlight;
        public bool flashlightOn;
        public float flashlightBattery;
    }

    [Serializable]
    public class GameStateSaveData
    {
        public List<string> trueFlags = new List<string>();
        public List<IntVariable> intVariables = new List<IntVariable>();
        public List<StringVariable> stringVariables = new List<StringVariable>();

        [Serializable]
        public class IntVariable
        {
            public string key;
            public int value;
        }

        [Serializable]
        public class StringVariable
        {
            public string key;
            public string value;
        }
    }

    [Serializable]
    public class InventorySaveData
    {
        public List<string> itemIds = new List<string>();
        public List<int> itemCounts = new List<int>();
        public int selectedSlot;
    }

    [Serializable]
    public class LevelSaveData
    {
        public string currentSegmentId;
        public List<string> completedSegments = new List<string>();
        public List<string> unlockedCheckpoints = new List<string>();
    }

    [Serializable]
    public class PuzzleSaveData
    {
        public List<string> solvedPuzzleIds = new List<string>();
        public Dictionary<string, int> puzzleProgress = new Dictionary<string, int>();
    }

    [Serializable]
    public class EnvironmentSaveData
    {
        public float skyboxMood;
        public float timeOfDay;
    }

    /// <summary>
    /// Metadata for save slot display (lightweight, loaded for menu).
    /// </summary>
    [Serializable]
    public class SaveSlotInfo
    {
        public string saveId;
        public string saveName;
        public DateTime saveTimestamp;
        public float playTime;
        public string currentScene;
        public bool isEmpty = true;
    }
}

