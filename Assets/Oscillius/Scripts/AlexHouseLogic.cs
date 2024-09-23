using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AlexHouseLogic : MonoBehaviour
{
	[Tooltip ("GameObject с компонентом AudioSource для речи Аиды в эпизоде First Encounter.")]
	public GameObject firstEncounterAidaSpeechGO;
	[Tooltip ("GameObject с компонентом AudioSource для речи Алекса в эпизоде First Encounter.")]
	public GameObject firstEncounterAlexSpeechGO;
	[Tooltip ("Дверь наружу.")]
	public GameObject door;
	[Tooltip ("GameObject с компонентом AudioSource для речи Алекса в эпизоде Conversation Before Library.")]
	public GameObject beforeLibraryAlexSpeechGO;
	[Tooltip ("GameObject с компонентом AudioSource для речи Аиды в эпизоде Conversation Before Library.")]
	public GameObject beforeLibraryAidaSpeechGO;
	[Tooltip ("GameObject с компонентом AudioSource для речи Алекса в эпизоде Aquamarine Talk.")]
	public GameObject aquamarineTalkAlexGO;
	[Tooltip ("GameObject с компонентом AudioSource для речи Аиды в эпизоде Aquamarine Talk.")]
	public GameObject aquamarineTalkAidaGO;
	[Tooltip ("Название сцены в доме Аиды.")]
	public string aidaHouseSceneName;
	[Tooltip ("Название сцены 'Продолжение следует'.")]
	public string toBeContinuedSceneName;

	private GameObject _startPointGO;
	private CharController _charController;

	// Закончили ли персонажи говорить в сцене First Encounter.
	private bool _firstEncounterSpeechIsOver = false;

	// В эпизоде "За шестеренкой": время между загрузкой сцены и Fade In.
	private float _goForGearFadeInDelay = 2.5f;
	// Значение координаты X персонажа, при котором начинается диалог с Аидой в эпизоде "Солгард отправлен". Чтобы не возиться с триггерами и не плодить сущностей.
	private float _solguardIsSentDialogueStartX = -1.5f;
	// В сцене разговора о ячейке: время между окончанием части диалога в доме Алекса и началом загрузки новой сцены. Ввожу из-за заикающегося при загрузке звука.
	private float _beforeLibraryAidaHouseLoadDelay = .02f;
	// Время, прошедшее с момента завершения куска разговора в доме Алекса.
	private float _beforeLibraryTimeToAidaHouseLoad = 0f;

	// Кэшированный аудиосорс стема Аиды.
	private AudioSource _aquamarineTalkAidaSource;


	#region Awake

	private void Awake ()
	{
		StoryState storyState = GlobalManager.instance.storyState;

		if (storyState == StoryState.Arrival)
			AwakeOnArrival ();
		else if (storyState == StoryState.ArrivalAfterSpeech)
			AwakeDefault ();
		else if (storyState == StoryState.FirstDays)
			AwakeDefault ();
		else if (storyState == StoryState.FirstEncounter)
			AwakeDefault ();
		else if (storyState == StoryState.SolguardHike)
			AwakeDefault ();
		else if (storyState == StoryState.GoForGear)
			AwakeOnGoForGear ();
		else if (storyState == StoryState.SolguardIsSent)
			AwakeOnSolguardIsSent ();
		else if (storyState == StoryState.AquamarineTalk)
			AwakeOnAquamarineTalk ();
	}

	private void AwakeDefault ()
	{
		// Кэшируем объект Start Point.
		CacheStartPointGO ();
	}

	private void AwakeOnArrival ()
	{
		// Если мы зашли в дом Алекса только после прибытия, переключаем стейт в следующий.
		GlobalManager.instance.storyState = StoryState.ArrivalAfterSpeech;
		// Кэшируем объект Start Point.
		CacheStartPointGO ();
	}

	private void AwakeOnGoForGear ()
	{
		// Кэшируем объект Start Point.
		CacheStartPointGO ();

		// Это костыль. Косвенный способ определить, грузится ли сцена во время разговора с Аидой или после того, как Алекс вышел на улицу за шестеренкой, но вернулся.
		// Если Алекс не у стены, то сбрасываем Fade In Delay в отрицательное значение. В Start() затем сделаем проверку (< 0), и если все норм, запустим стандартный плавный Fade In.
		if (_startPointGO.name != "Start Point (3)")
		{
			_goForGearFadeInDelay = -1;
		}
	}

	private void AwakeOnSolguardIsSent ()
	{
		CacheStartPointGO ();
	}

	private void AwakeOnAquamarineTalk ()
	{
		CacheStartPointGO ();
	}

	#endregion


	#region Start

	private void Start ()
	{
		StoryState storyState = GlobalManager.instance.storyState;

		if (storyState == StoryState.Arrival)
			StartOnArrival ();
		else if (storyState == StoryState.ArrivalAfterSpeech)
			StartOnArrivalAfterSpeech ();
		else if (storyState == StoryState.FirstDays)
			StartOnFirstDays ();
		else if (storyState == StoryState.FirstEncounter)
			StartOnFirstEncounter ();
		else if (storyState == StoryState.SolguardHike)
			StartOnSolguardHike ();
		else if (storyState == StoryState.GoForGear)
			StartOnGoForGear ();
		else if (storyState == StoryState.SolguardIsSent)
			StartOnSolguardIsSent ();
		else if (storyState == StoryState.AquamarineTalk)
			StartOnAquamarineTalk ();
	}

	private void StartOnArrival ()
	{
		// Делаем картинку черной и запускаем Fade In.
		StartDefaultFadeIn ();
		// Ставим персонажа в нужное положение.
		PlaceCharacterAtStartPoint ();
	}

	private void StartOnArrivalAfterSpeech ()
	{
		// Делаем картинку черной и запускаем Fade In.
		StartDefaultFadeIn ();
		// Ставим персонажа в нужное положение.
		PlaceCharacterAtStartPoint ();
	}

	private void StartOnFirstDays ()
	{
		// Делаем картинку черной и запускаем Fade In.
		StartDefaultFadeIn ();
		// Ставим персонажа в нужное положение.
		PlaceCharacterAtStartPoint ();
	}

	private void StartOnFirstEncounter ()
	{
		// Делаем картинку черной и запускаем Fade In.
		StartDefaultFadeIn ();
		// Ставим персонажа в нужное положение.
		PlaceCharacterAtStartPoint ();

		// Быстро приглушаем эмбиент, если он был.
		//AudioLayer ambientLayer = GlobalManager.instance.audioManager.GetLayerByType (AudioLayerType.Ambient);
		//ambientLayer.introSpeed = .25f;
		//ambientLayer.outroSpeed = 10f;
		//ambientLayer.FadeOut ();
		// Разговор начался.
		_firstEncounterSpeechIsOver = false;
		// Запускаем стем Аиды (первый разговор).
		firstEncounterAidaSpeechGO.GetComponent<AudioSource> ().Play ();
		// Запускаем стем Алекса (первый разговор).
		firstEncounterAlexSpeechGO.GetComponent<AudioSource> ().Play ();
		// Закрываем дверь, чтобы Алекс не выходил.
		AlexInnerDoor doorScript = door.GetComponent<AlexInnerDoor> ();
		if (doorScript == null)
			throw new System.Exception ("The door doesn't have an AlexInnerDoor component.");
		doorScript.preventInteractions = true;
	}

	private void StartOnSolguardHike ()
	{
		// Делаем картинку черной и запускаем Fade In.
		StartDefaultFadeIn ();
		// Ставим персонажа в нужное положение.
		PlaceCharacterAtStartPoint ();
	}

	private void StartOnGoForGear ()
	{
		// Ставим персонажа в нужное положение.
		PlaceCharacterAtStartPoint ();
		// Кэшируем контроллер персонажа.
		_charController = GlobalManager.instance.alexCamera.transform.root.gameObject.GetComponent<CharController> ();

		// Если Алекс вышел за шестеренкой, но вернулся, не отправив солгард.
		if (_goForGearFadeInDelay < 0)
		{
			StartDefaultFadeIn ();
		}
		// Если это тот случай, когда Алекс продолжает разговор с Аидой.
		else
		{
			// Парализуем Алекса на время (речь Аиды из-за стены играет глобально, а не в 3D).
			_charController.enableLook = false;
			_charController.enableMove = false; 
		}
	}

	private void StartOnSolguardIsSent ()
	{
		// Делаем картинку черной и запускаем Fade In.
		StartDefaultFadeIn ();
		// Ставим персонажа в нужное положение.
		PlaceCharacterAtStartPoint ();
		// Кэшируем контроллер персонажа.
		_charController = GlobalManager.instance.alexCamera.transform.root.gameObject.GetComponent<CharController> ();
	}

	private void StartOnAquamarineTalk ()
	{
		// Делаем картинку черной и запускаем Fade In.
		StartDefaultFadeIn ();
		// Ставим персонажа в нужное положение.
		PlaceCharacterAtStartPoint ();
		// Запускаем реплики Алекса.
		aquamarineTalkAlexGO.GetComponent<AudioSource> ().Play ();
		// Кэшируем аудиосорс стема Аиды.
		_aquamarineTalkAidaSource = aquamarineTalkAidaGO.GetComponent<AudioSource> ();
		// Запускаем реплики Аиды.
		_aquamarineTalkAidaSource.Play ();
		// Закрываем дверь, чтобы Алекс не выходил.
		AlexInnerDoor doorScript = door.GetComponent<AlexInnerDoor> ();
		if (doorScript == null)
			throw new System.Exception ("The door doesn't have an AlexInnerDoor component.");
		doorScript.preventInteractions = true;
	}

	#endregion

	private void Update ()
	{
		StoryState storyState = GlobalManager.instance.storyState;

		if (storyState == StoryState.FirstEncounter)
			UpdateOnFirstEncounter ();
		else if (storyState == StoryState.GoForGear)
			UpdateOnGoForGear ();
		else if (storyState == StoryState.SolguardIsSent)
			UpdateOnSolguardIsSent ();
		else if (storyState == StoryState.ConversationBeforeLibrary)
			UpdateOnConversationBeforeLibrary ();
		else if (storyState == StoryState.AquamarineTalk)
			UpdateOnAquamarineTalk ();
	}

	private void UpdateOnFirstEncounter ()
	{
		// Если персонажи закончили говорить (во время первого разговора), отпираем дверь для Алекса и назначаем новый стейт для сюжета: поход за солгардом.
		if (!_firstEncounterSpeechIsOver &&
			!firstEncounterAidaSpeechGO.GetComponent<AudioSource> ().isPlaying &&
			!firstEncounterAlexSpeechGO.GetComponent<AudioSource> ().isPlaying
		)
		{
			_firstEncounterSpeechIsOver = true;
			door.GetComponent<AlexInnerDoor> ().preventInteractions = false;

			// В поход за солгардом.
			GlobalManager.instance.storyState = StoryState.SolguardHike;
		}
	}

	private void UpdateOnGoForGear ()
	{
		// Продолжение костыля. Это случай, когда Алекс вышел на улицу, но вернулся, не отправив солгард.
		if (_goForGearFadeInDelay < 0)
		{

		}
		// Стандартный старт: Алекс и Аида говорят.
		else
		{
			// Fade In после первоначальной задержки.
			if (Time.timeSinceLevelLoad >= _goForGearFadeInDelay)
			{
				ScreenFader screenFader = GlobalManager.instance.screenFader;
				screenFader.speed = 100f;
				screenFader.StartFadeIn ();

				// Предотвращаем дальнейшее срабатывание этого блока кода.
				_goForGearFadeInDelay = Mathf.Infinity;
			}
		}

		// Если контроллер персонажа отключен, проверяем, не пора ли его включать.
		if (!_charController.enableLook)
		{
			OscClip gearClip = GlobalManager.instance.audioManager.GetLayerByType (AudioLayerType.Speech).GetClipWithResourceType (OscAudioResourceEnum.Speech_Gear);
			if (gearClip == null)
			{
				_charController.enableLook = true;
				_charController.enableMove = true;
			} 
		}
	}

	private void UpdateOnSolguardIsSent ()
	{
		// Если Алекс приближается к стене, начинаем разговор.
		if (_charController.gameObject.transform.position.x <= _solguardIsSentDialogueStartX)
		{
			// Переключаем стейт истории.
			GlobalManager.instance.storyState = StoryState.ConversationBeforeLibrary;

			// Запускаем стем Алекса.
			beforeLibraryAlexSpeechGO.GetComponent<AudioSource> ().Play ();
			// Запускаем стем Аиды.
			beforeLibraryAidaSpeechGO.GetComponent<AudioSource> ().Play ();
		}
	}

	private void UpdateOnConversationBeforeLibrary ()
	{
		// Если разговор (до перехода в комнату Аиды) уже закончился, готовимся к телепорту в дом Аиды.
		if (!beforeLibraryAidaSpeechGO.GetComponent<AudioSource> ().isPlaying)
		{
			// Если переход только что начался, запускаем резкий Fade To Black.
			if (Mathf.Approximately (_beforeLibraryTimeToAidaHouseLoad, 0))
			{
				ScreenFader screenFader = GlobalManager.instance.screenFader;
				screenFader.speed = 100f;
				screenFader.StartFadeToBlack ();
			}

			// Если время выдержки достигнуто, грузим сцену.
			_beforeLibraryTimeToAidaHouseLoad += Time.deltaTime;
			if (_beforeLibraryTimeToAidaHouseLoad >= _beforeLibraryAidaHouseLoadDelay)
			{
				_beforeLibraryAidaHouseLoadDelay = Mathf.Infinity;

				GlobalManager.instance.playerStartPointName = "Start Point (3)";
				StartCoroutine (LoadAidaHouseSceneBeforeLibrary ());
			}
		}
	}

	// Загрузка сцены в доме Аиды.
	private IEnumerator LoadAidaHouseSceneBeforeLibrary ()
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync (aidaHouseSceneName);
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
	}

	private void UpdateOnAquamarineTalk ()
	{
		// Когда Аида закончит говорить, загрузим сцену "Продолжение следует".
		if (!_aquamarineTalkAidaSource.isPlaying)
		{
			GlobalManager.instance.storyState = StoryState.ToBeContinued;

			ScreenFader screenFader = GlobalManager.instance.screenFader;
			screenFader.speed = .5f;
			screenFader.StartFadeToBlack (OnFadedOutOnAquamarineTalk);
		}
	}

	private void OnFadedOutOnAquamarineTalk ()
	{
		StartCoroutine (LoadToBeContinuedScene ());
	}

	// Загрузка сцены "Продолжение следует".
	private IEnumerator LoadToBeContinuedScene ()
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync (toBeContinuedSceneName);
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

	#endregion

}
