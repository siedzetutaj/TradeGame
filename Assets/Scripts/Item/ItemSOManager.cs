using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemManager", menuName = "ScriptableObjects/ItemManager", order = 1)]
public class ItemSOManager : ScriptableObject
{
    public List<ItemSO> allItems = new();
#if UNITY_EDITOR
    [Header("Item Creator")]
    [SerializeField] private string itemName;
    [SerializeField] private string description;
    [SerializeField] private Sprite sprite;
    [SerializeField] private int value;
    [SerializeField] private int maxCount;
    [SerializeField] private int weight;
    [SerializeField] private ItemRarity itemRarity = ItemRarity.debug;
    [SerializeField] public ItemType itemtype = ItemType.debug;
    [HideInInspector] public float ration;
    public List<Vector2Int> shapeOffsets = new(); // Offsets for shape cells
    public void CreateItem()
    {
        string fullPath = AssetDatabase.GetAssetPath(this);
        string path = Path.GetDirectoryName(fullPath);
        
        if (itemName != null
            && description != null
            && sprite != null
            && shapeOffsets.Count > 0
            && itemtype != ItemType.debug
            && itemRarity != ItemRarity.debug
            && !allItems.Any(x => x.itemName == itemName))
        {
            Debug.Log($"{itemName} is Created");

            ItemSO item = CreateAsset<ItemSO>(path, $"{itemName}");

            item.itemName = itemName;
            item.description = description;
            item.sprite = sprite;
            item.value = value;
            item.maxCount = maxCount;
            item.weight = weight;
            item.itemRarity = itemRarity;
            item.itemtype = itemtype;
            item.ration = ration;
            item.shapeOffsets = shapeOffsets;

            EditorUtility.SetDirty(item);
            allItems.Add(item);

            itemName = null;
            description = null;
            sprite = null;
            value = 0;
            maxCount = 0;
            weight = 0;
            itemRarity = ItemRarity.debug;
            itemtype = ItemType.debug;
            ration = 0;
            shapeOffsets = new List<Vector2Int>();
        }
        else
        {
            Debug.Log("Fill all fields under Thought Crreator/Or check name");
        }
    }


    public void DestroyItem(ItemSO itemToDestroy)
    {
        if (itemToDestroy != null)
        {
            Debug.Log($"{itemToDestroy.itemName} is Delated");
            allItems.Remove(itemToDestroy);
            AssetDatabase.DeleteAsset($"Assets/Items/{itemToDestroy.itemName}.asset");
        }
        else
        {
            Debug.Log("Pick Item under Item Destroyer");
        }
    }

    #region Utilites
    private static T CreateAsset<T>(string path, string assetName) where T : ScriptableObject
    {
        string fullPath = $"{path}/{assetName}.asset";

        T asset = LoadAsset<T>(path, assetName);

        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<T>();

            AssetDatabase.CreateAsset(asset, fullPath);
        }

        return asset;
    }
    private static T LoadAsset<T>(string path, string assetName) where T : ScriptableObject
    {
        string fullPath = $"{path}/{assetName}.asset";

        return AssetDatabase.LoadAssetAtPath<T>(fullPath);
    }
    #endregion
#endif
}

