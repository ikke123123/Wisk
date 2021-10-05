using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ThomasLib.Num;

[Serializable]
public class BikePhysics
{
    //Purpose:
    //- Hold physics values: e.g. wind resistance etc.
    //- Maintain a speed
    //- Socket for adding power
    //- Must be updated by an external thing and must be updated in a regular time interval.
    //- Build in internal timespan (later)

    /// <summary>
    /// Initialization of BikePhysics, put in the values that you want to change.
    /// </summary>
    public BikePhysics()
    {

    }

    //Public values
    /// <summary>
    /// Current speed of bike in m/s.
    /// </summary>
    public float SpeedMS { get; private set; } = 0;
    /// <summary>
    /// Current speed of bike in kph.
    /// </summary>
    public float SpeedKPH => SpeedMS * 3.6f;
    /// <summary>
    /// Current speed of bike in mph.
    /// </summary>
    public float SpeedMPH => SpeedMS * 2.237f;

    /// <summary>
    /// Air Drag Coefficient, default: 0.650
    /// </summary>
    public float dragCoeff = 0.650f;
    /// <summary>
    /// Surface area in m2, default: 0.375
    /// </summary>
    public float frontalArea = 0.375f;
    /// <summary>
    /// Air density in kg/m3, default: 1.225
    /// </summary>
    public float rho = 1.225f;
    /// <summary>
    /// Gravitational acceleration in m/s2, default: 9.8067
    /// </summary>
    public float g = 9.8067f;
    /// <summary>
    /// Rolling Resistance Coefficient, default: 0.003
    /// </summary>
    public float rollingCoeff = 0.003f;
    /// <summary>
    /// Mass of Bike and Rider in kg, default: 80
    /// </summary>
    public float mass = 80;

    /// <summary>
    /// Power in watts
    /// </summary>
    public int power = 0;
    /// <summary>
    /// Grade in percent
    /// </summary>
    public float grade = 0;

    public const bool allowNegative = false;
    public const float minSpeed = 0.1f;

    public float UpdateSpeed(float timeStep)
    {
        return SpeedMS = GetNewVelocity(power, SpeedMS, grade, mass, timeStep);
    }

    private float GetNewVelocity(int power, float velocity, float grade, float mass, float timeStep)
    {
        float powerNeeded = GetTotalForce(velocity, grade, mass) * velocity;
        float netPower = power - powerNeeded;
        float netSpeed = velocity * velocity + 2 * netPower * timeStep / mass;
        float output = Mathf.Sqrt(Mathf.Abs(netSpeed));
        return ((output < minSpeed) ? 0 : output * (netSpeed >= 0 && !allowNegative ? 1 : -1)); //Limiter and a thing that converts the value back to a negative value if allowNegative is turned on.
    }

    private float GetTotalForce(float velocity, float grade, float mass) => FDrag(velocity) + FRolling(grade, mass, velocity) + FGravity(grade, mass);

    private float FDrag(float velocity) => 0.5f * dragCoeff * frontalArea * rho * velocity * velocity;

    private float FRolling(float grade, float mass, float velocity)
    {
        if (!velocity.IsBetween(-0.01f, 0.01f))
            return g * Mathf.Cos(Mathf.Atan(grade / 100)) * mass * rollingCoeff;
        else
            return 0;
    }

    private float FGravity(float grade, float mass) => g * Mathf.Sin(Mathf.Atan(grade / 100)) * mass;
}