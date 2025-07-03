using UnityEngine;

[CreateAssetMenu(fileName = "Vendor Item Generator", menuName = "ScriptableObjects/VendorItemGenerator", order = 1)]
public class VendorItemGeneratorSO : ScriptableObject
{
    [Header("Items Generator")]

    [Tooltip("Selceted item types will appear in vendor eq")]
    [SerializeField] public SerializableDictionary<ItemType, int> GeneratedItemTypes = new();

    [Tooltip("Selected rarity will be randomized across item type pool")]
    [SerializeField] public SerializableDictionary<ItemRarity, int> GeneratedItemRarities = new();

    [Tooltip("When you select item here it will always appear in vendor eq")]
    [SerializeField] public SerializableDictionary<ItemSO, int> ItemsToGenerate = new();
    
    [Header("Vendor prices")]

    [Tooltip("Selceted item types will have this multiplayer on buy")]
    [SerializeField] public SerializableDictionary<ItemType, float> VendorBuyMultiplayerForTypes = new();

    [Tooltip("Selected items will have this multiplayer on buy (this have priority over the types)")]
    [SerializeField] public SerializableDictionary<ItemSO, float> VendorBuyMultiplayerForItems = new();
    
    [Header("Vendor wants this items")]

    [Tooltip("Selceted item types will have this multiplayer on sell")]
    [SerializeField] public SerializableDictionary<ItemType, float> VendorSellMultiplayerForTypes = new();

    [Tooltip("Selected items will have this multiplayer on sell (this have priority over the types)")]
    [SerializeField] public SerializableDictionary<ItemSO, float> VendorSellMultiplayerForItems = new();
}
