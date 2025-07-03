using System.Collections.Generic;
using UnityEngine;
public enum GridType
{
    none,
    caravan,
    vendorToSell,
    vendorToBuy,
    chest,
    gridWithSlots,
    upgrade,
    debug
}
public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int cellWidth = 100;
    public int cellHeight = 100;
    public List<int> cellsPerRow = new List<int> { 3, 4, 2 }; // Customize row counts
    public List<Vector2Int> disableCellsAt= new (); // Customize row counts
    
    public Vector2Int gridStartPosition = Vector2Int.zero; // Starting position of the grid
    public Vector2Int gridEndPosition = Vector2Int.zero; // Starting position of the grid
    protected GridBounds gridBounds;

    public GridType gridType = GridType.caravan;
    public Transform BGHolder;

    public bool IsCaravan = false;

    [Header("Grid Visualization")]
    public GameObject cellPrefab; // Prefab with a SpriteRenderer or UI Image to represent each cell
    public Transform gridParent;  // Parent object for all grid cells in the hierarchy

    public bool GridCanBeEndless = false;

    [SerializeField] public List<List<GridCell>> grid = new List<List<GridCell>>();
    protected List<GameObject> visualCells = new List<GameObject>(); // List to store created cell visuals

    protected virtual void OnEnable()
    {
        if (!GridManagerRegistry.Instance.gridsBounds.ContainsValue(this))
        {
            InitializeGrid();
        }
    }
    protected virtual void OnDisable()
    {
        if (!IsCaravan)
        {
            if(gridBounds != null)
            {
                GridManagerRegistry.Instance.gridsBounds.Remove(gridBounds); // Unregister this grid from the list when it is disabled
            }
        }
    }
    public void InitializeGrid()
    {
        ClearGrid();

        grid.Clear();
        int yOffset = gridStartPosition.y;

        for (int row = 0; row < cellsPerRow.Count; row++)
        {
            CreateRow(row, ref yOffset);
        }
        UpdateGridBounds();
    }
    protected void CreateRow(int row, ref int yOffset)
    {
        List<GridCell> currentRow = new List<GridCell>();
        int xOffset = gridStartPosition.x;

        for (int col = 0; col < cellsPerRow[row]; col++)
        {
            Vector2Int cellListPos = new Vector2Int(row, col);
            Vector2Int cellPosition = new Vector2Int(xOffset, yOffset);

            // Create cell regardless of disabled status
            GridCell newCell = new GridCell(cellPosition, cellListPos);
            newCell.isDisabled = IsCellDisabled(cellListPos);
            currentRow.Add(newCell);

            // Only create visual for non-disabled cells
            if (!newCell.isDisabled)
            {
                GameObject cellObject = Instantiate(cellPrefab, (Vector2)cellPosition, Quaternion.identity, BGHolder);
                cellObject.transform.localScale = new Vector3(cellWidth, cellHeight, 1);
                visualCells.Add(cellObject);
            }

            xOffset += cellWidth;
        }

        grid.Add(currentRow);
        yOffset -= cellHeight;
    }
    protected bool IsCellDisabled(Vector2Int cellPos)
    {
        return disableCellsAt.Contains(cellPos);
    }
    protected void UpdateGridBounds()
    {
        int lastRowIndex = grid.Count - 1;
        int lastColIndex = grid[lastRowIndex].Count - 1;

        gridEndPosition = new Vector2Int(
            gridStartPosition.x + (lastColIndex * cellWidth),
            gridStartPosition.y - (lastRowIndex * cellHeight)
        );

        gridBounds = new GridBounds(gridStartPosition, gridEndPosition);
        GridManagerRegistry.Instance.gridsBounds.Add(gridBounds, this);
    }
    public void ExpandGrid()
    {
        if (!GridCanBeEndless) return;

        int newRowIndex = grid.Count;
        int yOffset = gridStartPosition.y - (newRowIndex * cellHeight);
        cellsPerRow.Add(cellsPerRow[cellsPerRow.Count - 1]); // Copy last row structure
        CreateRow(newRowIndex, ref yOffset);
        UpdateGridBounds();
    }
    #region ToItemData
    public GridCell GetNearestCell(Vector2 position)
    {
        GridCell nearestCell = null;
        float smallestDistance = float.MaxValue;

        foreach (var row in grid)
        {
            foreach (var cell in row)
            {
                float distance = Vector2.Distance(cell.position, position);
                if (distance < smallestDistance)
                {
                    smallestDistance = distance;
                    nearestCell = cell;
                }
            }
        }

        return nearestCell;
    }
    public GridCell FindSpotToPlaceItem(List<Vector2Int> itemShapeOffsets)
    {
        foreach (var row in grid)
        {
            foreach (var cell in row)
            {
                if (cell.isOccupied || !IsWithinBounds(itemShapeOffsets, cell.listPosition))
                    continue;
                return cell;
            }
        }

        if (GridCanBeEndless)
        {
            ExpandGrid();
            return FindSpotToPlaceItem(itemShapeOffsets);
        }
        return null;
    }
    public bool IsWithinBounds(List<Vector2Int> itemShapeOffsets, Vector2Int listPosition)
    {
        foreach (Vector2Int offset in itemShapeOffsets)
        {
            Vector2Int cellPosition = listPosition + new Vector2Int(-offset.y, offset.x);

            // First check if position is within grid dimensions
            if (cellPosition.x < 0 || cellPosition.x >= grid.Count)
                return false;
            if (cellPosition.y < 0 || cellPosition.y >= cellsPerRow[cellPosition.x])
                return false;

            // Then check if cell is disabled or occupied
            GridCell cell = GetCellAtPosition(cellPosition);
            if (cell == null || cell.isDisabled || cell.isOccupied)
                return false;
        }
        return true;
    }
    public GridCell GetCellAtPosition(Vector2Int position)
    {
        if (position.x >= 0 && position.x < grid.Count)
        {
            foreach (GridCell cell in grid[position.x])
            {
                if (cell.listPosition == position)
                {
                    return cell;
                }
            }
        }
        return null;
    }
    #endregion
    #region utilites
    public void ClearGrid()
    {
        foreach (var cell in visualCells)
        {
            if (cell != null)
            {
                DestroyImmediate(cell);
            }
        }
        visualCells.Clear();
    }
    #endregion
}
public class GridBounds
{
    public Vector2Int TopLeft { get; private set; }
    public Vector2Int BottomRight { get; private set; }
    public GridBounds(Vector2Int topLeft, Vector2Int bottomRight)
    {
        TopLeft = topLeft;
        BottomRight = bottomRight;
    }
    // Helper: Get the closest point inside the bounds to a given position
    public Vector2 ClampPositionToBounds(Vector2 position)
    {
        float clampedX = Mathf.Clamp(position.x, TopLeft.x, BottomRight.x);
        float clampedY = Mathf.Clamp(position.y, BottomRight.y, TopLeft.y); // Note: y decreases downward
        return new Vector2(clampedX, clampedY);
    }
    // Helper: Check if a position is inside the bounds
    public bool IsPositionInside(Vector2 position)
    {
        return position.x >= TopLeft.x && position.x <= BottomRight.x &&
               position.y <= TopLeft.y && position.y >= BottomRight.y;
    }
}
