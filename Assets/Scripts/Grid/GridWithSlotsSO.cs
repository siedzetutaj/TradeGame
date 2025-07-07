using System;
using System.Collections.Generic;
using UnityEngine;

public class GridWithSlotsSO : ScriptableObject
{
    public string GridName;
    public SerializableDictionary<ItemSO, int> ItemsToPlaceIn = new();
    public List<ItemSaveData> ItemsPlacedIn = new();
    public List<int> CellsPerRow = new List<int> { 3, 4, 2 }; 
    public List<Vector2Int> DisableCellsAt = new();
    public UpgradeType UpgradeType = UpgradeType.Debug;
    public int UpgradeValue = 0;
    public bool IsUpgraded = false;

}
