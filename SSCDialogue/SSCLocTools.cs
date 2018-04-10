using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using I2.Loc;
using LinqTools;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System;

public class SSCLocTools : MonoBehaviour
{
	static int GetEnglishLanguageIndex()
	{
		if (LocalizationManager.Sources.Count == 0)
		{
			Debug.LogWarning("No localizationSources found. Use Tools/I2Localization/Localization Manager Prefab to open it first");
			return -1;
		}

		return LocalizationManager.Sources[0].GetLanguageIndex("English");
	}

	[MenuItem("Sausage Soccer/Localization/Process All Prefabs")]
	public static void ProcessAllPrefabs()
	{
		int languageIndex = GetEnglishLanguageIndex();
		if (languageIndex == -1)
			return;

		List <GameObject> allPrefabs = PrefabLoader.LoadAllPrefabs("Assets/Prefabs");
		foreach(GameObject prefab in allPrefabs)
		{
			MonoBehaviour[] monoBehaviours = prefab.GetComponentsInChildren<MonoBehaviour>();
			foreach (MonoBehaviour mb in monoBehaviours)
			{
				if(mb == null)
				{
					Debug.Log(prefab.name + " referencing dead component", prefab);
					continue;
				}

				string chunks = string.Empty;

				SerializedObject mbSO = new SerializedObject(mb);
				SerializedProperty propIter = mbSO.GetIterator();
				propIter.Next(true);

				bool anyChanged = false;

				while (propIter.Next(true))
				{
					if (propIter.type == typeof(DialogueChunk).ToString() && propIter.isArray)
					{
						for (int j = 0; j < propIter.arraySize; j++)
						{
							SerializedProperty chunkProp = propIter.GetArrayElementAtIndex(j);
							SerializedProperty dialogueTermProp = chunkProp.FindPropertyRelative("dialogueTerm");

							string displayPropPath = chunkProp.propertyPath
								.Replace("_", "")
								.Replace(".Array.data", "")
								.Replace("[", "-")
								.Replace("]", "");

							displayPropPath = char.ToUpper(displayPropPath[0]) + displayPropPath.Substring(1);

							string termName = string.Format("Dialogue/{0}_{1}", prefab.name, displayPropPath);
							TermData termData = LocalizationManager.GetTermData(termName);
							if (termData == null)
							{
								termData = LocalizationManager.Sources[0].AddTerm(termName);
								chunks += termName + "\n";
							}

							string prevTranslation = termData.GetTranslation(languageIndex);
							string newTranslation = chunkProp.FindPropertyRelative("dialogue").stringValue;

							if (prevTranslation == string.Empty ||
								newTranslation != prevTranslation ||
								dialogueTermProp.stringValue != termName)
							{
								termData.SetTranslation(languageIndex, newTranslation);
								dialogueTermProp.stringValue = termName;
								anyChanged = true;
							}
						}
					}
				}

				if (anyChanged)
				{
					mbSO.ApplyModifiedProperties();
					EditorUtility.SetDirty(LocalizationManager.Sources[0]);
				}

				if (chunks != string.Empty)
					Debug.Log(prefab.name + ":\n" + chunks);
			}
		}

		AssetDatabase.SaveAssets();
	}

	static List<MonoScript> LoadAllScripts(string path)
	{
		if (path != "")
		{
			if (path.EndsWith("/"))
				path = path.TrimEnd('/');
		}

		DirectoryInfo dirInfo = new DirectoryInfo(path);
		FileInfo[] fileInf = dirInfo.GetFiles("*.cs");

		//loop through directory loading the game objects
		List<MonoScript> scripts = new List<MonoScript>();

		DirectoryInfo[] childDirInfos = dirInfo.GetDirectories();
		for (int i = 0; i < childDirInfos.Length; i++)
			scripts.AddRange(LoadAllScripts(childDirInfos[i].FullName));

		foreach (FileInfo fileInfo in fileInf)
		{
			string fullPath = fileInfo.FullName.Replace(@"\", "/");
			string assetPath = "Assets" + fullPath.Replace(Application.dataPath, "");
			MonoScript script = AssetDatabase.LoadAssetAtPath(assetPath, typeof(MonoScript)) as MonoScript;

			if (script != null)
				scripts.Add(script);
		}
		return scripts;
	}

	[MenuItem("Sausage Soccer/Localization/Process All Scripts")]
	public static void ProcessAllScripts()
	{
		int languageIndex = GetEnglishLanguageIndex();
		if (languageIndex == -1)
			return;

		string newTerms = "New Terms:\n";

		List<MonoScript> scripts = LoadAllScripts("Assets/Scripts");
		for (int i = 0; i < scripts.Count; i++)
		{
			string pattern = @"LocUtils.Translate\((.*?)\)";

			MatchCollection matchCollection = Regex.Matches(scripts[i].text, pattern);
			foreach (Match m in matchCollection)
			{
				if (!m.Value.Contains('"') || scripts[i].GetClass() == MethodBase.GetCurrentMethod().DeclaringType)
					continue;

				string termName = m.Value; 
				termName = termName.Replace("LocUtils.Translate(\"", "").Replace("\")", "");

// 				Debug.Log(scripts[i].name + ": " + termName);

				TermData termData = LocalizationManager.GetTermData(termName);
				if (termData == null)
				{
					termData = LocalizationManager.Sources[0].AddTerm(termName);
					termData.SetTranslation(languageIndex, termName);

					newTerms += termName + "\n";
				}
			}
		}

		Debug.Log(newTerms);
		AssetDatabase.SaveAssets();
	}

	[MenuItem("Sausage Soccer/Localization/Fix Broken Term Popups")]
	public static void FixBrokenTermPopups()
	{
		// Iterate over all prefabs, find TermPopup attributed properties, check if valid term, if not try to fix by adding category

		int languageIndex = GetEnglishLanguageIndex();
		if (languageIndex == -1)
			return;

		string fixedProperties = string.Empty;

		List<GameObject> allPrefabs = PrefabLoader.LoadAllPrefabs("Assets/Prefabs");
		foreach (GameObject prefab in allPrefabs)
		{
			MonoBehaviour[] monoBehaviours = prefab.GetComponentsInChildren<MonoBehaviour>();
			foreach (MonoBehaviour mb in monoBehaviours)
			{
				if (mb == null)
				{
					Debug.Log(prefab.name + " referencing dead component", prefab);
					continue;
				}

				SerializedObject mbSO = new SerializedObject(mb);

				bool anyChanged = false;

				FieldInfo[] fieldInfos = mb.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (FieldInfo fieldInfo in fieldInfos)
				{
					object[] attributes = fieldInfo.GetCustomAttributes(true);
					foreach (object attribute in attributes)
					{
						TermsPopup termsPopup = attribute as TermsPopup;
						if (termsPopup != null)
						{
							SerializedProperty termProp = mbSO.FindProperty(fieldInfo.Name);
							string termName = termProp.stringValue;

							TermData termData = LocalizationManager.GetTermData(termName);
							if (termData == null)
							{
								foreach (var kvp in LocalizationManager.Sources[0].mDictionary)
								{
									if (kvp.Value.IsTerm(termName, true))
									{
										termData = kvp.Value;
										termProp.stringValue = termData.Term;

										fixedProperties += termProp.propertyPath + "\n";
										anyChanged = true;
										break;
									}
								}

								break;
							}
						}
					}
				}

				if (anyChanged)
					mbSO.ApplyModifiedProperties();
			}
		}

		if (fixedProperties != string.Empty)
		{
			Debug.Log("Fixed Properties:\n" + fixedProperties);
			AssetDatabase.SaveAssets();
		}
	}

	#region ENUMS
	static void ProcessEnums<T>() where T : struct
	{
		if(!typeof(T).IsEnum)
		{
			Debug.LogError("Must Pass Enum Type");
			return;
		}

		int languageIndex = GetEnglishLanguageIndex();
		if (languageIndex == -1)
			return;

		string newTerms = "New Terms:\n";

		T[] enums = (T[])System.Enum.GetValues(typeof(T));
		for (int i = 0; i < enums.Length; i++)
		{
			string termName = string.Format("{0}/{1}", typeof(T).ToString(), enums[i].ToString());
			TermData termData = LocalizationManager.GetTermData(termName);
			if (termData == null)
			{
				termData = LocalizationManager.Sources[0].AddTerm(termName);
				termData.SetTranslation(languageIndex, enums[i].ToString().SplitIntoWordsByCase());

				newTerms += termName + "\n";
			}
		}

		Debug.Log(newTerms);
	}

	[MenuItem("Sausage Soccer/Localization/Enums/Process Hat Types")]
	static void ProcessHatTypes()
	{ ProcessEnums<HatType>(); }

	[MenuItem("Sausage Soccer/Localization/Enums/Process Skin Types")]
	static void ProcessSkinTypes()
	{ ProcessEnums<SkinType>(); }

	[MenuItem("Sausage Soccer/Localization/Enums/Process Character Types")]
	static void ProcessCharacterTypes()
	{ ProcessEnums<CharacterType>(); }

	[MenuItem("Sausage Soccer/Localization/Enums/Process Game Scenes")]
	static void ProcessGameScenes()
	{ ProcessEnums<GameScenes>(); }

	[MenuItem("Sausage Soccer/Localization/Enums/Process Team IDs")]
	static void ProcessTeamIDs()
	{ ProcessEnums<TeamID>(); }

	[MenuItem("Sausage Soccer/Localization/Enums/Process Rating")]
	static void ProcessRatings()
	{ ProcessEnums<MedalLevel>(); }

	[MenuItem("Sausage Soccer/Localization/Enums/Process Game State Types")]
	static void ProcessGameStateTypes()
	{ ProcessEnums<GameStateType>(); }

	[MenuItem("Sausage Soccer/Localization/Enums/Process Input Control Types")]
	static void ProcessInputControlTypes()
	{ ProcessEnums<InControl.InputControlType>(); }

	[MenuItem("Sausage Soccer/Localization/Enums/Process Player Input - Input Types")]
	static void ProcessPlayerInputInputTypes()
	{ ProcessEnums<PlayerInput.InputType>(); }
	#endregion
}
