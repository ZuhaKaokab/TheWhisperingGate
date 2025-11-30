using UnityEngine;
using WhisperingGate.Core;
using WhisperingGate.Gameplay;

namespace WhisperingGate.Gameplay
{
    /// <summary>
    /// Checkpoint marker that saves game state when player enters its trigger zone.
    /// Can be used to restore progress after death or scene reload.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Checkpoint : MonoBehaviour
    {
        [Header("Checkpoint Settings")]
        [SerializeField] private string checkpointId = "checkpoint_01";
        [SerializeField] private bool activateOnEnter = true;
        [SerializeField] private bool savePlayerPosition = true;
        [SerializeField] private Transform spawnPoint;

        [Header("Visual Feedback")]
        [SerializeField] private GameObject activatedIndicator;
        [SerializeField] private bool showDebugInfo = false;

        private bool isActivated = false;

        void Start()
        {
            var collider = GetComponent<Collider>();
            if (collider != null)
                collider.isTrigger = true;

            // Use this transform as spawn point if none specified
            if (spawnPoint == null)
                spawnPoint = transform;

            // Check if already activated (from save data)
            if (LevelManager.Instance != null)
            {
                isActivated = LevelManager.Instance.IsCheckpointActivated(checkpointId);
            }

            UpdateVisualState();
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && activateOnEnter)
            {
                ActivateCheckpoint();
            }
        }

        /// <summary>
        /// Manually activates this checkpoint.
        /// </summary>
        public void ActivateCheckpoint()
        {
            if (isActivated)
            {
                if (showDebugInfo)
                    Debug.Log($"[Checkpoint] Checkpoint '{checkpointId}' already activated");
                return;
            }

            if (LevelManager.Instance == null)
            {
                Debug.LogError("[Checkpoint] LevelManager.Instance is null. Make sure LevelManager exists in scene.");
                return;
            }

            isActivated = true;
            LevelManager.Instance.SetCheckpoint(checkpointId);

            // Save player position if enabled
            if (savePlayerPosition && spawnPoint != null)
            {
                SavePlayerPosition();
            }

            UpdateVisualState();

            if (showDebugInfo)
                Debug.Log($"[Checkpoint] Checkpoint activated: {checkpointId}");
        }

        private void SavePlayerPosition()
        {
            // Save spawn point position to PlayerPrefs
            PlayerPrefs.SetFloat($"checkpoint_{checkpointId}_pos_x", spawnPoint.position.x);
            PlayerPrefs.SetFloat($"checkpoint_{checkpointId}_pos_y", spawnPoint.position.y);
            PlayerPrefs.SetFloat($"checkpoint_{checkpointId}_pos_z", spawnPoint.position.z);
            PlayerPrefs.SetFloat($"checkpoint_{checkpointId}_rot_y", spawnPoint.rotation.eulerAngles.y);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Gets the saved spawn position for this checkpoint.
        /// </summary>
        public Vector3 GetSpawnPosition()
        {
            if (spawnPoint != null)
            {
                return spawnPoint.position;
            }

            // Try to load from save data
            float x = PlayerPrefs.GetFloat($"checkpoint_{checkpointId}_pos_x", transform.position.x);
            float y = PlayerPrefs.GetFloat($"checkpoint_{checkpointId}_pos_y", transform.position.y);
            float z = PlayerPrefs.GetFloat($"checkpoint_{checkpointId}_pos_z", transform.position.z);
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Gets the saved spawn rotation for this checkpoint.
        /// </summary>
        public float GetSpawnRotation()
        {
            if (spawnPoint != null)
            {
                return spawnPoint.rotation.eulerAngles.y;
            }

            return PlayerPrefs.GetFloat($"checkpoint_{checkpointId}_rot_y", transform.rotation.eulerAngles.y);
        }

        private void UpdateVisualState()
        {
            if (activatedIndicator != null)
            {
                activatedIndicator.SetActive(isActivated);
            }
        }

        public bool IsActivated => isActivated;
        public string CheckpointId => checkpointId;
    }
}


