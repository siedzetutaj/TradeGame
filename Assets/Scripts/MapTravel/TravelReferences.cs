using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TravelReferences : MonoBehaviourSingleton<TravelReferences>
{
    public GameObject DestinationPanel;
    public GameObject CurrentDestianation;
    public RectTransform MovingDescriptionTransform;
    public TextMeshProUGUI MovingDescriptionText;
    public bool isPointerOverButton;
   
    public RectTransform lineImage;
    public GameObject ResourcesPanel;
    public void Update()
    {
        if (isPointerOverButton)
        {
            Vector2 mousePos = (Vector2)Input.mousePosition + new Vector2(1, 1);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)MovingDescriptionTransform.parent,
                mousePos,
                null,
                out Vector2 localPosition);

            // Update the position of the background image (aligning bottom-left corner)
            MovingDescriptionTransform.anchoredPosition = localPosition;
        }
    }
}
