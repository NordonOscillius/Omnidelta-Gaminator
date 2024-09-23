using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// �� ������ ��������� ������ �� ���� INTERACTIVE!!!!!!!!!!
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
		// ���� �� �� ��������� � ������� "Arrival After Speech", �� ������ ������ ��������������.
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

		// ���� ����� ����� ������ � ������� "Arrival After Speech". ���� �� ��� ������� ����� (FadeToBlack), �� ���� �������.
		if (globalManager.storyState != StoryState.ArrivalAfterSpeech)
		{
			return;
		}

		InteractionManager interactionManager = globalManager.interactionManager;
		if (interactionManager.intersectionFound && interactionManager.hitInfo.collider == _myCollider)
		{
			if (Input.GetKeyDown (interactionManager.interactKeyCode))
			{
				// ��������� ������.
				_myInteractive.doShowIcon = false;

				// ����� �������.
				_state = BedState.FadingToBlack;
				globalManager.screenFader.speed = 1f;
				globalManager.screenFader.StartFadeToBlack (OnFadedToBlack);

				// ��������� ������.
				AudioLayer audioLayer = globalManager.audioManager.GetLayerByType (AudioLayerType.Music);
				audioLayer.outroSpeed = 1 / 3.5f;
				audioLayer.FadeOut ();
			}
		}
	}

	private void UpdateWaitingOld ()
	{
		GlobalManager globalManager = GlobalManager.instance;

		// ���� ����� ����� ������ � ������� "Arrival After Speech". ���� �� ��� ������� ����� (FadeToBlack), �� ���� �������.
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

		// ���� ����� ������� � ���� ������.
		if (intersectionFound && hitInfo.collider == _myCollider)
		{
			globalManager.ShowInteractionIcon (InteractionType.Sleep);

			if (Input.GetKeyDown (KeyCode.E))
			{
				globalManager.HideInteractionIcon (InteractionType.Sleep);

				// ����� �������.
				_state = BedState.FadingToBlack;
				globalManager.screenFader.speed = 1f;
				globalManager.screenFader.StartFadeToBlack (OnFadedToBlack);

				// ��������� ������.
				AudioLayer audioLayer = globalManager.audioManager.GetLayerByType (AudioLayerType.Music);
				audioLayer.outroSpeed = 1 / 3.5f;
				audioLayer.FadeOut ();
			}
		}
		// ���� ����� �� �������.
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

			// ��������� ��������� ��������.
			GlobalManager.instance.screenFader.speed = .5f;
			GlobalManager.instance.screenFader.StartFadeIn (OnFadedIn);
		}
	}

	// ���������� ����� ����, ��� ����� ���� � ������.
	private void OnFadedToBlack ()
	{
		GlobalManager globalManager = GlobalManager.instance;

		// ����������� ������.
		globalManager.storyState = StoryState.FirstDays;
		_state = BedState.Sleeping;

		// �������� ��������� � ������ �����.
		CharController charController = globalManager.alexCamera.transform.root.gameObject.GetComponent<CharController> ();
		charController.transform.position = startPointAfterSleep.transform.position;
		Vector3 targetAngles = startPointAfterSleep.transform.rotation.eulerAngles;
		charController.yawDegrees = targetAngles.y;
		charController.pitchDegrees = targetAngles.x;
	}

	// ���������� ����� ����, ��� �������� ���������.
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
