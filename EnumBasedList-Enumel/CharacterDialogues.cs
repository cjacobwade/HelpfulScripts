using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterDialogues : Enumel<CharacterType>
{
	public CharacterDialogues(CharacterType inCharacter) : base(inCharacter)
	{ }

	public string[] tieBreakerDialogue = null;
	public string[] winTieBreakerDialogue = null;
	public string[] loseTieBreakerDialogue = null;

	public CharacterType GetCharacter()
	{ return enumType; }

	public static CharacterDialogues Create(CharacterType inCharacter)
	{ return new CharacterDialogues(inCharacter); }
}
