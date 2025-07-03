using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestMechanic : MonoBehaviourSingleton<ChestMechanic>
{
    private void OnDisable()
    {
        var tradeReferences = TradeReferences.Instance;
        tradeReferences.Trade.SetActive(true);
        tradeReferences.Chest.SetActive(false);
    }
}
