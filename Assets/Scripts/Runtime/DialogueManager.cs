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
        public event Action<string, int> OnImpactApplied;
        public event Action<string> OnItemGiven;
        public event Action<DialogueNode> OnChoiceSelected; // Fired when a choice is selected, before advancing
        
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
        /// Starts a dialogue tree at a specific node. Useful for segmented dialogue flows.
        /// </summary>
        /// <param name="tree">The dialogue tree to use. Must not be null.</param>
        /// <param name="startNode">The node to start from. Must not be null.</param>
        public void StartDialogueAtNode(DialogueTree tree, DialogueNode startNode)
        {
            if (tree == null)
            {
                Debug.LogError("[DialogueManager] Tried to start null dialogue tree");
                return;
            }

            if (startNode == null)
            {
                Debug.LogError("[DialogueManager] Tried to start dialogue with null start node");
                return;
            }

            currentTree = tree;
            isDialogueActive = true;
            ShowNode(startNode);
        }

        /// <summary>
        /// Starts a dialogue tree at a specific node by node ID. Searches for the node in the project.
        /// </summary>
        /// <param name="tree">The dialogue tree to use. Must not be null.</param>
        /// <param name="nodeId">The ID of the node to start from.</param>
        public void StartDialogueAtNodeId(DialogueTree tree, string nodeId)
        {
            if (tree == null)
            {
                Debug.LogError("[DialogueManager] Tried to start null dialogue tree");
                return;
            }

            if (string.IsNullOrWhiteSpace(nodeId))
            {
                Debug.LogError("[DialogueManager] Tried to start dialogue with empty node ID");
                return;
            }

            // Find the node by ID (search all DialogueNode assets)
            DialogueNode targetNode = FindNodeById(nodeId);
            
            if (targetNode == null)
            {
                Debug.LogError($"[DialogueManager] Could not find node with ID: {nodeId}");
                return;
            }

            StartDialogueAtNode(tree, targetNode);
        }

        /// <summary>
        /// Finds a dialogue node by its ID by searching all DialogueNode assets in the project.
        /// </summary>
        private DialogueNode FindNodeById(string nodeId)
        {
            // Search all DialogueNode assets in the project
            #if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:DialogueNode");
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                DialogueNode node = UnityEditor.AssetDatabase.LoadAssetAtPath<DialogueNode>(path);
                if (node != null && node.NodeId.Equals(nodeId, System.StringComparison.OrdinalIgnoreCase))
                {
                    return node;
                }
            }
            #endif

            // Fallback: search in Resources folder if needed
            DialogueNode[] allNodes = Resources.FindObjectsOfTypeAll<DialogueNode>();
            foreach (var node in allNodes)
            {
                if (node.NodeId.Equals(nodeId, System.StringComparison.OrdinalIgnoreCase))
                {
                    return node;
                }
            }

            return null;
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
            
            // Notify subscribers that a choice was selected (before advancing)
            OnChoiceSelected?.Invoke(currentNode);
            
            // Apply impacts before moving to next node
            ApplyImpacts(choice.Impacts);
            
            // Execute end commands from current node
            foreach (var cmd in currentNode.EndCommands)
                ExecuteCommand(cmd);
            
            // Check if next node is null (segment boundary)
            if (choice.NextNode == null)
            {
                Debug.Log($"[DialogueManager] Choice leads to null node (segment boundary). Ending dialogue.");
                EndDialogue();
                return;
            }
            
            // Check if we're trying to advance to a node that might be in a different segment
            // This is a safety check - if the current tree's start node is different from where we're going,
            // it might indicate a segment boundary
            // Note: This is a heuristic - the proper solution is to set choices to null or mark node as End Node
            
            // Move to next node
            ShowNode(choice.NextNode);
        }
        
        /// <summary>
        /// Advances to the next node automatically (for nodes with no choices or auto-advance enabled).
        /// </summary>
        public void AdvanceToNextNode()
        {
            if (!isDialogueActive || currentNode == null) return;
            
            // Check if there are any visible choices (not just total choices)
            var visibleChoices = GetVisibleChoices();
            if (visibleChoices.Count > 0)
            {
                Debug.Log($"[DialogueManager] Cannot auto-advance: node has {visibleChoices.Count} visible choices. Waiting for player input.");
                return;
            }
            
            // No visible choices, check if there's an auto-advance node
            if (currentNode.NextNodeIfAuto == null)
            {
                Debug.Log($"[DialogueManager] No visible choices and no NextNodeIfAuto. Ending dialogue.");
                EndDialogue();
                return;
            }
            
            // Execute end commands before advancing
            foreach (var cmd in currentNode.EndCommands)
                ExecuteCommand(cmd);
                
            Debug.Log($"[DialogueManager] Auto-advancing to node: {currentNode.NextNodeIfAuto.NodeId}");
            ShowNode(currentNode.NextNodeIfAuto);
        }
        
        public bool IsDialogueActive => isDialogueActive;
        public DialogueNode CurrentNode => currentNode;
        
        /// <summary>
        /// Gets list of visible choices (filtered by conditions) for the current node.
        /// </summary>
        public List<DialogueChoice> GetVisibleChoices()
        {
            var visibleChoices = new List<DialogueChoice>();
            
            if (currentNode == null)
            {
                Debug.LogWarning("[DialogueManager] GetVisibleChoices called but currentNode is null");
                return visibleChoices;
            }

            if (GameState.Instance == null)
            {
                Debug.LogWarning("[DialogueManager] GetVisibleChoices called but GameState.Instance is null. All choices will be hidden.");
                return visibleChoices;
            }
            
            Debug.Log($"[DialogueManager] Checking {currentNode.Choices.Count} choices for node '{currentNode.NodeId}'");
            
            foreach (var choice in currentNode.Choices)
            {
                if (!choice.HasCondition)
                {
                    // No condition, always show
                    visibleChoices.Add(choice);
                    Debug.Log($"[DialogueManager] Choice '{choice.ChoiceText}' is visible (no condition)");
                }
                else
                {
                    // Has condition, check if it's met
                    bool conditionMet = GameState.Instance.EvaluateCondition(choice.ShowCondition);
                    if (conditionMet)
                    {
                        visibleChoices.Add(choice);
                        Debug.Log($"[DialogueManager] Choice '{choice.ChoiceText}' is visible (condition '{choice.ShowCondition}' met)");
                    }
                    else
                    {
                        Debug.Log($"[DialogueManager] Choice '{choice.ChoiceText}' is HIDDEN (condition '{choice.ShowCondition}' not met)");
                    }
                }
            }
            
            Debug.Log($"[DialogueManager] Total visible choices: {visibleChoices.Count} out of {currentNode.Choices.Count}");
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
            
            // Auto-end if this is an end node AND has no choices
            // If it has choices, wait for player to make a choice first
            if (node.IsEndNode)
            {
                if (visibleChoiceCount == 0)
                {
                    // No choices, auto-end after delay
                    float delay = node.DisplayDuration > 0 ? node.DisplayDuration : 3f;
                    Debug.Log($"[DialogueManager] Node '{node.NodeId}' is an end node with no choices. Ending dialogue in {delay} seconds.");
                    Invoke(nameof(EndDialogue), delay);
                }
                else
                {
                    // Has choices, don't auto-end - wait for player choice
                    Debug.Log($"[DialogueManager] Node '{node.NodeId}' is an end node but has {visibleChoiceCount} choices. Waiting for player input.");
                }
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
                
                // Notify subscribers about the impact
                OnImpactApplied?.Invoke(impact.VariableName, impact.ValueChange);
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
                OnItemGiven?.Invoke(itemId);
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

