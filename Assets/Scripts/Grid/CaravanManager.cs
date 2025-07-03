using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CaravanManager : MonoBehaviourSingleton<CaravanManager>
{
    public List<GameObject> ItemsInCaravan = new();


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
    }
    public void ChangeWeightWalue(int itemWeight)
    {
        CurrentWeight += itemWeight;
    }
    public void GiveAwayItem(GameObject ItemObject)
    {
        ItemObject.transform.SetParent(_itemHolderTransform);
        ItemsInCaravan.Remove(ItemObject);
    }
    public void OnItemUsedAsRation(GridItem item)
    {
        ItemsInCaravan.Remove(item.gameObject);
        item.ClearOccupiedCells();
        Destroy(item.gameObject);
    }
}
