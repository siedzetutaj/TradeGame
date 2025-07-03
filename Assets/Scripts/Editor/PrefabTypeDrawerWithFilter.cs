using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(PrefabTypeAttribute))]
public class PrefabTypeDrawerWithFilter : PropertyDrawer
{
    private bool dropdownOpen = false; // Track dropdown state
    private Vector2 scrollPos;        // Scroll position for the dropdown list

    // Control the height of the property dynamically
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Default height for the object field
        float height = EditorGUIUtility.singleLineHeight;

        // Add extra space if the dropdown is open
        if (dropdownOpen)
        {
            height += 150; // Reserve space for the dropdown (adjust as needed)
        }

        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Get the attribute and the required type
        PrefabTypeAttribute prefabTypeAttribute = (PrefabTypeAttribute)attribute;
        System.Type requiredType = prefabTypeAttribute.RequiredType;

        // Calculate the object field area
        Rect fieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        // Display the label and dropdown button
        EditorGUI.LabelField(fieldRect, label);
        if (GUI.Button(new Rect(fieldRect.x + EditorGUIUtility.labelWidth, fieldRect.y, fieldRect.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight),
            property.objectReferenceValue ? property.objectReferenceValue.name : "None (GameObject)", EditorStyles.objectField))
        {
            dropdownOpen = !dropdownOpen; // Toggle dropdown
        }

        // Display the dropdown list if it's open
        if (dropdownOpen)
        {
            // Calculate dropdown area
            Rect dropdownArea = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, 150);
            GUI.Box(dropdownArea, GUIContent.none, EditorStyles.helpBox);

            // Get the list of valid prefabs
            List<GameObject> validPrefabs = GetValidPrefabs(requiredType);

            // Add scrollable dropdown content
            Rect scrollViewRect = new Rect(dropdownArea.x, dropdownArea.y + 5, dropdownArea.width, dropdownArea.height - 10);
            scrollPos = GUI.BeginScrollView(scrollViewRect, scrollPos, new Rect(0, 0, dropdownArea.width - 20, validPrefabs.Count * EditorGUIUtility.singleLineHeight));

            // Display each valid prefab
            for (int i = 0; i < validPrefabs.Count; i++)
            {
                if (GUI.Button(new Rect(5, i * EditorGUIUtility.singleLineHeight, dropdownArea.width - 20, EditorGUIUtility.singleLineHeight), validPrefabs[i].name, EditorStyles.miniButton))
                {
                    property.objectReferenceValue = validPrefabs[i];
                    dropdownOpen = false; // Close dropdown
                    GUI.FocusControl(null); // Remove focus from the dropdown
                }
            }

            GUI.EndScrollView();
        }
    }

    /// <summary>
    /// Finds all prefabs in the project that match the required type.
    /// </summary>
    private List<GameObject> GetValidPrefabs(System.Type requiredType)
    {
        List<GameObject> validPrefabs = new List<GameObject>();

        // Get all prefab paths
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null && PrefabContainsRequiredComponent(prefab, requiredType))
            {
                validPrefabs.Add(prefab);
            }
        }

        return validPrefabs;
    }

    /// <summary>
    /// Checks if the prefab or any of its children contain the required component.
    /// </summary>
    private bool PrefabContainsRequiredComponent(GameObject prefab, System.Type requiredType)
    {
        if (prefab == null) return false;

        // Check if the prefab itself has the component
        if (prefab.GetComponent(requiredType) != null)
        {
            return true;
        }

        // Check all children for the component
        foreach (Transform child in prefab.transform)
        {
            if (child.GetComponent(requiredType) != null)
            {
                return true;
            }
        }

        return false;
    }
}
