using System;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Reflection;

public static class CopyFromBaseType
{
	[MenuItem("CONTEXT/Component/Copy Values From Base Type", true)]
	static bool ValidateCopyFromInheritedType(MenuCommand menuCommand)
	{
		var clickedComponent = menuCommand.context as Component;

		UnityEngine.Component[] components = clickedComponent.gameObject.GetComponents(typeof(MonoBehaviour));
		UnityEditorInternal.ComponentUtility.PasteComponentAsNew(clickedComponent.gameObject);
		var tempPasteComponent = clickedComponent.gameObject.GetComponents(typeof(MonoBehaviour)).Where(c => !components.Contains(c)).FirstOrDefault();
		if (tempPasteComponent)
		{
			Type type = tempPasteComponent.GetType();
			UnityEngine.Component.DestroyImmediate(tempPasteComponent);
			return clickedComponent.GetType().IsSubclassOf(type);
		}

		return false;
	}

	[MenuItem("CONTEXT/Component/Copy Values From Base Type")]
	static void CopyFromInheritedType(MenuCommand menuCommand)
	{
		var clickedComponent = menuCommand.context as Component;

		UnityEngine.Component[] components = clickedComponent.gameObject.GetComponents(typeof(MonoBehaviour));
		UnityEditorInternal.ComponentUtility.PasteComponentAsNew(clickedComponent.gameObject);
		var tempPasteComponent = clickedComponent.gameObject.GetComponents(typeof(MonoBehaviour)).Where(c => !components.Contains(c)).First();

		Type type = clickedComponent.GetType();
		while (type != null)
		{
			FieldInfo[] myObjectFields = type.GetFields(
			BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

			foreach (FieldInfo fi in myObjectFields)
			{
				fi.SetValue(clickedComponent, fi.GetValue(tempPasteComponent));
			}

			type = type.BaseType;
			if (type == typeof(MonoBehaviour))
				break;
		}

		UnityEngine.Component.DestroyImmediate(tempPasteComponent);
	}
}
