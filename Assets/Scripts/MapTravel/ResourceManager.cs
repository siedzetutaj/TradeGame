using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [SerializeField] private Dictionary<GridItem, int> _resourcesToGenerate = new();
    
    [SerializeField] private List<Resource> _resources = new();

    [SerializeField] private GameObject _resourcePrefab;
    [SerializeField] private Transform _resourcesHolderTransform;
    [SerializeField] private Button _travelButton;
    [SerializeField] private GameObject _resourcePanel;
    [SerializeField] private Slider _resourceSlider;
    /*
     * Trzeba to przemyslec
     * resource dodaja sie do listy
     * ale kiedy mapa jest odpalona to generuja sie obiekty
     * kiedy mapa jest wylaczona to usuwaja sie
     * kiedy itemy sa dodawane w inventory to dodaja sie do dicionary
     * analogicznie usuwanie
     * 
     * do tego trzeba tu dostarczyc koszt podrozy
     * i przeniesc funkcjonalnosc ze dopiero jak travel button jest nacisniety to ziutek leci
     * 
     * 
     * GL HF
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

    public void AddResourceToInventory(GridItem item, int amount = 1)
    {
        if (_resourcesToGenerate.Keys.Contains(item))
            _resourcesToGenerate[item] += amount;
        else
            _resourcesToGenerate.Add(item, amount);
    }
    public void RemoveResourceFromInventory(GridItem item, int amount = 1)
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
    public void DestroyResourcesInMap()
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
        SelectedRationsAmount += resource.GridItem.ItemSO.ration * amountDiffrence;

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
                RemoveResourceFromInventory(resource.GridItem, resource.CurrentAmountValue);
                CaravanManager.Instance.OnItemUsedAsRation(resource.GridItem);
                CaravanManager.Instance.ChangeWeightWalue(-resource.GridItem.ItemSO.weight);
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
    }
   
}
