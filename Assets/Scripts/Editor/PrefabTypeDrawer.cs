using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(PrefabTypeAttribute))]
public class PrefabTypeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Get the attribute data
        PrefabTypeAttribute prefabTypeAttribute = (PrefabTypeAttribute)attribute;

        // Draw the ObjectField and validate the prefab selection
        EditorGUI.BeginProperty(position, label, property);

        // Draw the object field with the current value
        GameObject selectedPrefab = property.objectReferenceValue as GameObject;
        property.objectReferenceValue = EditorGUI.ObjectField(position, label, selectedPrefab, typeof(GameObject), false);

        // Validate the selection
        if (property.objectReferenceValue != null)
        {
            GameObject obj = property.objectReferenceValue as GameObject;

            if (obj != null && !PrefabContainsRequiredComponent(obj, prefabTypeAttribute.RequiredType))
            {
                // If the prefab is invalid, show a warning and clear the field
                Debug.LogWarning($"The selected prefab or its children must have a component of type {prefabTypeAttribute.RequiredType.Name}");
                property.objectReferenceValue = null;
            }
        }

        EditorGUI.EndProperty();
    }

    /// <summary>
    /// Checks if the GameObject or its children contain the required component.
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
