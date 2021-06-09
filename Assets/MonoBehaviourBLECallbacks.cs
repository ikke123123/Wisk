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
            monoBehaviourBLECallback.OnConnected();
    }

    public static void Disconnected(int reason)
    {
        foreach (MonoBehaviourBLECallbacks monoBehaviourBLECallback in callbacks)
            monoBehaviourBLECallback.OnDisconnected(reason);
    }

    private void OnDestroy() => callbacks.Remove(this);

    protected virtual void OnConnected()
    {

    }

    protected virtual void OnDisconnected(int reason)
    {

    }
}
