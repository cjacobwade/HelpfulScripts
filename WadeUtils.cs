using UnityEngine;
using System.Collections;

public struct InputTags
{
	public string horizontal;	// AD 			Left stick X
	public string vertical;		// WS 			Left stick Y
	public string horizontal2;	// MouseX		Right stick X
	public string vertical2;	// MouseY 		Right stick Y
	public string jump;			// Spacebar		A
	public string fire;			// Left mouse	LT
	public string fire2;		// Right mouse 	RT
	public string action;		// E	 		X
	public string action2;		// R			Y
	public string action3;		// Q			B
	public string action4;		// 1			LB
	public string action5;		// 2			RT
	public string pause;		// Escape		Start
}

public static class WadeUtils
{
	public static string horizontal = "Horizontal";	// AD 			Left stick X
	public static string vertical = "Vertical";		// WS 			Left stick Y
	public static string horizontal2 = "Mouse X";	// MouseX		Right stick X
	public static string vertical2 = "Mouse Y";		// MouseY 		Right stick Y
	public static string jump = "Jump";				// Spacebar		A
	public static string fire = "Fire";				// Left mouse	LT
	public static string fire2 = "Fire2";			// Right mouse 	RT
	public static string action = "";				// E	 		X
	public static string action2 = "";				// R			Y
	public static string action3 = "";				// Q			B
	public static string action4 = "";				// 1			LB
	public static string action5 = "";				// 2			RT
	public static string pause = "";				// Escape		Start

	public const int emptyLayer = 0;

	/// <summary>
	/// Gets the input tags.
	/// </summary>
	/// <value>The input tags.</value>
	public static InputTags inputTags
	{
		get
		{
			InputTags tags;

			tags.horizontal = horizontal;
			tags.vertical = vertical;
			tags.horizontal2 = horizontal2;
			tags.vertical2 = vertical2;
			tags.jump = jump;
			tags.fire = fire;
			tags.fire2 = fire2;
			tags.action = action;
			tags.action2 = action2;
			tags.action3 = action3;
			tags.action4 = action4;
			tags.action5 = action5;
			tags.pause = pause;

			return tags;
		}
	}

	#region Floats

	/// <summary>
	/// Clamps this floats value between min and max.
	/// </summary>
	/// <param name="value">Value.</param>
	/// <param name="min">Minimum.</param>
	/// <param name="max">Max.</param>
	public static void Clamp(ref float value, float min, float max)
	{
		value = Mathf.Clamp (value, min, max);
	}

	/// <summary>
	/// Interpolates to point t along a line between from and to.
	/// </summary>
	/// <param name="from">From.</param>
	/// <param name="to">To.</param>
	/// <param name="t">T.</param>
	public static void Lerp(ref float from, float to, float t)
	{
		from = Mathf.Lerp (from, to, t);
	}

	#endregion

	#region Vector2s

	/// <summary>
	/// Interpolates to point t along a line between from and to.
	/// </summary>
	/// <param name="from">From.</param>
	/// <param name="to">To.</param>
	/// <param name="t">T.</param>
	public static void Lerp(ref Vector2 from, Vector2 to, float t)
	{
		from = Vector2.Lerp(from, to, t);
	}


	/// <summary>
	/// Find distance from this point to another point.
	/// </summary>
	/// <returns>The to.</returns>
	/// <param name="pointA">Point a.</param>
	/// <param name="pointB">Point b.</param>
	public static float DistanceTo(this Vector2 pointA, Vector4 pointB)
	{
		return ((Vector4)pointA).DistanceTo(pointB);
	}

	#endregion

	#region Vector3s

	/// <summary>
	/// Interpolates to point t along a line between from and to.
	/// </summary>
	/// <param name="from">From.</param>
	/// <param name="to">To.</param>
	/// <param name="t">T.</param>
	public static void Lerp(ref Vector3 from, Vector3 to, float t)
	{
		from = Vector3.Lerp(from, to, t);
	}

	/// <summary>
	/// Slerp the specified from, to and t.
	/// </summary>
	/// <param name="from">From.</param>
	/// <param name="to">To.</param>
	/// <param name="t">T.</param>
	public static void Slerp(ref Vector3 from, Vector3 to, float t)
	{
		from = Vector3.Slerp(from, to, t);
	}

	/// <summary>
	/// Find distance from this point to another point.
	/// </summary>
	/// <returns>The to.</returns>
	/// <param name="pointA">Point a.</param>
	/// <param name="pointB">Point b.</param>
	public static float DistanceTo(this Vector3 pointA, Vector4 pointB)
	{
		return ((Vector4)pointA).DistanceTo(pointB);
	}

	#endregion

	#region Vector4s

	/// <summary>
	/// Interpolates to point t along a line between from and to.
	/// </summary>
	/// <param name="from">From.</param>
	/// <param name="to">To.</param>
	/// <param name="t">T.</param>
	public static void Lerp(ref Vector4 from, Vector4 to, float t)
	{
		from = Vector4.Lerp(from, to, t);
	}

	/// <summary>
	/// Find distance from this point to another point.
	/// </summary>
	/// <returns>The to.</returns>
	/// <param name="pointA">Point a.</param>
	/// <param name="pointB">Point b.</param>
	public static float DistanceTo(this Vector4 pointA, Vector4 pointB)
	{
		return Vector4.Distance (pointA, pointB);
	}

	/// <summary>
	/// Gets the Input on defined axes.
	/// </summary>
	/// <returns>The input axes.</returns>
	/// <param name="x">Axis1.</param>
	/// <param name="y">Axis2.</param>
	/// <param name="z">Axis3.</param>
	/// <param name="w">Axis4.</param>
	public static Vector4 GetInputAxes(string x = null, string y = null, string z = null, string w = null)
	{
		Vector4 axes = Vector4.zero;
		
		if(x != null)
		{
			axes.x = Input.GetAxis(x);
		}
		
		if(y != null)
		{
			axes.y = Input.GetAxis(y);
		}

		if(z != null)
		{
			axes.x = Input.GetAxis(x);
		}
		
		if(w != null)
		{
			axes.y = Input.GetAxis(y);
		}
		
		return axes;
	}

	#endregion

	#region Quaternions

	/// <summary>
	/// Sets the rotation x.
	/// </summary>
	/// <param name="rotation">Rotation.</param>
	/// <param name="angle">Angle.</param>
	public static void SetRotationX(ref Quaternion rotation, float angle)
	{
		Vector3 eulers = rotation.eulerAngles;
		eulers.x = angle;

		rotation.eulerAngles = eulers;
	}

	/// <summary>
	/// Sets the rotation y.
	/// </summary>
	/// <param name="rotation">Rotation.</param>
	/// <param name="angle">Angle.</param>
	public static void SetRotationY(ref Quaternion rotation, float angle)
	{
		Vector3 eulers = rotation.eulerAngles;
		eulers.y = angle;
		
		rotation.eulerAngles = eulers;
	}

	/// <summary>
	/// Sets the rotation z.
	/// </summary>
	/// <param name="rotation">Rotation.</param>
	/// <param name="angle">Angle.</param>
	public static void SetRotationZ(ref Quaternion rotation, float angle)
	{
		Vector3 eulers = rotation.eulerAngles;
		eulers.z = angle;
		
		rotation.eulerAngles = eulers;
	}

	/// <summary>
	/// Interpolates to point t along a line between from and to.
	/// </summary>
	/// <param name="from">From.</param>
	/// <param name="to">To.</param>
	/// <param name="t">T.</param>
	public static void Lerp(ref Quaternion from, Quaternion to, float t)
	{
		from = Quaternion.Lerp (from, to, t);
	}

	/// <summary>
	/// Slerp the specified from, to and t.
	/// </summary>
	/// <param name="from">From.</param>
	/// <param name="to">To.</param>
	/// <param name="t">T.</param>
	public static void Slerp(ref Quaternion from, Quaternion to, float t)
	{
		from = Quaternion.Slerp (from, to, t);
	}

	#endregion

	#region Transform

	/// <summary>
	/// Sets the position x.
	/// </summary>
	/// <param name="transform">Transform.</param>
	/// <param name="x">The x coordinate.</param>
	public static void SetPositionX(this Transform transform, float x)
	{
		transform.position = new Vector3 (x, transform.position.y, transform.position.z);
	}

	/// <summary>
	/// Sets the position y.
	/// </summary>
	/// <param name="transform">Transform.</param>
	/// <param name="y">The y coordinate.</param>
	public static void SetPositionY(this Transform transform, float y)
	{
		transform.position = new Vector3 (transform.position.x, y, transform.position.z);
	}

	/// <summary>
	/// Sets the position z.
	/// </summary>
	/// <param name="transform">Transform.</param>
	/// <param name="z">The z coordinate.</param>
	public static void SetPositionZ(this Transform transform, float z)
	{
		transform.position = new Vector3 (transform.position.x, transform.position.y, z);
	}

	/// <summary>
	/// Adds the position.
	/// </summary>
	/// <param name="transform">Transform.</param>
	/// <param name="offset">Offset.</param>
	public static void AddPosition(this Transform transform, Vector3 offset)
	{
		transform.position += offset;
	}
	
	/// <summary>
	/// Adds the position.
	/// </summary>
	/// <param name="transform">Transform.</param>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	public static void AddPosition(this Transform transform, float x, float y, float z)
	{
		transform.position += new Vector3(x, y, z);
	}

	/// <summary>
	/// Sets the rotation x.
	/// </summary>
	/// <param name="transform">Transform.</param>
	/// <param name="x">The x coordinate.</param>
	public static void SetRotationX(this Transform transform, float x)
	{
		transform.rotation *= Quaternion.Euler(x, transform.position.y, transform.position.z);
	}

	/// <summary>
	/// Sets the rotation y.
	/// </summary>
	/// <param name="transform">Transform.</param>
	/// <param name="y">The y coordinate.</param>
	public static void SetRotationY(this Transform transform, float y)
	{
		transform.rotation *= Quaternion.Euler(transform.position.x, y, transform.position.z);
	}

	/// <summary>
	/// Sets the rotation z.
	/// </summary>
	/// <param name="transform">Transform.</param>
	/// <param name="z">The z coordinate.</param>
	public static void SetRotationZ(this Transform transform, float z)
	{
		transform.rotation *= Quaternion.Euler(transform.position.x, transform.position.y, z);
	}
	
	/// <summary>
	/// Adds to the transform's rotation.
	/// </summary>
	/// <param name="transform">Transform.</param>
	/// <param name="offset">Offset.</param>
	public static void AddRotation(this Transform transform, Vector3 offset)
	{
		transform.rotation *= Quaternion.Euler(offset);
	}
	
	/// <summary>
	/// Adds to the transform's rotation.
	/// </summary>
	/// <param name="transform">Transform.</param>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	public static void AddRotation(this Transform transform, float x, float y, float z)
	{
		transform.rotation *= Quaternion.Euler(new Vector3(x, y, z));
	}

	/// <summary>
	/// Sets the rotation x.
	/// </summary>
	/// <param name="transform">Transform.</param>
	/// <param name="x">The x coordinate.</param>
	public static void SetScaleX(this Transform transform, float x)
	{
		transform.localScale = new Vector3(x, transform.position.y, transform.position.z);
	}
	
	/// <summary>
	/// Sets the rotation y.
	/// </summary>
	/// <param name="transform">Transform.</param>
	/// <param name="y">The y coordinate.</param>
	public static void SetScaleY(this Transform transform, float y)
	{
		transform.localScale = new Vector3(transform.position.x, y, transform.position.z);
	}
	
	/// <summary>
	/// Sets the rotation z.
	/// </summary>
	/// <param name="transform">Transform.</param>
	/// <param name="z">The z coordinate.</param>
	public static void SetScaleZ(this Transform transform, float z)
	{
		transform.localScale = new Vector3(transform.position.x, transform.position.y, z);
	}

	/// <summary>
	/// Adds the scale.
	/// </summary>
	/// <param name="transform">Transform.</param>
	/// <param name="offset">Offset.</param>
	public static void AddScale(this Transform transform, Vector3 offset)
	{
		transform.localScale += offset;
	}
	
	/// <summary>
	/// Adds the scale.
	/// </summary>
	/// <param name="transform">Transform.</param>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	public static void AddScale(this Transform transform, float x, float y, float z)
	{
		transform.localScale += new Vector3(x, y, z);
	}

	/// <summary>
	/// Lerps to face a target.
	/// </summary>
	/// <param name="transform">Transform.</param>
	/// <param name="target">Target.</param>
	/// <param name="t">T.</param>
	public static void LerpLookAt(this Transform transform, Transform target, float t)
	{
		Quaternion currentRot = transform.rotation;
		transform.LookAt (target);
		Quaternion lookRot = transform.rotation;

		transform.rotation = Quaternion.Lerp (currentRot, lookRot, t);
	}

	/// <summary>
	/// Lerps to face a position.
	/// </summary>
	/// <param name="transform">Transform.</param>
	/// <param name="target">Target.</param>
	/// <param name="t">T.</param>
	public static void LerpLookAt(this Transform transform, Vector3 target, float t)
	{
		Quaternion currentRot = transform.rotation;
		transform.LookAt (target);
		Quaternion lookRot = transform.rotation;

		transform.rotation = Quaternion.Lerp (currentRot, lookRot, t);
	}

	/// <summary>
	/// LookAt(Transform) with control of whether you rotate along X and Y axes.
	/// </summary>
	/// <param name="transform">Transform.</param>
	/// <param name="target">Target.</param>
	/// <param name="xRotate">If set to <c>true</c> x rotate.</param>
	/// <param name="yRotate">If set to <c>true</c> y rotate.</param>
	public static void LookAtWithAxisControl(this Transform transform, Transform target, bool xRotate, bool yRotate)
	{
		transform.LookAtWithAxisControl (target.position, xRotate, yRotate);
	}

	/// <summary>
	/// LookAt(Vector3) with control of whether you rotate along X and Y axes.
	/// </summary>
	/// <param name="transform">Transform.</param>
	/// <param name="targetPos">Target position.</param>
	/// <param name="xRotate">If set to <c>true</c> x rotate.</param>
	/// <param name="yRotate">If set to <c>true</c> y rotate.</param>
	public static void LookAtWithAxisControl(this Transform transform, Vector3 targetPos, bool xRotate, bool yRotate)
	{
		if(!xRotate && !yRotate)
		{
			Debug.LogError("Error: No LookAt because both axes are disabled");
			Debug.DebugBreak();
		}

		Vector3 lookPos = targetPos;
		lookPos.y = xRotate ? targetPos.y : transform.position.y;
		lookPos.x = yRotate ? targetPos.x : transform.position.x;

		transform.LookAt(lookPos);
	}

	/// <summary>
	/// Lerp towards LookAt(Transform) with control over whether you rotate on X and Y.
	/// </summary>
	/// <param name="transform">Transform.</param>
	/// <param name="target">Target.</param>
	/// <param name="xRotate">If set to <c>true</c> x rotate.</param>
	/// <param name="yRotate">If set to <c>true</c> y rotate.</param>
	/// <param name="t">T.</param>
	public static void LerpLookAtWithAxisControl(this Transform transform, Transform target, bool xRotate, bool yRotate, float t)
	{
		transform.LerpLookAtWithAxisControl(target.position, xRotate, yRotate, t);
	}

	/// <summary>
	/// Lerp towards LookAt(Vector3) with control over whether you rotate on X and Y.
	/// </summary>
	/// <param name="transform">Transform.</param>
	/// <param name="targetPos">Target position.</param>
	/// <param name="xRotate">If set to <c>true</c> x rotate.</param>
	/// <param name="yRotate">If set to <c>true</c> y rotate.</param>
	/// <param name="t">T.</param>
	public static void LerpLookAtWithAxisControl(this Transform transform, Vector3 targetPos, bool xRotate, bool yRotate, float t)
	{
		if(!xRotate && !yRotate)
		{
			Debug.LogError("Error: No LookAt because both axes are disabled");
			Debug.DebugBreak();
		}

		Vector3 lookPos = targetPos;
		lookPos.y = xRotate ? targetPos.y : transform.position.y;
		lookPos.x = yRotate ? targetPos.x : transform.position.x;
		
		transform.LerpLookAt(lookPos, t);
	}

	/// <summary>
	/// Convert vector into transform space.
	/// </summary>
	/// <returns>The vector relative.</returns>
	/// <param name="transform">Transform.</param>
	/// <param name="vec">Vec.</param>
	public static Vector3 SetVectorRelative(this Transform transform, Vector3 vec)
	{
		return transform.TransformDirection (vec);
	}

	/// <summary>
	/// Convert vector into transform space.
	/// </summary>
	/// <returns>The vector relative.</returns>
	/// <param name="transform">Transform.</param>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	public static Vector3 SetVectorRelative(this Transform transform, float x, float y, float z)
	{
		return transform.TransformDirection (x, y, z);
	}

	#endregion

	#region Physics

	/// <summary>
	/// Raycast and return HitInfo.
	/// </summary>
	/// <param name="hit">Hit.</param>
	/// <param name="ray">Ray.</param>
	/// <param name="layer">Layer.</param>
	/// <param name="dist">Dist.</param>
	public static RaycastHit RaycastAndGetInfo(Ray ray, LayerMask layer, float dist = Mathf.Infinity)
	{
		return RaycastAndGetInfo (ray.origin, ray.direction, layer, dist);
	}

	/// <summary>
	/// Raycast and return HitInfo.
	/// </summary>
	/// <returns>The and get info.</returns>
	/// <param name="origin">Origin.</param>
	/// <param name="dir">Dir.</param>
	/// <param name="layer">Layer.</param>
	/// <param name="dist">Dist.</param>
	public static RaycastHit RaycastAndGetInfo(Vector3 origin, Vector3 dir, LayerMask layer, float dist = Mathf.Infinity)
	{
		RaycastHit hit;
		Physics.Raycast(origin, dir, out hit, dist, layer);
		return hit;
	}

	/// <summary>
	/// Raycast and return HitInfo.
	/// </summary>
	/// <returns>The and get info.</returns>
	/// <param name="ray">Ray.</param>
	/// <param name="dist">Dist.</param>
	public static RaycastHit RaycastAndGetInfo( Ray ray, float dist = Mathf.Infinity)
	{
		return RaycastAndGetInfo (ray.origin, ray.direction, dist);
	}

	/// <summary>
	/// Raycast and return HitInfo.
	/// </summary>
	/// <returns>The and get info.</returns>
	/// <param name="origin">Origin.</param>
	/// <param name="dir">Dir.</param>
	/// <param name="dist">Dist.</param>
	public static RaycastHit RaycastAndGetInfo(Vector3 origin, Vector3 dir, float dist = Mathf.Infinity)
	{
		RaycastHit hit;
		Physics.Raycast (origin, dir, out hit, dist);
		return hit;
	}

	#endregion

	#region Coroutines

	/// <summary>
	/// Wait the specified time.
	/// </summary>
	/// <param name="time">Time.</param>
	public static YieldInstruction Wait(float time)
	{
		return new WaitForSeconds (time);
	}

	#endregion

	/// <summary>
	/// Use this in place of Monobehaviour's Instantiate to get GameObject reference without manually casting to GameObject.
	/// </summary>
	/// <param name="obj">Object.</param>
	public static GameObject Instantiate(GameObject obj)
	{
		return (GameObject)MonoBehaviour.Instantiate (obj);
	}

	/// <summary>
	/// Use this in place of Monobehaviour's Instantiate to get GameObject reference without manually casting to GameObject.
	/// </summary>
	/// <param name="obj">Object.</param>
	public static GameObject Instantiate(GameObject obj, Vector3 pos, Quaternion rot)
	{
		return (GameObject)MonoBehaviour.Instantiate (obj, pos, rot);
	}
}
