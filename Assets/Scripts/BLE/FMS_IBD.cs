using System;
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

    [Flags]
    public enum Flags : ushort
    {
        MoreData = 0b_1,
        AverageSpeed = 0b_10,
        InstantaneousCadence = 0b_100,
        AverageCadence = 0b_1000,
        TotalDistance = 0b_1_0000,
        ResistenceLevel = 0b_10_0000,
        InstantaneousPower = 0b_100_0000,
        AveragePower = 0b_1000_0000,
        ExpendedEnergy = 0b_1_0000_0000,
        HeartRate = 0b_10_0000_0000,
        MetabolicEquivalent = 0b_100_0000_0000,
        ElapsedTime = 0b_1000_0000_0000,
        RemainingTime = 0b_1_0000_0000_0000
    };

    Flags flags = Flags.MoreData;

    ushort flagValue = 0;

    private void GetFlags(byte[] bytes, ref string message)
    {
        flagValue = BitConverter.ToUInt16(bytes, 0);
        flags = (Flags)flagValue;
        message += flags;
    }

    public float InstSpeed { get; private set; } = 0;
    public float AvgSpeed { get; private set; } = 0;
    public float InstCad { get; private set; } = 0;
    public float AvgCad { get; private set; } = 0;
    public float TotDistance { get; private set; } = 0;
    public float ResistLvl { get; private set; } = 0;
    public int InstPwr { get; private set; } = 0;
    public int AvgPwr { get; private set; } = 0;
    public int EnergyExp { get; private set; } = 0;
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
        //Start at two, which is passing the flags, and is inclusive.
        int i = 2;

        InstSpeed = GetUShort(bytes, ref i, ref message, "Instantaneous speed") / 100;

        if (flags.HasFlag(Flags.AverageSpeed))
        {
            AvgSpeed = GetUShort(bytes, ref i, ref message, "Average Speed") / 100;
        }
        if (flags.HasFlag(Flags.InstantaneousCadence))
        {
            InstCad = GetUShort(bytes, ref i, ref message, "Instantaneous cadence") / 2;
        }
        if (flags.HasFlag(Flags.AverageCadence))
        {
            InstCad = GetUShort(bytes, ref i, ref message, "Average cadence") / 2;
        }
        if (flags.HasFlag(Flags.TotalDistance))
        {
            message += " Total Distance: ";
            TotDistance = bytes.GetUInt24(i) / 1000f;
            message += TotDistance;
            i += 3;
        }
        if (flags.HasFlag(Flags.ResistenceLevel))
        {
            ResistLvl = GetUShort(bytes, ref i, ref message, "Resistance Level") / 10;
        }
        if (flags.HasFlag(Flags.InstantaneousPower))
        {
            InstPwr = GetUShort(bytes, ref i, ref message, "Instantaneous power");
        }
        if (flags.HasFlag(Flags.AveragePower))
        {
            AvgPwr = GetUShort(bytes, ref i, ref message, "Average Power");
        }
        if (flags.HasFlag(Flags.ExpendedEnergy))
        {
            EnergyExp = GetUShort(bytes, ref i, ref message, "Expended Energy");

            EnergyHr = GetUShort(bytes, ref i, ref message, "Energy per hour");

            EnergyMin = GetByte(bytes, ref i, ref message, "Energy per minute");
        }
        if (flags.HasFlag(Flags.HeartRate))
        {
            HrtRate = GetByte(bytes, ref i, ref message, "Heart rate");
        }
        if (flags.HasFlag(Flags.MetabolicEquivalent))
        {
            Meta = GetByte(bytes, ref i, ref message, "Metabolic equivalent") / 10;
        }
        if (flags.HasFlag(Flags.ElapsedTime))
        {
            ElapsedSec = GetUShort(bytes, ref i, ref message, "Elapsed seconds");
        }
        if (flags.HasFlag(Flags.RemainingTime))
        {
            RemainingSec = GetUShort(bytes, ref i, ref message, "Remaining seconds");
        }
    }

    private ushort GetUShort(byte[] bytes, ref int i, ref string message, string variableName)
    {
        message += " "+ variableName + ": ";
        ushort output = BitConverter.ToUInt16(bytes, i);
        message += output;
        i += 2;
        return output;
    }

    private short GetShort(byte[] bytes, ref int i, ref string message, string variableName)
    {
        message += " " + variableName + ": ";
        short output = BitConverter.ToInt16(bytes, i);
        message += output;
        i += 2;
        return output;
    }

    private byte GetByte(byte[] bytes, ref int i, ref string message, string variableName)
    {
        message += " " + variableName + ": ";
        byte output = bytes[i];
        message += output;
        i ++;
        return output;
    }
}
