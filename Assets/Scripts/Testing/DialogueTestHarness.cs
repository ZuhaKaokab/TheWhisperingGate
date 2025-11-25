using UnityEngine;
using WhisperingGate.Dialogue;

namespace WhisperingGate.Testing
{
    /// <summary>
    /// Simple helper that starts a specified DialogueTree when a key is pressed (or automatically on Start).
    /// Useful for testing DialogueManager + UI without setting up world triggers.
    /// </summary>
    public class DialogueTestHarness : MonoBehaviour
    {
        [SerializeField] private DialogueTree dialogueTree;
        [SerializeField] private KeyCode triggerKey = KeyCode.T;
        [SerializeField] private bool triggerOnStart = false;

        private void Start()
        {
            if (triggerOnStart)
            {
                TriggerDialogue();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(triggerKey))
            {
                TriggerDialogue();
            }
        }

        private void TriggerDialogue()
        {
            if (dialogueTree == null)
            {
                Debug.LogWarning("[DialogueTestHarness] DialogueTree not assigned.");
                return;
            }

            if (DialogueManager.Instance == null)
            {
                Debug.LogError("[DialogueTestHarness] DialogueManager.Instance is null.");
                return;
            }

            DialogueManager.Instance.StartDialogue(dialogueTree);
            Debug.Log($"[DialogueTestHarness] Started dialogue tree '{dialogueTree.TreeId}'.");
        }
    }
}

