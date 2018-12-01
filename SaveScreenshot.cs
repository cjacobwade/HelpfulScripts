using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveScreenshot : MonoBehaviour
{
	int width = 1920;
	int height = 1080;

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.Space))
		{
			Camera cam = Camera.main;

			var screenRT = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);

			cam.targetTexture = screenRT;
			cam.Render();
			cam.targetTexture = null;

			Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false, true);

			RenderTexture.active = screenRT;
			tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			RenderTexture.active = null;

			screenRT.Release();
			tex.Apply();

			byte[] bytes = tex.EncodeToPNG();

			Debug.Log(Application.temporaryCachePath);
			File.WriteAllBytes(Application.temporaryCachePath + "/Screenshot.png", bytes);
		}
	}
}
