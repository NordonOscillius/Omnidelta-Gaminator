using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// НЕ ЗАБУДЬ НАЗНАЧИТЬ ОБЪЕКТ НА СЛОЙ INTERACTIVE!!!!!!!!!!
/// </summary>
public class Bed : MonoBehaviour
{
	public GameObject startPointAfterSleep;
	//public string nextSceneName;
	private BedState _state = BedState.Waiting;
	private float _sleepTime = 3.2f;

	private BoxCollider _myCollider;
	private Interactive _myInteractive;

	private enum BedState
	{
		Waiting,
		FadingToBlack,
		Sleeping,
		FadingIn
	}


	private void Awake ()
	{
		_myCollider = GetComponent<BoxCollider> ();
		if (_myCollider == null)
		{
			throw new Exception ("BoxCollider not found.");
		}

		_myInteractive = GetComponent<Interactive> ();
		if (_myInteractive == null)
		{
			throw new Exception ("Interactive not found.");
		}

		if (startPointAfterSleep == null)
		{
			throw new Exception ("Start Point After Sleep not found.");
		}
	}

	private void Start ()
	{
		// Если мы не находимся в эпизоде "Arrival After Speech", то прячем иконку взаимодействия.
		if (GlobalManager.instance.storyState != StoryState.ArrivalAfterSpeech)
		{
			_myInteractive.doShowIcon = false;
		}
	}

	private void Update ()
	{
		if (_state == BedState.Waiting)
			UpdateWaiting ();
		else if (_state == BedState.Sleeping)
			UpdateSleeping ();
	}

	private void UpdateWaiting ()
	{
		GlobalManager globalManager = GlobalManager.instance;

		// Лечь спать можно только в эпизоде "Arrival After Speech". Если мы уже ложимся спать (FadeToBlack), то тоже выходим.
		if (globalManager.storyState != StoryState.ArrivalAfterSpeech)
		{
			return;
		}

		InteractionManager interactionManager = globalManager.interactionManager;
		if (interactionManager.intersectionFound && interactionManager.hitInfo.collider == _myCollider)
		{
			if (Input.GetKeyDown (interactionManager.interactKeyCode))
			{
				// Отключаем иконку.
				_myInteractive.doShowIcon = false;

				// Ляжем поспать.
				_state = BedState.FadingToBlack;
				globalManager.screenFader.speed = 1f;
				globalManager.screenFader.StartFadeToBlack (OnFadedToBlack);

				// Заглушаем музыку.
				AudioLayer audioLayer = globalManager.audioManager.GetLayerByType (AudioLayerType.Music);
				audioLayer.outroSpeed = 1 / 3.5f;
				audioLayer.FadeOut ();
			}
		}
	}

	private void UpdateWaitingOld ()
	{
		GlobalManager globalManager = GlobalManager.instance;

		// Лечь спать можно только в эпизоде "Arrival After Speech". Если мы уже ложимся спать (FadeToBlack), то тоже выходим.
		//if (globalManager.storyState != StoryState.ArrivalAfterSpeech || _state != BedState.Waiting)
		if (globalManager.storyState != StoryState.ArrivalAfterSpeech)
		{
			return;
		}

		Camera alexCamera = globalManager.alexCamera;
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
			interactiveLayerMask,
			QueryTriggerInteraction.Collide
		);

		// Если игрок целится в этот объект.
		if (intersectionFound && hitInfo.collider == _myCollider)
		{
			globalManager.ShowInteractionIcon (InteractionType.Sleep);

			if (Input.GetKeyDown (KeyCode.E))
			{
				globalManager.HideInteractionIcon (InteractionType.Sleep);

				// Ляжем поспать.
				_state = BedState.FadingToBlack;
				globalManager.screenFader.speed = 1f;
				globalManager.screenFader.StartFadeToBlack (OnFadedToBlack);

				// Заглушаем музыку.
				AudioLayer audioLayer = globalManager.audioManager.GetLayerByType (AudioLayerType.Music);
				audioLayer.outroSpeed = 1 / 3.5f;
				audioLayer.FadeOut ();
			}
		}
		// Если игрок не целится.
		else
		{
			globalManager.HideInteractionIcon (InteractionType.Sleep);
		}
	}

	private void UpdateSleeping ()
	{
		_sleepTime -= Time.deltaTime;
		if (_sleepTime < 0)
		{
			_state = BedState.FadingIn;

			// Запускаем появление картинки.
			GlobalManager.instance.screenFader.speed = .5f;
			GlobalManager.instance.screenFader.StartFadeIn (OnFadedIn);
		}
	}

	// Вызывается после того, как экран ушел в черное.
	private void OnFadedToBlack ()
	{
		GlobalManager globalManager = GlobalManager.instance;

		// Переключаем стейты.
		globalManager.storyState = StoryState.FirstDays;
		_state = BedState.Sleeping;

		// Помещаем персонажа в нужное место.
		CharController charController = globalManager.alexCamera.transform.root.gameObject.GetComponent<CharController> ();
		charController.transform.position = startPointAfterSleep.transform.position;
		Vector3 targetAngles = startPointAfterSleep.transform.rotation.eulerAngles;
		charController.yawDegrees = targetAngles.y;
		charController.pitchDegrees = targetAngles.x;
	}

	// Вызывается после того, как персонаж проснулся.
	private void OnFadedIn ()
	{
		_state = BedState.Waiting;
	}

	//private IEnumerator LoadNextScene ()
	//{
	//	AsyncOperation asyncLoad = SceneManager.LoadSceneAsync (nextSceneName);

	//	while (!asyncLoad.isDone)
	//	{
	//		yield return null;
	//	}
	//}

}
