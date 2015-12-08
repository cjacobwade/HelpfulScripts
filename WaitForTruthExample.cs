using UnityEngine;
using System.Collections;
using System;

public class WaitForTruthExample : MonoBehaviour
{
	// Also note that start can be used as a coroutine!
	IEnumerator Start()
	{
		yield return WaitForTruth(() => rigidbody.velocity.magnitude > 100000000000f);
		Debug.LogWarning("That's a bit excessive, isn't it?");
		rigidbody.velocity = Vector3.zero;
	}

	// Move both of these to your own derived version of Monobehaviour and then
	// inherit that class instead whenever you want to use WaitForTruth
	protected Coroutine WaitForTruth(Func<bool> condition)
	{
		return StartCoroutine(WaitForTruthRoutine(condition));
	}

	IEnumerator WaitForTruthRoutine(Func<bool> condition)
	{
		while (!condition())
			yield return null;
	}
}
