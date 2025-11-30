# Impact Notifications & Stats Panel - Setup Guide
## Adding "He Will Remember That" Style Notifications

**Status:** Ready to Use  
**Features:** Impact notifications, real-time stats display

---

## OVERVIEW

Two new UI systems:
1. **ImpactNotificationUI** - Shows temporary notifications when choices have impacts
2. **StatsPanelUI** - Displays current player stats (courage, trust, sanity, etc.)

---

## PART 1: IMPACT NOTIFICATIONS SETUP

### Step 1: Create Notification Prefab
1. In your Canvas, create a new GameObject: `ImpactNotification`
2. Add components:
   - **Image** (background, semi-transparent)
   - **TMP_Text** (child object) for the notification text
3. Set up:
   - Image: Color with alpha ~0.8, rounded corners if desired
   - TMP_Text: Center alignment, bold font, readable size
4. Make it a **Prefab** → Save as `UI/ImpactNotification.prefab`
5. **Important:** Set prefab to **inactive** by default

### Step 2: Create Notification Container
1. In Canvas, create empty GameObject: `ImpactNotificationContainer`
2. Add **Vertical Layout Group** component:
   - Child Alignment: Upper Center
   - Spacing: 10
   - Padding: Top/Bottom 20, Left/Right 10
3. Add **Content Size Fitter** (optional, for auto-sizing)
4. Position: Top-center of screen (or wherever you want notifications)

### Step 3: Add ImpactNotificationUI Component
1. Add `ImpactNotificationUI` script to `ImpactNotificationContainer`
2. Assign in Inspector:
   - **Notification Container** → ImpactNotificationContainer (self)
   - **Notification Prefab** → ImpactNotification.prefab
   - **Notification Duration** → 3 seconds (default)
   - **Max Notifications** → 5 (how many can stack)

### Step 4: Connect to DialogueUIPanel
1. Select your `DialogueUIPanel` GameObject
2. In Inspector, find **"Impact Notifications"** section
3. Drag `ImpactNotificationContainer` into **"Impact Notification UI"** field

### Step 5: Test
1. Enter Play Mode
2. Trigger dialogue with choices that have impacts
3. Make a choice → Notification should appear showing variable change
4. Example: "Courage +10" appears in gold color

---

## PART 2: STATS PANEL SETUP

### Option A: Standalone Stats Panel (Recommended)

#### Step 1: Create Stats Panel UI
1. In Canvas, create GameObject: `StatsPanel`
2. Add **Image** component (background panel)
3. Add `StatsPanelUI` component
4. Create child objects for each stat:

**Hierarchy:**
```
StatsPanel
├─ Background (Image)
├─ CourageStat
│  ├─ Label (TMP_Text: "Courage:")
│  ├─ Value (TMP_Text: "50/100")
│  └─ ProgressBar (Image, fill type: Filled)
├─ TrustAlinaStat
│  ├─ Label
│  ├─ Value
│  └─ ProgressBar
├─ TrustWriterStat
│  ├─ Label
│  ├─ Value
│  └─ ProgressBar
├─ SanityStat
│  ├─ Label
│  ├─ Value
│  └─ ProgressBar
└─ InvestigationStat
   ├─ Label
   ├─ Value
   └─ ProgressBar
```

#### Step 2: Configure StatsPanelUI
1. Select `StatsPanel`
2. In Inspector, assign:
   - **Panel Root** → StatsPanel (self)
   - **Courage Text** → CourageStat/Value
   - **Trust Alina Text** → TrustAlinaStat/Value
   - **Trust Writer Text** → TrustWriterStat/Value
   - **Sanity Text** → SanityStat/Value
   - **Investigation Text** → InvestigationStat/Value
   - **Courage Bar** → CourageStat/ProgressBar (optional)
   - **Trust Alina Bar** → TrustAlinaStat/ProgressBar (optional)
   - **Trust Writer Bar** → TrustWriterStat/ProgressBar (optional)
   - **Sanity Bar** → SanityStat/ProgressBar (optional)
   - **Investigation Bar** → InvestigationStat/ProgressBar (optional)
   - **Toggle Key** → Tab (or your preferred key)
   - **Show By Default** → false (or true if you want it always visible)

#### Step 3: Style the Panel
- Position: Top-right corner (or wherever you prefer)
- Background: Semi-transparent dark panel
- Text: White/light color, readable font
- Progress bars: Color-coded (gold for courage, red for sanity, etc.)

### Option B: Integrate into Inventory Panel

#### Step 1: Add Stats Section to Inventory
1. Open your `InventoryUIPanel` GameObject
2. Add a new section at the top: `StatsSection`
3. Create stat displays (same as Option A, but inside inventory panel)
4. Add `StatsPanelUI` component
5. Assign references (same as Option A)

#### Step 2: Show Stats When Inventory Opens
- Stats automatically update when inventory is open
- Or keep stats always visible in a corner

---

## PART 3: CUSTOMIZATION

### Custom Impact Messages
Edit `ImpactNotificationUI.cs` to customize messages:

```csharp
// In ShowVariableChange method, you can add custom logic:
if (variableName == "courage" && change > 0)
    message = "He will remember your bravery";
else if (variableName == "trust_alina" && change > 0)
    message = "Alina trusts you more";
```

### Color Coding
Default colors:
- **Courage:** Gold (1, 0.8, 0)
- **Trust:** Light Blue (0.2, 0.8, 1)
- **Sanity:** Red (0.8, 0.2, 0.2)
- **Investigation:** Purple (0.6, 0.4, 1)

### Notification Animation
- Slides in from left
- Fades in over 0.3 seconds
- Stays for 3 seconds (configurable)
- Slides out to right and fades out

---

## TESTING CHECKLIST

### Impact Notifications
- [ ] Create notification prefab and container
- [ ] Assign to DialogueUIPanel
- [ ] Make a choice with impact → Notification appears
- [ ] Multiple impacts → Multiple notifications stack
- [ ] Notifications fade out correctly
- [ ] Item gained → Shows "Item gained: [name]"

### Stats Panel
- [ ] Create stats panel UI
- [ ] Assign all text/bar references
- [ ] Enter Play Mode → Stats show current values
- [ ] Make choice with impact → Stats update in real-time
- [ ] Press toggle key → Panel shows/hides
- [ ] Progress bars fill correctly (if using bars)

### Integration Test
- [ ] Full dialogue flow:
  1. Start dialogue
  2. Make choice with impact
  3. Notification appears
  4. Stats update
  5. Item gained → Notification appears
  6. Continue dialogue → All systems work together

---

## EXAMPLE: "He Will Remember That" Style

To add custom messages like "He will remember that":

1. In `ImpactNotificationUI.cs`, modify `ShowVariableChange`:

```csharp
public void ShowVariableChange(string variableName, int change)
{
    // Custom messages for specific variables
    if (variableName == "trust_writer" && change > 0)
    {
        ShowCustomNotification("The Writer will remember that", Color.cyan);
        return;
    }
    
    if (variableName == "trust_alina" && change > 0)
    {
        ShowCustomNotification("Alina will remember that", new Color(0.2f, 0.8f, 1f));
        return;
    }
    
    // Default behavior for other variables
    // ... rest of method
}
```

2. Or add to `DialogueUIPanel.cs` in `HandleImpactApplied`:

```csharp
private void HandleImpactApplied(string variableName, int change)
{
    if (impactNotificationUI != null)
    {
        // Show custom message for trust changes
        if ((variableName == "trust_writer" || variableName == "trust_alina") && change > 0)
        {
            string name = variableName == "trust_writer" ? "The Writer" : "Alina";
            impactNotificationUI.ShowCustomNotification($"{name} will remember that", Color.cyan);
        }
        else
        {
            // Show normal variable change
            impactNotificationUI.ShowVariableChange(variableName, change);
        }
    }
}
```

---

## TROUBLESHOOTING

| Issue | Solution |
|-------|----------|
| Notifications don't appear | Check ImpactNotificationUI is assigned to DialogueUIPanel |
| Stats don't update | Ensure GameState exists and StatsPanelUI is subscribed |
| Notifications overlap | Adjust spacing in Vertical Layout Group |
| Stats panel doesn't toggle | Check toggle key isn't conflicting (Tab might conflict with inventory) |
| Progress bars don't fill | Ensure Image component has "Fill" type set |

---

## QUICK SETUP SUMMARY

1. **Impact Notifications:**
   - Create prefab → Create container → Add component → Connect to DialogueUIPanel

2. **Stats Panel:**
   - Create panel UI → Add component → Assign references → Test

3. **Custom Messages:**
   - Edit `HandleImpactApplied` in DialogueUIPanel for custom text

---

**Both systems are event-driven and automatically update. No manual refresh needed!**







