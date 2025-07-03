using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GameLogic : MonoBehaviourSingleton<GameLogic> 
{
    [SerializeField] private List<GameObject> _Panels;

    private InputSystem _inputSystem;

    public Action onEnabledMapPanelAction;
    public Action onEnabledDestinationPanelAction;
    public Action onEnabledVendorPanelAction;

    private void OnEnable()
    {
        _inputSystem = InputSystem.Instance;
        _inputSystem.onMapAction += EnableMapPanel;
        _inputSystem.onDestinationAction += EnableDestinationPanel;
       // _inputSystem.onVendorAction += EnableVendorPanel;
    }
    private void OnDisable()
    {
        _inputSystem.onMapAction -= EnableMapPanel;
        _inputSystem.onDestinationAction -= EnableDestinationPanel;
     //   _inputSystem.onVendorAction -= EnableVendorPanel;
    }
    public void EnableMapPanel()
    {
        if (EnablePanel(_Panels.Find(x => x.CompareTag("MapPanel"))))
        {
            onEnabledMapPanelAction?.Invoke();
        }
    }
    public void EnableDestinationPanel()
    {
        if (EnablePanel(_Panels.Find(x => x.CompareTag("DestinationPanel"))))
        {
            onEnabledDestinationPanelAction?.Invoke();
        }
    }
    public void EnableVendorPanel()
    {
        if(EnablePanel(_Panels.Find(x => x.CompareTag("VendorPanel"))))
        {
            onEnabledVendorPanelAction?.Invoke();
        }
    }
    private bool EnablePanel(GameObject panel)
    {
        if (panel.activeInHierarchy == false)
        {
            panel.SetActive(true);
            DisableOtherPanels(panel);
            return true;
        }
        return false;
    }
    private void DisableOtherPanels(GameObject enabledPanel) 
    {
        var tempPanels = new List<GameObject>(_Panels);
        tempPanels.Remove(enabledPanel);
        foreach(var tempPanel in  tempPanels)
        {
            tempPanel.SetActive(false);
        }
    }
    public void OnCityExit(GameObject newDestination)
    {
        if (CaravanMapMovement.Instance.currentDestination != null
            && newDestination != CaravanMapMovement.Instance.currentDestination)
        {
            if (TradeReferences.Instance.ActiveItemGenerator)
            {
                TradeReferences.Instance.ActiveItemGenerator.OnCityExit();
            }
        }
    }
}
