using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AidaHouseLogic : MonoBehaviour
{
	[Tooltip ("����� ������.")]
	public GameObject doorGO;
	[Tooltip ("������ � ����������� ��� ���� i-Start (����� �� ������).")]
	public GameObject speechIStartGO;

	public GameObject invisibleInkBookGO;
	public GameObject libraryMapInvisibleGO;
	public GameObject libraryMapVisibleGO;
	public GameObject libraryBookGO;

	public GameObject beforeLibraryAidaSpeechGO;
	public GameObject beforeLibraryAlexSpeechGO;

	public GameObject alcoholTrayEmptyGO;
	public GameObject alcoholTrayFullGO;
	public GameObject mortarEmptyGO;
	public GameObject mortarFullGO;
	[Tooltip ("������� �� �������, ������� ����� �������� � ������� Conversation Before Library. ���� ����������� �� ���� ������� ������ ����.")]
	public GameObject alcoholBottleToHide;

	[Tooltip ("�������� �����, � ������� ����� ������� � ������� '����� ������������ �� �����������'.")]
	public string goForGearSceneName;

	private GameObject _startPointGO;
	private AidaInnerDoor _door;
	private AudioSource _speechIStart;

	private Readable _invisibleInkBookReadable;
	private Readable _libraryMapInvisibleReadable;
	private Readable _libraryBookReadable;

	// �������� ������� ����� Fade In � ����� "Wait For Solguard".
	private float _waitForSolguardFadeInDelay = 2f;
	// ���� false, ����-�� ��� �� ����� ���������.
	private bool _waitForSolguardFadeInIsNeeded = true;
	// ��������� ����� ������� i1...i3 ��� ��������� ������� � ����������.
	private float _speechGearLoudness = .8f;
	// ����� ����� ������� ������� i1...i3 (��������� �������) � �������� �������� � ��� ������.
	private float _solguardIsBroughtSwitchPeriod = 14.1f;
	// ���������� ����� (Time.time), ����� ��� ����� ������ ��� ��������� �������. ��������������� � ������ �������.
	private float _solguardIsBroughtStartTime = 0f;
	// ����� �� ������� ���������� �������� (��� �������� �� ���� ���� � ��� ������) �� Fade In � ���� ������.
	//private float _goForGearFadeInDelay = 1.5f;
	// ���������� ����� Time.time, ����� ��� �������� ������� � ����� Go For Gear. ��������������� � ������ �������.
	//private float _goForGearStartTime = 0f;
	// �������� Fade In ����� �������� ����� � ���� ���� (����� ��� ���������� � ����������).
	private float _beforeLibraryFadeInDelay = 1f;
	// �������� ������� ������� ����� �������� ����� � ������� Conversation Before Library.
	private float _beforeLibrarySpeechDelay = .02f;
	// ����� �� ������ �������� ����� Conversation Before Library �� ���� �������, ��� ���� ����� ��������� ������������ ������ (�� ����� �������).
	private float _beforeLibraryStorySwitchTime = 16.2f;


	#region Awake

	private void Awake ()
	{
		StoryState storyState = GlobalManager.instance.storyState;

		if (storyState == StoryState.WaitForSolguard)
			AwakeOnWaitForSolguard ();
		else if (storyState == StoryState.ConversationBeforeLibrary)
			AwakeOnConversationBeforeLibrary ();
		else if (
			storyState == StoryState.LibraryHike ||
			storyState == StoryState.LibraryHikeAfterSorryAlex ||
			storyState == StoryState.LibraryHikeAfterWhenRevenge ||
			storyState == StoryState.Labyrinth
		)
			AwakeOnLibraryHikeCommon ();
	}

	private void AwakeDefault ()
	{
		CacheStartPointGO ();
	}

	private void AwakeOnWaitForSolguard ()
	{
		CacheStartPointGO ();
		_door = doorGO.GetComponent<AidaInnerDoor> ();

		_speechIStart = speechIStartGO.GetComponent<AudioSource> ();
		if (_speechIStart == null)
			throw new System.Exception ("AudioSource component not found.");

		_invisibleInkBookReadable = TryGetReadableComp (invisibleInkBookGO);
		_libraryMapInvisibleReadable = TryGetReadableComp (libraryMapInvisibleGO);
		_libraryBookReadable = TryGetReadableComp (libraryBookGO);
	}

	private void AwakeOnConversationBeforeLibrary ()
	{
		CacheStartPointGO ();
		_door = doorGO.GetComponent<AidaInnerDoor> ();
	}

	private void AwakeOnLibraryHikeCommon ()
	{
		CacheStartPointGO ();
		_door = doorGO.GetComponent<AidaInnerDoor> ();
	}

	#endregion


	#region Start

	private void Start ()
	{
		StoryState storyState = GlobalManager.instance.storyState;

		if (storyState == StoryState.WaitForSolguard)
			StartOnWaitForSolguard ();
		else if (storyState == StoryState.ConversationBeforeLibrary)
			StartOnConversationBeforeLibrary ();
		else if (
			storyState == StoryState.LibraryHike ||
			storyState == StoryState.LibraryHikeAfterSorryAlex ||
			storyState == StoryState.LibraryHikeAfterWhenRevenge ||
			storyState == StoryState.Labyrinth
		)
			StartOnLibraryHikeCommon ();
	}

	private void StartOnWaitForSolguard ()
	{
		// �������� ��������� � Start Point, ���� ��� ����.
		PlaceCharacterAtStartPoint ();
		// ��������� �������.
		StopAmbience ();
		// ��������� ������������ ������, ���� ���� �� ���������.
		_door.preventInteractions = true;
	}

	private void StartOnConversationBeforeLibrary ()
	{
		// �������� ��������� � Start Point, ���� ��� ����.
		PlaceCharacterAtStartPoint ();
		// ��������� ������������ ������, ���� ���� �� ���������.
		_door.preventInteractions = true;

		// ������ ��������� �����.
		libraryMapInvisibleGO.SetActive (false);
		// ���������� ����������� �����.
		libraryMapVisibleGO.SetActive (true);
		// ������ ������ �������� ��� ��������.
		alcoholTrayEmptyGO.SetActive (false);
		// ���������� �������� �� ��������� ��������� ������ ����.
		alcoholTrayFullGO.SetActive (true);
		// ������ ������ ������.
		mortarEmptyGO.SetActive (false);
		// ���������� ������ � ������ �����.
		mortarFullGO.SetActive (true);
		// ������ �������������� ������� �� �������.
		alcoholBottleToHide.SetActive (false);
	}

	private void StartOnLibraryHikeCommon ()
	{
		// �������� ��������� � Start Point, ���� ��� ����.
		PlaceCharacterAtStartPoint ();
		// ����������� ��������� ��������.
		StartDefaultFadeIn ();

		// ������ ��������� �����.
		libraryMapInvisibleGO.SetActive (false);
		// ���������� ����������� �����.
		libraryMapVisibleGO.SetActive (true);
		// ������ ������ �������� ��� ��������.
		alcoholTrayEmptyGO.SetActive (false);
		// ���������� �������� �� ��������� ��������� ������ ����.
		alcoholTrayFullGO.SetActive (true);
		// ������ ������ ������.
		mortarEmptyGO.SetActive (false);
		// ���������� ������ � ������ �����.
		mortarFullGO.SetActive (true);
		// ������ �������������� ������� �� �������.
		alcoholBottleToHide.SetActive (false);
	}

	#endregion


	#region Update

	private void Update ()
	{
		StoryState storyState = GlobalManager.instance.storyState;

		if (storyState == StoryState.WaitForSolguard)
			UpdateOnWaitForSolguard ();
		else if (storyState == StoryState.SolguardIsBrought)
			UpdateOnSolguardIsBrought ();
		else if (storyState == StoryState.ConversationBeforeLibrary)
			UpdateOnConversationBeforeLibrary ();
	}

	private void UpdateOnWaitForSolguard ()
	{
		GlobalManager globalManager = GlobalManager.instance;

		if (Time.timeSinceLevelLoad >= _waitForSolguardFadeInDelay && _waitForSolguardFadeInIsNeeded)
		{
			_waitForSolguardFadeInIsNeeded = false;
			globalManager.screenFader.speed = .5f;
			globalManager.screenFader.StartFadeIn ();
		}

		// ���� ����� "� ������ ��������� ��� �����" ��� �� �������������.
		if (!_door.iHaveToWaitIsPronounced)
		{
			// ���� ���� ����������, ��������� ������������ ������.
			OscClip sunClip = globalManager.audioManager.GetLayerByType (AudioLayerType.Speech).GetClipWithResourceType (OscAudioResourceEnum.Speech_Sun);
			if (sunClip == null)
			{
				if (_door.preventInteractions)
					_door.preventInteractions = false;
			} 
		}

		// ���� ��������� ��� ��� ������� � ���� �� ���� �� ��� � ������ ������ ����� �� ������, ����������� ����� � �������� �������� (����� ��������).
		if (globalManager.invisibleInkBookIsRead &&
			globalManager.libraryMapInvisibleIsRead &&
			globalManager.libraryBookIsRead &&
			_invisibleInkBookReadable.state == ReadableState.Normal &&
			_libraryMapInvisibleReadable.state == ReadableState.Normal &&
			_libraryBookReadable.state == ReadableState.Normal
		)
		{
			// ����������� �����.
			globalManager.storyState = StoryState.SolguardIsBrought;
			// ���������� �����, � ������� ������������� � ����� ����� (������ ������).
			_solguardIsBroughtStartTime = Time.time;

			// ��������� ���� ������ (3D).
			_speechIStart.Play ();
			// ��������� ���� ���� (���������).
			AudioLayer speechLayer = globalManager.audioManager.GetLayerByType (AudioLayerType.Speech);
			speechLayer.introSpeed = 1000f;
			speechLayer.IntroduceClip (OscAudioResourceEnum.Speech_Gear, false, _speechGearLoudness);
		}
	}

	private void UpdateOnSolguardIsBrought ()
	{
		// ���� ����� �� ������ ������� "� ������ �������" �������� ������� ������������ � ��� ������.
		if (Time.time - _solguardIsBroughtStartTime >= _solguardIsBroughtSwitchPeriod)
		{
			// ����������� �����.
			GlobalManager.instance.storyState = StoryState.GoForGear;
			// ���������� ������ ��������.
			//_goForGearStartTime = Time.time;

			// ����� ��������� ��������.
			ScreenFader screenFader = GlobalManager.instance.screenFader;
			screenFader.speed = 100f;
			screenFader.StartFadeToBlack (OnFadedToBlackOnSolguardIsBrought);
		}
	}

	// ������� �� ���� ���� � ��� ������ (����� �� �����������).
	private void OnFadedToBlackOnSolguardIsBrought ()
	{
		// ����� ����� ������ ����� �� ������.
		GlobalManager.instance.playerStartPointName = "Start Point (3)";

		StartCoroutine (LoadGoForGearScene ());
	}

	private void UpdateOnConversationBeforeLibrary ()
	{
		// �������� Fade In.
		if (Time.timeSinceLevelLoad >= _beforeLibraryFadeInDelay)
		{
			_beforeLibraryFadeInDelay = Mathf.Infinity;

			ScreenFader screenFader = GlobalManager.instance.screenFader;
			screenFader.speed = 1f;
			screenFader.StartFadeIn ();
		}
		// �������� �������.
		if (Time.timeSinceLevelLoad >= _beforeLibrarySpeechDelay)
		{
			_beforeLibrarySpeechDelay = Mathf.Infinity;

			// ��������� ���� ����.
			beforeLibraryAidaSpeechGO.GetComponent<AudioSource> ().Play ();
			// ��������� ���� ������.
			beforeLibraryAlexSpeechGO.GetComponent<AudioSource> ().Play ();
		}

		//if (!beforeLibraryAidaSpeechGO.GetComponent<AudioSource> ().isPlaying)
		//{
		//	// ����������� ����� �������.
		//	GlobalManager.instance.storyState = StoryState.LibraryHike;
		//	// ��������� �����.
		//	_door.preventInteractions = false;
		//}

		// ���� ������ ����������.
		if (Time.timeSinceLevelLoad >= _beforeLibraryStorySwitchTime)
		{
			// ����������� ����� �������.
			GlobalManager.instance.storyState = StoryState.LibraryHike;
			// ��������� �����.
			_door.preventInteractions = false;
		}
	}

	private void UpdateOnLibraryHike ()
	{

	}

	#endregion


	// ������� �� ���� ���� � ��� ������ (����� �� �����������).
	private IEnumerator LoadGoForGearScene ()
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync (goForGearSceneName);
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
	}


	#region Utils

	// �������� ������ Start Point.
	private void CacheStartPointGO ()
	{
		string usedStartPointName = GlobalManager.instance.playerStartPointName;
		if (usedStartPointName != null && usedStartPointName != "")
		{
			_startPointGO = GameObject.Find (usedStartPointName);
			if (_startPointGO == null)
				throw new System.Exception ("Start Point with name '" + usedStartPointName + "' not found.");
		}
	}

	// ��������� ��������� � ��������� ��������� �����, ���� ��� ������.
	private void PlaceCharacterAtStartPoint ()
	{
		if (_startPointGO != null)
		{
			CharController charController = GlobalManager.instance.alexCamera.transform.root.gameObject.GetComponent<CharController> ();
			charController.transform.position = _startPointGO.transform.position;
			Vector3 targetAngles = _startPointGO.transform.rotation.eulerAngles;
			charController.yawDegrees = targetAngles.y;
			charController.pitchDegrees = targetAngles.x;
		}
	}

	private void StartDefaultFadeIn ()
	{
		// ������ �������� ������ � ��������� Fade In.
		ScreenFader screenFader = GlobalManager.instance.screenFader;
		screenFader.opacity = 1f;
		screenFader.speed = 1f;
		screenFader.StartFadeIn ();
	}

	private void StopAmbience ()
	{
		AudioLayer ambientLayer = GlobalManager.instance.audioManager.GetLayerByType (AudioLayerType.Ambient);
		ambientLayer.introSpeed = .25f;
		ambientLayer.outroSpeed = .25f;
		ambientLayer.FadeOut ();
	}

	private Readable TryGetReadableComp (GameObject gameObject)
	{
		Readable readable = gameObject.GetComponent<Readable> ();
		if (readable == null)
			throw new System.Exception ("Can't find Readable component.");
		return readable;
	}

	#endregion

}
