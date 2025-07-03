using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DebugAllItemsDisplay : MonoBehaviour
{
    [SerializeField] private ItemSOManager _itemSOManager;
    [SerializeField] private GameObject _itemButton;
    [SerializeField] private DebugGridForCreatingGridWithSlots _grid;

    private void Start()
    {
        foreach(ItemSO item in _itemSOManager.allItems)
        {
            GameObject itemButton = Instantiate(_itemButton, transform);
            DebugItemButton debugItem = _itemButton.GetComponent<DebugItemButton>();
            debugItem.ItemSO = item;
            debugItem.Image.sprite = item.sprite;
            debugItem.Grid = _grid;
        }
    }
}
