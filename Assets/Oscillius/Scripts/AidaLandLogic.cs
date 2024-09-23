using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AidaLandLogic : MonoBehaviour
{
	[Tooltip ("�������� �����, �������������� ��� ������.")]
	public string alexHouseSceneName;

	// ������������ ������ �� ������, �������������� Start Point.
	private GameObject _startPointGO;

	private float _libraryHikeFadeInDelay = 4f;
	// ��������� ����� "������, �����".
	private float _sorryAlexLoudness = .8f;
	// �������� ����� ������ ����� "������, �����" (��� ��������� ����� � ������ LibraryHikeAfterSorryAlex) � ������� ����� "����� ����� ���".
	private float _whenRevengePhraseDelay = 8f;
	// ��������� ����� "����� �����".
	private float _whenRevengeLoudness = .8f;
	// ����� �� �������� ����� �� ������� ���� "������� � ��������� �� ����������", ������� ���������� ������ � �������� �������� "����� ������".
	private float _timeToTransitionToAquamarineTalk = 1.5f;
	// ��������� �������� "����� ������".
	private float _speechIronyLoudness = .8f;
	// ������� ������� ������ ����� "����� ������", �� ������� ���������� ���������� ������ (�� ��� �� �������� ����� �����).
	private float _speechIronyFTBTime = 13.7f;


	private void Awake ()
	{
		CacheStartPointGO ();
	}

	private void Start ()
	{
		StoryState storyState = GlobalManager.instance.storyState;

		if (storyState == StoryState.LibraryHike)
			StartOnLibraryHike ();
		else if (storyState == StoryState.LibraryHikeAfterSorryAlex)
			StartOnLibraryHikeAfterSorryAlex ();
		else if (storyState == StoryState.LibraryHikeAfterWhenRevenge)
			StartOnLibraryHikeAfterWhenRevenge ();
		else if (storyState == StoryState.Labyrinth)
			StartOnLabyrinth ();
		else if (storyState == StoryState.InsideLibrary)
			StartOnInsideLibrary ();
	}

	private void StartOnLibraryHike ()
	{
		// ���������� ����� "������, �����".
		AudioLayer speechLayer = GlobalManager.instance.audioManager.GetLayerByType (AudioLayerType.Speech);
		speechLayer.introSpeed = 100f;
		speechLayer.IntroduceClip (OscAudioResourceEnum.Speech_Sorry_Alex, false, _sorryAlexLoudness);

		// �� ����� ��������� ����������� ��������� � ������������.
		CharController charCtrl = GlobalManager.instance.alexCamera.transform.root.GetComponent<CharController> ();
		charCtrl.enableLook = false;
		charCtrl.enableMove = false;
	}

	private void StartOnLibraryHikeAfterSorryAlex ()
	{
		StartDefaultFadeIn ();
		PlaceCharacterAtStartPoint ();
	}

	private void StartOnLibraryHikeAfterWhenRevenge ()
	{
		StartDefaultFadeIn ();
		PlaceCharacterAtStartPoint ();
	}

	private void StartOnLabyrinth ()
	{
		StartDefaultFadeIn ();
		PlaceCharacterAtStartPoint ();
	}

	private void StartOnInsideLibrary ()
	{
		StartDefaultFadeIn ();
		PlaceCharacterAtStartPoint ();
	}

	private void Update ()
	{
		StoryState storyState = GlobalManager.instance.storyState;

		if (storyState == StoryState.LibraryHike)
			UpdateOnLibraryHike ();
		else if (storyState == StoryState.LibraryHikeAfterSorryAlex)
			UpdateOnLibraryHikeAfterSorryAlex ();
		// Empty method.
		else if (storyState == StoryState.LibraryHikeAfterWhenRevenge)
			UpdateOnLibraryHikeAfterWhenRevenge ();
		else if (storyState == StoryState.InsideLibrary)
			UpdateOnInsideLibrary ();
	}

	private void UpdateOnLibraryHike ()
	{
		if (Time.timeSinceLevelLoad >= _libraryHikeFadeInDelay)
		{
			_libraryHikeFadeInDelay = Mathf.Infinity;

			// ��������� �������.
			ScreenFader screenFader = GlobalManager.instance.screenFader;
			screenFader.speed = 1f;
			screenFader.StartFadeIn ();

			// ��������� ���� ��������� � ��������.
			CharController charCtrl = GlobalManager.instance.alexCamera.transform.root.GetComponent<CharController> ();
			charCtrl.enableLook = true;
			charCtrl.enableMove = true;

			// ����������� ������� �����.
			GlobalManager.instance.storyState = StoryState.LibraryHikeAfterSorryAlex;
		}
	}

	private void UpdateOnLibraryHikeAfterSorryAlex ()
	{
		_whenRevengePhraseDelay -= Time.deltaTime;
		if (_whenRevengePhraseDelay < 0)
		{
			GlobalManager.instance.storyState = StoryState.LibraryHikeAfterWhenRevenge;

			// ���������� ����� "����� �����".
			AudioLayer speechLayer = GlobalManager.instance.audioManager.GetLayerByType (AudioLayerType.Speech);
			speechLayer.introSpeed = 100f;
			speechLayer.IntroduceClip (OscAudioResourceEnum.Speech_When_Revenge, false, _whenRevengeLoudness);
		}
	}

	private void UpdateOnLibraryHikeAfterWhenRevenge ()
	{

	}

	private void UpdateOnInsideLibrary ()
	{
		// ���� "������� � ��������� �� ����������" ��� �� �������.
		if (!GlobalManager.instance.isTransitioningToAquamarineTalk)
		{
			// ���� ����� ����� ��� ��������� � ���� �� �����, �� ����������� _timeToTransitionToAquamarineTalk ������ � ��������� ���� �������� � ��������� �� ����������.
			if (GlobalManager.instance.alphaBookIsRead && Time.timeSinceLevelLoad >= _timeToTransitionToAquamarineTalk)
			{
				// ����������� ����������� ������������.
				GlobalManager.instance.isTransitioningToAquamarineTalk = true;
				//_timeToTransitionToAquamarineTalk = Mathf.Infinity;

				AudioLayer speechLayer = GlobalManager.instance.audioManager.GetLayerByType (AudioLayerType.Speech);
				speechLayer.introSpeed = 100f;
				speechLayer.outroSpeed = 100f;
				speechLayer.IntroduceClip (OscAudioResourceEnum.Speech_Irony, false, _speechIronyLoudness);
			} 
		}
		// ���� ������� �������, �.�. ���� ��� �������, �� ���������� ������� ���������� ������ � �������� ����� �����.
		else
		{
			// ���� �� ���� ��� ���� ������ ��� ������, ������, ���� ����������. ������ ����� �����.
			AudioLayer speechLayer = GlobalManager.instance.audioManager.GetLayerByType (AudioLayerType.Speech);
			if (speechLayer.numClips == 0)
			{
				// ����������� ����� �������.
				GlobalManager.instance.storyState = StoryState.AquamarineTalk;
				// ������ ������ � �����.
				GlobalManager.instance.playerStartPointName = "Start Point (3)";
				// ������ �����.
				StartCoroutine (LoadNextScene (alexHouseSceneName));
			}
			// ���� ���� ��� �������, ���� ������� ����������.
			else
			{
				if (speechLayer.GetClipWithResourceType (OscAudioResourceEnum.Speech_Irony).audioSource.time >= _speechIronyFTBTime)
				{
					_speechIronyFTBTime = Mathf.Infinity;

					ScreenFader screenFader = GlobalManager.instance.screenFader;
					screenFader.speed = 100f;
					screenFader.StartFadeToBlack ();
				}
			}
		}
	}

	private IEnumerator LoadNextScene (string nextSceneName)
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync (nextSceneName);

		while (!asyncLoad.isDone)
		{
			yield return null;
		}
	}


	// �������� ������ Start Point.
	private void CacheStartPointGO ()
	{
		string usedStartPointName = GlobalManager.instance.playerStartPointName;
		if (usedStartPointName != null && usedStartPointName != "")
		{
			_startPointGO = GameObject.Find (usedStartPointName);
			if (_startPointGO == null)
				throw new System.Exception ("Start Point with name '" + usedStartPointName + "' not found.");
		}
	}

	// ��������� ��������� � ��������� ��������� �����, ���� ��� ������.
	private void PlaceCharacterAtStartPoint ()
	{
		if (_startPointGO != null)
		{
			CharController charController = GlobalManager.instance.alexCamera.transform.root.gameObject.GetComponent<CharController> ();

			charController.transform.position = _startPointGO.transform.position;
			Vector3 targetAngles = _startPointGO.transform.rotation.eulerAngles;
			charController.yawDegrees = targetAngles.y;
			charController.pitchDegrees = targetAngles.x;
		}
	}

	private void StartDefaultFadeIn ()
	{
		// ������ �������� ������ � ��������� Fade In.
		ScreenFader screenFader = GlobalManager.instance.screenFader;
		screenFader.opacity = 1f;
		screenFader.speed = 1f;
		screenFader.StartFadeIn ();
	}

}
