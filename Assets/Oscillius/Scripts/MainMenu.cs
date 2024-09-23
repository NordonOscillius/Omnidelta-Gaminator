using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
	public OscAudioResourceEnum music;
	public float musicLoudness = 1f;

	public Button startButton;
	public Button exitButton;

	public GameObject terrainGO;

	public string newGameSceneName;

	// Camera settings.
	private Vector3 _cameraInitPosition;
	private Quaternion _cameraInitRotation;
	private float _cameraMaxSpeed = 1f;
	private float _cameraCurSpeed = 0f;
	private bool _cameraIsMovingRight = true;
	private float _cameraTimePassed = 0;
	private float _cameraPeriod = 51f;
	private float _cameraAccelerationTime = 5f;


	private void Awake ()
	{
		GlobalManager globalManager = GlobalManager.instance;

		globalManager.storyState = StoryState.MainMenu;

		globalManager.audioManager.GetLayerByType (AudioLayerType.Music).IntroduceClip (music, musicLoudness);

		globalManager.screenFader.speed = .5f;
		globalManager.screenFader.opacity = 1f;
	}

	private void Start ()
	{
		GlobalManager.instance.screenFader.StartFadeIn (OnFadedIn);

		// Camera
		_cameraInitPosition = GlobalManager.instance.alexCamera.transform.position;
		_cameraInitRotation = GlobalManager.instance.alexCamera.transform.rotation;

		Cursor.lockState = CursorLockMode.None;

		//startButton.onClick.AddListener (OnStartButtonClicked);
		//exitButton.onClick.AddListener (OnExitButtonClicked);

		if (terrainGO != null)
		{
			Terrain terrain = terrainGO.GetComponent<Terrain> ();
			terrain.detailObjectDistance = GlobalManager.instance.grassDistance;
			terrain.detailObjectDensity = GlobalManager.instance.grassDensity;
		}
	}

	private void OnFadedIn ()
	{
		startButton.onClick.AddListener (OnStartButtonClicked);
		exitButton.onClick.AddListener (OnExitButtonClicked);
	}

	private void Update ()
	{
		// Двигаем камеру влево или вправо.
		_cameraTimePassed += Time.deltaTime;
		if (_cameraTimePassed < _cameraAccelerationTime)
		{
			_cameraCurSpeed = _cameraMaxSpeed * _cameraTimePassed / _cameraAccelerationTime;
		}
		else if (_cameraTimePassed > _cameraPeriod - _cameraAccelerationTime)
		{
			if (_cameraTimePassed >= _cameraPeriod)
			{
				_cameraTimePassed -= _cameraPeriod;
				_cameraIsMovingRight = !_cameraIsMovingRight;

				_cameraCurSpeed = _cameraMaxSpeed * _cameraTimePassed / _cameraAccelerationTime;
			}
			else
			{
				_cameraCurSpeed = _cameraMaxSpeed * (_cameraPeriod - _cameraTimePassed) / _cameraAccelerationTime;
			}
		}

		Camera alexCamera = GlobalManager.instance.alexCamera;
		float deltaCoord = _cameraCurSpeed * Time.deltaTime;
		if (!_cameraIsMovingRight)
			deltaCoord = -deltaCoord;

		alexCamera.transform.position = alexCamera.transform.position + alexCamera.transform.right * deltaCoord;
	}

	private void OnDisable ()
	{
		GlobalManager.instance.audioManager.GetLayerByType (AudioLayerType.Music).FadeOut ();
	}

	private void RemoveListeners ()
	{
		startButton.onClick.RemoveListener (OnStartButtonClicked);
		exitButton.onClick.RemoveListener (OnExitButtonClicked);
	}

	private void OnStartButtonClicked ()
	{
		if (newGameSceneName == null || newGameSceneName == "")
			return;

		RemoveListeners ();
		GlobalManager.instance.audioManager.GetLayerByType (AudioLayerType.Music).FadeOut ();

		//StartCoroutine (GlobalManager.instance.ScreenFade (true, 1));
		//GlobalManager.instance.blackScreen.color = new Color (0, 0, 0, 1);
		GlobalManager.instance.screenFader.speed = 1f;
		GlobalManager.instance.screenFader.StartFadeToBlack (OnFadedToBlack);

		//StartCoroutine (LoadNewGameScene ());
	}

	private void OnFadedToBlack ()
	{
		GlobalManager.instance.storyState = StoryState.Arrival;
		StartCoroutine (LoadNewGameScene ());
	}

	private void OnExitButtonClicked ()
	{
		Application.Quit ();
	}

	private IEnumerator LoadNewGameScene ()
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync (newGameSceneName);

		while (!asyncLoad.isDone)
		{
			yield return null;
		}
	}

}
