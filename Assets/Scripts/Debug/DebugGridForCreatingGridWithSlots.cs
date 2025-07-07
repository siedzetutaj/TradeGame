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
        // 1. Collect all occupied cells with their original positions
        HashSet<Vector2Int> originalOccupiedCells = new HashSet<Vector2Int>();

        foreach (var itemList in ItemsToPlaceIn.Values)
        {
            foreach (GridItem item in itemList)
            {
                if (item.Initialcell == null) continue;

                Vector2Int basePos = item.Initialcell.listPosition;
                originalOccupiedCells.Add(basePos);

                foreach (Vector2Int offset in item.ShapeOffsets)
                {
                    Vector2Int cellPos = new Vector2Int(
                        basePos.x - offset.y,
                        basePos.y + offset.x
                    );
                    originalOccupiedCells.Add(cellPos);
                }
            }
        }

        // 2. Calculate global normalization values
        int minRow = originalOccupiedCells.Min(c => c.x);
        int minCol = originalOccupiedCells.Min(c => c.y); // Global minimum column

        // 3. Create normalized grid structure
        Dictionary<int, int> rowWidths = new Dictionary<int, int>();
        Dictionary<Vector2Int, Vector2Int> positionMap = new Dictionary<Vector2Int, Vector2Int>();
        List<Vector2Int> tempDisableCellsAt = new List<Vector2Int>();

        // First pass: determine row widths and create position mapping
        foreach (Vector2Int pos in originalOccupiedCells)
        {
            int normalizedRow = pos.x - minRow;
            int normalizedCol = pos.y - minCol; // Use global column normalization

            // Track maximum column for each row
            if (!rowWidths.TryGetValue(normalizedRow, out int currentMax) || normalizedCol > currentMax)
            {
                rowWidths[normalizedRow] = normalizedCol;
            }

            positionMap[pos] = new Vector2Int(normalizedRow, normalizedCol);
        }

        // Second pass: create cellsPerRow list and find disabled cells
        List<int> tempCellsPerRow = new List<int>();
        int maxRow = rowWidths.Keys.Max();

        for (int row = 0; row <= maxRow; row++)
        {
            if (rowWidths.TryGetValue(row, out int maxCol))
            {
                tempCellsPerRow.Add(maxCol + 1); // +1 because columns are 0-based

                // Find empty cells in this row
                for (int col = 0; col <= maxCol; col++)
                {
                    Vector2Int testPos = new Vector2Int(row + minRow, col + minCol);
                    if (!originalOccupiedCells.Contains(testPos))
                    {
                        tempDisableCellsAt.Add(new Vector2Int(row, col));
                    }
                }
            }
            else
            {
                // Empty row (shouldn't happen since we only process rows with items)
                tempCellsPerRow.Add(0);
            }
        }

        // 4. Update item positions and offsets to normalized grid
        foreach (var itemList in ItemsToPlaceIn.Values)
        {
            foreach (GridItem item in itemList)
            {
                if (item.Initialcell == null) continue;

                Vector2Int originalBasePos = item.Initialcell.listPosition;
                if (positionMap.TryGetValue(originalBasePos, out Vector2Int newBasePos))
                {
                    // Update shape offsets relative to new base position
                    List<Vector2Int> newOffsets = new List<Vector2Int>();
                    foreach (Vector2Int offset in item.ShapeOffsets)
                    {
                        Vector2Int originalCellPos = new Vector2Int(
                            originalBasePos.x - offset.y,
                            originalBasePos.y + offset.x
                        );
                        if (positionMap.TryGetValue(originalCellPos, out Vector2Int newCellPos))
                        {
                            Vector2Int newOffset = new Vector2Int(
                                newCellPos.y - newBasePos.y,
                                newBasePos.x - newCellPos.x
                            );
                            newOffsets.Add(newOffset);
                        }
                    }
                    item.ShapeOffsets = newOffsets;

                    // Update the item's base position
                    item.Initialcell.listPosition = newBasePos;
                }
            }
        }

        // 5. Create the final grid
        CreateItem(tempCellsPerRow, tempDisableCellsAt);
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
            && !gridWithSlotsSOManager.allGridsWithSlotsSO.Any(x => x.GridName == NameInputField.text)
            && _intInputField.text != string.Empty)
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