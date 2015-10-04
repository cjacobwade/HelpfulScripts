// File #1
// Put this in BlendShapeController.cs
using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class BlendShapeController : MonoBehaviour 
{
	SkinnedMeshRenderer _skinnedMeshRenderer = null;
	public SkinnedMeshRenderer skinnedMeshRenderer
	{
		get 
		{
			if ( !_skinnedMeshRenderer )
			{
				_skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
			}
			return _skinnedMeshRenderer;
		}
	}
}

// File #2
// Put this in BlendShapeControllerEditor.cs in Assets/Scripts/Editor folder
using UnityEngine;
using UnityEditor;

[CustomEditor( typeof( BlendShapeController ) )]
public class BlendShapeControllerEditor : Editor 
{
	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();

		BlendShapeController blendShapeController = (BlendShapeController)target;

		Mesh blendMesh = blendShapeController.skinnedMeshRenderer.sharedMesh;
		int blendShapeCount = blendMesh.blendShapeCount;

		for( int i = 0; i < blendShapeCount; i++ )
		{
			float currentBlendWeight = blendShapeController.skinnedMeshRenderer.GetBlendShapeWeight( i );
			float setBlendWeight = EditorGUILayout.Slider( blendMesh.GetBlendShapeName( i ), currentBlendWeight, 0f, 100f );
			blendShapeController.skinnedMeshRenderer.SetBlendShapeWeight( i, setBlendWeight );
		}
	}
}
