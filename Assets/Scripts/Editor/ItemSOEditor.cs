using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemSO))]
public class ItemSOEditor : Editor
{
    private List<List<bool>> shape = new List<List<bool>>
        {
            new List<bool> { false, false, false, false,false },
            new List<bool> { false, false, false, false,false },
            new List<bool> { false, false, false, false,false },
            new List<bool> { false, false, false, false,false },
            new List<bool> { false, false, false, false,false }
        };
    private int gridSize = 5;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        ItemSO item = (ItemSO)target;

        if (item.itemtype == ItemType.food)
        {
            item.ration = EditorGUILayout.FloatField("Ration", item.ration);
        }

        // Save changes
        if (GUI.changed)
        {
            EditorUtility.SetDirty(item);
        }

        GUILayout.Space(10);
        GUILayout.Label("Item Shape", EditorStyles.boldLabel);

        // Begin checking for changes
        EditorGUI.BeginChangeCheck();

        // Editable field for the variable
        gridSize = EditorGUILayout.IntField("Variable:", gridSize);

        // Check if a change occurred
        if (EditorGUI.EndChangeCheck())
        {
            shape.Clear();
            GenerateListElements();
        }
        // Iterate through rows and columns in the grid list
        for (int row = 0; row < gridSize; row++)
        {
            GUILayout.BeginHorizontal(); // Begin row
            for (int col = 0; col < gridSize; col++)
            {
                bool cell = shape[col][row];

                // Display each cell as a small toggle button to show occupancy
                bool isOccupied = GUILayout.Toggle(cell, $"{col - gridSize / 2},{(row - gridSize / 2) * -1}",
                    "Button", GUILayout.Width(40), GUILayout.Height(40));

                if (isOccupied != cell)
                {
                    Undo.RecordObject(item, "Toggle Cell Occupancy");
                    shape[col][row] = isOccupied;
                }
            }
            GUILayout.EndHorizontal(); // End row
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(item);
        }
        if (GUILayout.Button("SetShape"))
        {
            item.shapeOffsets.Clear();
            for (int row = 0; row < gridSize; row++)
            {
                GUILayout.BeginHorizontal(); // Begin row
                for (int col = 0; col < gridSize; col++)
                {
                    if (shape[col][row])
                    {
                        item.shapeOffsets.Add(new Vector2Int(col - gridSize / 2, (row - gridSize / 2) * -1));
                    }
                }
            }
        }
    }

    private void GenerateListElements()
    {
        for (int row = 0; row < gridSize; row++)
        {
            shape.Add(new List<bool>());
            for (int col = 0; col < gridSize; col++)
            {
                shape[row].Add(false);
            }
        }
    }
}