using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TradeMechanic : MonoBehaviourSingleton<TradeMechanic>
{
    [SerializeField] private List<GridItem> _BoughtItems;
    [SerializeField] private List<GridItem> _SoldItems;
    [SerializeField] private GridManager _VendorToBuy;
    [SerializeField] private Button _TradeButton;
    [SerializeField] private Slider _Slider;

    private Color _colorGreen = new Color(0.3686f, 1.0f, 0.4196f, 1.0f);
    private Color _colorRed = new Color(1.0f, 0.3529f, 0.3686f, 1.0f);

    public VendorItemGenerator ActiveItemGenerator;

    private void OnEnable()
    {
        _Slider = TradeBar.Instance.slider;
        ResetSlider();
    }
    private void OnDisable()
    {
        ResetSlider();
    }
    public void CalculatePrice(GridItem item, GridType targetedGridType, int amount)
    {
        // First remove any existing references to this item
        _BoughtItems.Remove(item);
        _SoldItems.Remove(item);

        int itemValue = item.ItemSO.value * amount;
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
                    price -= itemValue * GetVendorBuyMultiplayer(item.ItemSO);
                    _BoughtItems.Add(item);
                }
                break;

            case GridType.vendorToBuy:
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
                if (!item.ItemAcquired)
                {
                    item.ItemAcquired = true;
                    ActiveItemGenerator.OnItemAcquired(item, item.CurrentStackCount);
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
        CaravanManager.Instance.UpdateCaravanItemStacks();
    }
    private float GetVendorBuyMultiplayer(ItemSO itemToBuy)
    {
        return ActiveItemGenerator.ItemBuyMultiplayer(itemToBuy);
    }
    private float GetVendorSellMultiplayer(ItemSO itemToSell)
    {
        return ActiveItemGenerator.ItemSellMultiplayer(itemToSell);
    } 
}
