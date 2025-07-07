using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemSaveData
{
    public ItemSO ItemSO;
    public List<Vector2Int> ShapeOffsets = new();
    public Vector2Int InitialCellPostion;
    public int Value;
    public ItemSaveData(ItemSO itemSO, List<Vector2Int> shapeOffsets, Vector2Int initialCellPostioninitialCell, int value)
    {
        ItemSO = itemSO;
        ShapeOffsets = new(shapeOffsets);
        InitialCellPostion = initialCellPostioninitialCell;
        Value = value;
    }
}
