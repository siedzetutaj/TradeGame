using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemData
{
    public ItemSO ItemSO;
    public List<Vector2Int> ShapeOffsets = new();
    public Vector2Int InitialCellPostion;
    public int StackCount;
    public ItemData(ItemSO itemSO, List<Vector2Int> shapeOffsets, Vector2Int initialCellPostioninitialCell, int stackCount)
    {
        ItemSO = itemSO;
        ShapeOffsets = new(shapeOffsets);
        InitialCellPostion = initialCellPostioninitialCell;
        StackCount = stackCount;
    }
}
