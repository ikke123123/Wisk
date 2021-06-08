using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BLEManager : MonoBehaviourSingleton<BLEManager>
{
    #region Bluetooth

    private BluetoothManager FManager = null;
    private GattClient FClient = null;
    [SerializeField] private List<long> FDevices = null; // List used during discovering.
    private bool isConnected;

    #region Discovering Event Handlers

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
        if (remoteName == targetDeviceName)
        {
            FClient.Connect(Radio, Address);
        }
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
        FDevices.Clear();
        FDevices = null;
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
            IntPtr Radio = IntPtr.Zero;
            for (int i = 0; i < FManager.Count; i++)
            {
                if (FManager.IsRadioAvailable(FManager[i]))
                {
                    Radio = FManager[i];
                    break;
                }
            }

            if (Radio == IntPtr.Zero)
                Debug.Log("No available Bluetooth Radio was found");
            else
            {
                // Here we can start discovering for device.
                int Res = FManager.Discover(Radio, 10); // 10 seconds timeout.
                if (Res != BluetoothErrors.WCL_E_SUCCESS) // !WCL_E_SUCCESS
                    Debug.Log("Failed to start discovering: 0x" + Res.ToString("X8"));
            }
        }
    }

    #endregion

    #region GATT client event handler
    private void FClient_OnConnect(object sender, int Error)
    {
        isConnected = true;
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
                        if (Services.Services[s].Uuid.IsShortUuid)
                            Debug.Log("Service: " + Services.Services[s].Uuid.ShortUuid.ToString("X4"));
                        else
                            Debug.Log("Service: " + Services.Services[s].Uuid.LongUuid.ToString());

                        Debug.Log("Read characteristics");
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
                                for (Byte c = 0; c < Chars.Count; c++)
                                {
                                    if (Chars.Chars[c].Uuid.IsShortUuid)
                                    {
                                        Debug.Log("Characteristic: " + Chars.Chars[c].Uuid.ShortUuid.ToString("X4"));
                                        //if (Chars.Chars[c].Uuid.ShortUuid == fitnessMachineControlPoint.address)
                                        //{
                                        //    fitnessMachineControlPoint.gattCharacteristic = Chars.Chars[c];

                                        //    FClient.Subscribe(Chars.Chars[c]);

                                        //    fitnessMachineControlPoint.InitializeData();
                                        //}
                                        ////else if (Chars.Chars[c].Uuid.ShortUuid == 0x2ADA)
                                        ////{
                                        ////    FClient.Subscribe(Chars.Chars[c]);
                                        ////}
                                        //if (Chars.Chars[c].Uuid.ShortUuid == fitnessMachineIndoorBikeData.address)
                                        //{
                                        //    fitnessMachineIndoorBikeData.correspondingCharacteristic = Chars.Chars[c];
                                        //    FClient.Subscribe(Chars.Chars[c]);
                                        //}
                                    }
                                    else
                                        Debug.Log("Characteristic: " + Chars.Chars[c].Uuid.LongUuid.ToString());
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void FClient_OnDisconnect(System.Object sender, Int32 Reason)
    {
        isConnected = false;
        Debug.Log("Disconnected with reason: 0x" + Reason.ToString("X8"));
    }

    private void FClient_OnChanged(System.Object sender, UInt16 Handle, Byte[] Value)
    {
        try
        {
            //Debug.Log("Characteristic " + Handle.ToString("X4") + " has been changed");
            if (Value != null && Value.Length > 0)
            {
                //if (Handle == fitnessMachineIndoorBikeData.correspondingCharacteristic.Handle)
                //{
                //    FMS_IBD.Output[] outputs = fitnessMachineIndoorBikeData.UpdateData(Value, true);
                //    Debug.Log(outputs.Length);
                //    foreach (FMS_IBD.Output outputting in outputs)
                //    {
                //        textTarget[(int)(outputting.flag)].text = string.Format("{0}: {1}", outputting.flag, outputting.value);
                //    }
                //}
                //if (Handle == fitnessMachineControlPoint.gattCharacteristic.Handle)
                //{
                //    string output;
                //    if (fitnessMachineControlPoint.InterpretResponseCode(Value, out output))
                //    {
                //        Debug.Log(output);
                //    }
                //}

                //string output = "";
                //for (int i = 0; i < Value.Length; i++)
                //{
                //    output += i.ToString() + ": " + ConvertToUInt(Value[i]) + " ";
                //}
                //Debug.Log("Length: " + Value.Length);

                //Debug.Log(output);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
    #endregion

    #endregion

    public string targetDeviceName = "";

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

        new FMS_CP(FClient).SendSimulationParameters(4.5f, 12.3f, 0.003f, 0.64f, 0.4f, 1.29f);
    }
}

public class GattClientDemo //: MonoBehaviour
{
    private BluetoothManager FManager = null;
    private GattClient FClient = null;
    [SerializeField] private List<long> FDevices = null; // List used during discovering.
    private bool isConnected;
    private GattCharacteristic characteristic;
    [SerializeField] private bool sendNewResistance = false;
    [SerializeField] private float resistence = 0;
    private FMS_IBD fitnessMachineIndoorBikeData = null;

    private Dictionary<int, TextMeshProUGUI> textTarget = new Dictionary<int, TextMeshProUGUI>();
    [SerializeField] private TextMeshProUGUI instantaneousSpeed, averageSpeed, instantaneousCadence, averageCadence, totalDistance, resistanceLevel, instantaneousPower, averagePower, expendedEnergy, heartRate, metabolicEquivalent, elapsedTime, remainingTime = null;
    [SerializeField] private bool resetData = false;
    [SerializeField] private bool sendSimulationParameters = false;
    [SerializeField, Tooltip("In meters per second, with a resolution of 0.001, from -32.768 m/s to 32.787 m/s"), Range(-32.768f, 32.787f)] private float windSpeed = 0;
    [SerializeField, Tooltip("In percentage, with a resolution of 0.01, from -327.68 to 327.67"), Range(-327.68f, 327.67f)] private float grade = 0;
    [SerializeField, Tooltip("Unitless, with a resolution of 0.0001, from 0 to 0.0255"), Range(0, 0.0255f)] private float coefficientOfRollingResistance = 0;
    [SerializeField, Tooltip("In kilogram per meter, with a resolution of 0.01, from 0 to 2.55"), Range(0, 2.55f)] private float coefficientOfWindResistance = 0;

    [SerializeField] private bool sendWheelCircumference = false;
    [SerializeField, Tooltip("In mm, with a resolution of 0.1 mm, from 0 to 6553.5 mm")] private float wheelCircumference = 0;

    [SerializeField] private string status = "";
    [SerializeField] private bool startOrResume = false;
    [SerializeField] private bool stop = false;
    [SerializeField] private bool pause = false;

    private FMS_CP fitnessMachineControlPoint = null;

    #region Discovering event handlers
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
        if (remoteName == "DIRETO X")
        {
            FClient.Connect(Radio, Address);
        }
    }

    private void FManager_OnDiscoveringCompleted(System.Object sender, IntPtr Radio, Int32 Error)
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
        FDevices.Clear();
        FDevices = null;
    }
    #endregion

    #region Common Bluetooth Manager event handlers
    private void FManager_BeforeClose(System.Object sender, EventArgs e)
    {
        Debug.Log("Bluetooth Manager has been closed");
    }

    private void FManager_AfterOpen(System.Object sender, EventArgs e)
    {
        Debug.Log("Bluetooth manager has been opened");

        // Look for working radio.
        if (FManager.Count == 0)
            Debug.Log("No Bluetooth Radios were found");
        else
        {
            IntPtr Radio = IntPtr.Zero;
            for (Int32 i = 0; i < FManager.Count; i++)
            {
                if (FManager.IsRadioAvailable(FManager[i]))
                {
                    Radio = FManager[i];
                    break;
                }
            }

            if (Radio == IntPtr.Zero)
                Debug.Log("No available Bluetooth Radio was found");
            else
            {
                // Here we can start discovering for device.
                Int32 Res = FManager.Discover(Radio, 10); // 10 seconds timeout.
                if (Res != BluetoothErrors.WCL_E_SUCCESS) // !WCL_E_SUCCESS
                    Debug.Log("Failed to start discovering: 0x" + Res.ToString("X8"));
            }
        }
    }
    #endregion

    #region GATT client event handler
    private void FClient_OnConnect(System.Object sender, Int32 Error)
    {
        status = "connected";
        isConnected = true;
        if (Error != BluetoothErrors.WCL_E_SUCCESS)
            Debug.Log("Connect error: 0x" + Error.ToString("X8"));
        else
        {
            Debug.Log("Connected");

            Debug.Log("Read services");
            GattServices Services;
            Int32 Res = FClient.GetServices(out Services);
            if (Res != BluetoothErrors.WCL_E_SUCCESS)
                Debug.Log("Failed to read services: 0x" + Res.ToString("X8"));
            else
            {
                if (Services.Count == 0)
                    Debug.Log("No services were found");
                else
                {
                    for (Byte s = 0; s < Services.Count; s++)
                    {
                        if (Services.Services[s].Uuid.IsShortUuid)
                            Debug.Log("Service: " + Services.Services[s].Uuid.ShortUuid.ToString("X4"));
                        else
                            Debug.Log("Service: " + Services.Services[s].Uuid.LongUuid.ToString());

                        Debug.Log("Read characteristics");
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
                                for (Byte c = 0; c < Chars.Count; c++)
                                {
                                    if (Chars.Chars[c].Uuid.IsShortUuid)
                                    {
                                        Debug.Log("Characteristic: " + Chars.Chars[c].Uuid.ShortUuid.ToString("X4"));
                                        if (Chars.Chars[c].Uuid.ShortUuid == fitnessMachineControlPoint.address)
                                        {
                                            fitnessMachineControlPoint.gattCharacteristic = Chars.Chars[c];

                                            FClient.Subscribe(Chars.Chars[c]);

                                            fitnessMachineControlPoint.InitializeData();
                                        }
                                        //else if (Chars.Chars[c].Uuid.ShortUuid == 0x2ADA)
                                        //{
                                        //    FClient.Subscribe(Chars.Chars[c]);
                                        //}
                                        if (Chars.Chars[c].Uuid.ShortUuid == fitnessMachineIndoorBikeData.address)
                                        {
                                            fitnessMachineIndoorBikeData.correspondingCharacteristic = Chars.Chars[c];
                                            FClient.Subscribe(Chars.Chars[c]);
                                        }
                                    }
                                    else
                                        Debug.Log("Characteristic: " + Chars.Chars[c].Uuid.LongUuid.ToString());
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void FClient_OnDisconnect(System.Object sender, Int32 Reason)
    {
        isConnected = false;
        Debug.Log("Disconnected with reason: 0x" + Reason.ToString("X8"));
    }

    private void FClient_OnChanged(System.Object sender, UInt16 Handle, Byte[] Value)
    {
        try
        {
            //Debug.Log("Characteristic " + Handle.ToString("X4") + " has been changed");
            if (Value != null && Value.Length > 0)
            {
                if (Handle == fitnessMachineIndoorBikeData.correspondingCharacteristic.Handle)
                {
                    FMS_IBD.Output[] outputs = fitnessMachineIndoorBikeData.UpdateData(Value, true);
                    Debug.Log(outputs.Length);
                    foreach (FMS_IBD.Output outputting in outputs)
                    {
                        textTarget[(int)(outputting.flag)].text = string.Format("{0}: {1}", outputting.flag, outputting.value);
                    }
                }
                if (Handle == fitnessMachineControlPoint.gattCharacteristic.Handle)
                {
                    string output;
                    if (fitnessMachineControlPoint.InterpretResponseCode(Value, out output))
                    {
                        Debug.Log(output);
                    }
                }

                //string output = "";
                //for (int i = 0; i < Value.Length; i++)
                //{
                //    output += i.ToString() + ": " + ConvertToUInt(Value[i]) + " ";
                //}
                //Debug.Log("Length: " + Value.Length);

                //Debug.Log(output);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
    #endregion

    private void OnDestroy()
    {
        if (isConnected) FClient.Disconnect();
        // Dispose RfComm Client object. It should also call Disconnect!
        FClient.Dispose();
        // Dispose Bluetooth Manager object, It should also fire BeforeClose
        FManager.Dispose();
    }

    void Start()
    {
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
        Int32 Res = FManager.Open();
        if (Res != BluetoothErrors.WCL_E_SUCCESS) // !WCL_E_SUCCESS
            Debug.Log("Failed to open Bluetooth Manager: 0x" + Res.ToString("X8"));
        fitnessMachineIndoorBikeData = new FMS_IBD();
        fitnessMachineControlPoint = new FMS_CP(FClient);

        //Fill in the dictionary:
        textTarget.Add(0, instantaneousSpeed);
        textTarget.Add(1, averageSpeed);
        textTarget.Add(2, instantaneousCadence);
        textTarget.Add(3, averageCadence);
        textTarget.Add(4, totalDistance);
        textTarget.Add(5, resistanceLevel);
        textTarget.Add(6, instantaneousPower);
        textTarget.Add(7, instantaneousSpeed);
        textTarget.Add(8, averagePower);
        textTarget.Add(9, expendedEnergy);
        textTarget.Add(10, heartRate);
        textTarget.Add(11, metabolicEquivalent);
        textTarget.Add(12, elapsedTime);
        textTarget.Add(13, remainingTime);

        status = "searching";
    }

    void Update()
    {
        if (sendNewResistance)
        {
            fitnessMachineControlPoint.SendResistanceValue(BitConverter.GetBytes(resistence)[0]);
            sendNewResistance = false;
        }

        if (resetData)
        {
            fitnessMachineControlPoint.ResetData();
            resetData = false;
        }

        if (sendSimulationParameters)
        {
            //SendSimulationParameters();
            sendSimulationParameters = false;
        }

        if (sendWheelCircumference)
        {
            ushort _wheelCircumference = Convert.ToUInt16(Mathf.RoundToInt(wheelCircumference * 10)); //To convert the value from millimeters to complete numbers.
            fitnessMachineControlPoint.SetWheelCircumference(_wheelCircumference);
            sendWheelCircumference = false;
        }

        if (startOrResume)
        {
            status = "running";
            fitnessMachineControlPoint.StartOrResume();
            startOrResume = false;
        }

        if (pause || stop)
        {
            status = stop ? "stopped" : "paused";
            fitnessMachineControlPoint.StartOrStop(stop);
            pause = stop = false;
        }
    }

    private uint ConvertToUInt(byte input)
    {
        byte[] bytes = new byte[4];
        for (int i = 0; i < bytes.Length; i++)
        {
            if (i == 0)
            {
                bytes[i] = input;
            }
            else bytes[i] = 0;
        }
        return BitConverter.ToUInt32(bytes, 0);
    }


}

public class FMS_IBD
{
    //UInt12 for flags
    //UInt16 for Instantaneous Speed
    //UInt16 for Average Speed
    //UInt

    public FMS_IBD()
    {
        modifiers.Add(Flags.InstantaneousSpeed, 0.1f);
        modifiers.Add(Flags.AverageSpeed, 0.1f);
        modifiers.Add(Flags.InstantaneousCadence, 0.5f);
        modifiers.Add(Flags.AverageCadence, 0.5f);
        modifiers.Add(Flags.TotalDistance, 0.1f);
        modifiers.Add(Flags.ResistenceLevel, 0.1f);
        modifiers.Add(Flags.InstantaneousPower, 1);
        modifiers.Add(Flags.AveragePower, 1);
        modifiers.Add(Flags.ExpendedEnergy, 1);
        modifiers.Add(Flags.HeartRate, 1);
        modifiers.Add(Flags.MetabolicEquivalent, 0.1f);
        modifiers.Add(Flags.ElapsedTime, 1);
        modifiers.Add(Flags.RemainingTime, 1);

        //infoSize.Add(Flags.InstantaneousSpeed, 2);
        //infoSize.Add(Flags.AverageSpeed, 1);
        //infoSize.Add(Flags.InstantaneousCadence, 1);
        //infoSize.Add(Flags.AverageCadence, 1);
        //infoSize.Add(Flags.TotalDistance, 2);
        //infoSize.Add(Flags.ResistenceLevel, 1);
        //infoSize.Add(Flags.InstantaneousPower, 1);
        //infoSize.Add(Flags.AveragePower, 1);
        //infoSize.Add(Flags.ExpendedEnergy, 1);
        //infoSize.Add(Flags.HeartRate, 1);
        //infoSize.Add(Flags.MetabolicEquivalent, 1);
        //infoSize.Add(Flags.ElapsedTime, 2);
        //infoSize.Add(Flags.RemainingTime, 1);
    }

    public Output[] UpdateData(byte[] input, bool resetFlags = false)
    {
        if (resetFlags || fieldOrder == null)
        {
            SetFlags(input);
        }

        List<Output> output = new List<Output>();

        //Starts at 2 to account for the flags. Increments by two to account for the size of to int.
        for (int i = 0; i < fieldOrder.Length; i++)
        {
            try
            {
                output.Add(new Output(fieldOrder[i], BitConverter.ToUInt16(input, 2 * i + 2) * modifiers[fieldOrder[i]]));
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        lastOutput = output.ToArray();

        return lastOutput;
    }

    public Output[] lastOutput = null;

    public GattCharacteristic correspondingCharacteristic;

    public ushort address = 0x2AD2;

    public Dictionary<Flags, float> modifiers = new Dictionary<Flags, float>();

    public struct Output
    {
        public Output(Flags _flag, float _value)
        {
            flag = _flag;
            value = _value;
        }

        public Flags flag;
        public float value;
    }

    public bool[] flags = null;

    public enum Flags { InstantaneousSpeed, AverageSpeed, InstantaneousCadence, AverageCadence, TotalDistance, ResistenceLevel, InstantaneousPower, AveragePower, ExpendedEnergy, HeartRate, MetabolicEquivalent, ElapsedTime, RemainingTime };

    //public Dictionary<Flags, int> infoSize = new Dictionary<Flags, int>();

    public Flags[] fieldOrder = null;

    private void SetFlags(byte[] bytes)
    {
        //Length of the number of flags.
        flags = new bool[13];

        //Length of the two bytes that contain the flags.
        for (int i = 0; i < 2; i++)
        {
            //Number of bits in a byte.
            for (int j = 0; j < 8; j++)
            {
                if (!(j + i * 7 < flags.Length)) break;
                //Set the corresponding flags to the true/false of the bit.
                flags[j + i * 7] = GetBit(bytes[i], j);
            }
        }

        //List of the order of the fields.
        List<Flags> _fieldOrder = new List<Flags>();
        for (int i = 0; i < flags.Length; i++)
        {
            //If the bool is true the field is present. Two exceptions are: Instantaneous Speed field (0) & Instantaneous Cadence (2).
            if ((i == 0 || i == 2) || flags[i])
            {
                _fieldOrder.Add((Flags)i);
            }
            Debug.LogFormat("{0}: {1}", (Flags)i, flags[i]);
        }
        fieldOrder = _fieldOrder.ToArray();
    }

    private static bool GetBit(byte input, int index) => (input & (1 << index - 1)) != 0;
}

public class FMS_CP
{
    //Purpose: Interact with the thing.

    public FMS_CP(GattClient _FClient)
    {
        FClient = _FClient;
    }

    //Commands:
    private const byte requestControl = 0x00;
    private const byte reset = 0x01;
    private const byte setTargetSpeed = 0x02;
    private const byte setTargetInclination = 0x03;
    private const byte setTargetResistenceLevel = 0x04;
    private const byte setTargetPower = 0x05;
    private const byte setTargetHeartRate = 0x06;
    private const byte startOrResume = 0x07;
    private const byte stopOrPause = 0x08;
    private const byte setTargetedExpendedEnergy = 0x09;
    private const byte setTargetedNumberOfSteps = 0x0A;
    private const byte setTargetedNumberOfStrides = 0x0B;
    private const byte setTargetedDistance = 0x0C;
    private const byte setTargetedTrainingTime = 0x0D;
    private const byte setTargetedTimeInTwoHeartRateZones = 0x0E;
    private const byte setTargetedTimeInThreeHeartRateZones = 0x0F;
    private const byte setTargetedTimeInFiveHeartRateZones = 0x10;
    private const byte setIndoorBikeSimulationParameters = 0x11;
    private const byte setWheelCircumference = 0x12;
    private const byte spinDownControl = 0x13;
    private const byte setTargetedCadence = 0x14;

    //Response code:
    private const byte responseCode = 0x80;

    //Success code:
    private const byte succes = 0x01;
    private const byte opCodeNotSupported = 0x02;
    private const byte invalidParameter = 0x03;
    private const byte operationFailed = 0x04;
    private const byte controlNotPermitted = 0x05;

    //Stop or Pause code:
    private const byte stop = 0x01;
    private const byte pause = 0x02;

    //Spin Down Control Procedure
    private const byte start = 0x01;
    private const byte ignore = 0x02;

    public ushort address = 0x2AD9;

    private GattClient FClient;

    public GattCharacteristic gattCharacteristic;

    public bool InterpretResponseCode(byte[] input, out string message)
    {
        if (input[0] != responseCode)
        {
            message = "The input is not a response code.";
            return false;
        }

        string output = "";

        switch (input[1])
        {
            case requestControl:
                output += "Request Control ";
                break;
            case reset:
                output += "Reset ";
                break;
            case setTargetSpeed:
                //UINT16 with speed in km/h with a res of 0.01 km/h.
                output += "Setting Target Speed to " + (BitConverter.ToUInt16(input, 3) * 0.01f).ToString() + " km/h ";
                break;
            case setTargetInclination:
                output += "Setting Target Inclination to " + (BitConverter.ToInt16(input, 3) * 0.1f).ToString() + "% ";
                break;
            case setTargetResistenceLevel:
                output += "Setting Target Resistance Level " + (input[3] * 0.1f).ToString() + " ";
                break;
            case setTargetPower:
                output += "Setting Target Power " + BitConverter.ToInt16(input, 3).ToString() + " W ";
                break;
            case setTargetHeartRate:
                output += "Setting Target Heart Rate to " + input[3].ToString() + " BPM ";
                break;
            case startOrResume:
                output += "Start or Resume ";
                break;
            case stopOrPause:
                output += (input[3] == stop ? "Stop " : "Pause ");
                break;
            case setTargetedExpendedEnergy:
                output += "Setting Targeted Expended Energy to " + BitConverter.ToUInt16(input, 3).ToString() + " Calories ";
                break;
            case setTargetedNumberOfSteps:
                output += "Setting Targeted Number of Steps to " + BitConverter.ToUInt16(input, 3).ToString() + " steps ";
                break;
            case setTargetedNumberOfStrides:
                output += "Setting Targeted Number of Strides to " + BitConverter.ToUInt16(input, 3).ToString() + " strides ";
                break;
            case setTargetedDistance:
                output += "Setting Targeted Distance to " + BitConverter.ToUInt32(input, 3).ToString() + " meters ";
                break;
            case setTargetedTrainingTime:
                output += "Setting Targeted Training Time to " + new DateTime().AddSeconds(BitConverter.ToUInt16(input, 3)).ToString("HH:mm:ss") + " ";
                break;
            case setTargetedTimeInTwoHeartRateZones:
                output += "Setting Targeted Time in Two Heart Rate Zones to " + new DateTime().AddSeconds(BitConverter.ToUInt16(input, 3)).ToString("HH:mm:ss") + " in Fat Burn Zone, and " + new DateTime().AddSeconds(BitConverter.ToUInt16(input, 5)).ToString("HH:mm:ss") + " in Fitness Zone ";
                break;
            case setTargetedTimeInThreeHeartRateZones:
                output += "Setting Targeted Time in Three Heart Rate Zones to " + new DateTime().AddSeconds(BitConverter.ToUInt16(input, 3)).ToString("HH:mm:ss") + " in Light Zone, " + new DateTime().AddSeconds(BitConverter.ToUInt16(input, 5)).ToString("HH:mm:ss") + " in Moderate Zone, and " + new DateTime().AddSeconds(BitConverter.ToUInt16(input, 3)).ToString("HH:mm:ss") + " in Hard Zone ";
                break;
            case setTargetedTimeInFiveHeartRateZones:
                output += "Setting Targeted Time in Five Heart Rate Zones to " + new DateTime().AddSeconds(BitConverter.ToUInt16(input, 3)).ToString("HH:mm:ss") + " in Very Light Zone, " + new DateTime().AddSeconds(BitConverter.ToUInt16(input, 3)).ToString("HH:mm:ss") + " in Light Zone, " + new DateTime().AddSeconds(BitConverter.ToUInt16(input, 5)).ToString("HH:mm:ss") + " in Moderate Zone, " + new DateTime().AddSeconds(BitConverter.ToUInt16(input, 3)).ToString("HH:mm:ss") + " in Hard Zone, and " + new DateTime().AddSeconds(BitConverter.ToUInt16(input, 3)).ToString("HH:mm:ss") + " in Maximum Zone ";
                break;
            case setIndoorBikeSimulationParameters:
                output += "Setting Indoor Bike Simulation Parameters to " + (BitConverter.ToInt16(input, 3) * 0.001f).ToString() + " m/s Wind Speed, " + (BitConverter.ToInt16(input, 5) * 0.01f).ToString() + "% Grade, " + (input[6] * 0.0001f).ToString() + " Coefficient of Rolling Resistance, and " + (input[7] * 0.01f).ToString() + " kg/m Wind Resistance Coefficient ";
                break;
            case setWheelCircumference:
                output += "Setting Wheel Circumference " /*+ (BitConverter.ToUInt16(input, 3) * 0.1f).ToString() + " mm "*/;
                break;
            case spinDownControl:
                output += "Setting spin Down Control " + (input[3] == start ? "Start " : "Ignore ");
                break;
            case setTargetedCadence:
                output += "Setting Targeted Cadence " + (BitConverter.ToUInt16(input, 3) * 0.5f).ToString() + " per minute ";
                break;
            default:
                output += "Unknown OP code ";
                break;
        }

        switch (input[2])
        {
            case succes:
                output += "was successful.";
                break;
            case opCodeNotSupported:
                output += "was an unsupported OP code";
                break;
            case invalidParameter:
                output += "contained an invalid parameter.";
                break;
            case operationFailed:
                output += "was unsuccessful.";
                break;
            case controlNotPermitted:
                output += "was not permitted.";
                break;
        }
        message = output;
        return true;
    }

    public void StartOrResume()
    {
        SendData(new byte[] { startOrResume });
    }

    public void StartOrStop(bool _stop)
    {
        byte value;
        if (_stop) value = stop;
        else value = pause;
        SendData(new byte[] { stopOrPause, value });
    }

    public void SendResistanceValue(byte sendData) => SendData(new byte[] { setTargetResistenceLevel, sendData });

    public void ResetData() => SendData(new byte[] { reset });

    [SerializeField, Tooltip("In meters per second, with a resolution of 0.001, from -32.768 m/s to 32.787 m/s"), Range(-32.768f, 32.787f)] private float windSpeed = 0;
    [SerializeField, Tooltip("In percentage, with a resolution of 0.01, from -327.68 to 327.67"), Range(-327.68f, 327.67f)] private float grade = 0;
    [SerializeField, Tooltip("Unitless, with a resolution of 0.0001, from 0 to 0.0255"), Range(0, 0.0255f)] private float coefficientOfRollingResistance = 0;
    [SerializeField, Tooltip("In kilogram per meter, with a resolution of 0.01, from 0 to 2.55"), Range(0, 2.55f)] private float coefficientOfWindResistance = 0;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_windspeed">In meters per second, with a resolution of 0.001, from -32.768 m/s to 32.787 m/s</param>
    /// <param name="_grade">In percentage, with a resolution of 0.01, from -327.68 to 327.67</param>
    /// <param name="rollingCoefficient">Unitless, with a resolution of 0.0001, from 0 to 0.0255</param>
    /// <param name="windResistanceCoefficient">In kilogram per meter, with a resolution of 0.01, from 0 to 2.55 which is the same as A*Cd*Rho.</param>
    public void SendSimulationParameters(float _windspeed, float _grade, float _rollingCoefficient, float _surfaceArea, float _airDensity, float _airResistanceCoefficient)
    {
        float _airResistance = _surfaceArea * _airDensity * _airResistanceCoefficient;
        _windspeed *= 1 / 0.001f;
        _grade *= 1 / 0.01f;
        _rollingCoefficient *= 1 / 0.0001f;
        _airResistance *= 1 / 0.01f;
        short windspeed = Convert.ToInt16(Mathf.RoundToInt(_windspeed));
        short grade = Convert.ToInt16(Mathf.RoundToInt(_grade));
        byte rollingCoefficient = Convert.ToByte(Mathf.RoundToInt(_rollingCoefficient));
        byte airResistanceCoefficient = Convert.ToByte(Mathf.RoundToInt(_airResistance));
        Debug.LogFormat("W {0}, G {1}, Crr {2}, Cd*A*Rho {3}", windSpeed, grade, rollingCoefficient, airResistanceCoefficient);
    }

    public void SendSimulationParameters(short _windSpeed, short _grade, byte _coefficientOfRollingResistance, byte _windResistanceCoefficient)
    {
        List<byte> sendData = new List<byte>();
        sendData.Add(setIndoorBikeSimulationParameters); //Address
        sendData.AddRange(BitConverter.GetBytes(_windSpeed));
        sendData.AddRange(BitConverter.GetBytes(_grade));
        sendData.Add(_coefficientOfRollingResistance);
        sendData.Add(_windResistanceCoefficient);

        SendData(sendData.ToArray());
    }

    public void SetWheelCircumference(ushort _wheelCircumference)
    {
        List<byte> sendData = new List<byte>();
        sendData.Add(setWheelCircumference); //Address
        sendData.AddRange(BitConverter.GetBytes(_wheelCircumference));

        SendData(sendData.ToArray());
    }

    public void InitializeData()
    {
        SendData(new byte[] { requestControl });
    }

    private void SendData(byte[] sendData)
    {
        if (FClient.WriteValue(gattCharacteristic, sendData) != BluetoothErrors.WCL_E_SUCCESS)
        {
            string output = "";
            foreach (byte bytes in sendData)
            {
                output += bytes.ToString("X4") + " ";
            }
            Debug.Log(output + "failed to send.");
        }
    }
}
