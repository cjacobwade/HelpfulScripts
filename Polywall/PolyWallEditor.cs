using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor( typeof( PolyWall ) )]
public class PolyWallEditor : Editor 
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		
		if ( !Application.isPlaying )
		{
			PolyWall polyWall = (PolyWall)target;
			polyWall.OnValidate();
		}
	}
}
