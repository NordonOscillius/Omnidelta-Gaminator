using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Каждая сцена должна содержать объект с именем "Canvas" (с соответствующим компонентом), в который добавлены подобъекты с компонентами Image и следующими именами:
/// 1) "Open Icon",
/// 2) "Read Icon",
/// 3) "Screen Fader".
/// Каждая сцена должна содержать камеру с именем "Alex Camera".
/// </summary>
public class GlobalManager : MonoBehaviour
{
	private static GlobalManager _instance;
	private AudioManager _audioManager;
	private GameObject _audioSourcesGO;
	private Image _blackScreen;
	private ScreenFader _screenFader;
	private InteractionManager _interactionManager;

	private StoryState _storyState = StoryState.Arrival;

	public float maxInteractionDistance = 1.5f;
	public string interactiveLayerName = "Interactive";

	[Tooltip ("Камера, используемая для взаимодействия с интерактивными объектами. Назначается автоматически при загрузке сцены.")]
	public Camera alexCamera;
	[Tooltip ("Иконка 'Открыть', которая используется при наведении курсора на двери. Назначается автоматически при загрузке сцены.")]
	public Image openIcon;
	[Tooltip ("Иконка 'Прочитать', которая используется при наведении курсора на двери. Назначается автоматически при загрузке сцены.")]
	public Image readIcon;
	[Tooltip ("Иконка 'Поспать', которая используется при наведении курсора на кровать. Назначается автоматически при загрузке сцены.")]
	public Image sleepIcon;
	[Tooltip ("Иконка 'Сорвать', которая используется при наведении курсора на цветок солгарда. Назначается автоматически при загрузке сцены.")]
	public Image pickFlowerIcon;
	[Tooltip ("Иконка 'Статуэтка Пересечения', которая используется при наведении курсора на статуэтку. Назначается автоматически при загрузке сцены.")]
	public Image statuetteIcon;
	[Tooltip ("Иконка 'Посох Забвения', которая используется при наведении курсора на посох. Назначается автоматически при загрузке сцены.")]
	public Image staffIcon;
	[Tooltip ("Иконка 'Шестеренка', которая используется при наведении курсора на шестеренку. Назначается автоматически при загрузке сцены.")]
	public Image gearIcon;
	[Tooltip ("Иконка 'Прикрутить шестеренку', которая используется при наведении курсора на почтовый ящик. Назначается автоматически при загрузке сцены.")]
	public Image screwGearIcon;
	[Tooltip ("Иконка 'Отправить солгард', которая используется при наведении курсора на почтовый ящик. Назначается автоматически при загрузке сцены.")]
	public Image sendSolguardIcon;

	private string _alexCameraName = "Alex Camera";
	//private string _alexCameraName = "PlayerCamera";
	private string _openIconName = "Open Icon";
	private string _readIconName = "Read Icon";
	private string _sleepIconName = "Sleep Icon";
	private string _pickFlowerIconName = "Pick Flower Icon";
	private string _statuetteIconName = "Statuette Icon";
	private string _staffIconName = "Staff Icon";
	private string _gearIconName = "Gear Icon";
	private string _screwGearIconName = "Screw Gear Icon";
	private string _sendSolguardIconName = "Send Solguard Icon";

	// Название игрового объекта, положение которого примет персонаж при загрузке очередной сцены. Если null или пустая строка, то персонаж окажется в той же точке, где он находится в редакторе. Если имя задано, другие скрипты могут использовать его для поиска пустого объекта и размещать персонажа там же. Предполагаемое использование такое: дверь назначает это свойство, а логика сцены - сбрасывает в методе Start().
	private string _playerStartPointName = "";
	// Список имен игровых объектов, представляющих солгард, который уже был собран. При загрузке сцены объекты с этими именами должны уничтожаться.
	private List<string> _collectedSolguardNames = new List<string> ();
	// Если false, то речь про пропавшего Мартина произносить уже не нужно.
	private bool _martinIsMissingSpeechIsNeeded = true;

	// Прочитана ли страница из книги про невидимые чернила.
	private bool _invisibleInkBookIsRead = false;
	// Прочитана ли непроявленная карта Лабиринта Библиотеки.
	private bool _libraryMapInvisibleIsRead = false;
	// Прочитана ли проявленная карта Лабиринта Библиотеки.
	private bool _libraryMapVisibleIsRead = false;
	// Прочитана ли страница из книги про Библиотеку.
	private bool _libraryBookIsRead = false;
	// Прочитана ли Книга Альфы про уничтожение ячейки.
	private bool _alphaBookIsRead = false;

	// Выполняется ли в текущий момент "переход к разговору об аквамаринах". Переход начинается в момент запуска глобального монолога Аиды "Какая ирония" и длится вплоть до Fade To Black и загрузки новой сцены (там уже переключится сюжетный стейт). Это свойство я ввожу, чтобы избежать введения дополнительного сюжетного стейта (это может привести к куче ошибок в разных местах, под конец не хотелось бы все похерить).
	private bool _isTransitioningToAquamarineTalk = false;

	// Чувствительность мыши.
	private float _mouseSensitivity = 5f;
	// Дальность прорисовки травы.
	private float _grassDistance = 130f;
	// Плотность травы.
	private float _grassDensity = 1f;


	public GlobalManager ()
	{
		//_audioManager = new AudioManager (this);
		_screenFader = new ScreenFader ();
		_interactionManager = new InteractionManager ();

		// =================================================
		// ======== Здесь можно установить _storyState, ====
		// ======== потому что в Bootstrap'е ===============
		// ======== он не устанавливается ==================
		// =================================================

		// В Bootstrap по умолчанию сцена GaminatorLogo.

		//_storyState = StoryState.FirstDays;
		//_playerStartPointName = "Start Point (1)";

		//_storyState = StoryState.FirstEncounter;
		//_playerStartPointName = "Start Point (3)";

		//_storyState = StoryState.SolguardHike;
		//_playerStartPointName = "Start Point (1)";
		//for (int i = 0; i < 19; i++)
		//	_collectedSolguardNames.Add ("Sol_" + i.ToString ());

		//_storyState = StoryState.WaitForSolguard;
		//_playerStartPointName = "Start Point (2)";

		//_storyState = StoryState.GoForGear;
		//_playerStartPointName = "Start Point (1)";

		//_storyState = StoryState.ConversationBeforeLibrary;
		//_playerStartPointName = "Start Point (3)";

		//// Scene: Library_a
		//_storyState = StoryState.InsideLibrary;
		//_libraryMapVisibleIsRead = true;
	}

	private void Awake ()
	{
		_audioManager = new AudioManager (this);

		if (_instance == null)
		{
			_instance = this;
		}
		else if (_instance != this)
		{
			Destroy (gameObject);
		}

		DontDestroyOnLoad (gameObject);

		// Ищем объект с именем "Audio Sources".
		_audioSourcesGO = GameObject.Find ("Audio Sources");
		if (_audioSourcesGO == null)
			throw new Exception ("Audio Sources game object not found.");

		// Debug.
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnSceneLoaded (Scene scene, LoadSceneMode mode)
	{
		//Debug.Log ("Scene Loaded: " + scene.name + ", mode: " + mode);

		// Ищем камеру игрока, кэшируем в свойство.
		GameObject cameraGO = GameObject.Find (_alexCameraName);
		Camera camera = cameraGO.GetComponent<Camera> ();
		if (camera == null)
			throw new Exception ("Camera component not found on the GameObject.");

		alexCamera = camera;

		// Ищем Canvas и все необходимые иконки. Запоминаем результаты в полях.
		GameObject canvasGO = GameObject.Find ("Canvas");
		if (canvasGO == null)
			throw new Exception ("GameObject named 'Canvas' not found.");

		Canvas canvas = canvasGO.GetComponent<Canvas> ();

		int numChildren = canvasGO.transform.childCount;
		for (int i = 0; i < numChildren; i++)
		{
			GameObject curChild = canvasGO.transform.GetChild (i).gameObject;
			Image curImage = curChild.GetComponent<Image> ();
			if (curImage == null)
				continue;

			if (curChild.name == _openIconName)
				openIcon = curImage;
			else if (curChild.name == _readIconName)
				readIcon = curImage;
			else if (curChild.name == _sleepIconName)
				sleepIcon = curImage;
			else if (curChild.name == _pickFlowerIconName)
				pickFlowerIcon = curImage;
			else if (curChild.name == _statuetteIconName)
				statuetteIcon = curImage;
			else if (curChild.name == _staffIconName)
				staffIcon = curImage;
			else if (curChild.name == _gearIconName)
				gearIcon = curImage;
			else if (curChild.name == _screwGearIconName)
				screwGearIcon = curImage;
			else if (curChild.name == _sendSolguardIconName)
				sendSolguardIcon = curImage;
		}

		// Ищем объект с именем Black Screen, который должен быть внутри Canvas'а, и его компонент Image.
		GameObject blackScreenGO = GameObject.Find ("Black Screen");
		if (blackScreenGO == null)
			throw new Exception ("GameObject named 'Black Screen' not found.");

		_blackScreen = blackScreenGO.GetComponent<Image> ();
		if (_blackScreen == null)
			throw new Exception ("The Black Screen image not found.");
	}

	private void Update ()
	{
		_audioManager.Update (Time.deltaTime);
		_screenFader.Update (Time.deltaTime);
		_interactionManager.Update ();
	}

	private void OnDisable ()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	/// <summary>
	/// Вызывать из скрипта на интерактивном объекте, когда игрок на него нацеливается.
	/// </summary>
	/// <param name="interactionType"></param>
	public void ShowInteractionIcon (InteractionType interactionType)
	{
		Image usedIcon = GetIconByInteractionType (interactionType);
		if (usedIcon == null)
			throw new Exception ("Иконка не назначена.");

		usedIcon.gameObject.SetActive (true);
	}

	/// <summary>
	/// Вызывать из скрипта на интерактивном объекте, когда игрок на него больше не нацеливается.
	/// </summary>
	/// <param name="interactionType"></param>
	public void HideInteractionIcon (InteractionType interactionType)
	{
		Image usedIcon = GetIconByInteractionType (interactionType);
		if (usedIcon == null)
			throw new Exception ("Иконка не назначена.");

		usedIcon.gameObject.SetActive (false);
	}

	public void HideAllInteractionIcons ()
	{
		openIcon.gameObject.SetActive (false);
		readIcon.gameObject.SetActive (false);
		sleepIcon.gameObject.SetActive (false);
		pickFlowerIcon.gameObject.SetActive (false);
		statuetteIcon.gameObject.SetActive (false);
		staffIcon.gameObject.SetActive (false);
		gearIcon.gameObject.SetActive (false);
		screwGearIcon.gameObject.SetActive (false);
		sendSolguardIcon.gameObject.SetActive (false);
		// ...
	}

	/// <summary>
	/// [Deprecated] Возвращает иконку, соответствующую типу действия.
	/// </summary>
	/// <param name="interactionType"></param>
	/// <returns></returns>
	private Image GetIconByInteractionType (InteractionType interactionType)
	{
		switch (interactionType)
		{
			case InteractionType.Open:
				return openIcon;
			case InteractionType.Read:
				return readIcon;
			case InteractionType.Sleep:
				return sleepIcon;
			case InteractionType.PickFlower:
				return pickFlowerIcon;
			case InteractionType.StatuetteOfIntersection:
				return statuetteIcon;
			case InteractionType.StaffOfOblivion:
				return staffIcon;
			case InteractionType.Gear:
				return gearIcon;
			case InteractionType.ScrewGear:
				return screwGearIcon;
			case InteractionType.SendSolguard:
				return sendSolguardIcon;
		}
		throw new Exception ("Unhandled interactionType.");
	}

	private string GetIconNameByInteractionType (InteractionType interactionType)
	{
		switch (interactionType)
		{
			case InteractionType.Open:
				return _openIconName;
			case InteractionType.Read:
				return _readIconName;
			case InteractionType.Sleep:
				return _sleepIconName;
			case InteractionType.PickFlower:
				return _pickFlowerIconName;
			case InteractionType.StatuetteOfIntersection:
				return _statuetteIconName;
			case InteractionType.StaffOfOblivion:
				return _staffIconName;
			case InteractionType.Gear:
				return _gearIconName;
			case InteractionType.ScrewGear:
				return _screwGearIconName;
			case InteractionType.SendSolguard:
				return _sendSolguardIconName;
		}
		throw new Exception ("Unhandled interactionType.");
	}


	// =================== SCREEN FADER ===================

	/// <summary>
	/// [Deprecated] Использовать в виде MonoBehavior.StartCoroutine (ScreenFade (doFadeToBlack, speed)). Делает Fade To Black или наоборот.
	/// </summary>
	/// <param name="doFadeToBlack">Если true, альфа затемняющего спрайта будет увеличиваться.</param>
	/// <param name="speed">Скорость перехода в черный или из черного.</param>
	/// <returns></returns>
	public IEnumerator ScreenFade (bool doFadeToBlack, float speed)
	{
		Color curColor = _blackScreen.color;
		float newAlpha;

		if (doFadeToBlack)
		{
			while (_blackScreen.color.a < 1)
			{
				newAlpha = curColor.a + speed * Time.deltaTime;
				_blackScreen.color = new Color (curColor.r, curColor.g, curColor.b, newAlpha);
				yield return null;
			}
		}
		else
		{
			while (_blackScreen.color.a > 0)
			{
				newAlpha = curColor.a - speed * Time.deltaTime;
				_blackScreen.color = new Color (curColor.r, curColor.g, curColor.b, newAlpha);
				yield return null;
			}
		}
	}


	// ====================== BOOKS =======================

	public void OnBookHasBeenRead (BookType bookType)
	{
		switch (bookType)
		{
			case BookType.Common:
				return;
			case BookType.InvisibleInkBook:
				_invisibleInkBookIsRead = true; return;
			case BookType.LibraryBook:
				_libraryBookIsRead = true; return;
			case BookType.LibraryMapInvisible:
				_libraryMapInvisibleIsRead = true; return;
			case BookType.LibraryMapVisible:
				_libraryMapVisibleIsRead = true; return;
			case BookType.AlphaBook:
				_alphaBookIsRead = true; return;
		}
	}


	// ====================================================
	// ==================== PROPERTIES ====================
	// ====================================================

	public static GlobalManager instance { get { return _instance; } }

	public AudioManager audioManager { get { return _audioManager; } }

	public GameObject audioSourcesGO { get { return _audioSourcesGO; } }

	public Image blackScreen { get { return _blackScreen; } }

	public ScreenFader screenFader { get { return _screenFader; } }

	public StoryState storyState
	{
		get { return _storyState; }
		set
		{
			_storyState = value;
		}
	}

	public InteractionManager interactionManager { get { return _interactionManager; } }

	public string playerStartPointName
	{
		get { return _playerStartPointName; }
		set { _playerStartPointName = value; }
	}

	/// <summary>
	/// Список имен игровых объектов, представляющих солгард, который уже был собран. Возвращается по ссылке.
	/// </summary>
	public List<string> collectedSolguardNames
	{
		get { return _collectedSolguardNames; }
	}

	public bool martinIsMissingSpeechIsNeeded
	{
		get { return _martinIsMissingSpeechIsNeeded; }
		set { _martinIsMissingSpeechIsNeeded = value; }
	}

	/// <summary>
	/// Прочитана ли страница из книги про невидимые чернила.
	/// </summary>
	public bool invisibleInkBookIsRead { get { return _invisibleInkBookIsRead; } }

	/// <summary>
	/// Прочитана ли непроявленная карта Лабиринта Библиотеки.
	/// </summary>
	public bool libraryMapInvisibleIsRead { get { return _libraryMapInvisibleIsRead; } }

	/// <summary>
	/// Прочитана ли проявленная карта Лабиринта Библиотеки.
	/// </summary>
	public bool libraryMapVisibleIsRead { get { return _libraryMapVisibleIsRead; } }

	/// <summary>
	/// Прочитана ли страница из книги про Библиотеку.
	/// </summary>
	public bool libraryBookIsRead { get { return _libraryBookIsRead; } }

	/// <summary>
	/// Прочитана ли Книга Альфы.
	/// </summary>
	public bool alphaBookIsRead { get { return _alphaBookIsRead; } }

	/// <summary>
	/// Выполняется ли в текущий момент "переход к разговору об аквамаринах". Переход начинается в момент запуска глобального монолога Аиды "Какая ирония" и длится вплоть до Fade To Black и загрузки новой сцены (там уже переключится сюжетный стейт). Это свойство я ввожу, чтобы избежать введения дополнительного сюжетного стейта (это может привести к куче ошибок в разных местах, под конец не хотелось бы все похерить).
	/// </summary>
	public bool isTransitioningToAquamarineTalk {
		get { return _isTransitioningToAquamarineTalk; }
		set { _isTransitioningToAquamarineTalk = value; }
	}

	public float mouseSensitivity
	{
		get { return _mouseSensitivity; }
		set { _mouseSensitivity = value; }
	}

	public float grassDistance
	{
		get { return _grassDistance; }
		set { _grassDistance = value; }
	}

	public float grassDensity
	{
		get { return _grassDensity; }
		set { _grassDensity = value; }
	}

}
