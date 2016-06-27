using UnityEngine;
using UnityEditor;

[CustomEditor( typeof( SkinnedMeshRenderer ) )]
public class SkinnedMeshRendererInspector : Editor 
{
	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI ();

		SkinnedMeshRenderer skinnedMesh = (SkinnedMeshRenderer)target;
		
		Mesh blendMesh = skinnedMesh.sharedMesh;
		int blendShapeCount = blendMesh.blendShapeCount;
		
		for( int i = 0; i < blendShapeCount; i++ )
		{
			float currentBlendWeight = skinnedMesh.GetBlendShapeWeight( i );
			float setBlendWeight = EditorGUILayout.Slider( blendMesh.GetBlendShapeName( i ), currentBlendWeight, 0f, 100f );
			skinnedMesh.SetBlendShapeWeight( i, setBlendWeight );
		}
	}
}