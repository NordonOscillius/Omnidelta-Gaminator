using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ������ ����� ������ ��������� ������ � ������ "Canvas" (� ��������������� �����������), � ������� ��������� ���������� � ������������ Image � ���������� �������:
/// 1) "Open Icon",
/// 2) "Read Icon",
/// 3) "Screen Fader".
/// ������ ����� ������ ��������� ������ � ������ "Alex Camera".
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

	[Tooltip ("������, ������������ ��� �������������� � �������������� ���������. ����������� ������������� ��� �������� �����.")]
	public Camera alexCamera;
	[Tooltip ("������ '�������', ������� ������������ ��� ��������� ������� �� �����. ����������� ������������� ��� �������� �����.")]
	public Image openIcon;
	[Tooltip ("������ '���������', ������� ������������ ��� ��������� ������� �� �����. ����������� ������������� ��� �������� �����.")]
	public Image readIcon;
	[Tooltip ("������ '�������', ������� ������������ ��� ��������� ������� �� �������. ����������� ������������� ��� �������� �����.")]
	public Image sleepIcon;
	[Tooltip ("������ '�������', ������� ������������ ��� ��������� ������� �� ������ ��������. ����������� ������������� ��� �������� �����.")]
	public Image pickFlowerIcon;
	[Tooltip ("������ '��������� �����������', ������� ������������ ��� ��������� ������� �� ���������. ����������� ������������� ��� �������� �����.")]
	public Image statuetteIcon;
	[Tooltip ("������ '����� ��������', ������� ������������ ��� ��������� ������� �� �����. ����������� ������������� ��� �������� �����.")]
	public Image staffIcon;
	[Tooltip ("������ '����������', ������� ������������ ��� ��������� ������� �� ����������. ����������� ������������� ��� �������� �����.")]
	public Image gearIcon;
	[Tooltip ("������ '���������� ����������', ������� ������������ ��� ��������� ������� �� �������� ����. ����������� ������������� ��� �������� �����.")]
	public Image screwGearIcon;
	[Tooltip ("������ '��������� �������', ������� ������������ ��� ��������� ������� �� �������� ����. ����������� ������������� ��� �������� �����.")]
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

	// �������� �������� �������, ��������� �������� ������ �������� ��� �������� ��������� �����. ���� null ��� ������ ������, �� �������� �������� � ��� �� �����, ��� �� ��������� � ���������. ���� ��� ������, ������ ������� ����� ������������ ��� ��� ������ ������� ������� � ��������� ��������� ��� ��. �������������� ������������� �����: ����� ��������� ��� ��������, � ������ ����� - ���������� � ������ Start().
	private string _playerStartPointName = "";
	// ������ ���� ������� ��������, �������������� �������, ������� ��� ��� ������. ��� �������� ����� ������� � ����� ������� ������ ������������.
	private List<string> _collectedSolguardNames = new List<string> ();
	// ���� false, �� ���� ��� ���������� ������� ����������� ��� �� �����.
	private bool _martinIsMissingSpeechIsNeeded = true;

	// ��������� �� �������� �� ����� ��� ��������� �������.
	private bool _invisibleInkBookIsRead = false;
	// ��������� �� ������������� ����� ��������� ����������.
	private bool _libraryMapInvisibleIsRead = false;
	// ��������� �� ����������� ����� ��������� ����������.
	private bool _libraryMapVisibleIsRead = false;
	// ��������� �� �������� �� ����� ��� ����������.
	private bool _libraryBookIsRead = false;
	// ��������� �� ����� ����� ��� ����������� ������.
	private bool _alphaBookIsRead = false;

	// ����������� �� � ������� ������ "������� � ��������� �� �����������". ������� ���������� � ������ ������� ����������� �������� ���� "����� ������" � ������ ������ �� Fade To Black � �������� ����� ����� (��� ��� ������������ �������� �����). ��� �������� � �����, ����� �������� �������� ��������������� ��������� ������ (��� ����� �������� � ���� ������ � ������ ������, ��� ����� �� �������� �� ��� ��������).
	private bool _isTransitioningToAquamarineTalk = false;

	// ���������������� ����.
	private float _mouseSensitivity = 5f;
	// ��������� ���������� �����.
	private float _grassDistance = 130f;
	// ��������� �����.
	private float _grassDensity = 1f;


	public GlobalManager ()
	{
		//_audioManager = new AudioManager (this);
		_screenFader = new ScreenFader ();
		_interactionManager = new InteractionManager ();

		// =================================================
		// ======== ����� ����� ���������� _storyState, ====
		// ======== ������ ��� � Bootstrap'� ===============
		// ======== �� �� ��������������� ==================
		// =================================================

		// � Bootstrap �� ��������� ����� GaminatorLogo.

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

		// ���� ������ � ������ "Audio Sources".
		_audioSourcesGO = GameObject.Find ("Audio Sources");
		if (_audioSourcesGO == null)
			throw new Exception ("Audio Sources game object not found.");

		// Debug.
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnSceneLoaded (Scene scene, LoadSceneMode mode)
	{
		//Debug.Log ("Scene Loaded: " + scene.name + ", mode: " + mode);

		// ���� ������ ������, �������� � ��������.
		GameObject cameraGO = GameObject.Find (_alexCameraName);
		Camera camera = cameraGO.GetComponent<Camera> ();
		if (camera == null)
			throw new Exception ("Camera component not found on the GameObject.");

		alexCamera = camera;

		// ���� Canvas � ��� ����������� ������. ���������� ���������� � �����.
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

		// ���� ������ � ������ Black Screen, ������� ������ ���� ������ Canvas'�, � ��� ��������� Image.
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
	/// �������� �� ������� �� ������������� �������, ����� ����� �� ���� ������������.
	/// </summary>
	/// <param name="interactionType"></param>
	public void ShowInteractionIcon (InteractionType interactionType)
	{
		Image usedIcon = GetIconByInteractionType (interactionType);
		if (usedIcon == null)
			throw new Exception ("������ �� ���������.");

		usedIcon.gameObject.SetActive (true);
	}

	/// <summary>
	/// �������� �� ������� �� ������������� �������, ����� ����� �� ���� ������ �� ������������.
	/// </summary>
	/// <param name="interactionType"></param>
	public void HideInteractionIcon (InteractionType interactionType)
	{
		Image usedIcon = GetIconByInteractionType (interactionType);
		if (usedIcon == null)
			throw new Exception ("������ �� ���������.");

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
	/// [Deprecated] ���������� ������, ��������������� ���� ��������.
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
	/// [Deprecated] ������������ � ���� MonoBehavior.StartCoroutine (ScreenFade (doFadeToBlack, speed)). ������ Fade To Black ��� ��������.
	/// </summary>
	/// <param name="doFadeToBlack">���� true, ����� ������������ ������� ����� �������������.</param>
	/// <param name="speed">�������� �������� � ������ ��� �� �������.</param>
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
	/// ������ ���� ������� ��������, �������������� �������, ������� ��� ��� ������. ������������ �� ������.
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
	/// ��������� �� �������� �� ����� ��� ��������� �������.
	/// </summary>
	public bool invisibleInkBookIsRead { get { return _invisibleInkBookIsRead; } }

	/// <summary>
	/// ��������� �� ������������� ����� ��������� ����������.
	/// </summary>
	public bool libraryMapInvisibleIsRead { get { return _libraryMapInvisibleIsRead; } }

	/// <summary>
	/// ��������� �� ����������� ����� ��������� ����������.
	/// </summary>
	public bool libraryMapVisibleIsRead { get { return _libraryMapVisibleIsRead; } }

	/// <summary>
	/// ��������� �� �������� �� ����� ��� ����������.
	/// </summary>
	public bool libraryBookIsRead { get { return _libraryBookIsRead; } }

	/// <summary>
	/// ��������� �� ����� �����.
	/// </summary>
	public bool alphaBookIsRead { get { return _alphaBookIsRead; } }

	/// <summary>
	/// ����������� �� � ������� ������ "������� � ��������� �� �����������". ������� ���������� � ������ ������� ����������� �������� ���� "����� ������" � ������ ������ �� Fade To Black � �������� ����� ����� (��� ��� ������������ �������� �����). ��� �������� � �����, ����� �������� �������� ��������������� ��������� ������ (��� ����� �������� � ���� ������ � ������ ������, ��� ����� �� �������� �� ��� ��������).
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
