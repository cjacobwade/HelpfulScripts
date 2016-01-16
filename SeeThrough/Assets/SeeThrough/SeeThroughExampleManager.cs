using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SeeThroughExampleManager : MonoBehaviour 
{
	[SerializeField]
	SeeThroughExampleTarget[] _seeThroughTargets = new SeeThroughExampleTarget[0];

	[SerializeField, Range(0.97f, 0.999f)]
	float _xRayMinDot = 0.996f;

	int _xRayMinDotPropertyID = 0;
	int[] _seeThroughTargetPropertyIDs = new int[0];

	void Awake()
	{
		if(_seeThroughTargets.Length < 1)
			FindSeeThroughTargets();

		string propertyName = "_XRayMinDot";
		Shader.SetGlobalFloat(propertyName, _xRayMinDot);
		_xRayMinDotPropertyID = Shader.PropertyToID(propertyName);

		_seeThroughTargetPropertyIDs = new int[_seeThroughTargets.Length];
		for (int i = 0; i < _seeThroughTargets.Length; ++i)
		{
			propertyName = "_Target" + i + "Pos";
			Shader.SetGlobalVector(propertyName, new Vector4(0f, 0f, -5000f, 1f)); // Set the default to be wayyyyyy behind the camera
			_seeThroughTargetPropertyIDs[i] = Shader.PropertyToID(propertyName);
		}
	}

	void Update()
	{
		if(_seeThroughTargets.Length < 1)
			enabled = false;
		else
		{
			for (int i = 0; i < _seeThroughTargets.Length; ++i)
			{
				if(_seeThroughTargets[i])
					Shader.SetGlobalVector(_seeThroughTargetPropertyIDs[i], _seeThroughTargets[i].transform.position);

				Shader.SetGlobalFloat(_xRayMinDotPropertyID, _xRayMinDot);
			}
		}
	}

	void FindSeeThroughTargets()
	{
		_seeThroughTargets = FindObjectsOfType<SeeThroughExampleTarget>();
	}
}
