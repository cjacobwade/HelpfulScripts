#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using LinqTools;

public static class EditorUtils
{
	public static bool Toggle(this SerializedObject so, string propName, string labelName, params GUILayoutOption[] options)
	{
		var prop = so.FindProperty(propName);
		prop.boolValue = EditorGUILayout.Toggle(labelName, prop.boolValue, options);
		return prop.boolValue;
	}

	public static bool Toggle(this SerializedObject so, string propName, params GUILayoutOption[] options)
	{
		var prop = so.FindProperty(propName);
		prop.boolValue = EditorGUILayout.Toggle(prop.boolValue, options);
		return prop.boolValue;
	}

	public static float FloatField(this SerializedObject so, string propName, string labelName, params GUILayoutOption[] options)
	{
		var prop = so.FindProperty(propName);
		prop.floatValue = EditorGUILayout.FloatField(labelName, prop.floatValue, options);
		return prop.floatValue;
	}

	public static float FloatField(this SerializedObject so, string propName, params GUILayoutOption[] options)
	{
		var prop = so.FindProperty(propName);
		prop.floatValue = EditorGUILayout.FloatField(prop.floatValue, options);
		return prop.floatValue;
	}

	public static int IntSlider(this SerializedObject so, string propName, string labelName, int leftValue, int rightValue, params GUILayoutOption[] options)
	{
		var prop = so.FindProperty(propName);
		prop.intValue = EditorGUILayout.IntSlider(labelName, prop.intValue, leftValue, rightValue, options);
		return prop.intValue;
	}

	public static int IntSlider(this SerializedObject so, string propName, int leftValue, int rightValue, params GUILayoutOption[] options)
	{
		var prop = so.FindProperty(propName);
		prop.intValue = EditorGUILayout.IntSlider(prop.intValue, leftValue, rightValue, options);
		return prop.intValue;
	}

	public static int EnumPopup<T>(this SerializedObject so, string propName, string labelName, params GUILayoutOption[] options)
	{
		var prop = so.FindProperty(propName);
		var enumValue = (System.Enum)System.Enum.ToObject(typeof(T), prop.intValue);
		prop.intValue = (int)((object)EditorGUILayout.EnumPopup(labelName, enumValue, options));
		return prop.intValue;
	}

	public static int EnumPopup<T>(this SerializedObject so, string propName, params GUILayoutOption[] options)
	{
		var prop = so.FindProperty(propName);
		var enumValue = (System.Enum)System.Enum.ToObject(typeof(T), prop.intValue);
		prop.intValue = (int)((object)EditorGUILayout.EnumPopup(enumValue, options));
		return prop.intValue;
	}

	public static bool PropertyField(this SerializedObject so, string propName, bool includeChildren = false, params GUILayoutOption[] options)
	{
		var prop = so.FindProperty(propName);
		return EditorGUILayout.PropertyField(prop, includeChildren, options);
	}

	public static T ObjectField<T>(this SerializedObject so, string propName, string labelName, bool allowSceneObjects, params GUILayoutOption[] layoutOptions) where T : Object
	{
		var prop = so.FindProperty(propName);
		prop.objectReferenceValue = EditorGUILayout.ObjectField(labelName,
			prop.objectReferenceValue, typeof(T), allowSceneObjects, layoutOptions);
		
		return (T)prop.objectReferenceValue;
	}

	public static T ObjectField<T>(this SerializedObject so, string propName, bool allowSceneObjects, params GUILayoutOption[] layoutOptions) where T : Object
	{
		var prop = so.FindProperty(propName);
		prop.objectReferenceValue = EditorGUILayout.ObjectField(prop.objectReferenceValue,
			typeof(T), allowSceneObjects, layoutOptions);

		return (T)prop.objectReferenceValue;
	}
}
#endif
