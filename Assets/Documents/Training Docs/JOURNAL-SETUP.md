# Journal System Setup Guide

## Overview
The journal system allows players to collect and read story content, clues, and lore. Pages can be unlocked through gameplay, dialogue, or finding the physical journal object.

---

## Quick Setup

### 1. Create Journal Manager
1. Create empty GameObject: `JournalManager`
2. Add `JournalManager` component
3. Create a `JournalConfig` ScriptableObject (see below)
4. Assign config to the manager

### 2. Create Journal Config
1. Right-click in Project: **Create → Whispering Gate → Journal → Journal Config**
2. Name it `MainJournalConfig`
3. Configure visual settings (colors, sounds)

### 3. Create Journal Pages
1. Right-click in Project: **Create → Whispering Gate → Journal → Journal Page**
2. Name it (e.g., `Page_Introduction`)
3. Configure:
   - **Page ID**: Unique identifier (e.g., `intro_1`)
   - **Page Title**: Display title
   - **Sort Order**: Position in journal (lower = earlier)
   - **Content Type**: Text, Image, or Both
   - **Text Content**: The actual text (supports rich text)
   - **Unlocked By Default**: Check for starting pages

4. Add page to `JournalConfig.allPages` list

### 4. Create Journal UI
1. Create Canvas for journal UI
2. Add `JournalUI` component
3. Set up the UI hierarchy (see UI Structure below)
4. Assign references to the component

### 5. Place Physical Journal Object
1. Create/import journal 3D model
2. Add `JournalPickup` component
3. Add Collider (set as Trigger)
4. Configure:
   - **Require Interaction**: true (press E to pick up)
   - **Auto Unlock Pages**: List of page IDs to unlock on pickup
   - **Pickup Sound**: Audio feedback

---

## UI Structure

```
Canvas (Journal)
└── JournalPanel (with CanvasGroup)
    ├── Background (Image - journal cover/spread)
    ├── LeftPageContainer
    │   ├── LeftPageBackground (Image)
    │   ├── LeftPageTitle (TextMeshPro)
    │   ├── LeftPageText (TextMeshPro)
    │   ├── LeftPageImage (Image)
    │   └── LeftPageNewIndicator (GameObject)
    ├── RightPageContainer
    │   ├── RightPageBackground (Image)
    │   ├── RightPageTitle (TextMeshPro)
    │   ├── RightPageText (TextMeshPro)
    │   ├── RightPageImage (Image)
    │   └── RightPageNewIndicator (GameObject)
    ├── Navigation
    │   ├── PrevButton (Button)
    │   ├── PageNumberText (TextMeshPro)
    │   └── NextButton (Button)
    └── CloseButton (Button)
```

---

## Command Reference

Use these in dialogue nodes or other command-enabled systems:

| Command | Description | Example |
|---------|-------------|---------|
| `journal:unlock:page_id` | Unlock a specific page | `journal:unlock:clue_forest` |
| `journal:open` | Open the journal UI | `journal:open` |
| `journal:goto:page_id` | Open to a specific page | `journal:goto:map_1` |
| `journal:pickup` | Give player the journal | `journal:pickup` |

---

## Page Content Types

### Text Only
```
Content Type: Text
Text Content: "The forest holds many secrets..."
```

### Image Only
```
Content Type: Image
Page Image: [Sprite]
Image Position: Full
```

### Text and Image
```
Content Type: TextAndImage
Text Content: "A map of the forest..."
Page Image: [Sprite]
Image Position: Top
```

### Image Positions
- **Top**: Image at top, text below
- **Bottom**: Text at top, image below
- **Full**: Image fills the page
- **Background**: Image behind text

---

## Unlock Methods

### 1. Default Unlock
Check "Unlocked By Default" on the page - available from start.

### 2. Dialogue Command
In dialogue node command field:
```
journal:unlock:page_id
```

### 3. GameState Condition
Set "Unlock Condition" on the page:
```
talked_to_writer
```
or
```
courage >= 20
```
Page auto-unlocks when condition is met.

### 4. Physical Pickup
Add page IDs to `JournalPickup.autoUnlockPages` array.

### 5. Script
```csharp
JournalManager.Instance.UnlockPage("page_id");
```

---

## Example: Prologue Journal Setup

### Pages to Create:

1. **Page_Welcome** (Sort: 0)
   - Unlocked by default
   - "Welcome to your journey..."

2. **Page_ForestLore** (Sort: 10)
   - Unlock condition: `talked_to_writer`
   - Lore about the forest

3. **Page_GridHint** (Sort: 20)
   - Unlock via dialogue: `journal:unlock:grid_hint`
   - Hint for grid puzzle

4. **Page_SymbolGuide** (Sort: 30)
   - Unlock via dialogue: `journal:unlock:symbol_guide`
   - Guide for rotation puzzle

5. **Page_PortalLore** (Sort: 40)
   - Unlock condition: `portal_unlocked`
   - Auto-unlocks when portal is activated

---

## Controls

| Key | Action |
|-----|--------|
| **J** | Open journal (configurable) |
| **Q** | Previous page |
| **E** | Next page |
| **Escape/Tab** | Close journal |

---

## Integration with Other Systems

### GameState
- `has_journal` - Set when journal is picked up
- `journal_found` - Set when journal is picked up
- Page unlock flags set automatically

### Dialogue
Use commands in dialogue nodes to unlock pages at story beats.

### Puzzles
Add `journal:unlock:hint_page` to puzzle triggers.

---

## Tips

1. **Start Small**: Create 3-5 pages for testing
2. **Sort Order**: Use increments of 10 (0, 10, 20...) to allow inserting pages later
3. **Rich Text**: Use `<b>bold</b>`, `<i>italic</i>`, `<color=#FF0000>red</color>`
4. **New Indicator**: Shows on unread pages - great for player guidance
5. **Audio Feedback**: Add page flip sounds for immersion

---

## Troubleshooting

**Journal won't open:**
- Check `hasJournal` is true on JournalManager
- Ensure player has picked up journal or call `journal:pickup`

**Pages not showing:**
- Verify page is in `JournalConfig.allPages`
- Check page is unlocked (`unlockedByDefault` or via command)
- Check sort order isn't causing issues

**UI not displaying:**
- Ensure all UI references are assigned in JournalUI
- Check Canvas is active and in correct render mode

