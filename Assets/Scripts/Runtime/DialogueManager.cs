using UnityEngine;
using System;
using System.Collections.Generic;
using WhisperingGate.Dialogue;
using WhisperingGate.Core;

namespace WhisperingGate.Dialogue
{
    /// <summary>
    /// Core manager that orchestrates dialogue flow, choice selection, and command execution.
    /// Implements Singleton pattern for global access. Uses event-driven architecture for loose coupling.
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }
        
        public event Action<DialogueNode> OnNodeDisplayed;
        public event Action OnDialogueEnded;
        public event Action<int> OnChoicesUpdated;
        
        private DialogueNode currentNode;
        private DialogueTree currentTree;
        private bool isDialogueActive = false;
        
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        /// <summary>
        /// Starts a dialogue tree. Sets up the conversation and displays the first node.
        /// </summary>
        /// <param name="tree">The dialogue tree to start. Must not be null.</param>
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
        
        /// <summary>
        /// Handles player choice selection. Applies impacts, executes commands, and advances to next node.
        /// </summary>
        /// <param name="choiceIndex">Index of the selected choice in the current node's choices list.</param>
        public void SelectChoice(int choiceIndex)
        {
            if (!isDialogueActive || currentNode == null) return;
            
            // Get visible choices only (filtered by conditions)
            var visibleChoices = GetVisibleChoices();
            if (choiceIndex < 0 || choiceIndex >= visibleChoices.Count)
            {
                Debug.LogWarning($"[DialogueManager] Invalid choice index: {choiceIndex}");
                return;
            }
            
            var choice = visibleChoices[choiceIndex];
            
            Debug.Log($"[DialogueManager] Choice selected: {choice.ChoiceText}");
            
            // Apply impacts before moving to next node
            ApplyImpacts(choice.Impacts);
            
            // Execute end commands from current node
            foreach (var cmd in currentNode.EndCommands)
                ExecuteCommand(cmd);
            
            // Move to next node
            ShowNode(choice.NextNode);
        }
        
        /// <summary>
        /// Advances to the next node automatically (for nodes with no choices or auto-advance enabled).
        /// </summary>
        public void AdvanceToNextNode()
        {
            if (!isDialogueActive || currentNode == null) return;
            
            if (currentNode.Choices.Count == 0)
            {
                // Execute end commands before advancing
                foreach (var cmd in currentNode.EndCommands)
                    ExecuteCommand(cmd);
                    
                ShowNode(currentNode.NextNodeIfAuto);
            }
        }
        
        public bool IsDialogueActive => isDialogueActive;
        public DialogueNode CurrentNode => currentNode;
        
        /// <summary>
        /// Gets list of visible choices (filtered by conditions) for the current node.
        /// </summary>
        public List<DialogueChoice> GetVisibleChoices()
        {
            var visibleChoices = new List<DialogueChoice>();
            
            if (currentNode == null || GameState.Instance == null)
                return visibleChoices;
            
            foreach (var choice in currentNode.Choices)
            {
                if (!choice.HasCondition || GameState.Instance.EvaluateCondition(choice.ShowCondition))
                {
                    visibleChoices.Add(choice);
                }
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
            
            Debug.Log($"[DialogueManager] Showing node: {node.NodeId}");
            
            // Execute start commands first
            foreach (var cmd in node.StartCommands)
                ExecuteCommand(cmd);
            
            // Notify subscribers that a new node is displayed
            OnNodeDisplayed?.Invoke(node);
            
            // Count and notify about visible choices
            int visibleChoiceCount = GetVisibleChoices().Count;
            OnChoicesUpdated?.Invoke(visibleChoiceCount);
            
            // Auto-end if this is an end node
            if (node.IsEndNode)
            {
                float delay = node.DisplayDuration > 0 ? node.DisplayDuration : 3f;
                Invoke(nameof(EndDialogue), delay);
            }
        }
        
        private int CountVisibleChoices(DialogueNode node)
        {
            if (node == null || GameState.Instance == null) return 0;
            
            int count = 0;
            foreach (var choice in node.Choices)
            {
                if (!choice.HasCondition || GameState.Instance.EvaluateCondition(choice.ShowCondition))
                    count++;
            }
            return count;
        }
        
        private void ApplyImpacts(List<ChoiceImpact> impacts)
        {
            if (impacts == null || GameState.Instance == null) return;
            
            foreach (var impact in impacts)
            {
                // Skip conditional impacts if condition not met
                if (impact.IsConditional && !GameState.Instance.EvaluateCondition(impact.ApplyCondition))
                    continue;
                
                // Apply the impact
                GameState.Instance.AddInt(impact.VariableName, impact.ValueChange);
                Debug.Log($"[DialogueManager] Impact: {impact.VariableName} += {impact.ValueChange}");
            }
        }
        
        /// <summary>
        /// Executes a command string. Commands are in format "command:parameter".
        /// Supported commands: item, flag, unflag, var, ending
        /// </summary>
        private void ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return;
            
            command = command.Trim();
            var parts = command.Split(':');
            var cmd = parts[0].ToLower();
            var param = parts.Length > 1 ? parts[1].Trim() : "";
            
            Debug.Log($"[DialogueManager] Executing command: {cmd} | {param}");
            
            switch (cmd)
            {
                case "item":
                    GiveItem(param);
                    break;
                
                case "flag":
                    if (GameState.Instance != null)
                        GameState.Instance.SetBool(param, true);
                    break;
                
                case "unflag":
                    if (GameState.Instance != null)
                        GameState.Instance.SetBool(param, false);
                    break;
                
                case "var":
                    if (param.Contains("+"))
                    {
                        var subparts = param.Split('+');
                        var varName = subparts[0].Trim();
                        if (int.TryParse(subparts[1].Trim(), out int delta) && GameState.Instance != null)
                        {
                            GameState.Instance.AddInt(varName, delta);
                        }
                    }
                    break;
                
                case "ending":
                    if (GameState.Instance != null)
                        GameState.Instance.SetString("current_ending_path", param);
                    break;
                
                default:
                    Debug.LogWarning($"[DialogueManager] Unknown command: {cmd}");
                    break;
            }
        }
        
        private void GiveItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                Debug.LogWarning("[DialogueManager] Item command called with empty item ID");
                return;
            }
            
            // Check if InventoryManager exists (may not be implemented yet)
            if (Gameplay.InventoryManager.Instance != null)
            {
                Gameplay.InventoryManager.Instance.AddItem(itemId);
            }
            else
            {
                Debug.LogWarning("[DialogueManager] InventoryManager not found. Item command ignored.");
            }
        }
        
        private void EndDialogue()
        {
            if (!isDialogueActive) return;
            
            isDialogueActive = false;
            currentNode = null;
            currentTree = null;
            OnDialogueEnded?.Invoke();
            
            Debug.Log("[DialogueManager] Dialogue ended. Player control restored.");
        }
    }
}

