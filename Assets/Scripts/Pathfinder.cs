using System.Collections.Generic;
using UnityEngine;
using ThomasLib.Unity;
using System;
using ThomasLib.Num;
using System.Linq;

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
        RoadPath goodPath = PathFinding.GetBestPath(new RoadPoint() { road = startPoint.roadsAway[0], normalizedT = 0 }, endPoint);

        debugPoints = new Vector3[goodPath.Roads.Count, 2];
        Point beginPoint = goodPath.startPoint.road.end;
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
    public static RoadPath GetBestPath(RoadPoint startPoint, Point end)
    {
        if (startPoint.road.end == end)
        {
            Debug.Log("End is the same as the beginning.");
            return new RoadPath()
            {
                startPoint = startPoint,
                endPoint = new RoadPoint() {
                    road = startPoint.road,
                    normalizedT = 1f
                },
                length = startPoint.road.spline.GetLengthApproximately(startPoint.normalizedT, 1f)
            };
        }

        List<Point> lockedPoints = new List<Point>();
        List<RoadPath> activePaths = new List<RoadPath>();

        RoadPath selectedPath = new RoadPath(startPoint)
        {
            length = startPoint.road.spline.GetLengthApproximately(startPoint.normalizedT, 1f),
            distance = float.MaxValue
        };

        int maxTimeOut = 1000;
        int timeOut = 0;

        lockedPoints.Add(startPoint.road.end);

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
        {
            selectedPath.endPoint = new RoadPoint() { road = selectedPath.Roads.Last(), normalizedT = 1 };
            return selectedPath;
        }

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
    public static RoadPath GetBestPath(RoadPoint startPoint, Vector3 end, Point[] allPoints, float distanceTolerance = 1f)
    {
        //Here I must get the target point for the 

        #region Getting the Closest Point
        //This part is for optimization, but it might be useful to just delete it instead if it gives any issues.
        float distance = float.MaxValue;
        Point closestPoint = null;
        foreach (Point point in allPoints)
        {
            float pointDistance = Vector3Tool.GetFlatDistance(point.transform.position, end);
            if (pointDistance.IsBetween(-distanceTolerance, distanceTolerance))
                return GetBestPath(startPoint, point);
            if (distance > pointDistance)
            {
                distance = pointDistance;
                closestPoint = point;
            }
        }
        #endregion

        #region Getting the Closest Road
        float closestRoadAwayDist = float.MaxValue;
        float closestRoadTowardDist = float.MaxValue;

        Road closestRoadAway = GetClosestRoad(closestPoint.roadsAway, end, ref closestRoadAwayDist);
        Road closestRoadToward = GetClosestRoad(closestPoint.roadsTowards, end, ref closestRoadTowardDist);

        Road closestRoad = closestRoadAwayDist > closestRoadTowardDist ? closestRoadToward : closestRoadAway; //If closestRoadToward is the shorter distance, take that one.
        #endregion

        #region Getting Closest Point on Line
        float normalizedT;
        closestRoad.spline.FindNearestPointTo(end, out normalizedT, 250);
        #endregion

        #region Deprecated
        //This one is a distance based approach and did not quite work out so well... At least theoretically, IDK, I didn't really test it.
        //RoadPath outputEnd = GetBestPath(start, closestRoad.end);
        //RoadPath outputBeginning = GetBestPath(start, closestRoad.origin);

        //RoadPath output = outputEnd.length > outputBeginning.length ? outputEnd : outputBeginning; //Select the longest one so that we ensure we are passing through the point.
        #endregion

        //Get the closest path to the beginning of the road.
        RoadPath output = GetBestPath(startPoint, closestRoad.origin);

        if (output == null) //Basically if the output cannot find a route, or if the end point is the same as the same as the begin point, it will return null.
            return null;

        output.endPoint = new RoadPoint() { 
            normalizedT = normalizedT,
            road = closestRoad
        };

        //Honestly the first one should ALWAYS fire, but if it doesn't for some weird reason, it'll just tell me.
        if (!output.Roads.Contains(closestRoad))
        {
            output.Roads.Add(closestRoad);
            output.length += output.endPoint.road.spline.GetLengthApproximately(0, output.endPoint.normalizedT);
        }
        else
        {
            Debug.Log("OKAY DONE");
            output.length -= output.endPoint.road.spline.GetLengthApproximately(output.endPoint.normalizedT, 1);
        }

        #region Debug Path
        string debugOutput = "Path: ";
        foreach (Road road in output.Roads)
            debugOutput += road.gameObject.name + " ";
        Debug.Log(debugOutput + " Distance: " + output.length + " m");
        #endregion

        return output;
    }

    /// <summary>
    /// Uses a less taxing accuracy to get the closest point.
    /// </summary>
    /// <param name="roads"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    private static Road GetClosestRoad(Road[] roads, Vector3 position, ref float distance, float accuracy = 25)
    {
        Road closestRoad = null;
        foreach (Road road in roads)
        {
            float roadDistance = Vector3Tool.GetFlatDistance(road.spline.FindNearestPointTo(position, accuracy), position);
            if (distance > roadDistance)
            {
                distance = roadDistance;
                closestRoad = road;
            }
        }
        return closestRoad;
    }
}

[Serializable]
public struct RoadPoint
{
    public Road road;
    public float normalizedT;
}

[Serializable]
public class RoadPath
{
    public RoadPath(RoadPoint startPoint)
    {
        lastPoint = startPoint.road.end;
        this.startPoint = startPoint;
    }

    public RoadPath(params Road[] roads)
    {
        Roads.AddRange(roads);
    }

    public RoadPath(RoadPath path)
    {
        length = path.length;
        Roads.AddRange(path.Roads);
        startPoint = path.startPoint;
    }

    public RoadPath(RoadPath path, params Road[] roads)
    {
        length = path.length;
        Roads.AddRange(path.Roads);
        Roads.AddRange(roads);
        startPoint = path.startPoint;
    }

    public List<Road> Roads { get; private set; } = new List<Road>();
    public float length = 0;
    public float distance = 0;
    public RoadPoint endPoint; //Endpoint is included in the roads.
    public RoadPoint startPoint = new RoadPoint();
    public Point LastPoint => lastPoint != null ? lastPoint : Roads[Roads.Count - 1].end;
    private Point lastPoint = null;
}