using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VendorItemGeneratorSOManager))]

public class VendorItemGeneratorSOManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        VendorItemGeneratorSOManager vendorManager = (VendorItemGeneratorSOManager)target;
       
        if (GUILayout.Button("Populate list", GUILayout.Height(30)))
        {
            vendorManager.PopulateList();
        }
    }
}