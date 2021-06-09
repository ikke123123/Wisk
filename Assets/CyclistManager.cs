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

    [Header("Debug")]
    [SerializeField] private BikePhysics bikePhysics = null;

    private Coroutine shortUpdater, speedUpdater, longUpdater;
    private FMS_IBD ibd = null;
    private FMS_CP cp = null;

    private void OnEnable()
    {
        ibd = BLEManager.Instance.fms_IBD;
        cp = BLEManager.Instance.fms_CP;

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
    }

    private IEnumerator LongUpdater()
    {
        while (true)
        {
            float normalizedT = walker.NormalizedT;

            Vector3 location = spline.MoveAlongSpline(ref normalizedT, bikePhysics.SpeedMS == 0 ? 1 : bikePhysics.SpeedMS * longRefreshTime);
            float oneSecGrade = ExtensionMethods.GetGrade(transform.position, location);

            if (cp.ReceivedPermission)
                cp.SetSimulationParameter(0f, oneSecGrade, bikePhysics.rollingCoeff, bikePhysics.frontalArea, bikePhysics.rho, bikePhysics.dragCoeff);

            onLongUpdate.Invoke();
            yield return new WaitForSeconds(longRefreshTime);
        }
    }

    private IEnumerator ShortUpdater()
    {
        while (true)
        {
            float normalizedT = walker.NormalizedT;
            Vector3 location = spline.MoveAlongSpline(ref normalizedT, bikePhysics.SpeedMS == 0 ? 0.1f : bikePhysics.SpeedMS * shortRefreshTime);
            bikePhysics.grade = ExtensionMethods.GetGrade(transform.position, location);
            onShortUpdate.Invoke();
            GradeOmeter.SetGrade(bikePhysics.grade);
            yield return new WaitForSeconds(shortRefreshTime);
        }
    }

    private IEnumerator SpeedUpdater()
    {
        while (true)
        {
            walker.speed = bikePhysics.UpdateSpeed(speedRefreshTime);
            onSpeedUpdate.Invoke();
            SpeedOmeter.SetSpeed(bikePhysics.SpeedKPH);
            yield return new WaitForSeconds(speedRefreshTime);
        }
    }
}
