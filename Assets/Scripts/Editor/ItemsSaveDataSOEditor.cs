using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(ItemsSaveDataSO))]
public class ItemsSaveDataSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw default inspector
        DrawDefaultInspector();

        // Add reset button
        if (GUILayout.Button("Remove All Saved Items", GUILayout.Height(30)))
        {
            ((ItemsSaveDataSO)target).ResetSavedValues();
            Debug.Log("Items Remove!");
        }

        if (GUILayout.Button("Clear empty slots in list", GUILayout.Height(30)))
        {
            ((ItemsSaveDataSO)target).ClearEmpty();
            Debug.Log("List cleared!");
        }
        if (GUILayout.Button("Disable/Enable Saving", GUILayout.Height(30)))
        {
            var data = (ItemsSaveDataSO)target;
            data.IsSavingEnabled = !data.IsSavingEnabled;
            Debug.Log($"Save is enabled: {data.IsSavingEnabled}");
        }
    }
}
