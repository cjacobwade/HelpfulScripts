using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using LinqTools;

[CustomPropertyDrawer(typeof(DialogueChunk))]
public class DialogueChunkDrawer : PropertyDrawer
{
	DateTime _lastGCTime = DateTime.Now;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginChangeCheck();

		int indentLevel = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		float startXMin = position.xMin;
		float startXMax = position.xMax;
		float fullWidth = position.width;

		SerializedProperty characterProp = property.FindPropertyRelative("character");

		GUIStyle boxSkin = new GUIStyle(GUI.skin.box);

		Color boxColor = Tweakables.Common.GetCharacterColor((CharacterType)characterProp.enumValueIndex);
		boxColor.a -= 0.6f;

		boxSkin.normal.background = EditorUtils.GetColoredTexture(boxColor);
		position.height = GetPropertyHeight(property, label);
		GUI.Box(position, new GUIContent(""), boxSkin);

		position.width = fullWidth / 2f;
		position.height = EditorGUIUtility.singleLineHeight;

		bool restrictedCharacters = false;
		if (Selection.activeGameObject)
		{
			List<string> restrictedNames = new List<string>();
			List<int> restrictedValues = new List<int>();

			Quest quest = Selection.activeGameObject.GetComponent<Quest>();
			if(quest)
			{
				restrictedCharacters = true;

				SerializedObject questSO = new SerializedObject(quest);
				SerializedProperty characterSetupsProp = questSO.FindProperty("_characterSetups");
				for(int i = 0; i < characterSetupsProp.arraySize; i++)
				{
					SerializedProperty characterSetupProp = characterSetupsProp.GetArrayElementAtIndex(i);
					SerializedProperty characterSetupCharacterProp = characterSetupProp.FindPropertyRelative("character");
					restrictedNames.Add(((CharacterType)characterSetupCharacterProp.intValue).ToString());
					restrictedValues.Add(characterSetupCharacterProp.intValue);
				}

				if (restrictedValues.Count > 0 && !restrictedValues.Contains(characterProp.intValue))
					characterProp.intValue = restrictedValues.First();

				characterProp.intValue = EditorGUI.IntPopup(position, characterProp.intValue, restrictedNames.ToArray(), restrictedValues.ToArray());
				questSO.ApplyModifiedProperties();
			}
		}
		
		if(!restrictedCharacters)
			characterProp.intValue = (int)(CharacterType)EditorGUI.EnumPopup(position, (CharacterType)characterProp.intValue);

		position.xMin = position.xMax;
		position.xMax = position.xMin + fullWidth / 2f;
		SerializedProperty chunkTypeProp = property.FindPropertyRelative("chunkType");
		DialogueChunk.ChunkType dialogueType = (DialogueChunk.ChunkType)EditorGUI.EnumPopup(position, (DialogueChunk.ChunkType)chunkTypeProp.intValue);
		chunkTypeProp.intValue = (int)dialogueType;

		position.xMin = startXMin;
		position.width = fullWidth * 0.95f;
		position.x += fullWidth * 0.025f;
		SerializedProperty dialogueProp = property.FindPropertyRelative("dialogue");
		position.height = EditorGUI.GetPropertyHeight(dialogueProp);
		EditorGUI.PropertyField(position, dialogueProp, new GUIContent(""));

		if (chunkTypeProp.enumValueIndex == (int)DialogueChunk.ChunkType.Choice)
		{
			EditorGUI.indentLevel = indentLevel - 1;
			SerializedProperty responsesProp = property.FindPropertyRelative("responses");
			position.y += position.height;
			position.height = EditorGUI.GetPropertyHeight(responsesProp);
			EditorGUI.PropertyField(position, responsesProp, responsesProp.isExpanded);

			if (responsesProp.arraySize > 3)
				responsesProp.arraySize = 3;
		}

		EditorGUI.indentLevel = indentLevel;

		SerializedProperty parentArrProp = GetParentArrProp(property);
		if (parentArrProp != null)
		{
			position.y += position.height;
			position.height = EditorGUIUtility.singleLineHeight;

			position.xMin = startXMin - 14f;
			position.xMax = startXMax;

			float prevLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 55f;

			// Emotes
			position.width = fullWidth/2f;
			SerializedProperty emoteProp = property.FindPropertyRelative("emote");
			EditorGUI.PropertyField(position, emoteProp, new GUIContent("Emote"), true);

			if((EmoteType)emoteProp.intValue != EmoteType.None)
			{
				EditorGUIUtility.labelWidth = 44f;
				position.x += position.width - 10f;
				position.width = fullWidth / 3f;

				SerializedProperty loopEmoteProp = property.FindPropertyRelative("loopEmote");
				EditorGUI.PropertyField(position, loopEmoteProp, new GUIContent("Loop"));
			}

			EditorGUIUtility.labelWidth = prevLabelWidth;

			for (int i = 0; i < parentArrProp.arraySize; i++)
			{
				SerializedProperty arrItem = parentArrProp.GetArrayElementAtIndex(i);
				if (SerializedProperty.EqualContents(arrItem, property))
				{
					float width = 20f;
					position.xMin = startXMax - width;
					position.xMax = startXMax;
					if (GUI.Button(position, "+"))
					{
						parentArrProp.InsertArrayElementAtIndex(i);
						break;
					}

					position.xMin = startXMax - width * 2f;
					position.xMax = startXMax - width;
					if (GUI.Button(position, "-"))
					{
						parentArrProp.DeleteArrayElementAtIndex(i);
						break;
					}

					position.xMin = startXMax - width * 3f;
					position.xMax = startXMax - width * 2f;
					if (GUI.Button(position, "▲"))
					{
						parentArrProp.MoveArrayElement(i, i - 1);
						break;
					}

					position.xMin = startXMax - width * 4f;
					position.xMax = startXMax - width * 3f;
					if (GUI.Button(position, "▼"))
					{
						parentArrProp.MoveArrayElement(i, i + 1);
						break;
					}
				}
			}
		}

		EditorGUI.EndChangeCheck();

		if ((DateTime.Now - _lastGCTime).Seconds > 10)
		{
			EditorUtility.UnloadUnusedAssetsImmediate();
			GC.Collect();

			_lastGCTime = DateTime.Now;
		}

		if (GUI.changed)
		{
			property.serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(property.serializedObject.targetObject);
		}
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float propHeight = 0f;

		SerializedProperty parentArrProp = GetParentArrProp(property);
		if (parentArrProp != null)
			propHeight += EditorGUI.GetPropertyHeight(parentArrProp, new GUIContent(parentArrProp.name), false);

		// Dialogue
		propHeight += EditorGUIUtility.singleLineHeight * 2f;

		SerializedProperty chunkTypeProp = property.FindPropertyRelative("chunkType");
		if(chunkTypeProp.enumValueIndex == (int)DialogueChunk.ChunkType.Choice)
		{ 
			SerializedProperty responsesProp = property.FindPropertyRelative("responses");
			propHeight += EditorGUI.GetPropertyHeight(responsesProp);
		}

		SerializedProperty emoteProp = property.FindPropertyRelative("emote");
		propHeight += EditorGUI.GetPropertyHeight(emoteProp, new GUIContent(""));

		return propHeight;
	}

	SerializedProperty GetParentArrProp(SerializedProperty property)
	{
		int j = 0;
		List<string> pathChunks = property.propertyPath.Split('.').ToList();

		int indexOfLastArrProp = 0;
		for (int k = pathChunks.Count - 1; k >= 0; k--)
		{
			if (pathChunks[k] == "Array")
			{
				indexOfLastArrProp = k;
				break;
			}
		}

		SerializedProperty parentArrProp = property.serializedObject.FindProperty(pathChunks[j++]);
		while (j != indexOfLastArrProp)
			parentArrProp = parentArrProp.FindPropertyRelative(pathChunks[j++]);

		return parentArrProp;
	}
}