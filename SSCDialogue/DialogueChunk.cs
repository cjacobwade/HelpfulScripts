using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueChunk
{
	public enum ChunkType
	{
		Default,
		Choice
	}

	public DialogueChunk() { }

	public DialogueChunk(string inDialogue, CharacterType inCharacter = CharacterType.Scout, EmoteType inEmote = EmoteType.None, bool inLoopEmote = false)
	{
		dialogue = inDialogue;
		character = inCharacter;
		emote = inEmote;
		loopEmote = inLoopEmote;
	}

	public CharacterType character = CharacterType.Scout; // Should only be able to pick from characters present in setup
	public ChunkType chunkType = ChunkType.Default;

	[TextArea(2, 2)]
	public string dialogue = string.Empty;

	[HideInInspector]
	public string dialogueTerm = string.Empty;

	public EmoteType emote = EmoteType.None;
	public bool loopEmote = false;

	public DialogueResponseOption[] responses = null;

	[HideInInspector]
	public int chosenResponseID = 0;

	public DialogueResponseOption GetChosenResponse()
	{ return responses[chosenResponseID]; }

	public void SaveResponse()
	{
		if(responses[chosenResponseID].setter != null)
			responses[chosenResponseID].setter.SetData();
	}
}

[System.Serializable]
public class DialogueResponseOption
{
	[TextArea(1, 2)]
	public string response = string.Empty;
	public ProgressSetter setter = null;
}