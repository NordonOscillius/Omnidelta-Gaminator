using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// НЕ ЗАБУДЬ НАЗНАЧИТЬ ОБЪЕКТУ СЛОЙ "Interactive".
/// </summary>
public class AlexDoorScript : MonoBehaviour
{
	private GlobalManager _globalManager;

	[Tooltip ("Название сцены, которая будет загружена при входе в эту дверь.")]
	public string nextSceneName;
	[Tooltip ("Название пустого игрового объекта, в положение которого будет перекинут персонаж при загрузке следующей сцены.")]
	public string nextStartPointName;
	[Tooltip ("Если true, то скрипт повешен на дверь внутри дома; иначе - на дверь снаружи.")]
	public bool isIndoor;

	private AlexDoorState _state = AlexDoorState.Waiting;
	// Время, в течение которого картинка перед сменой сцены будет затемняться.
	private float _fadeOutSpeed = 2f;
	private float _soundFadeOutSpeed = 1f;

	private bool _preventInteractions;

	// Бокс-коллайдер, добавленный к тому же игровому объекту, что и этот скрипт.
	private BoxCollider _myCollider;
	// Компонент Interactive, необходимый для работы Interaction Manager'а.
	private Interactive _myInteractive;
	// Целился ли игрок в этот объект в предыдущем апдейте.
	private bool _prevInteractionIsPossible = false;

	public enum AlexDoorState
	{
		Waiting,
		IsEntering,
		Entered
	}


	private void Awake ()
	{
		GameObject globalManagerGO = GameObject.Find ("Global Manager");
		if (globalManagerGO == null)
		{
			throw new Exception ("Can't find Global Manager game object.");
		}
		_globalManager = globalManagerGO.GetComponent<GlobalManager> ();
		if (_globalManager == null)
		{
			throw new Exception ("Global Manager game object doesn't contain a GlobalManager component.");
		}

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
		if (_preventInteractions)
			return;

		GlobalManager globalManager = GlobalManager.instance;

		if (globalManager.interactionManager.intersectionFound)
		{
			if (globalManager.interactionManager.hitInfo.collider == _myCollider)
			{
				// Запускаем переход к Fade Out и загрузке следующей сцены, только если он еще не запущен.
				if (Input.GetKeyDown (KeyCode.E) && _state == AlexDoorState.Waiting && nextSceneName != null && nextSceneName != "")
				{
					_state = AlexDoorState.IsEntering;

					_globalManager.screenFader.speed = _fadeOutSpeed;
					_globalManager.screenFader.StartFadeToBlack (OnFadedOut);

					AudioLayer ambientLayer = _globalManager.audioManager.GetLayerByType (AudioLayerType.Ambient);
					ambientLayer.outroSpeed = _soundFadeOutSpeed;
					ambientLayer.FadeOut ();

					globalManager.playerStartPointName = nextStartPointName;

					PlayDoorEffect ();
				}
			}
		}
	}

	/// <summary>
	/// [Deprecated].
	/// </summary>
	private void UpdateOld ()
	{
		if (_globalManager == null)
			return;

		if (_preventInteractions)
			return;

		Camera alexCamera = _globalManager.alexCamera;
		if (alexCamera == null)
			return;

		int interactiveLayer = LayerMask.NameToLayer (_globalManager.interactiveLayerName);
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
			_globalManager.maxInteractionDistance,
			interactiveLayerMask
		);

		// Если игрок целится в этот объект.
		if (intersectionFound && hitInfo.collider == _myCollider)
		{
			// Показываем иконку "Открыть", если нужно.
			if (!_prevInteractionIsPossible)
			{
				_globalManager.ShowInteractionIcon (InteractionType.Open);
			}

			// Запускаем переход к Fade Out и загрузке следующей сцены, только если он еще не запущен.
			if (Input.GetKeyDown (KeyCode.E) && _state == AlexDoorState.Waiting && nextSceneName != null && nextSceneName != "")
			{
				_state = AlexDoorState.IsEntering;

				_globalManager.screenFader.speed = _fadeOutSpeed;
				_globalManager.screenFader.StartFadeToBlack (OnFadedOut);

				AudioLayer ambientLayer = _globalManager.audioManager.GetLayerByType (AudioLayerType.Ambient);
				ambientLayer.outroSpeed = _soundFadeOutSpeed;
				ambientLayer.FadeOut ();

				GlobalManager.instance.playerStartPointName = nextStartPointName;
			}
		}
		// Игрок не целится.
		else
		{
			if (_prevInteractionIsPossible)
				_globalManager.HideInteractionIcon (InteractionType.Open);
		}

		// Сбрасываем prev-флаг.
		_prevInteractionIsPossible = intersectionFound;
	}

	private void OnFadedOut ()
	{
		_state = AlexDoorState.Entered;
		StartCoroutine (LoadHouseScene ());
	}

	/// <summary>
	/// Асинхронно загружает сцену.
	/// </summary>
	/// <returns></returns>
	private IEnumerator LoadHouseScene ()
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
