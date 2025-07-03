using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TradeReferences : MonoBehaviourSingleton<TradeReferences>
{
    public Transform ItemHolder;
    public GameObject GridItemBackgroundPrefab;
    
    public GridManager VendorToBuyGrid;
    public GridManager VendorToSellGrid;
    public GridManager CaravanGrid;
    public GridManager ChestGrid;
    public GridManagerWithSlots UpgradeGrid;
    
    public GameObject Trade;
    public GameObject Chest;
    public GameObject Upgrade;
    
    public Transform UpgradesHolder;

    public VendorItemGenerator ActiveItemGenerator;
}
