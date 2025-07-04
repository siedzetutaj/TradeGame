using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class GridManagerWithSlots : GridManager, IUpgradeAction
{
    public List<ItemForUpgradeData> ItemsPlacedIn;
    [SerializeField] private SerializableDictionary<GridItem, int> _gridItems = new();
    [SerializeField] private GameObject _itemPrefab;
    [SerializeField] private Transform _itemHolder;

    private GridWithSlotsSO _gridWithSlotsSO;
    private GameLogic _gameLogic;

    protected override void OnEnable()
    {
        _gameLogic = GameLogic.Instance;
        _gameLogic.onEnabledDestinationPanelAction += OnUpgradesExit;
        _gameLogic.onEnabledMapPanelAction += OnUpgradesExit;
        _gameLogic.onEnabledVendorPanelAction += OnUpgradesExit;
    }
    protected override void OnDisable()
    {
        _gameLogic.onEnabledDestinationPanelAction -= OnUpgradesExit;
        _gameLogic.onEnabledMapPanelAction -= OnUpgradesExit;
        _gameLogic.onEnabledVendorPanelAction -= OnUpgradesExit;
        
        base.OnDisable();
    }
    public void InitializeGridWithSlots(GridWithSlotsSO gridWithSlotsSO)
    {
        LoadDataFromSO(gridWithSlotsSO);
        ConfirmUpgradeButton.Instance.GridWithSlotsSO = gridWithSlotsSO;

        base.InitializeGrid();
        LoadItemsToGrid();
    }
    private void LoadDataFromSO(GridWithSlotsSO gridWithSlotsSO)
    {
        if (!gridWithSlotsSO.IsUpgraded)
        {
            _gridWithSlotsSO = gridWithSlotsSO;
            cellsPerRow = new(gridWithSlotsSO.CellsPerRow);
            disableCellsAt = new(gridWithSlotsSO.DisableCellsAt);
            ItemsPlacedIn = new(gridWithSlotsSO.ItemsPlacedIn);
        }
    }
    public void LoadItemsToGrid()
    {
        if (ItemsPlacedIn.Count > 0) 
        {
            foreach (var item in ItemsPlacedIn)
            {
                CreateItem(item, item.Value, _itemPrefab, _itemHolder);
            }
        }
        _gridWithSlotsSO.ItemsPlacedIn.Clear();
    }
    public void RemoveItemFromGrid(GridItem item, int value = 1)
    {
        if(ItemsToUpgradeManager.Instance.ItemsToUpgrade.Any(item=>item.RemoveItem(item.ItemSO, value)))
        {
            if (_gridItems[item] == value) 
            { 
                _gridItems.Remove(item);
            }
            else
            {
                _gridItems[item] -= value;
            }
        }
    }
    public bool AddItemToGrid(GridItem gridItem, int value = 1)
    {
        if(ItemsToUpgradeManager.Instance.IsThisItemInUpgradeItems( gridItem,  value))
        {
            _gridItems.Add(gridItem, value);
            return true;
        }
        return false;
    }
    private void CreateItem(ItemForUpgradeData data, int value, GameObject prefab, Transform hodler)
    {

        GameObject createdItem = Instantiate(prefab, hodler);

        GridItem gridItem = createdItem.GetComponent<GridItem>();
        gridItem.Initialize(data.ItemSO, false, GridType.upgrade, this);
        gridItem.ShapeOffsets = data.ShapeOffsets;
        gridItem.ResetShapeOffsets();
        gridItem.ItemTransitionSetup(this, GetCellAtPostion(data.InitialCellPostion));
        AddItemToGrid(gridItem, value);
    }
    public void Save()
    {
        if (_gridItems.Count>0)
        {

            foreach (var item in _gridItems)
            {
                ItemForUpgradeData itemData = new(item.Key.ItemSO, item.Key.ShapeOffsets, item.Key.Initialcell.listPosition, item.Value);
                _gridWithSlotsSO.ItemsPlacedIn.Add(itemData);
            }
            ClearItemsInGrid();
        }
    }
    public void ClearItemsInGrid()
    {
        _gridItems.Keys.ToList().ForEach(key =>
        {
            if (key != null) Destroy(key.gameObject);
        });
        _gridItems.Clear();
    }
    public void OnUpgradesExit()
    {
        Save();
        ClearGrid();
        ItemsToUpgradeManager.Instance.ClearItemsToUpgrade();
    }
    private GridCell GetCellAtPostion(Vector2Int cellPostion)
    {
        return grid[cellPostion.x][cellPostion.y];
    }
    public void PerformUpgrade(GridWithSlotsSO gridWithSlotsSO)
    {
        ClearGrid();
        ClearItemsInGrid();
        _gridWithSlotsSO.IsUpgraded = true;
    }
}