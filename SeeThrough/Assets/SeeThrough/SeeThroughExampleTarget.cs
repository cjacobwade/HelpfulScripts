using UnityEngine;
using System.Collections;

public class SeeThroughExampleTarget : MonoBehaviour 
{
	protected Transform _transform = null;
	public Transform transform
	{
		get
		{
			if(!_transform)
				_transform = GetComponent<Transform>();

			return _transform;
		}
	}
}
