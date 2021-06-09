using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class FMS_CP : Characteristic
{
    //Purpose: Interact with the thing.

    public FMS_CP()
    {
        Address = 0x2AD9;
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

    private const float wheelCircumference = 2105f; //in mm
    private Thread sendDataQueue = null;
    private bool responseReceived = true;
    private Queue<byte[]> sendStack = new Queue<byte[]>();

    public bool ReceivedPermission { get; private set; }  = false;

    protected override void ReceivedData(byte[] receivedData, ref string response)
    {
        InterpretResponseCode(receivedData, ref response);
    }

    protected override void OnInitialization()
    {
        sendDataQueue = new Thread(SendDataQueue);
        sendDataQueue.Start();
        InitializeData();
    }

    #region Response
    public bool InterpretResponseCode(byte[] input, ref string message)
    {
        if (input[0] != responseCode)
        {
            message = "The input is not a response code.";
            return false;
        }

        responseReceived = true;

        if (input == new byte[] { responseCode, requestControl, succes })
            ReceivedPermission = true;

        switch (input[1])
        {
            case requestControl:
                message += "Request Control ";
                break;
            case reset:
                message += "Reset ";
                break;
            case setTargetSpeed:
                //UINT16 with speed in km/h with a res of 0.01 km/h.
                message += "Setting Target Speed to " + (BitConverter.ToUInt16(input, 3) * 0.01f).ToString() + " km/h ";
                break;
            case setTargetInclination:
                message += "Setting Target Inclination to " + (BitConverter.ToInt16(input, 3) * 0.1f).ToString() + "% ";
                break;
            case setTargetResistenceLevel:
                message += "Setting Target Resistance Level " + (input[3] * 0.1f).ToString() + " ";
                break;
            case setTargetPower:
                message += "Setting Target Power " + BitConverter.ToInt16(input, 3).ToString() + " W ";
                break;
            case setTargetHeartRate:
                message += "Setting Target Heart Rate to " + input[3].ToString() + " BPM ";
                break;
            case startOrResume:
                message += "Start or Resume ";
                break;
            case stopOrPause:
                message += /*input[3] == stop ? "Stop " : "Pause "*/ "Stop or Pause ";
                break;
            case setTargetedExpendedEnergy:
                message += "Setting Targeted Expended Energy to " + BitConverter.ToUInt16(input, 3).ToString() + " Calories ";
                break;
            case setTargetedNumberOfSteps:
                message += "Setting Targeted Number of Steps to " + BitConverter.ToUInt16(input, 3).ToString() + " steps ";
                break;
            case setTargetedNumberOfStrides:
                message += "Setting Targeted Number of Strides to " + BitConverter.ToUInt16(input, 3).ToString() + " strides ";
                break;
            case setTargetedDistance:
                message += "Setting Targeted Distance to " + BitConverter.ToUInt32(input, 3).ToString() + " meters ";
                break;
            case setTargetedTrainingTime:
                message += "Setting Targeted Training Time to " + new DateTime().AddSeconds(BitConverter.ToUInt16(input, 3)).ToString("HH:mm:ss") + " ";
                break;
            case setTargetedTimeInTwoHeartRateZones:
                message += "Setting Targeted Time in Two Heart Rate Zones to " + new DateTime().AddSeconds(BitConverter.ToUInt16(input, 3)).ToString("HH:mm:ss") + " in Fat Burn Zone, and " + new DateTime().AddSeconds(BitConverter.ToUInt16(input, 5)).ToString("HH:mm:ss") + " in Fitness Zone ";
                break;
            case setTargetedTimeInThreeHeartRateZones:
                message += "Setting Targeted Time in Three Heart Rate Zones to " + new DateTime().AddSeconds(BitConverter.ToUInt16(input, 3)).ToString("HH:mm:ss") + " in Light Zone, " + new DateTime().AddSeconds(BitConverter.ToUInt16(input, 5)).ToString("HH:mm:ss") + " in Moderate Zone, and " + new DateTime().AddSeconds(BitConverter.ToUInt16(input, 3)).ToString("HH:mm:ss") + " in Hard Zone ";
                break;
            case setTargetedTimeInFiveHeartRateZones:
                message += "Setting Targeted Time in Five Heart Rate Zones to " + new DateTime().AddSeconds(BitConverter.ToUInt16(input, 3)).ToString("HH:mm:ss") + " in Very Light Zone, " + new DateTime().AddSeconds(BitConverter.ToUInt16(input, 3)).ToString("HH:mm:ss") + " in Light Zone, " + new DateTime().AddSeconds(BitConverter.ToUInt16(input, 5)).ToString("HH:mm:ss") + " in Moderate Zone, " + new DateTime().AddSeconds(BitConverter.ToUInt16(input, 3)).ToString("HH:mm:ss") + " in Hard Zone, and " + new DateTime().AddSeconds(BitConverter.ToUInt16(input, 3)).ToString("HH:mm:ss") + " in Maximum Zone ";
                break;
            case setIndoorBikeSimulationParameters:
                message += "Setting Indoor Bike Simulation Parameters "/* + (BitConverter.ToInt16(input, 2) * 0.001f).ToString() + " m/s Wind Speed, " + (BitConverter.ToInt16(input, 4) * 0.01f).ToString() + "% Grade, " + (input[5] * 0.0001f).ToString() + " Coefficient of Rolling Resistance, and " + (input[6] * 0.01f).ToString() + " kg/m Wind Resistance Coefficient "*/;
                break;
            case setWheelCircumference:
                message += "Setting Wheel Circumference " /*+ (BitConverter.ToUInt16(input, 3) * 0.1f).ToString() + " mm "*/;
                break;
            case spinDownControl:
                message += "Setting spin Down Control " + (input[3] == start ? "Start " : "Ignore ");
                break;
            case setTargetedCadence:
                message += "Setting Targeted Cadence " + (BitConverter.ToUInt16(input, 3) * 0.5f).ToString() + " per minute ";
                break;
            default:
                message += "Unknown OP code ";
                break;
        }

        switch (input[2])
        {
            case succes:
                message += "was successful.";
                break;
            case opCodeNotSupported:
                message += "was an unsupported OP code";
                break;
            case invalidParameter:
                message += "contained an invalid parameter.";
                break;
            case operationFailed:
                message += "was unsuccessful.";
                break;
            case controlNotPermitted:
                message += "was not permitted.";
                break;
        }

        Debug.Log(message);

        return true;
    }
    #endregion

    #region Specific Functions
    public void StartOrResume()
    {
        SendData(new byte[] { startOrResume });
    }

    public void PauseOrStop(bool _stop)
    {
        byte value;
        if (_stop) value = stop;
        else value = pause;
        SendData(new byte[] { stopOrPause, value });
    }

    public void SendResistanceValue(byte sendData) => SendData(new byte[] { setTargetResistenceLevel, sendData });

    public void ResetData() => SendData(new byte[] { reset });

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_windSpeed">In meters per second, with a resolution of 0.001, from -32.768 m/s to 32.787 m/s</param>
    /// <param name="_grade">In percentage, with a resolution of 0.01, from -327.68 to 327.67</param>
    /// <param name="rollingCoefficient">Unitless, with a resolution of 0.0001, from 0 to 0.0255</param>
    /// <param name="windResistanceCoefficient">In kilogram per meter, with a resolution of 0.01, from 0 to 2.55 which is the same as A*Cd*Rho.</param>
    public void SetSimulationParameter(float _windSpeed, float _grade, float _rollingCoefficient, float _surfaceArea, float _airDensity, float _airResistanceCoefficient)
    {
        float _airResistance = _surfaceArea * _airDensity * _airResistanceCoefficient;
        _windSpeed *= 1 / 0.001f;
        _grade *= 1 / 0.01f;
        _rollingCoefficient *= 1 / 0.0001f;
        _airResistance *= 1 / 0.01f;
        short windSpeed = Convert.ToInt16(Mathf.RoundToInt(_windSpeed));
        short grade = Convert.ToInt16(Mathf.RoundToInt(_grade));
        byte rollingCoefficient = Convert.ToByte(Mathf.RoundToInt(_rollingCoefficient));
        byte airResistanceCoefficient = Convert.ToByte(Mathf.RoundToInt(_airResistance));
        SetSimulationParameter(windSpeed, grade, rollingCoefficient, airResistanceCoefficient);
    }

    public void SetSimulationParameter(short _windSpeed, short _grade, byte _coefficientOfRollingResistance, byte _windResistanceCoefficient)
    {
        List<byte> sendData = new List<byte>();
        sendData.Add(setIndoorBikeSimulationParameters); //Address
        sendData.AddRange(BitConverter.GetBytes(_windSpeed));
        sendData.AddRange(BitConverter.GetBytes(_grade));
        sendData.Add(_coefficientOfRollingResistance);
        sendData.Add(_windResistanceCoefficient);

        SendData(sendData.ToArray());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_wheelCircumference">"In mm, with a resolution of 0.1 mm, from 0 to 6553.5 mm"</param>
    public void SetWheelCircumference(float _wheelCircumference)
    {
        _wheelCircumference *= 1 / 0.1f;
        ushort wheelCircumference = Convert.ToUInt16(Mathf.RoundToInt(_wheelCircumference));
        SetWheelCircumference(wheelCircumference);
    }

    public void SetWheelCircumference(ushort _wheelCircumference)
    {
        List<byte> sendData = new List<byte>();
        sendData.Add(setWheelCircumference); //Address
        sendData.AddRange(BitConverter.GetBytes(_wheelCircumference));

        SendData(sendData.ToArray());
    }

    #endregion

    public void InitializeData()
    {
        SendData(new byte[] { requestControl });
        ResetData();
        SetWheelCircumference(wheelCircumference);
        StartOrResume();
        SetSimulationParameter(0f, 0f, 0.00250f, 0.650f, 1.293f, 0.350f);
    }

    private void SendData(byte[] data)
    {
        sendStack.Enqueue(data);
        if (!sendDataQueue.IsAlive)
        {
            sendDataQueue = new Thread(SendDataQueue);
            sendDataQueue.Start();
        }
    }

    private void SendDataQueue()
    {
        while (sendStack.Count > 0)
        {
            if (responseReceived)
            {
                responseReceived = false;
                if (BLEManager.Instance.SendData(gattCharacteristic, sendStack.Peek()) != BluetoothErrors.WCL_E_SUCCESS)
                {
                    responseReceived = true;
                    Debug.LogWarning("Couldn't send, trying again.");
                }
                else sendStack.Dequeue();
            }
            //else Debug.Log("Waiting for response...");
            Thread.Sleep(250);
        }
    }
}
