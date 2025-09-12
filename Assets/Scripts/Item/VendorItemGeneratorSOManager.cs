using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

[CreateAssetMenu(fileName = "Vendor Item Generator SO Manager", menuName = "ScriptableObjects/Vendor Item Generator SO Manager", order = 1)]
           
public class VendorItemGeneratorSOManager : ScriptableSingleton<VendorItemGeneratorSOManager>
#if UNITY_EDITOR
    , IPreprocessBuildWithReport
#endif
{
    public List<VendorItemGeneratorSO> characterList = new List<VendorItemGeneratorSO>();


    public void PopulateList()
    {
        string[] assetNames = AssetDatabase.FindAssets("", new[] { "Assets/SO/Vendors" });
        characterList.Clear();
        foreach (string SOName in assetNames)
        {
            var SOpath = AssetDatabase.GUIDToAssetPath(SOName);
            var character = AssetDatabase.LoadAssetAtPath<VendorItemGeneratorSO>(SOpath);
            characterList.Add(character);
        }
    }
#if UNITY_EDITOR
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        PopulateList();
    }
#endif
}
