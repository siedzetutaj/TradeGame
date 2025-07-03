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

    //kalkuluje cene itemu
    //zmienia value baru
    private void OnEnable()
    {
        _Slider = TradeBar.Instance.slider;
        ResetSlider();
    }
    public void CalculatePrice(GridItem item)
    {
        int itemValue = item.ItemSO.value;
        float price = 0;
        switch(item.GridType)
        {
            case GridType.caravan:
                if (item.ItemAcquired)
                {
                    price -= itemValue * GetVendorSellMultiplayer(item.ItemSO);
                    Debug.Log(price);
                    _SoldItems.Remove(item);
                }
                else
                {
                    price -= itemValue * GetVendorBuyMultiplayer(item.ItemSO);
                    Debug.Log(price);
                    _BoughtItems.Add(item);
                }
                break;
            case GridType.vendorToBuy:
                price += itemValue * GetVendorBuyMultiplayer(item.ItemSO);
                Debug.Log(price);
                _BoughtItems.Remove(item);
                break;
            case GridType.vendorToSell:
                price += itemValue * GetVendorSellMultiplayer(item.ItemSO);
                Debug.Log(price);
                _SoldItems.Add(item);
                break;
            default: return;
        }
        ChangeTradeBarValue((int)price);
    }
    private void ChangeTradeBarValue(int itemPrice)
    {

        _Slider.value += itemPrice;
        ColorBlock colorBlock = _TradeButton.colors;

        if (_Slider.value >= 0 && (_BoughtItems.Count > 0 || _SoldItems.Count > 0)) 
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
        if (_Slider.value >= 0 && (_BoughtItems.Count > 0 || _SoldItems.Count > 0))
        {

            foreach(var item in _BoughtItems)
            {
                item.ItemAcquired = true;
                ActiveItemGenerator.OnItemAcquired(item);
            }

            foreach (GridItem item in _SoldItems)
            {
                foreach (GridCell cell in item.GetOccupiedCells())
                {
                    cell.isOccupied = false;
                }

                item.ItemAcquired = false;

                item.TryAutomaticPlacement(_VendorToBuy);
                ActiveItemGenerator.OnItemReturned(item);

            }
            ResetSlider();
        }
        _BoughtItems.Clear();
        _SoldItems.Clear();
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
