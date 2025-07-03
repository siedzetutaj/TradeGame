using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VendorItemGeneratorSO))]
public class VendorItemGeneratorSOEditor : Editor
{
    // Define colors for each group
    private static readonly Color Group3Color = new Color(1.0f, 0.8f, 0.8f, 1.0f); // Light Red (Last 2 variables)
    private static readonly Color Group2Color = new Color(0.9f, 1.0f, 0.8f, 1.0f); // Light Green (Next 2 variables)
    private static readonly Color Group1Color = new Color(0.8f, 0.9f, 1.0f, 1.0f); // Light Blue (First 3 variables)

    public override void OnInspectorGUI()
    {
        // Get the target script
        VendorItemGeneratorSO script = (VendorItemGeneratorSO)target;

        // Start tracking the property index
        int propertyIndex = 0;

        // Draw the default inspector but with custom backgrounds
        SerializedProperty iterator = serializedObject.GetIterator();
        bool enterChildren = true;

        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;

            // Skip the "m_Script" field (it's the script reference)
            if (iterator.name == "m_Script")
            {
                EditorGUILayout.PropertyField(iterator, true);
                continue;
            }

            // Assign colors based on the property index
            if (propertyIndex < 3) // First 3 variables
            {
                GUI.backgroundColor = Group1Color;
            }
            else if (propertyIndex < 5) // Next 2 variables
            {
                GUI.backgroundColor = Group2Color;
            }
            else // Last 2 variables
            {
                GUI.backgroundColor = Group3Color;
            }

            // Draw the property with the custom background color
            EditorGUILayout.PropertyField(iterator, true);

            // Increment the property index
            propertyIndex++;

            // Reset the background color
            GUI.backgroundColor = Color.white;
        }

        // Apply changes to the serialized object
        serializedObject.ApplyModifiedProperties();
    }
}