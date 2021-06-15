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
    /// <summary>
    /// Get the best path from point to point.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static RoadPath GetBestPath(Point start, Point end)
    {
        if (start == end)
        {
            Debug.Log("End is the same as the beginning, this is probably not the intention.");
            return null;
        }

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
            Road[] selectedRoads = selectedPath.LastPoint.roadsAway;

            foreach (Road road in selectedRoads)
            {
                if (!lockedPoints.Contains(road.end))
                {
                    RoadPath newPath = new RoadPath(selectedPath, road);
                    newPath.length += road.spline.length;
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
            {
                activePaths.Remove(scrubList[i]);
                scrubList[i] = null;
            }

            timeOut++;
        }
        while (selectedPath.LastPoint != end && timeOut < maxTimeOut);

        if (timeOut < maxTimeOut)
            return selectedPath;

        Debug.LogError("Couldn't find a path.");

        return null;
    }

    /// <summary>
    /// Get the best path from point to Vector3.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="allPoints"></param>
    /// <returns></returns>
    public static RoadPath GetBestPath(Point start, Vector3 end, Point[] allPoints)
    {
        //Here I must get the target point for the 

        //This part is for optimization, but it might be useful to just delete it instead if it gives any issues.
        float distance = float.MaxValue;
        Point closestPoint = null;
        foreach(Point point in allPoints)
        {
            if (end == point.transform.position)
                return GetBestPath(start, point);
            float pointDistance = Vector3Tool.GetFlatDistance(point.transform.position, end);
            if (distance > pointDistance)
            {
                distance = pointDistance;
                closestPoint = point;
            }
        }

        float closestRoadAwayDist = float.MaxValue;
        float closestRoadTowardDist = float.MaxValue;

        Road closestRoadAway = GetClosestRoad(closestPoint.roadsAway, end, ref closestRoadAwayDist);
        Road closestRoadToward = GetClosestRoad(closestPoint.roadsTowards, end, ref closestRoadTowardDist);

        Road closestRoad = closestRoadAwayDist > closestRoadTowardDist ? closestRoadToward : closestRoadAway; //If closestroadToward is the shorter distance, take that one.

        #region Deprecated

        //This one is a distance based approach and did not quite work out so well... At least theoretically, IDK, I didn't really test it.
        //RoadPath outputEnd = GetBestPath(start, closestRoad.end);
        //RoadPath outputBeginning = GetBestPath(start, closestRoad.origin);

        //RoadPath output = outputEnd.length > outputBeginning.length ? outputEnd : outputBeginning; //Select the longest one so that we ensure we are passing through the point.

        #endregion

        //This is a logical approach, where it would select the closest road and go to the origin of that road. It would then add the closest road, if it does not pass through that already.
        RoadPath output = GetBestPath(start, closestRoad.origin);
        if (!output.Roads.Contains(closestRoad)) //Honestly can't remember why I put this check in, but it doesn't matter, I guess it just makes sure it's safe.
            output.Roads.Add(closestRoad);

        #region For Debugging

        //string debugOutput = "Path: ";
        //foreach (Road road in output.Roads)
        //    debugOutput += road.gameObject.name + " ";
        //Debug.Log(debugOutput);

        #endregion

        return output;
    }

    private static Road GetClosestRoad(Road[] roads, Vector3 position, ref float distance)
    {
        Road closestRoad = null;
        foreach (Road road in roads)
        {
            float roadDistance = Vector3Tool.GetFlatDistance(road.spline.FindNearestPointTo(position), position);
            if (distance > roadDistance)
            {
                distance = roadDistance;
                closestRoad = road;
            }
        }
        return closestRoad;
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