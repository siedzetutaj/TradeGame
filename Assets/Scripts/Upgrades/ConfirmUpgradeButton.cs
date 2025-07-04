using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConfirmUpgradeButton : MonoBehaviourSingleton<ConfirmUpgradeButton>
{
    public GridWithSlotsSO GridWithSlotsSO;

    public void OnConfirmUpgradeButtonPressed()
    {
        var upgradables = FindObjectsOfType<MonoBehaviour>().OfType<IUpgradeAction>();

        foreach (var upgradable in upgradables)
        {
            upgradable.PerformUpgrade(GridWithSlotsSO);
        }

        // Optional: Also call the manager if needed
    }

}
public interface IUpgradeAction
{
    void PerformUpgrade(GridWithSlotsSO gridWithSlotsSO);
}