using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradesMenuButton : MonoBehaviour
{
    [SerializeField] private GridWIthSlotsSOManager _gridWithSlotsSOManager;
    private TradeReferences _tradeReferences;
    private UpgradeButtonsManager upgradeButtonsManager;

    public void OnUpgradesButtonPressed()
    {
        _tradeReferences = TradeReferences.Instance;
        GameLogic.Instance.EnableVendorPanel();
        DestroyItemsGameObjects(TradeMechanic.Instance);

        _tradeReferences.Chest.SetActive(false);
        _tradeReferences.Trade.SetActive(false);
        _tradeReferences.Upgrade.SetActive(true);

        upgradeButtonsManager = UpgradeButtonsManager.Instance;
        upgradeButtonsManager.TradeReferences = _tradeReferences;

        foreach (GridWithSlotsSO gridSO in _gridWithSlotsSOManager.allGridsWithSlotsSO)
        {
            if(!gridSO.IsUpgraded)
            {
                upgradeButtonsManager.CreateUpgradeButton(gridSO);
            }
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


}
