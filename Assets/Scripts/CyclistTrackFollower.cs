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
    [SerializeField] private float maxDegreesLookPerSecond = 60f;
    /// <summary>
    /// Speed as meters per second.
    /// </summary>
    public float speed = 5;
    public CyclistMode cyclistMode;
    public Point[] allPoints;
    [SerializeField] private MonoBehaviourOmeter distanceToGo = null;

#if UNITY_EDITOR
    [Header("Navigate To Point")]
    [SerializeField] private Point pointToGoTo = null;
    [SerializeField] private Transform locationToGoTo = null;
#endif

    [Header("Debug")]
    public Road currentTrack = null;
    [SerializeField] private RoadPath trackToFollow = null;
    [SerializeField] private List<Vector3> queuedTargetPosition = new List<Vector3>();
    

    [Range(0f, 1f)]
    public float m_normalizedT = 0f;
    private Quaternion lastRotation = Quaternion.identity;
    [SerializeField] private float lastNormalizedT = 0f;
    [SerializeField] private float distanceToTarget = 0f;
    [SerializeField] private bool isFollowingTrack = false;

    private void Awake()
    {
        if (cTF != null && cTF != this)
            Destroy(cTF);
        cTF = this;
    }

    private void Start()
    {
        if (trackToFollow != null && trackToFollow.Roads != null)
        {
            currentTrack = trackToFollow.Roads[0];
            isFollowingTrack = true;
        }
        else if (queuedTargetPosition.Count > 0)
        {
            cyclistMode = CyclistMode.ToPoint;
            trackToFollow = PathFinding.GetBestPath(new RoadPoint() { normalizedT = m_normalizedT, road = currentTrack }, queuedTargetPosition[0], allPoints);
            queuedTargetPosition.RemoveAt(0);
            isFollowingTrack = true;
        }
        else
        {
            cyclistMode = CyclistMode.Continuous;
            //currentTrack = allPoints[0].roadsAway[0];
            trackToFollow = null;
            //Add that it automatically selects tracks in here.
            isFollowingTrack = false;
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
        cTF.QueueRoute(targetPosition);
        //RoadPath roadPath = PathFinding.GetBestPath(cTF.currentTrack.end, targetPosition, cTF.allPoints);
        //if (roadPath != null)
        //{
        //    cTF.trackToFollow.AddRange(roadPath.Roads);
        //    cTF.cyclistMode = CyclistMode.ToPoint;
        //}
    }

    public static void SetRoute(Point targetPoint)
    {
        cTF.QueueRoute(targetPoint.transform.position);
        //RoadPath roadPath = PathFinding.GetBestPath(cTF.trackToFollow.Last().end, targetPoint);
        //if (roadPath != null)
        //{
        //    cTF.trackToFollow.AddRange(roadPath.Roads);
        //    cTF.cyclistMode = CyclistMode.ToPoint;
        //}
    }

    private void QueueRoute(Vector3 targetPosition)
    {
        queuedTargetPosition.Add(targetPosition);
        cyclistMode = CyclistMode.ToPoint;
        if (isFollowingTrack == false)
        {
            trackToFollow = PathFinding.GetBestPath(new RoadPoint() { normalizedT = m_normalizedT, road = currentTrack }, queuedTargetPosition[0], allPoints);
            queuedTargetPosition.RemoveAt(0);
        }
        isFollowingTrack = true;
    }

    private void UpdatePosition(float deltaTime)
    {
        //Check for not being on track.

        if (speed < 0)
            Debug.LogErrorFormat("Yeah this wasn't made for something like that, please just don't do it. Speed was: {0}", speed);

        //Get speed for the time in between frames.
        float targetSpeed = speed * deltaTime;

        //Get the new position where the bike should move to.
        Vector3 targetPos = currentTrack.spline.MoveAlongSpline(ref m_normalizedT, targetSpeed);

        if (cyclistMode == CyclistMode.ToPoint && isFollowingTrack && trackToFollow.endPoint.road == currentTrack && m_normalizedT >= trackToFollow.endPoint.normalizedT)
        {
            trackToFollow = null;
            isFollowingTrack = false;
            if (queuedTargetPosition.Count > 0) //Aka the track follow count = 0 && we do have a target in the queue.
            {
                //This is where we activate the navigation thing. BUT I am not going to do that right now, because I am lazy.
                RoadPath roadPath = PathFinding.GetBestPath(new RoadPoint() { normalizedT = m_normalizedT, road = currentTrack }, queuedTargetPosition[0], allPoints);
                trackToFollow = roadPath;
                queuedTargetPosition.RemoveAt(0);
                isFollowingTrack = true;
            }
        }

        OnNormalizedTAbove1(targetSpeed, ref targetPos);

        //Set the position.
        transform.position = targetPos;

        //Get the segment for the rotation
        BezierSpline.Segment segment = currentTrack.spline.GetSegmentAt(m_normalizedT);
        //Set the rotation of the cyclist, if the speed is not sufficient, it will not be changed, unless forced to by being not initiated the first time or reset to Quaternion.identity
        if (!speed.IsBetween(-0.01f, 0.01f) || lastRotation == Quaternion.identity)
            lastRotation = Quaternion.LookRotation(segment.GetTangent(), segment.GetNormal());
        //Apply look rotation
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lastRotation, maxDegreesLookPerSecond * deltaTime);

        //Calculate distance to point.
        if (cyclistMode == CyclistMode.ToPoint && trackToFollow != null)
        {
            if (trackToFollow.length <= 0)
            {
                trackToFollow.length = 0;
                distanceToTarget = 0;
            }
            else
            {
                trackToFollow.length -= targetSpeed;
                distanceToTarget = trackToFollow.length;
            }
            distanceToGo.SetValue(Mathf.Floor(distanceToTarget/100)/10);
        }

        lastNormalizedT = m_normalizedT;
    }

    private void OnNormalizedTAbove1(float targetSpeed, ref Vector3 targetPos)
    {
        //Made into a while loop because of the *FAINT* possibility that tracks may be too close together (which would - by the way - just be awful design).
        while (m_normalizedT >= 1f)
        {
            //Set it right back to zero.
            m_normalizedT = 0;

            //Detract the distance moved already from the target speed.
            targetSpeed -= currentTrack.spline.GetLengthApproximately(lastNormalizedT, 1);
            if (targetSpeed < 0)
                Debug.LogError("targetSpeed should NEVER be below 0!");

            //Reset rotation so it is forced to update, regardless, even though this should actually be redundant. But for the sake of me being a bad programmer, I'll just leave it, just to prevent problems.
            lastRotation = Quaternion.identity;

            //Check for the two modes, if there are no further points in ToPoint it will simply switch to the Continuous mode.
            if (cyclistMode == CyclistMode.ToPoint)
            {
                if (trackToFollow != null && trackToFollow.Roads != null && trackToFollow.Roads.Count > 0) //If there are more roads in the trackToFollow list select the next one.
                {
                    if (!currentTrack.end.roadsAway.Contains(trackToFollow.Roads[0]))
                        Debug.LogError("Trying to go to a track that is not connected... (aka the player will jump...)");
                    currentTrack = trackToFollow.Roads[0];
                    //Removes the track that's now selected from the list.
                    trackToFollow.Roads.RemoveAt(0);
                    isFollowingTrack = true;
                }
                else if (queuedTargetPosition.Count > 0) //Aka the track follow count = 0 && we do have a target in the queue.
                {
                    //This is where we activate the navigation thing. BUT I am not going to do that right now, because I am lazy.
                    RoadPath roadPath = PathFinding.GetBestPath(new RoadPoint() { normalizedT = m_normalizedT, road = currentTrack }, queuedTargetPosition[0], allPoints);
                    trackToFollow = roadPath;
                    queuedTargetPosition.RemoveAt(0);
                    isFollowingTrack = true;
                }
                else
                {
                    isFollowingTrack = false;
                    cyclistMode = CyclistMode.Continuous;
                }

                lastNormalizedT = 0;
            }

            //Randomly selects a new road.
            if (cyclistMode == CyclistMode.Continuous)
                currentTrack = currentTrack.end.roadsAway.SelectRandom(); //REALLY JANKY BUT OK.

            //Get the position that the bike should move to, just like it normally would.
            targetPos = currentTrack.spline.MoveAlongSpline(ref m_normalizedT, targetSpeed);
        }
    }
}
