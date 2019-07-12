using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Luckshot.Splines
{
	[System.Serializable]
	public class SplineNodeData
	{
		public virtual void Lerp(SplineNodeData a, SplineNodeData b, float alpha)
		{
		}
	}
}
