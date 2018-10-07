using UnityEngine;
using System;

public abstract class AdvSpline<T> : BezierSpline where T : SplineNodeData, new()
{
	// Array of node data
	// Access node data of specific main control point
	// Get interp'd node data at whatever alpha

	[SerializeField]
	T[] _nodeData = null;
	protected T[] nodeData
	{
		get
		{
			if (_nodeData == null || _nodeData.Length == 0)
				Reset();

			return _nodeData;
		}
	}

	public T GetNodeData(int i)
	{
		return nodeData[i];
	}

	public void InterpNodeData(T inNodeData, float a)
	{
		a = Mathf.Clamp01(a);

		a *= (nodeData.Length - 1);
		int prev = Mathf.FloorToInt(a);
		int next = Mathf.CeilToInt(a);

		if(prev < nodeData.Length - 1)
			a -= (float)prev;

		inNodeData.Lerp(nodeData[prev], nodeData[next], Mathf.Clamp01(a));
	}

	public void SetNodeData(int i, T inNodeData)
	{
		nodeData[i] = inNodeData;
	}

	public override void AddCurve()
	{
		base.AddCurve();

		Array.Resize(ref _nodeData, nodeData.Length + 1);
		nodeData[nodeData.Length - 1] = new T();

		if(_loop)
			nodeData[nodeData.Length - 1] = nodeData[0];
	}

	public override void RemoveCurve()
	{
		base.RemoveCurve();

		Array.Resize(ref _nodeData, nodeData.Length - 1);

		if (_loop)
			nodeData[nodeData.Length - 1] = nodeData[0];
	}

	public override void Reset()
	{
		base.Reset();

		_nodeData = new T[]
		{
			new T(),
			new T()
		};
	}
}