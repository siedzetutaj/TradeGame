using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class TravelButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea(5, 20)] public string descritpion;
    [PrefabType(typeof(VendorItemGenerator))]public GameObject destinationPrefab;
    public Button button;

    [SerializeField] private RectTransform targetTransform; // Target transform for the caravan

    private TravelReferences _travelReferences;
    private CaravanMapMovement _caravanOnMapMovement;
    private TravelButtonsManager _travelButtonsManager;
    private Image lineImage;




    // Adjustable movement speed
    [SerializeField] private float moveSpeed = 100f;

    [Tooltip("This destinations will be unlcoked")]
    [SerializeField] private List<TravelButton> EnableDestinationButtons = new();

    [Tooltip("From this place player can go to this destinations, with this food price")]
    public SerializableDictionary<TravelButton, float> PossibleDestiantionsToGo = new();

    private void OnEnable()
    {
        button = this.GetComponent<Button>();
        _travelReferences = TravelReferences.Instance;
        _caravanOnMapMovement = CaravanMapMovement.Instance;
        _travelButtonsManager = TravelButtonsManager.Instance;

        if (button.interactable)
        {
            _travelButtonsManager.AddTravelButton(this);
        }
    }
    public void OnTravelButtonPressed()
    {
        if (_travelButtonsManager.CanGoToThisDestination(this))
        {
            if (_travelButtonsManager.CurrentTravelButton == this)
            {
                StartTravelling();
                return;
            }
            DrawLine();
            _travelButtonsManager.IsResourceMenuOpen = true;
            _travelReferences.ResourcesPanel.SetActive(true);
            ResourceManager.Instance.SetUpResourceManager(this,
                _travelButtonsManager.TravelResourceCost(this));
        }
    }

    public void StartTravelling()
    {
        _travelButtonsManager.IsResourceMenuOpen = false;
        _travelReferences.ResourcesPanel.SetActive(false);
        OnPointerExit(null);

        GameLogic.Instance.OnCityExit(destinationPrefab);
        _travelButtonsManager.DisableTravelButtons();
        StartCoroutine(MoveToTarget());
    }

    private IEnumerator MoveToTarget()
    {
        _travelButtonsManager.IsMoving= true;
        // Cache references to the caravan and target positions
        RectTransform caravanTransform = _caravanOnMapMovement.caravanRectTransform;
        Vector3 targetPosition = targetTransform.position; // World position of the target

        // Move the caravan towards the target
        while (Vector3.Distance(caravanTransform.position, targetPosition) > 0.01f)
        {
            caravanTransform.position = Vector3.MoveTowards(
                caravanTransform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
            yield return null; // Wait for the next frame
        }
        _travelButtonsManager.IsMoving = false;
        // Once movement is complete, create the destination UI
        _caravanOnMapMovement.currentDestination = destinationPrefab;
        EnableDestinationButtons.ForEach(travelButton => {
            travelButton.button.interactable = true;
            _travelButtonsManager.AddTravelButton(this);
        });
        _travelButtonsManager.EnableTravelButtons();
        CreateDestinationUI();
    }
    private void CreateDestinationUI()
    {

        // Hide the travel references panel
        GameLogic.Instance.EnableDestinationPanel();
        // Destroy any existing destination UI
        if (_travelReferences.CurrentDestianation != null)
        {
            if (_travelButtonsManager.CurrentTravelButton == this)
            {
                return;
            }
            Destroy(_travelReferences.CurrentDestianation);
            _travelReferences.CurrentDestianation = null;
        }

        // Create a new destination UI
        GameObject destination = Instantiate(destinationPrefab, _travelReferences.DestinationPanel.transform);
        _travelReferences.CurrentDestianation = destination;
        _travelButtonsManager.CurrentTravelButton = this;

    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_travelButtonsManager.IsResourceMenuOpen)
        {
            DisplayDescription(eventData);
            _travelReferences.isPointerOverButton = true;
            if (_travelButtonsManager.CanGoToThisDestination(this) && !_travelButtonsManager)
            {
                DrawLine();
            }
        }
    }
    private void DrawLine()
    {
        
        _travelReferences.lineImage.gameObject.SetActive(true);
        lineImage = _travelReferences.lineImage.GetComponent<Image>();

        SetLineImageToRed();

        RectTransform pointA = targetTransform;
        RectTransform pointB = _caravanOnMapMovement.caravanRectTransform;
        
        Vector2 direction = pointB.position - pointA.position;
        float distance = direction.magnitude;

        // Rotate the line
        _travelReferences.lineImage.rotation = Quaternion.FromToRotation(Vector3.right, direction);

        // Set the size and position
        _travelReferences.lineImage.sizeDelta = new Vector2(distance, _travelReferences.lineImage.sizeDelta.y);
        _travelReferences.lineImage.position = (pointA.position + pointB.position) / 2;
    }
    private void DisplayDescription(PointerEventData eventData)
    {
        _travelReferences.MovingDescriptionTransform.gameObject.SetActive(true);

        // Get the mouse position in screen space
        Vector2 mousePos = eventData.position;

        // Get references to the description transform and text
        RectTransform movingDescriptionTransform = _travelReferences.MovingDescriptionTransform;
        TextMeshProUGUI descriptionTMP = _travelReferences.MovingDescriptionText;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)movingDescriptionTransform.parent,
            mousePos,
            null,
            out Vector2 localPosition);

        // Update the position of the description background (aligning bottom-left corner)
        movingDescriptionTransform.anchoredPosition = localPosition;

        // Update the description text
        descriptionTMP.text = descritpion;

        // Mark the pointer as hovering over the button
    }
    public void OnPointerExit(PointerEventData eventData)
    {
            _travelReferences.MovingDescriptionTransform.gameObject.SetActive(false);
            _travelReferences.isPointerOverButton = false;
        
        if (!_travelButtonsManager.IsResourceMenuOpen)
        {
            _travelReferences.lineImage.gameObject.SetActive(false);
        }
    }

    public void SetLineImageToGreen()
    {
        lineImage.color = Color.green;
    }   
    public void SetLineImageToRed()
    {
        lineImage.color = Color.red;
    }

}
