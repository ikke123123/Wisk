﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_2017_3_OR_NEWER
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo( "BezierSolution.Editor" )]
#else
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo( "Assembly-CSharp-Editor" )]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo( "Assembly-CSharp-Editor-firstpass" )]
#endif
namespace BezierSolution
{
	public enum SplineAutoConstructMode { None = 0, Linear = 1, Smooth1 = 2, Smooth2 = 3 };

	[System.Flags]
	internal enum InternalDirtyFlags
	{
		None = 0,
		EndPointTransformChange = 1 << 1,
		ControlPointPositionChange = 1 << 2,
		NormalChange = 1 << 3,
		NormalOffsetChange = 1 << 4,
		ExtraDataChange = 1 << 5,
		All = EndPointTransformChange | ControlPointPositionChange | NormalChange | NormalOffsetChange | ExtraDataChange
	};

	[System.Flags]
	public enum DirtyFlags
	{
		None = 0,
		SplineShapeChanged = 1 << 1,
		NormalsChanged = 1 << 2,
		ExtraDataChanged = 1 << 3,
		All = SplineShapeChanged | NormalsChanged | ExtraDataChanged
	};

	public delegate void SplineChangeDelegate( BezierSpline spline, DirtyFlags dirtyFlags );

	[AddComponentMenu( "Bezier Solution/Bezier Spline" )]
	[ExecuteInEditMode]
	public class BezierSpline : MonoBehaviour, IEnumerable<BezierPoint>
	{
		public struct Segment
		{
			public readonly BezierPoint point1, point2;
			public readonly float localT;

			public Segment( BezierPoint point1, BezierPoint point2, float localT )
			{
				this.point1 = point1;
				this.point2 = point2;
				this.localT = localT;
			}

			public float GetNormalizedT()
			{
				return GetNormalizedT( localT );
			}

			public float GetNormalizedT( float localT )
			{
				BezierSpline spline = point1.spline;
				return ( point1.index + localT ) / ( spline.m_loop ? spline.Count : ( spline.Count - 1 ) );
			}

			public Vector3 GetPoint()
			{
				return GetPoint( localT );
			}

			public Vector3 GetPoint( float localT )
			{
				float oneMinusLocalT = 1f - localT;

				return oneMinusLocalT * oneMinusLocalT * oneMinusLocalT * point1.position +
					   3f * oneMinusLocalT * oneMinusLocalT * localT * point1.followingControlPointPosition +
					   3f * oneMinusLocalT * localT * localT * point2.precedingControlPointPosition +
					   localT * localT * localT * point2.position;
			}

			public Vector3 GetTangent()
			{
				return GetTangent( localT );
			}

			public Vector3 GetTangent( float localT )
			{
				float oneMinusLocalT = 1f - localT;

				return 3f * oneMinusLocalT * oneMinusLocalT * ( point1.followingControlPointPosition - point1.position ) +
					   6f * oneMinusLocalT * localT * ( point2.precedingControlPointPosition - point1.followingControlPointPosition ) +
					   3f * localT * localT * ( point2.position - point2.precedingControlPointPosition );
			}

			public Vector3 GetNormal()
			{
				return GetNormal( localT );
			}

			public Vector3 GetNormal( float localT )
			{
				Vector3 startNormal = point1.normal;
				Vector3 endNormal = point2.normal;

				Vector3 normal = Vector3.LerpUnclamped( startNormal, endNormal, localT );
				if( normal.y == 0f && normal.x == 0f && normal.z == 0f )
				{
					// Don't return Vector3.zero as normal
					normal = Vector3.LerpUnclamped( startNormal, endNormal, localT > 0.01f ? ( localT - 0.01f ) : ( localT + 0.01f ) );
					if( normal.y == 0f && normal.x == 0f && normal.z == 0f )
						normal = Vector3.up;
				}

				return normal;
			}

			public BezierPoint.ExtraData GetExtraData()
			{
				return defaultExtraDataLerpFunction( point1.extraData, point2.extraData, localT );
			}

			public BezierPoint.ExtraData GetExtraData( float localT )
			{
				return defaultExtraDataLerpFunction( point1.extraData, point2.extraData, localT );
			}

			public BezierPoint.ExtraData GetExtraData( ExtraDataLerpFunction lerpFunction )
			{
				return lerpFunction( point1.extraData, point2.extraData, localT );
			}

			public BezierPoint.ExtraData GetExtraData( float localT, ExtraDataLerpFunction lerpFunction )
			{
				return lerpFunction( point1.extraData, point2.extraData, localT );
			}
		}

		public struct EvenlySpacedPointsHolder
		{
			public readonly BezierSpline spline;
			public readonly float splineLength;
			public readonly float[] uniformNormalizedTs;

			public EvenlySpacedPointsHolder( BezierSpline spline, float splineLength, float[] uniformNormalizedTs )
			{
				this.spline = spline;
				this.splineLength = splineLength;
				this.uniformNormalizedTs = uniformNormalizedTs;
			}

			public float GetNormalizedTAtPercentage( float percentage )
			{
				if( !spline.loop )
				{
					if( percentage <= 0f )
						return 0f;
					else if( percentage >= 1f )
						return 1f;
				}
				else
				{
					while( percentage < 0f )
						percentage += 1f;
					while( percentage >= 1f )
						percentage -= 1f;
				}

				float indexRaw = ( uniformNormalizedTs.Length - 1 ) * percentage;
				int index = (int) indexRaw;
				return Mathf.LerpUnclamped( uniformNormalizedTs[index], uniformNormalizedTs[index + 1], indexRaw - index );
			}

			public float GetNormalizedTAtDistance( float distance )
			{
				return GetNormalizedTAtPercentage( distance / splineLength );
			}

			public float GetPercentageAtNormalizedT( float normalizedT )
			{
				if( !spline.loop )
				{
					if( normalizedT <= 0f )
						return 0f;
					else if( normalizedT >= 1f )
						return 1f;
				}
				else
				{
					if( normalizedT < 0f )
						normalizedT += 1f;
					if( normalizedT >= 1f )
						normalizedT -= 1f;
				}

				// Perform binary search
				int lowerBound = 0;
				int upperBound = uniformNormalizedTs.Length - 1;
				while( lowerBound <= upperBound )
				{
					int index = lowerBound + ( ( upperBound - lowerBound ) >> 1 );
					float arrElement = uniformNormalizedTs[index];
					if( arrElement < normalizedT )
						lowerBound = index + 1;
					else if( arrElement > normalizedT )
						upperBound = index - 1;
					else
						return index / (float) ( uniformNormalizedTs.Length - 1 );
				}

				float inverseLerp = ( normalizedT - uniformNormalizedTs[lowerBound] ) / ( uniformNormalizedTs[lowerBound - 1] - uniformNormalizedTs[lowerBound] );
				return ( lowerBound - inverseLerp ) / ( uniformNormalizedTs.Length - 1 );
			}
		}

		public delegate BezierPoint.ExtraData ExtraDataLerpFunction( BezierPoint.ExtraData data1, BezierPoint.ExtraData data2, float normalizedT );

		private static readonly ExtraDataLerpFunction defaultExtraDataLerpFunction = BezierPoint.ExtraData.LerpUnclamped;
		private static Material gizmoMaterial;

		internal List<BezierPoint> endPoints = new List<BezierPoint>(); // This is not readonly because otherwise BezierWalkers' "Simulate In Editor" functionality may break after recompilation

		public int Count { get { return endPoints.Count; } }
		public BezierPoint this[int index] { get { return endPoints[index]; } }

		private float? m_length = null;
		public float length
		{
			get
			{
				if( m_length == null )
					m_length = GetLengthApproximately( 0f, 1f );

				return m_length.Value;
			}
		}

		[System.Obsolete( "Length is renamed to length" )]
		public float Length { get { return length; } }

		[SerializeField, HideInInspector]
		[UnityEngine.Serialization.FormerlySerializedAs( "loop" )]
		private bool m_loop = false;
		public bool loop
		{
			get { return m_loop; }
			set
			{
				if( m_loop != value )
				{
					m_loop = value;
					dirtyFlags |= InternalDirtyFlags.All;
				}
			}
		}

		public bool drawGizmos = false;
		public Color gizmoColor = Color.white;
		[UnityEngine.Serialization.FormerlySerializedAs( "m_gizmoSmoothness" )]
		public int gizmoSmoothness = 4;

		[SerializeField, HideInInspector]
		[UnityEngine.Serialization.FormerlySerializedAs( "Internal_AutoConstructMode" )]
		private SplineAutoConstructMode m_autoConstructMode = SplineAutoConstructMode.None;
		public SplineAutoConstructMode autoConstructMode
		{
			get { return m_autoConstructMode; }
			set
			{
				if( m_autoConstructMode != value )
				{
					m_autoConstructMode = value;

					if( value != SplineAutoConstructMode.None )
						dirtyFlags |= InternalDirtyFlags.EndPointTransformChange | InternalDirtyFlags.ControlPointPositionChange;
				}
			}
		}

		[SerializeField, HideInInspector]
		[UnityEngine.Serialization.FormerlySerializedAs( "Internal_AutoCalculateNormals" )]
		private bool m_autoCalculateNormals = false;
		public bool autoCalculateNormals
		{
			get { return m_autoCalculateNormals; }
			set
			{
				if( m_autoCalculateNormals != value )
				{
					m_autoCalculateNormals = value;
					dirtyFlags |= InternalDirtyFlags.NormalOffsetChange;
				}
			}
		}

		[SerializeField, HideInInspector]
		[UnityEngine.Serialization.FormerlySerializedAs( "Internal_AutoCalculatedNormalsAngle" )]
		private float m_autoCalculatedNormalsAngle = 0f;
		public float autoCalculatedNormalsAngle
		{
			get { return m_autoCalculatedNormalsAngle; }
			set
			{
				if( m_autoCalculatedNormalsAngle != value )
				{
					m_autoCalculatedNormalsAngle = value;
					dirtyFlags |= InternalDirtyFlags.NormalOffsetChange;
				}
			}
		}

		private EvenlySpacedPointsHolder? m_evenlySpacedPoints = null;
		public EvenlySpacedPointsHolder evenlySpacedPoints
		{
			get
			{
				if( m_evenlySpacedPoints == null )
					m_evenlySpacedPoints = CalculateEvenlySpacedPoints();

				return m_evenlySpacedPoints.Value;
			}
		}

		public event SplineChangeDelegate onSplineChanged;

		internal InternalDirtyFlags dirtyFlags;

		private void Awake()
		{
			Refresh();
		}

#if UNITY_EDITOR
		private void OnTransformChildrenChanged()
		{
			dirtyFlags |= InternalDirtyFlags.All;
			Refresh();
		}
#endif

		private void LateUpdate()
		{
			CheckDirty();
		}

		internal void CheckDirty()
		{
			for( int i = 0; i < endPoints.Count; i++ )
				endPoints[i].RefreshIfChanged();

			if( dirtyFlags != InternalDirtyFlags.None && endPoints.Count >= 2 )
			{
				DirtyFlags publishedDirtyFlags = DirtyFlags.None;

				if( ( dirtyFlags & InternalDirtyFlags.ExtraDataChange ) == InternalDirtyFlags.ExtraDataChange )
					publishedDirtyFlags |= DirtyFlags.ExtraDataChanged;

				if( ( dirtyFlags & ( InternalDirtyFlags.EndPointTransformChange | InternalDirtyFlags.ControlPointPositionChange ) ) != InternalDirtyFlags.None )
				{
					if( m_autoConstructMode == SplineAutoConstructMode.None )
						publishedDirtyFlags |= DirtyFlags.SplineShapeChanged;
					else
					{
						switch( m_autoConstructMode )
						{
							case SplineAutoConstructMode.Linear: ConstructLinearPath(); break;
							case SplineAutoConstructMode.Smooth1: AutoConstructSpline(); break;
							case SplineAutoConstructMode.Smooth2: AutoConstructSpline2(); break;
						}

						// If a control point position was changed only, we've reverted that change by auto constructing the spline again
						dirtyFlags &= ~InternalDirtyFlags.ControlPointPositionChange;

						// If an end point's position was changed, then the spline's shape has indeed changed
						if( ( dirtyFlags & InternalDirtyFlags.EndPointTransformChange ) == InternalDirtyFlags.EndPointTransformChange )
							publishedDirtyFlags |= DirtyFlags.SplineShapeChanged;
					}
				}

				if( ( dirtyFlags & ( InternalDirtyFlags.NormalChange | InternalDirtyFlags.NormalOffsetChange | InternalDirtyFlags.EndPointTransformChange | InternalDirtyFlags.ControlPointPositionChange ) ) != InternalDirtyFlags.None )
				{
					if( !m_autoCalculateNormals )
					{
						// Normals are actually changed only when NormalChange flag is on
						if( ( dirtyFlags & InternalDirtyFlags.NormalChange ) == InternalDirtyFlags.NormalChange )
							publishedDirtyFlags |= DirtyFlags.NormalsChanged;
					}
					else
					{
						AutoCalculateNormals( m_autoCalculatedNormalsAngle );

						// If an end point's normal vector was changed only, we've reverted that change by auto calculating the normals again
						dirtyFlags &= ~InternalDirtyFlags.NormalChange;

						// If an end point's position or normal calculation offset was changed, then the spline's normals have indeed changed
						if( ( dirtyFlags & ( InternalDirtyFlags.NormalOffsetChange | InternalDirtyFlags.EndPointTransformChange | InternalDirtyFlags.ControlPointPositionChange ) ) != InternalDirtyFlags.None )
							publishedDirtyFlags |= DirtyFlags.NormalsChanged;
					}
				}

				if( ( publishedDirtyFlags & DirtyFlags.SplineShapeChanged ) == DirtyFlags.SplineShapeChanged )
				{
					m_length = null;
					m_evenlySpacedPoints = null;
				}

				if( onSplineChanged != null )
				{
					try
					{
						onSplineChanged( this, publishedDirtyFlags );
					}
					catch( System.Exception e )
					{
						Debug.LogException( e );
					}
				}
			}

			dirtyFlags = InternalDirtyFlags.None;
		}

		public void Initialize( int endPointsCount )
		{
			if( endPointsCount < 2 )
			{
				Debug.LogError( "Can't initialize spline with " + endPointsCount + " point(s). At least 2 points are needed" );
				return;
			}

			// Destroy current end points
			endPoints.Clear();
			GetComponentsInChildren( endPoints );

			for( int i = endPoints.Count - 1; i >= 0; i-- )
				DestroyImmediate( endPoints[i].gameObject );

			// Create new end points
			endPoints.Clear();

			for( int i = 0; i < endPointsCount; i++ )
				InsertNewPointAt( i );

			Refresh();
		}

		public void Refresh()
		{
			endPoints.Clear();
			GetComponentsInChildren( endPoints );

			for( int i = 0; i < endPoints.Count; i++ )
			{
				endPoints[i].spline = this;
				endPoints[i].index = i;
			}

			CheckDirty();
		}

		public BezierPoint InsertNewPointAt( int index )
		{
			if( index < 0 || index > endPoints.Count )
			{
				Debug.LogError( "Index " + index + " is out of range: [0," + endPoints.Count + "]" );
				return null;
			}

			int prevCount = endPoints.Count;
			BezierPoint point = new GameObject( "Point" ).AddComponent<BezierPoint>();
			point.spline = this;

			Transform parent = endPoints.Count == 0 ? transform : ( index == 0 ? endPoints[0].transform.parent : endPoints[index - 1].transform.parent );
			int siblingIndex = index == 0 ? 0 : endPoints[index - 1].transform.GetSiblingIndex() + 1;
			point.transform.SetParent( parent, false );
			point.transform.SetSiblingIndex( siblingIndex );

			if( endPoints.Count == prevCount ) // If spline isn't automatically Refresh()'ed
				endPoints.Insert( index, point );

			for( int i = index; i < endPoints.Count; i++ )
				endPoints[i].index = i;

			dirtyFlags |= InternalDirtyFlags.All;

			return point;
		}

		public BezierPoint DuplicatePointAt( int index )
		{
			if( index < 0 || index >= endPoints.Count )
			{
				Debug.LogError( "Index " + index + " is out of range: [0," + ( endPoints.Count - 1 ) + "]" );
				return null;
			}

			BezierPoint newPoint = InsertNewPointAt( index + 1 );
			endPoints[index].CopyTo( newPoint );

			return newPoint;
		}

		public void RemovePointAt( int index )
		{
			if( endPoints.Count <= 2 )
			{
				Debug.LogError( "Can't remove point: spline must consist of at least two points!" );
				return;
			}

			if( index < 0 || index >= endPoints.Count )
			{
				Debug.LogError( "Index " + index + " is out of range: [0," + endPoints.Count + ")" );
				return;
			}

			BezierPoint point = endPoints[index];
			endPoints.RemoveAt( index );

			for( int i = index; i < endPoints.Count; i++ )
				endPoints[i].index = i;

			DestroyImmediate( point.gameObject );

			dirtyFlags |= InternalDirtyFlags.All;
		}

		public void SwapPointsAt( int index1, int index2 )
		{
			if( index1 == index2 )
				return;

			if( index1 < 0 || index1 >= endPoints.Count || index2 < 0 || index2 >= endPoints.Count )
			{
				Debug.LogError( "Indices must be in range [0," + ( endPoints.Count - 1 ) + "]" );
				return;
			}

			BezierPoint point1 = endPoints[index1];
			BezierPoint point2 = endPoints[index2];

			int point1SiblingIndex = point1.transform.GetSiblingIndex();
			int point2SiblingIndex = point2.transform.GetSiblingIndex();

			Transform point1Parent = point1.transform.parent;
			Transform point2Parent = point2.transform.parent;

			endPoints[index1] = point2;
			endPoints[index2] = point1;

			point1.index = index2;
			point2.index = index1;

			if( point1Parent != point2Parent )
			{
				point1.transform.SetParent( point2Parent, true );
				point2.transform.SetParent( point1Parent, true );
			}

			point1.transform.SetSiblingIndex( point2SiblingIndex );
			point2.transform.SetSiblingIndex( point1SiblingIndex );

			dirtyFlags |= InternalDirtyFlags.All;
		}

		public void ChangePointIndex( int previousIndex, int newIndex )
		{
			ChangePointIndex( previousIndex, newIndex, null );
		}

		internal void ChangePointIndex( int previousIndex, int newIndex, string undo )
		{
			if( previousIndex == newIndex )
				return;

			if( previousIndex < 0 || previousIndex >= endPoints.Count || newIndex < 0 || newIndex >= endPoints.Count )
			{
				Debug.LogError( "Indices must be in range [0," + ( endPoints.Count - 1 ) + "]" );
				return;
			}

			BezierPoint point1 = endPoints[previousIndex];
			BezierPoint point2 = endPoints[newIndex];

			if( previousIndex < newIndex )
			{
				for( int i = previousIndex; i < newIndex; i++ )
					endPoints[i] = endPoints[i + 1];
			}
			else
			{
				for( int i = previousIndex; i > newIndex; i-- )
					endPoints[i] = endPoints[i - 1];
			}

			endPoints[newIndex] = point1;

			Transform point2Parent = point2.transform.parent;
			if( point1.transform.parent != point2Parent )
			{
#if UNITY_EDITOR
				if( undo != null )
				{
					UnityEditor.Undo.SetTransformParent( point1.transform, point2Parent, undo );
					UnityEditor.Undo.RegisterCompleteObjectUndo( point2Parent, undo );
				}
				else
#endif
					point1.transform.SetParent( point2Parent, true );

				int point2SiblingIndex = point2.transform.GetSiblingIndex();
				if( previousIndex < newIndex )
				{
					if( point1.transform.GetSiblingIndex() < point2SiblingIndex )
						point1.transform.SetSiblingIndex( point2SiblingIndex );
					else
						point1.transform.SetSiblingIndex( point2SiblingIndex + 1 );
				}
				else
				{
					if( point1.transform.GetSiblingIndex() < point2SiblingIndex )
						point1.transform.SetSiblingIndex( point2SiblingIndex - 1 );
					else
						point1.transform.SetSiblingIndex( point2SiblingIndex );
				}
			}
			else
				point1.transform.SetSiblingIndex( point2.transform.GetSiblingIndex() );

			for( int i = 0; i < endPoints.Count; i++ )
				endPoints[i].index = i;

			dirtyFlags |= InternalDirtyFlags.All;
		}

		public Vector3 GetPoint( float normalizedT )
		{
			if( !m_loop )
			{
				if( normalizedT <= 0f )
					return endPoints[0].position;
				else if( normalizedT >= 1f )
					return endPoints[endPoints.Count - 1].position;
			}
			else
			{
				// 2nd conditions isn't 'else if' because in rare occasions, floating point precision issues may arise; e.g. for normalizedT = -0.0000000149,
				// incrementing the value by 1 results in perfect 1.0000000000 with no mantissa
				if( normalizedT < 0f )
					normalizedT += 1f;
				if( normalizedT >= 1f )
					normalizedT -= 1f;
			}

			float t = normalizedT * ( m_loop ? endPoints.Count : ( endPoints.Count - 1 ) );

			BezierPoint startPoint, endPoint;

			int startIndex = (int) t;
			int endIndex = startIndex + 1;

			if( endIndex == endPoints.Count )
				endIndex = 0;

			startPoint = endPoints[startIndex];
			endPoint = endPoints[endIndex];

			float localT = t - startIndex;
			float oneMinusLocalT = 1f - localT;

			return oneMinusLocalT * oneMinusLocalT * oneMinusLocalT * startPoint.position +
				   3f * oneMinusLocalT * oneMinusLocalT * localT * startPoint.followingControlPointPosition +
				   3f * oneMinusLocalT * localT * localT * endPoint.precedingControlPointPosition +
				   localT * localT * localT * endPoint.position;
		}

		public Vector3 GetTangent( float normalizedT )
		{
			if( !m_loop )
			{
				if( normalizedT <= 0f )
					return 3f * ( endPoints[0].followingControlPointPosition - endPoints[0].position );
				else if( normalizedT >= 1f )
				{
					int index = endPoints.Count - 1;
					return 3f * ( endPoints[index].position - endPoints[index].precedingControlPointPosition );
				}
			}
			else
			{
				if( normalizedT < 0f )
					normalizedT += 1f;
				if( normalizedT >= 1f )
					normalizedT -= 1f;
			}

			float t = normalizedT * ( m_loop ? endPoints.Count : ( endPoints.Count - 1 ) );

			BezierPoint startPoint, endPoint;

			int startIndex = (int) t;
			int endIndex = startIndex + 1;

			if( endIndex == endPoints.Count )
				endIndex = 0;

			startPoint = endPoints[startIndex];
			endPoint = endPoints[endIndex];

			float localT = t - startIndex;
			float oneMinusLocalT = 1f - localT;

            return 3f * oneMinusLocalT * oneMinusLocalT * (startPoint.followingControlPointPosition - startPoint.position) +
                   6f * oneMinusLocalT * localT * (endPoint.precedingControlPointPosition - startPoint.followingControlPointPosition) +
                   3f * localT * localT * (endPoint.position - endPoint.precedingControlPointPosition);
		}

		public Vector3 GetNormal( float normalizedT )
		{
			if( !m_loop )
			{
				if( normalizedT <= 0f )
					return endPoints[0].normal;
				else if( normalizedT >= 1f )
					return endPoints[endPoints.Count - 1].normal;
			}
			else
			{
				if( normalizedT < 0f )
					normalizedT += 1f;
				if( normalizedT >= 1f )
					normalizedT -= 1f;
			}

			float t = normalizedT * ( m_loop ? endPoints.Count : ( endPoints.Count - 1 ) );

			int startIndex = (int) t;
			int endIndex = startIndex + 1;

			if( endIndex == endPoints.Count )
				endIndex = 0;

			Vector3 startNormal = endPoints[startIndex].normal;
			Vector3 endNormal = endPoints[endIndex].normal;

			float localT = t - startIndex;

			Vector3 normal = Vector3.LerpUnclamped( startNormal, endNormal, localT );
			if( normal.y == 0f && normal.x == 0f && normal.z == 0f )
			{
				// Don't return Vector3.zero as normal
				normal = Vector3.LerpUnclamped( startNormal, endNormal, localT > 0.01f ? ( localT - 0.01f ) : ( localT + 0.01f ) );
				if( normal.y == 0f && normal.x == 0f && normal.z == 0f )
					normal = Vector3.up;
			}

			return normal;
		}

		public BezierPoint.ExtraData GetExtraData( float normalizedT )
		{
			return GetExtraData( normalizedT, defaultExtraDataLerpFunction );
		}

		public BezierPoint.ExtraData GetExtraData( float normalizedT, ExtraDataLerpFunction lerpFunction )
		{
			if( !m_loop )
			{
				if( normalizedT <= 0f )
					return endPoints[0].extraData;
				else if( normalizedT >= 1f )
					return endPoints[endPoints.Count - 1].extraData;
			}
			else
			{
				if( normalizedT < 0f )
					normalizedT += 1f;
				if( normalizedT >= 1f )
					normalizedT -= 1f;
			}

			float t = normalizedT * ( m_loop ? endPoints.Count : ( endPoints.Count - 1 ) );

			int startIndex = (int) t;
			int endIndex = startIndex + 1;

			if( endIndex == endPoints.Count )
				endIndex = 0;

			return lerpFunction( endPoints[startIndex].extraData, endPoints[endIndex].extraData, t - startIndex );
		}

		public float GetLengthApproximately( float startNormalizedT, float endNormalizedT, float accuracy = 50f )
		{
			if( endNormalizedT < startNormalizedT )
			{
				float temp = startNormalizedT;
				startNormalizedT = endNormalizedT;
				endNormalizedT = temp;
			}

			if( startNormalizedT < 0f )
				startNormalizedT = 0f;

			if( endNormalizedT > 1f )
				endNormalizedT = 1f;

			float step = AccuracyToStepSize( accuracy ) * ( endNormalizedT - startNormalizedT );

			float length = 0f;
			Vector3 lastPoint = GetPoint( startNormalizedT );
			for( float i = startNormalizedT + step; i < endNormalizedT; i += step )
			{
				Vector3 thisPoint = GetPoint( i );
				length += Vector3.Distance( thisPoint, lastPoint );
				lastPoint = thisPoint;
			}

			length += Vector3.Distance( lastPoint, GetPoint( endNormalizedT ) );

			return length;
		}

		public Segment GetSegmentAt( float normalizedT )
		{
			if( !m_loop )
			{
				if( normalizedT <= 0f )
					return new Segment( endPoints[0], endPoints[1], 0f );
				else if( normalizedT >= 1f )
					return new Segment( endPoints[endPoints.Count - 2], endPoints[endPoints.Count - 1], 1f );
			}
			else
			{
				if( normalizedT < 0f )
					normalizedT += 1f;
				if( normalizedT >= 1f )
					normalizedT -= 1f;
			}

			float t = normalizedT * ( m_loop ? endPoints.Count : ( endPoints.Count - 1 ) );

			int startIndex = (int) t;
			int endIndex = startIndex + 1;

			if( endIndex == endPoints.Count )
				endIndex = 0;

			return new Segment( endPoints[startIndex], endPoints[endIndex], t - startIndex );
		}

		[System.Obsolete( "GetNearestPointIndicesTo is renamed to GetSegmentAt" )]
		public Segment GetNearestPointIndicesTo( float normalizedT )
		{
			return GetSegmentAt( normalizedT );
		}

		public Vector3 FindNearestPointTo( Vector3 worldPos, float accuracy = 100f )
		{
			float normalizedT;
			return FindNearestPointTo( worldPos, out normalizedT, accuracy );
		}

		public Vector3 FindNearestPointTo( Vector3 worldPos, out float normalizedT, float accuracy = 100f )
		{
			Vector3 result = Vector3.zero;
			normalizedT = -1f;

			float step = AccuracyToStepSize( accuracy );

			float minDistance = Mathf.Infinity;
			for( float i = 0f; i < 1f; i += step )
			{
				Vector3 thisPoint = GetPoint( i );
				float thisDistance = ( worldPos - thisPoint ).sqrMagnitude;
				if( thisDistance < minDistance )
				{
					minDistance = thisDistance;
					result = thisPoint;
					normalizedT = i;
				}
			}

			return result;
		}

		public Vector3 FindNearestPointToLine( Vector3 lineStart, Vector3 lineEnd, float accuracy = 100f )
		{
			Vector3 pointOnLine;
			float normalizedT;
			return FindNearestPointToLine( lineStart, lineEnd, out pointOnLine, out normalizedT, accuracy );
		}

		public Vector3 FindNearestPointToLine( Vector3 lineStart, Vector3 lineEnd, out Vector3 pointOnLine, out float normalizedT, float accuracy = 100f )
		{
			Vector3 result = Vector3.zero;
			pointOnLine = Vector3.zero;
			normalizedT = -1f;

			float step = AccuracyToStepSize( accuracy );

			// Find closest point on line
			// Credit: https://github.com/Unity-Technologies/UnityCsReference/blob/61f92bd79ae862c4465d35270f9d1d57befd1761/Editor/Mono/HandleUtility.cs#L115-L128
			Vector3 lineDirection = lineEnd - lineStart;
			float length = lineDirection.magnitude;
			Vector3 normalizedLineDirection = lineDirection;
			if( length > .000001f )
				normalizedLineDirection /= length;

			float minDistance = Mathf.Infinity;
			for( float i = 0f; i < 1f; i += step )
			{
				Vector3 thisPoint = GetPoint( i );

				// Find closest point on line
				// Credit: https://github.com/Unity-Technologies/UnityCsReference/blob/61f92bd79ae862c4465d35270f9d1d57befd1761/Editor/Mono/HandleUtility.cs#L115-L128
				Vector3 closestPointOnLine = lineStart + normalizedLineDirection * Mathf.Clamp( Vector3.Dot( normalizedLineDirection, thisPoint - lineStart ), 0f, length );

				float thisDistance = ( closestPointOnLine - thisPoint ).sqrMagnitude;
				if( thisDistance < minDistance )
				{
					minDistance = thisDistance;
					result = thisPoint;
					pointOnLine = closestPointOnLine;
					normalizedT = i;
				}
			}

			return result;
		}

		// Credit: https://gamedev.stackexchange.com/a/27138
		public Vector3 MoveAlongSpline( ref float normalizedT, float deltaMovement, int accuracy = 3 )
		{
			float constant = deltaMovement / ( ( m_loop ? endPoints.Count : ( endPoints.Count - 1 ) ) * accuracy );
			for( int i = 0; i < accuracy; i++ )
				normalizedT += constant / GetTangent( normalizedT ).magnitude;

			return GetPoint( normalizedT );
		}

		public void ConstructLinearPath()
		{
			for( int i = 0; i < endPoints.Count; i++ )
			{
				endPoints[i].handleMode = BezierPoint.HandleMode.Free;
				endPoints[i].RefreshIfChanged();
			}

			for( int i = 0; i < endPoints.Count; i++ )
			{
				if( i < endPoints.Count - 1 )
				{
					Vector3 midPoint = ( endPoints[i].position + endPoints[i + 1].position ) * 0.5f;
					endPoints[i].followingControlPointPosition = midPoint;
					endPoints[i + 1].precedingControlPointPosition = midPoint;
				}
				else
				{
					Vector3 midPoint = ( endPoints[i].position + endPoints[0].position ) * 0.5f;
					endPoints[i].followingControlPointPosition = midPoint;
					endPoints[0].precedingControlPointPosition = midPoint;
				}
			}
		}

		// Credit: http://www.codeproject.com/Articles/31859/Draw-a-Smooth-Curve-through-a-Set-of-2D-Points-wit
		public void AutoConstructSpline()
		{
			for( int i = 0; i < endPoints.Count; i++ )
			{
				endPoints[i].handleMode = BezierPoint.HandleMode.Mirrored;
				endPoints[i].RefreshIfChanged();
			}

			int n = endPoints.Count - 1;
			if( n == 1 )
			{
				endPoints[0].followingControlPointPosition = ( 2 * endPoints[0].position + endPoints[1].position ) / 3f;
				endPoints[1].precedingControlPointPosition = 2 * endPoints[0].followingControlPointPosition - endPoints[0].position;

				return;
			}

			Vector3[] rhs;
			if( m_loop )
				rhs = new Vector3[n + 1];
			else
				rhs = new Vector3[n];

			for( int i = 1; i < n - 1; i++ )
				rhs[i] = 4 * endPoints[i].position + 2 * endPoints[i + 1].position;

			rhs[0] = endPoints[0].position + 2 * endPoints[1].position;

			if( !m_loop )
				rhs[n - 1] = ( 8 * endPoints[n - 1].position + endPoints[n].position ) * 0.5f;
			else
			{
				rhs[n - 1] = 4 * endPoints[n - 1].position + 2 * endPoints[n].position;
				rhs[n] = ( 8 * endPoints[n].position + endPoints[0].position ) * 0.5f;
			}

			// Get first control points
			int rhsLength = rhs.Length;
			Vector3[] controlPoints = new Vector3[rhsLength]; // Solution vector
			float[] tmp = new float[rhsLength]; // Temp workspace

			float b = 2f;
			controlPoints[0] = rhs[0] / b;
			for( int i = 1; i < rhsLength; i++ ) // Decomposition and forward substitution
			{
				float val = 1f / b;
				tmp[i] = val;
				b = ( i < rhsLength - 1 ? 4f : 3.5f ) - val;
				controlPoints[i] = ( rhs[i] - controlPoints[i - 1] ) / b;
			}

			for( int i = 1; i < rhsLength; i++ )
				controlPoints[rhsLength - i - 1] -= tmp[rhsLength - i] * controlPoints[rhsLength - i]; // Backsubstitution

			for( int i = 0; i < n; i++ )
			{
				// First control point
				endPoints[i].followingControlPointPosition = controlPoints[i];

				if( m_loop )
					endPoints[i + 1].precedingControlPointPosition = 2 * endPoints[i + 1].position - controlPoints[i + 1];
				else
				{
					// Second control point
					if( i < n - 1 )
						endPoints[i + 1].precedingControlPointPosition = 2 * endPoints[i + 1].position - controlPoints[i + 1];
					else
						endPoints[i + 1].precedingControlPointPosition = ( endPoints[n].position + controlPoints[n - 1] ) * 0.5f;
				}
			}

			if( m_loop )
			{
				float controlPointDistance = Vector3.Distance( endPoints[0].followingControlPointPosition, endPoints[0].position );
				Vector3 direction = Vector3.Normalize( endPoints[n].position - endPoints[1].position );
				endPoints[0].precedingControlPointPosition = endPoints[0].position + direction * controlPointDistance;
				endPoints[0].followingControlPointLocalPosition = -endPoints[0].precedingControlPointLocalPosition;
			}
		}

		// Credit: http://stackoverflow.com/questions/3526940/how-to-create-a-cubic-bezier-curve-when-given-n-points-in-3d
		public void AutoConstructSpline2()
		{
			// This method doesn't work well with 2 end poins
			if( endPoints.Count == 2 )
			{
				AutoConstructSpline();
				return;
			}

			for( int i = 0; i < endPoints.Count; i++ )
			{
				endPoints[i].handleMode = BezierPoint.HandleMode.Mirrored;
				endPoints[i].RefreshIfChanged();
			}

			for( int i = 0; i < endPoints.Count; i++ )
			{
				Vector3 pMinus1, p1, p2;

				if( i == 0 )
				{
					if( m_loop )
						pMinus1 = endPoints[endPoints.Count - 1].position;
					else
						pMinus1 = endPoints[0].position;
				}
				else
					pMinus1 = endPoints[i - 1].position;

				if( m_loop )
				{
					p1 = endPoints[( i + 1 ) % endPoints.Count].position;
					p2 = endPoints[( i + 2 ) % endPoints.Count].position;
				}
				else
				{
					if( i < endPoints.Count - 2 )
					{
						p1 = endPoints[i + 1].position;
						p2 = endPoints[i + 2].position;
					}
					else if( i == endPoints.Count - 2 )
					{
						p1 = endPoints[i + 1].position;
						p2 = endPoints[i + 1].position;
					}
					else
					{
						p1 = endPoints[i].position;
						p2 = endPoints[i].position;
					}
				}

				endPoints[i].followingControlPointPosition = endPoints[i].position + ( p1 - pMinus1 ) / 6f;

				if( i < endPoints.Count - 1 )
					endPoints[i + 1].precedingControlPointPosition = p1 - ( p2 - endPoints[i].position ) / 6f;
				else if( m_loop )
					endPoints[0].precedingControlPointPosition = p1 - ( p2 - endPoints[i].position ) / 6f;
			}
		}

		// Credit: https://stackoverflow.com/a/14241741/2373034
		// Alternative approach: https://stackoverflow.com/a/25458216/2373034
		public void AutoCalculateNormals( float normalAngle = 0f, int smoothness = 10 )
		{
			for( int i = 0; i < endPoints.Count; i++ )
				endPoints[i].RefreshIfChanged();

			smoothness = Mathf.Max( 1, smoothness );
			float _1OverSmoothness = 1f / smoothness;

			// Calculate initial point's normal using Frenet formula
			Segment segment = new Segment( endPoints[0], endPoints[1], 0f );
			Vector3 tangent1 = segment.GetTangent( 0f ).normalized;
			Vector3 tangent2 = segment.GetTangent( 0.025f ).normalized;
			Vector3 cross = Vector3.Cross( tangent2, tangent1 ).normalized;
			if( Mathf.Approximately( cross.sqrMagnitude, 0f ) ) // This is not a curved spline but rather a straight line
				cross = Vector3.Cross( tangent2, ( tangent2.x != 0f || tangent2.z != 0f ) ? Vector3.up : Vector3.forward );

			endPoints[0].normal = Vector3.Cross( cross, tangent1 ).normalized;

			// Calculate other points' normals by iteratively (smoothness) calculating normals between the previous point and the next point
			for( int i = 0; i < endPoints.Count; i++ )
			{
				if( i < endPoints.Count - 1 )
					segment = new Segment( endPoints[i], endPoints[i + 1], 0f );
				else if( m_loop )
					segment = new Segment( endPoints[i], endPoints[0], 0f );
				else
					break;

				Vector3 prevNormal = endPoints[i].normal;
				for( int j = 1; j <= smoothness; j++ )
				{
					Vector3 tangent = segment.GetTangent( j * _1OverSmoothness ).normalized;
					prevNormal = Vector3.Cross( tangent, Vector3.Cross( prevNormal, tangent ).normalized ).normalized;
				}

				if( i < endPoints.Count - 1 )
					endPoints[i + 1].normal = prevNormal;
				else if( prevNormal != -endPoints[0].normal )
					endPoints[0].normal = ( endPoints[0].normal + prevNormal ).normalized;
			}

			// Rotate normals
			for( int i = 0; i < endPoints.Count; i++ )
			{
				float rotateAngle = normalAngle + endPoints[i].autoCalculatedNormalAngleOffset;
				if( Mathf.Approximately( rotateAngle, 180f ) )
					endPoints[i].normal = -endPoints[i].normal;
				else if( !Mathf.Approximately( rotateAngle, 0f ) )
				{
					if( i < endPoints.Count - 1 )
						segment = new Segment( endPoints[i], endPoints[i + 1], 0f );
					else if( m_loop )
						segment = new Segment( endPoints[i], endPoints[0], 0f );
					else
						segment = new Segment( endPoints[i - 1], endPoints[i], 1f );

					endPoints[i].normal = Quaternion.AngleAxis( rotateAngle, segment.GetTangent() ) * endPoints[i].normal;
				}
			}
		}

		// Credit: https://www.youtube.com/watch?v=d9k97JemYbM
		public EvenlySpacedPointsHolder CalculateEvenlySpacedPoints( float resolution = 10f, float accuracy = 3f )
		{
			int segmentCount = m_loop ? endPoints.Count : ( endPoints.Count - 1 );
			List<float> evenlySpacedPoints = new List<float>( segmentCount + Mathf.CeilToInt( segmentCount * resolution * 1.25f ) );

			// Calculate each spline segment's approximate length and store it temporarily in the list so that
			// we won't have to calculate the same value twice in the 2nd loop. We'll remove these length values
			// from the list at the end of the operation
			float estimatedSplineLength = 0f;
			for( int i = 0; i < segmentCount; i++ )
			{
				BezierPoint point1 = endPoints[i];
				BezierPoint point2 = ( i < endPoints.Count - 1 ) ? endPoints[i + 1] : endPoints[0];

				float controlNetLength = Vector3.Distance( point1.position, point1.followingControlPointPosition ) + Vector3.Distance( point1.followingControlPointPosition, point2.precedingControlPointPosition ) + Vector3.Distance( point2.precedingControlPointPosition, point2.position );
				float estimatedCurveLength = Vector3.Distance( point1.position, point2.position ) + controlNetLength * 0.5f;

				estimatedSplineLength += estimatedCurveLength;
				evenlySpacedPoints.Add( estimatedCurveLength );
			}

			float averageSegmentLength = estimatedSplineLength / segmentCount;
			float distanceBetweenEvenlySpacedPoints = averageSegmentLength / resolution;
			float remainingDistanceToEvenlySpacedPoint = distanceBetweenEvenlySpacedPoints;
			float totalLength = 0f;

			Vector3 previousPoint = endPoints[0].position;
			evenlySpacedPoints.Add( 0f );

			for( int i = 0; i < segmentCount; i++ )
			{
				Segment segment = new Segment( endPoints[i], ( i < endPoints.Count - 1 ) ? endPoints[i + 1] : endPoints[0], 0f );

				float estimatedCurveLength = evenlySpacedPoints[i];
				float tMultiplier = 1f / ( resolution * accuracy * ( estimatedCurveLength / averageSegmentLength ) );
				float t = 0, previousT = 0f;
				while( t < 1f )
				{
					t += tMultiplier;
					if( t > 1f )
						t = 1f;

					Vector3 point = segment.GetPoint( t );
					float distanceToPreviousPoint = Vector3.Distance( previousPoint, point );
					while( distanceToPreviousPoint >= remainingDistanceToEvenlySpacedPoint )
					{
						float newEvenlySpacedPointLocalT = previousT + ( t - previousT ) * ( remainingDistanceToEvenlySpacedPoint / distanceToPreviousPoint );
						evenlySpacedPoints.Add( segment.GetNormalizedT( newEvenlySpacedPointLocalT ) );

						//distanceToPreviousPoint -= remainingDistanceToEvenlySpacedPoint;
						distanceToPreviousPoint = Vector3.Distance( segment.GetPoint( newEvenlySpacedPointLocalT ), point );
						remainingDistanceToEvenlySpacedPoint = distanceBetweenEvenlySpacedPoints;
						totalLength += distanceBetweenEvenlySpacedPoints;
						previousT = newEvenlySpacedPointLocalT;
					}

					remainingDistanceToEvenlySpacedPoint -= distanceToPreviousPoint;
					previousT = t;
					previousPoint = point;
				}
			}

			totalLength += distanceBetweenEvenlySpacedPoints - remainingDistanceToEvenlySpacedPoint;

			// If the last calculated evenly spaced point is too close to the final point (t=1f), remove it.
			// The space between last 3 evenly spaced points won't really be even but the difference will be
			// negligible when resolution isn't too small
			if( remainingDistanceToEvenlySpacedPoint >= distanceBetweenEvenlySpacedPoints * 0.5f )
				evenlySpacedPoints.RemoveAt( evenlySpacedPoints.Count - 1 );

			evenlySpacedPoints.Add( 1f );

			// Remove spline segment lengths from list (which were temporarily stored there)
			evenlySpacedPoints.RemoveRange( 0, segmentCount );

			return new EvenlySpacedPointsHolder( this, totalLength, evenlySpacedPoints.ToArray() );
		}

		private float AccuracyToStepSize( float accuracy )
		{
			if( accuracy <= 0f )
				return 0.2f;

			return Mathf.Clamp( 1f / accuracy, 0.001f, 0.2f );
		}

		// Renders the spline gizmo during gameplay
		// Credit: https://docs.unity3d.com/ScriptReference/GL.html
		private void OnRenderObject()
		{
			if( !drawGizmos || endPoints.Count < 2 )
				return;

			if( !gizmoMaterial )
			{
				Shader shader = Shader.Find( "Hidden/Internal-Colored" );
				gizmoMaterial = new Material( shader ) { hideFlags = HideFlags.HideAndDontSave };
				gizmoMaterial.SetInt( "_SrcBlend", (int) UnityEngine.Rendering.BlendMode.SrcAlpha );
				gizmoMaterial.SetInt( "_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha );
				gizmoMaterial.SetInt( "_Cull", (int) UnityEngine.Rendering.CullMode.Off );
				gizmoMaterial.SetInt( "_ZWrite", 0 );
			}

			gizmoMaterial.SetPass( 0 );

			GL.Begin( GL.LINES );
			GL.Color( gizmoColor );

			Vector3 lastPos = endPoints[0].position;

			float gizmoStep = 1f / ( endPoints.Count * Mathf.Clamp( gizmoSmoothness, 1, 30 ) );
			for( float i = gizmoStep; i < 1f; i += gizmoStep )
			{
				GL.Vertex3( lastPos.x, lastPos.y, lastPos.z );
				lastPos = GetPoint( i );
				GL.Vertex3( lastPos.x, lastPos.y, lastPos.z );
			}

			GL.Vertex3( lastPos.x, lastPos.y, lastPos.z );
			lastPos = GetPoint( 1f );
			GL.Vertex3( lastPos.x, lastPos.y, lastPos.z );

			GL.End();
		}

		IEnumerator<BezierPoint> IEnumerable<BezierPoint>.GetEnumerator()
		{
			return endPoints.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return endPoints.GetEnumerator();
		}

#if UNITY_EDITOR
		internal void Reset()
		{
			for( int i = endPoints.Count - 1; i >= 0; i-- )
				UnityEditor.Undo.DestroyObjectImmediate( endPoints[i].gameObject );

			Initialize( 2 );

			endPoints[0].localPosition = Vector3.back;
			endPoints[1].localPosition = Vector3.forward;

			UnityEditor.Undo.RegisterCreatedObjectUndo( endPoints[0].gameObject, "Initialize Spline" );
			UnityEditor.Undo.RegisterCreatedObjectUndo( endPoints[1].gameObject, "Initialize Spline" );

			UnityEditor.Selection.activeTransform = endPoints[0].transform;
		}
#endif
	}
}