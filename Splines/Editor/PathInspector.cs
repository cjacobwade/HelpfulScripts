using UnityEditor;
using UnityEngine;

namespace Luckshot.Splines
{
	[CustomEditor(typeof(Path), true)]
	public class PathInspector : Editor
	{
		protected const float handleSize = 0.04f;
		protected const float pickSize = 0.06f;

		protected Path path = null;
		protected Transform handleTransform = null;
		protected Quaternion handleRotation = Quaternion.identity;
		protected int selectedIndex = -1;

		public override void OnInspectorGUI()
		{
			if (selectedIndex == -1)
				base.OnInspectorGUI();

			path = target as Path;
			EditorGUI.BeginChangeCheck();
			bool loop = EditorGUILayout.Toggle("Loop", path.Loop);

			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(path, "Toggle Loop");
				EditorUtility.SetDirty(path);
				path.Loop = loop;
			}

			if (selectedIndex >= 0 && selectedIndex < path.PointCount)
			{
				DrawSelectedPointInspector();
			}

			if (GUILayout.Button("Add Point"))
			{
				Undo.RecordObject(path, "Add Point");
				path.AddPoint();
				EditorUtility.SetDirty(path);
			}

			if (path.PointCount > 1 && GUILayout.Button("Remove Point"))
			{
				Undo.RecordObject(path, "Remove Point");
				path.RemovePoint();
				EditorUtility.SetDirty(path);
			}
		}

		protected virtual void DrawSelectedPointInspector()
		{
			GUILayout.Label("Selected Point");
			EditorGUI.BeginChangeCheck();
			Vector3 point = EditorGUILayout.Vector3Field("Position", path.Points[selectedIndex]);

			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(path, "Move Point");
				EditorUtility.SetDirty(path);
				path.Points[selectedIndex] = point;
			}

			// Draw Node Data if ADV SPLINE
			// Would be in AdvSplineInspector, except AdvSpline is generic meaning
			// we can't use the CustomEditor attribute and force child types to use its inspector
			// which means either this is here or every AdvSpline child needs an associated TypeInspector script
			SerializedObject so = serializedObject;
			SerializedProperty nodeDataProp = so.FindProperty(string.Format("{0}.Array.data[{1}]", "_nodeData", selectedIndex));
			if (nodeDataProp != null)
			{
				EditorGUI.BeginChangeCheck();

				EditorGUILayout.PropertyField(nodeDataProp, new GUIContent("Node Data"), true);

				if (EditorGUI.EndChangeCheck())
					so.ApplyModifiedProperties();
			}
		}

		protected virtual void OnSceneGUI()
		{
			path = target as Path;
			handleTransform = path.transform;
			handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;

			for (int i = 0; i < path.PointCount; i++)
				ShowPoint(i);

			for (int i = 1; i < path.PointCount; i++)
			{
				Handles.color = Color.white;
				Handles.DrawLine(
					path.transform.TransformPoint(path.Points[i]),
					path.transform.TransformPoint(path.Points[i - 1]));
			}

			if (path.Loop)
			{
				Handles.DrawLine(
					path.transform.TransformPoint(path.Points[0]),
					path.transform.TransformPoint(path.Points[path.Points.Length - 1]));
			}
		}

		protected virtual Vector3 ShowPoint(int index)
		{
			Vector3 point = handleTransform.TransformPoint(path.Points[index]);
			float size = HandleUtility.GetHandleSize(point);
			if (index == 0)
			{
				size *= 2f;
			}

			Handles.color = Color.white;
			if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap))
			{
				selectedIndex = index;
				Repaint();
			}

			if (selectedIndex == index)
			{
				EditorGUI.BeginChangeCheck();
				point = Handles.DoPositionHandle(point, handleRotation);
				if (EditorGUI.EndChangeCheck())
				{
					Undo.RecordObject(path, "Move Point");
					EditorUtility.SetDirty(path);
					path.Points[index] = handleTransform.InverseTransformPoint(point);
				}
			}

			return point;
		}
	}
}