using System;
using UnityEngine;

[Serializable]
public struct LocationTriggerData
{
    public Vector3 position;
    public Target target;
    public string id;
    public Status targetStatus;
}