using UnityEngine;
using System;
using UnityEngine.Serialization;

namespace Luckshot.Splines
{
	public class Path : MonoBehaviour
	{
		[SerializeField, HideInInspector]
		private Vector3[] points = null;
		public Vector3[] Points
		{
			get
			{
				if (points == null || points.Length == 0)
					Reset();

				return points;
			}

			set
			{
				value = points;

				RecalculateClockwise();
			}
		}

		public int PointCount
		{ get { return Points.Length; } }

		[SerializeField, HideInInspector]
		protected bool loop = false;

		public bool Loop
		{
			get { return loop; }
			set { loop = value; }
		}

		private bool clockwise = false;
		public bool Clockwise
		{ get { return clockwise; } }

		private void Awake()
		{
			RecalculateClockwise();
		}

		[ContextMenu("Recalculate Clockwise")]
		private void RecalculateClockwise()
		{
			float sum = 0f;
			for (int i = 0; i < points.Length; i++)
			{
				Vector3 a = points[i];
				Vector3 b = points[(int)Mathf.Repeat(i - 1, points.Length)];

				sum += (b.x - a.x) * (b.z + a.z);
			}

			clockwise = sum > 0;
		}

		public Vector3 GetNormal(float t)
		{
			Vector3 velocity = GetVelocity(t);
			Vector3 normal = Vector3.Cross(Vector3.up, velocity.normalized);
			if (!clockwise)
				normal *= -1f;

			return normal;
		}

		public Vector3 GetPoint(float t)
		{
			t = Mathf.Clamp01(t);

			float alphaPerPoint = 1f / points.Length;

			int index = Mathf.FloorToInt(t / alphaPerPoint);
			index = Mathf.Clamp(index, 0, points.Length - 1);

			int nextIndex = index + 1;
			if (loop)
				nextIndex = (int)Mathf.Repeat(nextIndex, points.Length);
			else
				nextIndex = Mathf.Clamp(nextIndex, 0, points.Length - 1);

			float remainder = t % alphaPerPoint;

			Vector3 localPos = Vector3.Lerp(points[index], points[nextIndex], remainder / alphaPerPoint);
			return transform.TransformPoint(localPos);
		}

		public float GetNearestAlpha(Vector3 point, int iterations = 10)
		{
			int nearestIter = 0;
			float nearestAlpha = 0f;
			float nearestDistance = float.MaxValue;

			// Get a general spot along the spline that our point is near
			// This is more accurate then immediately halfing
			int totalIterations = iterations * PointCount;
			for (int i = 0; i < totalIterations; i++)
			{
				float iterAlpha = i / (float)totalIterations;

				Vector3 iterPos = GetPoint(iterAlpha);
				float iterDistance = Vector3.Distance(point, iterPos);

				if (iterDistance < nearestDistance)
				{
					nearestIter = i;
					nearestAlpha = iterAlpha;
					nearestDistance = iterDistance;
				}
			}

			// Within a range around closest large iteration,
			// keep halving range till we have a good approximation
			float minIterAlpha = Mathf.Max(0, nearestIter - 1) / (float)totalIterations;
			float maxIterAlpha = Mathf.Min(totalIterations, nearestIter + 1) / (float)totalIterations;
			for (int i = 0; i < totalIterations; i++)
			{
				float iterAlpha = Mathf.Lerp(minIterAlpha, maxIterAlpha, i / (float)totalIterations);

				Vector3 iterPos = GetPoint(iterAlpha);
				float iterDistance = Vector3.Distance(point, iterPos);

				if (iterDistance < nearestDistance)
				{
					nearestAlpha = iterAlpha;
					nearestDistance = iterDistance;
				}
			}

			return nearestAlpha;
		}

		public Vector3 GetNearestPathPoint(Vector3 position, int numIterations = 10)
		{ return GetPoint(GetNearestAlpha(position, numIterations)); }

		public Vector3 GetVelocity(float t)
		{
			t = Mathf.Clamp01(t);

			float alphaPerPoint = 1f / points.Length;

			int index = Mathf.FloorToInt(t / alphaPerPoint);
			index = Mathf.Clamp(index, 0, points.Length - 1);

			int nextIndex = index + 1;
			if (loop)
				nextIndex = (int)Mathf.Repeat(nextIndex, points.Length);
			else
				nextIndex = Mathf.Clamp(nextIndex, 0, points.Length - 1);

			return transform.TransformVector(points[nextIndex] - points[index]);
		}

		public virtual void AddPoint()
		{
			Vector3 point = Points[Points.Length - 1];
			Array.Resize(ref points, Points.Length + 1);
			point.x += 1f;
			Points[Points.Length - 1] = point;

			if (loop)
			{
				Points[Points.Length - 1] = Points[0];
			}
		}

		public virtual void RemovePoint()
		{
			Array.Resize(ref points, Points.Length - 1);

			if (loop)
			{
				Points[Points.Length - 1] = Points[0];
			}
		}

		public virtual void Reset()
		{
			points = new Vector3[]
			{
			new Vector3(1f, 0f, 0f),
			new Vector3(2f, 0f, 0f)
			};
		}
	}
}