using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BezierSolution;
using UnityEngine.Events;
using System;

public class CyclistManager : MonoBehaviour
{
    [SerializeField] private BezierWalkerWithSpeed walker = null;
    [SerializeField] private BezierSpline spline = null;

    [SerializeField] private float shortRefreshTime = 0.1f;
    [SerializeField] private float speedRefreshTime = 1 / 30;
    [SerializeField] private float longRefreshTime = 1;

    //[SerializeField] private int averageGradeAccuracy = 5;

    [SerializeField] private float mass = 80;

    [SerializeField] private UnityEvent onLongUpdate, onShortUpdate, onSpeedUpdate = null;
    [SerializeField] private MonoBehaviourOmeter speedOmeter, gradeOmeter, powerOmeter = null;

    [Header("Debug")]
    [SerializeField] private BikePhysics bikePhysics = null;

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
    }

    private IEnumerator LongUpdater()
    {
        while (true)
        {
            float normalizedT = walker.NormalizedT;

            Vector3 location = spline.MoveAlongSpline(ref normalizedT, Mathf.Clamp(bikePhysics.SpeedMS * longRefreshTime, 1f, 36f));
            float oneSecGrade = ExtensionMethods.GetGrade(transform.position, location);

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
            float normalizedT = walker.NormalizedT;
            Vector3 location = spline.MoveAlongSpline(ref normalizedT, Mathf.Clamp(bikePhysics.SpeedMS * shortRefreshTime, 0.5f, 3f));

            bikePhysics.grade = ExtensionMethods.GetGrade(transform.position, location);
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
            walker.speed = speed;
            onSpeedUpdate.Invoke();
            speedOmeter.SetValue(bikePhysics.SpeedKPH);
            yield return new WaitForSeconds(speedRefreshTime);
        }
    }
}
