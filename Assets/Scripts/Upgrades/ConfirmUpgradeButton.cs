using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfirmUpgradeButton : MonoBehaviourSingleton<ConfirmUpgradeButton>
{
    public UpgradeType UpgradeType;
    public int UpgradeValue;

    public void OnConfirmUpgradeButtonPressed()
    {
        ItemsToUpgradeManager.Instance.ApplyUpgrade(UpgradeType, UpgradeValue);
    }

}
