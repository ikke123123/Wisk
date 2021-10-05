using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationTriggerMonobehaviour : MonoBehaviour
{
    public LocationTriggerData locationTriggerData;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
            Trigger();
    }

    private void Trigger()
    {
        switch ((int)locationTriggerData.target)
        {
            case 0: //Quest
                QuestManager.SetQuestStatus(locationTriggerData.id, locationTriggerData.targetStatus);
                break;
            case 1: //Quest Element
                QuestManager.SetQuestElementStatus(locationTriggerData.id, locationTriggerData.targetStatus);
                break;
        }
        Destroy(gameObject);
    }
}