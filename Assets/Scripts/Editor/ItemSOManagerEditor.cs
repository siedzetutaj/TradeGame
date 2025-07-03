using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ClosedXML.Excel; // Add this for Excel functionality
using System.IO;
using System.Linq;
using Codice.Client.BaseCommands.Merge.Xml;

[CustomEditor(typeof(ItemSOManager))]
public class ItemSOManagerEditor : Editor
{
    private List<List<bool>> shape = new List<List<bool>>
    {
        new List<bool> { false, false, false, false, false },
        new List<bool> { false, false, false, false, false },
        new List<bool> { false, false, false, false, false },
        new List<bool> { false, false, false, false, false },
        new List<bool> { false, false, false, false, false }
    };
    private int gridSize = 5;

    private SerializedProperty itemDestroyProperty;
    [SerializeField] private ItemSO itemToDestroy;

    private void OnEnable()
    {
        itemDestroyProperty = serializedObject.FindProperty("itemToDestroy");
    }

    public override void OnInspectorGUI()
    {
        ItemSOManager itemManager = (ItemSOManager)target;
        // Add Export Button
        if (GUILayout.Button("Export All Items to Excel", GUILayout.Height(30)))
        {
            ExportItemsToExcel(itemManager.allItems);
        }
        serializedObject.Update();

        base.OnInspectorGUI();

        if (itemManager.itemtype == ItemType.food)
        {
            itemManager.ration = EditorGUILayout.FloatField("Ration", itemManager.ration);
        }

        // Save changes
        if (GUI.changed)
        {
            EditorUtility.SetDirty(itemManager);
        }

        GUILayout.Space(10);
        GUILayout.Label("Item Shape", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        gridSize = EditorGUILayout.IntField("Variable:", gridSize);
        if (EditorGUI.EndChangeCheck())
        {
            shape.Clear();
            GenerateListElements();
        }

        for (int row = 0; row < gridSize; row++)
        {
            GUILayout.BeginHorizontal();
            for (int col = 0; col < gridSize; col++)
            {
                bool cell = shape[col][row];
                bool isOccupied = GUILayout.Toggle(cell, $"{col - gridSize / 2},{(row - gridSize / 2) * -1}",
                    "Button", GUILayout.Width(40), GUILayout.Height(40));

                if (isOccupied != cell)
                {
                    Undo.RecordObject(itemManager, "Toggle Cell Occupancy");
                    shape[col][row] = isOccupied;
                }
            }
            GUILayout.EndHorizontal();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(itemManager);
        }

        if (GUILayout.Button("SetShape"))
        {
            itemManager.shapeOffsets.Clear();
            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    if (shape[col][row])
                    {
                        itemManager.shapeOffsets.Add(new Vector2Int(col - gridSize / 2, (row - gridSize / 2) * -1));
                    }
                }
            }
        }

        if (GUILayout.Button("Create Item", GUILayout.Height(30)))
        {
            itemManager.CreateItem();
        }

        itemToDestroy = (ItemSO)EditorGUILayout.ObjectField("Item to Destroy", itemToDestroy, typeof(ItemSO), false);

        if (GUILayout.Button("Destroy Item", GUILayout.Height(30)))
        {
            itemManager.DestroyItem(itemToDestroy);
            itemToDestroy = null;
        }


        serializedObject.ApplyModifiedProperties();
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

    private void ExportItemsToExcel(List<ItemSO> items)
    {
        items = items.OrderBy(item => item.itemtype)
                     .ThenBy(item => item.itemRarity)
                     .ThenBy(item => item.value)
                     .ToList();
        string filePath = "Assets/Excel/ItemData.xlsx";
        if (string.IsNullOrEmpty(filePath))
        {
            Debug.Log("Export canceled.");
            return;
        }

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Items");

            // Add headers
            worksheet.Cell(1, 1).Value = "Item Name";
            worksheet.Cell(1, 2).Value = "Value";
            worksheet.Cell(1, 3).Value = "Item Type";
            worksheet.Cell(1, 4).Value = "Item Rarity";
            worksheet.Cell(1, 5).Value = "Ration Amount";

            // Add data
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                worksheet.Cell(i + 2, 1).Value = item.itemName;
                worksheet.Cell(i + 2, 2).Value = item.value;
                worksheet.Cell(i + 2, 3).Value = item.itemtype.ToString();
                worksheet.Cell(i + 2, 4).Value = item.itemRarity.ToString();
                worksheet.Cell(i + 2, 5).Value = item.ration.ToString();
            }

            // Save the workbook
            workbook.SaveAs(filePath);
        }

        Debug.Log($"Exported {items.Count} items to {filePath}");
    }
}