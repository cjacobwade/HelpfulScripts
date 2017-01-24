using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode, ImageEffectOpaque]
public class DecalRenderer : MonoBehaviour
{
	[SerializeField]
	Mesh _cubeMesh = null;

	Dictionary<Camera, CommandBuffer> _cameras = new Dictionary<Camera, CommandBuffer>();

	public void OnDisable()
	{
		foreach(var cam in _cameras)
		{
			if(cam.Key)
			{
				cam.Key.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, cam.Value);
			}
		}
	}

	public void OnRenderImage(RenderTexture source, RenderTexture dest)
	{
		var act = gameObject.activeInHierarchy && enabled;
		if(!act)
		{
			OnDisable();
			return;
		}

		var cam = Camera.current;
		if(!cam)
			return;

		if(!_cubeMesh)
			return;

		if(cam.depthTextureMode != DepthTextureMode.DepthNormals)
			cam.depthTextureMode = DepthTextureMode.DepthNormals;

		CommandBuffer buf = null;
		if(_cameras.ContainsKey(cam))
		{
			buf = _cameras[cam];
			buf.Clear();
		}
		else
		{
			buf = new CommandBuffer();
			buf.name = "Forward Decals";
			_cameras[cam] = buf;
			cam.AddCommandBuffer(CameraEvent.AfterForwardOpaque, buf);
		}

		var system = DecalRegistry.instance;
		buf.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
		for(int i = 0; i < system.decals.Count; i++)
			buf.DrawMesh(_cubeMesh, system.decals[i].transform.localToWorldMatrix, system.decals[i].material);

		Graphics.Blit(source, dest);
	}

#if UNITY_EDITOR
	void Update()
	{
		var sceneCams = SceneView.GetAllSceneCameras();
		var system = DecalRegistry.instance;

		for(int i = 0; i < sceneCams.Length; i++)
		{
			CommandBuffer sceneBuf = null;
			if(_cameras.ContainsKey(sceneCams[i]))
			{
				sceneBuf = _cameras[sceneCams[i]];
				sceneBuf.Clear();
			}
			else
			{
				sceneBuf = new CommandBuffer();
				sceneBuf.name = "Forward Decals";
				_cameras[sceneCams[i]] = sceneBuf;
				sceneCams[i].AddCommandBuffer(CameraEvent.AfterForwardOpaque, sceneBuf);
			}

			
			sceneBuf.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
			for(int j = 0; j < system.decals.Count; j++)
				sceneBuf.DrawMesh(_cubeMesh, system.decals[j].transform.localToWorldMatrix, system.decals[j].material);
		}
	}
#endif
}

public class DecalRegistry
{
	static DecalRegistry _instance;
	static public DecalRegistry instance
	{
		get
		{
			if(_instance == null)
				_instance = new DecalRegistry();

			return _instance;
		}
	}

	List<Decal> _decals = new List<Decal>();

	public List<Decal> decals
	{ get { return _decals; } }

	public void AddDecal(Decal d)
	{
		RemoveDecal(d);
		_decals.Add(d);
	}
	public void RemoveDecal(Decal d)
	{
		_decals.Remove(d);
	}
}