using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// �� ������ ��������� ������� ���� "Interactive".
/// </summary>
public class AlexDoorScript : MonoBehaviour
{
	private GlobalManager _globalManager;

	[Tooltip ("�������� �����, ������� ����� ��������� ��� ����� � ��� �����.")]
	public string nextSceneName;
	[Tooltip ("�������� ������� �������� �������, � ��������� �������� ����� ��������� �������� ��� �������� ��������� �����.")]
	public string nextStartPointName;
	[Tooltip ("���� true, �� ������ ������� �� ����� ������ ����; ����� - �� ����� �������.")]
	public bool isIndoor;

	private AlexDoorState _state = AlexDoorState.Waiting;
	// �����, � ������� �������� �������� ����� ������ ����� ����� �����������.
	private float _fadeOutSpeed = 2f;
	private float _soundFadeOutSpeed = 1f;

	private bool _preventInteractions;

	// ����-���������, ����������� � ���� �� �������� �������, ��� � ���� ������.
	private BoxCollider _myCollider;
	// ��������� Interactive, ����������� ��� ������ Interaction Manager'�.
	private Interactive _myInteractive;
	// ������� �� ����� � ���� ������ � ���������� �������.
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
			throw new Exception ("����-��������� �� ������.");
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
				// ��������� ������� � Fade Out � �������� ��������� �����, ������ ���� �� ��� �� �������.
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

		// ���� ����� ������� � ���� ������.
		if (intersectionFound && hitInfo.collider == _myCollider)
		{
			// ���������� ������ "�������", ���� �����.
			if (!_prevInteractionIsPossible)
			{
				_globalManager.ShowInteractionIcon (InteractionType.Open);
			}

			// ��������� ������� � Fade Out � �������� ��������� �����, ������ ���� �� ��� �� �������.
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
		// ����� �� �������.
		else
		{
			if (_prevInteractionIsPossible)
				_globalManager.HideInteractionIcon (InteractionType.Open);
		}

		// ���������� prev-����.
		_prevInteractionIsPossible = intersectionFound;
	}

	private void OnFadedOut ()
	{
		_state = AlexDoorState.Entered;
		StartCoroutine (LoadHouseScene ());
	}

	/// <summary>
	/// ���������� ��������� �����.
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

	// ���� true, ��������������� ����� ����� ���������.
	public bool preventInteractions
	{
		get { return _preventInteractions; }
		set {
			_preventInteractions = value;
			_myInteractive.doShowIcon = !value;
		}
	}

}
