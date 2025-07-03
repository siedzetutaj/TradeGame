using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
public class VendorItemGenerator : MonoBehaviourSingleton<VendorItemGenerator>
{
    public VendorItemGeneratorSO vendorItemGeneratorSO;

    [SerializeField] private Transform _itemHolder;
    [SerializeField] private ItemSOManager _itemManager;
    [SerializeField] private GameObject _itemPrefab;
    [SerializeField] private GridType gridTypeToPlaceItems = GridType.vendorToBuy;


    [Header("Items Generator")]
    [Tooltip("Selceted item types will appear in vendor eq")]
    [SerializeField] private SerializableDictionary<ItemType, int> _generatedItemTypes = new();

    [Tooltip("Selected rarity will be randomized across item type pool")]
    [SerializeField] private SerializableDictionary<ItemRarity, int> _generatedItemRarities = new();

    [Tooltip("When you select item here it will always appear in vendor eq")]
    [SerializeField] private SerializableDictionary<ItemSO, int> _itemsToGenerate = new();

    [Header("Vendor prices")]

    [Tooltip("Selceted item types will have this multiplayer on buy")]
    [SerializeField] private SerializableDictionary<ItemType, float> _vendorBuyMultiplayerForTypes = new();

    [Tooltip("Selected items will have this multiplayer on buy (this have priority over the types)")]
    [SerializeField] private SerializableDictionary<ItemSO, float> _vendorBuyMultiplayerForItems = new();

    [Header("Vendor wants this items")]

    [Tooltip("Selceted item types will have this multiplayer on sell")]
    [SerializeField] private SerializableDictionary<ItemType, float> _vendorSellMultiplayerForTypes = new();

    [Tooltip("Selected items will have this multiplayer on sell (this have priority over the types)")]
    [SerializeField] private SerializableDictionary<ItemSO, float> _vendorSellMultiplayerForItems = new();

    private GridManager _venorToBuy;
    private GridManager _chest;
    private TradeReferences _tradeReferences;

    public List<ItemSO> ItemsToBuy = new();
    public List<GameObject> CreatedItemsToBuy = new();

    private TradeMechanic _tradeMechanic;

    /// <summary>
    /// To DO:
    /// Trzeb dodaæ przelicznik który zmienia mnoznik kiedy sprzeda sie kilka tych samych itemkow
    /// albo kiedy sprzeda sie duzo itemkow danego typu etc.
    /// </summary>
    public void OnVendorButtonPressed()
    {
        _tradeReferences = TradeReferences.Instance;
        GameLogic.Instance.EnableVendorPanel();
        _itemHolder = _tradeReferences.ItemHolder;
        _venorToBuy = _tradeReferences.VendorToBuyGrid;
        _chest = _tradeReferences.ChestGrid;
        _tradeReferences.ActiveItemGenerator = this;
        _tradeMechanic = TradeMechanic.Instance;
        DestroyItemsGameObjects();
        _tradeMechanic.ActiveItemGenerator = this;

        if (gridTypeToPlaceItems == GridType.chest)
        {
            _tradeReferences.Chest.SetActive(true);
            _tradeReferences.Trade.SetActive(false);
            _tradeReferences.Upgrade.SetActive(false);
        }

        if (ItemsToBuy.Count == 0)
        {
            _generatedItemTypes.Clear();
            _generatedItemRarities.Clear();
            _itemsToGenerate.Clear();

            _generatedItemTypes = new SerializableDictionary<ItemType, int>(vendorItemGeneratorSO.GeneratedItemTypes);
            _generatedItemRarities = new SerializableDictionary<ItemRarity, int>(vendorItemGeneratorSO.GeneratedItemRarities);
            _itemsToGenerate = new SerializableDictionary<ItemSO, int>(vendorItemGeneratorSO.ItemsToGenerate);

            _vendorBuyMultiplayerForTypes = new SerializableDictionary<ItemType, float>(vendorItemGeneratorSO.VendorBuyMultiplayerForTypes);
            _vendorBuyMultiplayerForItems = new SerializableDictionary<ItemSO, float>(vendorItemGeneratorSO.VendorBuyMultiplayerForItems);

            _vendorSellMultiplayerForTypes = new SerializableDictionary<ItemType, float>(vendorItemGeneratorSO.VendorSellMultiplayerForTypes);
            _vendorSellMultiplayerForItems = new SerializableDictionary<ItemSO, float>(vendorItemGeneratorSO.VendorSellMultiplayerForItems);

            GenerateRandomItemsFromSelectedData();
        }
        else
        {
            foreach (ItemSO item in ItemsToBuy)
            {
                CreateItem(item);
            }
        }
    }
    private void DestroyItemsGameObjects()
    {
        if (_tradeMechanic.ActiveItemGenerator)
        {
            foreach (var item in _tradeMechanic.ActiveItemGenerator.CreatedItemsToBuy)
            {
                if (item)
                {
                    Destroy(item);
                }
            }
            _tradeMechanic.ActiveItemGenerator.CreatedItemsToBuy.Clear();

        }
    }
    private void GenerateRandomItemsFromSelectedData()
    {
        var itemTypes = _generatedItemTypes.ToDictionary(entry => entry.Key, entry => entry.Value);
        int chance = itemTypes.Values.Sum();

        foreach (var itemRarity in _generatedItemRarities.Keys)
        {
            for (int i = _generatedItemRarities[itemRarity]; i > 0; i--)
            {
                int random = Random.Range(0, chance);

                foreach (var itemType in _generatedItemTypes.Keys)
                {
                    if (itemTypes[itemType] < random)
                    {
                        random -= itemTypes[itemType];
                        continue;
                    }

                    FindItem(itemType, itemRarity);

                    itemTypes[itemType]--;

                    chance--;
                    break;
                }
            }
        }
        foreach (var itemToGenerate in _itemsToGenerate.Keys)
        {
            for (int i = 0; i < _itemsToGenerate[itemToGenerate]; i++)
            {
                CreateItem(itemToGenerate);
                ItemsToBuy.Add(itemToGenerate);

            }
        }
    }
    private void FindItem(ItemType type, ItemRarity rarity)
    {
        List<ItemSO> foundItems = new();
        foreach (var iteminManager in _itemManager.allItems)
        {
            if (iteminManager.itemtype == type && iteminManager.itemRarity == rarity)
            {
                foundItems.Add(iteminManager);
            }
        }
        if (foundItems.Count > 0)
        {
            int random = Random.Range(0, foundItems.Count - 1);

            ItemSO itemData = foundItems[random];
            CreateItem(itemData);
            ItemsToBuy.Add(itemData);

        }
        else
            Debug.Log($"Didn't find item of this type: {type}, and this rarity: {rarity}");
    }
    private void CreateItem(ItemSO data)
    {

        GameObject createdItem = Instantiate(_itemPrefab, _itemHolder);
        CreatedItemsToBuy.Add(createdItem);

        GridItem gridItem = createdItem.GetComponent<GridItem>();
        if (gridTypeToPlaceItems == GridType.vendorToBuy)
        {
            gridItem.Initialize(data, false, GridType.vendorToBuy, _venorToBuy);
            gridItem.TryAutomaticPlacement(_venorToBuy);
        }
        else if (gridTypeToPlaceItems == GridType.chest)
        {
            gridItem.Initialize(data, false, GridType.chest, _chest);
            gridItem.TryAutomaticPlacement(_chest);
            gridItem.ItemAcquired = true;
        }
    }
    public void OnItemAcquired(GridItem acquiredItem)
    {
        CreatedItemsToBuy.Remove(acquiredItem.gameObject);
        ItemsToBuy.Remove(acquiredItem.ItemSO);
        //tutaj dodac ze itemy sa przypisane do caravany, ustawic transform zeby itemy byly dzieckiem caravany
        CaravanManager.Instance.TakeItem(acquiredItem.gameObject);
        if (acquiredItem.ItemSO.itemtype == ItemType.food && acquiredItem.ItemSO.ration > 0)
        {
            ResourceManager.Instance.AddResourceToInventory(acquiredItem);
        }
    }
    public void OnItemReturned(GridItem returnedItem)
    {
        CreatedItemsToBuy.Add(returnedItem.gameObject);
        ItemsToBuy.Add(returnedItem.ItemSO);
        CaravanManager.Instance.GiveAwayItem(returnedItem.gameObject);
        if (returnedItem.ItemSO.itemtype == ItemType.food && returnedItem.ItemSO.ration > 0)
        {
            ResourceManager.Instance.RemoveResourceFromInventory(returnedItem);
        }
    }
    public void OnCityExit()
    {
        ItemsToBuy.Clear();
        DestroyItemsGameObjects();
    }
    public float ItemBuyMultiplayer(ItemSO itemToBuy)
    {
        return
            _vendorBuyMultiplayerForItems.TryGetValue(itemToBuy, out var selectedItemMultiplier) ? selectedItemMultiplier
            : _vendorBuyMultiplayerForTypes.TryGetValue(itemToBuy.itemtype, out var itemTypeMultiplier) ? itemTypeMultiplier
            : 1.2f;
    }
    public float ItemSellMultiplayer(ItemSO itemToSell)
    {
        return
            _vendorSellMultiplayerForItems.TryGetValue(itemToSell, out var selectedItemMultiplier) ? selectedItemMultiplier
            : _vendorSellMultiplayerForTypes.TryGetValue(itemToSell.itemtype, out var itemTypeMultiplier) ? itemTypeMultiplier
            : 1f;
    }
}
