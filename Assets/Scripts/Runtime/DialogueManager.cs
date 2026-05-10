using UnityEngine;
using System;
using System.Collections.Generic;
using WhisperingGate.Dialogue;
using WhisperingGate.Core;
using WhisperingGate.Camera;

namespace WhisperingGate.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [Header("Audio Settings")]
        [SerializeField] private AudioSource dialogueAudioSource; // Audio play karne ke liye source

        public event Action<DialogueNode> OnNodeDisplayed;
        public event Action OnDialogueEnded;
        public event Action<int> OnChoicesUpdated;
        public event Action<string, int> OnImpactApplied;
        public event Action<string> OnItemGiven;
        public event Action<DialogueNode> OnChoiceSelected;

        private DialogueNode currentNode;
        private DialogueTree currentTree;
        private bool isDialogueActive = false;

        public bool IsDialogueActive => isDialogueActive;
        public DialogueNode CurrentNode => currentNode;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Agar Inspector mein assign nahi kiya toh khud dhoond lega
            if (dialogueAudioSource == null)
                dialogueAudioSource = GetComponent<AudioSource>();
        }

        public void StartDialogue(DialogueTree tree)
        {
            if (tree == null)
            {
                Debug.LogError("[DialogueManager] Tried to start null dialogue tree");
                return;
            }

            if (tree.StartNode == null)
            {
                Debug.LogError($"[DialogueManager] Dialogue tree '{tree.TreeId}' has no start node");
                return;
            }

            currentTree = tree;
            isDialogueActive = true;
            ShowNode(tree.StartNode);
        }

        public void StartDialogueAtNode(DialogueTree tree, DialogueNode startNode)
        {
            if (tree == null || startNode == null) return;
            currentTree = tree;
            isDialogueActive = true;
            ShowNode(startNode);
        }

        public void StartDialogueAtNodeId(DialogueTree tree, string nodeId)
        {
            if (tree == null || string.IsNullOrWhiteSpace(nodeId)) return;
            DialogueNode targetNode = FindNodeById(nodeId);
            if (targetNode != null) StartDialogueAtNode(tree, targetNode);
        }

        private DialogueNode FindNodeById(string nodeId)
        {
#if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:DialogueNode");
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                DialogueNode node = UnityEditor.AssetDatabase.LoadAssetAtPath<DialogueNode>(path);
                if (node != null && node.NodeId.Equals(nodeId, System.StringComparison.OrdinalIgnoreCase)) return node;
            }
#endif

            DialogueNode[] allNodes = Resources.FindObjectsOfTypeAll<DialogueNode>();
            foreach (var node in allNodes)
            {
                if (node.NodeId.Equals(nodeId, System.StringComparison.OrdinalIgnoreCase)) return node;
            }
            return null;
        }

        public void SelectChoice(int choiceIndex)
        {
            if (!isDialogueActive || currentNode == null) return;

            var visibleChoices = GetVisibleChoices();
            if (choiceIndex < 0 || choiceIndex >= visibleChoices.Count) return;

            var choice = visibleChoices[choiceIndex];
            OnChoiceSelected?.Invoke(currentNode);
            ApplyImpacts(choice.Impacts);

            foreach (var cmd in currentNode.EndCommands)
                ExecuteCommand(cmd);

            if (choice.NextNode == null) EndDialogue();
            else ShowNode(choice.NextNode);
        }

        public void AdvanceToNextNode()
        {
            if (!isDialogueActive || currentNode == null) return;

            var visibleChoices = GetVisibleChoices();
            if (visibleChoices.Count > 0) return;

            if (currentNode.NextNodeIfAuto == null)
            {
                EndDialogue();
                return;
            }

            foreach (var cmd in currentNode.EndCommands)
                ExecuteCommand(cmd);

            ShowNode(currentNode.NextNodeIfAuto);
        }

        public List<DialogueChoice> GetVisibleChoices()
        {
            var visibleChoices = new List<DialogueChoice>();
            if (currentNode == null || GameState.Instance == null) return visibleChoices;

            foreach (var choice in currentNode.Choices)
            {
                if (!choice.HasCondition || GameState.Instance.EvaluateCondition(choice.ShowCondition))
                    visibleChoices.Add(choice);
            }
            return visibleChoices;
        }

        private void ShowNode(DialogueNode node)
        {
            if (node == null)
            {
                EndDialogue();
                return;
            }

            currentNode = node;

            // --- AUDIO PLAYBACK LOGIC ---
            if (dialogueAudioSource != null && node.VoiceClip != null)
            {
                dialogueAudioSource.Stop(); // Purana audio band karo
                dialogueAudioSource.clip = node.VoiceClip; // Naya clip lagao
                dialogueAudioSource.Play(); // Play karo
            }

            foreach (var cmd in node.StartCommands)
                ExecuteCommand(cmd);

            OnNodeDisplayed?.Invoke(node);

            int visibleChoiceCount = GetVisibleChoices().Count;
            OnChoicesUpdated?.Invoke(visibleChoiceCount);

            if (node.IsEndNode)
            {
                if (visibleChoiceCount == 0)
                {
                    float delay = node.DisplayDuration > 0 ? node.DisplayDuration : 3f;
                    Invoke(nameof(EndDialogue), delay);
                }
            }
        }

        private void ApplyImpacts(List<ChoiceImpact> impacts)
        {
            if (impacts == null || GameState.Instance == null) return;
            foreach (var impact in impacts)
            {
                if (impact.IsConditional && !GameState.Instance.EvaluateCondition(impact.ApplyCondition))
                    continue;

                GameState.Instance.AddInt(impact.VariableName, impact.ValueChange);
                OnImpactApplied?.Invoke(impact.VariableName, impact.ValueChange);
            }
        }

        private void ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return;

            command = command.Trim();
            int colonIndex = command.IndexOf(':');
            string cmd, param;

            if (colonIndex > 0)
            {
                cmd = command.Substring(0, colonIndex).ToLower().Trim();
                param = command.Substring(colonIndex + 1).Trim();
            }
            else
            {
                cmd = command.ToLower().Trim();
                param = "";
            }

            switch (cmd)
            {
                case "item": GiveItem(param); break;
                case "flag": GameState.Instance?.SetBool(param, true); break;
                case "unflag": GameState.Instance?.SetBool(param, false); break;
                case "var": HandleVarCommand(param); break;
                case "cam": HandleCameraCommand(param); break;
                case "door": HandleDoorCommand(param); break;
                case "flashlight": HandleFlashlightCommand(param); break;
                case "sky": Environment.SkyboxTransitionTrigger.ExecuteCommand(param); break;
                case "activate": Interaction.ActivatableObject.ExecuteCommand("activate", param); break;
                case "deactivate": Interaction.ActivatableObject.ExecuteCommand("deactivate", param); break;
                case "journal": HandleJournalCommand(param); break;
                default: Debug.LogWarning($"[DialogueManager] Unknown command: {cmd}"); break;
            }
        }

        private void HandleVarCommand(string param)
        {
            if (param.Contains("+"))
            {
                var subparts = param.Split('+');
                var varName = subparts[0].Trim();
                if (int.TryParse(subparts[1].Trim(), out int delta))
                    GameState.Instance?.AddInt(varName, delta);
            }
        }

        private void GiveItem(string itemId)
        {
            if (Gameplay.InventoryManager.Instance != null)
            {
                Gameplay.InventoryManager.Instance.AddItem(itemId);
                OnItemGiven?.Invoke(itemId);
            }
        }

        private void HandleCameraCommand(string param)
        {
            if (CameraFocusController.Instance == null) return;
            string[] parts = param.Split(':');
            string target = parts[0].ToLower().Trim();
            float duration = (parts.Length > 1 && float.TryParse(parts[1].Trim(), out float d)) ? d : -1f;

            if (target == "reset" || target == "release") CameraFocusController.Instance.ReleaseFocus();
            else CameraFocusController.Instance.FocusOn(target, duration);
        }

        private void HandleDoorCommand(string param)
        {
            string[] parts = param.Split(':');
            string action = parts.Length > 1 ? parts[0].ToLower().Trim() : "open";
            string doorId = parts.Length > 1 ? parts[1].Trim() : parts[0].Trim();
            Interaction.Door.ExecuteCommand(action, doorId);
        }

        private void HandleFlashlightCommand(string param)
        {
            string[] parts = param.Split(':');
            string action = parts[0].ToLower().Trim();
            string actionParam = parts.Length > 1 ? parts[1].Trim() : "";
            Items.FlashlightController.ExecuteCommand(action, actionParam);
        }

        private void HandleJournalCommand(string param)
        {
            if (Journal.JournalManager.Instance == null) return;
            string[] parts = param.Split(':');
            string action = parts[0].ToLower().Trim();
            string actionParam = parts.Length > 1 ? parts[1].Trim() : "";
            Journal.JournalManager.Instance.ExecuteCommand(action, actionParam);
        }

        private void EndDialogue()
        {
            if (!isDialogueActive) return;

            // Dialogue khatam hote hi audio band kar den
            if (dialogueAudioSource != null) dialogueAudioSource.Stop();

            isDialogueActive = false;
            currentNode = null;
            currentTree = null;
            OnDialogueEnded?.Invoke();
        }
    }
}