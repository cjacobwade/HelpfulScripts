// NOTE: THIS WILL TOTALLY NOT WORK IN YOUR PROJECT, BUT DOES EXPLAIN HOW AND WHY SERIALIZEDOBJECT/SERIALIZEDPROPERTY WORKS

using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(SausageSoccerPrefs))]
public class SausageSoccerPrefsInspector : Editor
{
	public override void OnInspectorGUI()
	{
		// There are some useful functions/classes that you should know to make your own custom inspector!

		// EditorGUILayout holds a bunch of utilities to display various fields in the inspector
		// LabelField shows a string
		// FloatField shows a editable float
		// Toggle shows an editable bool
		// IntSlider shows an editable int...

		// To show these values we first need to get the individual property we want to show
		// Any variable that is either public or uses serializefield can be accessed through the function FindProperty

		// In OnInspectorGUI from a class using the CustomEditor attibute we have access to serializedObject
		// which holds the data of the object were looking at in the inspector!

		// SerializedObject -> Class or Monobehaviour as you might be used to
		// SerializedProperty -> Variable/Property/Field of that Class/Monobehaviour

		// serializedObject.FindProperty(string propertyName) gives us a serializedProperty which we can then use in the
		// EditorGUILayouts as described above

		// Now with our found property (which is of type: SerializedProperty), we can read/write our data through a few accessors:
		// serializedProperty.intValue
		// serializedProperty.boolValue
		// serializedProperty.floatValue...

		// Interestingly this serializeObject/Property thing lets us edit these fields without making them publicly accessible
		// That's the magic of serialization in Unity I guess
		// With this ability you can hide/show information only when you want it
		// Below, when in Twitch Mode I hide most of these variables because they simply don't matter in Twitch Mode
		// Similarly, when in Attract Mode I reveal a bunch of data that's only relevant to that mode

		// Last thing, to save all your changes, it's important to save your changes with
		// serializedObject.ApplyModifiedProperties();
		
		// I also do the BeginChangeCheck and EndChangeCheck because Unity's documentation says it's good to do (probably for performance?)
		// But all this will totally still work with or without it


		EditorGUI.BeginChangeCheck();

		var headerStyle = new GUIStyle(EditorStyles.boldLabel);
		headerStyle.fontSize = 14;

		var heightOption = GUILayout.Height(21f);

		var skipMenu = serializedObject.FindProperty("_skipMenu");
		var initGameState = serializedObject.FindProperty("_initGameState");

		EditorGUILayout.LabelField("Twitch", headerStyle, heightOption);

		var twitchEnabled = serializedObject.FindProperty("_enableTwitch");
		twitchEnabled.boolValue = EditorGUILayout.Toggle("Enable Twitch", twitchEnabled.boolValue);
		if (twitchEnabled.boolValue)
		{
			// If we're in twitch mode, we don't care about most settings
			skipMenu.boolValue = true;
			initGameState.intValue = (int)GameStateType.TwitchNameVote;
		}
		else
		{			
			EditorGUILayout.LabelField("Debug Game Mode", headerStyle, heightOption);

			skipMenu.boolValue = EditorGUILayout.Toggle("Skip Menu", skipMenu.boolValue);
			if(skipMenu.boolValue)
			{
				// If we're skipping the menu, we want to know where to skip and who we should spawn
				initGameState.intValue = (int)((GameStateType)EditorGUILayout.EnumPopup("Init Game State", (GameStateType)initGameState.intValue));

				var skinOverride = serializedObject.FindProperty("_skinOverride");
				skinOverride.objectReferenceValue = (CharacterSkinAsset)EditorGUILayout.ObjectField("Skin Override",
					skinOverride.objectReferenceValue, typeof(CharacterSkinAsset), false);
				if(skinOverride.objectReferenceValue)
				{
					var hatOverride = serializedObject.FindProperty("_hatOverride");
					hatOverride.enumValueIndex = (int)((HatType)EditorGUILayout.EnumPopup("Hat Override", (HatType)hatOverride.enumValueIndex));
				}

				var aiCount = serializedObject.FindProperty("_debugAICount");
				aiCount.intValue = EditorGUILayout.IntSlider("Debug AI Count", aiCount.intValue, 0, 8);
			}

			EditorGUILayout.LabelField("Attract Mode", headerStyle, heightOption);

			var attractModeEnabled = serializedObject.FindProperty("_attractModeEnabled");
			attractModeEnabled.boolValue = EditorGUILayout.Toggle("Attract Mode Enabled", attractModeEnabled.boolValue);
			if (attractModeEnabled.boolValue)
			{
				// In attract mode we want to know what modes are valid, how many AI will spawn and how long it takes to enter attract
				var randomStates = serializedObject.FindProperty("_randomStates");
				EditorGUILayout.PropertyField(randomStates, true);

				var attractModeAICount = serializedObject.FindProperty("_attractModeAICount");
				attractModeAICount.intValue = EditorGUILayout.IntSlider("Attract Mode AI Count", attractModeAICount.intValue, 0, 8);

				var attractModeEnterTime = serializedObject.FindProperty("_attractModeEnterTime");
				attractModeEnterTime.floatValue = EditorGUILayout.FloatField("Attract Mode Enter Time", attractModeEnterTime.floatValue);
			}
		}

		EditorGUILayout.LabelField("Miscellaneous", headerStyle, heightOption);

		// Also here's the miscellaneous info that's always applicable
		var skipCountdowns = serializedObject.FindProperty("_skipCountdowns");
		skipCountdowns.boolValue = EditorGUILayout.Toggle("Skip Countdowns", skipCountdowns.boolValue);

		var gameTimeOverride = serializedObject.FindProperty("_gameTimeOverride");
		gameTimeOverride.floatValue = EditorGUILayout.FloatField("Game Time Override", gameTimeOverride.floatValue);

		var disableMusic = serializedObject.FindProperty("_disableMusic");
		disableMusic.boolValue = EditorGUILayout.Toggle("Disable Music", disableMusic.boolValue);

		var enableTutorial = serializedObject.FindProperty("_enableTutorialUI");
		enableTutorial.boolValue = EditorGUILayout.Toggle("Enable Tutorial UI", enableTutorial.boolValue);

		EditorGUI.EndChangeCheck();

		if(GUI.changed)
			serializedObject.ApplyModifiedProperties();
	}
}
