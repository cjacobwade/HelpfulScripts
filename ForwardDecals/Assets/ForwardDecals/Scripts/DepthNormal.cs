using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DepthNormal : MonoBehaviour
{
	void Awake()
	{
		GetComponent<Camera>().depthTextureMode = DepthTextureMode.DepthNormals;
	}
}
