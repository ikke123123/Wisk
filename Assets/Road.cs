using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Road : MonoBehaviour
{
    public Point end;
    public Point origin;

    private readonly bool drawLine = false;

    void Update()
    {
        if (drawLine)
            Debug.DrawLine(origin.transform.position, end.transform.position);
    }
}
