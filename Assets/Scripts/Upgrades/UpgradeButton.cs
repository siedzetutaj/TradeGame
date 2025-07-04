using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradeButton : MonoBehaviour
{
    public GridWithSlotsSO GridWithSlotsSO;
    public GridManagerWithSlots GridWithSlots;

    public void OnUpgradeButtonPress()
    {
        GridWithSlots.Save();
        GenerateItemsToUpgrade();
        GridWithSlots.InitializeGridWithSlots(GridWithSlotsSO);
    }
    private void GenerateItemsToUpgrade()
    {
        ItemsToUpgradeManager itemsToUpgradeManager = ItemsToUpgradeManager.Instance;

        itemsToUpgradeManager.ClearItemsToUpgrade();
        CreateItemsToUpgrade(itemsToUpgradeManager);
        LoadSavedItemsToUpgrade(itemsToUpgradeManager);
    }
    private void LoadSavedItemsToUpgrade(ItemsToUpgradeManager itemsToUpgradeManager)
    {
        //foreach (var itemToPlaceIn in GridWithSlotsSO.itemsPlacedIn)
        //{
        //    ItemToUpgrade itemToUpgrade = itemsToUpgradeManager.ItemsToUpgrade.FirstOrDefault(x => x.ItemSO == itemToPlaceIn.Key);
        //    if (itemToUpgrade != null)
        //    {
        //        itemToUpgrade.ChangeItemValue(itemToPlaceIn.Value);
        //    }
        //}
    }
    private void CreateItemsToUpgrade(ItemsToUpgradeManager itemsToUpgradeManager)
    {
        foreach (var itemToPlaceIn in GridWithSlotsSO.ItemsToPlaceIn)
        {
            Transform holder = itemsToUpgradeManager.ItemsToUpgradeHolder;
            GameObject prefab = itemsToUpgradeManager.ItemToUpgradePrefab;

            GameObject item = Instantiate(prefab, holder);
            ItemToUpgrade itemToUpgrade = item.GetComponent<ItemToUpgrade>();
            itemToUpgrade.Initialize(0, itemToPlaceIn.Value, itemToPlaceIn.Key.sprite, itemToPlaceIn.Key);

            itemsToUpgradeManager.ItemsToUpgrade.Add(itemToUpgrade);
        }
    }
}
