using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class DialogueVars : MonoBehaviour
{
	[SerializeField]
	List<CharacterDialogues> _characterDialogues = new List<CharacterDialogues>();

	[SerializeField]
	List<ToyDialogue> _toyDialogues = new List<ToyDialogue>();

	public void OnValidate()
	{
		EnumelUtils.ValidateData<CharacterType, CharacterDialogues>(_characterDialogues);
		EnumelUtils.ValidateData<ToyType, ToyDialogue>(_toyDialogues);
	}

	public CharacterDialogues GetDialogues(CharacterType character)
	{ return _characterDialogues[(int)character]; }

	public ToyDialogue GetToyDialogue(ToyType inToy)
	{ return _toyDialogues[(int)inToy]; }
}
