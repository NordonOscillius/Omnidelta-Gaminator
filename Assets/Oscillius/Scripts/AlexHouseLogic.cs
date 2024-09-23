using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AlexHouseLogic : MonoBehaviour
{
	[Tooltip ("GameObject � ����������� AudioSource ��� ���� ���� � ������� First Encounter.")]
	public GameObject firstEncounterAidaSpeechGO;
	[Tooltip ("GameObject � ����������� AudioSource ��� ���� ������ � ������� First Encounter.")]
	public GameObject firstEncounterAlexSpeechGO;
	[Tooltip ("����� ������.")]
	public GameObject door;
	[Tooltip ("GameObject � ����������� AudioSource ��� ���� ������ � ������� Conversation Before Library.")]
	public GameObject beforeLibraryAlexSpeechGO;
	[Tooltip ("GameObject � ����������� AudioSource ��� ���� ���� � ������� Conversation Before Library.")]
	public GameObject beforeLibraryAidaSpeechGO;
	[Tooltip ("GameObject � ����������� AudioSource ��� ���� ������ � ������� Aquamarine Talk.")]
	public GameObject aquamarineTalkAlexGO;
	[Tooltip ("GameObject � ����������� AudioSource ��� ���� ���� � ������� Aquamarine Talk.")]
	public GameObject aquamarineTalkAidaGO;
	[Tooltip ("�������� ����� � ���� ����.")]
	public string aidaHouseSceneName;
	[Tooltip ("�������� ����� '����������� �������'.")]
	public string toBeContinuedSceneName;

	private GameObject _startPointGO;
	private CharController _charController;

	// ��������� �� ��������� �������� � ����� First Encounter.
	private bool _firstEncounterSpeechIsOver = false;

	// � ������� "�� �����������": ����� ����� ��������� ����� � Fade In.
	private float _goForGearFadeInDelay = 2.5f;
	// �������� ���������� X ���������, ��� ������� ���������� ������ � ����� � ������� "������� ���������". ����� �� �������� � ���������� � �� ������� ���������.
	private float _solguardIsSentDialogueStartX = -1.5f;
	// � ����� ��������� � ������: ����� ����� ���������� ����� ������� � ���� ������ � ������� �������� ����� �����. ����� ��-�� ������������ ��� �������� �����.
	private float _beforeLibraryAidaHouseLoadDelay = .02f;
	// �����, ��������� � ������� ���������� ����� ��������� � ���� ������.
	private float _beforeLibraryTimeToAidaHouseLoad = 0f;

	// ������������ ��������� ����� ����.
	private AudioSource _aquamarineTalkAidaSource;


	#region Awake

	private void Awake ()
	{
		StoryState storyState = GlobalManager.instance.storyState;

		if (storyState == StoryState.Arrival)
			AwakeOnArrival ();
		else if (storyState == StoryState.ArrivalAfterSpeech)
			AwakeDefault ();
		else if (storyState == StoryState.FirstDays)
			AwakeDefault ();
		else if (storyState == StoryState.FirstEncounter)
			AwakeDefault ();
		else if (storyState == StoryState.SolguardHike)
			AwakeDefault ();
		else if (storyState == StoryState.GoForGear)
			AwakeOnGoForGear ();
		else if (storyState == StoryState.SolguardIsSent)
			AwakeOnSolguardIsSent ();
		else if (storyState == StoryState.AquamarineTalk)
			AwakeOnAquamarineTalk ();
	}

	private void AwakeDefault ()
	{
		// �������� ������ Start Point.
		CacheStartPointGO ();
	}

	private void AwakeOnArrival ()
	{
		// ���� �� ����� � ��� ������ ������ ����� ��������, ����������� ����� � ���������.
		GlobalManager.instance.storyState = StoryState.ArrivalAfterSpeech;
		// �������� ������ Start Point.
		CacheStartPointGO ();
	}

	private void AwakeOnGoForGear ()
	{
		// �������� ������ Start Point.
		CacheStartPointGO ();

		// ��� �������. ��������� ������ ����������, �������� �� ����� �� ����� ��������� � ����� ��� ����� ����, ��� ����� ����� �� ����� �� �����������, �� ��������.
		// ���� ����� �� � �����, �� ���������� Fade In Delay � ������������� ��������. � Start() ����� ������� �������� (< 0), � ���� ��� ����, �������� ����������� ������� Fade In.
		if (_startPointGO.name != "Start Point (3)")
		{
			_goForGearFadeInDelay = -1;
		}
	}

	private void AwakeOnSolguardIsSent ()
	{
		CacheStartPointGO ();
	}

	private void AwakeOnAquamarineTalk ()
	{
		CacheStartPointGO ();
	}

	#endregion


	#region Start

	private void Start ()
	{
		StoryState storyState = GlobalManager.instance.storyState;

		if (storyState == StoryState.Arrival)
			StartOnArrival ();
		else if (storyState == StoryState.ArrivalAfterSpeech)
			StartOnArrivalAfterSpeech ();
		else if (storyState == StoryState.FirstDays)
			StartOnFirstDays ();
		else if (storyState == StoryState.FirstEncounter)
			StartOnFirstEncounter ();
		else if (storyState == StoryState.SolguardHike)
			StartOnSolguardHike ();
		else if (storyState == StoryState.GoForGear)
			StartOnGoForGear ();
		else if (storyState == StoryState.SolguardIsSent)
			StartOnSolguardIsSent ();
		else if (storyState == StoryState.AquamarineTalk)
			StartOnAquamarineTalk ();
	}

	private void StartOnArrival ()
	{
		// ������ �������� ������ � ��������� Fade In.
		StartDefaultFadeIn ();
		// ������ ��������� � ������ ���������.
		PlaceCharacterAtStartPoint ();
	}

	private void StartOnArrivalAfterSpeech ()
	{
		// ������ �������� ������ � ��������� Fade In.
		StartDefaultFadeIn ();
		// ������ ��������� � ������ ���������.
		PlaceCharacterAtStartPoint ();
	}

	private void StartOnFirstDays ()
	{
		// ������ �������� ������ � ��������� Fade In.
		StartDefaultFadeIn ();
		// ������ ��������� � ������ ���������.
		PlaceCharacterAtStartPoint ();
	}

	private void StartOnFirstEncounter ()
	{
		// ������ �������� ������ � ��������� Fade In.
		StartDefaultFadeIn ();
		// ������ ��������� � ������ ���������.
		PlaceCharacterAtStartPoint ();

		// ������ ���������� �������, ���� �� ���.
		//AudioLayer ambientLayer = GlobalManager.instance.audioManager.GetLayerByType (AudioLayerType.Ambient);
		//ambientLayer.introSpeed = .25f;
		//ambientLayer.outroSpeed = 10f;
		//ambientLayer.FadeOut ();
		// �������� �������.
		_firstEncounterSpeechIsOver = false;
		// ��������� ���� ���� (������ ��������).
		firstEncounterAidaSpeechGO.GetComponent<AudioSource> ().Play ();
		// ��������� ���� ������ (������ ��������).
		firstEncounterAlexSpeechGO.GetComponent<AudioSource> ().Play ();
		// ��������� �����, ����� ����� �� �������.
		AlexInnerDoor doorScript = door.GetComponent<AlexInnerDoor> ();
		if (doorScript == null)
			throw new System.Exception ("The door doesn't have an AlexInnerDoor component.");
		doorScript.preventInteractions = true;
	}

	private void StartOnSolguardHike ()
	{
		// ������ �������� ������ � ��������� Fade In.
		StartDefaultFadeIn ();
		// ������ ��������� � ������ ���������.
		PlaceCharacterAtStartPoint ();
	}

	private void StartOnGoForGear ()
	{
		// ������ ��������� � ������ ���������.
		PlaceCharacterAtStartPoint ();
		// �������� ���������� ���������.
		_charController = GlobalManager.instance.alexCamera.transform.root.gameObject.GetComponent<CharController> ();

		// ���� ����� ����� �� �����������, �� ��������, �� �������� �������.
		if (_goForGearFadeInDelay < 0)
		{
			StartDefaultFadeIn ();
		}
		// ���� ��� ��� ������, ����� ����� ���������� �������� � �����.
		else
		{
			// ���������� ������ �� ����� (���� ���� ��-�� ����� ������ ���������, � �� � 3D).
			_charController.enableLook = false;
			_charController.enableMove = false; 
		}
	}

	private void StartOnSolguardIsSent ()
	{
		// ������ �������� ������ � ��������� Fade In.
		StartDefaultFadeIn ();
		// ������ ��������� � ������ ���������.
		PlaceCharacterAtStartPoint ();
		// �������� ���������� ���������.
		_charController = GlobalManager.instance.alexCamera.transform.root.gameObject.GetComponent<CharController> ();
	}

	private void StartOnAquamarineTalk ()
	{
		// ������ �������� ������ � ��������� Fade In.
		StartDefaultFadeIn ();
		// ������ ��������� � ������ ���������.
		PlaceCharacterAtStartPoint ();
		// ��������� ������� ������.
		aquamarineTalkAlexGO.GetComponent<AudioSource> ().Play ();
		// �������� ��������� ����� ����.
		_aquamarineTalkAidaSource = aquamarineTalkAidaGO.GetComponent<AudioSource> ();
		// ��������� ������� ����.
		_aquamarineTalkAidaSource.Play ();
		// ��������� �����, ����� ����� �� �������.
		AlexInnerDoor doorScript = door.GetComponent<AlexInnerDoor> ();
		if (doorScript == null)
			throw new System.Exception ("The door doesn't have an AlexInnerDoor component.");
		doorScript.preventInteractions = true;
	}

	#endregion

	private void Update ()
	{
		StoryState storyState = GlobalManager.instance.storyState;

		if (storyState == StoryState.FirstEncounter)
			UpdateOnFirstEncounter ();
		else if (storyState == StoryState.GoForGear)
			UpdateOnGoForGear ();
		else if (storyState == StoryState.SolguardIsSent)
			UpdateOnSolguardIsSent ();
		else if (storyState == StoryState.ConversationBeforeLibrary)
			UpdateOnConversationBeforeLibrary ();
		else if (storyState == StoryState.AquamarineTalk)
			UpdateOnAquamarineTalk ();
	}

	private void UpdateOnFirstEncounter ()
	{
		// ���� ��������� ��������� �������� (�� ����� ������� ���������), �������� ����� ��� ������ � ��������� ����� ����� ��� ������: ����� �� ���������.
		if (!_firstEncounterSpeechIsOver &&
			!firstEncounterAidaSpeechGO.GetComponent<AudioSource> ().isPlaying &&
			!firstEncounterAlexSpeechGO.GetComponent<AudioSource> ().isPlaying
		)
		{
			_firstEncounterSpeechIsOver = true;
			door.GetComponent<AlexInnerDoor> ().preventInteractions = false;

			// � ����� �� ���������.
			GlobalManager.instance.storyState = StoryState.SolguardHike;
		}
	}

	private void UpdateOnGoForGear ()
	{
		// ����������� �������. ��� ������, ����� ����� ����� �� �����, �� ��������, �� �������� �������.
		if (_goForGearFadeInDelay < 0)
		{

		}
		// ����������� �����: ����� � ���� �������.
		else
		{
			// Fade In ����� �������������� ��������.
			if (Time.timeSinceLevelLoad >= _goForGearFadeInDelay)
			{
				ScreenFader screenFader = GlobalManager.instance.screenFader;
				screenFader.speed = 100f;
				screenFader.StartFadeIn ();

				// ������������� ���������� ������������ ����� ����� ����.
				_goForGearFadeInDelay = Mathf.Infinity;
			}
		}

		// ���� ���������� ��������� ��������, ���������, �� ���� �� ��� ��������.
		if (!_charController.enableLook)
		{
			OscClip gearClip = GlobalManager.instance.audioManager.GetLayerByType (AudioLayerType.Speech).GetClipWithResourceType (OscAudioResourceEnum.Speech_Gear);
			if (gearClip == null)
			{
				_charController.enableLook = true;
				_charController.enableMove = true;
			} 
		}
	}

	private void UpdateOnSolguardIsSent ()
	{
		// ���� ����� ������������ � �����, �������� ��������.
		if (_charController.gameObject.transform.position.x <= _solguardIsSentDialogueStartX)
		{
			// ����������� ����� �������.
			GlobalManager.instance.storyState = StoryState.ConversationBeforeLibrary;

			// ��������� ���� ������.
			beforeLibraryAlexSpeechGO.GetComponent<AudioSource> ().Play ();
			// ��������� ���� ����.
			beforeLibraryAidaSpeechGO.GetComponent<AudioSource> ().Play ();
		}
	}

	private void UpdateOnConversationBeforeLibrary ()
	{
		// ���� �������� (�� �������� � ������� ����) ��� ����������, ��������� � ��������� � ��� ����.
		if (!beforeLibraryAidaSpeechGO.GetComponent<AudioSource> ().isPlaying)
		{
			// ���� ������� ������ ��� �������, ��������� ������ Fade To Black.
			if (Mathf.Approximately (_beforeLibraryTimeToAidaHouseLoad, 0))
			{
				ScreenFader screenFader = GlobalManager.instance.screenFader;
				screenFader.speed = 100f;
				screenFader.StartFadeToBlack ();
			}

			// ���� ����� �������� ����������, ������ �����.
			_beforeLibraryTimeToAidaHouseLoad += Time.deltaTime;
			if (_beforeLibraryTimeToAidaHouseLoad >= _beforeLibraryAidaHouseLoadDelay)
			{
				_beforeLibraryAidaHouseLoadDelay = Mathf.Infinity;

				GlobalManager.instance.playerStartPointName = "Start Point (3)";
				StartCoroutine (LoadAidaHouseSceneBeforeLibrary ());
			}
		}
	}

	// �������� ����� � ���� ����.
	private IEnumerator LoadAidaHouseSceneBeforeLibrary ()
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync (aidaHouseSceneName);
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
	}

	private void UpdateOnAquamarineTalk ()
	{
		// ����� ���� �������� ��������, �������� ����� "����������� �������".
		if (!_aquamarineTalkAidaSource.isPlaying)
		{
			GlobalManager.instance.storyState = StoryState.ToBeContinued;

			ScreenFader screenFader = GlobalManager.instance.screenFader;
			screenFader.speed = .5f;
			screenFader.StartFadeToBlack (OnFadedOutOnAquamarineTalk);
		}
	}

	private void OnFadedOutOnAquamarineTalk ()
	{
		StartCoroutine (LoadToBeContinuedScene ());
	}

	// �������� ����� "����������� �������".
	private IEnumerator LoadToBeContinuedScene ()
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync (toBeContinuedSceneName);
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
	}


	#region Utils

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

	#endregion

}
