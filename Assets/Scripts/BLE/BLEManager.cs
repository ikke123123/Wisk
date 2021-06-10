using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;

public class BLEManager : MonoBehaviourSingleton<BLEManager>
{
    #region Bluetooth

    private BluetoothManager FManager = null;
    private GattClient FClient = null;
    [SerializeField] private List<long> FDevices = null; // List used during discovering.
    private bool isConnected = false;
    private bool readyToDiscover = false;
    private IntPtr radio = IntPtr.Zero;
    private const int discoveryTimeOut = 30;

    #region Discovery Event Handlers

    private void FManager_OnDiscoveringStarted(object sender, IntPtr Radio)
    {
        Debug.Log("Discovering has been started");
        FDevices = new List<long>();
    }

    private void FManager_OnDeviceFound(object sender, IntPtr Radio, long Address)
    {
        Debug.Log("Device found: " + Address.ToString("X12"));
        // Add device into found devices list.
        FDevices.Add(Address);
        string remoteName;
        FManager.GetRemoteName(Radio, Address, out remoteName);
        //Connect if it is the targeted device.
        if (remoteName == targetDeviceName)
            FClient.Connect(Radio, Address);
    }

    private void FManager_OnDiscoveringCompleted(object sender, IntPtr Radio, int Error)
    {
        Debug.Log("Discovering completed with result: 0x" + Error.ToString("X8"));

        if (Error == BluetoothErrors.WCL_E_SUCCESS)
        {
            if (FDevices.Count == 0)
                Debug.Log("No one Bluetooth LE device was found");
            else
            {
                Debug.Log("Found " + FDevices.Count.ToString() + " devices.");
            }
        }
        // Do not forget to clear found devices list.
        if (FDevices != null) FDevices.Clear();
        FDevices = null;
        readyToDiscover = true;
    }

    #endregion

    #region Common Bluetooth Event Handlers 

    private void FManager_BeforeClose(object sender, EventArgs e)
    {
        Debug.Log("Bluetooth Manager has been closed");
    }

    private void FManager_AfterOpen(object sender, EventArgs e)
    {
        Debug.Log("Bluetooth manager has been opened");

        // Look for working radio.
        if (FManager.Count == 0)
            Debug.Log("No Bluetooth Radios were found");
        else
        {
            for (int i = 0; i < FManager.Count; i++)
            {
                if (FManager.IsRadioAvailable(FManager[i]))
                {
                    radio = FManager[i];
                    break;
                }
            }

            if (radio == IntPtr.Zero)
                Debug.Log("No available Bluetooth Radio was found");
            else
            {
                readyToDiscover = true;
            }
        }
    }

    #endregion

    #region GATT client event handler
    private void FClient_OnConnect(object sender, int Error)
    {
        isConnected = true;
        MonoBehaviourBLECallbacks.Connected();
        if (Error != BluetoothErrors.WCL_E_SUCCESS)
            Debug.Log("Connect error: 0x" + Error.ToString("X8"));
        else
        {
            Debug.Log("Connected");

            Debug.Log("Read services");
            GattServices Services;
            int Res = FClient.GetServices(out Services);
            if (Res != BluetoothErrors.WCL_E_SUCCESS)
                Debug.Log("Failed to read services: 0x" + Res.ToString("X8"));
            else
            {
                if (Services.Count == 0)
                    Debug.Log("No services were found");
                else
                {
                    for (byte s = 0; s < Services.Count; s++)
                    {
                        //if (Services.Services[s].Uuid.IsShortUuid)
                        //    Debug.Log("Service: " + Services.Services[s].Uuid.ShortUuid.ToString("X4"));
                        //else
                        //    Debug.Log("Service: " + Services.Services[s].Uuid.LongUuid.ToString());

                        GattCharacteristics Chars;
                        Res = FClient.GetCharacteristics(Services.Services[s], out Chars);
                        if (Res != BluetoothErrors.WCL_E_SUCCESS)
                            Debug.Log("Failed to read characteristics: 0x" + Res.ToString("X8"));
                        else
                        {
                            if (Chars.Count == 0)
                                Debug.Log("No characteristics found");
                            else
                            {
                                for (byte c = 0; c < Chars.Count; c++)
                                {
                                    GattCharacteristic gattCharacteristic = Chars.Chars[c];
                                    if (gattCharacteristic.Uuid.IsShortUuid)
                                    {
                                        //Look for characteristics with the same target
                                        foreach (Characteristic characteristic in Characteristic.characteristics.Where(characteristic => characteristic.Address == gattCharacteristic.Uuid.ShortUuid))
                                        {
                                            characteristic.gattCharacteristic = gattCharacteristic;
                                            FClient.Subscribe(gattCharacteristic);
                                            characteristic.Initialize();
                                        }
                                    }
                                    //else
                                        //EXECUTE IF ITS A LONG HANDLE
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void FClient_OnDisconnect(object sender, int Reason)
    {
        isConnected = false;
        MonoBehaviourBLECallbacks.Disconnected(Reason);
        StartDiscovery();
        Debug.Log("Disconnected with reason: 0x" + Reason.ToString("X8"));
    }

    private void FClient_OnChanged(object sender, ushort Handle, byte[] Value)
    {
        try
        {
            string output = "";
            foreach (byte bytes in Value)
                output += bytes.ToString() + "-";
            //Debug.Log("Characteristic " + Handle.ToString("X4") + " has been changed " + output);
            if (Value != null && Value.Length > 0)
            {
                Debug.Log(Value.Length);
                string response = "";

                foreach (Characteristic characteristic in Characteristic.characteristics)
                    if (characteristic.ReceiveData(Handle, Value, out response))
                        break;

                //Debug.Log(response);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
    #endregion

    public bool StartDiscovery()
    {
        if (!readyToDiscover) //Can we discover?
            return false;

        int Res = FManager.Discover(radio, discoveryTimeOut);
        if (Res != BluetoothErrors.WCL_E_SUCCESS) // !WCL_E_SUCCESS
        {
            Debug.Log("Failed to start discovering: 0x" + Res.ToString("X8"));
            return false;
        }
        return true;
    }

    public int SendData(GattCharacteristic gattCharacteristic, byte[] sendData)
    {
        int response = FClient.WriteValue(gattCharacteristic, sendData);
        if (response != BluetoothErrors.WCL_E_SUCCESS)
        {
            string output = "";
            foreach (byte bytes in sendData)
                output += bytes.ToString("X4") + " ";
            Debug.Log(output + "failed to send.");
        }
        return response;
    }

    #endregion

    public string targetDeviceName = "";

    public FMS_CP fms_CP = null;
    public FMS_IBD fms_IBD = null;

    protected override void SingletonDestroyed()
    {
        if (isConnected) FClient.Disconnect();
        // Dispose RfComm Client object. It should also call Disconnect!
        FClient.Dispose();
        // Dispose Bluetooth Manager object, It should also fire BeforeClose
        FManager.Dispose();
    }

    protected override void SingletonStarted()
    {
        #region Bluetooth
        // Create RfComm client and setup its events.
        FClient = new GattClient();
        FClient.OnConnect += FClient_OnConnect;
        FClient.OnDisconnect += FClient_OnDisconnect;
        FClient.OnChanged += FClient_OnChanged;

        // Create Bluetooth Manager and setup its events.
        FManager = new BluetoothManager();
        FManager.AfterOpen += FManager_AfterOpen;
        FManager.BeforeClose += FManager_BeforeClose;
        FManager.OnDiscoveringStarted += FManager_OnDiscoveringStarted;
        FManager.OnDeviceFound += FManager_OnDeviceFound;
        FManager.OnDiscoveringCompleted += FManager_OnDiscoveringCompleted;

        // Try to open Bluetooth Manager.
        int Res = FManager.Open();
        if (Res != BluetoothErrors.WCL_E_SUCCESS) // !WCL_E_SUCCESS
            Debug.Log("Failed to open Bluetooth Manager: 0x" + Res.ToString("X8"));
        #endregion

        fms_CP = new FMS_CP();
        fms_IBD = new FMS_IBD();
    }
}