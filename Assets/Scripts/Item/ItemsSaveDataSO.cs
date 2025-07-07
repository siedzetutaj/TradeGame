using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Items Save Data", menuName = "ScriptableObjects/ItemsSaveDataSO", order = 1)]
public class ItemsSaveDataSO : ScriptableObject
{
    public List<ItemSaveData> ItemsPlacedIn = new();
    public GameObject ItemPrefab; 
}
