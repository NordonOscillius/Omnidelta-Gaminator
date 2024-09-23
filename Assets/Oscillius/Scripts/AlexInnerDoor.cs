using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AlexInnerDoor : MonoBehaviour
{
	[Tooltip ("Название сцены с прибытием Алекса.")]
	public string arrivalSceneName;
	[Tooltip ("Название пустого игрового объекта, в положение которого будет перекинут персонаж при загрузке сцены с прибытием Алекса.")]
	public string arrivalStartPointName;
	[Tooltip ("Название основной сцены при игре за Алекса (весна).")]
	public string alexSpringSceneName;
	[Tooltip ("Название пустого игрового объекта, в положение которого будет перекинут персонаж при загрузке сцены \"Весна\".")]
	public string alexSpringStartPointName;

	private AlexInnerDoorState _state = AlexInnerDoorState.Waiting;
	// Время, в течение которого картинка перед сменой сцены будет затемняться.
	private float _fadeOutSpeed = 2f;
	private float _soundFadeOutSpeed = 1f;

	// Если true, интерактивность двери будет отключена.
	private bool _preventInteractions = false;

	// Бокс-коллайдер, добавленный к тому же игровому объекту, что и этот скрипт.
	private BoxCollider _myCollider;
	// Компонент Interactive, необходимый для работы Interaction Manager'а.
	private Interactive _myInteractive;
	// Целился ли игрок в этот объект в предыдущем апдейте.
	private bool _prevInteractionIsPossible = false;


	public enum AlexInnerDoorState
	{
		Waiting,
		IsEntering,
		Entered
	}


	private void Awake ()
	{
		_myCollider = GetComponent<BoxCollider> ();
		if (_myCollider == null)
		{
			throw new Exception ("Бокс-коллайдер не найден.");
		}

		_myInteractive = GetComponent<Interactive> ();
		if (_myInteractive == null)
		{
			throw new Exception ("Interactive component not found.");
		}
		this.preventInteractions = _preventInteractions;
	}

	private void Update ()
	{
		// Если интерактивность двери отключена, выходим.
		if (_preventInteractions)
			return;

		GlobalManager globalManager = GlobalManager.instance;

		if (globalManager.interactionManager.intersectionFound)
		{
			if (globalManager.interactionManager.hitInfo.collider == _myCollider)
			{
				// Запускаем переход к Fade Out и загрузке следующей сцены, только если он еще не запущен.
				if (Input.GetKeyDown (KeyCode.E) && _state == AlexInnerDoorState.Waiting)
				{
					GetNextSceneParameters (out string usedSceneName, out string usedStartPointName);

					if (usedSceneName != null && usedSceneName != "")
					{
						_state = AlexInnerDoorState.IsEntering;

						globalManager.screenFader.speed = _fadeOutSpeed;
						globalManager.screenFader.StartFadeToBlack (OnFadedOut);

						AudioLayer ambientLayer = globalManager.audioManager.GetLayerByType (AudioLayerType.Ambient);
						ambientLayer.outroSpeed = _soundFadeOutSpeed;
						ambientLayer.FadeOut ();

						globalManager.playerStartPointName = usedStartPointName;

						PlayDoorEffect ();
					}
				}
			}
		}
	}

	/// <summary>
	/// [Deprecated].
	/// </summary>
	private void UpdateOld ()
	{
		// Если интерактивность двери отключена, выходим.
		if (_preventInteractions)
		{
			return;
		}

		GlobalManager globalManager = GlobalManager.instance;

		Camera alexCamera = globalManager.alexCamera;
		if (alexCamera == null)
			return;

		int interactiveLayer = LayerMask.NameToLayer (globalManager.interactiveLayerName);
		if (interactiveLayer < 0)
		{
			throw new Exception ("Interactive Layer not found by its name.");
		}
		int interactiveLayerMask = 1 << interactiveLayer;

		RaycastHit hitInfo;
		bool intersectionFound = Physics.Raycast (
			alexCamera.transform.position,
			alexCamera.transform.forward,
			out hitInfo,
			globalManager.maxInteractionDistance,
			interactiveLayerMask
		);

		// Если игрок целится в этот объект.
		if (intersectionFound && hitInfo.collider == _myCollider)
		{
			// Показываем иконку "Открыть", если нужно.
			if (!_prevInteractionIsPossible)
			{
				globalManager.ShowInteractionIcon (InteractionType.Open);
			}

			// Запускаем переход к Fade Out и загрузке следующей сцены, только если он еще не запущен.
			if (Input.GetKeyDown (KeyCode.E) && _state == AlexInnerDoorState.Waiting)
			{
				GetNextSceneParameters (out string usedSceneName, out string usedStartPointName);

				if (usedSceneName != null && usedSceneName != "")
				{
					_state = AlexInnerDoorState.IsEntering;

					globalManager.screenFader.speed = _fadeOutSpeed;
					globalManager.screenFader.StartFadeToBlack (OnFadedOut);

					AudioLayer ambientLayer = globalManager.audioManager.GetLayerByType (AudioLayerType.Ambient);
					ambientLayer.outroSpeed = _soundFadeOutSpeed;
					ambientLayer.FadeOut ();

					globalManager.playerStartPointName = usedStartPointName; 
				}
			}
		}
		// Игрок не целится.
		else
		{
			if (_prevInteractionIsPossible)
				globalManager.HideInteractionIcon (InteractionType.Open);
		}

		// Сбрасываем prev-флаг.
		_prevInteractionIsPossible = intersectionFound;
	}

	private void GetNextSceneParameters (out string nextSceneName, out string nextStartPointName)
	{
		nextSceneName = null;
		nextStartPointName = null;

		StoryState storyState = GlobalManager.instance.storyState;
		if (storyState == StoryState.Arrival || storyState == StoryState.ArrivalAfterSpeech)
		{
			nextSceneName = arrivalSceneName;
			nextStartPointName = arrivalStartPointName;
		}
		else if (storyState == StoryState.FirstDays)
		{
			nextSceneName = alexSpringSceneName;
			nextStartPointName = alexSpringStartPointName;
		}
		else if (storyState == StoryState.FirstEncounter)
		{
			nextSceneName = alexSpringSceneName;
			nextStartPointName = alexSpringStartPointName;
		}
		else if (storyState == StoryState.SolguardHike)
		{
			nextSceneName = alexSpringSceneName;
			nextStartPointName = alexSpringStartPointName;
		}
		else if (storyState == StoryState.GoForGear)
		{
			nextSceneName = alexSpringSceneName;
			nextStartPointName = alexSpringStartPointName;
		}
	}

	private void OnFadedOut ()
	{
		_state = AlexInnerDoorState.Entered;

		GetNextSceneParameters (out string usedSceneName, out string usedStartPointName);
		StartCoroutine (LoadNextScene (usedSceneName));
	}

	/// <summary>
	/// Асинхронно загружает сцену.
	/// </summary>
	/// <returns></returns>
	private IEnumerator LoadNextScene (string nextSceneName)
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync (nextSceneName);

		while (!asyncLoad.isDone)
		{
			yield return null;
		}
	}


	private void PlayDoorEffect ()
	{
		AudioLayer effectsLayer = GlobalManager.instance.audioManager.GetLayerByType (AudioLayerType.Effects);
		effectsLayer.introSpeed = 100f;
		effectsLayer.IntroduceClip (OscAudioResourceEnum.Effects_Door_a, false, 1f);
	}


	// ====================================================
	// ==================== PROPERTIES ====================
	// ====================================================

	// Если true, интерактивность двери будет отключена.
	public bool preventInteractions
	{
		get { return _preventInteractions; }
		set {
			_preventInteractions = value;
			_myInteractive.doShowIcon = !value;
		}
	}

}
