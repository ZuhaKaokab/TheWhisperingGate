using UnityEngine;
using WhisperingGate.Core;
using WhisperingGate.Gameplay;

namespace WhisperingGate.Items
{
    /// <summary>
    /// Controls the player's equipped flashlight.
    /// Handles on/off toggle, spotlight, and battery (optional).
    /// Supports both FPP and TPP view modes with separate anchor points.
    /// </summary>
    public class FlashlightController : MonoBehaviour
    {
        public static FlashlightController Instance { get; private set; }

        [Header("Flashlight Objects")]
        [Tooltip("The visible flashlight model (held in player's hand)")]
        [SerializeField] private GameObject flashlightModel;
        
        [Tooltip("Spotlight component for the flashlight beam")]
        [SerializeField] private Light flashlightSpotlight;

        [Header("View Mode Anchors")]
        [Tooltip("Anchor point for First Person view (e.g., near camera or hand)")]
        [SerializeField] private Transform fppAnchor;
        
        [Tooltip("Anchor point for Third Person view (e.g., player's hand bone)")]
        [SerializeField] private Transform tppAnchor;
        
        [Tooltip("Smoothly transition between anchors")]
        [SerializeField] private float anchorTransitionSpeed = 10f;
        
        [Tooltip("Hide flashlight model in FPP (only show light)")]
        [SerializeField] private bool hideModelInFPP = false;

        [Header("Light Settings")]
        [SerializeField] private float lightIntensity = 2f;
        [SerializeField] private float lightRange = 20f;
        [SerializeField] private float spotAngle = 45f;
        [SerializeField] private Color lightColor = Color.white;

        [Header("Controls")]
        [SerializeField] private KeyCode toggleKey = KeyCode.F;
        [SerializeField] private bool canToggle = true;

        [Header("Battery System (Optional)")]
        [SerializeField] private bool useBattery = false;
        [SerializeField] private float maxBattery = 100f;
        [SerializeField] private float batteryDrainRate = 1f; // per second
        [SerializeField] private float currentBattery = 100f;
        
        [Tooltip("Light dims as battery drains")]
        [SerializeField] private bool dimWithBattery = true;
        
        [Tooltip("Minimum intensity when battery is low")]
        [SerializeField] private float minIntensity = 0.3f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip toggleOnSound;
        [SerializeField] private AudioClip toggleOffSound;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        // Runtime state
        private bool hasFlashlight = false;
        private bool isOn = false;
        private bool isEnabled = false;
        private float baseIntensity;
        private Transform currentAnchor;
        private bool isFirstPerson = true;

        public bool HasFlashlight => hasFlashlight;
        public bool IsOn => isOn;
        public float CurrentBattery => currentBattery;
        public float BatteryPercent => maxBattery > 0 ? currentBattery / maxBattery : 0f;

        // Events
        public event System.Action<bool> OnFlashlightToggled;
        public event System.Action OnBatteryDepleted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            baseIntensity = lightIntensity;
            
            // Set initial anchor
            currentAnchor = fppAnchor != null ? fppAnchor : transform;

            // Start disabled
            SetFlashlightVisible(false);
            SetOn(false);

            // Check if already have flashlight from saved state
            if (GameState.Instance != null && GameState.Instance.GetBool("has_flashlight"))
            {
                hasFlashlight = true;
                bool wasOn = GameState.Instance.GetBool("flashlight_on");
                EnableFlashlight(wasOn);
            }
        }

        private void Update()
        {
            if (!hasFlashlight || !isEnabled) return;

            // Toggle
            if (canToggle && Input.GetKeyDown(toggleKey))
            {
                Toggle();
            }

            // Check view mode and update anchor
            UpdateViewModeAnchor();

            // Smoothly follow anchor
            UpdateFlashlightPosition();

            // Battery drain
            if (isOn && useBattery)
            {
                currentBattery -= batteryDrainRate * Time.deltaTime;
                
                // Dim light as battery drains
                if (dimWithBattery && flashlightSpotlight != null)
                {
                    float batteryRatio = currentBattery / maxBattery;
                    flashlightSpotlight.intensity = Mathf.Lerp(minIntensity, baseIntensity, batteryRatio);
                }
                
                if (currentBattery <= 0)
                {
                    currentBattery = 0;
                    TurnOff();
                    OnBatteryDepleted?.Invoke();
                    if (enableDebugLogs) Debug.Log("[Flashlight] Battery depleted!");
                }
            }
        }

        /// <summary>
        /// Check PlayerController view mode and update anchor accordingly.
        /// </summary>
        private void UpdateViewModeAnchor()
        {
            if (PlayerController.Instance == null) return;
            
            bool currentlyFirstPerson = PlayerController.Instance.CurrentViewMode == PlayerController.ViewMode.FirstPerson;
            
            if (currentlyFirstPerson != isFirstPerson)
            {
                isFirstPerson = currentlyFirstPerson;
                currentAnchor = isFirstPerson ? fppAnchor : tppAnchor;
                
                // Handle model visibility based on view mode
                if (hideModelInFPP && flashlightModel != null && hasFlashlight)
                {
                    flashlightModel.SetActive(!isFirstPerson);
                }
                
                if (enableDebugLogs) Debug.Log($"[Flashlight] Switched to {(isFirstPerson ? "FPP" : "TPP")} anchor");
            }
        }

        /// <summary>
        /// Update flashlight position to follow current anchor.
        /// </summary>
        private void UpdateFlashlightPosition()
        {
            if (currentAnchor == null || flashlightModel == null) return;
            
            // Smoothly move to anchor position
            flashlightModel.transform.position = Vector3.Lerp(
                flashlightModel.transform.position,
                currentAnchor.position,
                Time.deltaTime * anchorTransitionSpeed
            );
            
            // Smoothly rotate to anchor rotation
            flashlightModel.transform.rotation = Quaternion.Slerp(
                flashlightModel.transform.rotation,
                currentAnchor.rotation,
                Time.deltaTime * anchorTransitionSpeed
            );
        }

        /// <summary>
        /// Enable the flashlight system (called when flashlight is picked up).
        /// </summary>
        public void EnableFlashlight(bool startOn = false)
        {
            hasFlashlight = true;
            isEnabled = true;
            
            SetFlashlightVisible(true);
            
            if (startOn)
            {
                TurnOn();
            }
            else
            {
                TurnOff();
            }

            if (enableDebugLogs) Debug.Log($"[Flashlight] Enabled, on: {startOn}");
        }

        /// <summary>
        /// Disable the flashlight system.
        /// </summary>
        public void DisableFlashlight()
        {
            hasFlashlight = false;
            isEnabled = false;
            
            TurnOff();
            SetFlashlightVisible(false);

            if (enableDebugLogs) Debug.Log("[Flashlight] Disabled");
        }

        /// <summary>
        /// Turn on the flashlight.
        /// </summary>
        public void TurnOn()
        {
            if (!hasFlashlight || isOn) return;
            if (useBattery && currentBattery <= 0) return;

            SetOn(true);
            PlaySound(toggleOnSound);

            // Save state
            if (GameState.Instance != null)
            {
                GameState.Instance.SetBool("flashlight_on", true);
            }

            OnFlashlightToggled?.Invoke(true);
            if (enableDebugLogs) Debug.Log("[Flashlight] Turned on");
        }

        /// <summary>
        /// Turn off the flashlight.
        /// </summary>
        public void TurnOff()
        {
            if (!isOn) return;

            SetOn(false);
            PlaySound(toggleOffSound);

            // Save state
            if (GameState.Instance != null)
            {
                GameState.Instance.SetBool("flashlight_on", false);
            }

            OnFlashlightToggled?.Invoke(false);
            if (enableDebugLogs) Debug.Log("[Flashlight] Turned off");
        }

        /// <summary>
        /// Toggle flashlight on/off.
        /// </summary>
        public void Toggle()
        {
            if (isOn)
                TurnOff();
            else
                TurnOn();
        }

        /// <summary>
        /// Add battery charge.
        /// </summary>
        public void AddBattery(float amount)
        {
            currentBattery = Mathf.Clamp(currentBattery + amount, 0, maxBattery);
            if (enableDebugLogs) Debug.Log($"[Flashlight] Battery: {currentBattery}/{maxBattery}");
        }

        /// <summary>
        /// Refill battery to max.
        /// </summary>
        public void RefillBattery()
        {
            currentBattery = maxBattery;
            if (enableDebugLogs) Debug.Log("[Flashlight] Battery refilled");
        }

        private void SetFlashlightVisible(bool visible)
        {
            if (flashlightModel != null)
                flashlightModel.SetActive(visible);
        }

        private void SetOn(bool on)
        {
            isOn = on;

            if (flashlightSpotlight != null)
            {
                flashlightSpotlight.enabled = on;
                if (on)
                {
                    flashlightSpotlight.intensity = baseIntensity;
                    flashlightSpotlight.range = lightRange;
                    flashlightSpotlight.spotAngle = spotAngle;
                    flashlightSpotlight.color = lightColor;
                }
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip == null) return;

            if (audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
            else
            {
                AudioSource.PlayClipAtPoint(clip, transform.position, 0.5f);
            }
        }

        /// <summary>
        /// Execute flashlight command.
        /// Format: flashlight:action
        /// Actions: on, off, toggle, recharge
        /// </summary>
        public static void ExecuteCommand(string action, string param = "")
        {
            if (Instance == null)
            {
                Debug.LogWarning("[Flashlight] FlashlightController not found");
                return;
            }

            switch (action.ToLower())
            {
                case "on":
                    Instance.TurnOn();
                    break;
                case "off":
                    Instance.TurnOff();
                    break;
                case "toggle":
                    Instance.Toggle();
                    break;
                case "recharge":
                case "refill":
                    if (float.TryParse(param, out float amount))
                        Instance.AddBattery(amount);
                    else
                        Instance.RefillBattery();
                    break;
                case "enable":
                    Instance.EnableFlashlight(param == "on");
                    break;
                case "disable":
                    Instance.DisableFlashlight();
                    break;
                default:
                    Debug.LogWarning($"[Flashlight] Unknown action: {action}");
                    break;
            }
        }
    }
}

