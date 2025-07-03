using UnityEngine;
[System.Serializable]
public class GridCell
{
    public Vector2Int position;
    public Vector2Int listPosition;
    public bool isOccupied;
    public GameObject cellObject;
    public bool isDisabled;
    public GridCell(Vector2Int position, Vector2Int listPosition)
    {
        this.position = position;
        this.listPosition = listPosition;
        this.isOccupied = false;
        this.isDisabled = false;
    }
}
