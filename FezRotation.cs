using UnityEngine;
using System.Collections;

public class FezRotation : MonoBehaviour 
{
	//Camera object transform
	public Transform cam;
	
	//Rotation speed
	public int rotSpeed;
	
	//Target Y rotation
	float targetY;
	
	//Flags
	bool rotate = false, clamp = false;
	
	// Use this for initialization
	void Start ()
	{
		StartCoroutine("CheckClamp");
	}
	
	// Update is called once per frame
	void Update ()
	{
		//If I'm not already rotating and I hit space
		if(Input.GetButtonDown("Jump") && !rotate)
		{
			//Set target rotation
			targetY = Mathf.Round(cam.eulerAngles.y/90)*90 + 90;	
			rotate = true;
		}
	
		//Rotate if flag for rotation is true
		if(rotate) Rotate();
	}
	
	void Rotate()
	{
		//If we've reached our target y rotation
		if(cam.eulerAngles.y >= targetY || clamp)
		{
			//Set rotation flag to false
			rotate = false;
			
			//Set clamp flag back to false
			clamp = false;
		}
		else
		{
			//Rotate camera around player
			cam.RotateAround(transform.position, Vector3.up, rotSpeed*Time.deltaTime);
		}
		
	}
	
	IEnumerator CheckClamp()
	{
		//Store temporary angle to compare with change after a frame
		float tmpCam = cam.eulerAngles.y;
		
		//Wait one frame before executing the following code
		yield return Time.deltaTime;
		
		//If the camera's y rotation changes by more than 180 degrees in one frame (this only happens when clamping from 360 to 0)
		if(Mathf.Abs(tmpCam - cam.eulerAngles.y) > 180)
			//Set flag clamp to true
			clamp = true;
		
		//Restart coroutine
		StartCoroutine("CheckClamp");
	}
}
