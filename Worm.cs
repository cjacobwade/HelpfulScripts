using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Worm : MonoBehaviour 
{
	[SerializeField] int playerNum = 0;

	[SerializeField] float moveSpeed = 5.0f;
	[SerializeField] float rotSpeed = 3.0f;

	[Range (1, 25)] [SerializeField] int segmentNum = 1;
	[SerializeField] GameObject segmentPrefab;
	[SerializeField] Transform segmentHolder;
	Transform[] segments = new Transform[0];
	
	[SerializeField] float wiggleTime = 0.7f;
	float wiggleTimer = -1.0f;
	bool lastHitLeft = false;

	[SerializeField] float moveTime = 1.0f;
	float moveTimer = 0.0f;
	[SerializeField] AnimationCurve moveForce;

	[SerializeField] float swingDamp = 0.97f;

	Vector3 inputVec = Vector3.zero;

	[SerializeField] int changeSegTestIndex = 0;
	[SerializeField] float changeSegTestScale = 2.0f;

	[SerializeField] float test;
	[SerializeField] bool debug;

	[SerializeField] float circleDist;

	Transform trans;

	Vector3[] vertices;
	Vector2[] uvs;
	int[] triangles;

	LineRenderer lr;

	// Use this for initialization
	void Awake () 
	{
		trans = transform;
		segments = new Transform[segmentNum];

		SetupBody();

		SetupMeshData();
		CreateMesh();

		lr = GetComponent<LineRenderer>();
	}

	void SetupBody()
	{
		for(int i = 0; i < segmentNum; i++)
		{
			GameObject segment = WadeUtils.Instantiate(segmentPrefab, 
			                                           trans.position - trans.up * (i + 1) * circleDist, 
			                                           segmentPrefab.transform.rotation);
			segment.name = "Segment" + i;
			segment.transform.parent = segmentHolder;
			segment.layer = segmentHolder.gameObject.layer;

			segments[i] = segment.transform;

			DistanceJoint2D segJoint = segment.GetComponent<DistanceJoint2D>();
			segJoint.connectedBody = (i == 0) ? rigidbody2D : segments[i - 1].rigidbody2D;
			segJoint.collideConnected = false;
			segJoint.distance *= circleDist;

			if(i == segmentNum - 1)
			{
				TrailRenderer tr = segment.AddComponent<TrailRenderer>();
				tr.startWidth = 0.3f;
				tr.endWidth = 0.1f;
				tr.material = GetComponent<MeshRenderer>().material;

				segment.rigidbody2D.isKinematic = true;
			}
		}
	}

	void SetupMeshData()
	{
		vertices = new Vector3[segments.Length * 5 + 12];
		uvs = new Vector2[vertices.Length];
		List<int> triangleList = new List<int>();
	
		float scale = trans.localScale.x;

		// Draw mesh head
		vertices[0] = trans.InverseTransformPoint (trans.position + segments[0].up * 0.5f * scale);

		// COS(PI * 3/8) = 0.924
		vertices[1] = trans.InverseTransformPoint (trans.position + (segments[0].up * 0.924f - segments[0].right * 0.383f) * 0.5f * scale);
		vertices[2] = trans.InverseTransformPoint (trans.position + (segments[0].up * 0.924f + segments[0].right * 0.383f) * 0.5f * scale); 

		// COS(PI/4) = 0.707
		vertices[3] = trans.InverseTransformPoint (trans.position + (segments[0].up - segments[0].right) * 0.707f * 0.5f * scale);
		vertices[4] = trans.InverseTransformPoint (trans.position + (segments[0].up + segments[0].right) * 0.707f * 0.5f * scale);

		vertices[5] = trans.InverseTransformPoint (trans.position - segments[0].right * 0.5f * scale);
		vertices[6] = trans.InverseTransformPoint (trans.position + segments[0].right * 0.5f * scale);

		triangleList.Add(0);
		triangleList.Add(1);
		triangleList.Add(2);

		triangleList.Add(2);
		triangleList.Add(1);
		triangleList.Add(3);

		triangleList.Add(2);
		triangleList.Add(3);
		triangleList.Add(4);

		triangleList.Add(4);
		triangleList.Add(3);
		triangleList.Add(5);

		triangleList.Add(4);
		triangleList.Add(5);
		triangleList.Add(6);

		for(int i = 0; i < segments.Length; i++)
		{
			Transform seg = segments[i];
			scale = seg.localScale.x;

			Vector3 vert9 = trans.InverseTransformPoint (seg.position + seg.right * scale/2.0f);
			Vector3 vert10 = trans.InverseTransformPoint (seg.position - seg.right * scale/2.0f);

			if(i == 0)
			{
				Vector3 vert7 = trans.InverseTransformPoint (seg.position + (seg.up + seg.right) * 0.5f * scale);
				Vector3 vert8 = trans.InverseTransformPoint (seg.position + (seg.up - seg.right) * 0.5f * scale);

				vertices[i * 4 + 7] = (vert7 + vert9) * 0.5f;
				vertices[i * 4 + 8] = (vert8 + vert10) * 0.5f;
			}
			else
			{
				Transform lastSeg = segments[i - 1];

				Vector3 averageRight = ((seg.position + lastSeg.position) * 0.5f) + ((seg.right + lastSeg.right) * 0.25f * (scale + lastSeg.localScale.x) * 0.5f);
				Vector3 averageLeft = ((seg.position + lastSeg.position) * 0.5f) - ((seg.right + lastSeg.right) * 0.25f * (scale + lastSeg.localScale.x) * 0.5f);

				vertices[i * 4 + 7] = trans.InverseTransformPoint (averageRight);
				vertices[i * 4 + 8] = trans.InverseTransformPoint (averageLeft);
			}

			vertices[i * 4 + 9] = vert9;
			vertices[i * 4 + 10] = vert10;

			if(i == 0)
			{
				triangleList.Add(6);
				triangleList.Add(5);
				triangleList.Add(i * 4 + 7);

				triangleList.Add(i * 4 + 8);
				triangleList.Add(i * 4 + 7);
				triangleList.Add(5);
			}
			else
			{
				triangleList.Add(i * 4 + 8);
				triangleList.Add(i * 4 + 7);
				triangleList.Add((i - 1) * 4 + 10);

				triangleList.Add(i * 4 + 7);
				triangleList.Add((i - 1) * 4 + 9);
				triangleList.Add((i - 1) * 4 + 10);
			}

			triangleList.Add(i * 4 + 7);
			triangleList.Add(i * 4 + 8);
			triangleList.Add(i * 4 + 9);

			triangleList.Add(i * 4 + 10);
			triangleList.Add(i * 4 + 9);
			triangleList.Add(i * 4 + 8);

			if(i == segments.Length - 1)
			{
				// COS(PI/4) = 0.707
				vertices[i * 4 + 11] = trans.InverseTransformPoint (seg.position - (seg.up - seg.right) * 0.707f * scale/2.0f);
				vertices[i * 4 + 12] = trans.InverseTransformPoint (seg.position - (seg.up + seg.right) * 0.707f * scale/2.0f);

				// COS(PI * 3/8) = 0.924
				vertices[i * 4 + 13] = trans.InverseTransformPoint (seg.position - (seg.up * 0.924f - seg.right * 0.383f) * scale/2.0f);
				vertices[i * 4 + 14] = trans.InverseTransformPoint (seg.position - (seg.up * 0.924f + seg.right * 0.383f) * scale/2.0f);

				vertices[i * 4 + 15] = trans.InverseTransformPoint (seg.position - seg.up * scale/2.0f);

				triangleList.Add(i * 4 + 12);
				triangleList.Add(i * 4 + 9);
				triangleList.Add(i * 4 + 10);

				triangleList.Add(i * 4 + 12);
				triangleList.Add(i * 4 + 11);
				triangleList.Add(i * 4 + 9);

				triangleList.Add(i * 4 + 13);
				triangleList.Add(i * 4 + 11);
				triangleList.Add(i * 4 + 12);

				triangleList.Add(i * 4 + 14);
				triangleList.Add(i * 4 + 13);
				triangleList.Add(i * 4 + 12);

				triangleList.Add(i * 4 + 15);
				triangleList.Add(i * 4 + 13);
				triangleList.Add(i * 4 + 14);
			}
		}

		triangles = triangleList.ToArray();
	}

	void CreateMesh()
	{
		Mesh mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;
		mesh.vertices = vertices;

		for(int i = 0; i < uvs.Length; i++) 
		{
			uvs[i] = new Vector2(vertices[i].x, vertices[i].y);
		}
		mesh.uv = uvs;

		mesh.triangles = triangles;
		mesh.RecalculateBounds();
	}

	void UpdateMesh()
	{
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		mesh.vertices = vertices;
		//mesh.uv = uvs;
		mesh.triangles = triangles;
	}

	// Update is called once per frame
	void Update () 
	{
		if(Input.GetKeyDown(KeyCode.T))
		{
			ChangeSegmentSize(changeSegTestIndex, changeSegTestScale);
			ChangeSegmentSize(changeSegTestIndex - 2, changeSegTestScale * 0.8f);
		}

		inputVec = new Vector3(Input.GetAxis("Horizontal-P" + playerNum), Input.GetAxis("Vertical-P" + playerNum), 0.0f);

		if(wiggleTimer > wiggleTime)
		{
			wiggleTimer = -1.0f;
		}

		SetupMeshData();

		for(int i = 0; i < segments.Length; i++)
		{
			segments[i].GetComponent<DistanceJoint2D>().distance = segments[i].localScale.x * circleDist;
		}

		UpdateMesh();
	}

	void FixedUpdate()
	{
		Movement();
		Rotation();
	}

	void Movement()
	{
		WiggleLogic();

		float clampedMoveTimer = Mathf.Clamp(moveTimer, 0.0f, moveTime);
		float appliedSpeed = moveSpeed * moveForce.Evaluate(clampedMoveTimer/moveTime); //* wiggleBoost;

		if(moveTimer > 0.0f)
		{
			rigidbody2D.velocity = trans.up * appliedSpeed;

			for(int i = 0; i < segments.Length; i++)
			{
				segments[i].rigidbody2D.velocity *= swingDamp;
			}
		}
		else 
		{
			rigidbody2D.velocity = Vector2.Lerp(rigidbody2D.velocity, Vector2.zero, Time.deltaTime);

			for(int i = 0; i < segments.Length; i++)
			{
				segments[i].rigidbody2D.velocity = Vector3.Lerp(segments[i].rigidbody2D.velocity, Vector2.zero, Time.deltaTime/2.0f);
			}
		}

		if(moveTimer >= 0.0f)
		{
			moveTimer -= Time.deltaTime;
		}
		else
		{
			moveTimer = 0.0f;
		}
	}

	// Check for alternating between left and right
	void WiggleLogic()
	{
		if(wiggleTimer < 0.0f)
		{
			if(inputVec.x > 0.1f)
			{
				wiggleTimer = 0.0f;
				lastHitLeft = false;
			}
			else if(inputVec.x < -0.1f)
			{
				wiggleTimer = 0.0f;
				lastHitLeft = true;
			}
		}
		else
		{
			wiggleTimer += Time.deltaTime;
			
			if(Mathf.Abs(inputVec.x) > 0.1f)
			{
				bool hitLeft = inputVec.x < 0.0f;
				if(lastHitLeft != hitLeft)
				{
					lastHitLeft = hitLeft;
					moveTimer = moveTime;
				}
			}
			else
			{
				wiggleTimer = 0.0f;
			}
		}
	}

	void Rotation()
	{
		trans.rotation *= Quaternion.Euler(0.0f, 0.0f, -rotSpeed * inputVec.x);

		//pieces[0].position = trans.position - trans.up;
		segments[0].position = Vector3.Lerp(segments[0].position, trans.position - trans.up, Time.deltaTime * 3.0f);
		segments[0].LookAt(trans.position, Vector3.forward);
		segments[0].rotation *= Quaternion.Euler(90.0f, 0.0f, 0.0f);
		
		for(int i = 1; i < segments.Length; i++)
		{
			//pieces[i].position = pieces[i-1].position - pieces[i-1].up;
			if(i < segments.Length - 1)
			{
				segments[i].position = Vector3.Lerp(segments[i].position, segments[i-1].position - segments[i-1].up, Time.deltaTime);
			}
			segments[i].LookAt(segments[i - 1].position, Vector3.forward);
			segments[i].rotation *= Quaternion.Euler(90.0f, 0.0f, 0.0f);
		}
	}

	void ChangeSegmentSize(int index, float scale)
	{
		if(index >= segments.Length)
		{
			Debug.Break();
			Debug.LogError("Segment index larger than number of segments");
		}

		float distance = 1.0f;

		// half scale previous piece
		// change previous seg distance
		if(index > 0)
		{
			segments[index - 1].localScale = Vector3.one * (0.5f + scale/2.0f);

			if(index > 1)
			{
				distance = (segments[index - 2].localScale.x + segments[index - 1].localScale.x)/2.0f;
			}
			else
			{
				distance = (trans.localScale.x + segments[index - 1].localScale.x)/2.0f;
			}

			segments[index - 1].GetComponent<DistanceJoint2D>().distance = distance;
		}

		// scale piece
		// change segment distance
		segments[index].localScale = Vector3.one * scale;

		if(index > 0)
		{
			distance = (segments[index - 1].localScale.x + segments[index].localScale.x)/2.0f;
		}
		else
		{
			distance = (trans.localScale.x + segments[index].localScale.x)/2.0f;
		}

		segments[index].GetComponent<DistanceJoint2D>().distance = distance;

		// half scale next piece
		// change next seg distance
		if(index < segments.Length - 2)
		{
			segments[index + 1].localScale = Vector3.one * (0.5f + scale/2.0f);
			distance = (segments[index].localScale.x + segments[index + 1].localScale.x)/2.0f;
			segments[index + 1].GetComponent<DistanceJoint2D>().distance = distance;
		}

		// change next next seg distance
		if(index < segments.Length - 3)
		{
			distance = (segments[index + 2].localScale.x + segments[index + 1].localScale.x)/2.0f;
			segments[index + 2].GetComponent<DistanceJoint2D>().distance = distance;
		}
	}

	void OnGUI()
	{
		if(debug)
		{
			for(int i = 0; i < vertices.Length; i++)
			{
				Vector3 screenVec = Camera.main.WorldToScreenPoint(trans.position + trans.rotation * vertices[i]);
				GUI.Label(new Rect(screenVec.x - 10.0f,Screen.height - (screenVec.y + 10.0f), 20.0f, 20.0f), i.ToString());
			}
		}
	}

	void OnDrawGizmos()
	{
		if(segments.Length > 0)
		{
			for(int i = 0; i < segments.Length; i++)
			{
				Gizmos.DrawWireSphere(segments[i].position, segments[i].localScale.x/2.0f);
			}
		}
	}
}
