using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlexArrivalLogic : MonoBehaviour
{
	[Tooltip ("Объект, хранящий террейн этого уровня.")]
	public GameObject terrainGO;
	[Tooltip ("Объект, хранящий скрипт PauseMenu.")]
	public GameObject pauseMenuGO;

	private AlexArrivalState _state = AlexArrivalState.BeforeFadeIn;
	private AlexArrivalSpeechState _speechState = AlexArrivalSpeechState.BeforeSpeechStart;
	private float _timerBeforeFadeIn = 4.8f;
	private float _timerBeforeSpeech = 1.6f;
	private AudioSource _speechSource;
	private CharController _charController;
	private GameObject _playerStartPointGO;

	private float _fogDensityStart = .6f;
	// Устанавливается не здесь, а ниже при ините.
	private float _fogDensityEnd = 0f;
	// Задается для стандартного фиксированного времени кадра _fogDensityStiffnessStdTime.
	private float _fogDensityStiffness = .07f;
	private float _fogDensityStiffnessStdTime = 1 / 50f;
	private float _fogDensityTime = 4f;

	private float _ambienceLoudness = .6f;
	private float _musicLoudness = .4f;
	private float _speechVolume = .9f;

	private Animator _omnideltaAnimator;
	private Animator _titlesAnimator;

	private Terrain _terrain;

	public enum AlexArrivalState
	{
		BeforeFadeIn,
		FadeInInProgress,
		AfterFadeIn
	}

	public enum AlexArrivalSpeechState
	{
		BeforeSpeechStart,
		AfterSpeechStart
	}


	private void Awake ()
	{
		if (GlobalManager.instance.storyState == StoryState.Arrival)
		{
			AwakeOnArrival ();
		}
		else if (GlobalManager.instance.storyState == StoryState.ArrivalAfterSpeech)
		{
			AwakeOnArrivalAfterSpeech ();
		}
	}

	private void AwakeOnArrival ()
	{
		// Делаем фейдер непрозрачным и настраиваем его скорость.
		ScreenFader screenFader = GlobalManager.instance.screenFader;
		screenFader.opacity = 1f;
		screenFader.speed = 3f;

		// Ищем источник звука для рассказа Алекса.
		GameObject speechGO = GameObject.Find ("Speech A1");
		if (speechGO == null)
			throw new System.Exception ("Speech GameObject not found.");
		_speechSource = speechGO.GetComponent<AudioSource> ();
		if (_speechSource == null)
			throw new System.Exception ("Speech AudioSource component not found.");

		// Кэшируем Animator.
		GameObject omnideltaImage = GameObject.Find ("Omnidelta Image");
		if (omnideltaImage == null)
			throw new System.Exception ("GameObject named 'Omnidelta Image' not found.");
		_omnideltaAnimator = omnideltaImage.GetComponent<Animator> ();
		if (_omnideltaAnimator == null)
			throw new System.Exception ("Animator component not found.");

		GameObject titlesImage = GameObject.Find ("Titles Image");
		_titlesAnimator = titlesImage.GetComponent<Animator> ();
		if (_titlesAnimator == null)
			throw new System.Exception ("Animator component not found.");

		// Кэшируем террейн.
		if (terrainGO != null)
			_terrain = terrainGO.GetComponent<Terrain> ();
	}

	private void AwakeOnArrivalAfterSpeech ()
	{
		// Делаем фейдер непрозрачным и настраиваем его скорость.
		ScreenFader screenFader = GlobalManager.instance.screenFader;
		screenFader.opacity = 1f;
		screenFader.speed = 1f;

		// Ищем Start Point, в которой появится персонаж.
		string playerStartPointName = GlobalManager.instance.playerStartPointName;
		if (playerStartPointName != null && playerStartPointName != "")
		{
			GameObject playerStartPointGO = GameObject.Find (playerStartPointName);
			if (playerStartPointGO == null)
			{
				Debug.Log ("Start Point with name '" + playerStartPointName + "' not found.");
			}
			_playerStartPointGO = playerStartPointGO;
		}
		else
		{
			_playerStartPointGO = null;
		}

		// Кэшируем террейн.
		if (terrainGO != null)
			_terrain = terrainGO.GetComponent<Terrain> ();
	}

	private void Start ()
	{
		StoryState storyState = GlobalManager.instance.storyState;
		if (storyState == StoryState.Arrival)
		{
			StartOnArrival ();
		}
		else if (storyState == StoryState.ArrivalAfterSpeech)
		{
			StartOnArrivalAfterSpeech ();
		}

		// Terrain settings.
		if (_terrain != null)
		{
			_terrain.detailObjectDistance = GlobalManager.instance.grassDistance;
			_terrain.detailObjectDensity = GlobalManager.instance.grassDensity;

			// Передаем в меню паузы ссылку на террейн, чтобы он обновлялся при изменении настроек.
			pauseMenuGO.GetComponent<PauseMenu> ().terrain = _terrain;
		}
	}

	private void StartOnArrival ()
	{
		// Нагоняем эмбиент.
		AudioLayer ambientLayer = GlobalManager.instance.audioManager.GetLayerByType (AudioLayerType.Ambient);
		ambientLayer.introSpeed = .25f;
		ambientLayer.outroSpeed = .25f;
		OscClip forestClip = ambientLayer.IntroduceClip (OscAudioResourceEnum.Ambience_Forest, _ambienceLoudness);

		// Включаем музыку.
		AudioLayer musicLayer = GlobalManager.instance.audioManager.GetLayerByType (AudioLayerType.Music);
		musicLayer.introSpeed = 1e4f;
		musicLayer.outroSpeed = .5f;
		OscClip arrivalClip = musicLayer.IntroduceClip (OscAudioResourceEnum.Music_Arrival_a, false, _musicLoudness);

		// Отключаем управление на время.
		_charController = GlobalManager.instance.alexCamera.transform.root.gameObject.GetComponent<CharController> ();
		_charController.enableMove = false;
		_charController.enableLook = false;

		// Запоминаем максимум для плотности тумана (из сцены); устанавливаем плотность в начальное значение.
		_fogDensityEnd = RenderSettings.fogDensity;
		RenderSettings.fogDensity = _fogDensityStart;
	}

	private void StartOnArrivalAfterSpeech ()
	{
		// Запускаем Fade In (настройки были сделаны в AwakeOnArrivalAfterSpeech ()).
		GlobalManager.instance.screenFader.StartFadeIn ();

		// Нагоняем эмбиент.
		AudioLayer ambientLayer = GlobalManager.instance.audioManager.GetLayerByType (AudioLayerType.Ambient);
		ambientLayer.introSpeed = .25f;
		ambientLayer.outroSpeed = .25f;
		OscClip forestClip = ambientLayer.IntroduceClip (OscAudioResourceEnum.Ambience_Forest);

		// Кэшируем контроллер персонажа.
		_charController = GlobalManager.instance.alexCamera.transform.root.gameObject.GetComponent<CharController> ();

		// Если Start Point найдена, то перекидываем персонажа в нее.
		if (_playerStartPointGO != null)
		{
			_charController.transform.position = _playerStartPointGO.transform.position;
			Vector3 targetAngles = _playerStartPointGO.transform.rotation.eulerAngles;
			_charController.yawDegrees = targetAngles.y;
			_charController.pitchDegrees = targetAngles.x;
		}
	}

	private void Update ()
	{
		StoryState storyState = GlobalManager.instance.storyState;
		if (storyState == StoryState.Arrival)
		{
			UpdateOnArrival ();
		}
		else if (storyState == StoryState.ArrivalAfterSpeech)
		{
			UpdateOnArrivalAfterSpeech ();
		}
	}

	private void UpdateOnArrival ()
	{
		// Раздел для обновления таймера речи _timerBeforeSpeech.

		if (_speechState == AlexArrivalSpeechState.BeforeSpeechStart)
		{
			_timerBeforeSpeech -= Time.deltaTime;
			if (_timerBeforeSpeech <= 0)
			{
				_speechState = AlexArrivalSpeechState.AfterSpeechStart;
				_speechSource.volume = _speechVolume;
				_speechSource.Play ();
			}
		}
		else if (_speechState == AlexArrivalSpeechState.AfterSpeechStart)
		{
			// ...
		}


		// Раздел для обновления стейта _state (AlexArrivalState).

		if (_state == AlexArrivalState.BeforeFadeIn)
		{
			_timerBeforeFadeIn -= Time.deltaTime;
			if (_timerBeforeFadeIn <= 0)
			{
				_state = AlexArrivalState.FadeInInProgress;

				// Уменьшаем прозрачность черного фейдера.
				GlobalManager.instance.screenFader.speed = 100f;
				GlobalManager.instance.screenFader.StartFadeIn (OnFadedIn);
			}
		}
		else if (_state == AlexArrivalState.FadeInInProgress)
		{
			// Ничего не делаем, ждем, когда будет OnFadedIn ().
		}
		else if (_state == AlexArrivalState.AfterFadeIn)
		{
			//RenderSettings.fogDensity = .2f;
		}

		// Раздел для обновления тумана.
		
		// Туман начнет раздвигаться сразу после начала FadeIn. Туман анимируется только в эпизоде Arrival (если выйти из дома в ArrivalAfterSpeech, анимации не будет).
		if (_state != AlexArrivalState.BeforeFadeIn && GlobalManager.instance.storyState == StoryState.Arrival)
		{
			_fogDensityTime -= Time.deltaTime;
			if (_fogDensityTime > 0)
			{
				float curFogDensity = RenderSettings.fogDensity;
				float stiffnessUnfixed = 1f - Mathf.Pow ((1f - _fogDensityStiffness), Time.deltaTime / _fogDensityStiffnessStdTime);
				//float stiffnessUnfixed = _fogDensityStiffness;
				RenderSettings.fogDensity = curFogDensity + (_fogDensityEnd - curFogDensity) * stiffnessUnfixed;
			}
			else
			{
				RenderSettings.fogDensity = _fogDensityEnd;
			}
		}

		// Раздел для обновления титров.

		_omnideltaAnimator.SetFloat ("Arrival Time", Time.timeSinceLevelLoad);
		_titlesAnimator.SetFloat ("Arrival Time", Time.timeSinceLevelLoad);
	}

	private void UpdateOnArrivalAfterSpeech ()
	{

	}

	// В начале сцены: прозрачность фейдера уменьшена до нуля, картинка полностью видна.
	private void OnFadedIn ()
	{
		_state = AlexArrivalState.AfterFadeIn;

		_charController.enableMove = true;
		_charController.enableLook = true;
	}

}
