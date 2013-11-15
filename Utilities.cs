using UnityEngine;
using System.Collections;

public static class Utilities
{
    /*DESIRED UTILITIES:
        - Set position properties
        - Add to position properties
        - Set rotation properties
        - Add to rotation properties
        - Set euler angles
        - Get component
        - Default instantiate(object)
        - Fix look at
    */
    
    #region Set Position Properties
    public static void SetPosX(this Transform t, float newX)
    {
        t.position = new Vector3(newX, t.position.y, t.position.z);
    }
    
    public static void SetPosY(this Transform t, float newY)
    {
        t.position = new Vector3(t.position.x, newY, t.position.z);
    }
    
    public static void SetPosZ(this Transform t, float newZ)
    {
        t.position = new Vector3(t.position.x, t.position.y, newZ);
    }
    #endregion
	
	#region Increment Position Properties
    public static void AddPosX(this Transform t, float newX)
    {
        t.position = new Vector3(t.position.x + newX, t.position.y, t.position.z);
    }
    
    public static void AddPosY(this Transform t, float newY)
    {
        t.position = new Vector3(t.position.x, t.position.y + newY, t.position.z);
    }
    
    public static void AddPosZ(this Transform t, float newZ)
    {
        t.position = new Vector3(t.position.x, t.position.y, t.position.z + newZ);
    }
    #endregion
    
    #region Set Rotation Properties
    public static void SetRotX(this Transform t, float newX)
    {
        t.eulerAngles = new Vector3(newX, t.eulerAngles.y, t.eulerAngles.z);
    }
    
    public static void SetRotY(this Transform t, float newY)
    {
       t.eulerAngles = new Vector3(t.eulerAngles.x, newY, t.eulerAngles.z);
    }
    
    public static void SetRotZ(this Transform t, float newZ)
    {
       t.eulerAngles = new Vector3(t.eulerAngles.x, t.eulerAngles.y, newZ);
    }
    #endregion
	
	#region Increment Rotation
	
	public static void AddRot(this Transform t, Vector3 axis)
    {
        t.rotation *= Quaternion.Euler(axis);
    }
       
    #endregion
    
    #region LookAtOnAxis
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
