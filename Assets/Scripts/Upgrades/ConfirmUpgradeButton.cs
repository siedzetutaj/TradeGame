using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfirmUpgradeButton : MonoBehaviourSingleton<ConfirmUpgradeButton>
{
    public UpgradeType UpgradeType;
    public int UpgradeValue;

    public void OnConfirmUpgradeButtonPressed()
    {
        switch (UpgradeType)
        {
            case (UpgradeType.CaravanWeigh):
                {
                    return;
                }
            
            
            case (UpgradeType.Debug):
            default:
                {
                    Debug.Log("missingUpgradeType");
                    return;
                }
        }
    }

}
