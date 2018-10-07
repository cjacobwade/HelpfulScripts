using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BezierSpline))]
public class BezierDebug : WadeBehaviour
{
	[SerializeField]
	int _iterationCount = 1000;

	BezierSpline _spline = null;
	BezierSpline GetSpline()
	{
		if(!_spline)
			_spline = GetComponent<BezierSpline>();

		return _spline;
	}

	void Awake()
	{
#if !UNITY_EDITOR
		Destroy(this);
#endif
	}

	void OnDrawGizmosSelected()
	{
		for (int i = 0; i <= _iterationCount; i++)
		{
			float alpha = i / (float)_iterationCount;
			Vector3 point = GetSpline().GetPoint(alpha);
			Vector3 direction = GetSpline().GetDirection(alpha);
			Gizmos.DrawLine(point, point + direction);
		}

		Debug.Log(GetSpline().GetDirection(0f) + " " + GetSpline().GetDirection(1f));
	}
}
