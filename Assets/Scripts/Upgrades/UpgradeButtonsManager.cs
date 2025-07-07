using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradeButtonsManager : MonoBehaviourSingleton<UpgradeButtonsManager>, IUpgradeAction
{
    public List<UpgradeButton> UpgradeButtons = new();
    [SerializeField] private GameObject _upgradeButtonPrefab;
    public TradeReferences TradeReferences;
    public void PerformUpgrade(GridWithSlotsSO gridWithSlotsSO)
    {
        UpgradeButton button = UpgradeButtons.FirstOrDefault(x => x.GridWithSlotsSO == gridWithSlotsSO);
        UpgradeButtons.Remove(button);
        Destroy(button.gameObject);
    }

    public void CreateUpgradeButton(GridWithSlotsSO gridSO)
    {
        if (!UpgradeButtons.FirstOrDefault(x => x.GridWithSlotsSO == gridSO))
        {
            GameObject upgradeButtonGameObject = Instantiate(_upgradeButtonPrefab, TradeReferences.UpgradesHolder);

            UpgradeButton upgradeButton = upgradeButtonGameObject.GetComponent<UpgradeButton>();
            upgradeButton.GridWithSlotsSO = gridSO;
            upgradeButton.GridWithSlots = TradeReferences.UpgradeGrid;
            upgradeButton.TMP.text = gridSO.GridName;

            UpgradeButtons.Add(upgradeButton);
        }
    }
}
