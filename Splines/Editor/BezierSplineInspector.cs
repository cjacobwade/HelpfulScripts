using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierSpline), true)]
public class BezierSplineInspector : Editor 
{
	protected const int _stepsPerCurve = 10;
	protected const float _directionScale = 0.5f;
	protected const float _handleSize = 0.04f;
	protected const float _pickSize = 0.06f;

	protected static Color[] _modeColors = { Color.white, Color.yellow, Color.cyan };

	protected BezierSpline _spline = null;
	protected Transform _handleTransform = null;
	protected Quaternion _handleRotation = Quaternion.identity;
	protected int _selectedIndex = -1;

	public override void OnInspectorGUI()
	{
		if(_selectedIndex == -1)
			base.OnInspectorGUI();

		_spline = target as BezierSpline;
		EditorGUI.BeginChangeCheck();
		bool loop = EditorGUILayout.Toggle( "Loop", _spline.Loop );

		if ( EditorGUI.EndChangeCheck() )
		{
			Undo.RecordObject( _spline, "Toggle Loop" );
			EditorUtility.SetDirty( _spline );
			_spline.Loop = loop;
		}

		if ( _selectedIndex >= 0 && _selectedIndex < _spline.ControlPointCount )
		{
			DrawSelectedPointInspector();
		}

		if ( GUILayout.Button( "Add Curve" ) )
		{
			Undo.RecordObject( _spline, "Add Curve" );
			_spline.AddCurve();
			EditorUtility.SetDirty( _spline );
		}

		if ( _spline.CurveCount > 1 &&  GUILayout.Button( "Remove Curve" ) )
		{
			Undo.RecordObject( _spline, "Remove Curve" );
			_spline.RemoveCurve();
			EditorUtility.SetDirty( _spline );
		}
	}

	protected virtual void DrawSelectedPointInspector()
	{
		GUILayout.Label( "Selected Point" );
		EditorGUI.BeginChangeCheck();
		Vector3 point = EditorGUILayout.Vector3Field( "Position", _spline.GetControlPoint( _selectedIndex ) );

		if ( EditorGUI.EndChangeCheck() )
		{
			Undo.RecordObject( _spline, "Move Point" );
			EditorUtility.SetDirty( _spline );
			_spline.SetControlPoint( _selectedIndex, point );
		}

		EditorGUI.BeginChangeCheck();
		BezierControlPointMode mode = (BezierControlPointMode)EditorGUILayout.EnumPopup( "Mode", _spline.GetControlPointMode( _selectedIndex ) );

		if ( EditorGUI.EndChangeCheck() )
		{
			Undo.RecordObject( _spline, "Change Point Mode" );
			_spline.SetControlPointMode( _selectedIndex, mode );
			EditorUtility.SetDirty( _spline );
		}

		// Draw Node Data if ADV SPLINE
		// Would be in AdvSplineInspector, except AdvSpline is generic meaning
		// we can't use the CustomEditor attribute and force child types to use its inspector
		// which means either this is here or every AdvSpline child needs an associated TypeInspector script
		if (_selectedIndex % 3 == 0)
		{
			int advNodeIndex = _selectedIndex / 3;
			SerializedObject so = serializedObject;
			SerializedProperty nodeDataProp = so.FindProperty(string.Format("{0}.Array.data[{1}]", "_nodeData", advNodeIndex));
			if (nodeDataProp != null)
			{
				EditorGUI.BeginChangeCheck();

				EditorGUILayout.PropertyField(nodeDataProp, new GUIContent("Node Data"), true);

				if (EditorGUI.EndChangeCheck())
					so.ApplyModifiedProperties();
			}
		}
	}

	protected virtual void OnSceneGUI()
	{
		_spline = target as BezierSpline;
		_handleTransform = _spline.transform;
		_handleRotation = Tools.pivotRotation == PivotRotation.Local ? _handleTransform.rotation : Quaternion.identity;
		
		Vector3 p0 = ShowPoint( 0 );
		for ( int i = 1; i < _spline.ControlPointCount; i += 3 )
		{
			Vector3 p1 = ShowPoint( i );
			Vector3 p2 = ShowPoint( i + 1 );
			Vector3 p3 = ShowPoint( i + 2 );
			
			Handles.color = Color.gray;
			Handles.DrawLine( p0, p1 );
			Handles.DrawLine( p2, p3 );
			
			Handles.DrawBezier( p0, p3, p1, p2, Color.white, null, 2f );
			p0 = p3;
		}

		ShowDirections();
	}

	void ShowDirections()
	{
		Handles.color = Color.green;
		Vector3 point = _spline.GetPoint( 0f );
		Handles.DrawLine( point, point + _spline.GetDirection( 0f ) * _directionScale );
		int steps = _stepsPerCurve * _spline.CurveCount;
		for ( int i = 1; i <= steps; i++ )
		{
			point = _spline.GetPoint( i / (float)steps );
			Handles.DrawLine( point, point + _spline.GetDirection( i / (float)steps ) * _directionScale );
		}
	}

	protected virtual Vector3 ShowPoint( int index )
	{
		Vector3 point = _handleTransform.TransformPoint( _spline.GetControlPoint( index ) );
		float size = HandleUtility.GetHandleSize( point );
		if ( index == 0 )
		{
			size *= 2f;
		}

		Handles.color = _modeColors[ (int)_spline.GetControlPointMode( index ) ];
		if ( Handles.Button( point, _handleRotation, size * _handleSize, size * _pickSize, Handles.DotHandleCap ) )
		{
			_selectedIndex = index;
			Repaint();
		}

		if ( _selectedIndex == index )
		{
			EditorGUI.BeginChangeCheck();
			point = Handles.DoPositionHandle( point, _handleRotation );
			if ( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObject( _spline, "Move Point" );
				EditorUtility.SetDirty( _spline );
				_spline.SetControlPoint( index, _handleTransform.InverseTransformPoint( point ) );
			}
		}

		return point;
	}
}