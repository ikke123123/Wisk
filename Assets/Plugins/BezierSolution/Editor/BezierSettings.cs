﻿using UnityEditor;
using UnityEngine;

namespace BezierSolution.Extras
{
	public static class BezierSettings
	{
		#region Colors
		private static Color? m_splineColor = null;
		public static Color NormalSplineColor
		{
			get
			{
				if( m_splineColor == null )
					m_splineColor = GetColor( "BezierSolution_SplineColor", new Color( 0.8f, 0.6f, 0.8f, 1f ) );

				return m_splineColor.Value;
			}
			set
			{
				m_splineColor = value;
				SetColor( "BezierSolution_SplineColor", value );
			}
		}

		private static Color? m_selectedSplineColor = null;
		public static Color SelectedSplineColor
		{
			get
			{
				if( m_selectedSplineColor == null )
					m_selectedSplineColor = GetColor( "BezierSolution_SelectedSplineColor", new Color( 0.8f, 0.6f, 0.8f, 1f ) );

				return m_selectedSplineColor.Value;
			}
			set
			{
				m_selectedSplineColor = value;
				SetColor( "BezierSolution_SelectedSplineColor", value );
			}
		}

		private static Color? m_endPointColor = null;
		public static Color NormalEndPointColor
		{
			get
			{
				if( m_endPointColor == null )
					m_endPointColor = GetColor( "BezierSolution_EndPointColor", Color.white );

				return m_endPointColor.Value;
			}
			set
			{
				m_endPointColor = value;
				SetColor( "BezierSolution_EndPointColor", value );
			}
		}

		private static Color? m_selectedEndPointColor = null;
		public static Color SelectedEndPointColor
		{
			get
			{
				if( m_selectedEndPointColor == null )
					m_selectedEndPointColor = GetColor( "BezierSolution_SelectedEndPointColor", Color.yellow );

				return m_selectedEndPointColor.Value;
			}
			set
			{
				m_selectedEndPointColor = value;
				SetColor( "BezierSolution_SelectedEndPointColor", value );
			}
		}

		private static Color? m_controlPointColor = null;
		public static Color NormalControlPointColor
		{
			get
			{
				if( m_controlPointColor == null )
					m_controlPointColor = GetColor( "BezierSolution_ControlPointColor", Color.white );

				return m_controlPointColor.Value;
			}
			set
			{
				m_controlPointColor = value;
				SetColor( "BezierSolution_ControlPointColor", value );
			}
		}

		private static Color? m_selectedControlPointColor = null;
		public static Color SelectedControlPointColor
		{
			get
			{
				if( m_selectedControlPointColor == null )
					m_selectedControlPointColor = GetColor( "BezierSolution_SelectedControlPointColor", Color.green );

				return m_selectedControlPointColor.Value;
			}
			set
			{
				m_selectedControlPointColor = value;
				SetColor( "BezierSolution_SelectedControlPointColor", value );
			}
		}

		private static Color? m_normalsPreviewColor = null;
		public static Color NormalsPreviewColor
		{
			get
			{
				if( m_normalsPreviewColor == null )
					m_normalsPreviewColor = GetColor( "BezierSolution_NormalsPreviewColor", Color.blue );

				return m_normalsPreviewColor.Value;
			}
			set
			{
				m_normalsPreviewColor = value;
				SetColor( "BezierSolution_NormalsPreviewColor", value );
			}
		}
		#endregion

		#region Size Adjustments
		private static float? m_splineThickness = null;
		public static float SplineThickness
		{
			get
			{
				if( m_splineThickness == null )
					m_splineThickness = EditorPrefs.GetFloat( "BezierSolution_SplineThickness", 8f );

				return m_splineThickness.Value;
			}
			set
			{
				m_splineThickness = value;
				EditorPrefs.SetFloat( "BezierSolution_SplineThickness", value );
			}
		}

		private static float? m_endPointSize = null;
		public static float EndPointSize
		{
			get
			{
				if( m_endPointSize == null )
					m_endPointSize = EditorPrefs.GetFloat( "BezierSolution_EndPointSize", 0.075f );

				return m_endPointSize.Value;
			}
			set
			{
				m_endPointSize = value;
				EditorPrefs.SetFloat( "BezierSolution_EndPointSize", value );
			}
		}

		private static float? m_selectedEndPointSize = null;
		public static float SelectedEndPointSize
		{
			get
			{
				if( m_selectedEndPointSize == null )
					m_selectedEndPointSize = EditorPrefs.GetFloat( "BezierSolution_SelectedEndPointSize", 0.075f * 1.5f );

				return m_selectedEndPointSize.Value;
			}
			set
			{
				m_selectedEndPointSize = value;
				EditorPrefs.SetFloat( "BezierSolution_SelectedEndPointSize", value );
			}
		}

		private static float? m_controlPointSize = null;
		public static float ControlPointSize
		{
			get
			{
				if( m_controlPointSize == null )
					m_controlPointSize = EditorPrefs.GetFloat( "BezierSolution_ControlPointSize", 0.05f );

				return m_controlPointSize.Value;
			}
			set
			{
				m_controlPointSize = value;
				EditorPrefs.SetFloat( "BezierSolution_ControlPointSize", value );
			}
		}

		private static float? m_normalsPreviewLength = null;
		public static float NormalsPreviewLength
		{
			get
			{
				if( m_normalsPreviewLength == null )
					m_normalsPreviewLength = EditorPrefs.GetFloat( "BezierSolution_NormalsPreviewLength", 0.35f );

				return m_normalsPreviewLength.Value;
			}
			set
			{
				m_normalsPreviewLength = value;
				EditorPrefs.SetFloat( "BezierSolution_NormalsPreviewLength", value );
			}
		}

		private static float? m_extraDataAsFrustumSize = null;
		public static float ExtraDataAsFrustumSize
		{
			get
			{
				if( m_extraDataAsFrustumSize == null )
					m_extraDataAsFrustumSize = EditorPrefs.GetFloat( "BezierSolution_ExtraDataFrustumSize", 2.2f );

				return m_extraDataAsFrustumSize.Value;
			}
			set
			{
				m_extraDataAsFrustumSize = value;
				EditorPrefs.SetFloat( "BezierSolution_ExtraDataFrustumSize", value );
			}
		}
		#endregion

		#region Other Settings
		private static float? m_splineSmoothness = null;
		public static float SplineSmoothness
		{
			get
			{
				if( m_splineSmoothness == null )
					m_splineSmoothness = EditorPrefs.GetFloat( "BezierSolution_SplineSmoothness", 10f );

				return m_splineSmoothness.Value;
			}
			set
			{
				value = Mathf.Max( value, 1f );
				m_splineSmoothness = value;
				EditorPrefs.SetFloat( "BezierSolution_SplineSmoothness", value );
			}
		}

		private static bool? m_moveMultiplePointsInOppositeDirections = null;
		public static bool MoveMultiplePointsInOppositeDirections
		{
			get
			{
				if( m_moveMultiplePointsInOppositeDirections == null )
					m_moveMultiplePointsInOppositeDirections = EditorPrefs.GetBool( "BezierSolution_OppositeTransformation", false );

				return m_moveMultiplePointsInOppositeDirections.Value;
			}
			set
			{
				m_moveMultiplePointsInOppositeDirections = value;
				EditorPrefs.SetBool( "BezierSolution_OppositeTransformation", value );
			}
		}
		#endregion

		#region Visibility Settings
		private static bool? m_showControlPoints = null;
		public static bool ShowControlPoints
		{
			get
			{
				if( m_showControlPoints == null )
					m_showControlPoints = EditorPrefs.GetBool( "BezierSolution_ShowControlPoints", true );

				return m_showControlPoints.Value;
			}
			set
			{
				m_showControlPoints = value;
				EditorPrefs.SetBool( "BezierSolution_ShowControlPoints", value );
			}
		}

		private static bool? m_showControlPointDirections = null;
		public static bool ShowControlPointDirections
		{
			get
			{
				if( m_showControlPointDirections == null )
					m_showControlPointDirections = EditorPrefs.GetBool( "BezierSolution_ShowControlPointDirs", true );

				return m_showControlPointDirections.Value;
			}
			set
			{
				m_showControlPointDirections = value;
				EditorPrefs.SetBool( "BezierSolution_ShowControlPointDirs", value );
			}
		}

		private static bool? m_showEndPointLabels = null;
		public static bool ShowEndPointLabels
		{
			get
			{
				if( m_showEndPointLabels == null )
					m_showEndPointLabels = EditorPrefs.GetBool( "BezierSolution_ShowEndPointLabels", true );

				return m_showEndPointLabels.Value;
			}
			set
			{
				m_showEndPointLabels = value;
				EditorPrefs.SetBool( "BezierSolution_ShowEndPointLabels", value );
			}
		}

		private static bool? m_showNormals = null;
		public static bool ShowNormals
		{
			get
			{
				if( m_showNormals == null )
					m_showNormals = EditorPrefs.GetBool( "BezierSolution_ShowNormals", true );

				return m_showNormals.Value;
			}
			set
			{
				m_showNormals = value;
				EditorPrefs.SetBool( "BezierSolution_ShowNormals", value );
			}
		}

		private static bool? m_visualizeExtraDataAsFrustum = null;
		public static bool VisualizeExtraDataAsFrustum
		{
			get
			{
				if( m_visualizeExtraDataAsFrustum == null )
					m_visualizeExtraDataAsFrustum = EditorPrefs.GetBool( "BezierSolution_VisualizeFrustum", false );

				return m_visualizeExtraDataAsFrustum.Value;
			}
			set
			{
				m_visualizeExtraDataAsFrustum = value;
				EditorPrefs.SetBool( "BezierSolution_VisualizeFrustum", value );
			}
		}
		#endregion

#if UNITY_2018_3_OR_NEWER
		[SettingsProvider]
		public static SettingsProvider CreatePreferencesGUI()
		{
			return new SettingsProvider( "yasirkula/Bezier Solution", SettingsScope.Project )
			{
				guiHandler = ( searchContext ) => PreferencesGUI(),
				keywords = new System.Collections.Generic.HashSet<string>() { "Bezier", "Spline", "Point", "Normals", "Color", "Size" }
			};
		}

		[MenuItem( "CONTEXT/BezierSpline/Open Settings" )]
		[MenuItem( "CONTEXT/BezierPoint/Open Settings" )]
		private static void OpenPreferencesWindow( MenuCommand command )
		{
			SettingsService.OpenProjectSettings( "yasirkula/Bezier Solution" );
		}
#endif

#if !UNITY_2018_3_OR_NEWER
		[PreferenceItem( "Bezier Solution" )]
#endif
		public static void PreferencesGUI()
		{
			Color c;
			float f;
			bool b;

			EditorGUI.BeginChangeCheck();

			EditorGUI.BeginChangeCheck();
			c = EditorGUILayout.ColorField( "Selected Spline Color", SelectedSplineColor );
			if( EditorGUI.EndChangeCheck() )
				SelectedSplineColor = c;

			EditorGUI.BeginChangeCheck();
			c = EditorGUILayout.ColorField( "Unselected Spline Color", NormalSplineColor );
			if( EditorGUI.EndChangeCheck() )
				NormalSplineColor = c;

			EditorGUI.BeginChangeCheck();
			f = EditorGUILayout.FloatField( "Selected Spline Thickness", SplineThickness );
			if( EditorGUI.EndChangeCheck() )
				SplineThickness = f;

			EditorGUI.BeginChangeCheck();
			f = EditorGUILayout.FloatField( "Unselected Spline Smoothness", SplineSmoothness );
			if( EditorGUI.EndChangeCheck() )
				SplineSmoothness = f;

			EditorGUILayout.Space();

			EditorGUI.BeginChangeCheck();
			c = EditorGUILayout.ColorField( "Selected End Points Color", SelectedEndPointColor );
			if( EditorGUI.EndChangeCheck() )
				SelectedEndPointColor = c;

			EditorGUI.BeginChangeCheck();
			c = EditorGUILayout.ColorField( "Unselected End Point Color", NormalEndPointColor );
			if( EditorGUI.EndChangeCheck() )
				NormalEndPointColor = c;

			EditorGUI.BeginChangeCheck();
			f = EditorGUILayout.FloatField( "Selected End Points Size", SelectedEndPointSize );
			if( EditorGUI.EndChangeCheck() )
				SelectedEndPointSize = f;

			EditorGUI.BeginChangeCheck();
			f = EditorGUILayout.FloatField( "Unselected End Points Size", EndPointSize );
			if( EditorGUI.EndChangeCheck() )
				EndPointSize = f;

			EditorGUI.BeginChangeCheck();
			b = EditorGUILayout.Toggle( "Show End Point Labels", ShowEndPointLabels );
			if( EditorGUI.EndChangeCheck() )
				ShowEndPointLabels = b;

			EditorGUILayout.Space();

			EditorGUI.BeginChangeCheck();
			b = EditorGUILayout.Toggle( "Show Control Points", ShowControlPoints );
			if( EditorGUI.EndChangeCheck() )
				ShowControlPoints = b;

			EditorGUI.indentLevel++;

			EditorGUI.BeginChangeCheck();
			b = EditorGUILayout.Toggle( "Show Control Point Directions", ShowControlPointDirections );
			if( EditorGUI.EndChangeCheck() )
				ShowControlPointDirections = b;

			EditorGUI.BeginChangeCheck();
			c = EditorGUILayout.ColorField( "Selected Control Point Color", SelectedControlPointColor );
			if( EditorGUI.EndChangeCheck() )
				SelectedControlPointColor = c;

			EditorGUI.BeginChangeCheck();
			c = EditorGUILayout.ColorField( "Unselected Control Point Color", NormalControlPointColor );
			if( EditorGUI.EndChangeCheck() )
				NormalControlPointColor = c;

			EditorGUI.BeginChangeCheck();
			f = EditorGUILayout.FloatField( "Control Points Size", ControlPointSize );
			if( EditorGUI.EndChangeCheck() )
				ControlPointSize = f;

			EditorGUI.indentLevel--;

			EditorGUILayout.Space();

			EditorGUI.BeginChangeCheck();
			b = EditorGUILayout.Toggle( "Show Normals", ShowNormals );
			if( EditorGUI.EndChangeCheck() )
				ShowNormals = b;

			EditorGUI.indentLevel++;

			EditorGUI.BeginChangeCheck();
			c = EditorGUILayout.ColorField( "Normals Preview Color", NormalsPreviewColor );
			if( EditorGUI.EndChangeCheck() )
				NormalsPreviewColor = c;

			EditorGUI.BeginChangeCheck();
			f = EditorGUILayout.FloatField( "Normals Preview Length", NormalsPreviewLength );
			if( EditorGUI.EndChangeCheck() )
				NormalsPreviewLength = f;

			EditorGUI.indentLevel--;

			EditorGUILayout.Space();

			EditorGUI.BeginChangeCheck();
			b = EditorGUILayout.Toggle( new GUIContent( "Visualize Extra Data As Frustum", "Visualize end points' Extra Data as camera frustum in Scene window" ), VisualizeExtraDataAsFrustum );
			if( EditorGUI.EndChangeCheck() )
				VisualizeExtraDataAsFrustum = b;

			EditorGUI.indentLevel++;

			EditorGUI.BeginChangeCheck();
			f = EditorGUILayout.FloatField( "Frustum Size", ExtraDataAsFrustumSize );
			if( EditorGUI.EndChangeCheck() )
				ExtraDataAsFrustumSize = f;

			EditorGUI.indentLevel--;

			EditorGUILayout.Space();

			if( GUILayout.Button( "Reset To Defaults" ) )
			{
				NormalSplineColor = new Color( 0.8f, 0.6f, 0.8f, 1f );
				SelectedSplineColor = new Color( 0.8f, 0.6f, 0.8f, 1f );
				NormalEndPointColor = Color.white;
				SelectedEndPointColor = Color.yellow;
				NormalControlPointColor = Color.white;
				SelectedControlPointColor = Color.green;
				NormalsPreviewColor = Color.blue;

				SplineThickness = 8f;
				EndPointSize = 0.075f;
				SelectedEndPointSize = 0.075f * 1.5f;
				ControlPointSize = 0.05f;
				NormalsPreviewLength = 0.35f;
				ExtraDataAsFrustumSize = 2.2f;

				SplineSmoothness = 10f;
				MoveMultiplePointsInOppositeDirections = false;

				ShowControlPoints = true;
				ShowControlPointDirections = true;
				ShowEndPointLabels = true;
				ShowNormals = true;
				VisualizeExtraDataAsFrustum = false;

				SceneView.RepaintAll();
			}

			if( EditorGUI.EndChangeCheck() )
				SceneView.RepaintAll();
		}

		private static Color GetColor( string pref, Color defaultColor )
		{
			if( !EditorPrefs.HasKey( pref ) )
				return defaultColor;

			string[] parts = EditorPrefs.GetString( pref ).Split( ';' );
			return new Color32( byte.Parse( parts[0] ), byte.Parse( parts[1] ), byte.Parse( parts[2] ), byte.Parse( parts[3] ) );
		}

		private static void SetColor( string pref, Color32 value )
		{
			EditorPrefs.SetString( pref, string.Concat( value.r.ToString(), ";", value.g.ToString(), ";", value.b.ToString(), ";", value.a.ToString() ) );
		}
	}
}