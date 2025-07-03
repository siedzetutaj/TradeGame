using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManagerRegistry : MonoBehaviourSingleton<GridManagerRegistry>
{
    public Dictionary<GridBounds, GridManager> gridsBounds = new();
    public GridManager GetNearestGrid(Vector2 itemPosition)
    {
        GridManager nearestGrid = null;
        float closestDistance = float.MaxValue;

        foreach (var gridBounds in gridsBounds.Keys)
        {
            // Get the closest point inside the current grid's bounds
            Vector2 closestPoint = gridBounds.ClampPositionToBounds(itemPosition);

            // Calculate distance between the item and the closest point
            float distance = Vector2.Distance(itemPosition, closestPoint);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearestGrid = gridsBounds[gridBounds];
            }
        }

        return nearestGrid;
    }

}
