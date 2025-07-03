using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(DebugGridForCreatingGridWithSlots))]
public class DebugEditorGridForCreatingGridWithSlots : GridManagerEditor
{
    protected bool showGridDebug = true;

    protected override void DrawProperties(GridManager gridManager)
    {
        DrawGridDebug();
        base.DrawProperties(gridManager);
    }
    protected void DrawGridDebug()
    {
        showGridDebug = EditorGUILayout.BeginFoldoutHeaderGroup(showGridDebug, "Grid Debug");
        if (showGridDebug)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("NameInputField"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("saveButton"));
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

    }
}
