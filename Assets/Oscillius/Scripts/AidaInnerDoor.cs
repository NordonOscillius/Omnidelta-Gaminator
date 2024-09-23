using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AidaInnerDoor : MonoBehaviour
{
	[Tooltip ("������� ������, �������� ��������� ��� ����� ���� '� ������ ���������'.")]
	public GameObject iHaveToWaitSpeechGO;
	[Tooltip ("�������� �����, ������� ������ ���� ��������� ��� ������ �� �����.")]
	public string nextSceneName;
	[Tooltip ("�������� �������, ��������������� Start Point ��������� �����, � ������� ������ ��������� ��������.")]
	public string nextStartPointName;

	private Collider _collider;
	private Interactive _interactive;
	// ���� true, ����� ������ ����� �������.
	private bool _preventInteractions = false;
	private AudioSource _iHaveToWaitAudioSource;
	// True, ���� ����� "� ������ ��������� ������" ��� ������������� (��� ������������ � ������� ������).
	private bool _iHaveToWaitIsPronounced = false;


	private void Awake ()
	{
		_collider = GetComponent<Collider> ();
		if (_collider == null)
			throw new System.Exception ("Collider not found.");

		_interactive = GetComponent<Interactive> ();
		if (_interactive == null)
			throw new System.Exception ("Interactive component not found.");
		this.preventInteractions = _preventInteractions;

		_iHaveToWaitAudioSource = iHaveToWaitSpeechGO.GetComponent<AudioSource> ();
	}


	private void Update ()
	{
		StoryState storyState = GlobalManager.instance.storyState;

		if (storyState == StoryState.WaitForSolguard)
			UpdateOnWaitForSolguard ();
		//else if (storyState == StoryState.SolguardHike)
		//	UpdateOnSolguardHike ();
		else if (
			storyState == StoryState.LibraryHike ||
			storyState == StoryState.LibraryHikeAfterSorryAlex ||
			storyState == StoryState.LibraryHikeAfterWhenRevenge ||
			storyState == StoryState.Labyrinth
		)
			UpdateOnLibraryHikeCommon ();
	}

	private void UpdateOnWaitForSolguard ()
	{
		// ���� ����� ������ �������, �������.
		if (_preventInteractions)
			return;

		GlobalManager globalManager = GlobalManager.instance;
		InteractionManager interactionManager = globalManager.interactionManager;

		if (interactionManager.intersectionFound && interactionManager.hitInfo.collider == _collider)
		{
			// ���� ����� "� ������ ��� ���������" ��� �� �������������.
			if (!_iHaveToWaitIsPronounced)
			{
				// ���� ���� �������� �����, ���������� ����� "� ������ ��������� ��� �����".
				if (Input.GetKeyDown (interactionManager.interactKeyCode))
				{
					if (!_iHaveToWaitAudioSource.isPlaying)
					{
						_iHaveToWaitAudioSource.Play ();

						// ����� ������� �����, �����, ������. ��� ����� ��������� ��� �� ��������� ���� � �� ��.
						_iHaveToWaitIsPronounced = true;
						this.preventInteractions = true;
					}
				}
			}
		}
	}

	private void UpdateOnLibraryHikeCommon ()
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
				this.preventInteractions = true;

				ScreenFader screenFader = globalManager.screenFader;
				screenFader.speed = 2f;
				screenFader.StartFadeToBlack (OnFadedOut);

				PlayDoorEffect ();
			}
		}
	}

	private void OnFadedOut ()
	{
		GlobalManager.instance.playerStartPointName = nextStartPointName;

		StartCoroutine (LoadNextScene ());
	}

	private IEnumerator LoadNextScene ()
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
		set {
			_preventInteractions = value;
			_interactive.doShowIcon = !value;
		}
	}

	public bool iHaveToWaitIsPronounced { get { return _iHaveToWaitIsPronounced; } }

}
