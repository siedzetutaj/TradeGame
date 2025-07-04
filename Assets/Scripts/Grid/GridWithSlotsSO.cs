using System;
using System.Collections.Generic;
using UnityEngine;

public class GridWithSlotsSO : ScriptableObject
{
    public string GridName;
    public SerializableDictionary<ItemSO, int> ItemsToPlaceIn = new();
    public List<ItemForUpgradeData> ItemsPlacedIn = new();
    public List<int> CellsPerRow = new List<int> { 3, 4, 2 }; 
    public List<Vector2Int> DisableCellsAt = new();
    public UpgradeType UpgradeType = UpgradeType.Debug;
    public int UpgradeValue = 0;
    public bool IsUpgraded = false;

}
[Serializable]
public class ItemForUpgradeData
{
    public ItemSO ItemSO;
    public List<Vector2Int> ShapeOffsets = new();
    public Vector2Int InitialCellPostion;
    public int Value;
    public ItemForUpgradeData(ItemSO itemSO, List<Vector2Int> shapeOffsets, Vector2Int initialCellPostioninitialCell, int value)
    {
        ItemSO = itemSO;
        ShapeOffsets = new(shapeOffsets);
        InitialCellPostion = initialCellPostioninitialCell;
        Value = value;
    }
}