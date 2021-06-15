using BezierSolution;
using System;
using System.Collections;
using System.Collections.Generic;
using ThomasLib;
using ThomasLib.Enumaration;
using ThomasLib.Num;
using UnityEngine;
using System.Linq;

//BUILD THE GRADE MEASURER INTO THIS ONE.
//[ExecuteInEditMode]
public class CyclistTrackFollower : MonoBehaviour
{
    public static CyclistTrackFollower cTF = null;

    [Header("Settings")]
    [SerializeField] private float maxDegreesLookPerSecond = 5f;
    /// <summary>
    /// Speed as meters per second.
    /// </summary>
    public float speed = 5;
    public CyclistMode cyclistMode;
    public Point[] allPoints;

    [Header("Navigate To Point")]
    [SerializeField] private Point pointToGoTo = null;
    [SerializeField] private Transform locationToGoTo = null;

    [Header("Debug")]
    public Road currentTrack = null;
    [SerializeField] public List<Road> trackToFollow = new List<Road>();

    [Range(0f, 1f)]
    public float m_normalizedT = 0f;
    private Quaternion lastRotation = Quaternion.identity;

    private void Awake()
    {
        if (cTF != null && cTF != this)
            Destroy(cTF);
        cTF = this;
    }

    private void Start()
    {
        if (trackToFollow.Count > 0 && cyclistMode == CyclistMode.ToPoint)
            currentTrack = trackToFollow[0];
        else
        {
            cyclistMode = CyclistMode.Continuous;
            //Add that it automatically selects tracks in here.
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (pointToGoTo != null)
        {
            SetRoute(pointToGoTo);
            pointToGoTo = null;
        }
        if (locationToGoTo != null)
        {
            SetRoute(locationToGoTo.position);
            locationToGoTo = null;
        }
#endif
        UpdatePosition(Time.deltaTime);
    }

    public static void SetRoute(Vector3 targetPosition)
    {
        RoadPath roadPath = PathFinding.GetBestPath(cTF.currentTrack.end, targetPosition, cTF.allPoints);
        if (roadPath != null)
        {
            cTF.trackToFollow.AddRange(roadPath.Roads);
            cTF.cyclistMode = CyclistMode.ToPoint;
        }
    }

    public static void SetRoute(Point targetPoint)
    {
        RoadPath roadPath = PathFinding.GetBestPath(cTF.trackToFollow.Last().end, targetPoint);
        if (roadPath != null)
        {
            cTF.trackToFollow.AddRange(roadPath.Roads);
            cTF.cyclistMode = CyclistMode.ToPoint;
        }
    }

    private void UpdatePosition(float deltaTime)
    {
        if (speed < 0)
            Debug.LogErrorFormat("Yeah this wasn't made for something like that, please just don't do it. Speed was: {0}", speed);

        //Get speed for the time in between frames.
        float targetSpeed = speed * deltaTime;

        //Get the new position where the bike should move to.
        Vector3 targetPos = currentTrack.spline.MoveAlongSpline(ref m_normalizedT, targetSpeed);
        //Create a new current position for later use in the while loop.
        Vector3 currentPos = transform.position;

        //Made into a while loop because of the *FAINT* possibility that tracks may be too close together (which would - by the way - just be awful design).
        while (m_normalizedT >= 1f)
        {
            //Set it right back to zero.
            m_normalizedT = 0;

            //Detract the distance moved already from the target speed.
            targetSpeed -= Vector3.Distance(currentPos, targetPos);
            if (targetSpeed < 0)
                Debug.LogError("targetSpeed should NEVER be below 0!");

            //Reset rotation so it is forced to update, regardless, even though this should actually be redundant, but for the sake of me being a bad programmer, I'll just leave it in to prevent problems.
            lastRotation = Quaternion.identity;

            //Check for the two modes, if there are no further points in ToPoint it will simply switch to the Continuous mode.
            if (cyclistMode == CyclistMode.ToPoint)
            {
                int indexOfCurrentTrack = trackToFollow.GetIndexOfFirst(currentTrack);
                if (trackToFollow.Count > indexOfCurrentTrack + 1) //If there are more roads in the trackToFollow list select the next one.
                {
                    currentTrack = trackToFollow[indexOfCurrentTrack + 1];
                    trackToFollow.RemoveAt(indexOfCurrentTrack);
                }
                else
                {
                    trackToFollow.Clear();
                    cyclistMode = CyclistMode.Continuous;
                }
            }

            //Randomly selects a new road.
            if (cyclistMode == CyclistMode.Continuous)
            {
                //REALLY JANKY BUT OK.
                currentTrack = currentTrack.end.roadsAway.SelectRandom();
            }

            //Get the position that the bike should move to, just like it normally would.
            targetPos = currentTrack.spline.MoveAlongSpline(ref m_normalizedT, targetSpeed);

            //If the new spline is too small for the current distance get the point from which the player had to travel, this will be used again for the distance and targetSpeed calculations.
            if (m_normalizedT >= 1f)
                currentPos = currentTrack.spline.GetPoint(0f);
        }

        //Set the position.
        transform.position = targetPos;

        //Get the segment.
        BezierSpline.Segment segment = currentTrack.spline.GetSegmentAt(m_normalizedT);

        //Set the rotation of the cyclist, if the speed is not sufficient, it will not be changed, unless forced to by being either not initiated the first time
        if (!speed.IsBetween(-0.01f, 0.01f) || lastRotation == Quaternion.identity)
            lastRotation = Quaternion.LookRotation(segment.GetTangent(), segment.GetNormal());

        //Apply look rotation
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lastRotation, maxDegreesLookPerSecond * deltaTime);
    }
}
