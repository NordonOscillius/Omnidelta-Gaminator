using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AidaHouseLogic : MonoBehaviour
{
	[Tooltip ("Дверь наружу.")]
	public GameObject doorGO;
	[Tooltip ("Объект с аудиосорсом для речи i-Start (Алекс за стеной).")]
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
	[Tooltip ("Бутылка со спиртом, которую нужно спрятать в эпизоде Conversation Before Library. Аида приготовила из него раствор желтой пыли.")]
	public GameObject alcoholBottleToHide;

	[Tooltip ("Название сцены, в которую нужно перейти в эпизоде 'Алекс отправляется за шестеренкой'.")]
	public string goForGearSceneName;

	private GameObject _startPointGO;
	private AidaInnerDoor _door;
	private AudioSource _speechIStart;

	private Readable _invisibleInkBookReadable;
	private Readable _libraryMapInvisibleReadable;
	private Readable _libraryBookReadable;

	// Задержка времени перед Fade In в сцене "Wait For Solguard".
	private float _waitForSolguardFadeInDelay = 2f;
	// Если false, фейд-ин уже не нужно запускать.
	private bool _waitForSolguardFadeInIsNeeded = true;
	// Громкость стема диалога i1...i3 про собранный солгард и шестеренку.
	private float _speechGearLoudness = .8f;
	// Время между началом диалога i1...i3 (собранный солгард) и моментом перехода в дом Алекса.
	private float _solguardIsBroughtSwitchPeriod = 14.1f;
	// Глобальное время (Time.time), когда был начат диалог про собранный солгард. Устанавливается в методе апдейта.
	private float _solguardIsBroughtStartTime = 0f;
	// Время от момента затемнения картинки (при переходе из дома Аиды в дом Алекса) до Fade In в доме Алекса.
	//private float _goForGearFadeInDelay = 1.5f;
	// Глобальное время Time.time, когда был совершен переход в стейт Go For Gear. Устанавливается в методе апдейта.
	//private float _goForGearStartTime = 0f;
	// Задержка Fade In после загрузки сцены в доме Аиды (когда она собирается в библиотеку).
	private float _beforeLibraryFadeInDelay = 1f;
	// Задержка запуска диалога после загрузки сцены в эпизоде Conversation Before Library.
	private float _beforeLibrarySpeechDelay = .02f;
	// Время от начала загрузки сцены Conversation Before Library до того момента, как Аиде будет разрешено пользоваться дверью (до конца диалога).
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
		// Помещаем персонажа в Start Point, если она есть.
		PlaceCharacterAtStartPoint ();
		// Разгоняем эмбиент.
		StopAmbience ();
		// Запрещаем пользоваться дверью, пока Аида не договорит.
		_door.preventInteractions = true;
	}

	private void StartOnConversationBeforeLibrary ()
	{
		// Помещаем персонажа в Start Point, если она есть.
		PlaceCharacterAtStartPoint ();
		// Запрещаем пользоваться дверью, пока Аида не договорит.
		_door.preventInteractions = true;

		// Прячем невидимую карту.
		libraryMapInvisibleGO.SetActive (false);
		// Показываем проявленную карту.
		libraryMapVisibleGO.SetActive (true);
		// Прячем пустую ванночку без алкоголя.
		alcoholTrayEmptyGO.SetActive (false);
		// Показываем ванночку со спиртовым раствором желтой пыли.
		alcoholTrayFullGO.SetActive (true);
		// Прячем пустую ступку.
		mortarEmptyGO.SetActive (false);
		// Показываем ступку с желтой пылью.
		mortarFullGO.SetActive (true);
		// Прячем использованную бутылку со спиртом.
		alcoholBottleToHide.SetActive (false);
	}

	private void StartOnLibraryHikeCommon ()
	{
		// Помещаем персонажа в Start Point, если она есть.
		PlaceCharacterAtStartPoint ();
		// Стандартное появление картинки.
		StartDefaultFadeIn ();

		// Прячем невидимую карту.
		libraryMapInvisibleGO.SetActive (false);
		// Показываем проявленную карту.
		libraryMapVisibleGO.SetActive (true);
		// Прячем пустую ванночку без алкоголя.
		alcoholTrayEmptyGO.SetActive (false);
		// Показываем ванночку со спиртовым раствором желтой пыли.
		alcoholTrayFullGO.SetActive (true);
		// Прячем пустую ступку.
		mortarEmptyGO.SetActive (false);
		// Показываем ступку с желтой пылью.
		mortarFullGO.SetActive (true);
		// Прячем использованную бутылку со спиртом.
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

		// Если фраза "Я должна подождать его здесь" еще не произносилась.
		if (!_door.iHaveToWaitIsPronounced)
		{
			// Если Аида договорила, позволяем пользоваться дверью.
			OscClip sunClip = globalManager.audioManager.GetLayerByType (AudioLayerType.Speech).GetClipWithResourceType (OscAudioResourceEnum.Speech_Sun);
			if (sunClip == null)
			{
				if (_door.preventInteractions)
					_door.preventInteractions = false;
			} 
		}

		// Если прочитаны все три заметки и если ни одну из них в данный момент игрок не читает, переключаем стейт и начинаем разговор (Алекс вернулся).
		if (globalManager.invisibleInkBookIsRead &&
			globalManager.libraryMapInvisibleIsRead &&
			globalManager.libraryBookIsRead &&
			_invisibleInkBookReadable.state == ReadableState.Normal &&
			_libraryMapInvisibleReadable.state == ReadableState.Normal &&
			_libraryBookReadable.state == ReadableState.Normal
		)
		{
			// Переключаем стейт.
			globalManager.storyState = StoryState.SolguardIsBrought;
			// Запоминаем время, в котором переключились в новый стейт (начали диалог).
			_solguardIsBroughtStartTime = Time.time;

			// Запускаем речь Алекса (3D).
			_speechIStart.Play ();
			// Запускаем стем Аиды (глобально).
			AudioLayer speechLayer = globalManager.audioManager.GetLayerByType (AudioLayerType.Speech);
			speechLayer.introSpeed = 1000f;
			speechLayer.IntroduceClip (OscAudioResourceEnum.Speech_Gear, false, _speechGearLoudness);
		}
	}

	private void UpdateOnSolguardIsBrought ()
	{
		// Если время от начала диалога "Я принес солгард" достигло времени переключения в дом Алекса.
		if (Time.time - _solguardIsBroughtStartTime >= _solguardIsBroughtSwitchPeriod)
		{
			// Переключаем стейт.
			GlobalManager.instance.storyState = StoryState.GoForGear;
			// Запоминаем момент перехода.
			//_goForGearStartTime = Time.time;

			// Резко затемняем картинку.
			ScreenFader screenFader = GlobalManager.instance.screenFader;
			screenFader.speed = 100f;
			screenFader.StartFadeToBlack (OnFadedToBlackOnSolguardIsBrought);
		}
	}

	// Переход из дома Аиды в дом Алекса (поход за шестеренкой).
	private void OnFadedToBlackOnSolguardIsBrought ()
	{
		// Алекс будет стоять рядом со стеной.
		GlobalManager.instance.playerStartPointName = "Start Point (3)";

		StartCoroutine (LoadGoForGearScene ());
	}

	private void UpdateOnConversationBeforeLibrary ()
	{
		// Задержка Fade In.
		if (Time.timeSinceLevelLoad >= _beforeLibraryFadeInDelay)
		{
			_beforeLibraryFadeInDelay = Mathf.Infinity;

			ScreenFader screenFader = GlobalManager.instance.screenFader;
			screenFader.speed = 1f;
			screenFader.StartFadeIn ();
		}
		// Задержка диалога.
		if (Time.timeSinceLevelLoad >= _beforeLibrarySpeechDelay)
		{
			_beforeLibrarySpeechDelay = Mathf.Infinity;

			// Запускаем стем Аиды.
			beforeLibraryAidaSpeechGO.GetComponent<AudioSource> ().Play ();
			// Запускаем стем Алекса.
			beforeLibraryAlexSpeechGO.GetComponent<AudioSource> ().Play ();
		}

		//if (!beforeLibraryAidaSpeechGO.GetComponent<AudioSource> ().isPlaying)
		//{
		//	// Переключаем стейт истории.
		//	GlobalManager.instance.storyState = StoryState.LibraryHike;
		//	// Открываем дверь.
		//	_door.preventInteractions = false;
		//}

		// Если диалог завершился.
		if (Time.timeSinceLevelLoad >= _beforeLibraryStorySwitchTime)
		{
			// Переключаем стейт истории.
			GlobalManager.instance.storyState = StoryState.LibraryHike;
			// Открываем дверь.
			_door.preventInteractions = false;
		}
	}

	private void UpdateOnLibraryHike ()
	{

	}

	#endregion


	// Переход из дома Аиды в дом Алекса (поход за шестеренкой).
	private IEnumerator LoadGoForGearScene ()
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync (goForGearSceneName);
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
