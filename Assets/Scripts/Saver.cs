using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using ThomasLib.Time;
using ThomasLib.Unity;
using UnityEngine;
using UnityEngine.UI;

public class Saver : MonoBehaviour
{
    [SerializeField, Tooltip("In time in seconds.")] private float pollFrequency = 1;
    [SerializeField] private CyclistManager cyclistManager = null;
    [SerializeField] private Button startButton = null;
    [SerializeField] private Button stopButton = null;
    [SerializeField] private MonoBehaviourOmeter avgSpeedOmeter, avgPowerOmeter, elevationOmeter = null;

    /// <summary>
    /// Turn off poll data and save it safely.
    /// </summary>
    public bool active = false;

    private FMS_IBD ibd = null;
    private BikePhysics bikePhysics = null;

    private Training currentTraining = null;

    private Coroutine pollDataAsync = null;

    private void Start()
    {
        bikePhysics = cyclistManager.bikePhysics;
        ibd = BLEManager.Instance.fms_IBD;
        startButton.gameObject.SetActive(true);
        stopButton.gameObject.SetActive(false);
    }

    public void StartPoll()
    {
        if (pollDataAsync != null)
            return;
        active = true;
        startButton.gameObject.SetActive(false);
        stopButton.gameObject.SetActive(true);
        pollDataAsync = StartCoroutine(PollDataAsync());
    }

    public void StopPoll()
    {
        active = false;
        stopButton.interactable = false;
    }

    private IEnumerator PollDataAsync()
    {
        currentTraining = new Training();

        Vector3 lastPosition = transform.position;

        while (active)
        {
            currentTraining.Add(lastPosition, transform.position, bikePhysics.power, bikePhysics.SpeedKPH, ibd.InstCad);
            SetOmeters(currentTraining.AverageSpeed, currentTraining.AveragePower, currentTraining.TotalElevation);
            lastPosition = transform.position;
            yield return new WaitForSeconds(pollFrequency);
        }

        SetOmeters(0, 0, 0);

        currentTraining.Save();

        startButton.gameObject.SetActive(true);
        stopButton.gameObject.SetActive(false);
        stopButton.interactable = true;

        currentTraining = null;
    }

    private void SetOmeters(float averageSpeed, float averagePower, float elevation)
    {
        avgSpeedOmeter.SetValue(averageSpeed);
        avgPowerOmeter.SetValue(averagePower);
        elevationOmeter.SetValue(elevation);
    }
}

public class Training
{
    public Training()
    {

    }

    public Training(byte[] bytes)
    {
        FromBytes(bytes);
    }
    
    public float AverageSpeed { get; private set; } = 0; //In kph
    public float Distance { get; private set; } = 0; //In kilometer
    public float TotalElevation { get; private set; } = 0; //In meters
    public TimeSpan ElapsedTime { get; private set; } = new TimeSpan(0, 0, 0);
    public DateTime StartDateTime = DateTime.UtcNow;
    public int TotalPower { get; private set; } = 0;
    public float TotalCadence { get; private set; } = 0;
    public float AverageCadance { get; private set; } = 0;
    public float AveragePower { get; private set; } = 0; //In watt

    public List<Poll> polls = new List<Poll>();

    public void Add(Vector3 lastPosition, Vector3 position, int power, float velocity, float cadence)
    {
        Poll newPoll = new Poll
        {
            position = position,
            power = power,
            velocity = velocity,
            elevationDelta = Vector3Tool.GetElevationDifference(lastPosition, position),
            distanceDelta = Vector3Tool.GetFlatDistance(lastPosition, position),
            cadence = cadence
        };
        Distance += newPoll.distanceDelta / 1000f; //convert from meters to km
        ElapsedTime = StartDateTime - DateTime.UtcNow;
        AverageSpeed = (float)(Distance / (ElapsedTime.TotalSeconds > 0 ? ElapsedTime.TotalHours : 0.1f));
        TotalPower += power;
        TotalCadence += cadence;
        TotalElevation += Mathf.Abs(newPoll.elevationDelta);
        polls.Add(newPoll);
        AverageCadance = TotalCadence / polls.Count;
        AveragePower = TotalPower / polls.Count;
    }

    public byte[] ToBytes()
    {
        List<byte> output = new List<byte>();
        output.AddRange(AverageSpeed.ToBytes()); //4
        output.AddRange(Distance.ToBytes()); //4
        output.AddRange(BitConverter.GetBytes(ElapsedTime.Ticks)); //8
        output.AddRange(BitConverter.GetBytes(StartDateTime.DateTimeToUnix())); //8
        output.AddRange(TotalPower.ToBytes()); //4
        output.AddRange(TotalCadence.ToBytes()); //4
        output.AddRange(AverageCadance.ToBytes()); //4
        output.AddRange(AveragePower.ToBytes()); //4
        output.AddRange(TotalElevation.ToBytes()); //4

        foreach (Poll poll in polls)
            output.AddRange(poll.ToBytes());
        return output.ToArray();
    }

    public void FromBytes(byte[] input)
    {
        AverageSpeed = input.ToFloat(0); //4
        Distance = input.ToFloat(4); //4
        ElapsedTime = new TimeSpan(input.ToLong(8)); //8
        StartDateTime = TimeTool.UnixToDateTime(input.ToLong(16)); //8
        TotalPower = BitConverter.ToInt32(input, 24); //4
        TotalCadence = input.ToFloat(28); //4
        AverageCadance = input.ToFloat(32); //4
        AveragePower = input.ToFloat(36); //4
        TotalElevation = input.ToFloat(40); //4
        for (int i = 44; i < input.Length; i += 32 /* Size of a poll*/)
            polls.Add(new Poll(input, i));
    }

    public void Save()
    {
        string fileName = string.Format("Wisk-{0}-{1}-{2}-{3}-{4}.txt", AverageSpeed, Distance, ElapsedTime.ToString(@"hh\:mm\:ss"), StartDateTime.ToString("d", CultureInfo.CurrentCulture), StartDateTime.ToString("T", CultureInfo.CurrentCulture)).Replace("\\", "-");

        foreach (char character in Path.GetInvalidFileNameChars())
            fileName = fileName.Replace(character, '-'); //Remove the invalid characters from the filename.

        string parentFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Fifth Quarter Studios", "Wisk");

        Directory.CreateDirectory(parentFolder);

        using (FileStream fS = new FileStream(Path.Combine(parentFolder, fileName), FileMode.Create, FileAccess.Write))
        {
            byte[] byters = ToBytes();
            string debug = "";
            foreach (byte bytes in byters)
            {
                debug += bytes.ToString("X4");
            }
            Debug.Log(debug);
            fS.Write(ToBytes(), 0, byters.Length);
            fS.Close();
        }
    }
}

public struct Poll
{
    public Poll(byte[] bytes, int startPos)
    {
        position = Vector3.zero;
        power = 0;
        velocity = 0;
        elevationDelta = 0;
        distanceDelta = 0;
        cadence = 0;

        ToPoll(bytes, startPos);
    }

    public Vector3 position; //Position
    public int power; //Watts
    public float velocity; //In kph
    public float elevationDelta; //In m vs pos last second
    public float distanceDelta; //In m vs pos last second
    public float cadence;

    public byte[] ToBytes()
    {
        byte[] output = new byte[32];
        ExtensionMethods.AddToArray(ref output, position.ToBytes(), 0); //Takes 12 bytes
        ExtensionMethods.AddToArray(ref output, power.ToUShort().ToBytes(), 12); //Takes 2 bytes
        ExtensionMethods.AddToArray(ref output, velocity.ToBytes(), 14); //Takes 4 too
        ExtensionMethods.AddToArray(ref output, elevationDelta.ToBytes(), 18); //Takes 4 too
        ExtensionMethods.AddToArray(ref output, distanceDelta.ToBytes(), 22); //Takes 4 too
        ExtensionMethods.AddToArray(ref output, cadence.ToBytes(), 26); //4
        //There are 4 bytes left over for future values, such as heart rate, grade, wind resistance, etc.
        return output;
    }

    private void ToPoll(byte[] bytes, int startPos)
    {
        position = bytes.ToVector3(0 + startPos); //12
        power = bytes.ToUShort(12 + startPos); //2
        velocity = bytes.ToFloat(14 + startPos); //4
        elevationDelta = bytes.ToFloat(18 + startPos); //4
        distanceDelta = bytes.ToFloat(22 + startPos); //4
        cadence = bytes.ToFloat(26 + startPos); //4
    }
}