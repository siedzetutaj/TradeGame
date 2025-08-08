using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class CaravanManager : MonoBehaviourSingleton<CaravanManager>
{
    public List<GameObject> ItemsInCaravan = new();
    [NonSerialized] public List<GameObject> ClonedItemsFromCaravan = new();
    public List<ItemData> CaravanItemStacks = new();    

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
    private void OnEnable()
    {
        RestoreCaravanItemsFromStacks();
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
    public void TakeItem(GridItem item)
    {
        item.transform.SetParent(_caravanItemHolderTransform);
        
        if(!ItemsInCaravan.FirstOrDefault(x=>x==item.gameObject))
            ItemsInCaravan.Add(item.gameObject);
    }
    public void ChangeWeightWalue(int itemWeight)
    {
        CurrentWeight += itemWeight;
    }
    public void GiveAwayItem(GridItem item)
    {
        item.transform.SetParent(_itemHolderTransform);
        ItemsInCaravan.Remove(item.gameObject);
    }
    public void OnItemUsedAsRation(ItemData item, int AmountUsed)
    {
        int AmountAfterUse = item.StackCount - AmountUsed;

        ChangeWeightWalue(-item.ItemSO.weight * AmountUsed);

        if (AmountAfterUse == 0)
        {
            CaravanItemStacks.Remove(item);
            return;
        }
        item.StackCount = AmountAfterUse;
    }
    public void LoadItemsFromSaveData()
    {
        if (_caravanItemsSaveData.IsSavingEnabled)
        {
            ItemsInCaravan.Clear();
            if (_caravanItemsSaveData == null || _caravanItemsSaveData.ItemsPlacedIn == null)
            {
                Debug.LogWarning("No items to load from save data.");
                return;
            }
            foreach (ItemData itemData in _caravanItemsSaveData.ItemsPlacedIn)
            {
                GameObject itemObject = Instantiate(
                    _caravanItemsSaveData.ItemPrefab, _caravanItemHolderTransform);

                GridItem gridItem = itemObject.GetComponent<GridItem>();
                gridItem.Initialize(itemData.ItemSO, true, GridType.caravan, _caravanGrid, itemData.StackCount);
                gridItem.TryAutomaticPlacement(_caravanGrid,true);

                ItemsInCaravan.Add(itemObject);

                CurrentWeight += itemData.ItemSO.weight * itemData.StackCount;
            }
        }
    }
    public void SaveItemsToSaveData()
    {
        if (_caravanItemsSaveData.IsSavingEnabled)
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
                    ItemData itemData = new ItemData(
                        gridItem.ItemSO, gridItem.ShapeOffsets,
                        gridItem.Initialcell.listPosition, gridItem.CurrentStackCount);
                    _caravanItemsSaveData.ItemsPlacedIn.Add(itemData);
                }
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
    public void UpdateCaravanItemStacks()
    {
        CaravanItemStacks.Clear();
        foreach (var itemObj in ItemsInCaravan)
        {
            var gridItem = itemObj.GetComponent<GridItem>();
            if (gridItem != null && gridItem.ItemSO != null)
            {
                CaravanItemStacks.Add(new ItemData(
                    gridItem.ItemSO,
                    new List<Vector2Int>(gridItem.ShapeOffsets),
                    gridItem.Initialcell != null ? gridItem.Initialcell.listPosition : Vector2Int.zero,
                    gridItem.CurrentStackCount
                ));
            }
        }
    }
    public void RestoreCaravanItemsFromStacks()
    {
        // Usuñ stare obiekty
        foreach (var item in ItemsInCaravan)
        {
            if(item)
                item.GetComponent<GridItem>().DestroyItem();
        }
        ItemsInCaravan.Clear(); 
        
        foreach (var item in ClonedItemsFromCaravan)
        {
            if (item)
                item.GetComponent<GridItem>().DestroyItem();
        }
        ClonedItemsFromCaravan.Clear();

        CurrentWeight = 0;

        foreach (var stackData in CaravanItemStacks)
        {
            GameObject itemObj = Instantiate(_caravanItemsSaveData.ItemPrefab, _caravanItemHolderTransform);
            var gridItem = itemObj.GetComponent<GridItem>();
            gridItem.Initialize(stackData.ItemSO, true, GridType.caravan, _caravanGrid, stackData.StackCount);
            gridItem.SetShapeOffsets(stackData.ShapeOffsets);
            // Ustaw pozycjê startow¹
            var cell = _caravanGrid.GetCellAtPosition(stackData.InitialCellPostion);
            if (cell != null)
            {
                gridItem.ItemTransitionSetup(_caravanGrid, cell);
            }
            ItemsInCaravan.Add(itemObj);
            ChangeWeightWalue(stackData.ItemSO.weight * stackData.StackCount);
        }
    }
}
