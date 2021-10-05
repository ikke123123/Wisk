using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BezierSolution;

[ExecuteInEditMode]
public class Road : MonoBehaviour
{
    public Point end;
    public Point origin;
    [Space]
    public BezierSpline spline;

    private readonly bool drawLine = false;

    void Update()
    {
        if (drawLine)
            Debug.DrawLine(origin.transform.position, end.transform.position);
    }
}
