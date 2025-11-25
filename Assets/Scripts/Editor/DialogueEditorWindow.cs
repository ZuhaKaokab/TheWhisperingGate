using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using WhisperingGate.Dialogue;

namespace WhisperingGate.Editor
{
    /// <summary>
    /// Main editor window for creating and editing dialogue trees visually.
    /// Provides node-based graph view, inspector panel, and real-time preview.
    /// </summary>
    public class DialogueEditorWindow : EditorWindow
    {
        private DialogueTree currentTree;
        private DialogueNode selectedNode;
        private SerializedObject selectedNodeSO;
        private SerializedObject currentTreeSO;
        private Vector2 graphScrollPos;
        private Vector2 inspectorScrollPos;
        private Dictionary<DialogueNode, Rect> nodeRects = new();
        private Dictionary<DialogueNode, Vector2> nodePositions = new();
        private bool isDragging = false;
        private DialogueNode draggedNode;
        private Vector2 dragOffset;
        
        private const float NODE_WIDTH = 200f;
        private const float NODE_HEIGHT = 100f;
        private const float INSPECTOR_WIDTH = 350f;
        private const float STATUS_HEIGHT = 30f;
        
        [MenuItem("Window/Whispering Gate/Dialogue Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<DialogueEditorWindow>("Dialogue Editor");
            window.minSize = new Vector2(1200, 700);
            window.maxSize = new Vector2(4096, 4096);
            
            // Set reasonable default size
            if (window.position.width < 1200)
                window.position = new Rect(window.position.x, window.position.y, 1200, 800);
        }
        
        void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        
        void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }
        
        void OnGUI()
        {
            DrawToolbar();
            
            if (currentTree == null)
            {
                DrawEmptyState();
                return;
            }
            
            // Ensure inspector width is reasonable
            float inspectorWidth = Mathf.Clamp(INSPECTOR_WIDTH, 300, position.width * 0.4f);
            float graphWidth = position.width - inspectorWidth;
            
            Rect graphRect = new Rect(0, 20, graphWidth, position.height - STATUS_HEIGHT);
            Rect inspectorRect = new Rect(graphWidth, 20, inspectorWidth, position.height - STATUS_HEIGHT);
            Rect statusRect = new Rect(0, position.height - STATUS_HEIGHT, position.width, STATUS_HEIGHT);
            
            DrawGraphView(graphRect);
            DrawInspectorPanel(inspectorRect);
            DrawStatusBar(statusRect);
            
            HandleEvents();
        }
        
        private bool showAllNodes = false;
        
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            if (GUILayout.Button("New Tree", EditorStyles.toolbarButton))
            {
                CreateNewTree();
            }
            
            if (GUILayout.Button("Load Tree", EditorStyles.toolbarButton))
            {
                LoadTree();
            }
            
            GUILayout.Space(10);
            
            EditorGUI.BeginChangeCheck();
            currentTree = (DialogueTree)EditorGUILayout.ObjectField(currentTree, typeof(DialogueTree), false, GUILayout.Width(200));
            if (EditorGUI.EndChangeCheck())
            {
                selectedNode = null;
                selectedNodeSO = null;
                if (currentTree != null)
                    currentTreeSO = new SerializedObject(currentTree);
                RefreshNodePositions();
            }
            
            GUILayout.Space(10);
            
            if (currentTree != null)
            {
                EditorGUI.BeginChangeCheck();
                showAllNodes = GUILayout.Toggle(showAllNodes, "Show All Nodes", EditorStyles.toolbarButton);
                if (EditorGUI.EndChangeCheck())
                {
                    RefreshNodePositions();
                    Repaint();
                }
            }
            
            GUILayout.FlexibleSpace();
            
            if (currentTree != null)
            {
                if (GUILayout.Button("Preview", EditorStyles.toolbarButton))
                {
                    PreviewDialogue();
                }
                
                if (GUILayout.Button("Validate", EditorStyles.toolbarButton))
                {
                    ValidateTree();
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawEmptyState()
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            GUILayout.BeginVertical();
            GUILayout.Label("No Dialogue Tree Selected", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Space(10);
            if (GUILayout.Button("Create New Tree", GUILayout.Width(150)))
            {
                CreateNewTree();
            }
            GUILayout.EndVertical();
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }
        
        private void DrawGraphView(Rect rect)
        {
            GUI.Box(rect, "", EditorStyles.helpBox);
            
            graphScrollPos = GUI.BeginScrollView(rect, graphScrollPos, new Rect(0, 0, 2000, 2000));
            
            // Draw grid background
            DrawGrid(rect);
            
            // Draw connections first (behind nodes)
            DrawConnections();
            
            // Draw nodes
            DrawNodes();
            
            GUI.EndScrollView();
            
            // Handle context menu
            if (Event.current.type == EventType.ContextClick && rect.Contains(Event.current.mousePosition))
            {
                ShowContextMenu(Event.current.mousePosition);
            }
        }
        
        private void DrawGrid(Rect rect)
        {
            float gridSize = 20f;
            Handles.BeginGUI();
            Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
            
            for (float x = 0; x < rect.width; x += gridSize)
            {
                Handles.DrawLine(new Vector3(x, 0), new Vector3(x, rect.height));
            }
            
            for (float y = 0; y < rect.height; y += gridSize)
            {
                Handles.DrawLine(new Vector3(0, y), new Vector3(rect.width, y));
            }
            
            Handles.EndGUI();
        }
        
        private void DrawConnections()
        {
            if (currentTree == null || currentTree.StartNode == null) return;
            
            Handles.BeginGUI();
            
            // Draw connections from start node
            DrawNodeConnections(currentTree.StartNode);
            
            // Draw connections from all nodes
            var allNodes = GetAllNodes();
            foreach (var node in allNodes)
            {
                DrawNodeConnections(node);
            }
            
            Handles.EndGUI();
        }
        
        private void DrawNodeConnections(DialogueNode node)
        {
            if (node == null) return;
            
            Vector2 nodePos = GetNodePosition(node);
            Vector2 nodeCenter = nodePos + new Vector2(NODE_WIDTH / 2, NODE_HEIGHT / 2);
            
            // Draw connection to auto-advance node
            if (node.NextNodeIfAuto != null)
            {
                Vector2 targetPos = GetNodePosition(node.NextNodeIfAuto);
                Vector2 targetCenter = targetPos + new Vector2(NODE_WIDTH / 2, NODE_HEIGHT / 2);
                DrawConnection(nodeCenter, targetCenter, Color.cyan, true);
            }
            
            // Draw connections from choices
            foreach (var choice in node.Choices)
            {
                if (choice.NextNode != null)
                {
                    Vector2 targetPos = GetNodePosition(choice.NextNode);
                    Vector2 targetCenter = targetPos + new Vector2(NODE_WIDTH / 2, NODE_HEIGHT / 2);
                    Color lineColor = choice.HasCondition ? Color.yellow : Color.white;
                    DrawConnection(nodeCenter, targetCenter, lineColor, false);
                }
            }
        }
        
        private void DrawConnection(Vector2 from, Vector2 to, Color color, bool isDashed)
        {
            Handles.color = color;
            if (isDashed)
            {
                Handles.DrawDottedLine(from, to, 5f);
            }
            else
            {
                Handles.DrawLine(from, to);
            }
            
            // Draw arrow
            Vector2 direction = (to - from).normalized;
            Vector2 arrowBase = to - direction * 20f;
            Vector2 arrowRight = arrowBase + new Vector2(-direction.y, direction.x) * 5f;
            Vector2 arrowLeft = arrowBase - new Vector2(-direction.y, direction.x) * 5f;
            
            Handles.DrawLine(to, arrowRight);
            Handles.DrawLine(to, arrowLeft);
        }
        
        private void DrawNodes()
        {
            if (currentTree == null) return;
            
            List<DialogueNode> nodesToShow;
            
            if (showAllNodes)
            {
                // Show all nodes in project (for easier node management)
                nodesToShow = GetAllNodesInProject();
            }
            else
            {
                // Show only connected nodes (default behavior)
                nodesToShow = GetAllNodes();
            }
            
            // Mark which nodes are actually connected
            var connectedNodes = new HashSet<DialogueNode>(GetAllNodes());
            
            foreach (var node in nodesToShow)
            {
                if (!nodePositions.ContainsKey(node))
                {
                    nodePositions[node] = new Vector2(UnityEngine.Random.Range(100, 500), UnityEngine.Random.Range(100, 400));
                }
                
                Vector2 pos = nodePositions[node];
                Rect nodeRect = new Rect(pos.x, pos.y, NODE_WIDTH, NODE_HEIGHT);
                nodeRects[node] = nodeRect;
                
                // Determine node color
                Color bgColor = Color.gray;
                if (node == currentTree.StartNode)
                    bgColor = Color.green;
                else if (node.IsEndNode)
                    bgColor = Color.red;
                else if (selectedNode == node)
                    bgColor = Color.yellow;
                else if (!connectedNodes.Contains(node) && showAllNodes)
                    bgColor = new Color(0.5f, 0.5f, 0.5f, 0.7f); // Grayed out for unconnected nodes
                
                // Draw node
                GUI.backgroundColor = bgColor;
                GUI.Box(nodeRect, "", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
                
                // Draw node content
                GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
                labelStyle.wordWrap = true;
                labelStyle.alignment = TextAnchor.MiddleCenter;
                
                string nodeText = string.IsNullOrEmpty(node.NodeId) ? "Node" : node.NodeId;
                if (node.Speaker != null)
                    nodeText += $"\n({node.Speaker.DisplayName})";
                
                // Add indicator for unconnected nodes
                if (!connectedNodes.Contains(node) && showAllNodes)
                    nodeText += "\n[Unlinked]";
                
                GUI.Label(new Rect(nodeRect.x + 5, nodeRect.y + 5, nodeRect.width - 10, nodeRect.height - 10), 
                    nodeText, labelStyle);
            }
        }
        
        private void DrawInspectorPanel(Rect rect)
        {
            GUI.Box(rect, "", EditorStyles.helpBox);
            
            // Calculate content height dynamically
            float contentHeight = selectedNode != null ? 1500f : 400f;
            
            inspectorScrollPos = GUI.BeginScrollView(
                new Rect(rect.x, rect.y, rect.width, rect.height), 
                inspectorScrollPos, 
                new Rect(0, 0, rect.width - 20, contentHeight),
                false, 
                true
            );
            
            GUILayout.BeginArea(new Rect(0, 0, rect.width - 20, contentHeight));
            
            if (selectedNode != null)
            {
                DrawNodeInspector();
            }
            else
            {
                DrawTreeInspector();
            }
            
            GUILayout.EndArea();
            GUI.EndScrollView();
        }
        
        private void DrawNodeInspector()
        {
            if (selectedNode == null) return;
            
            if (selectedNodeSO == null || selectedNodeSO.targetObject != selectedNode)
                selectedNodeSO = new SerializedObject(selectedNode);
            
            selectedNodeSO.Update();
            
            EditorGUILayout.LabelField("Node Properties", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Node ID
            EditorGUILayout.PropertyField(selectedNodeSO.FindProperty("nodeId"), new GUIContent("Node ID"));
            
            // Speaker
            EditorGUILayout.PropertyField(selectedNodeSO.FindProperty("speaker"), new GUIContent("Speaker"));
            
            // Line Text
            SerializedProperty lineTextProp = selectedNodeSO.FindProperty("lineText");
            EditorGUILayout.LabelField("Line Text");
            lineTextProp.stringValue = EditorGUILayout.TextArea(lineTextProp.stringValue, GUILayout.Height(60));
            
            // Voice Clip
            EditorGUILayout.PropertyField(selectedNodeSO.FindProperty("voiceClip"), new GUIContent("Voice Clip"));
            
            // Voice Delay
            EditorGUILayout.PropertyField(selectedNodeSO.FindProperty("voiceDelay"), new GUIContent("Voice Delay"));
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Choices", EditorStyles.boldLabel);
            
            // Draw choices using SerializedProperty
            SerializedProperty choicesProp = selectedNodeSO.FindProperty("choices");
            if (choicesProp != null)
            {
                for (int i = 0; i < choicesProp.arraySize; i++)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    SerializedProperty choiceProp = choicesProp.GetArrayElementAtIndex(i);
                    
                    EditorGUILayout.LabelField($"Choice {i + 1}", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(choiceProp.FindPropertyRelative("choiceText"), new GUIContent("Text"));
                    EditorGUILayout.PropertyField(choiceProp.FindPropertyRelative("nextNode"), new GUIContent("Next Node"));
                    EditorGUILayout.PropertyField(choiceProp.FindPropertyRelative("hasCondition"), new GUIContent("Has Condition"));
                    
                    if (choiceProp.FindPropertyRelative("hasCondition").boolValue)
                    {
                        EditorGUILayout.PropertyField(choiceProp.FindPropertyRelative("showCondition"), new GUIContent("Condition"));
                    }
                    
                    // Impacts
                    SerializedProperty impactsProp = choiceProp.FindPropertyRelative("impacts");
                    if (impactsProp != null)
                    {
                        EditorGUILayout.LabelField("Impacts", EditorStyles.miniLabel);
                        for (int j = 0; j < impactsProp.arraySize; j++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            SerializedProperty impactProp = impactsProp.GetArrayElementAtIndex(j);
                            EditorGUILayout.PropertyField(impactProp.FindPropertyRelative("variableName"), GUIContent.none);
                            EditorGUILayout.PropertyField(impactProp.FindPropertyRelative("valueChange"), GUIContent.none);
                            if (GUILayout.Button("X", GUILayout.Width(20)))
                            {
                                impactsProp.DeleteArrayElementAtIndex(j);
                                break;
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        if (GUILayout.Button("Add Impact", EditorStyles.miniButton))
                        {
                            impactsProp.arraySize++;
                        }
                    }
                    
                    // Remove choice button
                    if (GUILayout.Button("Remove Choice", EditorStyles.miniButton))
                    {
                        choicesProp.DeleteArrayElementAtIndex(i);
                        break;
                    }
                    
                    EditorGUILayout.EndVertical();
                }
                
                if (GUILayout.Button("Add Choice"))
                {
                    choicesProp.arraySize++;
                }
            }
            
            EditorGUILayout.Space();
            
            // Next Node If Auto
            EditorGUILayout.PropertyField(selectedNodeSO.FindProperty("nextNodeIfAuto"), new GUIContent("Next Node (Auto)"));
            
            // Commands
            SerializedProperty startCommandsProp = selectedNodeSO.FindProperty("startCommands");
            if (startCommandsProp != null)
            {
                EditorGUILayout.LabelField("Start Commands", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(startCommandsProp, true);
            }
            
            SerializedProperty endCommandsProp = selectedNodeSO.FindProperty("endCommands");
            if (endCommandsProp != null)
            {
                EditorGUILayout.LabelField("End Commands", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(endCommandsProp, true);
            }
            
            // Is End Node
            EditorGUILayout.PropertyField(selectedNodeSO.FindProperty("isEndNode"), new GUIContent("Is End Node"));
            
            // Display Duration
            EditorGUILayout.PropertyField(selectedNodeSO.FindProperty("displayDuration"), new GUIContent("Display Duration"));
            
            selectedNodeSO.ApplyModifiedProperties();
            
            // Delete node button
            EditorGUILayout.Space();
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Delete Node"))
            {
                if (EditorUtility.DisplayDialog("Delete Node", $"Delete node '{selectedNode.NodeId}'?", "Yes", "No"))
                {
                    DeleteNode(selectedNode);
                }
            }
            GUI.backgroundColor = Color.white;
        }
        
        private void DrawTreeInspector()
        {
            if (currentTree == null) return;
            
            if (currentTreeSO == null || currentTreeSO.targetObject != currentTree)
                currentTreeSO = new SerializedObject(currentTree);
            
            currentTreeSO.Update();
            
            EditorGUILayout.LabelField("Tree Properties", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(currentTreeSO.FindProperty("treeId"), new GUIContent("Tree ID"));
            EditorGUILayout.PropertyField(currentTreeSO.FindProperty("treeTitle"), new GUIContent("Tree Title"));
            EditorGUILayout.PropertyField(currentTreeSO.FindProperty("startNode"), new GUIContent("Start Node"));
            EditorGUILayout.PropertyField(currentTreeSO.FindProperty("defaultTypewriterSpeed"), new GUIContent("Typewriter Speed"));
            EditorGUILayout.PropertyField(currentTreeSO.FindProperty("autoAdvanceIfSingleChoice"), new GUIContent("Auto Advance Single Choice"));
            
            currentTreeSO.ApplyModifiedProperties();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Statistics", EditorStyles.boldLabel);
            var allNodes = GetAllNodes();
            EditorGUILayout.LabelField($"Total Nodes: {allNodes.Count}");
            EditorGUILayout.LabelField($"End Nodes: {allNodes.Count(n => n.IsEndNode)}");
            EditorGUILayout.LabelField($"Start Node: {(currentTree.StartNode != null ? currentTree.StartNode.NodeId : "None")}");
        }
        
        private void DrawStatusBar(Rect rect)
        {
            GUI.Box(rect, "", EditorStyles.toolbar);
            GUILayout.BeginArea(rect);
            GUILayout.BeginHorizontal();
            
            if (currentTree != null)
            {
                var allNodes = GetAllNodes();
                GUILayout.Label($"Nodes: {allNodes.Count} | Selected: {(selectedNode != null ? selectedNode.NodeId : "None")}", EditorStyles.miniLabel);
            }
            else
            {
                GUILayout.Label("No tree loaded", EditorStyles.miniLabel);
            }
            
            GUILayout.FlexibleSpace();
            
            if (currentTree != null && Application.isPlaying)
            {
                GUI.color = Color.green;
                GUILayout.Label("● Play Mode", EditorStyles.miniLabel);
                GUI.color = Color.white;
            }
            
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
        
        private void HandleEvents()
        {
            Event e = Event.current;
            
            // Handle node selection
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                selectedNode = null;
                foreach (var kvp in nodeRects)
                {
                    if (kvp.Value.Contains(e.mousePosition - graphScrollPos))
                    {
                        selectedNode = kvp.Key;
                        selectedNodeSO = new SerializedObject(selectedNode);
                        isDragging = true;
                        draggedNode = kvp.Key;
                        dragOffset = e.mousePosition - nodePositions[kvp.Key];
                        e.Use();
                        break;
                    }
                }
                Repaint();
            }
            
            // Handle node dragging
            if (isDragging && draggedNode != null && e.type == EventType.MouseDrag)
            {
                nodePositions[draggedNode] = e.mousePosition - dragOffset - graphScrollPos;
                e.Use();
                Repaint();
            }
            
            if (e.type == EventType.MouseUp)
            {
                isDragging = false;
                draggedNode = null;
            }
        }
        
        private void ShowContextMenu(Vector2 position)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Create Node"), false, () => CreateNodeAtPosition(position + graphScrollPos));
            menu.AddItem(new GUIContent("Create Start Node"), false, () => {
                CreateNodeAtPosition(position + graphScrollPos);
                if (currentTree != null && currentTree.StartNode == null && selectedNode != null)
                {
                    if (currentTreeSO == null)
                        currentTreeSO = new SerializedObject(currentTree);
                    currentTreeSO.Update();
                    currentTreeSO.FindProperty("startNode").objectReferenceValue = selectedNode;
                    currentTreeSO.ApplyModifiedProperties();
                    EditorUtility.SetDirty(currentTree);
                }
            });
            menu.ShowAsContext();
        }
        
        private void CreateNodeAtPosition(Vector2 position)
        {
            if (currentTree == null) return;
            
            DialogueNode newNode = CreateInstance<DialogueNode>();
            newNode.name = "NewNode";
            
            // Auto-generate path if in a dialogue folder
            string defaultPath = "Assets/Dialogue/Nodes/NewNode.asset";
            string folderPath = "Assets/Dialogue/Nodes";
            
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string assetsFolder = "Assets";
                if (!AssetDatabase.IsValidFolder("Assets/Dialogue"))
                {
                    AssetDatabase.CreateFolder(assetsFolder, "Dialogue");
                }
                if (!AssetDatabase.IsValidFolder(folderPath))
                {
                    AssetDatabase.CreateFolder("Assets/Dialogue", "Nodes");
                }
            }
            
            // Find unique name
            string uniquePath = AssetDatabase.GenerateUniqueAssetPath(defaultPath);
            AssetDatabase.CreateAsset(newNode, uniquePath);
            AssetDatabase.SaveAssets();
            
            // Store position relative to scroll view
            nodePositions[newNode] = position;
            
            // If no start node, set this as start
            if (currentTree.StartNode == null)
            {
                Undo.RecordObject(currentTree, "Set Start Node");
                if (currentTreeSO == null)
                    currentTreeSO = new SerializedObject(currentTree);
                currentTreeSO.Update();
                currentTreeSO.FindProperty("startNode").objectReferenceValue = newNode;
                currentTreeSO.ApplyModifiedProperties();
                EditorUtility.SetDirty(currentTree);
            }
            
            selectedNode = newNode;
            selectedNodeSO = new SerializedObject(newNode);
            Repaint();
        }
        
        private void CreateNewTree()
        {
            DialogueTree newTree = CreateInstance<DialogueTree>();
            newTree.name = "NewDialogueTree";
            
            string path = EditorUtility.SaveFilePanelInProject("Save Tree", "NewDialogueTree", "asset", "Save dialogue tree");
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(newTree, path);
                AssetDatabase.SaveAssets();
                currentTree = newTree;
                selectedNode = null;
                RefreshNodePositions();
                Repaint();
            }
        }
        
        private void LoadTree()
        {
            string path = EditorUtility.OpenFilePanel("Load Dialogue Tree", "Assets", "asset");
            if (!string.IsNullOrEmpty(path))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
                currentTree = AssetDatabase.LoadAssetAtPath<DialogueTree>(path);
                selectedNode = null;
                RefreshNodePositions();
                Repaint();
            }
        }
        
        private void DeleteNode(DialogueNode node)
        {
            if (node == null) return;
            
            // Remove from tree if it's the start node
            if (currentTree.StartNode == node)
            {
                Undo.RecordObject(currentTree, "Clear Start Node");
                currentTree.GetType().GetField("startNode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(currentTree, null);
                EditorUtility.SetDirty(currentTree);
            }
            
            // Remove references from other nodes
            var allNodes = GetAllNodes();
            foreach (var n in allNodes)
            {
                if (n == node) continue;
                
                bool changed = false;
                
                // Check auto-advance
                if (n.NextNodeIfAuto == node)
                {
                    Undo.RecordObject(n, "Clear Auto-Advance");
                    n.GetType().GetField("nextNodeIfAuto", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        ?.SetValue(n, null);
                    changed = true;
                }
                
                // Check choices
                foreach (var choice in n.Choices)
                {
                    if (choice.NextNode == node)
                    {
                        Undo.RecordObject(n, "Clear Choice Next Node");
                        choice.GetType().GetField("nextNode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            ?.SetValue(choice, null);
                        changed = true;
                    }
                }
                
                if (changed)
                    EditorUtility.SetDirty(n);
            }
            
            nodePositions.Remove(node);
            nodeRects.Remove(node);
            selectedNode = null;
            
            // Delete asset
            string assetPath = AssetDatabase.GetAssetPath(node);
            AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.SaveAssets();
            
            Repaint();
        }
        
        private void PreviewDialogue()
        {
            if (currentTree == null || !Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Preview", "Please enter Play Mode first, then click Preview.", "OK");
                return;
            }
            
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(currentTree);
            }
            else
            {
                EditorUtility.DisplayDialog("Preview", "DialogueManager not found in scene. Make sure it exists and is active.", "OK");
            }
        }
        
        private void ValidateTree()
        {
            if (currentTree == null) return;
            
            var issues = new List<string>();
            var allNodes = GetAllNodes();
            
            if (currentTree.StartNode == null)
                issues.Add("No start node assigned");
            
            foreach (var node in allNodes)
            {
                if (string.IsNullOrEmpty(node.NodeId))
                    issues.Add($"Node at {AssetDatabase.GetAssetPath(node)} has no ID");
                
                if (string.IsNullOrEmpty(node.LineText))
                    issues.Add($"Node '{node.NodeId}' has no line text");
                
                if (!node.IsEndNode && node.Choices.Count == 0 && node.NextNodeIfAuto == null)
                    issues.Add($"Node '{node.NodeId}' has no outgoing connections");
            }
            
            if (issues.Count == 0)
            {
                EditorUtility.DisplayDialog("Validation", "Tree is valid! ✓", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Validation Issues", string.Join("\n", issues), "OK");
            }
        }
        
        private List<DialogueNode> GetAllNodes()
        {
            var nodes = new List<DialogueNode>();
            if (currentTree == null) return nodes;
            
            var visited = new HashSet<DialogueNode>();
            var toVisit = new Queue<DialogueNode>();
            
            // Start from the start node
            if (currentTree.StartNode != null)
                toVisit.Enqueue(currentTree.StartNode);
            
            // Traverse all connected nodes
            while (toVisit.Count > 0)
            {
                var node = toVisit.Dequeue();
                if (visited.Contains(node) || node == null) continue;
                
                visited.Add(node);
                nodes.Add(node);
                
                if (node.NextNodeIfAuto != null && !visited.Contains(node.NextNodeIfAuto))
                    toVisit.Enqueue(node.NextNodeIfAuto);
                
                foreach (var choice in node.Choices)
                {
                    if (choice.NextNode != null && !visited.Contains(choice.NextNode))
                        toVisit.Enqueue(choice.NextNode);
                }
            }
            
            return nodes;
        }
        
        private List<DialogueNode> GetAllNodesInProject()
        {
            var allNodes = new List<DialogueNode>();
            string[] guids = AssetDatabase.FindAssets("t:DialogueNode");
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                DialogueNode node = AssetDatabase.LoadAssetAtPath<DialogueNode>(path);
                if (node != null)
                    allNodes.Add(node);
            }
            
            return allNodes;
        }
        
        private Vector2 GetNodePosition(DialogueNode node)
        {
            if (nodePositions.ContainsKey(node))
                return nodePositions[node];
            return Vector2.zero;
        }
        
        private void RefreshNodePositions()
        {
            if (currentTree == null) return;
            
            List<DialogueNode> nodesToPosition;
            
            if (showAllNodes)
            {
                nodesToPosition = GetAllNodesInProject();
            }
            else
            {
                nodesToPosition = GetAllNodes();
            }
            
            // Only set positions for nodes that don't have one yet
            const float spacing = 250f;
            int nodesPerRow = 5;
            int currentRow = 0;
            int currentCol = 0;
            
            foreach (var node in nodesToPosition)
            {
                // Only set position if node doesn't have one
                if (!nodePositions.ContainsKey(node))
                {
                    float x = 100f + (currentCol * spacing);
                    float y = 100f + (currentRow * spacing);
                    nodePositions[node] = new Vector2(x, y);
                }
                
                currentCol++;
                if (currentCol >= nodesPerRow)
                {
                    currentCol = 0;
                    currentRow++;
                }
            }
        }
        
        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            Repaint();
        }
    }
}

