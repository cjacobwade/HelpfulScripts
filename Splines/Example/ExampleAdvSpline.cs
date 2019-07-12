using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Luckshot.Splines
{
	public class ExampleAdvSpline : AdvSpline<ExampleSplineNodeData>
	{
		[SerializeField]
		float _testAlpha = 0f;

		ExampleSplineNodeData _interpData = new ExampleSplineNodeData();

		protected override void OnDrawGizmosSelected()
		{
			base.OnDrawGizmosSelected();

			float repeatAlpha = Mathf.Repeat(_testAlpha, 1f);

			InterpNodeData(_interpData, repeatAlpha);

			Vector3 point = GetPoint(repeatAlpha);

			Gizmos.DrawSphere(point, _interpData.size);
			Gizmos.DrawLine(point, point + _interpData.lookDirection);
		}
	}
}