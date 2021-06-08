using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMS_IBD : Characteristic
{
    public FMS_IBD()
    {
        Address = 0x2AD2;
    }

    protected override void ReceivedData(byte[] receivedData, ref string response)
    {
        if (BitConverter.ToUInt16(receivedData, 0) != flagValue)
            GetFlags(receivedData, ref response);
        GetData(receivedData, ref response);
    }

    public enum Flags { MoreData, AverageSpeed, InstantaneousCadence, AverageCadence, TotalDistance, ResistenceLevel, InstantaneousPower, AveragePower, ExpendedEnergy, HeartRate, MetabolicEquivalent, ElapsedTime, RemainingTime };

    bool[] flags = null;
    ushort flagValue = 0;

    private void GetFlags(byte[] bytes, ref string message)
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
                flags[j + i * 7] = bytes[i].GetBit(j);
                message += " " + (Flags)(j + i * 7) + ": " + flags[j + i * 7];
            }
        }

        flagValue = BitConverter.ToUInt16(bytes, 0);
    }

    public float InstSpeed { get; private set; } = 0;
    public float AvgSpeed { get; private set; } = 0;
    public float InstCad { get; private set; } = 0;
    public float AvgCad { get; private set; } = 0;
    public float TotDistance { get; private set; } = 0;
    public float ResistLvl { get; private set; } = 0;
    public int InstPwr { get; private set; } = 0;
    public int AvgPwr { get; private set; } = 0;
    public int TotEnergy { get; private set; } = 0;
    public int EnergyHr { get; private set; } = 0;
    public int EnergyMin { get; private set; } = 0;
    public int HrtRate { get; private set; } = 0;
    public float Meta { get; private set; } = 0;
    public int ElapsedSec { get; private set; } = 0;
    public int RemainingSec { get; private set; } = 0;

    public TimeSpan ElapsedTime => DateTime.Now - DateTime.Now.AddSeconds(ElapsedSec);
    public TimeSpan RemainingTime => DateTime.Now.AddSeconds(RemainingSec) - DateTime.Now;

    private void GetData(byte[] bytes, ref string message)
    {
        int i = 2;
        for (int j = 0; j < flags.Length; j++)
        {
            if (flags[j] || (!flags[j] && j == 0) || (!flags[j] && j == 2))
            {
                //Messages for outliers
                if (j != 8 || j == 0)
                    message += " " + (Flags)j + ": ";

                switch (j)
                {
                    case 0:
                        message += "Instantaneous speed: "; 
                        message += InstSpeed = BitConverter.ToUInt16(bytes, i) / 100;
                        i += 2;
                        break;
                    case 1:
                        message += AvgSpeed = BitConverter.ToUInt16(bytes, i) / 100;
                        i += 2;
                        break;
                    case 2:
                        message += InstCad = BitConverter.ToUInt16(bytes, i) / 2;
                        i += 2;
                        break;
                    case 3:
                        message += AvgCad = BitConverter.ToUInt16(bytes, i) / 2;
                        i += 2;
                        break;
                    case 4:
                        message += TotDistance = bytes.GetUInt24(i) / 1000;
                        i += 3;
                        break;
                    case 5:
                        message += ResistLvl = BitConverter.ToInt16(bytes, i) / 10;
                        i += 2;
                        break;
                    case 6:
                        message += InstPwr = BitConverter.ToInt16(bytes, i);
                        i += 2;
                        break;
                    case 7:
                        message += AvgPwr = BitConverter.ToInt16(bytes, i);
                        i += 2;
                        break;
                    case 8:
                        message += "Total Energy: ";
                        message += TotEnergy = BitConverter.ToUInt16(bytes, i);
                        i += 2;
                        message += "Energy per Hour: ";
                        message += EnergyHr = BitConverter.ToUInt16(bytes, i);
                        i += 2;
                        message += "Energy per Minute: ";
                        message += EnergyMin = bytes[i];
                        i++;
                        break;
                    case 9:
                        message += HrtRate = bytes[i];
                        i++;
                        break;
                    case 10:
                        message += Meta = bytes[i] / 10;
                        i++;
                        break;
                    case 11:
                        message += ElapsedSec = BitConverter.ToUInt16(bytes, i);
                        i += 2;
                        break;
                    case 12:
                        message += RemainingSec = BitConverter.ToUInt16(bytes, i);
                        break;

                }
            }
        }
    }
}
