using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Resource : MonoBehaviour
{
    public GridItem GridItem;
    public int MaxAmountValue = 0;
    public int CurrentAmountValue = 0;
    public int PreviousAmountValue = 0;

    [SerializeField] private Image ResourceImage;
    [SerializeField] private TextMeshProUGUI AmountDisplay;

    private ResourceManager _resourceManager;
    public void Initialize(Sprite sprite, int maxAmount, GridItem item)
    {
        ResourceImage.sprite = sprite;
        MaxAmountValue = maxAmount;
        GridItem = item;
    }
    private void Start()
    {
        AmountDisplay.text = "0";
        _resourceManager = ResourceManager.Instance;
    }

    public void OnAddValueButtonClick()
    {
        CurrentAmountValue++;
        if (CurrentAmountValue > MaxAmountValue)
        {
            CurrentAmountValue = 0;
        }
        UpdateAmountDisplay();
    }
    public void OnRemoveValueButtonClick()
    {
        CurrentAmountValue--;
        if (CurrentAmountValue < 0)
        {
            CurrentAmountValue = MaxAmountValue;
        }
        UpdateAmountDisplay();
    }
    private void UpdateAmountDisplay()
    {
        AmountDisplay.text = CurrentAmountValue.ToString();
        _resourceManager.OnResourceAmountChange(this, CurrentAmountValue, PreviousAmountValue);
        PreviousAmountValue = CurrentAmountValue;
    }
}
