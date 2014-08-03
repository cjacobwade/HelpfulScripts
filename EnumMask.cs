using UnityEngine;
using UnityEditor;

public class EnumMaskAttribute : PropertyAttribute 
{
	public readonly Effects effect;
	public EnumMaskAttribute () 
	{

	}
}

[CustomPropertyDrawer (typeof (EnumMaskAttribute))]
public class EnumMaskDrawer : PropertyDrawer 
{
	// These constants describe the height of the help box and the text field.
	const int helpHeight = 5;
	const int textHeight = 16;
	
	// Provide easy access to the RegexAttribute for reading information from it.
	EnumMaskAttribute enumMaskAttribute { get { return ((EnumMaskAttribute)attribute); } }

	public override float GetPropertyHeight (SerializedProperty prop, GUIContent label) 
	{
		return base.GetPropertyHeight (prop, label) + helpHeight;
	}

	// Here you can define the GUI for your property drawer. Called by Unity.
	public override void OnGUI (Rect position, SerializedProperty prop, GUIContent label) 
	{
		prop.intValue = (int)(Effects)EditorGUI.MaskField (position, prop.enumValueIndex, prop.enumNames);
	}
}
