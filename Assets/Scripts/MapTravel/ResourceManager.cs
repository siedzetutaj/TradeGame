using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceManager : MonoBehaviourSingleton<ResourceManager>
{
    private TravelButton _currentTravelButton;

    private float _selectedRationsAmount = 0;
    private float _rationsNeededToTravel = 0;

    float SelectedRationsAmount
    {
        get
        {
            return _selectedRationsAmount;
        }
        set
        {
            _selectedRationsAmount = value;
            _resourceSlider.value = _selectedRationsAmount;
        }
    }
    float RationNeededToTravel
    {
        get
        {
            return _rationsNeededToTravel;
        }
        set
        {
            _rationsNeededToTravel = value;
            _resourceSlider.maxValue = _rationsNeededToTravel;
        }
    }

    [SerializeField] private SerializableDictionary<ItemData, int> _resourcesToGenerate = new();
    
    [SerializeField] private List<Resource> _resources = new();

    [SerializeField] private GameObject _resourcePrefab;
    [SerializeField] private Transform _resourcesHolderTransform;
    [SerializeField] private Button _travelButton;
    [SerializeField] private GameObject _resourcePanel;
    [SerializeField] private Slider _resourceSlider;
    /*TODO:
     * Trzeba to przerobic zeby przeliczalo zasoby dopiero jak sie wejdzie do mapy
     * A nie za kazdym razem jak sie doda jedzenie do eq
     * Plus musi uwzgledniac curr stack count
    */
    private void OnEnable()
    {
        GameLogic.Instance.onEnabledMapPanelAction += WhenMapIsEnabled;
    }
    private void OnDisable()
    {
        GameLogic.Instance.onEnabledMapPanelAction -= WhenMapIsEnabled;
    }
    private void Start()
    {
        _resourcePanel.SetActive(false);
    }
    private void AddResourceToInventory(ItemData item)
    {
        if (_resourcesToGenerate.Keys.Contains(item))
            _resourcesToGenerate[item] += item.StackCount;
        else
            _resourcesToGenerate.Add(item, item.StackCount);
    }
    private void RemoveResourceFromInventory(ItemData item, int amount = 1)
    {
        _resourcesToGenerate[item] -= amount;

        if (_resourcesToGenerate[item] <= 0)
            _resourcesToGenerate.Remove(item);
    }
    public void SetUpResourceManager(TravelButton travelButton, float rations)
    {
        DestroyResourcesInMap();
        RationNeededToTravel = rations;
        _currentTravelButton = travelButton;
        foreach(var item in _resourcesToGenerate)
        {
            GameObject resourceObject = Instantiate(_resourcePrefab, _resourcesHolderTransform);
            Resource resource = resourceObject.GetComponent<Resource>();
            resource.Initialize(
                 item.Key.ItemSO.sprite,
                 item.Value,
                 item.Key
                );
            _resources.Add(resource);
        }

    }
    private void DestroyResourcesInMap()
    {
        //GridItem item = resource.GridItem;

        //item.ClearOccupiedCells();
        //CaravanManager.Instance.ChangeWeightWalue(-item.ItemSO.weight);

        //Destroy(item);
        foreach (Resource resource in _resources)
        {
            Destroy(resource.gameObject);
        }
        _resources.Clear();
        SelectedRationsAmount = 0;
    }
    public void OnResourceAmountChange(Resource resource, int newAmount, int previousAmount)
    {
        int amountDiffrence = newAmount - previousAmount;
        SelectedRationsAmount += resource.ItemData.ItemSO.ration * amountDiffrence;

        if (SelectedRationsAmount >= RationNeededToTravel)
        {
            _travelButton.interactable = true;
            _currentTravelButton.SetLineImageToGreen();
        }
        else
        {
            _travelButton.interactable = false;
            _currentTravelButton.SetLineImageToRed();
        }
    }
    public void OnTravelButtonPressed()
    {
        foreach (Resource resource in _resources)
        {
            if (resource.CurrentAmountValue > 0)
            {
                CaravanManager.Instance.OnItemUsedAsRation(resource.ItemData, resource.CurrentAmountValue);
            }
        }
        DestroyResourcesInMap();
        _currentTravelButton.StartTravelling();
        _travelButton.interactable = false;
        _resourcePanel.SetActive(false);
    }
    private void WhenMapIsEnabled()
    {
        if (_resourcePanel.activeInHierarchy)
        {
            DestroyResourcesInMap();
            _resourcePanel.SetActive(false);
            TravelButtonsManager.Instance.IsResourceMenuOpen = false;
            _currentTravelButton.OnPointerExit(null);
        }

        _resourcesToGenerate.Clear();

        foreach (ItemData item in CaravanManager.Instance.CaravanItemsData)
        {
            if(item == null) continue;
            if (item.ItemSO.itemType==ItemType.food)
            {
                AddResourceToInventory(item);
            }
        }
    }
   
}
