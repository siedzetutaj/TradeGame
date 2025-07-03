using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystem : MonoBehaviourSingleton<InputSystem>
{
    /* To add new input action
     * First add it in editor
     * Next make Action
     * Then Connect Action to input
     * Lastly create void that sends singal
     */
    public PlayerControls playerControls;

    #region Actions

    // Movement
    public Action onMapAction;
    public Action onDestinationAction;
    public Action onVendorAction;
    public Action onRotateLeftAction;
    public Action onRotateRightAction;
    public Action onMirrorAction;

    #endregion
    #region Assembly
    private void Awake()
    {
        playerControls = new PlayerControls();
    }
    private void OnEnable()
    {
        playerControls.Enable();
    }
    #endregion
    #region Connect/Disconnect actions
    private void OnDisable()
    {
        playerControls.PlayerActionMap.MapAction.performed -= OnMapActionPerformed;
        playerControls.PlayerActionMap.DestinationAction.performed -= OnDestinationActionPerformed;
        playerControls.PlayerActionMap.VendorAction.performed -= OnVendorActionPerformed;

        playerControls.PlayerActionMap.RotateLeftAction.performed -= OnRotateLeftActionPerformed;
        playerControls.PlayerActionMap.RotateRightAction.performed -= OnRotateRightActionPerformed;
        playerControls.PlayerActionMap.MirrorAction.performed -= OnMirrortActionPerformed;

        playerControls.Disable();
    }
    private void Start()
    {
        playerControls.PlayerActionMap.MapAction.performed += OnMapActionPerformed;
        playerControls.PlayerActionMap.DestinationAction.performed += OnDestinationActionPerformed;
        playerControls.PlayerActionMap.VendorAction.performed += OnVendorActionPerformed;

        playerControls.PlayerActionMap.RotateLeftAction.performed += OnRotateLeftActionPerformed;
        playerControls.PlayerActionMap.RotateRightAction.performed += OnRotateRightActionPerformed;
        playerControls.PlayerActionMap.MirrorAction.performed += OnMirrortActionPerformed;

    }

    #endregion
    private void OnMapActionPerformed(InputAction.CallbackContext context)
    {
        onMapAction?.Invoke();
    }
    private void OnDestinationActionPerformed(InputAction.CallbackContext context)
    {
        onDestinationAction?.Invoke();
    }
    private void OnVendorActionPerformed(InputAction.CallbackContext context)
    {
        onVendorAction?.Invoke();
    }  
    private void OnRotateLeftActionPerformed(InputAction.CallbackContext context)
    {
        onRotateLeftAction?.Invoke();
    }   
    private void OnRotateRightActionPerformed(InputAction.CallbackContext context)
    {
        onRotateRightAction?.Invoke();
    }
    private void OnMirrortActionPerformed(InputAction.CallbackContext context)
    {
        onMirrorAction?.Invoke();
    }
}
//public interface IInputProvider
//{
//    PlayerInput GetInput();
//}
//[System.Serializable]
//public struct PlayerInput
//{
//    public static PlayerInput None => new PlayerInput();
//}