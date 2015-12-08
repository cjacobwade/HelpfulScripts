using UnityEngine;
using System.Collections;

public class WaitForTruthExample : MonoBehaviour
{
	// Also note that start can be used as a coroutine!
	IEnumerator Start()
	{
		yield return StartCoroutine(WaitForTruth(() => rigidbody.velocity.magnitude > 100000000000f));
		Debug.LogWarning("That's a bit excessive, isn't it?");
		rigidbody.velocity = Vector3.zero;
	}

	protected IEnumerator WaitForTruth(Func<bool> condition)
	{
		while (!condition())
			yield return null;
	}
}
