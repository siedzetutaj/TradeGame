using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/CreateItem", order = 1)]
public class ItemSO : ScriptableObject
{
    public string itemName;
    public string description;
    public Sprite sprite;
    public int value;
    public int maxCount;
    public int weight;
    public ItemRarity itemRarity;
    public ItemType itemtype;
    [HideInInspector] public float ration;
    public List<Vector2Int> shapeOffsets = new List<Vector2Int>(); // Offsets for shape cells
}

public enum ItemType
{
    food,
    valuable,
    ore,
    tool,
    weapon,
    potion,
    magic,
    material,
    debug
}
public enum ItemRarity
{
    common,
    uncommon,
    rare,
    veryRare,
    debug
}