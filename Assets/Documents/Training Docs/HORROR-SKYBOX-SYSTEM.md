# Horror Skybox System Documentation
## The Whispering Gate

**Version:** 1.0  
**Last Updated:** December 2025

---

## Overview

A procedural horror skybox system with mood transitions, designed for atmospheric horror games. Supports smooth transitions between "blood red bleeding sky" and "dark night" moods.

---

## Features

- **Procedural gradient sky** (top, horizon, bottom colors)
- **Sun/Moon** with glow and customizable position
- **Procedural clouds** with animation
- **Stars** (visible at night)
- **Mood system** with smooth transitions
- **Fog integration** synced with sky colors
- **Dialogue command integration**
- **Save/Load support**

---

## Files

| File | Purpose |
|------|---------|
| `Shaders/HorrorSkybox.shader` | Procedural skybox shader |
| `Scripts/Environment/HorrorSkyboxController.cs` | Controls skybox, handles transitions |
| `Scripts/Environment/SkyboxTransitionTrigger.cs` | Trigger zones + dialogue commands |

---

## Setup

### Step 1: Create Material
1. Right-click in Project → **Create → Material**
2. Name it `HorrorSkyboxMaterial`
3. Set shader to **WhisperingGate/HorrorSkybox**

### Step 2: Add Controller to Scene
1. Create empty GameObject → name it `SkyboxController`
2. Add component: **HorrorSkyboxController**
3. Drag `HorrorSkyboxMaterial` to **Skybox Material** field

### Step 3: Done!
Tweak values in Inspector to match your desired look.

---

## Mood System

The skybox uses a **Mood Blend** value from 0 to 1:

| Value | Mood | Description |
|-------|------|-------------|
| **0** | Blood Red | Dark red bleeding sky (prologue start) |
| **0.4** | Twilight | Eerie transition state |
| **1** | Dark Night | Deep blue/black night sky |

---

## Inspector Settings

### Mood A - Blood Red Sky (Prologue Start)
```
Top Color:      Dark blood red     (0.15, 0.02, 0.02)
Horizon Color:  Bright blood red   (0.6, 0.1, 0.05)
Bottom Color:   Dark crimson       (0.08, 0.02, 0.02)
Sun Color:      Red/orange         (1, 0.2, 0.1)
Cloud Color:    Dark red           (0.3, 0.05, 0.02)
Sun Intensity:  0.8
Cloud Density:  0.6
```

### Mood B - Dark Night Sky
```
Top Color:      Deep dark blue     (0.02, 0.02, 0.08)
Horizon Color:  Slightly lighter   (0.05, 0.08, 0.15)
Bottom Color:   Almost black       (0.01, 0.01, 0.03)
Sun Color:      Pale moon          (0.6, 0.7, 0.9)
Cloud Color:    Dark blue          (0.03, 0.04, 0.08)
Sun Intensity:  0.3
Cloud Density:  0.4
```

### Sun/Moon Settings
```
Sun Height:     -1 to 1 (below/above horizon)
Sun Rotation:   0-360 degrees
Sun Size:       0.01 - 0.3
Sun Glow:       0 - 1
```

### Stars
```
Stars Intensity: 0 - 1 (0 = no stars, 1 = full stars)
Stars Speed:     Twinkle animation speed
```

---

## Usage

### In Inspector (Real-time)
Drag the **Mood Blend** slider to see instant changes.

### Via Script

```csharp
// Instant change
HorrorSkyboxController.Instance.SetMood(0f);    // Blood sky
HorrorSkyboxController.Instance.SetMood(1f);    // Night sky

// Smooth transition over 10 seconds
HorrorSkyboxController.Instance.TransitionToBloodSky(10f);
HorrorSkyboxController.Instance.TransitionToNightSky(10f);

// Custom transition with callback
HorrorSkyboxController.Instance.TransitionToMood(0.5f, 15f, () => {
    Debug.Log("Transition complete!");
});

// Set sun position
HorrorSkyboxController.Instance.SetSunPosition(height: 0.2f, rotation: 45f);

// Set stars
HorrorSkyboxController.Instance.SetStarsIntensity(0.5f);

// Check current state
float mood = HorrorSkyboxController.Instance.CurrentMood;
bool transitioning = HorrorSkyboxController.Instance.IsTransitioning;
```

### Via Dialogue Commands

```
sky:blood           → Transition to blood red (5s default)
sky:night           → Transition to dark night
sky:night:15        → Transition to night over 15 seconds
sky:twilight        → Transition to twilight (0.4)
sky:0.3             → Transition to custom blend value
sky:0.7:20          → Custom blend (0.7) over 20 seconds
```

### Via Trigger Zone

1. Create a trigger collider in scene
2. Add `SkyboxTransitionTrigger` component
3. Set **Target Mood** (0 = blood, 1 = night)
4. Set **Transition Duration**
5. Player walks through → sky changes!

---

## Context Menu Presets

Right-click on HorrorSkyboxController in Inspector:

- **Apply Preset: Blood Sky** - Prologue start look
- **Apply Preset: Dark Night** - After transition
- **Apply Preset: Twilight Horror** - Eerie in-between state

---

## Prologue Example Flow

```
Scene Start:
├── SetMood(0)              → Blood red bleeding sky
│
Player progresses through jungle...
├── sky:twilight:30         → Slowly fade to twilight (30s)
│
Player reaches the house...
├── sky:night:60            → Fade to dark night over 1 minute
│
Jump scare moment:
└── sky:blood:3             → Quick flash back to blood (3s)
    sky:night:5             → Return to night
```

---

## Shader Properties

The `HorrorSkybox.shader` exposes these properties:

### Sky Gradient
| Property | Type | Description |
|----------|------|-------------|
| `_TopColor` | Color | Sky color at zenith |
| `_HorizonColor` | Color | Sky color at horizon |
| `_BottomColor` | Color | Sky color below horizon |
| `_HorizonSharpness` | Float | Blend sharpness (0.1-2) |

### Sun/Moon
| Property | Type | Description |
|----------|------|-------------|
| `_SunColor` | Color | Sun disc color |
| `_SunDirection` | Vector3 | Direction to sun |
| `_SunSize` | Float | Sun disc size (0.001-0.5) |
| `_SunIntensity` | Float | Sun brightness (0-2) |
| `_SunGlow` | Float | Glow around sun (0-2) |

### Clouds
| Property | Type | Description |
|----------|------|-------------|
| `_CloudColor` | Color | Cloud color |
| `_CloudDensity` | Float | Cloud coverage (0-1) |
| `_CloudSpeed` | Float | Cloud movement speed |
| `_CloudScale` | Float | Cloud size (0.5-5) |
| `_CloudHeight` | Float | Minimum height for clouds |

### Stars
| Property | Type | Description |
|----------|------|-------------|
| `_StarsIntensity` | Float | Star visibility (0-1) |
| `_StarsSpeed` | Float | Twinkle speed |
| `_StarsDensity` | Float | Number of stars |

### Effects
| Property | Type | Description |
|----------|------|-------------|
| `_DistortionStrength` | Float | Horror distortion effect |
| `_DistortionSpeed` | Float | Distortion animation speed |

---

## Integration with Other Systems

### Save/Load
The skybox mood is automatically saved and restored:
```csharp
// In SaveManager - already integrated
data.environment.skyboxMood = HorrorSkyboxController.Instance.CurrentMood;
```

### Dialogue System
Commands are integrated via `DialogueManager.ExecuteCommand()`:
```csharp
case "sky":
    Environment.SkyboxTransitionTrigger.ExecuteCommand(param);
    break;
```

### Fog
Fog color is automatically synced with skybox mood:
- Blood sky → reddish fog
- Night sky → bluish fog

---

## Performance Notes

- Shader uses **5-octave FBM** for clouds (can reduce for mobile)
- Stars use **procedural generation** (no textures needed)
- All effects are **GPU-based** (minimal CPU overhead)
- Transitions use **smooth interpolation** (no popping)

---

## Customization

### Adding New Moods

In `HorrorSkyboxController.cs`, add new presets:

```csharp
[ContextMenu("Apply Preset: Sunset Horror")]
public void ApplyPresetSunsetHorror()
{
    SetMood(0.3f);
    sunHeight = -0.05f;
    sunSize = 0.25f;
    sunGlow = 1f;
    starsIntensity = 0f;
}
```

### Adding New Commands

In `SkyboxTransitionTrigger.ExecuteCommand()`:

```csharp
case "sunset":
    targetMood = 0.3f;
    break;
```

---

## Troubleshooting

### Sky is Pink/Magenta
- Shader not found - reimport or check shader path

### No Transition Happening
- Check if `HorrorSkyboxController.Instance` exists
- Check Console for errors

### Clouds Not Visible
- Increase `Cloud Density`
- Check `Cloud Height` setting

### Stars Not Showing
- Increase `Stars Intensity`
- Make sure `Mood Blend` is closer to 1 (night)

---

## Best Practices

1. **Start with presets** and tweak from there
2. **Use slow transitions** (10-60 seconds) for atmosphere
3. **Sync with story beats** via dialogue commands
4. **Test in Play mode** - changes are visible in real-time
5. **Save your settings** as presets for consistency

---

*This system creates immersive atmospheric horror through dynamic sky changes.*

