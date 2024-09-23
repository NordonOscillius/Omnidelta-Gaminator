using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Дверь, ведущая из локации Аиды в Лабиринт.
/// </summary>
public class LabyrinthDoor : MonoBehaviour
{
	[Tooltip ("Название сцены истинного Лабиринта Зайтаку.")]
	public string labyrinthSceneName;
	[Tooltip ("Название сцены ложного Лабиринта Зайтаку.")]
	public string labyrinthFalseSceneName;

	private Collider _collider;
	private Interactive _interactive;
	// Если true, дверь нельзя будет открыть.
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
		StoryState storyState = GlobalManager.instance.storyState;

		if (storyState == StoryState.LibraryHike ||
			storyState == StoryState.LibraryHikeAfterSorryAlex ||
			storyState == StoryState.LibraryHikeAfterWhenRevenge ||
			storyState == StoryState.Labyrinth ||
			storyState == StoryState.InsideLibrary
		)
		{
			UpdateOnLibraryHikeCommon ();
		}
	}

	private void UpdateOnLibraryHikeCommon ()
	{
		// Если дверь нельзя открыть, выходим.
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
		// Переключаем стейт истории.
		GlobalManager.instance.storyState = StoryState.Labyrinth;

		string nextSceneName = "";
		if (GlobalManager.instance.libraryMapVisibleIsRead)
		{
			nextSceneName = labyrinthSceneName;
			GlobalManager.instance.playerStartPointName = "Start Point (1)";
		}
		else
		{
			nextSceneName = labyrinthFalseSceneName;
		}

		StartCoroutine (LoadNextScene (nextSceneName));
	}

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
