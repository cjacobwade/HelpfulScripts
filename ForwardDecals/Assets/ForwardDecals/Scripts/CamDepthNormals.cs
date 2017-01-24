using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class CamDepthNormals : MonoBehaviour
{
	void Awake()
	{
		GetComponent<Camera>().depthTextureMode = DepthTextureMode.DepthNormals;
	}
}
