using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ItemsToUpgradeManager : MonoBehaviourSingleton<ItemsToUpgradeManager>
{
    public Transform ItemsToUpgradeHolder;
    public GameObject ItemToUpgradePrefab;
    public List<ItemToUpgrade> ItemsToUpgrade = new();
    public Button ConfirmUpgradeButton;
    
    public void ClearItemsToUpgrade()
    {
        foreach (var itemToUpgrade in ItemsToUpgrade)
        {
            Destroy(itemToUpgrade.gameObject);
        }
        ItemsToUpgrade.Clear();
        ConfirmUpgradeButton.interactable = false;
    }
    public void ConfirmUpgradeButtonCheck()
    {
        bool allItemsFull = ItemsToUpgrade.Count
            (item => !item.canContainMoreItems) == ItemsToUpgrade.Count;
        ConfirmUpgradeButton.interactable = allItemsFull;
    }
    public bool IsThisItemInUpgradeItems(GridItem gridItem, int value = 1)
    {
        bool found = ItemsToUpgrade.Any(item => item.AddItem(gridItem.ItemSO, value));
        if (found) ConfirmUpgradeButtonCheck();
        return found;
    }
}

public enum UpgradeType
{
    CaravanWeigh,
    CaravanSize,
    TravelCosts,
    Base,
    NewUpgrades,
    Debug
}