using System.Collections.Generic;
using UnityEngine;
using ThomasLib.Unity;
using System;

[ExecuteInEditMode]
public class Pathfinder : MonoBehaviour
{
    public Point startPoint;
    public Point endPoint;

    private RoadPath goodPath = null;
    private Vector3[,] debugPoints = null;

    private void Update()
    {
        if (goodPath == null) 
            CalculatePath();
        for (int i = 0; i < debugPoints.Length / 2; i++)
            Debug.DrawLine(debugPoints[i, 0], debugPoints[i, 1], Color.red);
    }

    public void CalculatePath()
    {
        RoadPath goodPath = PathFinding.GetBestPath(startPoint, endPoint);

        debugPoints = new Vector3[goodPath.Roads.Count, 2];
        Point beginPoint = goodPath.StartPoint;
        for (int i = 0; i < goodPath.Roads.Count; i++)
        {
            debugPoints[i, 0] = beginPoint.transform.position;
            beginPoint = goodPath.Roads[i].end;
            debugPoints[i, 1] = beginPoint.transform.position;
        }
    }
}

public static class PathFinding
{
    public static RoadPath GetBestPath(Point start, Point end)
    {
        List<Point> lockedPoints = new List<Point>();
        List<RoadPath> activePaths = new List<RoadPath>();
        RoadPath selectedPath = null;
        int maxTimeOut = 1000;
        int timeOut = 0;

        lockedPoints.Add(start);
        selectedPath = new RoadPath(start)
        {
            length = 0,
            distance = float.MaxValue
        };

        do
        {
            Road[] selectedRoads = selectedPath.LastPoint.roads;

            foreach (Road road in selectedRoads)
            {
                if (!lockedPoints.Contains(road.end))
                {
                    if (road.end == end)
                        return new RoadPath(selectedPath, road);

                    RoadPath newPath = new RoadPath(selectedPath, road);
                    newPath.length += Vector3Tool.GetFlatDistance(selectedPath.LastPoint.transform.position, road.end.transform.position);
                    newPath.distance = Vector3Tool.GetFlatDistance(end.transform.position, road.end.transform.position);
                    activePaths.Add(newPath);
                }
            }

            float lowestTotal = float.MaxValue;
            List<RoadPath> scrubList = new List<RoadPath>();

            foreach (RoadPath activePath in activePaths)
            {
                if (!lockedPoints.Contains(activePath.LastPoint))
                {
                    float distanceValue = activePath.distance + activePath.length;
                    if (distanceValue < lowestTotal)
                    {
                        selectedPath = activePath;
                        lowestTotal = distanceValue;
                    }
                }
                else scrubList.Add(activePath);
            }

            lockedPoints.Add(selectedPath.LastPoint);

            //For loop to destroy the paths that have been bested.
            for (int i = scrubList.Count - 1; i >= 0; i--)
                scrubList[i] = null;

            timeOut++;
        }
        while (selectedPath.LastPoint != end && timeOut < maxTimeOut);

        Debug.LogError("Couldn't find a path.");

        return null;
    }
}

public class RoadPath
{
    public RoadPath(Point lastPoint)
    {
        this.lastPoint = lastPoint;
        StartPoint = lastPoint;
    }

    public RoadPath(params Road[] roads)
    {
        Roads.AddRange(roads);
    }

    public RoadPath(RoadPath path)
    {
        length = path.length;
        Roads.AddRange(path.Roads);
        StartPoint = path.StartPoint;
    }

    public RoadPath(RoadPath path, params Road[] roads)
    {
        length = path.length;
        Roads.AddRange(path.Roads);
        Roads.AddRange(roads);
        StartPoint = path.StartPoint;
    }

    public List<Road> Roads { get; private set; } = new List<Road>();
    public float length = 0;
    public float distance = 0;
    public Point StartPoint { get; private set; } = null;
    public Point LastPoint => lastPoint != null ? lastPoint : Roads[Roads.Count - 1].end;
    private Point lastPoint = null;
}