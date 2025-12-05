using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using WhisperingGate.Dialogue;

namespace WhisperingGate.Editor
{
    /// <summary>
    /// Modern visual dialogue editor with node-based graph view.
    /// Features zoom, pan, dark theme, and persistent node layouts.
    /// </summary>
    public class DialogueEditorWindow : EditorWindow
    {
        private DialogueTree currentTree;
        private string currentTreeGUID;
        private DialogueNode selectedNode;
        private SerializedObject selectedNodeSO;
        private SerializedObject currentTreeSO;
        private Vector2 graphOffset = Vector2.zero;
        private Vector2 inspectorScrollPos;
        private Dictionary<DialogueNode, Rect> nodeRects = new();
        private Dictionary<DialogueNode, Vector2> nodePositions = new();
        private bool isDragging = false;
        private bool isPanning = false;
        private DialogueNode draggedNode;
        private Vector2 dragOffset;
        private Rect graphViewRect;
        
        // Zoom settings
        private float zoomLevel = 1f;
        private const float MIN_ZOOM = 0.2f;
        private const float MAX_ZOOM = 2f;
        private const float ZOOM_STEP = 0.1f;
        
        // Canvas settings
        private const float NODE_WIDTH = 220f;
        private const float NODE_HEIGHT = 90f;
        private const float INSPECTOR_WIDTH = 400f;
        private const float TOOLBAR_HEIGHT = 25f;
        private const float STATUS_HEIGHT = 24f;
        private const float GRID_SIZE = 20f;
        private const float GRID_SIZE_LARGE = 100f;
        
        // Persistence key prefix
        private const string PREFS_PREFIX = "DialogueEditor_";
        
        // Modern color palette (dark theme)
        private static readonly Color BG_COLOR = new Color(0.12f, 0.12f, 0.14f);
        private static readonly Color GRID_COLOR_SMALL = new Color(1f, 1f, 1f, 0.03f);
        private static readonly Color GRID_COLOR_LARGE = new Color(1f, 1f, 1f, 0.06f);
        private static readonly Color NODE_BG = new Color(0.22f, 0.22f, 0.25f);
        private static readonly Color NODE_BORDER = new Color(0.35f, 0.35f, 0.38f);
        private static readonly Color NODE_SELECTED = new Color(0.4f, 0.7f, 1f, 0.9f);
        private static readonly Color NODE_START = new Color(0.3f, 0.8f, 0.4f, 0.9f);
        private static readonly Color NODE_END = new Color(0.9f, 0.35f, 0.35f, 0.9f);
        private static readonly Color PANEL_BG = new Color(0.16f, 0.16f, 0.18f);
        private static readonly Color CONNECTION_COLOR = new Color(0.7f, 0.7f, 0.7f, 0.8f);
        private static readonly Color CONNECTION_AUTO = new Color(0.4f, 0.85f, 0.95f, 0.9f);
        private static readonly Color CONNECTION_CONDITION = new Color(1f, 0.85f, 0.3f, 0.9f);
        
        // Cached styles
        private GUIStyle nodeTitleStyle;
        private GUIStyle nodeSubtitleStyle;
        private GUIStyle panelHeaderStyle;
        private GUIStyle statusLabelStyle;
        private bool stylesInitialized = false;
        private bool showAllNodes = false;
        
        // Cached data for performance
        private List<DialogueNode> cachedNodes;
        private HashSet<DialogueNode> cachedConnectedNodes;
        private bool nodesCacheDirty = true;
        
        [MenuItem("Window/Whispering Gate/Dialogue Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<DialogueEditorWindow>("Dialogue Editor");
            window.minSize = new Vector2(1200, 700);
            window.wantsMouseMove = true;
            window.Show();
        }
        
        void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            Undo.undoRedoPerformed += OnUndoRedo;
            stylesInitialized = false;
            nodesCacheDirty = true;
            
            // Try to restore last opened tree
            string lastTreeGUID = EditorPrefs.GetString(PREFS_PREFIX + "LastTree", "");
            if (!string.IsNullOrEmpty(lastTreeGUID))
            {
                string path = AssetDatabase.GUIDToAssetPath(lastTreeGUID);
                if (!string.IsNullOrEmpty(path))
                {
                    currentTree = AssetDatabase.LoadAssetAtPath<DialogueTree>(path);
                    if (currentTree != null)
                    {
                        currentTreeGUID = lastTreeGUID;
                        currentTreeSO = new SerializedObject(currentTree);
                        LoadNodePositions();
                        LoadViewState();
                    }
                }
            }
        }
        
        void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            Undo.undoRedoPerformed -= OnUndoRedo;
            
            // Save on close
            SaveNodePositions();
            SaveViewState();
        }
        
        private void OnUndoRedo()
        {
            InvalidateNodeCache();
            Repaint();
        }
        
        #region Position Persistence
        
        private void SaveNodePositions()
        {
            if (currentTree == null || string.IsNullOrEmpty(currentTreeGUID)) return;
            
            // Build position data string: "nodeGUID:x:y|nodeGUID:x:y|..."
            List<string> positionData = new List<string>();
            
            foreach (var kvp in nodePositions)
            {
                if (kvp.Key == null) continue;
                
                string nodePath = AssetDatabase.GetAssetPath(kvp.Key);
                string nodeGUID = AssetDatabase.AssetPathToGUID(nodePath);
                
                if (!string.IsNullOrEmpty(nodeGUID))
                {
                    positionData.Add($"{nodeGUID}:{kvp.Value.x:F1}:{kvp.Value.y:F1}");
                }
            }
            
            string dataString = string.Join("|", positionData);
            EditorPrefs.SetString(PREFS_PREFIX + "Positions_" + currentTreeGUID, dataString);
            EditorPrefs.SetString(PREFS_PREFIX + "LastTree", currentTreeGUID);
        }
        
        private void LoadNodePositions()
        {
            if (currentTree == null || string.IsNullOrEmpty(currentTreeGUID)) return;
            
            string dataString = EditorPrefs.GetString(PREFS_PREFIX + "Positions_" + currentTreeGUID, "");
            if (string.IsNullOrEmpty(dataString)) return;
            
            nodePositions.Clear();
            
            string[] entries = dataString.Split('|');
            foreach (string entry in entries)
            {
                string[] parts = entry.Split(':');
                if (parts.Length != 3) continue;
                
                string nodeGUID = parts[0];
                if (!float.TryParse(parts[1], out float x)) continue;
                if (!float.TryParse(parts[2], out float y)) continue;
                
                string nodePath = AssetDatabase.GUIDToAssetPath(nodeGUID);
                if (string.IsNullOrEmpty(nodePath)) continue;
                
                DialogueNode node = AssetDatabase.LoadAssetAtPath<DialogueNode>(nodePath);
                if (node != null)
                {
                    nodePositions[node] = new Vector2(x, y);
                }
            }
        }
        
        private void SaveViewState()
        {
            if (string.IsNullOrEmpty(currentTreeGUID)) return;
            
            EditorPrefs.SetFloat(PREFS_PREFIX + "Zoom_" + currentTreeGUID, zoomLevel);
            EditorPrefs.SetFloat(PREFS_PREFIX + "OffsetX_" + currentTreeGUID, graphOffset.x);
            EditorPrefs.SetFloat(PREFS_PREFIX + "OffsetY_" + currentTreeGUID, graphOffset.y);
        }
        
        private void LoadViewState()
        {
            if (string.IsNullOrEmpty(currentTreeGUID)) return;
            
            zoomLevel = EditorPrefs.GetFloat(PREFS_PREFIX + "Zoom_" + currentTreeGUID, 1f);
            graphOffset.x = EditorPrefs.GetFloat(PREFS_PREFIX + "OffsetX_" + currentTreeGUID, 0f);
            graphOffset.y = EditorPrefs.GetFloat(PREFS_PREFIX + "OffsetY_" + currentTreeGUID, 0f);
        }
        
        #endregion
        
        private void InitializeStyles()
        {
            if (stylesInitialized && nodeTitleStyle != null) return;
            
            nodeTitleStyle = new GUIStyle(EditorStyles.boldLabel);
            nodeTitleStyle.fontSize = 12;
            nodeTitleStyle.normal.textColor = Color.white;
            nodeTitleStyle.alignment = TextAnchor.MiddleLeft;
            nodeTitleStyle.clipping = TextClipping.Clip;
            
            nodeSubtitleStyle = new GUIStyle(EditorStyles.label);
            nodeSubtitleStyle.fontSize = 10;
            nodeSubtitleStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
            nodeSubtitleStyle.alignment = TextAnchor.UpperLeft;
            nodeSubtitleStyle.wordWrap = true;
            nodeSubtitleStyle.clipping = TextClipping.Clip;
            
            panelHeaderStyle = new GUIStyle(EditorStyles.boldLabel);
            panelHeaderStyle.fontSize = 12;
            panelHeaderStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
            panelHeaderStyle.padding = new RectOffset(0, 0, 6, 6);
            panelHeaderStyle.margin = new RectOffset(0, 0, 0, 0);
            
            statusLabelStyle = new GUIStyle(EditorStyles.label);
            statusLabelStyle.fontSize = 11;
            statusLabelStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
            
            stylesInitialized = true;
        }
        
        private void InvalidateNodeCache()
        {
            nodesCacheDirty = true;
        }
        
        private void RefreshNodeCache()
        {
            if (!nodesCacheDirty) return;
            
            cachedNodes = showAllNodes ? GetAllNodesInProject() : GetAllNodes();
            cachedConnectedNodes = new HashSet<DialogueNode>(GetAllNodes());
            nodesCacheDirty = false;
        }
        
        void OnGUI()
        {
            InitializeStyles();
            RefreshNodeCache();
            
            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), BG_COLOR);
            
            Event e = Event.current;
            HandleInputEvents(e);
            
            DrawToolbar();
            
            if (currentTree == null)
            {
                DrawEmptyState();
                return;
            }
            
            float inspectorWidth = Mathf.Clamp(INSPECTOR_WIDTH, 350, position.width * 0.4f);
            float graphWidth = position.width - inspectorWidth;
            
            graphViewRect = new Rect(0, TOOLBAR_HEIGHT, graphWidth, position.height - TOOLBAR_HEIGHT - STATUS_HEIGHT);
            Rect inspectorRect = new Rect(graphWidth, TOOLBAR_HEIGHT, inspectorWidth, position.height - TOOLBAR_HEIGHT - STATUS_HEIGHT);
            Rect statusRect = new Rect(0, position.height - STATUS_HEIGHT, position.width, STATUS_HEIGHT);
            
            EditorGUI.DrawRect(new Rect(graphWidth - 1, TOOLBAR_HEIGHT, 2, position.height - TOOLBAR_HEIGHT - STATUS_HEIGHT), 
                new Color(0, 0, 0, 0.5f));
            
            DrawGraphView(graphViewRect);
            DrawInspectorPanel(inspectorRect);
            DrawStatusBar(statusRect);
        }
        
        private void HandleInputEvents(Event e)
        {
            // Handle zoom with scroll wheel
            if (e.type == EventType.ScrollWheel && graphViewRect.Contains(e.mousePosition))
            {
                Vector2 mouseInGraph = e.mousePosition - graphViewRect.position;
                Vector2 graphPosBefore = (mouseInGraph - graphOffset) / zoomLevel;
                
                float oldZoom = zoomLevel;
                zoomLevel -= e.delta.y * ZOOM_STEP * 0.1f;
                zoomLevel = Mathf.Clamp(zoomLevel, MIN_ZOOM, MAX_ZOOM);
                
                // Zoom towards mouse
                if (oldZoom != zoomLevel)
                {
                    Vector2 graphPosAfter = graphPosBefore * zoomLevel + graphOffset;
                    graphOffset += mouseInGraph - graphPosAfter;
                }
                
                e.Use();
                Repaint();
                return;
            }
            
            // Pan with middle mouse
            if (e.type == EventType.MouseDown && e.button == 2 && graphViewRect.Contains(e.mousePosition))
            {
                isPanning = true;
                e.Use();
                return;
            }
            
            if (e.type == EventType.MouseUp && e.button == 2)
            {
                isPanning = false;
                e.Use();
                return;
            }
            
            if (isPanning && (e.type == EventType.MouseDrag || e.type == EventType.MouseMove))
            {
                graphOffset += e.delta;
                e.Use();
                Repaint();
                return;
            }
            
            if (!graphViewRect.Contains(e.mousePosition))
                return;
            
            Vector2 mouseInContent = ScreenToGraph(e.mousePosition);
            
            // Node selection and dragging
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                DialogueNode clickedNode = null;
                
                foreach (var kvp in nodeRects)
                {
                    if (kvp.Value.Contains(mouseInContent))
                    {
                        clickedNode = kvp.Key;
                        break;
                    }
                }
                
                if (clickedNode != null)
                {
                    selectedNode = clickedNode;
                    selectedNodeSO = new SerializedObject(selectedNode);
                    isDragging = true;
                    draggedNode = clickedNode;
                    dragOffset = mouseInContent - nodePositions[clickedNode];
                    e.Use();
                }
                else
                {
                    selectedNode = null;
                    selectedNodeSO = null;
                }
                
                Repaint();
            }
            
            // Dragging
            if (isDragging && draggedNode != null && e.type == EventType.MouseDrag && e.button == 0)
            {
                Vector2 newPos = mouseInContent - dragOffset;
                
                // Snap to grid
                newPos.x = Mathf.Round(newPos.x / GRID_SIZE) * GRID_SIZE;
                newPos.y = Mathf.Round(newPos.y / GRID_SIZE) * GRID_SIZE;
                
                nodePositions[draggedNode] = newPos;
                e.Use();
                Repaint();
            }
            
            // End drag and save positions
            if (e.type == EventType.MouseUp && e.button == 0)
            {
                if (isDragging && draggedNode != null)
                {
                    SaveNodePositions(); // Save when drag ends
                }
                isDragging = false;
                draggedNode = null;
            }
            
            // Context menu
            if (e.type == EventType.ContextClick && graphViewRect.Contains(e.mousePosition))
            {
                ShowContextMenu(mouseInContent);
                e.Use();
            }
        }
        
        private Vector2 ScreenToGraph(Vector2 screenPos)
        {
            return (screenPos - graphViewRect.position - graphOffset) / zoomLevel;
        }
        
        private Vector2 GraphToScreen(Vector2 graphPos)
        {
            return graphPos * zoomLevel + graphOffset + graphViewRect.position;
        }
        
        private void DrawToolbar()
        {
            EditorGUI.DrawRect(new Rect(0, 0, position.width, TOOLBAR_HEIGHT), new Color(0.18f, 0.18f, 0.2f));
            
            GUILayout.BeginArea(new Rect(0, 0, position.width, TOOLBAR_HEIGHT));
            GUILayout.BeginHorizontal();
            
            GUILayout.Space(8);
            
            if (GUILayout.Button("New", EditorStyles.toolbarButton, GUILayout.Width(50)))
                CreateNewTree();
            
            if (GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Width(50)))
                LoadTree();
            
            GUILayout.Space(15);
            
            EditorGUI.BeginChangeCheck();
            var newTree = (DialogueTree)EditorGUILayout.ObjectField(currentTree, typeof(DialogueTree), false, GUILayout.Width(200));
            if (EditorGUI.EndChangeCheck() && newTree != currentTree)
            {
                // Save current before switching
                SaveNodePositions();
                SaveViewState();
                
                currentTree = newTree;
                selectedNode = null;
                selectedNodeSO = null;
                
                if (currentTree != null)
                {
                    currentTreeGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(currentTree));
                    currentTreeSO = new SerializedObject(currentTree);
                    nodePositions.Clear();
                    LoadNodePositions();
                    LoadViewState();
                }
                else
                {
                    currentTreeGUID = null;
                }
                
                InvalidateNodeCache();
                RefreshNodePositions();
            }
            
            GUILayout.Space(15);
            
            if (currentTree != null)
            {
                EditorGUI.BeginChangeCheck();
                showAllNodes = GUILayout.Toggle(showAllNodes, "Show All", EditorStyles.toolbarButton, GUILayout.Width(70));
                if (EditorGUI.EndChangeCheck())
                {
                    InvalidateNodeCache();
                    RefreshNodePositions();
                }
                
                GUILayout.Space(10);
                
                // Zoom controls
                GUILayout.Label($"Zoom: {zoomLevel:P0}", EditorStyles.toolbarButton, GUILayout.Width(80));
                if (GUILayout.Button("-", EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    zoomLevel = Mathf.Clamp(zoomLevel - ZOOM_STEP, MIN_ZOOM, MAX_ZOOM);
                }
                if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    zoomLevel = Mathf.Clamp(zoomLevel + ZOOM_STEP, MIN_ZOOM, MAX_ZOOM);
                }
                if (GUILayout.Button("Reset", EditorStyles.toolbarButton, GUILayout.Width(45)))
                {
                    zoomLevel = 1f;
                    graphOffset = Vector2.zero;
                    SaveViewState();
                }
            }
            
            GUILayout.FlexibleSpace();
            
            if (currentTree != null)
            {
                if (GUILayout.Button("Auto Layout", EditorStyles.toolbarButton, GUILayout.Width(80)))
                {
                    AutoLayoutNodes();
                    SaveNodePositions();
                }
                
                GUILayout.Space(5);
                
                if (GUILayout.Button("Validate", EditorStyles.toolbarButton, GUILayout.Width(60)))
                    ValidateTree();
                
                if (GUILayout.Button("Preview", EditorStyles.toolbarButton, GUILayout.Width(60)))
                    PreviewDialogue();
            }
            
            GUILayout.Space(8);
            
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
        
        private void DrawEmptyState()
        {
            float centerX = position.width / 2;
            float centerY = position.height / 2;
            
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.fontSize = 18;
            titleStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
            titleStyle.alignment = TextAnchor.MiddleCenter;
            
            GUIStyle subtitleStyle = new GUIStyle(EditorStyles.label);
            subtitleStyle.fontSize = 12;
            subtitleStyle.normal.textColor = new Color(0.4f, 0.4f, 0.4f);
            subtitleStyle.alignment = TextAnchor.MiddleCenter;
            
            GUI.Label(new Rect(centerX - 150, centerY - 60, 300, 30), "No Dialogue Tree Loaded", titleStyle);
            GUI.Label(new Rect(centerX - 150, centerY - 25, 300, 20), "Create a new tree or load an existing one", subtitleStyle);
            
            if (GUI.Button(new Rect(centerX - 70, centerY + 10, 140, 32), "Create New Tree"))
                CreateNewTree();
        }
        
        private void DrawGraphView(Rect rect)
        {
            // Draw background
            EditorGUI.DrawRect(rect, BG_COLOR);
            
            // Begin GUI group for the graph area (no clipping issues with zoom)
            GUI.BeginGroup(rect);
            
            // Draw grid first (in screen space)
            DrawGrid(new Rect(0, 0, rect.width, rect.height));
            
            // Draw everything in world space (applying zoom and pan manually)
            if (cachedNodes != null)
            {
                // Draw connections
                Handles.BeginGUI();
                foreach (var node in cachedNodes)
                {
                    if (node != null && nodePositions.ContainsKey(node))
                        DrawNodeConnections(node);
                }
                Handles.EndGUI();
                
                // Draw nodes
                foreach (var node in cachedNodes)
                {
                    if (node != null)
                        DrawNode(node);
                }
            }
            
            GUI.EndGroup();
        }
        
        private void DrawGrid(Rect rect)
        {
            float scaledGridSmall = GRID_SIZE * zoomLevel;
            float scaledGridLarge = GRID_SIZE_LARGE * zoomLevel;
            
            // Prevent too small grid lines when zoomed out
            if (scaledGridSmall < 5f) scaledGridSmall = GRID_SIZE_LARGE * zoomLevel;
            
            float offsetX = graphOffset.x % scaledGridSmall;
            float offsetY = graphOffset.y % scaledGridSmall;
            float offsetXLarge = graphOffset.x % scaledGridLarge;
            float offsetYLarge = graphOffset.y % scaledGridLarge;
            
            Handles.BeginGUI();
            
            // Small grid (skip if too dense)
            if (scaledGridSmall >= 5f)
            {
                Handles.color = GRID_COLOR_SMALL;
                int numLinesX = Mathf.CeilToInt(rect.width / scaledGridSmall) + 2;
                int numLinesY = Mathf.CeilToInt(rect.height / scaledGridSmall) + 2;
                
                for (int i = -1; i < numLinesX; i++)
                {
                    float x = offsetX + i * scaledGridSmall;
                    Handles.DrawLine(new Vector3(x, 0), new Vector3(x, rect.height));
                }
                for (int i = -1; i < numLinesY; i++)
                {
                    float y = offsetY + i * scaledGridSmall;
                    Handles.DrawLine(new Vector3(0, y), new Vector3(rect.width, y));
                }
            }
            
            // Large grid
            Handles.color = GRID_COLOR_LARGE;
            int numLinesXLarge = Mathf.CeilToInt(rect.width / scaledGridLarge) + 2;
            int numLinesYLarge = Mathf.CeilToInt(rect.height / scaledGridLarge) + 2;
            
            for (int i = -1; i < numLinesXLarge; i++)
            {
                float x = offsetXLarge + i * scaledGridLarge;
                Handles.DrawLine(new Vector3(x, 0), new Vector3(x, rect.height));
            }
            for (int i = -1; i < numLinesYLarge; i++)
            {
                float y = offsetYLarge + i * scaledGridLarge;
                Handles.DrawLine(new Vector3(0, y), new Vector3(rect.width, y));
            }
            
            Handles.EndGUI();
        }
        
        private void DrawNodeConnections(DialogueNode node)
        {
            if (!nodePositions.ContainsKey(node)) return;
            
            Vector2 nodeScreenPos = nodePositions[node] * zoomLevel + graphOffset;
            float scaledWidth = NODE_WIDTH * zoomLevel;
            float scaledHeight = NODE_HEIGHT * zoomLevel;
            
            Vector2 startPoint = nodeScreenPos + new Vector2(scaledWidth, scaledHeight / 2);
            
            // Auto-advance connection
            if (node.NextNodeIfAuto != null && nodePositions.ContainsKey(node.NextNodeIfAuto))
            {
                Vector2 targetScreenPos = nodePositions[node.NextNodeIfAuto] * zoomLevel + graphOffset;
                Vector2 endPoint = targetScreenPos + new Vector2(0, scaledHeight / 2);
                DrawBezierConnection(startPoint, endPoint, CONNECTION_AUTO, 3f * zoomLevel);
            }
            
            // Choice connections
            int choiceIndex = 0;
            foreach (var choice in node.Choices)
            {
                if (choice.NextNode != null && nodePositions.ContainsKey(choice.NextNode))
                {
                    Vector2 targetScreenPos = nodePositions[choice.NextNode] * zoomLevel + graphOffset;
                    Vector2 choiceStart = nodeScreenPos + new Vector2(scaledWidth, (30 + choiceIndex * 15) * zoomLevel);
                    Vector2 endPoint = targetScreenPos + new Vector2(0, scaledHeight / 2);
                    
                    Color lineColor = choice.HasCondition ? CONNECTION_CONDITION : CONNECTION_COLOR;
                    DrawBezierConnection(choiceStart, endPoint, lineColor, 2f * zoomLevel);
                }
                choiceIndex++;
            }
        }
        
        private void DrawBezierConnection(Vector2 start, Vector2 end, Color color, float width)
        {
            float distance = Vector2.Distance(start, end);
            float tangentOffset = Mathf.Clamp(distance * 0.4f, 30f * zoomLevel, 150f * zoomLevel);
            
            Vector3 startPos = new Vector3(start.x, start.y, 0);
            Vector3 endPos = new Vector3(end.x, end.y, 0);
            Vector3 startTan = startPos + Vector3.right * tangentOffset;
            Vector3 endTan = endPos + Vector3.left * tangentOffset;
            
            Handles.DrawBezier(startPos, endPos, startTan, endTan, color, null, Mathf.Max(1f, width));
            
            // Arrow
            Vector2 direction = (end - start).normalized;
            float arrowSize = Mathf.Max(6f, 10f * zoomLevel);
            Vector2 arrowBase = end - direction * arrowSize;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x) * arrowSize * 0.5f;
            
            Handles.color = color;
            Handles.DrawAAConvexPolygon(end, arrowBase + perpendicular, arrowBase - perpendicular);
        }
        
        private void DrawNode(DialogueNode node)
        {
            if (!nodePositions.ContainsKey(node))
            {
                nodePositions[node] = new Vector2(Random.Range(100, 600), Random.Range(100, 400));
            }
            
            Vector2 screenPos = nodePositions[node] * zoomLevel + graphOffset;
            float scaledWidth = NODE_WIDTH * zoomLevel;
            float scaledHeight = NODE_HEIGHT * zoomLevel;
            
            Rect nodeRect = new Rect(screenPos.x, screenPos.y, scaledWidth, scaledHeight);
            
            // Store in graph space for hit testing
            nodeRects[node] = new Rect(nodePositions[node].x, nodePositions[node].y, NODE_WIDTH, NODE_HEIGHT);
            
            // Skip if completely outside view
            Rect viewRect = new Rect(0, 0, graphViewRect.width, graphViewRect.height);
            if (!nodeRect.Overlaps(viewRect)) return;
            
            // Determine colors
            Color headerColor = NODE_BORDER;
            bool isSelected = selectedNode == node;
            bool isUnlinked = cachedConnectedNodes != null && !cachedConnectedNodes.Contains(node) && showAllNodes;
            
            if (node == currentTree.StartNode)
                headerColor = NODE_START;
            else if (node.IsEndNode)
                headerColor = NODE_END;
            else if (isSelected)
                headerColor = NODE_SELECTED;
            else if (isUnlinked)
                headerColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            
            // Shadow
            EditorGUI.DrawRect(new Rect(nodeRect.x + 3 * zoomLevel, nodeRect.y + 3 * zoomLevel, scaledWidth, scaledHeight), 
                new Color(0, 0, 0, 0.3f));
            
            // Background
            EditorGUI.DrawRect(nodeRect, NODE_BG);
            
            // Header
            float headerHeight = 28 * zoomLevel;
            EditorGUI.DrawRect(new Rect(nodeRect.x, nodeRect.y, scaledWidth, headerHeight), headerColor);
            
            // Border
            float borderWidth = isSelected ? 2 : 1;
            Color borderColor = isSelected ? NODE_SELECTED : new Color(0.3f, 0.3f, 0.33f);
            DrawRectBorder(nodeRect, borderColor, borderWidth);
            
            // Only draw text if zoom is sufficient
            if (zoomLevel >= 0.4f)
            {
                // Title
                string nodeId = string.IsNullOrEmpty(node.NodeId) ? "Unnamed" : node.NodeId;
                float padding = 10 * zoomLevel;
                
                GUIStyle scaledTitle = new GUIStyle(nodeTitleStyle);
                scaledTitle.fontSize = Mathf.RoundToInt(12 * zoomLevel);
                
                GUI.Label(new Rect(nodeRect.x + padding, nodeRect.y + 5 * zoomLevel, scaledWidth - padding * 2, 20 * zoomLevel), 
                    nodeId, scaledTitle);
                
                // Subtitle
                if (zoomLevel >= 0.5f)
                {
                    string speakerName = node.Speaker != null ? node.Speaker.DisplayName : "No Speaker";
                    string preview = speakerName;
                    
                    if (!string.IsNullOrEmpty(node.LineText))
                    {
                        int maxChars = Mathf.RoundToInt(28 / zoomLevel);
                        string linePreview = node.LineText.Length > maxChars ? 
                            node.LineText.Substring(0, maxChars) + "..." : node.LineText;
                        preview += "\n" + linePreview;
                    }
                    
                    if (isUnlinked) preview += "\n[Unlinked]";
                    
                    GUIStyle scaledSubtitle = new GUIStyle(nodeSubtitleStyle);
                    scaledSubtitle.fontSize = Mathf.RoundToInt(10 * zoomLevel);
                    
                    GUI.Label(new Rect(nodeRect.x + padding, nodeRect.y + headerHeight + 2 * zoomLevel, 
                        scaledWidth - padding * 2, scaledHeight - headerHeight - 5 * zoomLevel), 
                        preview, scaledSubtitle);
                }
            }
            
            // Connection points
            float pointSize = 5 * zoomLevel;
            EditorGUI.DrawRect(new Rect(nodeRect.x - pointSize, nodeRect.y + scaledHeight / 2 - pointSize, pointSize * 2, pointSize * 2), 
                new Color(0.8f, 0.6f, 0.6f));
            EditorGUI.DrawRect(new Rect(nodeRect.xMax - pointSize, nodeRect.y + scaledHeight / 2 - pointSize, pointSize * 2, pointSize * 2), 
                new Color(0.6f, 0.8f, 0.6f));
        }
        
        private void DrawRectBorder(Rect rect, Color color, float thickness)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }
        
        private void DrawInspectorPanel(Rect rect)
        {
            EditorGUI.DrawRect(rect, PANEL_BG);
            
            float contentHeight = selectedNode != null ? 1800f : 500f;
            float contentWidth = rect.width - 30;
            
            inspectorScrollPos = GUI.BeginScrollView(
                new Rect(rect.x, rect.y, rect.width, rect.height), 
                inspectorScrollPos, 
                new Rect(0, 0, contentWidth, contentHeight)
            );
            
            GUILayout.BeginArea(new Rect(12, 12, contentWidth - 10, contentHeight - 20));
            
            if (selectedNode != null)
                DrawNodeInspector();
            else
                DrawTreeInspector();
            
            GUILayout.EndArea();
            
            GUI.EndScrollView();
        }
        
        private void DrawNodeInspector()
        {
            if (selectedNode == null) return;
            
            if (selectedNodeSO == null || selectedNodeSO.targetObject != selectedNode)
                selectedNodeSO = new SerializedObject(selectedNode);
            
            selectedNodeSO.Update();
            
            EditorGUILayout.LabelField("Node Properties", panelHeaderStyle);
            DrawHorizontalLine();
            EditorGUILayout.Space(3);
            
            EditorGUILayout.PropertyField(selectedNodeSO.FindProperty("nodeId"), new GUIContent("Node ID"));
            EditorGUILayout.PropertyField(selectedNodeSO.FindProperty("speaker"), new GUIContent("Speaker"));
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Dialogue Text", EditorStyles.boldLabel);
            SerializedProperty lineTextProp = selectedNodeSO.FindProperty("lineText");
            lineTextProp.stringValue = EditorGUILayout.TextArea(lineTextProp.stringValue, GUILayout.Height(60));
            EditorGUILayout.Space(8);
            
            EditorGUILayout.LabelField("Audio Settings", panelHeaderStyle);
            DrawHorizontalLine();
            EditorGUILayout.PropertyField(selectedNodeSO.FindProperty("voiceClip"), new GUIContent("Voice Clip"));
            EditorGUILayout.PropertyField(selectedNodeSO.FindProperty("voiceDelay"), new GUIContent("Delay"));
            EditorGUILayout.Space(8);
            
            EditorGUILayout.LabelField("Choices", panelHeaderStyle);
            DrawHorizontalLine();
            
            SerializedProperty choicesProp = selectedNodeSO.FindProperty("choices");
            if (choicesProp != null)
            {
                for (int i = 0; i < choicesProp.arraySize; i++)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    SerializedProperty choiceProp = choicesProp.GetArrayElementAtIndex(i);
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"Choice {i + 1}", EditorStyles.boldLabel, GUILayout.Width(70));
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("×", GUILayout.Width(22), GUILayout.Height(18)))
                    {
                        choicesProp.DeleteArrayElementAtIndex(i);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.PropertyField(choiceProp.FindPropertyRelative("choiceText"), new GUIContent("Text"));
                    EditorGUILayout.PropertyField(choiceProp.FindPropertyRelative("nextNode"), new GUIContent("Next Node"));
                    EditorGUILayout.PropertyField(choiceProp.FindPropertyRelative("hasCondition"), new GUIContent("Conditional"));
                    
                    if (choiceProp.FindPropertyRelative("hasCondition").boolValue)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(choiceProp.FindPropertyRelative("showCondition"), new GUIContent("Condition"));
                        EditorGUI.indentLevel--;
                    }
                    
                    SerializedProperty impactsProp = choiceProp.FindPropertyRelative("impacts");
                    if (impactsProp != null && impactsProp.arraySize > 0)
                    {
                        EditorGUILayout.LabelField("Impacts", EditorStyles.miniLabel);
                        for (int j = 0; j < impactsProp.arraySize; j++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            SerializedProperty impactProp = impactsProp.GetArrayElementAtIndex(j);
                            EditorGUILayout.PropertyField(impactProp.FindPropertyRelative("variableName"), GUIContent.none, GUILayout.Width(120));
                            EditorGUILayout.PropertyField(impactProp.FindPropertyRelative("valueChange"), GUIContent.none, GUILayout.Width(50));
                            if (GUILayout.Button("×", GUILayout.Width(22)))
                            {
                                impactsProp.DeleteArrayElementAtIndex(j);
                                break;
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    if (GUILayout.Button("+ Impact", EditorStyles.miniButton, GUILayout.Width(70)))
                    {
                        impactsProp.arraySize++;
                    }
                    
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(2);
                }
                
                if (GUILayout.Button("+ Add Choice", GUILayout.Height(24)))
                {
                    choicesProp.arraySize++;
                }
            }
            
            EditorGUILayout.Space(8);
            
            EditorGUILayout.LabelField("Flow Control", panelHeaderStyle);
            DrawHorizontalLine();
            EditorGUILayout.PropertyField(selectedNodeSO.FindProperty("nextNodeIfAuto"), new GUIContent("Auto-Advance"));
            EditorGUILayout.PropertyField(selectedNodeSO.FindProperty("isEndNode"), new GUIContent("End Node"));
            EditorGUILayout.PropertyField(selectedNodeSO.FindProperty("displayDuration"), new GUIContent("Duration"));
            EditorGUILayout.Space(8);
            
            EditorGUILayout.LabelField("Commands", panelHeaderStyle);
            DrawHorizontalLine();
            
            SerializedProperty startCommandsProp = selectedNodeSO.FindProperty("startCommands");
            if (startCommandsProp != null)
                EditorGUILayout.PropertyField(startCommandsProp, new GUIContent("On Start"), true);
            
            SerializedProperty endCommandsProp = selectedNodeSO.FindProperty("endCommands");
            if (endCommandsProp != null)
                EditorGUILayout.PropertyField(endCommandsProp, new GUIContent("On End"), true);
            
            selectedNodeSO.ApplyModifiedProperties();
            
            EditorGUILayout.Space(12);
            
            GUI.backgroundColor = new Color(0.9f, 0.3f, 0.3f);
            if (GUILayout.Button("Delete Node", GUILayout.Height(26)))
            {
                if (EditorUtility.DisplayDialog("Delete Node", $"Delete node '{selectedNode.NodeId}'?", "Delete", "Cancel"))
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
            
            EditorGUILayout.LabelField("Tree Properties", panelHeaderStyle);
            DrawHorizontalLine();
            EditorGUILayout.Space(3);
            
            EditorGUILayout.PropertyField(currentTreeSO.FindProperty("treeId"), new GUIContent("Tree ID"));
            EditorGUILayout.PropertyField(currentTreeSO.FindProperty("treeTitle"), new GUIContent("Title"));
            EditorGUILayout.PropertyField(currentTreeSO.FindProperty("startNode"), new GUIContent("Start Node"));
            EditorGUILayout.PropertyField(currentTreeSO.FindProperty("defaultTypewriterSpeed"), new GUIContent("Typewriter Speed"));
            EditorGUILayout.PropertyField(currentTreeSO.FindProperty("autoAdvanceIfSingleChoice"), new GUIContent("Auto Single Choice"));
            
            currentTreeSO.ApplyModifiedProperties();
            
            EditorGUILayout.Space(12);
            
            EditorGUILayout.LabelField("Statistics", panelHeaderStyle);
            DrawHorizontalLine();
            
            if (cachedNodes != null)
            {
                EditorGUILayout.LabelField($"Connected Nodes: {cachedNodes.Count}");
                EditorGUILayout.LabelField($"End Nodes: {cachedNodes.Count(n => n != null && n.IsEndNode)}");
            }
            EditorGUILayout.LabelField($"Start Node: {(currentTree.StartNode != null ? currentTree.StartNode.NodeId : "None")}");
            
            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox("Node positions are saved automatically.\nScroll to zoom, middle-drag to pan.", MessageType.Info);
        }
        
        private void DrawHorizontalLine()
        {
            EditorGUILayout.Space(2);
            Rect lineRect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(lineRect, new Color(0.35f, 0.35f, 0.4f));
            EditorGUILayout.Space(3);
        }
        
        private void DrawStatusBar(Rect rect)
        {
            EditorGUI.DrawRect(rect, new Color(0.14f, 0.14f, 0.16f));
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), new Color(0, 0, 0, 0.3f));
            
            GUILayout.BeginArea(new Rect(rect.x + 10, rect.y + 4, rect.width - 20, rect.height - 8));
            GUILayout.BeginHorizontal();
            
            if (currentTree != null && cachedNodes != null)
            {
                GUILayout.Label($"Nodes: {cachedNodes.Count}", statusLabelStyle);
                GUILayout.Space(15);
                GUILayout.Label($"Selected: {(selectedNode != null ? selectedNode.NodeId : "None")}", statusLabelStyle);
            }
            else
            {
                GUILayout.Label("No tree loaded", statusLabelStyle);
            }
            
            GUILayout.FlexibleSpace();
            
            GUILayout.Label($"Zoom: {zoomLevel:P0}", statusLabelStyle);
            
            GUILayout.Space(15);
            
            if (Application.isPlaying)
            {
                GUIStyle playModeStyle = new GUIStyle(statusLabelStyle);
                playModeStyle.normal.textColor = new Color(0.4f, 0.9f, 0.4f);
                GUILayout.Label("● PLAY", playModeStyle);
            }
            
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
        
        private void ShowContextMenu(Vector2 position)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Create Node"), false, () => CreateNodeAtPosition(position));
            menu.AddItem(new GUIContent("Create Start Node"), false, () => {
                CreateNodeAtPosition(position);
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
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Auto Layout"), false, () => { AutoLayoutNodes(); SaveNodePositions(); });
            menu.AddItem(new GUIContent("Reset View"), false, () => { zoomLevel = 1f; graphOffset = Vector2.zero; SaveViewState(); });
            menu.ShowAsContext();
        }
        
        private void CreateNodeAtPosition(Vector2 position)
        {
            if (currentTree == null) return;
            
            DialogueNode newNode = CreateInstance<DialogueNode>();
            newNode.name = "NewNode";
            
            string folderPath = "Assets/Dialogue/Nodes";
            
            if (!AssetDatabase.IsValidFolder("Assets/Dialogue"))
                AssetDatabase.CreateFolder("Assets", "Dialogue");
            if (!AssetDatabase.IsValidFolder(folderPath))
                AssetDatabase.CreateFolder("Assets/Dialogue", "Nodes");
            
            string uniquePath = AssetDatabase.GenerateUniqueAssetPath(folderPath + "/NewNode.asset");
            AssetDatabase.CreateAsset(newNode, uniquePath);
            AssetDatabase.SaveAssets();
            
            position.x = Mathf.Round(position.x / GRID_SIZE) * GRID_SIZE;
            position.y = Mathf.Round(position.y / GRID_SIZE) * GRID_SIZE;
            
            nodePositions[newNode] = position;
            
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
            InvalidateNodeCache();
            SaveNodePositions();
            Repaint();
        }
        
        private void AutoLayoutNodes()
        {
            if (currentTree == null || currentTree.StartNode == null) return;
            
            var allNodes = GetAllNodes();
            var visited = new HashSet<DialogueNode>();
            var levels = new Dictionary<DialogueNode, int>();
            
            var queue = new Queue<DialogueNode>();
            queue.Enqueue(currentTree.StartNode);
            levels[currentTree.StartNode] = 0;
            
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (visited.Contains(node)) continue;
                visited.Add(node);
                
                int currentLevel = levels[node];
                
                if (node.NextNodeIfAuto != null && !visited.Contains(node.NextNodeIfAuto))
                {
                    levels[node.NextNodeIfAuto] = currentLevel + 1;
                    queue.Enqueue(node.NextNodeIfAuto);
                }
                
                foreach (var choice in node.Choices)
                {
                    if (choice.NextNode != null && !visited.Contains(choice.NextNode))
                    {
                        levels[choice.NextNode] = currentLevel + 1;
                        queue.Enqueue(choice.NextNode);
                    }
                }
            }
            
            var nodesPerLevel = new Dictionary<int, List<DialogueNode>>();
            foreach (var kvp in levels)
            {
                if (!nodesPerLevel.ContainsKey(kvp.Value))
                    nodesPerLevel[kvp.Value] = new List<DialogueNode>();
                nodesPerLevel[kvp.Value].Add(kvp.Key);
            }
            
            float xSpacing = NODE_WIDTH + 80f;
            float ySpacing = NODE_HEIGHT + 60f;
            
            foreach (var kvp in nodesPerLevel)
            {
                int level = kvp.Key;
                var nodes = kvp.Value;
                float startY = (nodes.Count - 1) * ySpacing / 2f;
                
                for (int i = 0; i < nodes.Count; i++)
                {
                    float x = 100f + level * xSpacing;
                    float y = 300f + i * ySpacing - startY;
                    nodePositions[nodes[i]] = new Vector2(x, y);
                }
            }
            
            // Center on start node
            if (nodePositions.ContainsKey(currentTree.StartNode))
            {
                graphOffset = -nodePositions[currentTree.StartNode] * zoomLevel + 
                    new Vector2(graphViewRect.width / 2 - NODE_WIDTH * zoomLevel / 2, graphViewRect.height / 2 - NODE_HEIGHT * zoomLevel / 2);
            }
            
            Repaint();
        }
        
        private void CreateNewTree()
        {
            SaveNodePositions();
            SaveViewState();
            
            DialogueTree newTree = CreateInstance<DialogueTree>();
            newTree.name = "NewDialogueTree";
            
            string path = EditorUtility.SaveFilePanelInProject("Save Tree", "NewDialogueTree", "asset", "Save dialogue tree");
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(newTree, path);
                AssetDatabase.SaveAssets();
                currentTree = newTree;
                currentTreeGUID = AssetDatabase.AssetPathToGUID(path);
                currentTreeSO = new SerializedObject(currentTree);
                selectedNode = null;
                nodePositions.Clear();
                nodeRects.Clear();
                zoomLevel = 1f;
                graphOffset = Vector2.zero;
                InvalidateNodeCache();
                RefreshNodePositions();
                Repaint();
            }
        }
        
        private void LoadTree()
        {
            SaveNodePositions();
            SaveViewState();
            
            string path = EditorUtility.OpenFilePanel("Load Dialogue Tree", "Assets", "asset");
            if (!string.IsNullOrEmpty(path))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
                currentTree = AssetDatabase.LoadAssetAtPath<DialogueTree>(path);
                if (currentTree != null)
                {
                    currentTreeGUID = AssetDatabase.AssetPathToGUID(path);
                    currentTreeSO = new SerializedObject(currentTree);
                    selectedNode = null;
                    nodePositions.Clear();
                    nodeRects.Clear();
                    LoadNodePositions();
                    LoadViewState();
                    InvalidateNodeCache();
                    RefreshNodePositions();
                    Repaint();
                }
            }
        }
        
        private void DeleteNode(DialogueNode node)
        {
            if (node == null) return;
            
            if (currentTree.StartNode == node)
            {
                Undo.RecordObject(currentTree, "Clear Start Node");
                currentTree.GetType().GetField("startNode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(currentTree, null);
                EditorUtility.SetDirty(currentTree);
            }
            
            var allNodes = GetAllNodes();
            foreach (var n in allNodes)
            {
                if (n == node) continue;
                
                bool changed = false;
                
                if (n.NextNodeIfAuto == node)
                {
                    Undo.RecordObject(n, "Clear Auto-Advance");
                    n.GetType().GetField("nextNodeIfAuto", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        ?.SetValue(n, null);
                    changed = true;
                }
                
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
            
            string assetPath = AssetDatabase.GetAssetPath(node);
            AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.SaveAssets();
            
            InvalidateNodeCache();
            SaveNodePositions();
            Repaint();
        }
        
        private void PreviewDialogue()
        {
            if (currentTree == null || !Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Preview", "Enter Play Mode first, then click Preview.", "OK");
                return;
            }
            
            if (DialogueManager.Instance != null)
                DialogueManager.Instance.StartDialogue(currentTree);
            else
                EditorUtility.DisplayDialog("Preview", "DialogueManager not found in scene.", "OK");
        }
        
        private void ValidateTree()
        {
            if (currentTree == null) return;
            
            var issues = new List<string>();
            var allNodes = GetAllNodes();
            
            if (currentTree.StartNode == null)
                issues.Add("• No start node assigned");
            
            foreach (var node in allNodes)
            {
                if (string.IsNullOrEmpty(node.NodeId))
                    issues.Add($"• Node has no ID");
                
                if (string.IsNullOrEmpty(node.LineText))
                    issues.Add($"• Node '{node.NodeId}' has no text");
                
                if (!node.IsEndNode && node.Choices.Count == 0 && node.NextNodeIfAuto == null)
                    issues.Add($"• Node '{node.NodeId}' has no connections");
            }
            
            if (issues.Count == 0)
                EditorUtility.DisplayDialog("Validation ✓", "Tree is valid!", "OK");
            else
                EditorUtility.DisplayDialog("Issues Found", string.Join("\n", issues), "OK");
        }
        
        private List<DialogueNode> GetAllNodes()
        {
            var nodes = new List<DialogueNode>();
            if (currentTree == null) return nodes;
            
            var visited = new HashSet<DialogueNode>();
            var toVisit = new Queue<DialogueNode>();
            
            if (currentTree.StartNode != null)
                toVisit.Enqueue(currentTree.StartNode);
            
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
        
        private void RefreshNodePositions()
        {
            if (currentTree == null) return;
            
            List<DialogueNode> nodesToPosition = showAllNodes ? GetAllNodesInProject() : GetAllNodes();
            
            const float spacing = 280f;
            int nodesPerRow = 5;
            int currentRow = 0;
            int currentCol = 0;
            
            foreach (var node in nodesToPosition)
            {
                if (!nodePositions.ContainsKey(node))
                {
                    float x = 100f + (currentCol * spacing);
                    float y = 100f + (currentRow * (NODE_HEIGHT + 80f));
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
