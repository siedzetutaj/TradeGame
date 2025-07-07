using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CaravanManager : MonoBehaviourSingleton<CaravanManager>
{
    public List<GameObject> ItemsInCaravan = new();
    [SerializeField] private GridManager _caravanGrid;

    [SerializeField] private ItemsSaveDataSO _caravanItemsSaveData;

    [SerializeField] private int _MaxWeight = 50;
    public int MaxWeight
    {
        get
        {
            return _MaxWeight;
        }
        set
        {
            _MaxWeight = value;
            _maxWeightDisplay.text = _MaxWeight.ToString();
        }
    }

    [SerializeField] private int _CurrentWeight = 0;
    public int CurrentWeight
    {
        get
        {
            return _CurrentWeight;
        }

        set
        {
            _CurrentWeight = value;
            _currentWeightDisplay.text = _CurrentWeight.ToString();
        }
    }

    [SerializeField] private Transform _caravanItemHolderTransform;
    [SerializeField] private Transform _itemHolderTransform;

    [SerializeField] private TextMeshProUGUI _maxWeightDisplay;
    [SerializeField] private TextMeshProUGUI _currentWeightDisplay;


    private void Start()
    {
        MaxWeight = _MaxWeight;
        CurrentWeight = _CurrentWeight;
        _maxWeightDisplay.text = _MaxWeight.ToString();
        _currentWeightDisplay.text= _CurrentWeight.ToString();
        LoadItemsFromSaveData();
    }

    public bool IsHeavierThenCaravanCapacity(int itemWeight)
    {
        int tempCapacity = CurrentWeight;
        tempCapacity += itemWeight;
        if (tempCapacity > MaxWeight)
        {
            return true;
        }
        CurrentWeight += itemWeight;
        return false;
    }
    public void TakeItem( GameObject ItemObject)
    {
        ItemObject.transform.SetParent(_caravanItemHolderTransform);
        ItemsInCaravan.Add(ItemObject);
        SaveItemsToSaveData();
    }
    public void ChangeWeightWalue(int itemWeight)
    {
        CurrentWeight += itemWeight;
    }
    public void GiveAwayItem(GameObject ItemObject)
    {
        ItemObject.transform.SetParent(_itemHolderTransform);
        ItemsInCaravan.Remove(ItemObject);
        SaveItemsToSaveData();
    }
    public void OnItemUsedAsRation(GridItem item)
    {
        ItemsInCaravan.Remove(item.gameObject);
        item.ClearOccupiedCells();
        Destroy(item.gameObject);
    }
    public void LoadItemsFromSaveData()
    {
        ItemsInCaravan.Clear();
        if (_caravanItemsSaveData == null || _caravanItemsSaveData.ItemsPlacedIn == null)
        {
            Debug.LogWarning("No items to load from save data.");
            return;
        }
        foreach (ItemSaveData itemData in _caravanItemsSaveData.ItemsPlacedIn)
        {
            GameObject itemObject = Instantiate(
                _caravanItemsSaveData.ItemPrefab, _caravanItemHolderTransform);

            GridItem gridItem = itemObject.GetComponent<GridItem>();
            gridItem.Initialize(itemData.ItemSO, true, GridType.caravan, _caravanGrid);
            gridItem.TryAutomaticPlacement(_caravanGrid);

            ItemsInCaravan.Add(itemObject);

            if (itemData.ItemSO.itemtype == ItemType.food && itemData.ItemSO.ration > 0)
            {
                ResourceManager.Instance.AddResourceToInventory(gridItem);
            }
            CurrentWeight += itemData.ItemSO.weight;
        }
    }
    public void SaveItemsToSaveData()
    {
        if (_caravanItemsSaveData == null)
        {
            Debug.LogError("CaravanItemsSaveData is not assigned.");
            return;
        }
        _caravanItemsSaveData.ItemsPlacedIn.Clear();
        foreach (GameObject itemObject in ItemsInCaravan)
        {
            GridItem gridItem = itemObject.GetComponent<GridItem>();
            if (gridItem != null && gridItem.ItemSO != null)
            {
                ItemSaveData itemData = new ItemSaveData(
                    gridItem.ItemSO, gridItem.ShapeOffsets,
                    gridItem.Initialcell.listPosition, 1);
                _caravanItemsSaveData.ItemsPlacedIn.Add(itemData);
            }
        }
    }
    public void CaravanGridPreset(int value)
    {
        SaveItemsToSaveData();
        foreach(GameObject item in ItemsInCaravan)
        {
            Destroy(item);
        }   
        switch (value)
        {
            case 0:
                _caravanGrid.cellsPerRow = new List<int> { 4, 4, 4, 4 };
                break;
            case 1:
                _caravanGrid.cellsPerRow = new List<int> { 4, 5, 5, 5, 4};
                _caravanGrid.disableCellsAt = new List<Vector2Int> { 
                    new Vector2Int(0, 0),
                    new Vector2Int(4, 0)
                };
                break;
            case 2:
                _caravanGrid.cellsPerRow = new List<int> { 5, 6, 6, 6, 6};
                _caravanGrid.disableCellsAt = new List<Vector2Int> {
                    new Vector2Int(0, 0)
                }; 
                break;
            default:
                Debug.LogError("Invalid caravan grid preset value");
                break;
        }
        _caravanGrid.InitializeGrid();
        LoadItemsFromSaveData();
    }
}
