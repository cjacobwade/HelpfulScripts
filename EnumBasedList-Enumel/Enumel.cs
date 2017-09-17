using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LinqTools;

public class Enumel<T> where T : struct
{
	public Enumel(T inEnumType)
	{
		name = inEnumType.ToString();
		enumType = inEnumType;
	}

	[HideInInspector]
	public string name = string.Empty;

	[HideInInspector]
	public T enumType = default(T);
}

public static class EnumelUtils
{
	public static void ValidateData<T, K>(List<K> data) where T : struct where K : Enumel<T>
	{
		Debug.Assert(typeof(T).IsEnum, "Must use enum for T");

		T[] enumTypes = (T[])System.Enum.GetValues(typeof(T));
		if (data.Count != enumTypes.Length)
		{
			List<K> dataToAdd = new List<K>();
			List<K> dataToRemove = new List<K>();
			for (int i = 0; i < enumTypes.Length; i++)
			{
				List<K> dataMatches = data.Where(cd => cd.enumType.Equals(enumTypes[i])).ToList();
				if (dataMatches.Count == 0)
				{
					System.Reflection.MethodInfo createMethod = typeof(K).GetMethod("Create");
					K newData = (K)createMethod.Invoke(null, new object[] { enumTypes[i] });
					dataToAdd.Add(newData);
				}
				else if (dataMatches.Count > 1)
				{
					for (int j = 1; j < dataMatches.Count; j++)
						dataToRemove.Add(dataMatches[j]);
				}
			}

			foreach (K toRemove in dataToRemove)
				data.Remove(toRemove);

			foreach (K toAdd in dataToAdd)
				data.Add(toAdd);

			data = data.OrderBy(cd => cd.enumType).ToList();
		}
	}
}
