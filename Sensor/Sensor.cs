using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// In addition to this rigidbody, you'll also need some type of collider set to trigger
[RequireComponent(typeof(Rigidbody))]
public class Sensor<T> : MonoBehaviour where T : MonoBehaviour
{
	// We cache colliders of sensed objects so we can skip getcomponents on things we've already hit
	protected Dictionary<Collider, T> _colliderToSensedMap = new Dictionary<Collider, T>();

	protected List<T> _sensedTs = new List<T>();
	public List<T> GetSensed()
	{
		// This verifies the contents of our sensed list before passing it on
		// Needed for when gameobjects are destroyed while in our sensor
		for(int i = 0; i < _sensedTs.Count; i++)
		{
			if(!_sensedTs[i])
				_sensedTs.RemoveAt(i--);
		}

		return _sensedTs;
	}

	// Subscribe to these callbacks to be notified when something is sensed/unsensed
	public System.Action<T> SensedCallback = delegate{};
	public System.Action<T> UnsensedCallback = delegate{};

	void OnTriggerEnter(Collider collider)
	{
		T hitT = null;
		if(_colliderToSensedMap.ContainsKey(collider))
			hitT = _colliderToSensedMap[collider];
		else
		{
			if(collider.attachedRigidbody)
				hitT = collider.attachedRigidbody.GetComponent<T>();
			else
				hitT = collider.GetComponent<T>();

			// Cache even null hitT's so we don't have to check them again
			_colliderToSensedMap.Add(collider, hitT);
		}

		if(hitT)
		{
			_sensedTs.Add(hitT);
			SensedCallback(hitT);
		}
	}

	void OnTriggerExit(Collider collider)
	{
		if(_colliderToSensedMap.ContainsKey(collider))
		{
			T unhitT = _colliderToSensedMap[collider];
			if(unhitT)
			{
				_sensedTs.Remove(_colliderToSensedMap[collider]);
				UnsensedCallback(unhitT);
			}
		}
	}
}
