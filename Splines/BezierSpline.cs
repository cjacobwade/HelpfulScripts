using UnityEngine;
using System;
using UnityEngine.Serialization;

public class BezierSpline : MonoBehaviour 
{
	[SerializeField, HideInInspector]
	Vector3[] _points = null;
	public Vector3[] GetPoints()
	{
		if (_points == null || _points.Length == 0)
			Reset();

		return _points;
	}

	[SerializeField, HideInInspector]
	BezierControlPointMode[] _modes = null;
	protected BezierControlPointMode[] modes
	{
		get
		{
			if (_modes == null || _modes.Length == 0)
				Reset();

			return _modes;
		}
	}

	public int ModeCount
	{ get { return modes.Length; } }

	[SerializeField, HideInInspector]
	protected bool _loop = false;

	public bool Loop 
	{
		get { return _loop; }
		set 
		{
			_loop = value;
			if ( value == true ) 
			{
				modes[ modes.Length - 1 ] = modes[ 0 ];
				SetControlPoint( 0, GetPoints()[ 0 ] );
			}
		}
	}

	public int ControlPointCount
	{
		get { return GetPoints().Length; }
	}

	public Vector3 GetControlPoint( int index ) 
	{
		return GetPoints()[ index ];
	}

	public void SetControlPoint( int index, Vector3 point )
	{
		if (index % 3 == 0) 
		{
			Vector3 delta = point - GetPoints()[ index ];
			if ( _loop )
			{
				if ( index == 0 )
				{
					GetPoints()[ 1 ] += delta;
					GetPoints()[ GetPoints().Length - 2 ] += delta;
					GetPoints()[ GetPoints().Length - 1 ] = point;
				}
				else if ( index == GetPoints().Length - 1 )
				{
					GetPoints()[ 0 ] = point;
					GetPoints()[ 1 ] += delta;
					GetPoints()[ index - 1 ] += delta;
				}
				else 
				{
					GetPoints()[ index - 1 ] += delta;
					GetPoints()[ index + 1 ] += delta;
				}
			}
			else
			{
				if (index > 0)
				{
					GetPoints()[ index - 1 ] += delta;
				}
				if ( index + 1 < GetPoints().Length )
				{
					GetPoints()[ index + 1 ] += delta;
				}
			}
		}

		GetPoints()[ index ] = point;
		EnforceMode( index );
	}

	public BezierControlPointMode GetControlPointMode( int index ) 
	{
		return modes[ ( index + 1 ) / 3 ];
	}

	public void SetControlPointMode ( int index, BezierControlPointMode mode )
	{
		int modeIndex = ( index + 1 ) / 3;
		modes[ modeIndex ] = mode;
		if ( _loop )
		{
			if ( modeIndex == 0 )
			{
				modes[ modes.Length - 1 ] = mode;
			}
			else if ( modeIndex == modes.Length - 1 )
			{
				modes[ 0 ] = mode;
			}
		}

		EnforceMode( index );
	}

	private void EnforceMode ( int index )
	{
		int modeIndex = ( index + 1 ) / 3; // What the hell is this doing? Every xth point has a specific control mode
		BezierControlPointMode mode = modes[ modeIndex ];
		if ( mode == BezierControlPointMode.Free || !_loop && ( modeIndex == 0 || modeIndex == modes.Length - 1 ) )
		{
			return;
		}

		int middleIndex = modeIndex * 3;
		int fixedIndex, enforcedIndex;
		if ( index <= middleIndex )
		{
			fixedIndex = middleIndex - 1;
			if ( fixedIndex < 0 )
			{
				fixedIndex = GetPoints().Length - 2;
			}
			enforcedIndex = middleIndex + 1;
			if ( enforcedIndex >= GetPoints().Length )
			{
				enforcedIndex = 1;
			}
		}
		else
		{
			fixedIndex = middleIndex + 1;
			if ( fixedIndex >= GetPoints().Length )
			{
				fixedIndex = 1;
			}
			enforcedIndex = middleIndex - 1;
			if ( enforcedIndex < 0 )
			{
				enforcedIndex = GetPoints().Length - 2;
			}
		}

		Vector3 middle = GetPoints()[ middleIndex ];
		Vector3 enforcedTangent = middle - GetPoints()[ fixedIndex ];
		if ( mode == BezierControlPointMode.Aligned )
		{
			enforcedTangent = enforcedTangent.normalized * Vector3.Distance( middle, GetPoints()[ enforcedIndex ] );
		}
		GetPoints()[ enforcedIndex ] = middle + enforcedTangent;
	}

	public int CurveCount
	{
		get { return (GetPoints().Length - 1) / 3; }
	}

	public Vector3 GetPoint ( float t )
	{
		if (GetPoints().Length < 4)
			return Vector3.zero;

		int i;
		if ( t >= 1f )
		{
			t = 1f;
			i = GetPoints().Length - 4;
		}
		else
		{
			t = Mathf.Clamp01( t ) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}
		return transform.TransformPoint( Bezier.GetPoint( GetPoints()[ i ], GetPoints()[ i + 1 ], GetPoints()[ i + 2 ], GetPoints()[ i + 3 ], t ) );
	}

	public float GetNearestAlpha(Vector3 point, int iterations = 10)
	{
		int nearestIter = 0;
		float nearestAlpha = 0f;
		float nearestDistance = float.MaxValue;

		// Get a general spot along the spline that our point is near
		// This is more accurate then immediately halfing
		int totalIterations = iterations * ControlPointCount;
		for (int i = 0; i < totalIterations; i++)
		{
			float iterAlpha = i / (float)totalIterations;

			Vector3 iterPos = GetPoint(iterAlpha);
			float iterDistance = Vector3.Distance(point, iterPos);

			if (iterDistance < nearestDistance)
			{
				nearestIter = i;
				nearestAlpha = iterAlpha;
				nearestDistance = iterDistance;
			}
		}

		// Within a range around closest large iteration,
		// keep halving range till we have a good approximation
		float minIterAlpha = Mathf.Max(0, nearestIter - 1) / (float)totalIterations;
		float maxIterAlpha = Mathf.Min(totalIterations, nearestIter + 1) / (float)totalIterations;
		for(int i = 0; i < totalIterations; i++)
		{
			float iterAlpha = Mathf.Lerp(minIterAlpha, maxIterAlpha, i / (float)totalIterations);

			Vector3 iterPos = GetPoint(iterAlpha);
			float iterDistance = Vector3.Distance(point, iterPos);

			if (iterDistance < nearestDistance)
			{
				nearestAlpha = iterAlpha;
				nearestDistance = iterDistance;
			}
		}

		return nearestAlpha;
	}

	public Vector3 GetNearestSplinePoint(Vector3 position, int numIterations = 10)
	{ return GetPoint(GetNearestAlpha(position, numIterations)); }
	
	public Vector3 GetVelocity ( float t )
	{
		int i;
		if ( t >= 1f )
		{
			t = 1f;
			i = GetPoints().Length - 4;
		}
		else
		{
			t = Mathf.Clamp01( t ) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}

		return transform.TransformPoint( Bezier.GetFirstDerivative( GetPoints()[ i ], GetPoints()[ i + 1 ], GetPoints()[ i + 2 ], GetPoints()[ i + 3 ], t ) ) - transform.position;
	}
	
	public Vector3 GetDirection ( float t ) 
	{
		return GetVelocity( t ).normalized;
	}

	public virtual void AddCurve () 
	{
		Vector3 point = GetPoints()[ GetPoints().Length - 1 ];
		Array.Resize( ref _points, GetPoints().Length + 3 );
		point.x += 1f;
		GetPoints()[ GetPoints().Length - 3 ] = point;
		point.x += 1f;
		GetPoints()[ GetPoints().Length - 2 ] = point;
		point.x += 1f;
		GetPoints()[ GetPoints().Length - 1 ] = point;

		Array.Resize( ref _modes, modes.Length + 1 );
		modes[ modes.Length - 1 ] = BezierControlPointMode.Aligned;
		EnforceMode( GetPoints().Length - 4 );

		if ( _loop )
		{
			GetPoints()[ GetPoints().Length - 1 ] = GetPoints()[ 0 ];
			modes[ modes.Length - 1 ] = modes[ 0 ];
			EnforceMode( 0 );
		}
	}

	public virtual void RemoveCurve()
	{
		Array.Resize( ref _points, GetPoints().Length - 3 );
		Array.Resize( ref _modes, modes.Length - 1 );

		if ( _loop )
		{
			GetPoints()[ GetPoints().Length - 1 ] = GetPoints()[ 0 ];
			modes[ modes.Length - 1 ] = modes[ 0 ];
			EnforceMode( 0 );
		}
	}
	
	public virtual void Reset () 
	{
		_points = new Vector3[] 
		{
			new Vector3(1f, 0f, 0f),
			new Vector3(2f, 0f, 0f),
			new Vector3(3f, 0f, 0f),
			new Vector3(4f, 0f, 0f)
		};

		_modes = new BezierControlPointMode[] 
		{
			BezierControlPointMode.Free,
			BezierControlPointMode.Free
		};
	}

	protected virtual void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(GetPoint(0f), 0.3f);

		Gizmos.color = Color.red;
		Gizmos.DrawSphere(GetPoint(1f), 0.3f);

		int numIterations = 10 * ControlPointCount;
		for(int i = 1; i < numIterations; i++)
		{
			Gizmos.color = Color.Lerp(Color.green, Color.red, i / (float)numIterations);
			Gizmos.DrawLine(GetPoint(i / (float)numIterations), GetPoint((i - 1) / (float)numIterations));
		}
	}
}