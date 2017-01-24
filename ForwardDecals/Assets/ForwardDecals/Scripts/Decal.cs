using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Decal : MonoBehaviour
{
	[SerializeField]
	Material _material = null;
	public Material material
	{
		get
		{
			if(!_material)
				_material = GetComponent<Renderer>().sharedMaterial;

			return _material;
		}
	}

	void OnEnable()
	{
		DecalRegistry.instance.AddDecal(this);
	}

	void OnDisable()
	{
		DecalRegistry.instance.RemoveDecal(this);
	}

	void DrawGizmo(bool selected)
	{
		var col = new Color(0.0f, 0.7f, 1f, 1.0f);

		col.a = selected ? 0.3f : 0.1f;
		Gizmos.color = col;
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.DrawCube(Vector3.zero, Vector3.one);

		col.a = selected ? 0.5f : 0.2f;
		Gizmos.color = col;
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
	}

	public void OnDrawGizmos()
	{
		DrawGizmo(false);
	}
	public void OnDrawGizmosSelected()
	{
		DrawGizmo(true);
	}
}
