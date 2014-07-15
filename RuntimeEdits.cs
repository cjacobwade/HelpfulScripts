using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
using System.IO;

public class RuntimeEdits : MonoBehaviour 
{
	[MenuItem ("RuntimeEdits/Save Edits")]
	static void SaveEdits()
	{
		//Do save popup?
		if(EditorUtility.DisplayDialog("Save?", "This will save the changes you've made to the scene.", "Save", "Cancel"))
		{
			// Get a list of all our changes
			RuntimeChanges[] rcObjs = FindObjectsOfType<RuntimeChanges>() as RuntimeChanges[];
			GameObject[] edits = new GameObject[rcObjs.Length];

			for(int i = 0; i < rcObjs.Length; i++)
			{
				edits[i] = rcObjs[i].gameObject;
				Debug.Log(edits[i].name);
			}

			// Create a folder Editor\RuntimeChanges
			if(!Directory.Exists("Assets/Resources/RuntimeChanges"))
			{
				AssetDatabase.CreateFolder("Assets/Resources", "RuntimeChanges");
			}

			int counter = 0;

			List<string> objNames = new List<string>();

			// Save all our saved edits into the RuntimeChanges folder
			foreach(GameObject go in edits)
			{
				if(go.GetComponent<RuntimeChanges>().saveChanges)
				{
					// Check to make sure names will be different
					int nameCount = 1;
					string origName = go.name;
					while(objNames.Contains(go.name))
					{
						go.name = origName + nameCount;
						nameCount++;
					}

					objNames.Add(go.name);
					DestroyImmediate(go.GetComponent<RuntimeChanges>());
					PrefabUtility.CreatePrefab("Assets/Resources/RuntimeChanges/" + go.name + ".prefab", go);
					counter++;
				}
			}

			// End play mode
			EditorApplication.isPaused = false;
			EditorApplication.isPlaying = false;
		}

		EditorApplication.playmodeStateChanged += ApplySavedChanges;
	}

	[MenuItem ("RuntimeEdits/Save Edits", true)]
	static bool SaveEditsValidate()
	{
		return EditorApplication.isPlaying;
	}

	static void ApplySavedChanges()
	{
		// This makes sure this function only gets called once
		EditorApplication.playmodeStateChanged -= ApplySavedChanges;

		// Spawn all of our edits into the scene
		foreach(Object o in Resources.LoadAll("RuntimeChanges"))
		{
			GameObject go = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)o);
			PrefabUtility.DisconnectPrefabInstance(go);
		}
		
		// Delete folder Editor\RuntimeChanges along with any prefabs within
		FileUtil.DeleteFileOrDirectory("Assets/Resources/RuntimeChanges");
		AssetDatabase.Refresh ();
	}
}
