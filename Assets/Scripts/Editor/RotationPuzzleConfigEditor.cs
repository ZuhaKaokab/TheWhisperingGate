using UnityEngine;
using UnityEditor;

namespace WhisperingGate.Puzzles.Editor
{
    /// <summary>
    /// Custom editor for RotationPuzzleConfig that provides visual editing
    /// of the puzzle grid and solution.
    /// </summary>
    [CustomEditor(typeof(RotationPuzzleConfig))]
    public class RotationPuzzleConfigEditor : UnityEditor.Editor
    {
        private RotationPuzzleConfig config;
        private bool showSolutionGrid = true;
        private bool showStartingGrid = false;
        private bool showCommands = true;

        private readonly Color cellBgColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        private readonly Color cellSelectedColor = new Color(0.3f, 0.5f, 0.7f, 1f);
        private readonly Color headerColor = new Color(0.15f, 0.15f, 0.15f, 1f);

        private void OnEnable()
        {
            config = (RotationPuzzleConfig)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Header
            EditorGUILayout.Space(5);
            DrawHeader("ROTATION PUZZLE CONFIG");
            EditorGUILayout.Space(10);

            // Identity
            DrawSection("Puzzle Identity", () =>
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("puzzleId"));
            });

            // Grid Settings
            DrawSection("Grid Settings", () =>
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("rows"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("columns"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("elementSpacing"));

                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    config.ValidateArraySizes();
                    EditorUtility.SetDirty(config);
                }

                EditorGUILayout.LabelField($"Total Elements: {config.TotalElements}", EditorStyles.boldLabel);
            });

            // Rotation Settings
            DrawSection("Rotation Settings", () =>
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationSteps"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationAxis"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationSpeed"));

                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    config.ValidateArraySizes();
                    EditorUtility.SetDirty(config);
                }

                EditorGUILayout.LabelField($"Angle Per Step: {config.AnglePerStep}°", EditorStyles.miniLabel);
            });

            EditorGUILayout.Space(10);

            // Solution Grid
            showSolutionGrid = EditorGUILayout.Foldout(showSolutionGrid, "Solution Grid", true, EditorStyles.foldoutHeader);
            if (showSolutionGrid)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                DrawPuzzleGrid(config.solutionIndices, "Solution");
                
                EditorGUILayout.Space(5);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("🎲 Randomize Solution", GUILayout.Height(25)))
                {
                    RandomizeSolution();
                }
                if (GUILayout.Button("🔄 Reset to Zero", GUILayout.Height(25)))
                {
                    ResetToZero(config.solutionIndices);
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(5);

            // Starting Grid
            showStartingGrid = EditorGUILayout.Foldout(showStartingGrid, "Starting Positions", true, EditorStyles.foldoutHeader);
            if (showStartingGrid)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("randomizeStart"));
                
                if (!config.randomizeStart)
                {
                    EditorGUILayout.Space(5);
                    DrawPuzzleGrid(config.startingIndices, "Start");
                    
                    EditorGUILayout.Space(5);
                    
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("🎲 Randomize Start", GUILayout.Height(25)))
                    {
                        RandomizeList(config.startingIndices);
                    }
                    if (GUILayout.Button("📋 Copy from Solution", GUILayout.Height(25)))
                    {
                        CopyFromSolution();
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.HelpBox("Starting positions will be randomized when puzzle activates.", MessageType.Info);
                }
                
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(10);

            // Visual Settings
            DrawSection("Visual Settings", () =>
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedHighlightColor"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("correctHighlightColor"));
            });

            // Camera Settings
            DrawSection("Camera Focus", () =>
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("cameraFocusPointId"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("solvedCameraHoldDuration"));
            });

            // Commands
            showCommands = EditorGUILayout.Foldout(showCommands, "On Solved Commands", true, EditorStyles.foldoutHeader);
            if (showCommands)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onSolvedCommands"), true);
                
                EditorGUILayout.Space(3);
                EditorGUILayout.LabelField("Available: flag:name, unflag:name, var:name+5, cam:point:duration", EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeader(string title)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.FlexibleSpace();
            GUILayout.Label(title, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSection(string title, System.Action content)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.Space(3);
            content?.Invoke();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        private void DrawPuzzleGrid(System.Collections.Generic.List<int> values, string prefix)
        {
            if (config == null) return;

            float cellSize = 50f;
            float padding = 5f;

            // Draw grid from top to bottom (row 0 at bottom in game, but top visually for editing)
            for (int row = config.rows - 1; row >= 0; row--)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                for (int col = 0; col < config.columns; col++)
                {
                    int index = row * config.columns + col;
                    if (index >= values.Count) continue;

                    // Draw cell
                    Rect cellRect = GUILayoutUtility.GetRect(cellSize, cellSize);
                    
                    // Background
                    EditorGUI.DrawRect(cellRect, cellBgColor);
                    
                    // Border
                    Handles.color = Color.gray;
                    Handles.DrawSolidRectangleWithOutline(cellRect, Color.clear, Color.gray);

                    // Value and controls
                    Rect innerRect = new Rect(cellRect.x + padding, cellRect.y + padding, 
                                              cellRect.width - padding * 2, cellRect.height - padding * 2);

                    // Current value display
                    GUIStyle valueStyle = new GUIStyle(EditorStyles.boldLabel);
                    valueStyle.alignment = TextAnchor.MiddleCenter;
                    valueStyle.fontSize = 16;
                    valueStyle.normal.textColor = Color.white;

                    int currentValue = values[index];
                    float angle = currentValue * config.AnglePerStep;
                    
                    // Draw rotation indicator
                    DrawRotationIndicator(cellRect, angle);

                    // Value text
                    GUI.Label(new Rect(cellRect.x, cellRect.y, cellRect.width, 20), 
                              $"{currentValue} ({angle}°)", valueStyle);

                    // Position label
                    GUIStyle posStyle = new GUIStyle(EditorStyles.miniLabel);
                    posStyle.alignment = TextAnchor.LowerCenter;
                    posStyle.normal.textColor = Color.gray;
                    GUI.Label(new Rect(cellRect.x, cellRect.y + cellRect.height - 15, cellRect.width, 15),
                              $"[{row},{col}]", posStyle);

                    // Click to cycle value
                    if (Event.current.type == EventType.MouseDown && cellRect.Contains(Event.current.mousePosition))
                    {
                        Undo.RecordObject(config, $"Change {prefix} Value");
                        
                        if (Event.current.button == 0) // Left click - increase
                        {
                            values[index] = (values[index] + 1) % config.rotationSteps;
                        }
                        else if (Event.current.button == 1) // Right click - decrease
                        {
                            values[index] = (values[index] - 1 + config.rotationSteps) % config.rotationSteps;
                        }
                        
                        EditorUtility.SetDirty(config);
                        Event.current.Use();
                        Repaint();
                    }
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(5);
            }

            EditorGUILayout.HelpBox("Left-click to increase, Right-click to decrease rotation index", MessageType.None);
        }

        private void DrawRotationIndicator(Rect cellRect, float angle)
        {
            // Draw a simple arrow indicator showing rotation
            Vector2 center = cellRect.center;
            float radius = Mathf.Min(cellRect.width, cellRect.height) * 0.3f;

            // Convert angle to radians and rotate (0 = up, clockwise)
            float rad = (-angle + 90) * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            Vector2 tip = center + direction * radius;

            // Draw arrow line
            Handles.color = new Color(0.4f, 0.8f, 1f, 0.8f);
            Handles.DrawLine(center, tip);

            // Draw arrow head
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);
            Vector2 arrowLeft = tip - direction * 8 + perpendicular * 5;
            Vector2 arrowRight = tip - direction * 8 - perpendicular * 5;
            
            Handles.DrawLine(tip, arrowLeft);
            Handles.DrawLine(tip, arrowRight);
        }

        private void RandomizeSolution()
        {
            Undo.RecordObject(config, "Randomize Solution");
            RandomizeList(config.solutionIndices);
            EditorUtility.SetDirty(config);
        }

        private void RandomizeList(System.Collections.Generic.List<int> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = Random.Range(0, config.rotationSteps);
            }
        }

        private void ResetToZero(System.Collections.Generic.List<int> list)
        {
            Undo.RecordObject(config, "Reset to Zero");
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = 0;
            }
            EditorUtility.SetDirty(config);
        }

        private void CopyFromSolution()
        {
            Undo.RecordObject(config, "Copy from Solution");
            for (int i = 0; i < config.startingIndices.Count && i < config.solutionIndices.Count; i++)
            {
                config.startingIndices[i] = config.solutionIndices[i];
            }
            EditorUtility.SetDirty(config);
        }
    }
}


