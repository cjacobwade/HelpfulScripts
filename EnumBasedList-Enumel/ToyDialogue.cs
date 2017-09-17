using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ToyDialogue : Enumel<ToyType>
{
	public ToyDialogue(ToyType inToyType) : base(inToyType)
	{ }

	public string[] discoverDialogue = null;
	public string[] revisitDialogue = null;

	public ToyType GetToy()
	{ return enumType; }

	public static ToyDialogue Create(ToyType inToyType)
	{ return new ToyDialogue(inToyType); }
}
