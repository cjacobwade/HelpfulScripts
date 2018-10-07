using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ExampleSplineNodeData : SplineNodeData
{
	public float size = 1f;
	public Vector3 lookDirection = Vector3.zero;

	public override void Lerp(SplineNodeData a, SplineNodeData b, float alpha)
	{
		base.Lerp(a, b, alpha);

		ExampleSplineNodeData ea = (ExampleSplineNodeData)a;
		ExampleSplineNodeData eb = (ExampleSplineNodeData)b;

		size = Mathf.Lerp(ea.size, eb.size, alpha);
		lookDirection = Vector3.Lerp(ea.lookDirection.normalized, eb.lookDirection.normalized, alpha);
	}
}
