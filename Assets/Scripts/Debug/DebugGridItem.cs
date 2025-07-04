using UnityEngine;
using UnityEngine.EventSystems;

public class DebugGridItem : GridItem
{
    public DebugGridForCreatingGridWithSlots debugGrid;
    public override void Initialize(ItemSO itemSO, bool isItemAcquired,
        GridType type, GridManager manager)
    {
        base.Initialize(itemSO, isItemAcquired, type, manager);

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
    }
    public override void DestroyItem()
    {
        debugGrid.RemoveItem(this);
        base.DestroyItem();
    }

}
