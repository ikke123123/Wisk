using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoBehaviourBLECallbacks : MonoBehaviour
{
    protected static List<MonoBehaviourBLECallbacks> callbacks = null;

    public MonoBehaviourBLECallbacks()
    {
        if (callbacks == null)
            callbacks = new List<MonoBehaviourBLECallbacks>();
        if (callbacks.Contains(this))
            return;
        callbacks.Add(this);
    }

    public static void Connected()
    {
        foreach (MonoBehaviourBLECallbacks monoBehaviourBLECallback in callbacks)
        {
            monoBehaviourBLECallback.OnConnected();
        }
    }

    private void OnDestroy() => callbacks.Remove(this);

    protected virtual void OnConnected()
    {

    }
}
