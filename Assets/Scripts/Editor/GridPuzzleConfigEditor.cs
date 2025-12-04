using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using WhisperingGate.Puzzles;

namespace WhisperingGate.Editor
{
    /// <summary>
    /// Custom editor for GridPuzzleConfig with visual path editing.
    /// </summary>
    [CustomEditor(typeof(GridPuzzleConfig))]
    public class GridPuzzleConfigEditor : UnityEditor.Editor
    {
        private GridPuzzleConfig config;
        private bool showGridPreview = true;
        private bool isRecordingPath = false;

        private void OnEnable()
        {
            config = (GridPuzzleConfig)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw default inspector
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Path Editing Tools", EditorStyles.boldLabel);

            // Grid preview toggle
            showGridPreview = EditorGUILayout.Toggle("Show Grid Preview", showGridPreview);

            EditorGUILayout.Space(5);

            // Path editing buttons
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Clear Path", GUILayout.Height(25)))
            {
                Undo.RecordObject(config, "Clear Path");
                config.correctPath.Clear();
                EditorUtility.SetDirty(config);
            }

            if (GUILayout.Button("Reverse Path", GUILayout.Height(25)))
            {
                Undo.RecordObject(config, "Reverse Path");
                config.correctPath.Reverse();
                EditorUtility.SetDirty(config);
            }

            EditorGUILayout.EndHorizontal();

            // Generate sample paths
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Quick Path Templates", EditorStyles.miniLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Straight Line"))
            {
                GenerateStraightPath();
            }

            if (GUILayout.Button("Diagonal"))
            {
                GenerateDiagonalPath();
            }

            if (GUILayout.Button("Zigzag"))
            {
                GenerateZigzagPath();
            }

            if (GUILayout.Button("Snake"))
            {
                GenerateSnakePath();
            }

            EditorGUILayout.EndHorizontal();

            // Path info
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                $"Grid Size: {config.cols}x{config.rows}\n" +
                $"Path Length: {config.correctPath.Count} tiles\n" +
                $"Start: ({config.startTile.x}, {config.startTile.y})\n" +
                $"End: ({config.endTile.x}, {config.endTile.y})",
                MessageType.Info
            );

            // Grid visualization
            if (showGridPreview)
            {
                DrawGridPreview();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGridPreview()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Grid Preview (Click to toggle tiles)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Click tiles to add/remove from path. Green = Start, Red = End, Yellow = Path", MessageType.None);

            float cellSize = 30f;
            float padding = 5f;
            float totalWidth = config.cols * (cellSize + padding);
            float totalHeight = config.rows * (cellSize + padding);

            Rect gridRect = GUILayoutUtility.GetRect(totalWidth, totalHeight + 20);
            
            // Draw from bottom to top (so Y=0 is at bottom)
            for (int y = config.rows - 1; y >= 0; y--)
            {
                for (int x = 0; x < config.cols; x++)
                {
                    Vector2Int coord = new Vector2Int(x, y);
                    
                    float posX = gridRect.x + x * (cellSize + padding);
                    float posY = gridRect.y + (config.rows - 1 - y) * (cellSize + padding);
                    Rect cellRect = new Rect(posX, posY, cellSize, cellSize);

                    // Determine cell color
                    Color cellColor = Color.gray;
                    
                    if (coord == config.startTile)
                        cellColor = Color.green;
                    else if (coord == config.endTile)
                        cellColor = Color.red;
                    else if (config.correctPath.Contains(coord))
                        cellColor = Color.yellow;

                    // Draw cell
                    EditorGUI.DrawRect(cellRect, cellColor);
                    
                    // Draw border
                    Handles.color = Color.black;
                    Handles.DrawWireDisc(cellRect.center, Vector3.forward, cellSize * 0.4f);

                    // Draw path index if on path
                    int pathIndex = config.correctPath.IndexOf(coord);
                    if (pathIndex >= 0)
                    {
                        GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel);
                        labelStyle.alignment = TextAnchor.MiddleCenter;
                        labelStyle.normal.textColor = Color.black;
                        GUI.Label(cellRect, pathIndex.ToString(), labelStyle);
                    }

                    // Handle clicks
                    if (Event.current.type == EventType.MouseDown && cellRect.Contains(Event.current.mousePosition))
                    {
                        ToggleTileInPath(coord);
                        Event.current.Use();
                    }
                }
            }

            // Row/column labels would go here
        }

        private void ToggleTileInPath(Vector2Int coord)
        {
            Undo.RecordObject(config, "Toggle Tile in Path");

            if (config.correctPath.Contains(coord))
            {
                config.correctPath.Remove(coord);
            }
            else
            {
                config.correctPath.Add(coord);
            }

            EditorUtility.SetDirty(config);
        }

        private void GenerateStraightPath()
        {
            Undo.RecordObject(config, "Generate Straight Path");
            config.correctPath.Clear();

            int midX = config.cols / 2;
            for (int y = 0; y < config.rows; y++)
            {
                config.correctPath.Add(new Vector2Int(midX, y));
            }

            config.startTile = config.correctPath[0];
            config.endTile = config.correctPath[config.correctPath.Count - 1];
            EditorUtility.SetDirty(config);
        }

        private void GenerateDiagonalPath()
        {
            Undo.RecordObject(config, "Generate Diagonal Path");
            config.correctPath.Clear();

            int steps = Mathf.Min(config.cols, config.rows);
            for (int i = 0; i < steps; i++)
            {
                config.correctPath.Add(new Vector2Int(i, i));
            }

            config.startTile = config.correctPath[0];
            config.endTile = config.correctPath[config.correctPath.Count - 1];
            EditorUtility.SetDirty(config);
        }

        private void GenerateZigzagPath()
        {
            Undo.RecordObject(config, "Generate Zigzag Path");
            config.correctPath.Clear();

            bool goRight = true;
            int x = 0;
            
            for (int y = 0; y < config.rows; y++)
            {
                config.correctPath.Add(new Vector2Int(x, y));
                
                if (goRight && x < config.cols - 1)
                    x++;
                else if (!goRight && x > 0)
                    x--;
                    
                goRight = !goRight;
            }

            config.startTile = config.correctPath[0];
            config.endTile = config.correctPath[config.correctPath.Count - 1];
            EditorUtility.SetDirty(config);
        }

        private void GenerateSnakePath()
        {
            Undo.RecordObject(config, "Generate Snake Path");
            config.correctPath.Clear();

            bool goRight = true;
            
            for (int y = 0; y < config.rows; y++)
            {
                if (goRight)
                {
                    for (int x = 0; x < config.cols; x++)
                        config.correctPath.Add(new Vector2Int(x, y));
                }
                else
                {
                    for (int x = config.cols - 1; x >= 0; x--)
                        config.correctPath.Add(new Vector2Int(x, y));
                }
                goRight = !goRight;
            }

            config.startTile = config.correctPath[0];
            config.endTile = config.correctPath[config.correctPath.Count - 1];
            EditorUtility.SetDirty(config);
        }
    }
}



