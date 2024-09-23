using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AidaLandLogic : MonoBehaviour
{
	[Tooltip ("Название сцены, представляющей дом Алекса.")]
	public string alexHouseSceneName;

	// Кэшированная ссылка на объект, представляющий Start Point.
	private GameObject _startPointGO;

	private float _libraryHikeFadeInDelay = 4f;
	// Громкость фразы "Прости, Алекс".
	private float _sorryAlexLoudness = .8f;
	// Задержка между концом фразы "Прости, Алекс" (или загрузкой сцены в стейте LibraryHikeAfterSorryAlex) и началом фразы "Когда месть это".
	private float _whenRevengePhraseDelay = 8f;
	// Громкость фразы "Когда месть".
	private float _whenRevengeLoudness = .8f;
	// Время от загрузки сцены до запуска фазы "переход к разговору об аквамарине", которая начинается вместе с запуском монолога "Какая ирония".
	private float _timeToTransitionToAquamarineTalk = 1.5f;
	// Громкость монолога "Какая ирония".
	private float _speechIronyLoudness = .8f;
	// Отметка времени внутри клипа "Какая ирония", на которой происходит затемнение экрана (но еще не загрузка новой сцены).
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
		// Произносим фразу "Прости, Алекс".
		AudioLayer speechLayer = GlobalManager.instance.audioManager.GetLayerByType (AudioLayerType.Speech);
		speechLayer.introSpeed = 100f;
		speechLayer.IntroduceClip (OscAudioResourceEnum.Speech_Sorry_Alex, false, _sorryAlexLoudness);

		// На время отключаем возможность двигаться и оглядываться.
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

			// Разгоняем темноту.
			ScreenFader screenFader = GlobalManager.instance.screenFader;
			screenFader.speed = 1f;
			screenFader.StartFadeIn ();

			// Позволяем Аиде двигаться и смотреть.
			CharController charCtrl = GlobalManager.instance.alexCamera.transform.root.GetComponent<CharController> ();
			charCtrl.enableLook = true;
			charCtrl.enableMove = true;

			// Переключаем игровой стейт.
			GlobalManager.instance.storyState = StoryState.LibraryHikeAfterSorryAlex;
		}
	}

	private void UpdateOnLibraryHikeAfterSorryAlex ()
	{
		_whenRevengePhraseDelay -= Time.deltaTime;
		if (_whenRevengePhraseDelay < 0)
		{
			GlobalManager.instance.storyState = StoryState.LibraryHikeAfterWhenRevenge;

			// Произносим фразу "Когда месть".
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
		// Если "переход к разговору об аквамарине" еще не запущен.
		if (!GlobalManager.instance.isTransitioningToAquamarineTalk)
		{
			// Если Книга Альфы уже прочитана и Аида на улице, то выдерживаем _timeToTransitionToAquamarineTalk секунд и запускаем фазу перехода к разговору об аквамарине.
			if (GlobalManager.instance.alphaBookIsRead && Time.timeSinceLevelLoad >= _timeToTransitionToAquamarineTalk)
			{
				// Гарантируем однократное срабатывание.
				GlobalManager.instance.isTransitioningToAquamarineTalk = true;
				//_timeToTransitionToAquamarineTalk = Mathf.Infinity;

				AudioLayer speechLayer = GlobalManager.instance.audioManager.GetLayerByType (AudioLayerType.Speech);
				speechLayer.introSpeed = 100f;
				speechLayer.outroSpeed = 100f;
				speechLayer.IntroduceClip (OscAudioResourceEnum.Speech_Irony, false, _speechIronyLoudness);
			} 
		}
		// Если переход запущен, т.е. Аида уже говорит, то дожидаемся времени затемнения экрана и загрузки новой сцены.
		else
		{
			// Если на слое для речи больше нет клипов, значит, Аида договорила. Грузим новую сцену.
			AudioLayer speechLayer = GlobalManager.instance.audioManager.GetLayerByType (AudioLayerType.Speech);
			if (speechLayer.numClips == 0)
			{
				// Переключаем стейт истории.
				GlobalManager.instance.storyState = StoryState.AquamarineTalk;
				// Ставим Алекса у стены.
				GlobalManager.instance.playerStartPointName = "Start Point (3)";
				// Грузим сцену.
				StartCoroutine (LoadNextScene (alexHouseSceneName));
			}
			// Если Аида еще говорит, ждем момента затемнения.
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


	// Кэширует объект Start Point.
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

	// Размещает персонажа в положении начальной точки, если она задана.
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
		// Делаем картинку черной и запускаем Fade In.
		ScreenFader screenFader = GlobalManager.instance.screenFader;
		screenFader.opacity = 1f;
		screenFader.speed = 1f;
		screenFader.StartFadeIn ();
	}

}
