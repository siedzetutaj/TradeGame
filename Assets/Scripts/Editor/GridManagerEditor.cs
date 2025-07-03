using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridManager))]
public class GridManagerEditor : Editor
{
    protected bool showGridSettings = true;
    protected bool showGridVisualization = true;
    protected bool showDebugTools = true;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        GridManager gridManager = (GridManager)target;

        DrawProperties(gridManager);

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(gridManager);
        }
    }
    protected virtual void DrawProperties(GridManager gridManager)
    {
        DrawGridSettings();
        DrawGridVisualization();
        DrawGridDebug(gridManager);
    }
    protected void DrawGridSettings()
    {
        showGridSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showGridSettings, "Grid Settings");
        if (showGridSettings)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cellWidth"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cellHeight"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gridStartPosition"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gridEndPosition"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gridType"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("BGHolder"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("IsCaravan"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("GridCanBeEndless"));
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("cellsPerRow"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("disableCellsAt"));
    }
    protected void DrawGridVisualization()
    {
        showGridVisualization = EditorGUILayout.BeginFoldoutHeaderGroup(showGridVisualization, "Grid Visualization");
        if (showGridVisualization)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cellPrefab"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gridParent"));
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
    protected void DrawGridDebug(GridManager gridManager)
    {
        if (gridManager.grid != null && gridManager.grid.Count > 0)
        {
            GUILayout.Space(5);
            GUILayout.Label("Grid Layout", EditorStyles.boldLabel);

            for (int row = 0; row < gridManager.grid.Count; row++)
            {
                GUILayout.BeginHorizontal();
                for (int col = 0; col < gridManager.cellsPerRow[row]; col++)
                {
                    Vector2Int cellPos = new Vector2Int(row, col);
                    GridCell cell = gridManager.GetCellAtPosition(cellPos);

                    if (cell == null || cell.isDisabled)
                    {
                        GUILayout.Button("X", "Button", GUILayout.Width(30), GUILayout.Height(30));
                        continue;
                    }

                    bool isOccupied = GUILayout.Toggle(cell.isOccupied, $"{cell.listPosition.x},{cell.listPosition.y}",
                        "Button", GUILayout.Width(30), GUILayout.Height(30));

                    if (isOccupied != cell.isOccupied)
                    {
                        Undo.RecordObject(gridManager, "Toggle Cell Occupancy");
                        cell.isOccupied = isOccupied;
                    }
                }
                GUILayout.EndHorizontal();
            }
        }

        // Control buttons
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Create/Refresh Grid"))
        {
            Undo.RecordObject(gridManager, "Create Grid");
            gridManager.InitializeGrid();
        }
        if (GUILayout.Button("Clear Grid"))
        {
            Undo.RecordObject(gridManager, "Clear Grid");
            gridManager.ClearGrid();
        }
        GUILayout.EndHorizontal();

        // Disabled cells management
        GUILayout.Space(10);
        GUILayout.Label("Disabled Cells Management", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Selected as Disabled"))
        {
            // You could implement selection logic here
        }
        if (GUILayout.Button("Clear All Disabled"))
        {
            Undo.RecordObject(gridManager, "Clear Disabled Cells");
            gridManager.disableCellsAt.Clear();
            gridManager.InitializeGrid();
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.EndFoldoutHeaderGroup();
    }
}