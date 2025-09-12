using System.Collections.Generic;
using UnityEngine;

public class QuestSO : ScriptableObject
{
    public VendorItemGeneratorSO Destination;
    public List<NeededItems> NeededItemsList = new();

}

public class NeededItems
{
    public ItemSO Item;
    public int Amount;
}