using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using InControl;
using UnityEngine.EventSystems;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using Fabric;
using System;
using I2.Loc;

public enum FacingMode
{
	FacePlayer,
	FaceForward,
	Stay
}

[System.Serializable]
public struct DialogueData
{
	public DialogueData(DialogueData copy)
	{
		dialogue = copy.dialogue;
		camSettings = copy.camSettings;
		target = copy.target;
		lookTarget = copy.lookTarget;
		offset = copy.offset;
		camWeight = copy.camWeight;
		snap = copy.snap;
		snapBack = copy.snapBack;
		facingMode = copy.facingMode;
		voiceEvent = copy.voiceEvent;
	}

	public DialogueData(DialogueChunk[] inDialogue, CameraSettings inCamSettings = null)
	{
		dialogue = inDialogue;
		camSettings = inCamSettings ?? new CameraSettings(CameraSettings.dialogue);
		target = null;
		lookTarget = null;
		offset = Vector3.zero;
		camWeight = 10000f;
		snap = false;
		snapBack = false;
		facingMode = FacingMode.Stay;
		voiceEvent = GameSound.NONE;
	}

	public DialogueChunk[] dialogue;
	public CameraSettings camSettings;
	public Transform target;
	public Transform lookTarget;
	public Vector3 offset;
	public float camWeight;
	public bool snap;
	public bool snapBack;
	public FacingMode facingMode;
	public GameSound voiceEvent;
}

public class PanelDialogue : PanelBase
{
	[SerializeField]
	RectTransform _dialogueBubble = null;

	[SerializeField]
	Image _background = null;

	[SerializeField]
	Image _gradient = null;

	[SerializeField]
	Image _stemGradient = null;

	[SerializeField]
	float _showTime = 0.3f;

	[SerializeField]
	AnimationCurve _showCurve = new AnimationCurve();

	Coroutine _showWidgetRoutine = null;

	[SerializeField]
	ScaleTween _wobbleTween = null;

	[SerializeField]
	RectTransform _stemBoundsRect = null;

	Transform _target = null;

	[SerializeField]
	Image _spriteStem = null;

	[SerializeField]
	float _stemLerpSpeed = 5f;

	[SerializeField]
	TextTweenIn _lineText = null;

	int _lineTextDefaultSize = 43;

	[SerializeField]
	GameObject _continuePrompt = null;

	float _textSpeed = 0.02f;

	[SerializeField]
	GameObject _optionLayout = null;

	[SerializeField]
	Button[] _optionButtons = null;

	TextMeshProUGUI[] _optionTexts = null;
	TextMeshProUGUI[] GetOptionTexts()
	{
		if(_optionTexts == null)
		{
			_optionTexts = new TextMeshProUGUI[_optionButtons.Length];
			for (int i = 0; i < _optionButtons.Length; i++)
				_optionTexts[i] = _optionButtons[i].GetComponentInChildren<TextMeshProUGUI>(true);
		}

		return _optionTexts;
	}

	float _optionTextDefaultSize = 30f;

	Vector3[] _rectScreenCorners = new Vector3[8];

	[SerializeField]
	float _moveToOpeningSpeed = 10f;

	Vector3 _smoothedTargetPos = Vector3.zero;

	[SerializeField]
	float _smoothTargetSpeed = 0.3f;

	bool _snapBubbleToAlonePoint = false;

	float _mouthFlapSpeed = 6.5f;

	[SerializeField]
	float _resetMouthTime = 0.4f;
	Coroutine _resetMouthRoutine = null;

	int _chunkIndex = 0;
	public void PageDialogueBack()
	{
		if (_readChunkRoutine != null)
			StopCoroutine(_readChunkRoutine);

		_readChunkRoutine = null;
		_lineText.text = string.Empty;
		_chunkIndex -= 2;
	}

	Coroutine _readChunkRoutine = null;

	Dictionary<string, string> _locDynamicText = new Dictionary<string, string>();
	public void SetLocDynamicText(string key, string value)
	{
		if (_locDynamicText.ContainsKey(key))
			_locDynamicText[key] = value;
		else
			_locDynamicText.Add(key, value);
	}

	void OnEnable()
	{
		_lineText.text = "";
		_dialogueBubble.gameObject.SetActive(false);
		_spriteStem.gameObject.SetActive(_target);

		for (int i = 0; i < _optionButtons.Length; i++)
			_optionButtons[i].gameObject.SetActive(false);

		_lineTextDefaultSize = _lineText.fontSize;
		_optionTextDefaultSize = GetOptionTexts()[0].fontSize;

		SetTextSize(PlatformServices.GetGameSave().GetSettings().textSize);

		UIManager.RefreshPlatformIcons(this);

		_continuePrompt.SetActive(false);
	}

	void Update()
	{
		if (_target && _spriteStem.gameObject.activeInHierarchy)
			PointStemAtTarget();
	}

	public void SetTextSize(MenuSettings.TextSize textSize)
	{
		_lineText.fontSize = Mathf.FloorToInt(_lineTextDefaultSize * Tweakables.UI.textSizeModOptions[(int)textSize]);

		for (int i = 0; i < GetOptionTexts().Length; i++)
			GetOptionTexts()[i].fontSize = _optionTextDefaultSize * Tweakables.UI.textSizeModOptions[(int)textSize];
	}

	public void SetTextSpeed(MenuSettings.TextSpeed textSpeed)
	{
		_textSpeed = Tweakables.UI.textSpeedOptions[(int)textSpeed];
	}

	void UpdateBubblePos()
	{
		_rectScreenCorners[0] = Camera.main.ViewportToScreenPoint(Vector2.zero); // botleft
		_rectScreenCorners[1] = Camera.main.ViewportToScreenPoint(Vector2.right); // botright
		_rectScreenCorners[2] = Camera.main.ViewportToScreenPoint(Vector2.up); // topleft
		_rectScreenCorners[3] = Camera.main.ViewportToScreenPoint(Vector2.one); // topright
		_rectScreenCorners[4] = Camera.main.ViewportToScreenPoint(Vector2.up * 0.5f); // midleft
		_rectScreenCorners[5] = Camera.main.ViewportToScreenPoint(Vector2.right * 0.5f); //botmid
		_rectScreenCorners[6] = Camera.main.ViewportToScreenPoint(Vector2.up + Vector2.right * 0.5f); // topmid
		_rectScreenCorners[7] = Camera.main.ViewportToScreenPoint(Vector2.right + Vector2.up * 0.5f); // midright

		float maxDist = Mathf.NegativeInfinity;
		Vector3 mostAlonePoint = Vector2.zero;
		Vector3 headPos = _target ? Camera.main.WorldToScreenPoint(_target.position) : CameraManager.instance.transform.position;
		for (int j = 0; j < _rectScreenCorners.Length; j++)
		{
			Vector3 toCorner = _rectScreenCorners[j] - headPos;

			float upDot = Vector2.Dot(toCorner.normalized, Vector2.up);
			Vector3 midPointPos = headPos + toCorner * (Mathf.Abs(upDot) * 0.175f + 0.4f); // Account for screen being a rectangle rather than a square

			float dist = (midPointPos - headPos).magnitude;
			if (dist > maxDist)
			{
				maxDist = dist;
				mostAlonePoint = midPointPos;
			}
		}

		if (_snapBubbleToAlonePoint)
			_smoothedTargetPos = mostAlonePoint;
		else
			_smoothedTargetPos = Vector3.Lerp(_smoothedTargetPos, mostAlonePoint, Time.deltaTime * _smoothTargetSpeed);

		Vector3 targetPos = Vector3.zero;
		RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, _smoothedTargetPos, null, out targetPos);

		Vector3 toAlonePos = targetPos - _dialogueBubble.transform.position;
		Vector3 moveVec = toAlonePos * _moveToOpeningSpeed * Time.deltaTime;

		if (_snapBubbleToAlonePoint)
		{
			_snapBubbleToAlonePoint = false;
			_dialogueBubble.transform.position = targetPos;
		}
		else
			_dialogueBubble.transform.position += Vector3.ClampMagnitude(moveVec, toAlonePos.magnitude);

		_stemGradient.transform.position = _gradient.transform.position;
		_stemGradient.transform.rotation = _gradient.transform.rotation;
	}

	public Coroutine ReadDialogue(DialogueChunk[] dialogueChunks, FacingMode facingMode = FacingMode.Stay)
	{
		DialogueData dialogueData = new DialogueData(dialogueChunks);
		dialogueData.facingMode = facingMode;
		return ReadDialogue(dialogueData);
	}

	public Coroutine ReadDialogue(DialogueData dialogueData)
	{ return StartCoroutine(ReadDialogueRoutine(dialogueData)); }

	IEnumerator ReadDialogueRoutine(DialogueData dialogueData)
	{
		List<Player> players = PlayerManager.GetSpawnedPlayers();
		for (int i = 0; i < players.Count; i++)
			players[i].GetInput().bottomControlLens.AddRequest(new LensHandle<bool>(this, false));

		for (_chunkIndex = 0; _chunkIndex < dialogueData.dialogue.Length; _chunkIndex++)
		{
			if (_chunkIndex < 0)
				_chunkIndex = 0;

			Vector3 faceDirection = Vector3.zero;
			Transform prevLookTarget = null;

			bool hasNPC = false;
			PlayerNPCInput npc = null;
			List<PlayerNPCInput> spawnedNPCs = PlayerManager.GetNPCs();
			for (int j = 0; j < spawnedNPCs.Count; j++)
			{
				if (spawnedNPCs[j].gameObject.activeInHierarchy && spawnedNPCs[j].GetPlayer().GetInfo().character == dialogueData.dialogue[_chunkIndex].character)
				{
					hasNPC = true;
					npc = spawnedNPCs[j];

					faceDirection = npc.transform.forward.SetY(0f);
					if (dialogueData.facingMode != FacingMode.Stay)
					{
						faceDirection = (PlayerManager.GetLeadPlayer().transform.position - npc.transform.position).SetY(0f);
						if (dialogueData.facingMode != FacingMode.FaceForward)
							npc.TweenLookDirection(faceDirection);
					}

					prevLookTarget = npc.GetPlayer().GetPhysics().GetBody().lookTarget;
					npc.GetPlayer().GetPhysics().GetBody().lookTarget = dialogueData.lookTarget ?? CameraManager.instance.transform;

					break;
				}
			}

			string pattern = @"\{(.*?)\}";
			string dialogue = LocUtils.Translate(dialogueData.dialogue[_chunkIndex].dialogueTerm);

			#region Dialogue Commands
			// LOOK FOR COMMANDS IN DIALOGUE
			// OH LORD WE BEGIN OUR DESCENT INTO MADNESS
			MatchCollection matchCollection = Regex.Matches(dialogue, pattern);
			foreach (Match m in matchCollection)
			{
				dialogue = dialogue.Replace(m.Value, "");

				string value = m.Value.Remove(0, 1);
				value = value.Remove(value.Length - 1, 1);

				string[] sections = value.Split(',');
				if (sections[0] == "SetHat")
				{
					if (hasNPC && sections.Length == 2)
					{
						if (Enum.IsDefined(typeof(HatType), sections[1]))
						{
							HatType hatType = (HatType)Enum.Parse(typeof(HatType), sections[1]);
							npc.GetPlayer().SetHat(hatType);

							if(hatType != HatType.None)
								Tweakables.VFX.bubblePop.Spawn(ObjectPool.instance.transform, npc.GetPlayer().GetActiveHats()[0].transform.position);
						}
					}
					else if (sections.Length == 3)
					{
						if (Enum.IsDefined(typeof(CharacterType), sections[1]))
						{
							CharacterType character = (CharacterType)Enum.Parse(typeof(CharacterType), sections[1]);
							for (int j = 0; j < spawnedNPCs.Count; j++)
							{
								if (spawnedNPCs[j].GetPlayer().GetInfo().character == character)
								{
									if (Enum.IsDefined(typeof(HatType), sections[2]))
									{
										HatType hatType = (HatType)Enum.Parse(typeof(HatType), sections[2]);
										spawnedNPCs[j].GetPlayer().SetHat(hatType);

										if(hatType != HatType.None)
											Tweakables.VFX.bubblePop.Spawn(ObjectPool.instance.transform, spawnedNPCs[j].GetPlayer().GetActiveHats()[0].transform.position);
									}

									break;
								}
							}
						}
					}
				}
				else if(sections[0] == "SetSkin")
				{
					if (hasNPC && sections.Length == 2)
					{
						if (Enum.IsDefined(typeof(SkinType), sections[1]))
						{
							SkinType skinType = (SkinType)Enum.Parse(typeof(SkinType), sections[1]);
							npc.GetPlayer().SetSkin(skinType);
						}
					}
					else if (sections.Length == 3)
					{
						if (Enum.IsDefined(typeof(CharacterType), sections[1]))
						{
							CharacterType character = (CharacterType)Enum.Parse(typeof(CharacterType), sections[1]);
							for (int j = 0; j < spawnedNPCs.Count; j++)
							{
								if (spawnedNPCs[j].GetPlayer().GetInfo().character == character)
								{
									if (Enum.IsDefined(typeof(SkinType), sections[2]))
									{
										SkinType skinType = (SkinType)Enum.Parse(typeof(SkinType), sections[2]);
										spawnedNPCs[j].GetPlayer().SetSkin(skinType);
									}

									break;
								}
							}
						}
					}
				}
			}

			dialogueData.dialogue[_chunkIndex].dialogue = dialogue;
			#endregion

			SetCharacter(dialogueData.dialogue[_chunkIndex].character);

			// Stem Target
			Transform stemTarget = dialogueData.target;
			if (!stemTarget && hasNPC)
				stemTarget = npc.GetPlayer().GetPhysics().GetSausage().transform;

			SetTarget(stemTarget);

			// Camera Target
			Vector3 camOffset = dialogueData.offset;
			Transform camTarget = dialogueData.target;
			if (camTarget)
				faceDirection = camTarget.forward;
			else if(hasNPC)
			{
				camTarget = npc.transform.GetBoneTagInChildren(BoneLocation.SpineMiddle).transform;
				camOffset = Vector3.up * 0.5f;
			}

			Quaternion targetLookRot = Quaternion.identity;
			if (camTarget)
			{
				if(dialogueData.facingMode == FacingMode.FaceForward && PlayerManager.GetLeadPlayer())
					faceDirection = (PlayerManager.GetLeadPlayer().transform.position - camTarget.transform.position).SetY(0f);
				
				if (faceDirection != Vector3.zero)
					targetLookRot = Quaternion.LookRotation(faceDirection, Vector3.up);

				CameraManager.RegisterFollowTarget(camTarget, camOffset, dialogueData.camWeight);
			}

			CameraManager.SetOverrideSettings(dialogueData.camSettings, targetLookRot.eulerAngles.y);

			// Jump camera to end position, calculate where bubble will be and then jump it back
			Vector3 prevCamPos = CameraManager.instance.transform.position;
			Quaternion prevCamRot = CameraManager.instance.transform.rotation;

			_snapBubbleToAlonePoint = true;

			CameraManager.SimpleSnapToTarget();

			UpdateBubblePos();

			if (_chunkIndex == 0 && dialogueData.snap)
				CameraManager.SnapToTarget();
			
			if(!dialogueData.snap)
			{
				CameraManager.instance.transform.position = prevCamPos;
				CameraManager.instance.transform.rotation = prevCamRot;
			}

			// Do emote stuff
			if (hasNPC)
			{
				if (dialogueData.dialogue[_chunkIndex].emote != EmoteType.None)
				{
					EmoteTag emoteTag = npc.GetPayload().GetEmote(dialogueData.dialogue[_chunkIndex].emote);
					emoteTag.Play(dialogueData.dialogue[_chunkIndex].loopEmote);

					npc.headSwingScaleLens.AddRequest(
						new LensHandle<float>(this, npc.GetPayload().GetEmote(dialogueData.dialogue[_chunkIndex].emote).GetShakeForce()));

					if (emoteTag.GetUseEyeBlend())
						npc.GetPlayer().eyeBlendLens.AddRequest(new LensHandle<float>(this, emoteTag.GetEyeBlend()));
				}
				else
					npc.headSwingScaleLens.AddRequest(new LensHandle<float>(this, 0.3f));
			}

			int prevIndex = _chunkIndex; // Hack to go back to page backwards for debug

			_readChunkRoutine = StartCoroutine(ReadChunkRoutine(dialogueData.dialogue[_chunkIndex], npc, dialogueData.voiceEvent));
			while (_readChunkRoutine != null)
				yield return null;

			if (!gameObject.activeSelf || npc == null)
				yield break;

			_optionLayout.SetActive(false);

			foreach (var optionButton in _optionButtons)
				optionButton.gameObject.SetActive(false);

			if (dialogueData.dialogue[prevIndex].chunkType == DialogueChunk.ChunkType.Choice)
				dialogueData.dialogue[prevIndex].SaveResponse();

			if (hasNPC)
			{
				if (dialogueData.dialogue[prevIndex].emote != EmoteType.None && dialogueData.dialogue[prevIndex].loopEmote)
					npc.GetPayload().GetEmote(dialogueData.dialogue[prevIndex].emote).Stop();

				npc.GetPlayer().eyeBlendLens.RemoveRequestsWithContext(this);
				npc.GetPlayer().GetPhysics().GetBody().lookTarget = prevLookTarget;

				npc.headSwingScaleLens.RemoveRequestsWithContext(this);
			}

			CameraManager.SetOverrideSettings(null, null);

			if(camTarget)
				CameraManager.DeregisterFollowTarget(camTarget);

			if ((prevIndex == dialogueData.dialogue.Length - 1) && dialogueData.snapBack)
				CameraManager.SnapToTarget();
		}

		for (int i = 0; i < players.Count; i++)
		{
			if (players[i])
				players[i].GetInput().bottomControlLens.RemoveRequestsWithContext(this);
		}
	}

	IEnumerator ReadChunkRoutine(DialogueChunk dialogueChunk, PlayerNPCInput npc, GameSound voiceEventOverride)
	{
		bool hasNPC = (npc != null);

		if (_showWidgetRoutine != null)
			StopCoroutine(_showWidgetRoutine);

		_showWidgetRoutine = StartCoroutine(ShowWidgetRoutine(true));
		yield return _showWidgetRoutine;

		if(dialogueChunk.chunkType == DialogueChunk.ChunkType.Choice)
		{
			_optionLayout.SetActive(true);

			for (int i = 0; i < dialogueChunk.responses.Length; i++)
			{
				_optionButtons[i].gameObject.SetActive(true);
				GetOptionTexts()[i].color = GetOptionTexts()[i].color.SetA(0f);
				GetOptionTexts()[i].text = dialogueChunk.responses[i].response;
			}
		}

		string dialogueText = LocUtils.Translate(dialogueChunk.dialogueTerm);

		foreach (KeyValuePair<string, string> kvp in _locDynamicText)
		{
			if(dialogueText.Contains(kvp.Key))
				dialogueText = dialogueText.Replace(kvp.Key, LocUtils.Translate(kvp.Value));
		}

		if (_textSpeed > Mathf.Epsilon)
		{
			_lineText.text = dialogueText;

			if (_resetMouthRoutine != null)
			{
				StopCoroutine(_resetMouthRoutine);
				_resetMouthRoutine = null;
			}

			GameSound dialogueSound = voiceEventOverride;
			if (dialogueSound == GameSound.NONE)
			{
				string soundName = "SFX_UI_Dialogue_" + dialogueChunk.character.ToString();
				if (Enum.IsDefined(typeof(GameSound), soundName))
					dialogueSound = (GameSound)Enum.Parse(typeof(GameSound), soundName);
			}

			GameSounds.PostEvent2D(dialogueSound);

			float lineStartTime = Time.time;
			for (int i = 0; i < dialogueText.Length; i++)
			{
				// Skip rich text
				if(dialogueText[i] == '<')
				{
					while (dialogueText[i] != '>')
						i++;

					i++;
				}

				_lineText.ScaleCharacter(i);

				float timer = 0;
				while (timer < _textSpeed)
				{
					if (hasNPC && npc.GetPlayer().HasMouthBlend())
					{
						float timeSinceLineState = Time.time - lineStartTime;
						float mouthBlendWeight = (Mathf.Sin(timeSinceLineState * _mouthFlapSpeed * Mathf.PI) * 0.5f + 0.5f) * 100f;

						if(npc.GetPlayer().GetSkinnedMeshRenderer())
							npc.GetPlayer().GetSkinnedMeshRenderer().SetBlendShapeWeight(npc.GetPlayer().GetMouthBlendIndex(), mouthBlendWeight);
					}

					if(!GameManager.GSM.IsStateActive(GameStateType.PauseMenu) && InputManager.ActiveDevice.AnyButtonWasPressed)
					{
						_lineText.ForceAllScaled();
						goto lineFinished;
					}

					timer += Time.deltaTime;
					yield return null;
				}
			}

			lineFinished:

			if (hasNPC && npc.GetPlayer().HasMouthBlend())
				_resetMouthRoutine = StartCoroutine(ResetMouthRoutine(npc));

			if (dialogueSound != GameSound.NONE)
				GameSounds.PostEvent2D(dialogueSound, EventAction.StopSound);
		}
		else
			_lineText.text = dialogueText;

		if (dialogueChunk.chunkType == DialogueChunk.ChunkType.Default)
		{
			_continuePrompt.SetActive(true);

			yield return null;

			while(!InputManager.ActiveDevice.AnyButtonWasPressed)
				yield return null;

			if (_continuePrompt != null)
				_continuePrompt.SetActive(false);
		}
		else
			yield return StartCoroutine(DialogueOptionsRoutine(dialogueChunk));

		_lineText.text = string.Empty;

		if (_showWidgetRoutine != null)
			StopCoroutine(_showWidgetRoutine);

		_showWidgetRoutine = StartCoroutine(ShowWidgetRoutine(false));
		yield return _showWidgetRoutine;

		_readChunkRoutine = null;
	}

	IEnumerator ResetMouthRoutine(PlayerNPCInput npc)
	{
		float startBlend = npc.GetPlayer().GetSkinnedMeshRenderer().GetBlendShapeWeight(0);
		float endBlend = 0f;

		float timer = 0f;
		while(timer < _resetMouthTime)
		{
			timer += Time.deltaTime;

			float mouthBlendWeight = Mathf.Lerp(startBlend, endBlend, timer / _resetMouthTime);

			if(npc.GetPlayer().GetSkinnedMeshRenderer())
				npc.GetPlayer().GetSkinnedMeshRenderer().SetBlendShapeWeight(npc.GetPlayer().GetMouthBlendIndex(), mouthBlendWeight);

			yield return null;
		}

		_resetMouthRoutine = null;		
	}

	IEnumerator DialogueOptionsRoutine(DialogueChunk dialogueChunk)
	{
		for (int i = 0; i < dialogueChunk.responses.Length; i++)
		{
			GetOptionTexts()[i].color = GetOptionTexts()[i].color.SetA(1f);
			GetOptionTexts()[i].text = dialogueChunk.responses[i].response;
		}

		EventSystem.current.firstSelectedGameObject = _optionButtons[0].gameObject;
		EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);

		yield return new WaitForSeconds(0.2f);

		while (true)
		{
			if (EventSystem.current.currentSelectedGameObject == null)
			{
				EventSystem.current.firstSelectedGameObject = _optionButtons[0].gameObject;
				EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);
			}
			else
			{
				List<Player> players = PlayerManager.GetSpawnedPlayers();
				for (int j = 0; j < players.Count; j++)
				{
					Player player = players[j];
					if (player.GetInfo().IsHuman())
					{
						PlayerManualInput manualInput = (player.GetInput() as PlayerManualInput);
						if (manualInput && manualInput.JumpPressed())
							goto selectedOption;
					}
				}
			}

			yield return null;
		}

		selectedOption:

		// Mark which dialogue option we've chosen
		for(int i = 0; i < _optionButtons.Length; i++)
		{
			if(EventSystem.current.currentSelectedGameObject == _optionButtons[i].gameObject)
			{
				dialogueChunk.chosenResponseID = i;
				break;
			}
		}

		for (int i = 0; i < dialogueChunk.responses.Length; i++)
			_optionButtons[i].gameObject.SetActive(false);
	}

	IEnumerator ShowWidgetRoutine(bool show)
	{
		if(show)
			_snapBubbleToAlonePoint = true;

		yield return null;

		_dialogueBubble.gameObject.SetActive(true);
		_wobbleTween.Stop();

		Vector3 startScale = show ? Vector3.zero : _dialogueBubble.transform.localScale;
		Vector3 endScale = show ? Vector3.one : Vector3.zero;

		float showTimer = 0f;
		while(showTimer < _showTime)
		{
			showTimer += Time.deltaTime;

			float alpha = show ? _showCurve.Evaluate(showTimer / _showTime) : showTimer/_showTime;
			_dialogueBubble.transform.localScale = Vector3.LerpUnclamped(startScale, endScale, alpha);

			yield return null;
		}

		_dialogueBubble.gameObject.SetActive(show);

		if (show)
			_wobbleTween.Play();

		_showWidgetRoutine = null;
	}

	public void SetTarget(Transform inTransform)
	{
		_spriteStem.gameObject.SetActive(inTransform);
		_target = inTransform;
	}

	public void SetCharacter(CharacterType character)
	{
		Color characterColor = Tweakables.Common.GetCharacterColor(character);
		_background.color = characterColor;
		_spriteStem.color = characterColor;

		Color characterColor2 = Tweakables.Common.GetCharacterColor2(character);
		_gradient.color = characterColor2;
		_stemGradient.color = characterColor2;
	}

	void PointStemAtTarget()
	{
		Vector3 screenPos = Camera.main.WorldToScreenPoint(_target.position);

		Vector3 targetPos = Vector3.zero;
		RectTransformUtility.ScreenPointToWorldPointInRectangle(_stemBoundsRect, screenPos, null, out targetPos);
	
		Vector3[] fourCorners = new Vector3[4];
		_stemBoundsRect.GetWorldCorners(fourCorners);

		_spriteStem.transform.position = WadeUtils.Clamp(targetPos, fourCorners[0], fourCorners[2]);
		_spriteStem.transform.LerpLookAt2D(targetPos, Time.deltaTime * _stemLerpSpeed);
	}

	void OnDisable()
	{
		StopAllCoroutines();

		_readChunkRoutine = null;
		_resetMouthRoutine = null;

		 _lineText.fontSize = _lineTextDefaultSize;

		for(int i = 0; i < _optionTexts.Length; i++)
			_optionTexts[i].fontSize = _optionTextDefaultSize;
	}
}
