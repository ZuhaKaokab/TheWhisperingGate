using UnityEngine;

namespace WhisperingGate.Environment
{
    /// <summary>
    /// Trigger component to start skybox transitions.
    /// Can be placed on trigger zones or called via dialogue commands.
    /// </summary>
    public class SkyboxTransitionTrigger : MonoBehaviour
    {
        [Header("Transition Settings")]
        [Tooltip("Target mood: 0 = Blood Red, 1 = Dark Night")]
        [Range(0f, 1f)]
        [SerializeField] private float targetMood = 1f;
        
        [Tooltip("Duration of the transition in seconds")]
        [SerializeField] private float transitionDuration = 10f;
        
        [Header("Trigger Settings")]
        [SerializeField] private bool triggerOnEnter = true;
        [SerializeField] private bool oneShot = true;
        
        private bool hasTriggered = false;

        private void OnTriggerEnter(Collider other)
        {
            if (!triggerOnEnter) return;
            if (oneShot && hasTriggered) return;
            
            if (other.CompareTag("Player"))
            {
                TriggerTransition();
            }
        }

        [ContextMenu("Trigger Transition")]
        public void TriggerTransition()
        {
            if (HorrorSkyboxController.Instance != null)
            {
                HorrorSkyboxController.Instance.TransitionToMood(targetMood, transitionDuration, () =>
                {
                    Debug.Log($"[Skybox] Transition complete! Now at mood {targetMood}");
                });
                hasTriggered = true;
            }
            else
            {
                Debug.LogWarning("[SkyboxTransition] No HorrorSkyboxController found in scene!");
            }
        }

        /// <summary>
        /// Static method for dialogue command integration.
        /// Format: sky:blood or sky:night or sky:0.5 or sky:night:10 (with duration)
        /// </summary>
        public static void ExecuteCommand(string param)
        {
            if (HorrorSkyboxController.Instance == null)
            {
                Debug.LogWarning("[SkyboxTransition] No HorrorSkyboxController found!");
                return;
            }

            string[] parts = param.Split(':');
            string target = parts[0].ToLower().Trim();
            float duration = 5f;

            // Parse duration if provided
            if (parts.Length > 1 && float.TryParse(parts[1], out float parsedDuration))
            {
                duration = parsedDuration;
            }

            // Parse target mood
            float targetMood;
            switch (target)
            {
                case "blood":
                case "red":
                case "bleeding":
                    targetMood = 0f;
                    break;
                    
                case "night":
                case "dark":
                case "blue":
                    targetMood = 1f;
                    break;
                    
                case "twilight":
                case "dusk":
                    targetMood = 0.4f;
                    break;
                    
                case "dawn":
                    targetMood = 0.6f;
                    break;
                    
                default:
                    // Try to parse as float
                    if (float.TryParse(target, out float customMood))
                    {
                        targetMood = Mathf.Clamp01(customMood);
                    }
                    else
                    {
                        Debug.LogWarning($"[SkyboxTransition] Unknown mood: {target}");
                        return;
                    }
                    break;
            }

            HorrorSkyboxController.Instance.TransitionToMood(targetMood, duration);
            Debug.Log($"[SkyboxTransition] Starting transition to {target} ({targetMood}) over {duration}s");
        }
    }
}

