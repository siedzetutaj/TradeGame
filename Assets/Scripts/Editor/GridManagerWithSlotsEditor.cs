using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridManagerWithSlots))]
public class GridManagerWithSlotsEditor : GridManagerEditor
{

    protected override void DrawProperties(GridManager gridManager)
    {
        DrawGridWithSlots();
        base.DrawProperties(gridManager);
    }
    protected void DrawGridWithSlots()
    {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ItemsPlacedIn"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_gridItems"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_itemPrefab"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_itemHolder"));
    }
}
