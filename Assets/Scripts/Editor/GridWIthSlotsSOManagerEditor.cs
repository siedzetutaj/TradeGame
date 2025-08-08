using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridWIthSlotsSOManager))]
public class GridWIthSlotsSOManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw default inspector
        DrawDefaultInspector();

        // Add reset button
        if (GUILayout.Button("Reset All Upgrades", GUILayout.Height(30)))
        {
            ((GridWIthSlotsSOManager)target).ResetSavedValues();
            Debug.Log("Upgrades reseted!");
        }      
        
        if (GUILayout.Button("Clear empty slots in list", GUILayout.Height(30)))
        {
            ((GridWIthSlotsSOManager)target).ClearEmpty();
            Debug.Log("List cleared!");
        }
    }
}