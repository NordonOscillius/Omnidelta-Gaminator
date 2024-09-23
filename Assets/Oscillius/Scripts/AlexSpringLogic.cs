using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AlexSpringLogic : MonoBehaviour
{
	[Tooltip ("Игровой объект, хранящий компонент аудиосорса для рассказа Алекса о первых днях пребывания в Дельте.")]
	public GameObject firstDaysSpeechGO;
	[Tooltip ("Название сцены, которую нужно грузить для перехода к первому разговору с Аидой.")]
	public string firstEncounterSceneName;
	[Tooltip ("Название игрового объекта, положение которого должен принять персонаж при первом разговоре с Аидой.")]
	public string firstEncounterStartPointName;
	[Tooltip ("Название сцены, в которой Аида в своем доме ждет Алекса с солгардом.")]
	public string waitForSolguardSceneName;
	[Tooltip ("Название игрового объекта, положение которого должен принять персонаж Аиды в сцене 'Wait For Solguard'.")]
	public string waitForSolguardStartPointName;
	[Tooltip ("Игровой объект: дверь, ведущая в дом Алекса.")]
	public GameObject alexHouseDoor;
	[Tooltip ("Объект, хранящий террейн этого уровня.")]
	public GameObject terrainGO;
	[Tooltip ("Объект, хранящий скрипт PauseMenu.")]
	public GameObject pauseMenuGO;

	// Кэшированная ссылка на объект, представляющий Start Point.
	private GameObject _startPointGO;

	//private AlexSpringState _state = AlexSpringState.FirstDays;
	private FaderState _faderState = FaderState.BeforeFadeIn;

	// Задержка рассказа Алекса о первых днях в Дельте, в секундах.
	private float _firstDaysSpeechDelay = 1f;
	// Время от начала рассказа Алекса до резкого Fade To Black. Считается по текущему времени воспроизводимого клипа.
	private float _firstDaysTimeToFTB = 72f;
	//private float _firstDaysTimeToFTB = 5f;

	// Громкость стандартного эмбиента.
	private float _defaultAmbienceLoudness = .6f;

	// Задержка между загрузкой сцены и началом реплики о пропавшем Мартине.
	private float _martinIsMissingSpeechDelay = 4f;
	// Громкость реплики о пропавшем Мартине.
	private float _martinIsMissingSpeechLoudness = .8f;
	// Осц-клип с рассказом о Дельте. Запускается глобально и звучит при переходе между сценами.
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
		// Кэшируем Start Point, если она есть.
		CacheStartPointGO ();
		// Кэшируем террейн.
		if (terrainGO != null)
			_terrain = terrainGO.GetComponent<Terrain> ();
	}

	private void AwakeOnSolguardHike ()
	{
		// Кэшируем Start Point, если она есть.
		CacheStartPointGO ();
		// Кэшируем террейн.
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

			// Передаем в меню паузы ссылку на террейн, чтобы он обновлялся при изменении настроек.
			pauseMenuGO.GetComponent<PauseMenu> ().terrain = _terrain;
		}
	}

	private void StartOnFirstDays ()
	{
		// Помещаем персонажа в Start Point, если она есть.
		PlaceCharacterAtStartPoint ();
		// Появление картинки.
		StartDefaultFadeIn ();
		// Нагоняем стандартный эмбиент.
		IntroduceDefaultAmbience ();

		// Запускаем рассказ Алекса о первых днях пребывания в Дельте.
		AudioSource firstDaysAudio = firstDaysSpeechGO.GetComponent<AudioSource> ();
		firstDaysAudio.PlayDelayed (_firstDaysSpeechDelay);
		// Закрываем дверь в дом Алекса.
		alexHouseDoor.GetComponent<AlexDoorScript> ().preventInteractions = true;
	}

	private void StartOnSolguardHike ()
	{
		// Помещаем персонажа в Start Point, если она есть.
		PlaceCharacterAtStartPoint ();
		// Появление картинки.
		StartDefaultFadeIn ();
		// Нагоняем стандартный эмбиент.
		IntroduceDefaultAmbience ();
	}

	private void StartOnGoForGear ()
	{
		// Помещаем персонажа в Start Point, если она есть.
		PlaceCharacterAtStartPoint ();
		// Появление картинки.
		StartDefaultFadeIn ();
		// Нагоняем стандартный эмбиент.
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

		// Если рассказ уже закончился, меняем storyState загружаем сцену с первой встречей с Аидой.
		if (!firstDaysAudioSource.isPlaying)
		{
			if (globalManager.storyState != StoryState.FirstEncounter)
			{
				globalManager.storyState = StoryState.FirstEncounter;

				globalManager.playerStartPointName = firstEncounterStartPointName;
				StartCoroutine (LoadFirstEncounterScene ());
			}
		}

		// В момент стука резко затемняем картинку и убираем эмбиент.
		if (firstDaysAudioSource.time >= _firstDaysTimeToFTB && _faderState == FaderState.BeforeFadeIn)
		{
			_faderState = FaderState.FadingIn;

			globalManager.screenFader.speed = 100f;
			globalManager.screenFader.StartFadeToBlack ();

			// Разгоняем эмбиент.
			AudioLayer ambientLayer = globalManager.audioManager.GetLayerByType (AudioLayerType.Ambient);
			ambientLayer.outroSpeed = .67f;
			ambientLayer.FadeOut ();
		}
	}

	private void UpdateOnSolduardHike ()
	{
		// Если речь о пропавшем Мартине еще не звучала и если она еще не воспроизводится, запускаем ее.
		if (GlobalManager.instance.martinIsMissingSpeechIsNeeded && Time.timeSinceLevelLoad >= _martinIsMissingSpeechDelay)
		{
			AudioLayer martinSpeechLayer = GlobalManager.instance.audioManager.GetLayerByType (AudioLayerType.Speech);
			martinSpeechLayer.introSpeed = 1000f;
			martinSpeechLayer.IntroduceClip (OscAudioResourceEnum.Speech_Martin_Is_Missing, false, _martinIsMissingSpeechLoudness);

			GlobalManager.instance.martinIsMissingSpeechIsNeeded = false;
		}

		// Если собрано двадцать соцветий солгарда, начинаем рассказ о солнце Дельты.
		if (GlobalManager.instance.collectedSolguardNames.Count >= 20)
		{
			// Переключаем стейт в "Рассказ о солнце Дельты".
			GlobalManager.instance.storyState = StoryState.SunSpeech;

			// Запускаем рассказ о солнце.
			AudioLayer speechLayer = GlobalManager.instance.audioManager.GetLayerByType (AudioLayerType.Speech);
			speechLayer.introSpeed = 1000f;
			_sunSpeechClip = speechLayer.IntroduceClip (OscAudioResourceEnum.Speech_Sun, false, .8f);
		}
	}

	private void UpdateOnSunSpeech ()
	{
		// Если дошли до транзишена между Алексом и Аидой, переключаем стейт истории, затемняем картинку и грузим следующую сцену.
		if (_sunSpeechClip.audioSource.time >= 20.6f)
		{
			// Переключаем стейт в "Ожидание солгарда".
			GlobalManager.instance.storyState = StoryState.WaitForSolguard;

			// Начальная точка Аиды.
			GlobalManager.instance.playerStartPointName = waitForSolguardStartPointName;

			// Затемняем картинку.
			GlobalManager.instance.screenFader.speed = .67f;
			GlobalManager.instance.screenFader.StartFadeToBlack (OnFadedToBlackOnSunSpeech);
		}
	}

	// При переходе от "Рассказа о солнце" к "Ожиданию солгарда": картинка полностью ушла в черный.
	private void OnFadedToBlackOnSunSpeech ()
	{
		StartCoroutine (LoadWaitForSolguardScene ());
	}

	#endregion


	// Загрузка сцены первой встречи с Аидой.
	private IEnumerator LoadFirstEncounterScene ()
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync (firstEncounterSceneName);
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
	}

	// Загрузка сцены, где Аида ждет Алекса с солгардом.
	private IEnumerator LoadWaitForSolguardScene ()
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync (waitForSolguardSceneName);
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
	}


	#region Utils

	// Кэширует объект Start Point.
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

	// Размещает персонажа в положении начальной точки, если она задана.
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
		// Делаем картинку черной и запускаем Fade In.
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
