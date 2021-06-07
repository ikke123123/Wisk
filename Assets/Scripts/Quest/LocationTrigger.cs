using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LocationTrigger
{
    public Vector3 position = Vector3.zero;
    public Target target = Target.Quest;
    public string id = null;
    public GameObject prefab = null;
    public Status targetStatus = Status.Completed;

    private GameObject spawnedObject = null;

    public void Initiate()
    {
        spawnedObject = GameObject.Instantiate(prefab, position, Quaternion.identity);
        spawnedObject.GetComponent<LocationPinger>().locationTrigger = this;
    }

    public void Trigger()
    {
        switch ((int)target)
        {
            case 0: //Quest
                QuestManager.SetQuestStatus(id, targetStatus);
                break;
            case 1: //Quest Element
                QuestManager.SetQuestElementStatus(id, targetStatus);
                break;
            case 2: //Sub Quest Element
                //ADD THIS
                break;
        }
    }

    public enum Target { Quest, QuestElement, SubQuestElement };
}
