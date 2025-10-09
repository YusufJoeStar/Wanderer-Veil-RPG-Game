using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationHistoryTracker : MonoBehaviour
{
    
    private readonly HashSet<LocationSO> locationsVisited = new HashSet<LocationSO>();

   

    public void RecordLocation(LocationSO locationSO)
    {
        if (locationSO != null && !locationsVisited.Contains(locationSO))
            locationsVisited.Add(locationSO);
    }

    public bool HasVisited(LocationSO locationSO)
    {
        return locationSO != null && locationsVisited.Contains(locationSO);
    }
}
