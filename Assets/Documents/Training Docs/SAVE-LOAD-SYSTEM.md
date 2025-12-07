# Save/Load System Documentation
## The Whispering Gate

**Version:** 1.0  
**Last Updated:** December 2025

---

## Overview

The Save/Load system provides complete game state persistence with multiple save slots, auto-save functionality, and seamless scene transitions.

---

## Architecture

```
SaveManager (Singleton - DontDestroyOnLoad)
├── Persists across ALL scenes
├── Handles: Save, Load, Delete, Auto-save
├── Stores: JSON files in persistent data path
└── Metadata: PlayerPrefs for quick slot info

SaveData (Serializable Container)
├── Player position, rotation, flashlight
├── GameState (flags, variables)
├── Inventory items
├── Level/segment progress
├── Puzzle states
└── Environment (skybox mood)
```

---

## Files

| File | Purpose |
|------|---------|
| `SaveSystem/SaveData.cs` | Data structures for all saveable data |
| `SaveSystem/SaveManager.cs` | Core manager - save/load/auto-save |
| `SaveSystem/SaveLoadUI.cs` | Simple UI for save slots |
| `UI/MainMenu/MainMenuManager.cs` | Main menu with Resume/New Game/Load |
| `UI/MainMenu/MainMenuSaveSlot.cs` | Individual slot UI component |
| `UI/MainMenu/PauseMenuManager.cs` | In-game pause menu with save/load |

---

## What Gets Saved

### Player Data
```csharp
public class PlayerSaveData
{
    public float positionX, positionY, positionZ;
    public float rotationY;
    public float cameraPitch;
    public bool hasFlashlight;
    public bool flashlightOn;
    public float flashlightBattery;
}
```

### Game State
```csharp
public class GameStateSaveData
{
    public List<string> trueFlags;           // Boolean flags that are true
    public List<IntVariable> intVariables;   // Integer variables (courage, trust, etc.)
    public List<StringVariable> stringVariables;
}
```

### Inventory
```csharp
public class InventorySaveData
{
    public List<string> itemIds;    // Item IDs in inventory
    public List<int> itemCounts;    // Quantity of each item
    public int selectedSlot;
}
```

### Level Progress
```csharp
public class LevelSaveData
{
    public string currentSegmentId;
    public List<string> completedSegments;
    public List<string> unlockedCheckpoints;
}
```

### Puzzles
```csharp
public class PuzzleSaveData
{
    public List<string> solvedPuzzleIds;  // IDs of solved puzzles
}
```

### Environment
```csharp
public class EnvironmentSaveData
{
    public float skyboxMood;  // 0 = blood red, 1 = dark night
}
```

---

## Save File Location

```
Windows: %USERPROFILE%/AppData/LocalLow/[Company]/[Game]/Saves/
├── save_0.json (Auto-save)
├── save_1.json (Manual save)
├── save_2.json
├── save_3.json
└── save_4.json
```

---

## Usage

### Quick Save/Load (In-Game)
| Key | Action |
|-----|--------|
| **F5** | Quick Save |
| **F9** | Quick Load |
| **ESC** | Open Pause Menu |

### Via Script

```csharp
// Ensure SaveManager exists
SaveManager.GetOrCreate();

// Save to slot
SaveManager.Instance.Save(1, "My Save");

// Load from slot
SaveManager.Instance.Load(1);

// Quick save/load
SaveManager.Instance.QuickSave();
SaveManager.Instance.QuickLoad();

// Check if saves exist
bool hasSaves = SaveManager.Instance.HasAnySave();

// Get most recent save (excludes auto-save by default)
int slot = SaveManager.Instance.GetMostRecentSaveSlot();

// Get slot info for UI
SaveSlotInfo info = SaveManager.Instance.GetSlotInfo(1);

// Delete save
SaveManager.Instance.DeleteSave(1);
```

### Via Dialogue Commands
```
save:1              → Save to slot 1
save:auto           → Trigger auto-save
```

---

## Inspector Settings

### SaveManager
```
Save Settings
├── Max Save Slots: 5
├── Save File Prefix: "save_"
├── Save File Extension: ".json"
├── Use Encryption: ☐
└── Pretty Print JSON: ☑

Scene Settings
└── Fallback Gameplay Scene: "GameplayScene"

Auto-Save
├── Enable Auto Save: ☑
├── Auto Save Interval: 300 (5 minutes)
├── Auto Save On Scene Change: ☑
└── Auto Save Slot: 0

Keyboard Shortcuts
├── Quick Save Key: F5
└── Quick Load Key: F9
```

---

## Main Menu Setup

### Scene Hierarchy
```
MainMenu Scene
├── Canvas
│   ├── MainMenuPanel
│   │   ├── ResumeButton
│   │   ├── NewGameButton
│   │   ├── LoadGameButton
│   │   ├── OptionsButton
│   │   └── ExitButton
│   │
│   ├── LoadGamePanel
│   │   ├── SaveSlotContainer (Vertical Layout)
│   │   └── BackButton
│   │
│   ├── OptionsPanel
│   │   └── BackButton
│   │
│   └── ConfirmDialog
│       ├── ConfirmText
│       ├── YesButton
│       └── NoButton
│
└── MainMenuManager (Component)
```

### MainMenuManager Inspector
```
Scene Settings
├── Gameplay Scene Name: "GameplayScene"
└── New Game Start Scene: "GameplayScene"

Main Menu Panel
├── Resume Button: [drag ResumeButton]
├── New Game Button: [drag NewGameButton]
├── Load Game Button: [drag LoadGameButton]
├── Options Button: [drag OptionsButton]
└── Exit Button: [drag ExitButton]

Load Game Panel
├── Load Game Panel: [drag LoadGamePanel]
├── Save Slot Container: [drag container]
└── Load Back Button: [drag BackButton]
```

---

## Pause Menu Setup (In-Game)

### Scene Hierarchy
```
Gameplay Scene
├── PauseMenuManager
├── Canvas
│   ├── PauseMenuPanel (hidden by default)
│   │   ├── ResumeButton
│   │   ├── SaveGameButton
│   │   ├── LoadGameButton
│   │   ├── OptionsButton
│   │   └── MainMenuButton
│   │
│   ├── SaveGamePanel
│   ├── LoadGamePanel
│   └── ConfirmDialog
```

---

## Events

```csharp
// Subscribe to save events
SaveManager.Instance.OnSaveStarted += (slot) => { };
SaveManager.Instance.OnSaveCompleted += (slot, success) => { };
SaveManager.Instance.OnLoadStarted += (slot) => { };
SaveManager.Instance.OnLoadCompleted += (slot, success) => { };
SaveManager.Instance.OnAutoSave += () => { };
```

---

## Adding New Saveable Data

### 1. Add to SaveData.cs
```csharp
[Serializable]
public class MySaveData
{
    public int myValue;
    public string myString;
}

// Add to SaveData class
public MySaveData myData = new MySaveData();
```

### 2. Gather in SaveManager.cs
```csharp
private void GatherMyData(MySaveData data)
{
    if (MyManager.Instance != null)
    {
        data.myValue = MyManager.Instance.GetValue();
        data.myString = MyManager.Instance.GetString();
    }
}
```

### 3. Apply in SaveManager.cs
```csharp
private void ApplyMyData(MySaveData data)
{
    if (MyManager.Instance != null)
    {
        MyManager.Instance.SetValue(data.myValue);
        MyManager.Instance.SetString(data.myString);
    }
}
```

---

## Troubleshooting

### Resume Button Greyed Out
- No saves exist yet
- Check Console for errors

### Scene Not Found Error
- Add scene to Build Settings (File → Build Settings)
- Set `Fallback Gameplay Scene` in SaveManager

### Save Not Loading Correctly
- Check if all managers exist in scene (GameState, InventoryManager, etc.)
- Check Console for specific errors

### PlayerPrefs Out of Sync
- Edit → Clear All PlayerPrefs
- Delete save files from Saves folder

---

## Best Practices

1. **Always use SaveManager.GetOrCreate()** in Main Menu to ensure it exists
2. **Add scenes to Build Settings** before testing save/load
3. **Test save/load early** to catch integration issues
4. **Use auto-save** for player convenience
5. **Show confirmation dialogs** before overwriting or deleting saves

---

*This system integrates with GameState, InventoryManager, LevelManager, Puzzles, and Environment systems.*

