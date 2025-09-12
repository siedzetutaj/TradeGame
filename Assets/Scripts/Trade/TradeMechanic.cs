using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.ExtendedProperties;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TradeMechanic : MonoBehaviourSingleton<TradeMechanic>
{
    [SerializeField] private List<GridItem> _BoughtItems = new();
    [SerializeField] private List<GridItem> _SoldItems = new();
    [SerializeField] private GridManager _VendorToBuy;
    [SerializeField] private Button _TradeButton;
    [SerializeField] private Slider _Slider;

    private Dictionary<ItemSO, int> _ItemsToExchange = new();
    private Dictionary<ItemSO, int> _ItemsExchanged = new();
    [SerializeField] private List<GridItem> _ItemsThatCanBeExchanged = new();

    private readonly Color _colorGreen = new Color(0.3686f, 1.0f, 0.4196f, 1.0f);
    private readonly Color _colorRed = new Color(1.0f, 0.3529f, 0.3686f, 1.0f);

    public VendorItemGenerator ActiveItemGenerator;

    private void OnEnable()
    {
        _BoughtItems.Clear();
        _SoldItems.Clear();
        _ItemsToExchange.Clear();
        _Slider = TradeBar.Instance.slider;
        ResetSlider();
    }
    private void OnDisable()
    {
        ResetSlider();
    }
    public void CalculatePrice(GridItem item, GridType targetedGridType, int amount)
    {
        _BoughtItems.Remove(item);
        _SoldItems.Remove(item);

        float itemValue = item.ItemSO.value * amount;

        if (item is ExchangeGridItem exchangeItem)
            itemValue = exchangeItem.ItemToExchangeFor.value * amount * 0.1f;

        float price = 0;

        switch (targetedGridType)
        {
            case GridType.caravan:
                if (item.ItemAcquired)
                {
                    price -= itemValue * GetVendorSellMultiplayer(item.ItemSO);
                    _SoldItems.Add(item);
                }
                else
                {
                    if (ExchangeItemCheck(item, amount))
                        break;

                    price -= itemValue * GetVendorBuyMultiplayer(item.ItemSO);
                    _BoughtItems.Add(item);
                }
                break;

            case GridType.vendorToBuy:
                if(IsItemInExchangeList(item))
                    break;
                price += itemValue * GetVendorBuyMultiplayer(item.ItemSO);
                _BoughtItems.Add(item);
                break;

            case GridType.vendorToSell:
                price += itemValue * GetVendorSellMultiplayer(item.ItemSO);
                _SoldItems.Add(item);
                break;

            default:
                return;
        }

        ChangeTradeBarValue((int)price);
    }
    private void ChangeTradeBarValue(int itemPrice)
    {

        _Slider.value += itemPrice;
        ColorBlock colorBlock = _TradeButton.colors;

        if (_Slider.value > 0 && (_BoughtItems.Count > 0 || _SoldItems.Count > 0))
        {
            _Slider.targetGraphic.color = _colorGreen;
            colorBlock.normalColor = _colorGreen;
            _TradeButton.interactable = true;
        }
        else
        {
            _Slider.targetGraphic.color = _colorRed;
            colorBlock.normalColor = _colorRed;
            _TradeButton.interactable = false;
        }
        _TradeButton.colors = colorBlock;

    }
    private void ResetSlider()
    {
        if (_Slider.gameObject.activeInHierarchy)
        {
            _Slider.value = 0;
            ChangeTradeBarValue(0);
        }
    }
    public void TradeButton()
    {
        TradeReferences tradeReferences = TradeReferences.Instance;
        if (_Slider.value >= 0 && (_BoughtItems.Count > 0 || _SoldItems.Count > 0))
        {

            foreach (GridItem item in tradeReferences.CaravanGrid.GridItems)
            {
                if (item.DiffrentAcquiredStackCount > 0)
                {
                    item.ItemAcquired = true;
                    ActiveItemGenerator.OnItemAcquired(item, item.DiffrentAcquiredStackCount);
                    item.DiffrentAcquiredStackCount = 0;
                    item.SetBackgroundColor(item.green);
                }
                if (!item.ItemAcquired)
                {
                    item.ItemAcquired = true;
                    ActiveItemGenerator.OnItemAcquired(item, item.CurrentStackCount);
                    item.SetBackgroundColor(item.green);
                }
            }
            var soldItems = new List<GridItem>(tradeReferences.VendorToSellGrid.GridItems);
            foreach (GridItem item in soldItems)
            {
                if (item.ItemAcquired)
                {
                    item.ItemAcquired = false;

                    foreach (GridCell cell in item.GetOccupiedCells())
                    {
                        cell.isOccupied = false;
                    }

                    item.TryAutomaticPlacement(_VendorToBuy, true);
                    ActiveItemGenerator.OnItemReturned(item, item.CurrentStackCount);
                }

            }
            ResetSlider();
        }
        _BoughtItems.Clear();
        _SoldItems.Clear();
        CaravanManager.Instance.UpdateCaravanItemsData();
    }
    private float GetVendorBuyMultiplayer(ItemSO itemToBuy)
    {
        return ActiveItemGenerator.ItemBuyMultiplayer(itemToBuy);
    }
    private float GetVendorSellMultiplayer(ItemSO itemToSell)
    {
        return ActiveItemGenerator.ItemSellMultiplayer(itemToSell);
    }
    public int BoughtItemAmount(ItemSO item)
    {
        int boughtItemAmount = 0;
        foreach (GridItem boughtItem in _BoughtItems)
        {
            if (boughtItem)
                if (boughtItem.ItemSO == item)
                {
                    boughtItemAmount += boughtItem.CurrentStackCount;
                }
        }
        return boughtItemAmount;
    }
    #region Exchange
    private bool ExchangeItemCheck(GridItem item, int amount)
    {
        foreach (ItemSO itemSO in _ItemsToExchange.Keys)
        {
            if (itemSO == item.ItemSO)
            {
                if(_ItemsExchanged.ContainsKey(itemSO))
                    _ItemsExchanged[itemSO] += amount;
                else
                    _ItemsExchanged.Add(itemSO, amount);

                if (_ItemsToExchange[itemSO] < _ItemsExchanged[itemSO])
                {
                    int ExchangeItemDiffrence = _ItemsExchanged[itemSO] - _ItemsToExchange[itemSO];
                    amount = amount < ExchangeItemDiffrence ? amount : ExchangeItemDiffrence;

                    int itemValue = item.ItemSO.value * amount;
                    float price = 0;

                    price -= itemValue * GetVendorBuyMultiplayer(itemSO);
                    ChangeTradeBarValue((int)price);
                }

                _ItemsThatCanBeExchanged.Remove(item);
                _BoughtItems.Add(item);

                if (_ItemsExchanged[itemSO] >= _ItemsToExchange[itemSO])
                    ExchangeItemResetColor(itemSO);

                return true;
            }
        }

        return false;
    }
    public int ExchangeItemAmount(ExchangeGridItem exchangeItem)
    {
        int exchangeItemAmount = 0;

        foreach (GridItem soldItem in _SoldItems)
        {
            if (soldItem is ExchangeGridItem soldExchangeItem
                && soldExchangeItem.ItemToExchangeFor == exchangeItem.ItemToExchangeFor)
            {
                exchangeItemAmount += soldExchangeItem.CurrentStackCount;
            }
        }
        return exchangeItemAmount;
    }
    public void ExchangeItemPriceReset(ItemSO item, int amount)
    {
        int itemValue = item.value * amount;
        float price = 0;
        price += itemValue * GetVendorBuyMultiplayer(item);
        ChangeTradeBarValue((int)price);
    }
    public void ExchangeItemHighlight(ItemSO item, int amount)
    {
        if (_ItemsToExchange.ContainsKey(item))
            _ItemsToExchange[item] += amount;
        else
            _ItemsToExchange.Add(item, amount);

        foreach (GridItem gridItem in TradeReferences.Instance.VendorToBuyGrid.GridItems)
        {
            if (gridItem.ItemSO == item && !_ItemsThatCanBeExchanged.Contains(gridItem))
            {
                gridItem.SetBackgroundColor(gridItem.yellow);
                _ItemsThatCanBeExchanged.Add(gridItem);
            }
        }
    }
    public void ExchangeItemResetColor(ItemSO item)
    {
        foreach (GridItem gridItem in TradeReferences.Instance.VendorToBuyGrid.GridItems)
        {
            if (gridItem.ItemSO == item && !_ItemsThatCanBeExchanged.Contains(gridItem))
            {
                gridItem.SetBackgroundColor(gridItem.yellow);
                _ItemsThatCanBeExchanged.Add(gridItem);
            }
        }

        foreach (GridItem gridItem in _ItemsThatCanBeExchanged)
        {
            if (gridItem && gridItem.ItemSO == item)
            {
                gridItem.SetBackgroundColor(gridItem.defaultColor);
            }
        }
    }
    public void RemoveItemToExchange(ItemSO item, int amount)
    {
        if (_ItemsToExchange.ContainsKey(item))
        {
            if (_ItemsToExchange[item] > amount)
                _ItemsToExchange[item] -= amount;
            else
            { 
                _ItemsToExchange.Remove(item);
                ExchangeItemResetColor(item);
                if(_ItemsExchanged.ContainsKey(item))
                    _ItemsExchanged.Remove(item);
                _ItemsThatCanBeExchanged.RemoveAll(g => g.ItemSO == item);
            }
        }
    }
    public void RemoveExchangedItem(ItemSO item, int amount)
    {
        _ItemsExchanged[item] -= amount;
        if (_ItemsExchanged[item] <= 0)
            _ItemsExchanged.Remove(item);
    }
    private bool IsItemInExchangeList(GridItem item)
    {
        bool isInList = false;
        if (_ItemsExchanged.ContainsKey(item.ItemSO))
        {
            if(_ItemsExchanged[item.ItemSO] > _ItemsToExchange[item.ItemSO])
            {
                int ExchangeItemDiffrence = _ItemsExchanged[item.ItemSO] - _ItemsToExchange[item.ItemSO];
                int amount = item.CurrentStackCount < ExchangeItemDiffrence ? item.CurrentStackCount : ExchangeItemDiffrence;

                float price = 0;
                price += item.ItemSO.value * amount * GetVendorBuyMultiplayer(item.ItemSO);
                ChangeTradeBarValue((int)price);

                ExchangeItemResetColor(item.ItemSO);
                _ItemsThatCanBeExchanged.Remove(item);
            }
            RemoveExchangedItem(item.ItemSO, item.CurrentStackCount);
            _BoughtItems.Add(item);

            if (!_ItemsExchanged.ContainsKey(item.ItemSO) ||
                _ItemsToExchange[item.ItemSO]> _ItemsExchanged[item.ItemSO])
            {
                item.SetBackgroundColor(item.yellow);
                foreach (GridItem gridItem in _ItemsThatCanBeExchanged)
                {
                    if(gridItem && gridItem.ItemSO == item.ItemSO)
                    {
                        gridItem.SetBackgroundColor(gridItem.yellow);
                    }
                }
            }

            isInList = true;
        }

        return isInList;
    }
    #endregion
}
