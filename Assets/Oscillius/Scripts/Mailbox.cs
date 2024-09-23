using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mailbox : MonoBehaviour
{

	#region Mailbox States

	/// <summary>
	/// �������� ����� ��������� �����.
	/// </summary>
	public enum MailboxState
	{
		/// <summary>
		/// ����� ������� ��������� �����. ������ ������, ������ �������.
		/// </summary>
		EmptyClosed,
		/// <summary>
		/// ����� ������� ��������� �����. ������ ������, ������ �������.
		/// </summary>
		EmptyOpen,
		/// <summary>
		/// ���������� ������, ���� ������. ������ ������, ������ �������.
		/// </summary>
		GearInsideClosed,
		/// <summary>
		/// ���������� ������, ���� ������. ������ ������, ������ �������.
		/// </summary>
		GearInsideOpen,
		/// <summary>
		/// ���� ����, ��� � ���� ��������� ����������. ������ ������, ������ �������.
		/// </summary>
		WaitForGearUpgrade,
		/// <summary>
		/// � ����� ���������� ����������, �� ������� �������� ��������. ������ ������, ������ �������.
		/// </summary>
		WaitForSolguardShipment,
		/// <summary>
		/// ���� � �������� �������� ��������. ������ ������, ������ �������. ����� �����-�� ����� ���� �������� � ����� EmptyClosed.
		/// </summary>
		SendingSolguard
	}

	/// <summary>
	/// ��������� ������ ��������� �����.
	/// </summary>
	public enum MailboxCapState
	{
		Closed,
		Open
	}

	/// <summary>
	/// ��������� ������ ��������� �����.
	/// </summary>
	public enum MailboxFlagState
	{
		Down,
		Up
	}

	#endregion


	public GameObject capGO;
	public GameObject flagGO;
	// ����� ���� null.
	public GameObject gearGO;
	public GameObject alexHouseDoorGO;

	public GameObject openSoundGO;
	public GameObject takeSoundGO;
	public GameObject screwSoundGO;
	public GameObject closeSoundGO;
	public GameObject flagSoundGO;

	private Interactive _myInteractive;
	private TwoStated _capTwoStated;
	private TwoStated _flagTwoStated;
	private List<Collider> _colliders;
	// ����� ���� null.
	private Gear _gear;
	private AlexDoorScript _alexDoor;
	
	private MailboxState _state = MailboxState.EmptyClosed;
	private MailboxCapState _capState = MailboxCapState.Closed;
	private MailboxFlagState _flagState = MailboxFlagState.Down;

	// �����, �� ������� ������� ����� ��������� �� ��������� ����� (�.�. �� ������� ������ ���������).
	private float _solguardSendPeriod = 2f;
	// ����� Time.time � ������ �������� ��������.
	private float _solguardSendStartTime = 0f;


	#region Awake

	private void Awake ()
	{
		AwakePreInit ();
		
		switch (GlobalManager.instance.storyState)
		{
			case StoryState.Arrival:
			case StoryState.ArrivalAfterSpeech:
			case StoryState.FirstDays:
			case StoryState.SolguardHike:
			case StoryState.SunSpeech:
				AwakeDefault ();
				break;
			case StoryState.GoForGear:
				AwakeOnGoForGear ();
				break;
		}

		AwakePostInit ();
	}

	private void AwakePreInit ()
	{
		_myInteractive = gameObject.GetComponent<Interactive> ();
		if (_myInteractive == null)
			throw new System.Exception ("The Interactive component not found.");

		_capTwoStated = capGO.GetComponent<TwoStated> ();
		if (_capTwoStated == null)
			throw new System.Exception ("Cap's TwoStated component not found.");

		_flagTwoStated = flagGO.GetComponent<TwoStated> ();
		if (_flagTwoStated == null)
			throw new System.Exception ("Flag's TwoStated component not found.");

		// Gear ����� �������������, ���� ������� �� ���� � ����.
		if (gearGO != null)
			_gear = gearGO.GetComponent<Gear> ();

		_alexDoor = alexHouseDoorGO.GetComponent<AlexDoorScript> ();
		if (_alexDoor == null)
			throw new System.Exception ("AlexDoorScript not found.");

		// �������� ��� ����������, ������� ���� �� ���� ������� � �� ��� ��������.
		_colliders = new List<Collider> (10);
		gameObject.GetComponentsInChildren<Collider> (_colliders);
	}

	private void AwakePostInit ()
	{
		// ���������� ��������� ������ ������ � ������ �� ������ ������ ��������� �����.
		DefineCapAndFlagStates (out _capState, out _flagState);

		_capTwoStated.state = ConvertCapStateToBool (_capState);
		_capTwoStated.Snap ();

		_flagTwoStated.state = ConvertFlagStateToBool (_flagState);
		_flagTwoStated.Snap ();
	}

	private void AwakeDefault ()
	{
		_state = MailboxState.EmptyClosed;
		_myInteractive.doShowIcon = false;
	}

	// Awake: ����� ������������ � ��������� ����� �� �����������.
	private void AwakeOnGoForGear ()
	{
		_state = MailboxState.GearInsideClosed;
		_myInteractive.interactionType = InteractionType.Open;
		_myInteractive.doShowIcon = true;
	}

	#endregion


	#region Update

	private void Update ()
	{
		StoryState storyState = GlobalManager.instance.storyState;

		if (storyState == StoryState.GoForGear)
			UpdateOnGoForGear ();
		else if (storyState == StoryState.SolguardIsSent)
			UpdateOnSolguardIsSent ();
	}

	private void UpdateOnGoForGear ()
	{
		InteractionManager interactionManager = GlobalManager.instance.interactionManager;
		if (interactionManager.intersectionFound && _colliders.Contains (interactionManager.hitInfo.collider))
		{
			if (Input.GetKeyDown (interactionManager.interactKeyCode))
			{
				// ���������� ���� ������.
				if (_state == MailboxState.GearInsideClosed)
				{
					// ����������� ����� � ���������.
					_state = MailboxState.GearInsideOpen;
					// �������� ������ ��������������.
					_myInteractive.doShowIcon = false;

					// ����������� ������ ������ � ������.
					DefineCapAndFlagStates (out _capState, out _flagState);
					_capTwoStated.state = ConvertCapStateToBool (_capState);
					_flagTwoStated.state = ConvertFlagStateToBool (_flagState);

					// �������� ��������������� ����������, ����� �� ����� ���� ���������.
					if (_gear == null)
						throw new System.Exception ("Gear not found.");
					_gear.enableInteractions = true;
					_gear.onStartBeingTaken = OnGearStartBeingTaken;
					_gear.onTaken = OnGearTaken;

					// ��������� ����� ������, ����� �� �������� ����.
					_alexDoor.preventInteractions = true;

					// ���� ���������� ������.
					openSoundGO.GetComponent<AudioSource> ().Play ();
				}
				// ���������� ������, �� ���� ������.
				else if (_state == MailboxState.GearInsideOpen)
				{
					// ���� ����������� ����������.
					//takeSoundGO.GetComponent<AudioSource> ().Play ();
					// ...
				}
				// ����� ������ ����������, ����� �� �������������.
				else if (_state == MailboxState.WaitForGearUpgrade)
				{
					_gear.onScrewed = OnGearScrewed;
					_gear.StartScrewing ();

					// �� ����� ������������� ������ ������ ��������������.
					_myInteractive.doShowIcon = false;

					// ����������� �����, ����� �� ������������ ���������� �� ������� ����.
					_state = MailboxState.WaitForSolguardShipment;

					// ���� �������������� ����������.
					screwSoundGO.GetComponent<AudioSource> ().Play ();
				}
				// ���������� �������������� ��� ��� ����������.
				else if (_state == MailboxState.WaitForSolguardShipment)
				{
					if (_gear.screwComplete)
					{
						// ������������� � ����� ��������. ����� �����-�� ����� ����� ����� ��������� ���� � ����� EmptyClosed.
						_state = MailboxState.SendingSolguard;

						// ����������� ������ ������ � ������.
						DefineCapAndFlagStates (out _capState, out _flagState);
						_capTwoStated.state = ConvertCapStateToBool (_capState);
						_flagTwoStated.state = ConvertFlagStateToBool (_flagState);

						// �������� ������ ��������������.
						_myInteractive.doShowIcon = false;

						// ����������� ����� ������� � ��������� (�������� �� ��, ��� ������� ��� � �����).
						GlobalManager.instance.storyState = StoryState.SolguardIsSent;

						// ���������� ������ �������� ��������.
						_solguardSendStartTime = Time.time;

						// ��������� ����� � ��� ������.
						_alexDoor.preventInteractions = false;

						// ���� ����������� ������.
						closeSoundGO.GetComponent<AudioSource> ().Play ();
					}
				}
			}
		}
	}

	private void UpdateOnSolguardIsSent ()
	{
		// ���� �������� �������� ��� �� ���������, ���������, �� ������ �� ��� ����� �� ���������.
		if (_state == MailboxState.SendingSolguard && Time.time - _solguardSendStartTime >= _solguardSendPeriod)
		{
			_state = MailboxState.EmptyClosed;

			// ����������� ������ ������ � ������.
			DefineCapAndFlagStates (out _capState, out _flagState);
			_capTwoStated.state = ConvertCapStateToBool (_capState);
			_flagTwoStated.state = ConvertFlagStateToBool (_flagState);

			// ���� ����������� ������.
			flagSoundGO.GetComponent<AudioSource> ().Play ();
		}
	}

	private void OnGearStartBeingTaken ()
	{
		takeSoundGO.GetComponent<AudioSource> ().Play ();
	}

	// ���� ���������� ������������ ������� :), ����������� �����, ����� ����� ���� �� ����������.
	private void OnGearTaken ()
	{
		// ����������� �����.
		_state = MailboxState.WaitForGearUpgrade;

		// ����������� ������ ������ � ������.
		DefineCapAndFlagStates (out _capState, out _flagState);
		_capTwoStated.state = ConvertCapStateToBool (_capState);
		_flagTwoStated.state = ConvertFlagStateToBool (_flagState);

		// ���������� ������ ��������������.
		_myInteractive.doShowIcon = true;
		_myInteractive.interactionType = InteractionType.ScrewGear;
	}

	private void OnGearScrewed ()
	{
		//_state = MailboxState.WaitForSolguardShipment;

		// ���������� ������ "��������� �������".
		_myInteractive.interactionType = InteractionType.SendSolguard;
		_myInteractive.doShowIcon = true;
	}

	#endregion


	#region Utils

	/// <summary>
	/// ���������� ������ ������ � ������ �� ������ ��������� �����.
	/// </summary>
	/// <param name="capState"></param>
	/// <param name="flagState"></param>
	private void DefineCapAndFlagStates (out MailboxCapState capState, out MailboxFlagState flagState)
	{
		switch (_state)
		{
			case MailboxState.EmptyClosed:
				capState = MailboxCapState.Closed;
				flagState = MailboxFlagState.Down;
				break;
			case MailboxState.EmptyOpen:
				capState = MailboxCapState.Open;
				flagState = MailboxFlagState.Down;
				break;
			case MailboxState.GearInsideClosed:
				capState = MailboxCapState.Closed;
				flagState = MailboxFlagState.Up;
				break;
			case MailboxState.GearInsideOpen:
				capState = MailboxCapState.Open;
				flagState = MailboxFlagState.Up;
				break;
			case MailboxState.WaitForGearUpgrade:
				capState = MailboxCapState.Open;
				flagState = MailboxFlagState.Down;
				break;
			case MailboxState.WaitForSolguardShipment:
				capState = MailboxCapState.Open;
				flagState = MailboxFlagState.Down;
				break;
			case MailboxState.SendingSolguard:
				capState = MailboxCapState.Closed;
				flagState = MailboxFlagState.Up;
				break;
			default:
				throw new System.Exception ("Unknown mailbox state.");
		}
	}

	public static bool ConvertCapStateToBool (MailboxCapState capState)
	{
		switch (capState)
		{
			case MailboxCapState.Closed: return false;
			case MailboxCapState.Open: return true;
			default:
				throw new System.Exception ("Unknown state.");
		}
	}

	public static bool ConvertFlagStateToBool (MailboxFlagState flagState)
	{
		switch (flagState)
		{
			case MailboxFlagState.Down: return false;
			case MailboxFlagState.Up: return true;
			default:
				throw new System.Exception ("Unknown state.");
		}
	}

	#endregion

}
