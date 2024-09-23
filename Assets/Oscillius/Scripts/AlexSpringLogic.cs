using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AlexSpringLogic : MonoBehaviour
{
	[Tooltip ("������� ������, �������� ��������� ���������� ��� �������� ������ � ������ ���� ���������� � ������.")]
	public GameObject firstDaysSpeechGO;
	[Tooltip ("�������� �����, ������� ����� ������� ��� �������� � ������� ��������� � �����.")]
	public string firstEncounterSceneName;
	[Tooltip ("�������� �������� �������, ��������� �������� ������ ������� �������� ��� ������ ��������� � �����.")]
	public string firstEncounterStartPointName;
	[Tooltip ("�������� �����, � ������� ���� � ����� ���� ���� ������ � ���������.")]
	public string waitForSolguardSceneName;
	[Tooltip ("�������� �������� �������, ��������� �������� ������ ������� �������� ���� � ����� 'Wait For Solguard'.")]
	public string waitForSolguardStartPointName;
	[Tooltip ("������� ������: �����, ������� � ��� ������.")]
	public GameObject alexHouseDoor;
	[Tooltip ("������, �������� ������� ����� ������.")]
	public GameObject terrainGO;
	[Tooltip ("������, �������� ������ PauseMenu.")]
	public GameObject pauseMenuGO;

	// ������������ ������ �� ������, �������������� Start Point.
	private GameObject _startPointGO;

	//private AlexSpringState _state = AlexSpringState.FirstDays;
	private FaderState _faderState = FaderState.BeforeFadeIn;

	// �������� �������� ������ � ������ ���� � ������, � ��������.
	private float _firstDaysSpeechDelay = 1f;
	// ����� �� ������ �������� ������ �� ������� Fade To Black. ��������� �� �������� ������� ���������������� �����.
	private float _firstDaysTimeToFTB = 72f;
	//private float _firstDaysTimeToFTB = 5f;

	// ��������� ������������ ��������.
	private float _defaultAmbienceLoudness = .6f;

	// �������� ����� ��������� ����� � ������� ������� � ��������� �������.
	private float _martinIsMissingSpeechDelay = 4f;
	// ��������� ������� � ��������� �������.
	private float _martinIsMissingSpeechLoudness = .8f;
	// ���-���� � ��������� � ������. ����������� ��������� � ������ ��� �������� ����� �������.
	private OscClip _sunSpeechClip;

	private Terrain _terrain;


	//public enum AlexSpringState
	//{
	//	FirstDays
	//}

	private enum FaderState
	{
		BeforeFadeIn,
		FadingIn,
		AfterFadeIn
	}


	#region Awake

	private void Awake ()
	{
		StoryState storyState = GlobalManager.instance.storyState;

		if (storyState == StoryState.FirstDays)
			AwakeOnFirstDays ();
		else if (storyState == StoryState.SolguardHike)
			AwakeOnSolguardHike ();
	}

	private void AwakeOnFirstDays ()
	{
		// �������� Start Point, ���� ��� ����.
		CacheStartPointGO ();
		// �������� �������.
		if (terrainGO != null)
			_terrain = terrainGO.GetComponent<Terrain> ();
	}

	private void AwakeOnSolguardHike ()
	{
		// �������� Start Point, ���� ��� ����.
		CacheStartPointGO ();
		// �������� �������.
		if (terrainGO != null)
			_terrain = terrainGO.GetComponent<Terrain> ();
	}

	#endregion


	#region Start

	private void Start ()
	{
		StoryState storyState = GlobalManager.instance.storyState;

		if (storyState == StoryState.FirstDays)
			StartOnFirstDays ();
		else if (storyState == StoryState.SolguardHike)
			StartOnSolguardHike ();
		else if (storyState == StoryState.GoForGear)
			StartOnGoForGear ();

		// Terrain settings.
		if (_terrain != null)
		{
			_terrain.detailObjectDistance = GlobalManager.instance.grassDistance;
			_terrain.detailObjectDensity = GlobalManager.instance.grassDensity;

			// �������� � ���� ����� ������ �� �������, ����� �� ���������� ��� ��������� ��������.
			pauseMenuGO.GetComponent<PauseMenu> ().terrain = _terrain;
		}
	}

	private void StartOnFirstDays ()
	{
		// �������� ��������� � Start Point, ���� ��� ����.
		PlaceCharacterAtStartPoint ();
		// ��������� ��������.
		StartDefaultFadeIn ();
		// �������� ����������� �������.
		IntroduceDefaultAmbience ();

		// ��������� ������� ������ � ������ ���� ���������� � ������.
		AudioSource firstDaysAudio = firstDaysSpeechGO.GetComponent<AudioSource> ();
		firstDaysAudio.PlayDelayed (_firstDaysSpeechDelay);
		// ��������� ����� � ��� ������.
		alexHouseDoor.GetComponent<AlexDoorScript> ().preventInteractions = true;
	}

	private void StartOnSolguardHike ()
	{
		// �������� ��������� � Start Point, ���� ��� ����.
		PlaceCharacterAtStartPoint ();
		// ��������� ��������.
		StartDefaultFadeIn ();
		// �������� ����������� �������.
		IntroduceDefaultAmbience ();
	}

	private void StartOnGoForGear ()
	{
		// �������� ��������� � Start Point, ���� ��� ����.
		PlaceCharacterAtStartPoint ();
		// ��������� ��������.
		StartDefaultFadeIn ();
		// �������� ����������� �������.
		IntroduceDefaultAmbience ();
	}

	#endregion


	#region Update

	private void Update ()
	{
		StoryState storyState = GlobalManager.instance.storyState;

		if (storyState == StoryState.FirstDays)
			UpdateOnFirstDays ();
		else if (storyState == StoryState.SolguardHike)
			UpdateOnSolduardHike ();
		else if (storyState == StoryState.SunSpeech)
			UpdateOnSunSpeech ();
	}

	private void UpdateOnFirstDays ()
	{
		GlobalManager globalManager = GlobalManager.instance;
		AudioSource firstDaysAudioSource = firstDaysSpeechGO.GetComponent<AudioSource> ();

		// ���� ������� ��� ����������, ������ storyState ��������� ����� � ������ �������� � �����.
		if (!firstDaysAudioSource.isPlaying)
		{
			if (globalManager.storyState != StoryState.FirstEncounter)
			{
				globalManager.storyState = StoryState.FirstEncounter;

				globalManager.playerStartPointName = firstEncounterStartPointName;
				StartCoroutine (LoadFirstEncounterScene ());
			}
		}

		// � ������ ����� ����� ��������� �������� � ������� �������.
		if (firstDaysAudioSource.time >= _firstDaysTimeToFTB && _faderState == FaderState.BeforeFadeIn)
		{
			_faderState = FaderState.FadingIn;

			globalManager.screenFader.speed = 100f;
			globalManager.screenFader.StartFadeToBlack ();

			// ��������� �������.
			AudioLayer ambientLayer = globalManager.audioManager.GetLayerByType (AudioLayerType.Ambient);
			ambientLayer.outroSpeed = .67f;
			ambientLayer.FadeOut ();
		}
	}

	private void UpdateOnSolduardHike ()
	{
		// ���� ���� � ��������� ������� ��� �� ������� � ���� ��� ��� �� ���������������, ��������� ��.
		if (GlobalManager.instance.martinIsMissingSpeechIsNeeded && Time.timeSinceLevelLoad >= _martinIsMissingSpeechDelay)
		{
			AudioLayer martinSpeechLayer = GlobalManager.instance.audioManager.GetLayerByType (AudioLayerType.Speech);
			martinSpeechLayer.introSpeed = 1000f;
			martinSpeechLayer.IntroduceClip (OscAudioResourceEnum.Speech_Martin_Is_Missing, false, _martinIsMissingSpeechLoudness);

			GlobalManager.instance.martinIsMissingSpeechIsNeeded = false;
		}

		// ���� ������� �������� �������� ��������, �������� ������� � ������ ������.
		if (GlobalManager.instance.collectedSolguardNames.Count >= 20)
		{
			// ����������� ����� � "������� � ������ ������".
			GlobalManager.instance.storyState = StoryState.SunSpeech;

			// ��������� ������� � ������.
			AudioLayer speechLayer = GlobalManager.instance.audioManager.GetLayerByType (AudioLayerType.Speech);
			speechLayer.introSpeed = 1000f;
			_sunSpeechClip = speechLayer.IntroduceClip (OscAudioResourceEnum.Speech_Sun, false, .8f);
		}
	}

	private void UpdateOnSunSpeech ()
	{
		// ���� ����� �� ���������� ����� ������� � �����, ����������� ����� �������, ��������� �������� � ������ ��������� �����.
		if (_sunSpeechClip.audioSource.time >= 20.6f)
		{
			// ����������� ����� � "�������� ��������".
			GlobalManager.instance.storyState = StoryState.WaitForSolguard;

			// ��������� ����� ����.
			GlobalManager.instance.playerStartPointName = waitForSolguardStartPointName;

			// ��������� ��������.
			GlobalManager.instance.screenFader.speed = .67f;
			GlobalManager.instance.screenFader.StartFadeToBlack (OnFadedToBlackOnSunSpeech);
		}
	}

	// ��� �������� �� "�������� � ������" � "�������� ��������": �������� ��������� ���� � ������.
	private void OnFadedToBlackOnSunSpeech ()
	{
		StartCoroutine (LoadWaitForSolguardScene ());
	}

	#endregion


	// �������� ����� ������ ������� � �����.
	private IEnumerator LoadFirstEncounterScene ()
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync (firstEncounterSceneName);
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
	}

	// �������� �����, ��� ���� ���� ������ � ���������.
	private IEnumerator LoadWaitForSolguardScene ()
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync (waitForSolguardSceneName);
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
			//CharController charController = GlobalManager.instance.alexCamera.transform.root.gameObject.GetComponent<CharController> ();
			//charController.transform.position = _startPointGO.transform.position;
			//Vector3 targetAngles = _startPointGO.transform.rotation.eulerAngles;
			//charController.yawDegrees = targetAngles.y;
			//charController.pitchDegrees = targetAngles.x;


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

	private void IntroduceDefaultAmbience ()
	{
		AudioLayer ambientLayer = GlobalManager.instance.audioManager.GetLayerByType (AudioLayerType.Ambient);
		ambientLayer.introSpeed = .25f;
		ambientLayer.outroSpeed = .25f;
		ambientLayer.IntroduceClip (OscAudioResourceEnum.Ambience_Forest, _defaultAmbienceLoudness);
	}

	#endregion

}
