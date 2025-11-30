using UnityEngine;
using UnityEngine.SceneManagement;
using WhisperingGate.Gameplay;

namespace WhisperingGate.Gameplay
{
    /// <summary>
    /// Handles transitions between scenes/areas. Can be triggered by player interaction
    /// or automatically when prerequisites are met.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class SceneTransition : MonoBehaviour
    {
        [Header("Transition Settings")]
        [SerializeField] private TransitionType transitionType = TransitionType.LoadScene;
        [SerializeField] private string targetSceneName = "";
        [SerializeField] private string targetLevelId = "";
        [SerializeField] private bool requireInteraction = true;
        [SerializeField] private KeyCode interactionKey = KeyCode.E;

        [Header("Prerequisites")]
        [Tooltip("Required completed segments (comma-separated).")]
        [SerializeField] private string requiredSegments = "";

        [Tooltip("GameState condition that must be true.")]
        [SerializeField] private string requiredCondition = "";

        [Header("Visual Feedback")]
        [SerializeField] private GameObject interactionPromptUI;
        [SerializeField] private bool showDebugInfo = false;

        private bool playerInRange = false;
        private bool prerequisitesMet = false;

        public enum TransitionType
        {
            LoadScene,      // Load a new Unity scene
            ChangeLevel,     // Change level ID (same scene, different area)
            TeleportPlayer   // Teleport player to a specific location
        }

        void Start()
        {
            var collider = GetComponent<Collider>();
            if (collider != null)
                collider.isTrigger = true;

            UpdatePrerequisites();
            UpdateVisualState();
        }

        void Update()
        {
            // Check prerequisites periodically
            bool previousPrerequisitesMet = prerequisitesMet;
            UpdatePrerequisites();

            if (prerequisitesMet != previousPrerequisitesMet)
            {
                UpdateVisualState();
            }

            // Handle interaction
            if (requireInteraction && playerInRange && prerequisitesMet && Input.GetKeyDown(interactionKey))
            {
                ExecuteTransition();
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = true;
                UpdateVisualState();

                if (!requireInteraction && prerequisitesMet)
                {
                    ExecuteTransition();
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
                UpdateVisualState();
            }
        }

        private void UpdatePrerequisites()
        {
            prerequisitesMet = true;

            // Check required segments
            if (!string.IsNullOrWhiteSpace(requiredSegments))
            {
                string[] segments = requiredSegments.Split(',');
                foreach (var segment in segments)
                {
                    string trimmedSegment = segment.Trim();
                    if (!string.IsNullOrEmpty(trimmedSegment))
                    {
                        if (LevelManager.Instance == null || !LevelManager.Instance.IsSegmentCompleted(trimmedSegment))
                        {
                            prerequisitesMet = false;
                            break;
                        }
                    }
                }
            }

            // Check game state condition
            if (prerequisitesMet && !string.IsNullOrWhiteSpace(requiredCondition))
            {
                if (Core.GameState.Instance == null)
                {
                    prerequisitesMet = false;
                }
                else
                {
                    prerequisitesMet = Core.GameState.Instance.EvaluateCondition(requiredCondition);
                }
            }
        }

        private void UpdateVisualState()
        {
            if (interactionPromptUI != null)
            {
                interactionPromptUI.SetActive(playerInRange && prerequisitesMet);
            }
        }

        private void ExecuteTransition()
        {
            if (!prerequisitesMet)
            {
                if (showDebugInfo)
                    Debug.Log($"[SceneTransition] Prerequisites not met for transition on {gameObject.name}");
                return;
            }

            switch (transitionType)
            {
                case TransitionType.LoadScene:
                    LoadScene();
                    break;

                case TransitionType.ChangeLevel:
                    ChangeLevel();
                    break;

                case TransitionType.TeleportPlayer:
                    TeleportPlayer();
                    break;
            }
        }

        private void LoadScene()
        {
            if (string.IsNullOrEmpty(targetSceneName))
            {
                Debug.LogError($"[SceneTransition] Target scene name is empty on {gameObject.name}");
                return;
            }

            if (showDebugInfo)
                Debug.Log($"[SceneTransition] Loading scene: {targetSceneName}");

            SceneManager.LoadScene(targetSceneName);
        }

        private void ChangeLevel()
        {
            if (string.IsNullOrEmpty(targetLevelId))
            {
                Debug.LogError($"[SceneTransition] Target level ID is empty on {gameObject.name}");
                return;
            }

            if (LevelManager.Instance == null)
            {
                Debug.LogError("[SceneTransition] LevelManager.Instance is null. Make sure LevelManager exists in scene.");
                return;
            }

            if (showDebugInfo)
                Debug.Log($"[SceneTransition] Changing level: {targetLevelId}");

            LevelManager.Instance.ChangeLevel(targetLevelId);
        }

        private void TeleportPlayer()
        {
            // This would teleport the player to a specific location
            // Implementation depends on your player controller setup
            if (showDebugInfo)
                Debug.Log($"[SceneTransition] Teleport player functionality not yet implemented");
        }
    }
}


