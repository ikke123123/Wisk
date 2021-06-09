using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Characteristic
{
    public static List<Characteristic> characteristics;

    public static Characteristic GetCharacteristic(ushort address)
    {
        return characteristics.Where(characteristic => characteristic.Address == address).First();
    }

    public Characteristic()
    {
        if (characteristics == null)
            characteristics = new List<Characteristic>();
        if (characteristics.Contains(this))
            return;
        characteristics.Add(this);
        callbackTargets = new List<EventHandler>();
    }

    public ushort Address { get; protected set; }
    public GattCharacteristic gattCharacteristic;
    public List<EventHandler> callbackTargets;

    public bool ReceiveData(ushort checkAgainst, byte[] receivedData, out string response)
    {
        response = "";
        if (checkAgainst != gattCharacteristic.Handle)
            return false;
        response += Address.ToString("X4");
        ReceivedData(receivedData, ref response);
        foreach (EventHandler eventHandler in callbackTargets)
            eventHandler.Invoke(this, new EventArgs());
        return true;
    }

    public void Initialize()
    {
        OnInitialization();
    }

    protected virtual void ReceivedData(byte[] receivedData, ref string response)
    {

    }

    protected virtual void OnInitialization()
    {

    }

    /// <summary>
    /// Calls the target when a value has changed.
    /// </summary>
    /// <param name="target"></param>
    public void AddCallbackTarget(EventHandler target)
    {
        callbackTargets.Add(target);
    }
}
