using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradesMenuButton : MonoBehaviour
{
    [SerializeField] private GridWIthSlotsSOManager _gridWithSlotsSOManager;
    [SerializeField] private GameObject _upgradeButtonPrefab;
    private List<UpgradeButton> _upgradeButtons = new();
    private TradeReferences _tradeReferences;

    public void OnUpgradesButtonPressed()
    {
        _tradeReferences = TradeReferences.Instance;
        GameLogic.Instance.EnableVendorPanel();
        DestroyItemsGameObjects(TradeMechanic.Instance);
        _tradeReferences.Chest.SetActive(false);
        _tradeReferences.Trade.SetActive(false);
        _tradeReferences.Upgrade.SetActive(true);
        foreach(GridWithSlotsSO gridSO in _gridWithSlotsSOManager.allGridsWithSlotsSO)
        {
            CreateUpgradeButton(gridSO);
        }
    }
    private void DestroyItemsGameObjects(TradeMechanic tradeMechanic)
    {
        if (tradeMechanic.ActiveItemGenerator)
        {
            foreach (var item in tradeMechanic.ActiveItemGenerator.CreatedItemsToBuy)
            {
                if (item)
                {
                    Destroy(item);
                }
            }
            tradeMechanic.ActiveItemGenerator.CreatedItemsToBuy.Clear();
        }
    }
    private void CreateUpgradeButton(GridWithSlotsSO gridSO)
    {
        if(!_upgradeButtons.FirstOrDefault(x=>x.GridWithSlotsSO == gridSO))
        {
            GameObject upgradeButtonGameObject = Instantiate(_upgradeButtonPrefab, _tradeReferences.UpgradesHolder);
            
            UpgradeButton upgradeButton = upgradeButtonGameObject.GetComponent<UpgradeButton>();
            upgradeButton.GridWithSlotsSO = gridSO;
            upgradeButton.GridWithSlots = _tradeReferences.UpgradeGrid;
            
            _upgradeButtons.Add(upgradeButton);
        }
    }
}
