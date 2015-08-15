using UnityEngine;
using System.Collections;

[RequireComponent( typeof( PolygonCollider2D ), typeof( MeshFilter ), typeof( MeshRenderer ) )]
public class PolyWall : MonoBehaviour 
{
	protected MeshRenderer _meshRenderer = null;
	protected MeshRenderer meshRenderer
	{
		get
		{
			if ( !_meshRenderer )
			{
				_meshRenderer = GetComponent<MeshRenderer>();
				if ( !_meshRenderer )
				{
					_meshRenderer = gameObject.AddComponent<MeshRenderer>();
				}
			}
			
			return _meshRenderer;
		}
	}
	
	protected PolygonCollider2D _polygonCollider2D = null;
	protected PolygonCollider2D polygonCollider2D
	{
		get
		{
			if ( !_polygonCollider2D )
			{
				_polygonCollider2D = GetComponent<PolygonCollider2D>();
				if ( !_polygonCollider2D )
				{
					_polygonCollider2D = gameObject.AddComponent<PolygonCollider2D>();
				}
			}
			
			return _polygonCollider2D;
		}
	}
	
	Vector2[] _prevPoints = null;

	public void OnValidate()
	{
		if ( _prevPoints != polygonCollider2D.points )
		{
			FillShape();
			_prevPoints = polygonCollider2D.points;
		}
	}

	public void FillShape()
	{
		meshRenderer.enabled = true;
		
		Mesh mesh = new Mesh();
		
		Vector2[] points = polygonCollider2D.points;
		Vector3[] vertices = new Vector3[ polygonCollider2D.GetTotalPointCount() ];
		
		for( int j=0; j < polygonCollider2D.GetTotalPointCount(); j++ )
		{
			Vector2 actual = points[ j ];
			vertices[ j ] = new Vector3( actual.x, actual.y, 1 );
		}
		
		Triangulator tr = new Triangulator( points );
		int [] triangles = tr.Triangulate();
		
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		
		MeshFilter mf = GetComponent<MeshFilter>();
		if( !mf )
		{
			mf = gameObject.AddComponent<MeshFilter>();
		}
		mf.mesh = mesh;
	}
}
