#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.IO;
using TMPro;
using UnityEngine.UI;

public class DebugGridForCreatingGridWithSlots : GridManager
{
    public Button saveButton; 

    public Dictionary<ItemSO, List<GridItem>> ItemsToPlaceIn = new();
    public TMP_InputField NameInputField;

    [SerializeField] private TMP_Dropdown _dropdown;
    [SerializeField] private TMP_InputField _intInputField;
    protected override void OnEnable()
    {
        base.OnEnable();
        _dropdown.ClearOptions();
        _dropdown.AddOptions(new List<string>(System.Enum.GetNames(typeof(UpgradeType))));
        _intInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
    }
    private void Update()
    {
        if (CanSave())
            saveButton.interactable = true;
        else 
            saveButton.interactable = false;
    }

    public void ButtonForResetingTheGrid()
    {
        var allGridItems = ItemsToPlaceIn.Values.SelectMany(list => list).ToList();

        foreach (GridItem gridItem in allGridItems)
        {
            if (gridItem != null && gridItem.gameObject != null)
            {
                gridItem.DestroyItem();
            }
        }

        ItemsToPlaceIn.Clear();
    }
    //dodac zapis jakie itemy
    public void ButtonForSavingGridToSO()
    {
        List<int> tempCellsPerRow = new List<int>(cellsPerRow);
        List<Vector2Int> tempdisableCellsAt = new();
        for (int row = 0; row < cellsPerRow.Count; row++)
        {
            for (int col = 0; col < cellsPerRow[row]; col++)
            {
                if (!grid[row][col].isOccupied)
                    tempdisableCellsAt.Add(new Vector2Int(row, col));
            }
        }
        for (int row = cellsPerRow.Count - 1; row >= 0; row--)
        {
            List<Vector2Int> disabledCellsAtRow = new();
            for (int col = 0; col < cellsPerRow[row]; col++)
            {
                Vector2Int cellPosition = new(row, col);
                if (tempdisableCellsAt.Contains(cellPosition))
                {
                    disabledCellsAtRow.Add(cellPosition);
                }
            }
            if (disabledCellsAtRow.Count >= cellsPerRow[row])
            {
                tempCellsPerRow.RemoveAt(row); // Safe: Doesn't affect earlier indices
                tempdisableCellsAt.RemoveAll(x => disabledCellsAtRow.Contains(x));
            }
        }
        CreateItem(tempCellsPerRow, tempdisableCellsAt);
    }
    public void AddItem(GridItem item)
    {
        if (ItemsToPlaceIn.ContainsKey(item.ItemSO))
        {
            ItemsToPlaceIn[item.ItemSO].Add(item);
        }
        else
        {
            ItemsToPlaceIn.Add(item.ItemSO, new List<GridItem> { item });
        }
        Debug.Log($"{item.ItemSO.name} count: {ItemsToPlaceIn[item.ItemSO].Count}");
    }
    public void RemoveItem(GridItem item)
    {
        if (ItemsToPlaceIn.ContainsKey(item.ItemSO))
        {
            if (ItemsToPlaceIn[item.ItemSO].Count > 1)
            {
                ItemsToPlaceIn[item.ItemSO].Remove(item);
                Debug.Log($"{item.ItemSO.name} count: {ItemsToPlaceIn[item.ItemSO].Count}");
            }
            else
            {
                ItemsToPlaceIn.Remove(item.ItemSO);
                Debug.Log($"{item.ItemSO.name} is removed");
            }
        }
    }
    public void CreateItem(List<int> cellsPerRow, List<Vector2Int> disableCellsAt)
    {
        string fullPath = AssetDatabase.GetAssetPath(GridWIthSlotsSOManager.instance);
        string path = Path.GetDirectoryName(fullPath);

        Debug.Log($"{NameInputField.text} is Created");

        GridWithSlotsSO gridSO = CreateAsset<GridWithSlotsSO>(path, $"{NameInputField.text}");

        gridSO.GridName = new(NameInputField.text);
        gridSO.ItemsToPlaceIn = new(ItemsToPlaceIn.ToDictionary(
                                    pair => pair.Key,
                                    pair => pair.Value.Count
                                    ));
        gridSO.CellsPerRow = new(cellsPerRow);
        gridSO.DisableCellsAt = new(disableCellsAt);
        gridSO.UpgradeType = (UpgradeType)_dropdown.value;
        gridSO.UpgradeValue = _intInputField.text != string.Empty ? int.Parse(_intInputField.text) : 0;
        EditorUtility.SetDirty(gridSO);
        GridWIthSlotsSOManager.instance.allGridsWithSlotsSO.Add(gridSO);

        NameInputField.text = null;

        Debug.Log("Fill all fields under Thought Crreator/Or check name");
    }
    #region Utilites
    public bool CanSave()
    {
        GridWIthSlotsSOManager gridWithSlotsSOManager = GridWIthSlotsSOManager.instance;
        if (NameInputField.text != string.Empty
            && ItemsToPlaceIn != null
            && cellsPerRow != null
            && !gridWithSlotsSOManager.allGridsWithSlotsSO.Any(x => x.GridName == NameInputField.text))
        {
            return true;
        }
        return false;
    }
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
}
#endif