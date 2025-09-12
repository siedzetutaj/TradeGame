using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class GridItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    /*
     * TODO:
     * Naciœniecie prawym i scrollowanie ¿eby wyci¹gn¹æ wiecej itemów
     * Upgrady - stackowanie nie jest wrzucone do tego (tryStack)
     * Co ze zwrotami ???
     * Pomarañczowy kolor i ogarniêcie DiffrentAcquiredStackCount
     */



    public GridType GridType;
    public bool ItemAcquired = false;
    public ItemSO ItemSO;
    public Vector3 InitialPosition;
    public List<Vector2Int> ShapeOffsets = new List<Vector2Int>(); // Offsets for shape cells
    public GridCell Initialcell;
    public int CurrentStackCount = 0;
    public bool IsItemStacked = false;


    [SerializeField] protected GridManager _gridManager;
    [SerializeField] protected TextMeshProUGUI _itemStackCounterTMP;
    [SerializeField] protected Image _itemImage;

    [NonSerialized] public GridItem OrginalStackedItem;
    [NonSerialized] public int DiffrentAcquiredStackCount;
    protected bool IsItemFromDiffrentAcquiredStack = false;

    protected Canvas _canvas;
    protected RectTransform _rectTransform;
    protected Vector3 _offset;

    protected float _lastClickTime = 0f;
    protected const float _doubleClickThreshold = 0.3f;

    protected List<Vector2Int> _tempShapeOffsets = new();
    protected List<GameObject> _graphicOffsets = new();
    protected TradeReferences _tradeReference;

    protected int StackCountBeforeStacking = 0;

    public readonly Color red = new Color(1f, 0f, 0f, 0.439f);
    public readonly Color green = new Color(0f, 1f, 0f, 0.439f);
    public readonly Color orange = new Color(1f, 0.5f, 0f, 0.439f);
    public readonly Color yellow = new Color(1f, 1f, 0f, 0.439f);
    public readonly Color defaultColor = new Color(0.745f, 0.745f, 0.745f, 0.439f);
    public Color currentColor;

    #region Setup
    public virtual void Initialize(ItemSO itemSO, bool isItemAcquired,
        GridType type, GridManager manager, int StackCount = 1)
    {
        CurrentStackCount = StackCount;
        ItemSO = itemSO;
        ItemAcquired = isItemAcquired;
        _gridManager = manager;
        GridType = type;

        OrginalStackedItem = null;

        ShapeOffsets = new(ItemSO.shapeOffsets);

        SimpleInitialize();

        _itemImage.sprite = ItemSO.sprite;
    }
    public void SimpleInitialize()
    {
        _tradeReference = TradeReferences.Instance;
        _tempShapeOffsets = new(ShapeOffsets);
        _gridManager.GridItems.Add(this);
        UpdateStackCounterTMP();
        GraphicOffsetSetup();
        currentColor = defaultColor;
        DiffrentAcquiredStackCount = 0;
    }
    protected virtual void OnEnable()
    {
        _canvas = GetComponentInParent<Canvas>();
        _rectTransform = GetComponent<RectTransform>();
    }
    protected void OnDisable()
    {
        RemoveDragActions();
        HandleDebugCleanup();
    }
    #endregion
    #region Inputs
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (Time.time - _lastClickTime < _doubleClickThreshold)
            {
                OnDoubleClick(true);
            }
            _lastClickTime = Time.time;

            return;
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (Time.time - _lastClickTime < _doubleClickThreshold)
            {
                OnDoubleClick(false);
            }
            _lastClickTime = Time.time;

            return;
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        InitialPosition = _rectTransform.position;
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            DragSetup(eventData);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if( CurrentStackCount <= 1)
            {
                DragSetup(eventData);
            }
            else
            {
                DragOneFromStack(eventData);
            }
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
        HandleDebugCleanup();

        GridManager gridManager = GridManagerRegistry.Instance.GetNearestGrid(transform.position);
        if (gridManager == null)
        {
            DestroyItem();
            return;
        }

        TryPlaceItem(gridManager,eventData.button == PointerEventData.InputButton.Right);
    }
    #endregion
    #region Main Funcionality
    private void HandleDebugCleanup()
    {
        if (GridType == GridType.debug)
        {
            InputSystem.Instance.onVendorAction -= DestroyItem;
        }
    }
    private void TryPlaceItem(GridManager gridManager, bool isLeftClick)
    {
        GridCell cell = gridManager.GetNearestCell(transform.position);

        GridItem itemToStack = TryStackItem(gridManager, cell);

        if (gridManager.gridType == GridType && itemToStack != null)
        {
            StackItem(gridManager, itemToStack);
            if (itemToStack.ItemAcquired != ItemAcquired || itemToStack.DiffrentAcquiredStackCount > 0)
            {
                itemToStack.SetBackgroundColor(orange);
                itemToStack.DiffrentAcquiredStackCount += CurrentStackCount;
            }
            return;
        }

        if (!IsValidPlacement(gridManager, cell) && itemToStack == null)
        {
            ItemCanNotBePlacedInThisGrid();
            return;
        }

        if (gridManager.gridType == GridType)
        {
            ItemTransitionSetup(gridManager, cell);
            return;
        }

        HandleGridSpecificPlacement(gridManager, cell, itemToStack);
    }
    private bool IsValidPlacement(GridManager gridManager, GridCell cell)
    {
        return cell != null
            && !cell.isOccupied
            && gridManager.IsWithinBounds(_tempShapeOffsets, cell.listPosition);
    }
    private void HandleGridSpecificPlacement(GridManager gridManager, GridCell cell, GridItem itemToStack)
    {
        var trade = TradeReferences.Instance;
        var caravan = CaravanManager.Instance;

        switch (gridManager.gridType)
        {
            case GridType.caravan when !caravan.IsHeavierThenCaravanCapacity(ItemSO.weight * CurrentStackCount):
                HandleCaravanPlacement(gridManager, cell, itemToStack);
                break;

            case GridType.vendorToBuy when GridType == GridType.caravan && (!ItemAcquired || DiffrentAcquiredStackCount > 0):
                CompleteTradePlacement(gridManager, cell, itemToStack, -ItemSO.weight * CurrentStackCount, defaultColor);
                break;

            case GridType.vendorToSell when GridType == GridType.caravan && (ItemAcquired || DiffrentAcquiredStackCount > 0):
                CompleteTradePlacement(gridManager, cell, itemToStack, -ItemSO.weight * CurrentStackCount, defaultColor);
                break;

            case GridType.chest when GridType == GridType.caravan:
                HandleChestPlacement(gridManager, cell, itemToStack);
                SetBackgroundColor(defaultColor);
                break;

            case GridType.upgrade when GridType == GridType.caravan:
                TryUpgradeItemTransitionSetup(gridManager, cell);
                break;

            default:
                ItemCanNotBePlacedInThisGrid();
                break;
        }
    }
    private void HandleCaravanPlacement(GridManager gridManager, GridCell cell, GridItem itemToStack)
    {
        if (GridType == GridType.vendorToBuy && !ItemAcquired)
        {
            CompleteTradePlacement(gridManager, cell, itemToStack, 0, red);
        }
        else if (GridType == GridType.vendorToSell && ItemAcquired)
        {
            CompleteTradePlacement(gridManager, cell, itemToStack, 0, green);
        }
        else if (GridType == GridType.chest)
        {
            SetBackgroundColor(green);
            if (itemToStack == null)
            {
                ItemTransitionSetup(gridManager, cell);
                TradeMechanic.Instance.ActiveItemGenerator.OnChestItemAcquired(this, CurrentStackCount);
            }
            else
            {
                StackItem(gridManager, itemToStack);             
                TradeMechanic.Instance.ActiveItemGenerator.OnChestItemAcquired(itemToStack, StackCountBeforeStacking);
            }
        }
        else if (GridType == GridType.upgrade)
        {
            TryUpgradeItemTransitionSetup(gridManager, cell);
        }
        else
        {
            ItemCanNotBePlacedInThisGrid();
        }
    }
    protected virtual void CompleteTradePlacement(GridManager gridManager, GridCell cell, GridItem itemToStack, int weightChange, Color color)
    {
        if (itemToStack == null) 
        {
            if (DiffrentAcquiredStateCheck(gridManager, cell))
                ItemTransitionSetup(gridManager, cell);
            else
                return;
        }
        else
        {
            StackItem(gridManager, itemToStack);
            if (itemToStack.ItemAcquired != ItemAcquired)
            {
                itemToStack.SetBackgroundColor(orange);
                itemToStack.DiffrentAcquiredStackCount += CurrentStackCount;
            }
            GridType = itemToStack.GridType;
        }

        SetBackgroundColor(color);
        CaravanManager.Instance.ChangeWeightWalue(weightChange);
        TradeMechanic.Instance.CalculatePrice(this, GridType, CurrentStackCount);
    }
    private void HandleChestPlacement(GridManager gridManager, GridCell cell, GridItem itemToStack)
    {
        if (itemToStack == null) 
        {
            ItemTransitionSetup(gridManager, cell);
        }
        else
        {
            StackItem(gridManager, itemToStack);
        }
        TradeMechanic.Instance.ActiveItemGenerator.OnChestItemReturned(this, CurrentStackCount);
        CaravanManager.Instance.ChangeWeightWalue(-ItemSO.weight * CurrentStackCount);
    }
    private void RemoveDragActions()
    {
        InputSystem input = InputSystem.Instance;
        input.onRotateRightAction -= RotateRight;
        input.onRotateLeftAction -= RotateLeft;
        input.onMirrorAction -= Mirror;
    }
    private void DragOneFromStack(PointerEventData eventData)
    {
        GridItem ClonedGridItem = CloningGridItem(1);

        if (DiffrentAcquiredStackCount > 0)
        {
            if (ItemAcquired)
                DiffrentAcquiredStateSetup(ClonedGridItem, false, red, 1);
            else
                DiffrentAcquiredStateSetup(ClonedGridItem, true, green, 1);

            if (DiffrentAcquiredStackCount == 0)
                SetBackgroundColor(ItemAcquired ? green : red);

        }
        ClonedGridItem.OrginalStackedItem = this;
        eventData.pointerDrag = ClonedGridItem.gameObject;

        ClonedGridItem.SettingUpDragVariables(eventData);
    }
    private bool DiffrentAcquiredStateCheck(GridManager gridManager, GridCell cell)
    {
        if (DiffrentAcquiredStackCount > 0)
        {
            if (gridManager.gridType == GridType.vendorToBuy)
            {
                GridItem ClonedGridItem = CloningGridItem(DiffrentAcquiredStackCount);
                ClonedGridItem.Initialcell = Initialcell;

                if (ItemAcquired)
                {
                    SetBackgroundColor(green);
                    DiffrentAcquiredStateSetup(ClonedGridItem, false, defaultColor, DiffrentAcquiredStackCount);
                    
                    ItemCanNotBePlacedInThisGrid();
                    
                    ClonedGridItem.ItemTransitionSetup(gridManager, cell);
                    ClonedGridItem.IsItemFromDiffrentAcquiredStack = false;

                    CaravanManager.Instance.ChangeWeightWalue(-ClonedGridItem.ItemSO.weight * ClonedGridItem.CurrentStackCount);
                    TradeMechanic.Instance.CalculatePrice(ClonedGridItem, ClonedGridItem.GridType, ClonedGridItem.CurrentStackCount);
                }
                else
                {
                    SetBackgroundColor(defaultColor);
                    DiffrentAcquiredStateSetup(ClonedGridItem, true, green, DiffrentAcquiredStackCount);
                    
                    ItemTransitionSetup(gridManager, cell);
                    
                    ClonedGridItem.ItemCanNotBePlacedInThisGrid();
                    ClonedGridItem.IsItemFromDiffrentAcquiredStack = false;

                    CaravanManager.Instance.ChangeWeightWalue(-ItemSO.weight * CurrentStackCount);
                    TradeMechanic.Instance.CalculatePrice(this, GridType, CurrentStackCount);
                }
                return false;
            }
            else if (gridManager.gridType == GridType.vendorToSell)
            {
                GridItem ClonedGridItem = CloningGridItem(DiffrentAcquiredStackCount);
                ClonedGridItem.Initialcell = Initialcell;

                if (ItemAcquired)
                {
                    SetBackgroundColor(defaultColor);
                    DiffrentAcquiredStateSetup(ClonedGridItem, false, red, DiffrentAcquiredStackCount);

                    ItemTransitionSetup(gridManager, cell);

                    ClonedGridItem.ItemCanNotBePlacedInThisGrid();
                    ClonedGridItem.IsItemFromDiffrentAcquiredStack = false;

                    CaravanManager.Instance.ChangeWeightWalue(-ItemSO.weight * CurrentStackCount);
                    TradeMechanic.Instance.CalculatePrice(this, GridType, CurrentStackCount);
                }
                else
                {
                    SetBackgroundColor(red);
                    DiffrentAcquiredStateSetup(ClonedGridItem, true, defaultColor, DiffrentAcquiredStackCount);
                   
                    ItemCanNotBePlacedInThisGrid();

                    ClonedGridItem.ItemTransitionSetup(gridManager, cell);
                    ClonedGridItem.IsItemFromDiffrentAcquiredStack = false;

                    CaravanManager.Instance.ChangeWeightWalue(-ClonedGridItem.ItemSO.weight * ClonedGridItem.CurrentStackCount);
                    TradeMechanic.Instance.CalculatePrice(ClonedGridItem, ClonedGridItem.GridType, ClonedGridItem.CurrentStackCount);
                }
                return false;
            }
        }
        return true;
    }
    private void DiffrentAcquiredStateSetup(GridItem ClonedGridItem, bool AcquiredState, Color color, int StackAmount)
    {
        ClonedGridItem.ItemAcquired = AcquiredState;
        ClonedGridItem.SetBackgroundColor(color);
        DiffrentAcquiredStackCount -= StackAmount;
        ClonedGridItem.IsItemFromDiffrentAcquiredStack = true;

        ClonedGridItem.CurrentStackCount = StackAmount;
        ClonedGridItem.UpdateStackCounterTMP();
    }
    private void DragSetup(PointerEventData eventData)
    {
        if (_gridManager != null)
        {
            ClearOccupiedCells();
        }
        SettingUpDragVariables(eventData);
        if (GridType == GridType.debug)
        {
            InputSystem.Instance.onVendorAction += DestroyItem;
        }
    }
    private void SettingUpDragVariables(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            (RectTransform)_canvas.transform, eventData.position, null, out var worldPoint);
        _offset = _rectTransform.position - worldPoint;

        _tempShapeOffsets = new(ShapeOffsets);

        InputSystem input = InputSystem.Instance;
        input.onRotateRightAction += RotateRight;
        input.onRotateLeftAction += RotateLeft;
        input.onMirrorAction += Mirror;
    }
    #endregion
    #region DoubleClick Funcionality
    protected void OnDoubleClick(bool isLeftClick)
    {
        switch (GridType)
        {
            case GridType.caravan:
                HandleCaravanGrid(isLeftClick);
                break;
            case GridType.vendorToBuy:
            case GridType.vendorToSell:
                HandleVendorGrids(isLeftClick);
                break;
            case GridType.chest:
            case GridType.upgrade:
                HandleStorageGrids(isLeftClick);
                break;
            default:
                ItemCanNotBePlacedInThisGrid();
                break;
        }
    }
    private void HandleCaravanGrid(bool isLeftClick)
    {
        if (_tradeReference.VendorToBuyGrid.gameObject.activeInHierarchy && !ItemAcquired)
        {
            HandleVendorPlacement(_tradeReference.VendorToBuyGrid, isLeftClick);
        }
        else if (_tradeReference.VendorToSellGrid.gameObject.activeInHierarchy && ItemAcquired)
        {
            HandleVendorPlacement(_tradeReference.VendorToSellGrid, isLeftClick);
        }
        else if (_tradeReference.ChestGrid.gameObject.activeInHierarchy)
        {
            if(TryAutomaticPlacement(_tradeReference.ChestGrid, isLeftClick))
            {
                SetBackgroundColor(defaultColor);

                if (isLeftClick)
                    CaravanManager.Instance.ChangeWeightWalue(-ItemSO.weight * CurrentStackCount);
                else
                    CaravanManager.Instance.ChangeWeightWalue(-ItemSO.weight);

                if (isLeftClick) 
                    TradeMechanic.Instance.ActiveItemGenerator.OnChestItemReturned(this, StackCountBeforeStacking);
                else
                    TradeMechanic.Instance.ActiveItemGenerator.OnChestItemReturned(this, 1, true);
            }
            ;
        }
        else if (_tradeReference.UpgradeGrid.gameObject.activeInHierarchy)
        {
            if (!TryAutomaticPlacementForUpgrade(_tradeReference.UpgradeGrid))
            {
                ItemCanNotBePlacedInThisGrid();
            }
        }
    }
    private void HandleVendorGrids(bool isLeftClick)
    {
        var tempGirdType = GridType;
        GridItem item = TryAutomaticPlacement(_tradeReference.CaravanGrid, isLeftClick);
        if (CanCarryItem(isLeftClick) && item)
        {
            if (item == this)
                SetBackgroundColor(tempGirdType == GridType.vendorToBuy ? red : green);
            else if (item.ItemAcquired != ItemAcquired || DiffrentAcquiredStackCount > 0)
            {
                item.SetBackgroundColor(orange);
                if(isLeftClick)
                    item.DiffrentAcquiredStackCount += CurrentStackCount;
                else
                    item.DiffrentAcquiredStackCount += 1;
            }
            else if (item.ItemAcquired == ItemAcquired)
                item.SetBackgroundColor(tempGirdType == GridType.vendorToBuy ? red : green);
            
            if (isLeftClick)
                TradeMechanic.Instance.CalculatePrice(this, GridType.caravan, StackCountBeforeStacking);
            else
                TradeMechanic.Instance.CalculatePrice(this, GridType.caravan, 1);
        }
        else
        {
            HandleFailedPlacement(isLeftClick);
        }
    }
    private void HandleStorageGrids(bool isLeftClick)
    {
        if (CanCarryItem(isLeftClick))
        {
            GridItem temp;
            bool success = GridType == GridType.upgrade
                ? temp = TryAutomaticPlacementForUpgrade(_tradeReference.CaravanGrid)
                : temp = TryAutomaticPlacement(_tradeReference.CaravanGrid, isLeftClick);

            if (success)
            {
                if (isLeftClick)
                {
                    TradeMechanic.Instance.ActiveItemGenerator.OnChestItemAcquired(temp, StackCountBeforeStacking);
                    SetBackgroundColor(green);

                }
                else
                {
                    SetBackgroundColor(green);
                    TradeMechanic.Instance.ActiveItemGenerator.OnChestItemAcquired(temp, 1);
                }
            }
            else
            {
                HandleFailedPlacement(isLeftClick);
            }
        }
        else
        {
            ItemCanNotBePlacedInThisGrid();
        }
    }
    private bool CanCarryItem(bool isLeftCLick)
    {
        if (isLeftCLick)
            return !CaravanManager.Instance.IsHeavierThenCaravanCapacity(ItemSO.weight * CurrentStackCount);
        else
            return !CaravanManager.Instance.IsHeavierThenCaravanCapacity(ItemSO.weight);
    }
    private void AdjustWeight(int amount) => CaravanManager.Instance.ChangeWeightWalue(amount);
    private void HandleFailedPlacement(bool isLeftCLick)
    {
        if(isLeftCLick)
            AdjustWeight(-ItemSO.weight * CurrentStackCount);
        else
            AdjustWeight(-ItemSO.weight);
    }
    private void HandleVendorPlacement(GridManager gridManager, bool isLeftClick)
    {
        if (DiffrentAcquiredStateAutomat(gridManager, isLeftClick))
        {
            return;
        }
        GridItem item = TryAutomaticPlacement(gridManager, isLeftClick);
        if (item)
        {
            if (item == this)
                SetBackgroundColor(defaultColor);
            else
                item.SetBackgroundColor(defaultColor); 

            if (isLeftClick)
            {
                TradeMechanic.Instance.CalculatePrice(this, gridManager.gridType, StackCountBeforeStacking);
                CaravanManager.Instance.ChangeWeightWalue(-item.ItemSO.weight * CurrentStackCount);
            }
            else
            {
                TradeMechanic.Instance.CalculatePrice(this, gridManager.gridType, 1);
                CaravanManager.Instance.ChangeWeightWalue(-item.ItemSO.weight);
            }
        }
    }
    private bool DiffrentAcquiredStateAutomat(GridManager gridManager, bool isLeftClick)
    {
        if (DiffrentAcquiredStackCount > 0)
        {
            if (gridManager.gridType == GridType.vendorToBuy)
            {
                GridItem ClonedGridItem = CloningGridItem(DiffrentAcquiredStackCount);
                ClonedGridItem.Initialcell = Initialcell;

                if (ItemAcquired)
                {
                    SetBackgroundColor(green);
                    if(isLeftClick)
                        DiffrentAcquiredStateSetup(ClonedGridItem, false, defaultColor, DiffrentAcquiredStackCount);
                    else
                        DiffrentAcquiredStateSetup(ClonedGridItem, false, defaultColor, 1);

                    ItemCanNotBePlacedInThisGrid();

                    ClonedGridItem.TryAutomaticPlacement(gridManager, true);
                    ClonedGridItem.IsItemFromDiffrentAcquiredStack = false;

                    CaravanManager.Instance.ChangeWeightWalue(-ClonedGridItem.ItemSO.weight * ClonedGridItem.CurrentStackCount);
                    TradeMechanic.Instance.CalculatePrice(ClonedGridItem, ClonedGridItem.GridType, ClonedGridItem.CurrentStackCount);
                }
                else
                {
                    SetBackgroundColor(defaultColor);
                    if(isLeftClick)
                        DiffrentAcquiredStateSetup(ClonedGridItem, true, green, DiffrentAcquiredStackCount);
                    else
                        DiffrentAcquiredStateSetup(ClonedGridItem, true, green, 1);

                     TryAutomaticPlacement(gridManager, true);

                    ClonedGridItem.ItemCanNotBePlacedInThisGrid();
                    ClonedGridItem.IsItemFromDiffrentAcquiredStack = false;

                    CaravanManager.Instance.ChangeWeightWalue(-ItemSO.weight * CurrentStackCount);
                    TradeMechanic.Instance.CalculatePrice(this, GridType, CurrentStackCount);
                }
                return true;    
            }
            else if (gridManager.gridType == GridType.vendorToSell)
            {
                GridItem ClonedGridItem = CloningGridItem(DiffrentAcquiredStackCount);
                ClonedGridItem.Initialcell = Initialcell;

                if (ItemAcquired)
                {
                    SetBackgroundColor(defaultColor);
                    if(isLeftClick)
                        DiffrentAcquiredStateSetup(ClonedGridItem, false, red, DiffrentAcquiredStackCount);
                    else
                        DiffrentAcquiredStateSetup(ClonedGridItem, false, red, 1);

                     TryAutomaticPlacement(gridManager, true);

                    ClonedGridItem.ItemCanNotBePlacedInThisGrid();
                    ClonedGridItem.IsItemFromDiffrentAcquiredStack = false;

                    CaravanManager.Instance.ChangeWeightWalue(-ItemSO.weight * CurrentStackCount);
                    TradeMechanic.Instance.CalculatePrice(this, GridType, CurrentStackCount);

                }
                else
                {
                    SetBackgroundColor(red);
                    if(isLeftClick)
                        DiffrentAcquiredStateSetup(ClonedGridItem, true, defaultColor, DiffrentAcquiredStackCount);
                    else
                        DiffrentAcquiredStateSetup(ClonedGridItem, true, defaultColor, 1);

                    ItemCanNotBePlacedInThisGrid();

                    ClonedGridItem.TryAutomaticPlacement(gridManager, true);
                    ClonedGridItem.IsItemFromDiffrentAcquiredStack = false;

                    CaravanManager.Instance.ChangeWeightWalue(-ClonedGridItem.ItemSO.weight * ClonedGridItem.CurrentStackCount);
                    TradeMechanic.Instance.CalculatePrice(ClonedGridItem, ClonedGridItem.GridType, ClonedGridItem.CurrentStackCount);
                }
                return true;
            }
        }
        return false;
    }
    #endregion
    #region ShapeOffests
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
    public void ResetShapeOffsets()
    {
        for (int i = 0; i < ShapeOffsets.Count; i++)
        {
            Vector2Int cord = ShapeOffsets[i];
            GraphicOffsetUpdate(i, cord);
        }
        _tempShapeOffsets = new(ShapeOffsets);
    }
    public void GraphicOffsetUpdate(int pos, Vector2Int newOffset)
    {
        _graphicOffsets[pos].GetComponent<RectTransform>().anchoredPosition = newOffset * 100;
    }
    protected void GraphicOffsetSetup()
    {
        // Clear previous graphics
        foreach (var go in _graphicOffsets)
        {
            Destroy(go);
        }
        _graphicOffsets.Clear();

        GameObject backgroundPrefab = TradeReferences.Instance.GridItemBackgroundPrefab;
        foreach (Vector2Int offset in ShapeOffsets)
        {
            GameObject prefab = Instantiate(backgroundPrefab, transform);
            prefab.GetComponent<RectTransform>().anchoredPosition = offset * 100;
            prefab.transform.SetAsFirstSibling();
            _graphicOffsets.Add(prefab);
        }
    }
    public void SetBackgroundColor(Color color)
    {
        currentColor = color;
        foreach (GameObject graphicOffset in _graphicOffsets)
        {
            Image image = graphicOffset.GetComponent<Image>();
            if (image != null)
            {
                image.color = color;
            }
        }
    }
    protected void ReadGraphicOffset()
    {
        foreach(Transform child in transform)
        {
            if (child.CompareTag("ItemBackground"))
            {
                _graphicOffsets.Add(child.gameObject);
            }
        }
    }
    public void SetShapeOffsets(List<Vector2Int> shapeOffsets)
    {
        ShapeOffsets = new(shapeOffsets);
        _tempShapeOffsets = new(shapeOffsets);

        GraphicOffsetSetup();
    }
    #endregion
    #region Placement
    public GridItem TryUpgradeItemTransitionSetup(GridManager newGridManager, GridCell initialCell)
    {
        if (newGridManager is GridManagerWithSlots gridWithSlots)
        {
            if (gridWithSlots.AddItemToGrid(this))
            {
                CaravanManager.Instance.ChangeWeightWalue(-ItemSO.weight);
                ItemTransitionSetup(newGridManager, initialCell);
                CaravanManager.Instance.GiveAwayItem(this);

                return this;
            }
        }
        else if (_gridManager is GridManagerWithSlots _gridWithSlots)
        {
            _gridWithSlots.RemoveItemFromGrid(this);

            ItemTransitionSetup(newGridManager, initialCell);

            CaravanManager.Instance.TakeItem(this);
            return this;
        }
        ItemCanNotBePlacedInThisGrid();
        return null;
    }
    public void ItemTransitionSetup(GridManager newGridManager, GridCell initialCell)
    {
        if (_gridManager != null && !_gridManager.GridItems.Contains(this))
        {
            _gridManager.GridItems.Add(this);
        }
       
        _gridManager.GridItems.Remove(this);
        ShapeOffsets = new(_tempShapeOffsets);
        _gridManager = newGridManager;
        Initialcell = initialCell;
        Initialcell.isOccupied = true;
        transform.position = (Vector2)Initialcell.position;
        GridType = newGridManager.gridType;

        foreach (GridCell cell in GetOccupiedCells())
        {
            cell.isOccupied = true;
        }
        _gridManager.GridItems.Add(this);
        OrginalStackedItem = null;
    }
    protected void ItemCanNotBePlacedInThisGrid()
    {
        if (OrginalStackedItem)
        {
            ItemCanNotBeStacked();
            return;
        }
        _rectTransform.position = InitialPosition;
        ResetShapeOffsets();
        ItemTransitionSetup(_gridManager, Initialcell);
    }
    public GridItem TryAutomaticPlacementForUpgrade(GridManager gridManager)
    {
        GridCell cellToPlaceItem = gridManager.FindSpotToPlaceItem(ShapeOffsets);
        if (cellToPlaceItem != null)
        {
            return TryUpgradeItemTransitionSetup(gridManager, cellToPlaceItem);
        }
        return null;
    }
    public GridItem TryAutomaticPlacement(GridManager gridManager, bool isLeftClick = true)
    {
        StackCountBeforeStacking = CurrentStackCount;
        GridItem returnItem = TryAutomaticStackItem(gridManager, isLeftClick);
        if (returnItem)
        {
            return returnItem;
        }
        
        GridCell cellToPlaceItem = gridManager.FindSpotToPlaceItem(ShapeOffsets);
        
        if (cellToPlaceItem != null && !isLeftClick)
        {
            returnItem = CloningGridItem(1);

            returnItem.ItemTransitionSetup(gridManager, cellToPlaceItem);
            return returnItem;
        }
        if (cellToPlaceItem != null)
        {
            ClearOccupiedCells();
            ItemTransitionSetup(gridManager, cellToPlaceItem);
            return this;
        }
        ItemCanNotBePlacedInThisGrid();
        return null;
    }
    #endregion
    #region Stacking
    public GridItem TryAutomaticStackItem(GridManager gridManager, bool isLeftClick)
    {
        if (isLeftClick)
        {
            GridItem targetItem = gridManager.GridItems.FirstOrDefault(
                x => x != this
                && x.ItemSO == ItemSO
                && CanAllItemsBeStacked(x));

            if (targetItem != null)
            {
                targetItem.CurrentStackCount += CurrentStackCount;
                targetItem.UpdateStackCounterTMP();

                IsItemStacked = true;

                _gridManager.GridItems.Remove(this);
                DestroyItem();
                return targetItem;
            }
        }
        else
        {
            GridItem targetItem = gridManager.GridItems.FirstOrDefault(
                x => x != this
                && x.ItemSO == ItemSO
                && CanOneItemBeStacked(x));

            if (targetItem != null)
            {
                if (CurrentStackCount <= 0)
                {
                    IsItemStacked = true;
                    _gridManager.GridItems.Remove(this);
                    DestroyItem();
                }
                return targetItem;
            }
        }
        return null;
    }
    public void ItemCanNotBeStacked()
    {
        if (IsItemFromDiffrentAcquiredStack)
        {
            OrginalStackedItem.DiffrentAcquiredStackCount += CurrentStackCount;
            OrginalStackedItem.SetBackgroundColor(orange);
        }
        OrginalStackedItem.CurrentStackCount += CurrentStackCount;
        OrginalStackedItem.UpdateStackCounterTMP();
        DestroyItem();
    }
    private GridItem TryStackItem(GridManager gridManager, GridCell cell)
    {
        List<GridItem> potentialStackItems = new List<GridItem>();

        foreach (Vector2Int offset in _tempShapeOffsets)
        {
            Vector2Int checkPos = cell.listPosition + new Vector2Int(-offset.y, offset.x);
            GridCell offsetCell = gridManager.GetCellAtPosition(checkPos);
            if (offsetCell != null)
            {
                GridItem itemAtOffset = gridManager.GetItemAtCell(offsetCell);
                if (itemAtOffset != null && itemAtOffset != this)
                {
                    potentialStackItems.Add(itemAtOffset);
                }
            }
        }

        foreach (GridItem targetItem in potentialStackItems)
        {
            if (targetItem.ItemSO == this.ItemSO && CanAllItemsBeStacked(targetItem))
            {
                return targetItem;
            }
        }

        return null;
    }
    private void StackItem(GridManager gridManager, GridItem targetItem)
    {
        targetItem.CurrentStackCount += CurrentStackCount;
        targetItem.UpdateStackCounterTMP();
        DestroyItem();
    }
    public virtual bool CanAllItemsBeStacked(GridItem itemTobeStackedIn)
    {
        int stackCount = itemTobeStackedIn.CurrentStackCount + CurrentStackCount;
        if (ItemSO.maxStackCount > 0 && stackCount <= ItemSO.maxStackCount)
        {


            return true;
        }
        return false;
    }
    public virtual bool CanOneItemBeStacked(GridItem itemTobeStackedIn)
    {
        int stackCount = itemTobeStackedIn.CurrentStackCount + 1;
        if (ItemSO.maxStackCount > 0 && stackCount <= ItemSO.maxStackCount)
        {
            itemTobeStackedIn.CurrentStackCount = stackCount;
            itemTobeStackedIn.UpdateStackCounterTMP();
            CurrentStackCount--;
            UpdateStackCounterTMP();

            return true;
        }
        return false;
    }
    #endregion
    #region Utilities
    private GridItem CloningGridItem(int amount)
    {
        CurrentStackCount -= amount;
        UpdateStackCounterTMP();

        GameObject ClonedGameObject = Instantiate(gameObject, gameObject.transform.parent);
        GridItem ClonedGridItem = ClonedGameObject.GetComponent<GridItem>();

        ClonedGridItem.ReadGraphicOffset();
        ClonedGridItem.SimpleInitialize();
        ClonedGridItem.SetBackgroundColor(currentColor);

        ClonedGridItem.CurrentStackCount = amount;
        ClonedGridItem.UpdateStackCounterTMP();
        
        switch(GridType)
        {
            case GridType.caravan:
            case GridType.vendorToSell:
                CaravanManager.Instance.ClonedItemsFromCaravan.Add(ClonedGameObject);
                break;
            case GridType.vendorToBuy:
            case GridType.chest:
                TradeReferences.Instance.ActiveItemGenerator.ClonedItemsToBuy.Add(ClonedGameObject);
                break;
                //TODO: ????
            case GridType.upgrade:
                break;
            default:
                break;
        }
        if (CurrentStackCount <= 0)
        {
            DestroyItem();
        }
        return ClonedGridItem;
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
    public virtual void DestroyItem()
    {
        if (_gridManager != null)
        {
            _gridManager.GridItems.Remove(this);
        }
        InputSystem.Instance.onVendorAction -= DestroyItem;
        RemoveDragActions();
        
        if(!OrginalStackedItem)
            ClearOccupiedCells();
        
        Destroy(gameObject);
    }
    public void UpdateStackCounterTMP()
    {
        _itemStackCounterTMP.text = $"{CurrentStackCount}/{ItemSO.maxStackCount}";
    }
    #endregion
}
