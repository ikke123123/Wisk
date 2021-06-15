
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Location Trigger Prefab", menuName = "Location Trigger Prefab")]
public class LocationTrigger : ScriptableObject
{
    public GameObject prefab = null;

    public GameObject Spawn(LocationTriggerData locationTriggerData, bool setRoute = false)
    {
        GameObject locationTrigger = Instantiate(prefab, locationTriggerData.position, Quaternion.identity);
        locationTrigger.GetComponent<LocationTriggerMonobehaviour>().locationTriggerData = locationTriggerData;
        if (setRoute)
            CyclistTrackFollower.SetRoute(locationTriggerData.position);
        return locationTrigger;
    }
}
