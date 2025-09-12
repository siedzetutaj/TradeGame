using UnityEngine;
using UnityEngine.UI;

public class ExchangeGridItem : GridItem
{
    public ItemSO ItemToExchangeFor;
    TradeMechanic tradeMechanic;
    [SerializeField] protected Image _exchangeItemImage;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="itemSO">Item to exchagne this for not like in normal grid item</param>
    /// <param name="isItemAcquired"></param>
    /// <param name="type"></param>
    /// <param name="manager"></param>
    /// <param name="StackCount"></param>
    public override void Initialize(ItemSO itemSO, bool isItemAcquired, GridType type, GridManager manager, int StackCount = 1)
    {
        CurrentStackCount = StackCount;
        ItemToExchangeFor = itemSO;
        ItemAcquired = isItemAcquired;
        _gridManager = manager;
        GridType = type;

        OrginalStackedItem = null;

        ShapeOffsets = new(ItemSO.shapeOffsets);

        SimpleInitialize();

        _itemImage.sprite = ItemSO.sprite;
        _exchangeItemImage.sprite = ItemToExchangeFor.sprite;
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        tradeMechanic = TradeMechanic.Instance;
    }
    protected override void CompleteTradePlacement(GridManager gridManager, GridCell cell, GridItem itemToStack, int weightChange, Color color)
    {
        int exchangeGridItemAmount;

        if (gridManager.gridType == GridType.vendorToSell)
        {
            VendorItemGenerator itemGenerator = TradeMechanic.Instance.ActiveItemGenerator;
            if (itemGenerator.ItemsToBuy.ContainsKey(ItemToExchangeFor)
                && itemGenerator.ItemsToBuy[ItemToExchangeFor] >= CurrentStackCount)
            {
                int boughtItemAmount = tradeMechanic.BoughtItemAmount(ItemToExchangeFor);
               
                exchangeGridItemAmount = tradeMechanic.ExchangeItemAmount(this);

                if (boughtItemAmount > exchangeGridItemAmount)
                {
                    int amountDiffrence = boughtItemAmount - exchangeGridItemAmount;

                    if (amountDiffrence >= CurrentStackCount)
                    {
                        amountDiffrence = CurrentStackCount;
                        tradeMechanic.ExchangeItemPriceReset(ItemToExchangeFor, amountDiffrence);                        
                        base.CompleteTradePlacement(gridManager, cell, itemToStack, weightChange, color);
                        return;
                    }
                    
                    tradeMechanic.ExchangeItemPriceReset(ItemToExchangeFor, amountDiffrence);

                }
                
                exchangeGridItemAmount += CurrentStackCount;
                
                exchangeGridItemAmount -= boughtItemAmount;

                tradeMechanic.ExchangeItemHighlight(ItemToExchangeFor, exchangeGridItemAmount);
            }
            else
            {
                ItemCanNotBePlacedInThisGrid();
                return;
            }

        }
        else
        {
            tradeMechanic.RemoveItemToExchange(ItemToExchangeFor, CurrentStackCount);
        }

        base.CompleteTradePlacement(gridManager, cell, itemToStack, weightChange, color);
    }
    public override bool CanAllItemsBeStacked(GridItem itemTobeStackedIn)
    {
        if (itemTobeStackedIn is ExchangeGridItem exchangeItem)
        {
            if (exchangeItem.ItemToExchangeFor != ItemToExchangeFor)
                return false;
        }
        return base.CanAllItemsBeStacked(itemTobeStackedIn);
    }
    public override bool CanOneItemBeStacked(GridItem itemTobeStackedIn)
    {
        if(itemTobeStackedIn is ExchangeGridItem exchangeItem)
        {
            if (exchangeItem.ItemToExchangeFor != ItemToExchangeFor)
                return false;
        }
        return base.CanOneItemBeStacked(itemTobeStackedIn);
    }
}
