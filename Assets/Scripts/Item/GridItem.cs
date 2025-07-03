using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class GridItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{

    public GridType GridType;
    public bool ItemAcquired = false;
    public ItemSO ItemSO;
    public Vector3 InitialPosition;
    public List<Vector2Int> ShapeOffsets = new List<Vector2Int>(); // Offsets for shape cells
    public GridCell Initialcell;

    [SerializeField] protected GridManager _gridManager;


    protected Canvas _canvas;
    protected RectTransform _rectTransform;
    protected Vector3 _offset;
    protected string _bgAddress = "itembg";
    protected List<GameObject> _bgCells = new();

    protected float _lastClickTime = 0f;
    protected const float _doubleClickThreshold = 0.3f;

    protected List<Vector2Int> _tempShapeOffsets = new();
    protected List<GameObject> _graphicOffsets = new();
    
    #region Setup
    public virtual void Initialize(ItemSO itemSO, bool isItemAcquired,
        GridType type, GridManager manager)
    {
        ItemSO = itemSO;
        ItemAcquired = isItemAcquired;
        _gridManager = manager;
        GridType = type;

        ShapeOffsets = new(ItemSO.shapeOffsets);
        _tempShapeOffsets = new(ShapeOffsets);

        GraphicOffsetSetup();

        GetComponent<Image>().sprite = ItemSO.sprite;
    }
    protected void OnEnable()
    {

        _canvas = GetComponentInParent<Canvas>();
        _rectTransform = GetComponent<RectTransform>();
    }

    #endregion

    #region Drag
    protected void OnDoubleClick()
    {
        TradeReferences tradeReference = TradeReferences.Instance;
        ClearOccupiedCells();
        switch (GridType)
        {
            case (GridType.caravan):
                {
                    if (tradeReference.VendorToBuyGrid.gameObject.activeInHierarchy && !ItemAcquired)
                    {
                        TryAutomaticPlacement(tradeReference.VendorToBuyGrid);
                        TradeMechanic.Instance.CalculatePrice(this);
                        CaravanManager.Instance.ChangeWeightWalue(-ItemSO.weight);

                    }
                    else if (tradeReference.VendorToSellGrid.gameObject.activeInHierarchy && ItemAcquired)
                    {
                        TryAutomaticPlacement(tradeReference.VendorToSellGrid);
                        TradeMechanic.Instance.CalculatePrice(this);
                        CaravanManager.Instance.ChangeWeightWalue(-ItemSO.weight);
                    }
                    else if (tradeReference.ChestGrid.gameObject.activeInHierarchy)
                    {
                        if (TryAutomaticPlacement(tradeReference.ChestGrid))
                        {
                            TradeMechanic.Instance.ActiveItemGenerator.OnItemReturned(this);
                            CaravanManager.Instance.ChangeWeightWalue(-ItemSO.weight);
                        }
                    }else if (tradeReference.UpgradeGrid.gameObject.activeInHierarchy)
                    {
                        if (TryAutomaticPlacementForUpgrade(tradeReference.UpgradeGrid))
                        {
                        }
                        else
                        {
                            ItemCanNotBePlacedInThisGrid();
                        }
                    }

                    return;
                }
            case (GridType.vendorToBuy):
                {
                    if (!CaravanManager.Instance.IsHeavierThenCaravanCapacity(ItemSO.weight))
                    {
                        if (TryAutomaticPlacement(tradeReference.CaravanGrid))
                        {
                            TradeMechanic.Instance.CalculatePrice(this);
                        }
                        else
                        {
                            CaravanManager.Instance.ChangeWeightWalue(-ItemSO.weight);
                        }
                    }
                    else
                    {
                        ItemCanNotBePlacedInThisGrid();
                    }
                    return;
                }
            case (GridType.vendorToSell):
                {
                    if (!CaravanManager.Instance.IsHeavierThenCaravanCapacity(ItemSO.weight))
                    {
                        if (TryAutomaticPlacement(tradeReference.CaravanGrid))
                        {
                            TradeMechanic.Instance.CalculatePrice(this);
                        }
                        else
                        {
                            CaravanManager.Instance.ChangeWeightWalue(-ItemSO.weight);
                        }
                    }
                    else
                    {
                        ItemCanNotBePlacedInThisGrid();
                    }
                    return;
                }
            case (GridType.chest):
                {
                    if (!CaravanManager.Instance.IsHeavierThenCaravanCapacity(ItemSO.weight))
                    {
                        if (TryAutomaticPlacement(tradeReference.CaravanGrid))
                        {
                            TradeMechanic.Instance.ActiveItemGenerator.OnItemAcquired(this);
                        }
                        else
                        {
                            CaravanManager.Instance.ChangeWeightWalue(-ItemSO.weight);
                        }
                    }
                    else
                    {
                        ItemCanNotBePlacedInThisGrid();
                    }
                    return;
                }
            case (GridType.upgrade):
                {
                    if (!CaravanManager.Instance.IsHeavierThenCaravanCapacity(ItemSO.weight))
                    {
                        if (TryAutomaticPlacementForUpgrade(tradeReference.CaravanGrid))
                        {
                            TradeMechanic.Instance.ActiveItemGenerator.OnItemAcquired(this);
                        }
                        else
                        {
                            CaravanManager.Instance.ChangeWeightWalue(-ItemSO.weight);
                            ItemCanNotBePlacedInThisGrid();
                        }
                    }
                    else
                    {
                        ItemCanNotBePlacedInThisGrid();
                    }
                    return;
                }
            default:
                ItemCanNotBePlacedInThisGrid();
                return;
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (Time.time - _lastClickTime < _doubleClickThreshold)
        {
            // Double-click detected
            OnDoubleClick();
        }

        // Update the last click time
        _lastClickTime = Time.time;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Store the initial position of the item in case we need to reset it
        InitialPosition = _rectTransform.position;

        if (_gridManager != null)
        {
            ClearOccupiedCells();
        }

        // Calculate the pointer offset from the item's position
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            (RectTransform)_canvas.transform, eventData.position, null, out var worldPoint);
        _offset = _rectTransform.position - worldPoint;

        _tempShapeOffsets = new(ShapeOffsets);

        InputSystem input = InputSystem.Instance;
        input.onRotateRightAction += RotateRight;
        input.onRotateLeftAction += RotateLeft;
        input.onMirrorAction += Mirror;

        if(GridType == GridType.debug)
        {
            InputSystem.Instance.onVendorAction += DestroyItem;
        }
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            (RectTransform)_canvas.transform, eventData.position, null, out var worldPoint))
        {
            _rectTransform.position = worldPoint + _offset;
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        RemoveDragActions();
        if (GridType == GridType.debug)
        {
            InputSystem.Instance.onVendorAction -= DestroyItem;
        }
        GridManager gridManager = GridManagerRegistry.Instance.GetNearestGrid(transform.position);
        if (gridManager == null)
        {
            Destroy(gameObject);
            return;
        }
        GridCell initialCell = gridManager.GetNearestCell(transform.position);
        GridType newGridType = gridManager.gridType;

        TradeReferences tradeReference = TradeReferences.Instance;

        if (gridManager.IsWithinBounds(_tempShapeOffsets, initialCell.listPosition)
            && initialCell != null
            && !initialCell.isOccupied)
        {
            if (newGridType == GridType)
            {
                ItemTransitionSetup(gridManager, initialCell);
                return;
            }
            switch (newGridType)
            {
                case (GridType.caravan):
                    {
                        if (!CaravanManager.Instance.IsHeavierThenCaravanCapacity(ItemSO.weight))
                        {
                            if (GridType == GridType.vendorToBuy && !ItemAcquired)
                            {
                                ItemTransitionSetup(gridManager, initialCell);
                                TradeMechanic.Instance.CalculatePrice(this);
                            }
                            else if (GridType == GridType.vendorToSell && ItemAcquired)
                            {
                                ItemTransitionSetup(gridManager, initialCell);
                                TradeMechanic.Instance.CalculatePrice(this);
                            }
                            else if (GridType == GridType.chest)
                            {
                                ItemTransitionSetup(gridManager, initialCell);
                                TradeMechanic.Instance.ActiveItemGenerator.OnItemAcquired(this);
                            }
                            else if (GridType == GridType.upgrade)
                            {
                                TryUpgradeItemTransitionSetup(gridManager, initialCell);
                            }
                        }
                        else
                        {
                            ItemCanNotBePlacedInThisGrid();
                        }
                        return;
                    }
                case (GridType.vendorToBuy):
                    {
                        if (GridType == GridType.caravan && !ItemAcquired)
                        {
                            ItemTransitionSetup(gridManager, initialCell);
                            TradeMechanic.Instance.CalculatePrice(this);
                            CaravanManager.Instance.ChangeWeightWalue(-ItemSO.weight);
                        }
                        else
                        {
                            ItemCanNotBePlacedInThisGrid();
                        }
                        return;
                    }
                case (GridType.vendorToSell):
                    {
                        if (GridType == GridType.caravan && ItemAcquired)
                        {
                            ItemTransitionSetup(gridManager, initialCell);
                            TradeMechanic.Instance.CalculatePrice(this);
                            CaravanManager.Instance.ChangeWeightWalue(-ItemSO.weight);
                        }
                        else
                        {
                            ItemCanNotBePlacedInThisGrid();
                        }
                        return;
                    }
                case (GridType.chest):
                    {
                        if (GridType == GridType.caravan)
                        {
                            ItemTransitionSetup(gridManager, initialCell);
                            TradeMechanic.Instance.ActiveItemGenerator.OnItemReturned(this);
                            CaravanManager.Instance.ChangeWeightWalue(-ItemSO.weight);
                        }
                        else
                        {
                            ItemCanNotBePlacedInThisGrid();
                        }
                        return;
                    }
                case (GridType.upgrade):
                    {
                        if (GridType == GridType.caravan)
                        {
                            TryUpgradeItemTransitionSetup(gridManager, initialCell);
                        }
                        else
                        {
                            ItemCanNotBePlacedInThisGrid();
                        }
                        return;
                    }

                default:
                    ItemCanNotBePlacedInThisGrid();
                    return;
            }
        }
        else
        {
            // Reset to initial position if placement is invalid
            ItemCanNotBePlacedInThisGrid();
        }
    }

    #endregion

    #region Utilities
    private void RemoveDragActions()
    {
        InputSystem input = InputSystem.Instance;
        input.onRotateRightAction -= RotateRight;
        input.onRotateLeftAction -= RotateLeft;
        input.onMirrorAction -= Mirror;
    }
    protected void RotateLeft()
    {
        List<Vector2Int> RotatedOffsets = new();

        for (int i = 0; i < _tempShapeOffsets.Count; i++)
        {
            Vector2Int cord = _tempShapeOffsets[i];
            Vector2Int newCord = new Vector2Int(-cord.y, cord.x);
            RotatedOffsets.Add(newCord);
            GraphicOffsetUpdate(i, newCord);
        }
        _tempShapeOffsets = new(RotatedOffsets);
    }
    protected void RotateRight()
    {
        List<Vector2Int> RotatedOffsets = new();

        for (int i = 0; i < _tempShapeOffsets.Count; i++)
        {
            Vector2Int cord = _tempShapeOffsets[i];
            Vector2Int newCord = new Vector2Int(cord.y, -cord.x);
            RotatedOffsets.Add(newCord);
            GraphicOffsetUpdate(i, newCord);
        }
        _tempShapeOffsets = new(RotatedOffsets);
    }
    protected void Mirror()
    {
        List<Vector2Int> MirroredOffsets = new();

        for (int i = 0; i < _tempShapeOffsets.Count; i++)
        {
            Vector2Int cord = _tempShapeOffsets[i];
            Vector2Int newCord = new Vector2Int(-cord.x, cord.y);
            MirroredOffsets.Add(newCord);
            GraphicOffsetUpdate(i, newCord);
        }
        _tempShapeOffsets = new(MirroredOffsets);
    }
    public void GraphicOffsetUpdate(int pos, Vector2Int newOffset)
    {
        _graphicOffsets[pos].GetComponent<RectTransform>().anchoredPosition = newOffset * 100;
    }
    protected void GraphicOffsetSetup()
    {
        GameObject GridItemPrefab = TradeReferences.Instance.GridItemBackgroundPrefab;
        foreach (Vector2Int offset in ShapeOffsets)
        {
            GameObject prefab = Instantiate(GridItemPrefab, transform);
            prefab.GetComponent<RectTransform>().anchoredPosition = offset * 100;
            _graphicOffsets.Add(prefab);
        }
    }
    public bool TryUpgradeItemTransitionSetup(GridManager gridManager, GridCell initialCell)
    {
        if (gridManager is GridManagerWithSlots gridWithSlots)
        {
            if (gridWithSlots.AddItemToGrid(this))
            {
                CaravanManager.Instance.ChangeWeightWalue(-ItemSO.weight);
                ItemTransitionSetup(gridManager, initialCell);
                return true;
            }
        }
        else if (_gridManager is GridManagerWithSlots _gridWithSlots)
        {
            _gridWithSlots.RemoveItemFromGrid(this);

            ItemTransitionSetup(gridManager, initialCell);
            return true;
        }
        ItemCanNotBePlacedInThisGrid();
        return false;
    }
    public void ItemTransitionSetup(GridManager gridManager, GridCell initialCell)
    {
        ShapeOffsets = new(_tempShapeOffsets);
        _gridManager = gridManager;
        Initialcell = initialCell;
        Initialcell.isOccupied = true;
        transform.position = (Vector2)Initialcell.position;
        GridType = gridManager.gridType;

        foreach (GridCell cell in GetOccupiedCells())
        {
            cell.isOccupied = true;
        }
    }
    protected  void ItemCanNotBePlacedInThisGrid()
    {
        _rectTransform.position = InitialPosition;
        ResetShapeOffsets();
        ItemTransitionSetup(_gridManager, Initialcell);
    }
    public void ResetShapeOffsets()
    {
        for (int i = 0; i < ShapeOffsets.Count; i++)
        {
            Vector2Int cord = ShapeOffsets[i];
            GraphicOffsetUpdate(i, cord);
        }
        _tempShapeOffsets = new(ShapeOffsets);
    }
    public List<GridCell> GetOccupiedCells()
    {
        List<GridCell> occupiedCells = new List<GridCell>();

        foreach (Vector2Int offset in ShapeOffsets)
        {
            int rowIndex = GetRowIndex(Initialcell) - offset.y;
            if (rowIndex >= 0 && rowIndex < _gridManager.cellsPerRow.Count)
            {
                int colIndex = GetColumnIndex(Initialcell) + offset.x;
                if (colIndex >= 0 && colIndex < _gridManager.grid[rowIndex].Count)
                {
                    occupiedCells.Add(_gridManager.grid[rowIndex][colIndex]);
                }
            }
        }
        return occupiedCells;
    }
    protected int GetRowIndex(GridCell cell)
    {
        for (int i = 0; i < _gridManager.grid.Count; i++)
        {
            if (_gridManager.grid[i].Contains(cell)) return i;
        }
        return -1;
    }
    protected int GetColumnIndex(GridCell cell)
    {
        for (int i = 0; i < _gridManager.grid.Count; i++)
        {
            int index = _gridManager.grid[i].IndexOf(cell);
            if (index >= 0) return index;
        }
        return -1;
    }
    public void ClearOccupiedCells()
    {
        foreach (GridCell cell in GetOccupiedCells())
        {
            cell.isOccupied = false;
        }
    }
    public bool TryAutomaticPlacementForUpgrade(GridManager gridManager)
    {
        GridCell cellToPlaceItem = gridManager.FindSpotToPlaceItem(ShapeOffsets);
        if (cellToPlaceItem != null)
        {
            return TryUpgradeItemTransitionSetup(gridManager, cellToPlaceItem); 
        }
        return false;
    }
    public bool TryAutomaticPlacement(GridManager gridManager)
    {

        GridCell cellToPlaceItem = gridManager.FindSpotToPlaceItem(ShapeOffsets);
        if (cellToPlaceItem != null)
        {
            ItemTransitionSetup(gridManager, cellToPlaceItem);
            return true;
        }
        ItemCanNotBePlacedInThisGrid();
        return false;
    }
    public virtual void DestroyItem()
    {
        InputSystem.Instance.onVendorAction -= DestroyItem;
        RemoveDragActions();
        ClearOccupiedCells();
        Destroy(gameObject);
    }
    public void SetShapeOffsets(List<Vector2Int> shapeOffsets)
    {
        ShapeOffsets = new(shapeOffsets);
        _tempShapeOffsets = new(shapeOffsets);

        GraphicOffsetSetup();
    }
    #endregion
}
