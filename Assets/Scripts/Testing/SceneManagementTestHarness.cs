using UnityEngine;
using WhisperingGate.Gameplay;

namespace WhisperingGate.Testing
{
    /// <summary>
    /// Test harness for Scene/Level Management System.
    /// Provides hotkeys to test segment completion, checkpoints, and level progression.
    /// </summary>
    public class SceneManagementTestHarness : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool enableTestHarness = true;
        [SerializeField] private KeyCode completeSegmentKey = KeyCode.F1;
        [SerializeField] private KeyCode checkSegmentKey = KeyCode.F2;
        [SerializeField] private KeyCode setCheckpointKey = KeyCode.F3;
        [SerializeField] private KeyCode resetProgressKey = KeyCode.F4;
        [SerializeField] private KeyCode showDebugInfoKey = KeyCode.F5;

        [Header("Test Segment IDs")]
        [SerializeField] private string testSegmentId = "jungle_awakening";
        [SerializeField] private string testCheckpointId = "checkpoint_test_01";

        void Update()
        {
            if (!enableTestHarness) return;

            if (LevelManager.Instance == null)
            {
                Debug.LogWarning("[SceneManagementTestHarness] LevelManager.Instance is null. Make sure LevelManager exists in scene.");
                return;
            }

            // F1: Complete a segment
            if (Input.GetKeyDown(completeSegmentKey))
            {
                LevelManager.Instance.CompleteSegment(testSegmentId);
                Debug.Log($"[TestHarness] Completed segment: {testSegmentId}");
            }

            // F2: Check segment status
            if (Input.GetKeyDown(checkSegmentKey))
            {
                bool completed = LevelManager.Instance.IsSegmentCompleted(testSegmentId);
                Debug.Log($"[TestHarness] Segment '{testSegmentId}' completed: {completed}");
            }

            // F3: Set checkpoint
            if (Input.GetKeyDown(setCheckpointKey))
            {
                LevelManager.Instance.SetCheckpoint(testCheckpointId);
                Debug.Log($"[TestHarness] Checkpoint set: {testCheckpointId}");
            }

            // F4: Reset level progress
            if (Input.GetKeyDown(resetProgressKey))
            {
                LevelManager.Instance.ResetLevelProgress();
                Debug.Log("[TestHarness] Level progress reset");
            }

            // F5: Show debug info
            if (Input.GetKeyDown(showDebugInfoKey))
            {
                string debugInfo = LevelManager.Instance.GetDebugInfo();
                Debug.Log($"[TestHarness] Debug Info:\n{debugInfo}");
            }
        }

        void OnGUI()
        {
            if (!enableTestHarness) return;

            // Display test controls
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 12;
            style.normal.textColor = Color.white;

            string controls = "Scene Management Test Harness:\n" +
                             $"{completeSegmentKey}: Complete segment '{testSegmentId}'\n" +
                             $"{checkSegmentKey}: Check segment status\n" +
                             $"{setCheckpointKey}: Set checkpoint '{testCheckpointId}'\n" +
                             $"{resetProgressKey}: Reset level progress\n" +
                             $"{showDebugInfoKey}: Show debug info";

            GUI.Label(new Rect(10, 10, 400, 150), controls, style);
        }
    }
}


