using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CaravanMapMovement : MonoBehaviourSingleton<CaravanMapMovement>
{
    public RectTransform caravanRectTransform;
    public GameObject currentDestination;
    [SerializeField] private List<GridItem> _gridItems;

}
