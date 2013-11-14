using UnityEngine;
using System.Collections;

public static class Utilities
{
	/*DESIRED UTILITIES:
		- Set position properties
		- Set rotation properties
		- Set euler angles
		- Get component
		- Default instantiate(object)
		- Fix look at
	
	
	*/
	
	#region Set Position Properties
	public static void SetPosX(this Transform t, float newX)
	{
		t.position = new Vector3(t.position.x + newX, t.position.y, t.position.z);
	}
	
	public static void SetPosY(this Transform t, float newY)
	{
		t.position = new Vector3(t.position.x, t.position.y + newY, t.position.z);
	}
	
	public static void SetPosZ(this Transform t, float newZ)
	{
		t.position = new Vector3(t.position.x, t.position.y, t.position.z + newZ);
	}
	#endregion
	
	#region Set Euler
	public static void SetEuler(this Transform t, Vector3 angles)
	{
		//Clamp X
		if(angles.x > 360) angles.x -= 360;
		if(angles.x < 0) angles.x += 360;
		
		//Clamp Y
		if(angles.y > 360) angles.y -= 360;
		if(angles.y < 0) angles.y += 360;
		
		//Clamp Z
		if(angles.z > 360) angles.z -= 360;
		if(angles.z < 0) angles.z += 360;
		
		Debug.Log(angles);
		t.eulerAngles = angles;
	}
	
	
	#endregion
	
	#region Set Rotation Properties
	public static void SetRotX(this Transform t, float newX)
	{
		//READING FROM EULER ANGLES IS INACCURATE
		//NEED TO FIND A WORKAROUND FOR THIS
		Vector3 euler = t.eulerAngles;
		
		t.SetEuler(new Vector3(euler.x + newX, euler.y, euler.z));
	}
	
	public static void SetRotY(this Transform t, float newY)
	{
		t.SetEuler(new Vector3(t.eulerAngles.x, t.eulerAngles.y + newY, t.eulerAngles.z));
	}
	
	public static void SetRotZ(this Transform t, float newZ)
	{
		t.SetEuler(new Vector3(t.eulerAngles.x, t.eulerAngles.y, t.eulerAngles.z + newZ));
	}
	#endregion
	
	#region Fix Lookat
	public static void LookAtOnAxis(this Transform t, Vector3 targetPos, Vector3 axis)
	{
		Vector3 lookPos = t.position;
		
		//Rotating around X axis
		if(axis.x != 0) 
		{
			lookPos.z = targetPos.z;
			lookPos.y = targetPos.y;
		}
		
		//Rotating around Y axis
		if(axis.y != 0)
		{
			lookPos.z = targetPos.z;
			lookPos.x = targetPos.x;	
		}
		
		//Rotating around Z axis
		if(axis.z != 0)
		{
			lookPos.x = targetPos.x;
			lookPos.y = targetPos.y;	
		}
		
		t.LookAt(lookPos);
	}
	#endregion
}
