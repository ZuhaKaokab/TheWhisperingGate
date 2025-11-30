using UnityEngine;
using WhisperingGate.Interaction;
using WhisperingGate.Dialogue;
using WhisperingGate.Gameplay;
using WhisperingGate.Core;

namespace WhisperingGate.Testing
{
    /// <summary>
    /// Debug script to check DialogueSegmentTrigger setup and diagnose issues.
    /// Attach to any GameObject to see diagnostic info in the console.
    /// </summary>
    public class DialogueSegmentTriggerDebug : MonoBehaviour
    {
        void Start()
        {
            Debug.Log("=== DIALOGUE SEGMENT TRIGGER DIAGNOSTICS ===");
            
            // Check DialogueManager
            if (DialogueManager.Instance == null)
            {
                Debug.LogError("[DIAGNOSTIC] ❌ DialogueManager.Instance is NULL! Create a GameObject with DialogueManager component.");
            }
            else
            {
                Debug.Log("[DIAGNOSTIC] ✅ DialogueManager.Instance exists");
            }

            // Check LevelManager
            if (LevelManager.Instance == null)
            {
                Debug.LogError("[DIAGNOSTIC] ❌ LevelManager.Instance is NULL! Create a GameObject with LevelManager component.");
            }
            else
            {
                Debug.Log("[DIAGNOSTIC] ✅ LevelManager.Instance exists");
            }

            // Check GameState
            if (GameState.Instance == null)
            {
                Debug.LogError("[DIAGNOSTIC] ❌ GameState.Instance is NULL! Create a GameObject with GameState component.");
            }
            else
            {
                Debug.Log("[DIAGNOSTIC] ✅ GameState.Instance exists");
            }

            // Check Player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError("[DIAGNOSTIC] ❌ No GameObject with 'Player' tag found! Tag your player GameObject as 'Player'.");
            }
            else
            {
                Debug.Log($"[DIAGNOSTIC] ✅ Player found: {player.name}");
                
                // Check if player has a collider
                Collider playerCollider = player.GetComponent<Collider>();
                if (playerCollider == null)
                {
                    Debug.LogWarning("[DIAGNOSTIC] ⚠️ Player has no Collider component. OnTriggerEnter won't work!");
                }
                else
                {
                    Debug.Log($"[DIAGNOSTIC] ✅ Player has Collider: {playerCollider.GetType().Name}");
                }
            }

            // Check all DialogueSegmentTriggers in scene
            DialogueSegmentTrigger[] triggers = FindObjectsOfType<DialogueSegmentTrigger>();
            Debug.Log($"[DIAGNOSTIC] Found {triggers.Length} DialogueSegmentTrigger(s) in scene");

            for (int i = 0; i < triggers.Length; i++)
            {
                var trigger = triggers[i];
                Debug.Log($"\n--- Trigger {i + 1}: {trigger.gameObject.name} ---");
                
                // Check collider
                Collider collider = trigger.GetComponent<Collider>();
                if (collider == null)
                {
                    Debug.LogError($"  ❌ No Collider component on {trigger.gameObject.name}");
                }
                else
                {
                    if (!collider.isTrigger)
                    {
                        Debug.LogError($"  ❌ Collider on {trigger.gameObject.name} is NOT set as Trigger!");
                    }
                    else
                    {
                        Debug.Log($"  ✅ Collider is set as Trigger: {collider.GetType().Name}");
                    }
                }

                // Check dialogue tree
                var tree = trigger.GetComponent<DialogueSegmentTrigger>();
                if (tree == null)
                {
                    Debug.LogError($"  ❌ DialogueSegmentTrigger component not found (this shouldn't happen)");
                }
                else
                {
                    // Use reflection to check private fields
                    var treeField = typeof(DialogueSegmentTrigger).GetField("dialogueTree", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (treeField != null)
                    {
                        var assignedTree = treeField.GetValue(trigger) as DialogueTree;
                        if (assignedTree == null)
                        {
                            Debug.LogError($"  ❌ No Dialogue Tree assigned!");
                        }
                        else
                        {
                            Debug.Log($"  ✅ Dialogue Tree assigned: {assignedTree.name}");
                        }
                    }
                }
            }

            Debug.Log("\n=== END DIAGNOSTICS ===");
            Debug.Log("Press F6 to re-run diagnostics");
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F6))
            {
                Start(); // Re-run diagnostics
            }
        }
    }
}


