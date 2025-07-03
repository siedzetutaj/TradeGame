using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TravelButtonsManager : MonoBehaviourSingleton<TravelButtonsManager>
{
    private List<TravelButton> _enabledTravelButtons = new();
    [SerializeField] private TravelButton _currentTravelButton;
    public bool IsResourceMenuOpen = false;
    public bool IsMoving = false;

    public TravelButton CurrentTravelButton
    {
        get
        {
            return _currentTravelButton;
        }
        set
        {
            _currentTravelButton = value;
        }
    }
    public void AddTravelButton(TravelButton button)
    {
        _enabledTravelButtons.Add(button);
    }
    public void RemoveTravelButton(TravelButton button)
    {
        _enabledTravelButtons.Remove(button);
    }

    public void DisableTravelButtons()
    {
        foreach(TravelButton travelButton in _enabledTravelButtons)
        {
            travelButton.button.interactable = false;
        }
    }
    public void EnableTravelButtons()
    {
        foreach (TravelButton travelButton in _enabledTravelButtons)
        {
            travelButton.button.interactable = true;
        }
    }
    public bool CanGoToThisDestination(TravelButton travelButton)
    {
        return (_currentTravelButton.PossibleDestiantionsToGo.ContainsKey(travelButton)
            || _currentTravelButton == travelButton)
            && _enabledTravelButtons.Contains(travelButton);
    }
    public float TravelResourceCost(TravelButton travelButton)
    {
        return travelButton == _currentTravelButton ? 0 
            : _currentTravelButton.PossibleDestiantionsToGo[travelButton];
    }
}
