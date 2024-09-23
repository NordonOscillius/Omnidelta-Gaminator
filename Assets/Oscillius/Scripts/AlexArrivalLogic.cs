using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlexArrivalLogic : MonoBehaviour
{
	[Tooltip ("������, �������� ������� ����� ������.")]
	public GameObject terrainGO;
	[Tooltip ("������, �������� ������ PauseMenu.")]
	public GameObject pauseMenuGO;

	private AlexArrivalState _state = AlexArrivalState.BeforeFadeIn;
	private AlexArrivalSpeechState _speechState = AlexArrivalSpeechState.BeforeSpeechStart;
	private float _timerBeforeFadeIn = 4.8f;
	private float _timerBeforeSpeech = 1.6f;
	private AudioSource _speechSource;
	private CharController _charController;
	private GameObject _playerStartPointGO;

	private float _fogDensityStart = .6f;
	// ��������������� �� �����, � ���� ��� �����.
	private float _fogDensityEnd = 0f;
	// �������� ��� ������������ �������������� ������� ����� _fogDensityStiffnessStdTime.
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
		// ������ ������ ������������ � ����������� ��� ��������.
		ScreenFader screenFader = GlobalManager.instance.screenFader;
		screenFader.opacity = 1f;
		screenFader.speed = 3f;

		// ���� �������� ����� ��� �������� ������.
		GameObject speechGO = GameObject.Find ("Speech A1");
		if (speechGO == null)
			throw new System.Exception ("Speech GameObject not found.");
		_speechSource = speechGO.GetComponent<AudioSource> ();
		if (_speechSource == null)
			throw new System.Exception ("Speech AudioSource component not found.");

		// �������� Animator.
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

		// �������� �������.
		if (terrainGO != null)
			_terrain = terrainGO.GetComponent<Terrain> ();
	}

	private void AwakeOnArrivalAfterSpeech ()
	{
		// ������ ������ ������������ � ����������� ��� ��������.
		ScreenFader screenFader = GlobalManager.instance.screenFader;
		screenFader.opacity = 1f;
		screenFader.speed = 1f;

		// ���� Start Point, � ������� �������� ��������.
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

		// �������� �������.
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

			// �������� � ���� ����� ������ �� �������, ����� �� ���������� ��� ��������� ��������.
			pauseMenuGO.GetComponent<PauseMenu> ().terrain = _terrain;
		}
	}

	private void StartOnArrival ()
	{
		// �������� �������.
		AudioLayer ambientLayer = GlobalManager.instance.audioManager.GetLayerByType (AudioLayerType.Ambient);
		ambientLayer.introSpeed = .25f;
		ambientLayer.outroSpeed = .25f;
		OscClip forestClip = ambientLayer.IntroduceClip (OscAudioResourceEnum.Ambience_Forest, _ambienceLoudness);

		// �������� ������.
		AudioLayer musicLayer = GlobalManager.instance.audioManager.GetLayerByType (AudioLayerType.Music);
		musicLayer.introSpeed = 1e4f;
		musicLayer.outroSpeed = .5f;
		OscClip arrivalClip = musicLayer.IntroduceClip (OscAudioResourceEnum.Music_Arrival_a, false, _musicLoudness);

		// ��������� ���������� �� �����.
		_charController = GlobalManager.instance.alexCamera.transform.root.gameObject.GetComponent<CharController> ();
		_charController.enableMove = false;
		_charController.enableLook = false;

		// ���������� �������� ��� ��������� ������ (�� �����); ������������� ��������� � ��������� ��������.
		_fogDensityEnd = RenderSettings.fogDensity;
		RenderSettings.fogDensity = _fogDensityStart;
	}

	private void StartOnArrivalAfterSpeech ()
	{
		// ��������� Fade In (��������� ���� ������� � AwakeOnArrivalAfterSpeech ()).
		GlobalManager.instance.screenFader.StartFadeIn ();

		// �������� �������.
		AudioLayer ambientLayer = GlobalManager.instance.audioManager.GetLayerByType (AudioLayerType.Ambient);
		ambientLayer.introSpeed = .25f;
		ambientLayer.outroSpeed = .25f;
		OscClip forestClip = ambientLayer.IntroduceClip (OscAudioResourceEnum.Ambience_Forest);

		// �������� ���������� ���������.
		_charController = GlobalManager.instance.alexCamera.transform.root.gameObject.GetComponent<CharController> ();

		// ���� Start Point �������, �� ������������ ��������� � ���.
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
		// ������ ��� ���������� ������� ���� _timerBeforeSpeech.

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


		// ������ ��� ���������� ������ _state (AlexArrivalState).

		if (_state == AlexArrivalState.BeforeFadeIn)
		{
			_timerBeforeFadeIn -= Time.deltaTime;
			if (_timerBeforeFadeIn <= 0)
			{
				_state = AlexArrivalState.FadeInInProgress;

				// ��������� ������������ ������� �������.
				GlobalManager.instance.screenFader.speed = 100f;
				GlobalManager.instance.screenFader.StartFadeIn (OnFadedIn);
			}
		}
		else if (_state == AlexArrivalState.FadeInInProgress)
		{
			// ������ �� ������, ����, ����� ����� OnFadedIn ().
		}
		else if (_state == AlexArrivalState.AfterFadeIn)
		{
			//RenderSettings.fogDensity = .2f;
		}

		// ������ ��� ���������� ������.
		
		// ����� ������ ������������ ����� ����� ������ FadeIn. ����� ����������� ������ � ������� Arrival (���� ����� �� ���� � ArrivalAfterSpeech, �������� �� �����).
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

		// ������ ��� ���������� ������.

		_omnideltaAnimator.SetFloat ("Arrival Time", Time.timeSinceLevelLoad);
		_titlesAnimator.SetFloat ("Arrival Time", Time.timeSinceLevelLoad);
	}

	private void UpdateOnArrivalAfterSpeech ()
	{

	}

	// � ������ �����: ������������ ������� ��������� �� ����, �������� ��������� �����.
	private void OnFadedIn ()
	{
		_state = AlexArrivalState.AfterFadeIn;

		_charController.enableMove = true;
		_charController.enableLook = true;
	}

}
