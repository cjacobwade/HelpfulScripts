using UnityEngine;
using System.Collections;

public class CreatePrimitive : MonoBehaviour 
{
	void Update()
	{
		if(Input.GetMouseButtonDown(0))
		{
			Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if(Physics.Raycast(r,out hit))
			{
				GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
				cube.transform.position = hit.point;
				cube.AddComponent(typeof(RuntimeChanges));
			}
		}
	}
}
