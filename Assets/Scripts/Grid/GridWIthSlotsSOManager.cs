using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "GridWIthSlotsSOManager", menuName = "ScriptableObjects/GridWIthSlotsSOManager", order = 1)]
public class GridWIthSlotsSOManager : ScriptableSingleton<GridWIthSlotsSOManager>
{
    public List<GridWithSlotsSO> allGridsWithSlotsSO;

    public void ResetSavedValues()
    {
        foreach(GridWithSlotsSO grid in allGridsWithSlotsSO)
        {
            grid.ItemsPlacedIn.Clear();
        }
    }

}
