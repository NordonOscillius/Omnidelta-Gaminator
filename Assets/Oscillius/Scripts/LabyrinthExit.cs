using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LabyrinthExit : MonoBehaviour
{
	[Tooltip ("�������� �����, �������������� ����� ������������ ���������� � ������ �����.")]
	public string librarySceneName;

	private Collider _collider;
	private Interactive _interactive;
	// ���� true, ����� ������ ����� �������.
	private bool _preventInteractions = false;


	private void Awake ()
	{
		_collider = GetComponent<Collider> ();
		if (_collider == null)
			throw new System.Exception ("Collider not found.");

		_interactive = GetComponent<Interactive> ();
		if (_interactive == null)
			throw new System.Exception ("Interactive component not found.");

		this.preventInteractions = _preventInteractions;
	}

	private void Update ()
	{
		// ���� ����� ������ �������, �������.
		if (_preventInteractions)
			return;

		GlobalManager globalManager = GlobalManager.instance;
		InteractionManager interactionManager = globalManager.interactionManager;

		if (interactionManager.intersectionFound && interactionManager.hitInfo.collider == _collider)
		{
			if (Input.GetKeyDown (interactionManager.interactKeyCode))
			{
				ScreenFader screenFader = GlobalManager.instance.screenFader;
				screenFader.speed = 1f;
				screenFader.StartFadeToBlack (OnFadedOut);

				preventInteractions = true;

				PlayDoorEffect ();
			}
		}
	}

	private void OnFadedOut ()
	{
		// ����������� ����� �������.
		GlobalManager.instance.storyState = StoryState.InsideLibrary;

		StartCoroutine (LoadNextScene ());
	}

	private IEnumerator LoadNextScene ()
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync (librarySceneName);

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

	public bool preventInteractions
	{
		get { return _preventInteractions; }
		set
		{
			_preventInteractions = value;
			_interactive.doShowIcon = !value;
		}
	}

}
