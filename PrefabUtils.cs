using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using System.Linq;

public class PrefabUtils
{
  [MenuItem("Utilities/Prefabs/Revert All Selected", true)]
	static bool ValidateRevertSelectedPrefabs()
	{
		return Selection.gameObjects.Count(g => PrefabUtility.GetPrefabParent(g)) > 0;
	}
	#endregion

	[MenuItem("Utilities/Prefabs/Revert All Selected")]
	static void RevertSelectedPrefabs()
	{
		GameObject[] selectedGOs = Selection.gameObjects;
		Undo.RecordObjects(selectedGOs, "Revert All Selected");
		
		for(int i = 0; i < selectedGOs.Length; i++)
		{
			if (PrefabUtility.GetPrefabParent(selectedGOs[i]))
			{
				PrefabUtility.RevertPrefabInstance(selectedGOs[i]);
				EditorUtility.SetDirty(selectedGOs[i]);
			}
		}
	}
}
