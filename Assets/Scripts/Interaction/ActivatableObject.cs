using UnityEngine;
using System.Collections;
using WhisperingGate.Core;

namespace WhisperingGate.Interaction
{
    /// <summary>
    /// Generic activatable object - can be a portal, torch sconce, mechanism, etc.
    /// Supports enabling objects, spawning prefabs, playing effects.
    /// </summary>
    public class ActivatableObject : MonoBehaviour
    {
        [Header("Identity")]
        [Tooltip("Unique ID for command system (activate:this_id)")]
        [SerializeField] private string objectId = "portal_1";

        [Header("Activation Type")]
        [SerializeField] private ActivationType activationType = ActivationType.EnableObject;

        [Header("Enable Object (if EnableObject)")]
        [Tooltip("Object to enable/disable on activation")]
        [SerializeField] private GameObject targetObject;

        [Header("Spawn Prefab (if SpawnPrefab)")]
        [Tooltip("Prefab to instantiate on activation")]
        [SerializeField] private GameObject spawnPrefab;
        
        [Tooltip("Where to spawn the prefab")]
        [SerializeField] private Transform spawnPoint;
        
        [Tooltip("Destroy spawned object on deactivation")]
        [SerializeField] private bool destroyOnDeactivate = false;

        [Header("Animation (if PlayAnimation)")]
        [SerializeField] private Animator animator;
        [SerializeField] private string activateTrigger = "Activate";
        [SerializeField] private string deactivateTrigger = "Deactivate";

        [Header("Visual Effects")]
        [Tooltip("Particles to play on activation")]
        [SerializeField] private ParticleSystem activationParticles;
        
        [Tooltip("Light to enable/fade in")]
        [SerializeField] private Light activationLight;
        
        [Tooltip("Light fade duration")]
        [SerializeField] private float lightFadeDuration = 1f;
        
        [Tooltip("Renderer to change material/emission")]
        [SerializeField] private Renderer glowRenderer;
        
        [Tooltip("Emission color when activated")]
        [SerializeField] private Color emissionColor = Color.cyan;
        
        [Tooltip("Emission intensity")]
        [SerializeField] private float emissionIntensity = 2f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip activateSound;
        [SerializeField] private AudioClip deactivateSound;
        [SerializeField] private AudioClip loopSound;

        [Header("Timing")]
        [SerializeField] private float activationDelay = 0f;
        [SerializeField] private float effectDuration = 2f;

        [Header("State")]
        [SerializeField] private bool startsActive = false;
        [SerializeField] private bool canDeactivate = false;

        [Header("GameState Integration")]
        [Tooltip("Flag to set when activated")]
        [SerializeField] private string onActivateFlag = "";
        
        [Tooltip("Commands to execute when activated")]
        [SerializeField] private string[] onActivateCommands;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        // Runtime state
        private bool isActive = false;
        private bool isAnimating = false;
        private GameObject spawnedInstance;
        private AudioSource loopAudioSource;
        private MaterialPropertyBlock propertyBlock;

        // Static registry
        private static System.Collections.Generic.Dictionary<string, ActivatableObject> registry = 
            new System.Collections.Generic.Dictionary<string, ActivatableObject>();

        public string ObjectId => objectId;
        public bool IsActive => isActive;

        private void Awake()
        {
            // Register
            if (!string.IsNullOrWhiteSpace(objectId))
            {
                registry[objectId] = this;
            }

            propertyBlock = new MaterialPropertyBlock();

            // Initial state
            if (startsActive)
            {
                isActive = true;
                ApplyActiveState(true, immediate: true);
            }
            else
            {
                ApplyActiveState(false, immediate: true);
            }
        }

        private void OnDestroy()
        {
            if (!string.IsNullOrWhiteSpace(objectId) && registry.ContainsKey(objectId))
            {
                registry.Remove(objectId);
            }
        }

        /// <summary>
        /// Get an activatable object by ID.
        /// </summary>
        public static ActivatableObject GetObject(string id)
        {
            if (registry.TryGetValue(id, out ActivatableObject obj))
                return obj;
            return null;
        }

        /// <summary>
        /// Activate this object.
        /// </summary>
        public void Activate()
        {
            if (isActive || isAnimating) return;

            if (enableDebugLogs) Debug.Log($"[Activatable] Activating '{objectId}'");
            StartCoroutine(PerformActivation(true));
        }

        /// <summary>
        /// Deactivate this object.
        /// </summary>
        public void Deactivate()
        {
            if (!isActive || isAnimating || !canDeactivate) return;

            if (enableDebugLogs) Debug.Log($"[Activatable] Deactivating '{objectId}'");
            StartCoroutine(PerformActivation(false));
        }

        /// <summary>
        /// Toggle activation state.
        /// </summary>
        public void Toggle()
        {
            if (isActive)
                Deactivate();
            else
                Activate();
        }

        private IEnumerator PerformActivation(bool activating)
        {
            isAnimating = true;

            if (activationDelay > 0)
                yield return new WaitForSeconds(activationDelay);

            // Play sound
            PlaySound(activating ? activateSound : deactivateSound);

            // Play particles
            if (activationParticles != null && activating)
            {
                activationParticles.Play();
            }

            // Handle based on type
            switch (activationType)
            {
                case ActivationType.EnableObject:
                    if (targetObject != null)
                        targetObject.SetActive(activating);
                    break;

                case ActivationType.SpawnPrefab:
                    if (activating)
                    {
                        SpawnObject();
                    }
                    else if (destroyOnDeactivate && spawnedInstance != null)
                    {
                        Destroy(spawnedInstance);
                        spawnedInstance = null;
                    }
                    break;

                case ActivationType.PlayAnimation:
                    if (animator != null)
                    {
                        animator.SetTrigger(activating ? activateTrigger : deactivateTrigger);
                    }
                    break;
            }

            // Visual effects
            if (activating)
            {
                StartCoroutine(FadeInEffects());
            }
            else
            {
                StartCoroutine(FadeOutEffects());
            }

            // Loop sound
            if (loopSound != null && activating)
            {
                StartLoopSound();
            }
            else if (!activating)
            {
                StopLoopSound();
            }

            yield return new WaitForSeconds(effectDuration);

            isActive = activating;
            isAnimating = false;

            // Set flag and execute commands
            if (activating)
            {
                if (!string.IsNullOrWhiteSpace(onActivateFlag) && GameState.Instance != null)
                {
                    GameState.Instance.SetBool(onActivateFlag, true);
                }

                ExecuteCommands(onActivateCommands);
            }

            if (enableDebugLogs) Debug.Log($"[Activatable] '{objectId}' is now {(isActive ? "active" : "inactive")}");
        }

        private void SpawnObject()
        {
            if (spawnPrefab == null) return;

            Transform point = spawnPoint != null ? spawnPoint : transform;
            spawnedInstance = Instantiate(spawnPrefab, point.position, point.rotation);
            
            if (enableDebugLogs) Debug.Log($"[Activatable] Spawned '{spawnPrefab.name}'");
        }

        private void ApplyActiveState(bool active, bool immediate)
        {
            switch (activationType)
            {
                case ActivationType.EnableObject:
                    if (targetObject != null)
                        targetObject.SetActive(active);
                    break;
            }

            // Light
            if (activationLight != null)
            {
                activationLight.enabled = active;
                if (immediate)
                    activationLight.intensity = active ? 1f : 0f;
            }

            // Emission
            if (glowRenderer != null && immediate)
            {
                SetEmission(active ? emissionIntensity : 0f);
            }
        }

        private IEnumerator FadeInEffects()
        {
            float elapsed = 0f;

            while (elapsed < lightFadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / lightFadeDuration;

                if (activationLight != null)
                {
                    activationLight.enabled = true;
                    activationLight.intensity = Mathf.Lerp(0f, 1f, t);
                }

                if (glowRenderer != null)
                {
                    SetEmission(Mathf.Lerp(0f, emissionIntensity, t));
                }

                yield return null;
            }

            if (activationLight != null)
                activationLight.intensity = 1f;
            if (glowRenderer != null)
                SetEmission(emissionIntensity);
        }

        private IEnumerator FadeOutEffects()
        {
            float elapsed = 0f;

            while (elapsed < lightFadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / lightFadeDuration;

                if (activationLight != null)
                {
                    activationLight.intensity = Mathf.Lerp(1f, 0f, t);
                }

                if (glowRenderer != null)
                {
                    SetEmission(Mathf.Lerp(emissionIntensity, 0f, t));
                }

                yield return null;
            }

            if (activationLight != null)
            {
                activationLight.intensity = 0f;
                activationLight.enabled = false;
            }
            if (glowRenderer != null)
                SetEmission(0f);
        }

        private void SetEmission(float intensity)
        {
            if (glowRenderer == null) return;

            glowRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_EmissionColor", emissionColor * intensity);
            glowRenderer.SetPropertyBlock(propertyBlock);
        }

        private void StartLoopSound()
        {
            if (loopSound == null) return;

            if (loopAudioSource == null)
            {
                loopAudioSource = gameObject.AddComponent<AudioSource>();
                loopAudioSource.loop = true;
                loopAudioSource.playOnAwake = false;
            }

            loopAudioSource.clip = loopSound;
            loopAudioSource.Play();
        }

        private void StopLoopSound()
        {
            if (loopAudioSource != null && loopAudioSource.isPlaying)
            {
                loopAudioSource.Stop();
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
                AudioSource.PlayClipAtPoint(clip, transform.position);
            }
        }

        private void ExecuteCommands(string[] commands)
        {
            if (commands == null) return;

            foreach (string cmd in commands)
            {
                if (string.IsNullOrWhiteSpace(cmd)) continue;
                if (enableDebugLogs) Debug.Log($"[Activatable] Execute command: {cmd}");
            }
        }

        /// <summary>
        /// Execute command from command system.
        /// Format: activate:object_id or deactivate:object_id
        /// </summary>
        public static void ExecuteCommand(string action, string targetId)
        {
            ActivatableObject obj = GetObject(targetId);
            if (obj == null)
            {
                Debug.LogWarning($"[Activatable] Object not found: {targetId}");
                return;
            }

            switch (action.ToLower())
            {
                case "activate":
                case "on":
                case "enable":
                    obj.Activate();
                    break;
                case "deactivate":
                case "off":
                case "disable":
                    obj.Deactivate();
                    break;
                case "toggle":
                    obj.Toggle();
                    break;
                default:
                    Debug.LogWarning($"[Activatable] Unknown action: {action}");
                    break;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = isActive ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);

            if (spawnPoint != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(spawnPoint.position, 0.3f);
                Gizmos.DrawLine(transform.position, spawnPoint.position);
            }
        }
#endif
    }

    public enum ActivationType
    {
        EnableObject,   // Enable/disable a GameObject
        SpawnPrefab,    // Instantiate a prefab
        PlayAnimation   // Trigger animator
    }
}

