using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Items Save Data", menuName = "ScriptableObjects/ItemsSaveDataSO", order = 1)]
public class ItemsSaveDataSO : ScriptableObject
{
    public List<ItemData> ItemsPlacedIn = new();
    public GameObject ItemPrefab;
    public bool IsSavingEnabled;
    public void ResetSavedValues()
    {
        ItemsPlacedIn.Clear();
    }
    public void ClearEmpty()
    {
        ItemsPlacedIn.RemoveAll(item => item == null);
    }

}
