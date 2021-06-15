using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BezierSolution;
using UnityEngine.Events;
using System;
using ThomasLib.Unity;

public class CyclistManager : MonoBehaviour
{
    [Tooltip("Slope modifier makes hills easier or harder. 1 is like real life, 0.1 is a tenth as hard, and 2 is twice as hard."), Range(0.1f, 2f)] public float slopeModifier = 1;

    [SerializeField] private CyclistTrackFollower cyclistTrackFollower;

    [SerializeField] private float shortRefreshTime = 0.1f;
    [SerializeField] private float speedRefreshTime = 1 / 30;
    [SerializeField] private float longRefreshTime = 1;

    //[SerializeField] private int averageGradeAccuracy = 5;

    [SerializeField] private float mass = 80;

    [SerializeField] private UnityEvent onLongUpdate, onShortUpdate, onSpeedUpdate = null;
    [SerializeField] private MonoBehaviourOmeter speedOmeter, gradeOmeter, powerOmeter, cadanceOmeter = null;

    [Header("Debug")]
    public BikePhysics bikePhysics = null;

    private Coroutine shortUpdater, speedUpdater, longUpdater;
    private FMS_IBD ibd = null;
    private FMS_CP cp = null;

    private void Start()
    {
        ibd = BLEManager.Instance.fms_IBD;
        cp = BLEManager.Instance.fms_CP;
        Debug.Log(ibd);
        Debug.Log(cp);

        bikePhysics = new BikePhysics()
        {
            mass = mass
        };
        shortUpdater = StartCoroutine(ShortUpdater());
        speedUpdater = StartCoroutine(SpeedUpdater());
        longUpdater = StartCoroutine(LongUpdater());

        ibd.AddCallbackTarget(IBDUpdater);
    }

    public void IBDUpdater(object sender, EventArgs e)
    {
        bikePhysics.power = ibd.InstPwr;
        powerOmeter.SetValue(bikePhysics.power);
        cadanceOmeter.SetValue(ibd.InstCad);
    }

    private IEnumerator LongUpdater()
    {
        while (true)
        {
            float normalizedT = cyclistTrackFollower.m_normalizedT;

            Vector3 location = cyclistTrackFollower.currentTrack.spline.MoveAlongSpline(ref normalizedT, Mathf.Clamp(bikePhysics.SpeedMS * longRefreshTime, 1f, 36f));
            float oneSecGrade = Vector3Tool.GetGrade(transform.position, location) * slopeModifier;

            if (cp.ReceivedPermission)
                cp.SetSimulationParameter(0f, oneSecGrade, bikePhysics.rollingCoeff, bikePhysics.frontalArea, bikePhysics.rho, bikePhysics.dragCoeff);
            else Debug.Log("No permission yet");

            onLongUpdate.Invoke();
            yield return new WaitForSeconds(longRefreshTime);
        }
    }

    private IEnumerator ShortUpdater()
    {
        while (true)
        {
            float normalizedT = cyclistTrackFollower.m_normalizedT;
            Vector3 location = cyclistTrackFollower.currentTrack.spline.MoveAlongSpline(ref normalizedT, Mathf.Clamp(bikePhysics.SpeedMS * shortRefreshTime, 0.5f, 3f));

            bikePhysics.grade = Vector3Tool.GetGrade(transform.position, location) * slopeModifier;
            onShortUpdate.Invoke();
            gradeOmeter.SetValue(bikePhysics.grade);
            yield return new WaitForSeconds(shortRefreshTime);
        }
    }

    private IEnumerator SpeedUpdater()
    {
        while (true)
        {
            float speed = bikePhysics.UpdateSpeed(speedRefreshTime);
            cyclistTrackFollower.speed = speed;
            onSpeedUpdate.Invoke();
            speedOmeter.SetValue(bikePhysics.SpeedKPH);
            yield return new WaitForSeconds(speedRefreshTime);
        }
    }
}
