using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemToUpgrade : MonoBehaviour
{
    [SerializeField] private Image _itemImage;
    [SerializeField] private TextMeshProUGUI _textMeshProUGUI;
    private int _maxItemValue;
    private int _itemValue;
    public ItemSO ItemSO;
    public bool canContainMoreItems = true;
    public void Initialize(int itemValue, int maxItemValue, Sprite itemSprite, ItemSO itemSO)
    {
        _itemImage.sprite = itemSprite;
        _maxItemValue = maxItemValue;
        _itemValue = itemValue;
        ChangeItemValueDisplay();
        ItemSO = itemSO;
        canContainMoreItems = true;
    }
    public void ChangeItemValueDisplay()
    {
        _textMeshProUGUI.text = $"{_itemValue}/{_maxItemValue}";
    }
    public bool AddItem(ItemSO item, int value = 1)
    {
        if (item == ItemSO && canContainMoreItems)
        {
            _itemValue += value;
            if (_itemValue == _maxItemValue)
            {
                canContainMoreItems = false;
            }
            else if (_itemValue >= _maxItemValue)
            {
                _itemValue -= value;
                return false;
            }
            ChangeItemValueDisplay();
            return true;
        }
        return false;
    }
    public bool RemoveItem(ItemSO item, int value = 1)
    {
        if (item == ItemSO)
        {
            _itemValue -= value;
            canContainMoreItems = true;
            ChangeItemValueDisplay();
            return true;
        }
        return false;
    }
}
