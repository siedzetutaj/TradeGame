using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DebugItemButton : MonoBehaviour
{
    public Image Image;
    public ItemSO ItemSO;
    public DebugGridForCreatingGridWithSlots Grid;
    [SerializeField] private GameObject _itemPrefab;

    public void ButtonForCreatingShape()
    {
        GameObject Item = Instantiate(_itemPrefab, transform);
        DebugGridItem debugGridItem = Item.AddComponent<DebugGridItem>();
        debugGridItem.Initialize(ItemSO, false, GridType.debug, null);
        debugGridItem.TryAutomaticPlacement(Grid);
        debugGridItem.debugGrid = Grid;
        Grid.AddItem(debugGridItem);
    }
}
