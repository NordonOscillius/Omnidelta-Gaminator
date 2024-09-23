using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mailbox : MonoBehaviour
{

	#region Mailbox States

	/// <summary>
	/// Основной стейт почтового ящика.
	/// </summary>
	public enum MailboxState
	{
		/// <summary>
		/// Стейт пустого закрытого ящика. Флажок опущен, крышка закрыта.
		/// </summary>
		EmptyClosed,
		/// <summary>
		/// Стейт пустого открытого ящика. Флажок опущен, крышка открыта.
		/// </summary>
		EmptyOpen,
		/// <summary>
		/// Шестеренка внутри, ящик закрыт. Флажок поднят, крышка закрыта.
		/// </summary>
		GearInsideClosed,
		/// <summary>
		/// Шестеренка внутри, ящик открыт. Флажок поднят, крышка открыта.
		/// </summary>
		GearInsideOpen,
		/// <summary>
		/// Ящик ждет, что к нему прикрутят шестеренку. Флажок опущен, крышка открыта.
		/// </summary>
		WaitForGearUpgrade,
		/// <summary>
		/// К ящику прикручена шестеренка, он ожидает отправки солгарда. Флажок опущен, крышка открыта.
		/// </summary>
		WaitForSolguardShipment,
		/// <summary>
		/// Ящик в процессе отправки солгарда. Флажок поднят, крышка закрыта. Через какое-то время ящик перейдет в стейт EmptyClosed.
		/// </summary>
		SendingSolguard
	}

	/// <summary>
	/// Состояние крышки почтового ящика.
	/// </summary>
	public enum MailboxCapState
	{
		Closed,
		Open
	}

	/// <summary>
	/// Состояние флажка почтового ящика.
	/// </summary>
	public enum MailboxFlagState
	{
		Down,
		Up
	}

	#endregion


	public GameObject capGO;
	public GameObject flagGO;
	// Может быть null.
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
	// Может быть null.
	private Gear _gear;
	private AlexDoorScript _alexDoor;
	
	private MailboxState _state = MailboxState.EmptyClosed;
	private MailboxCapState _capState = MailboxCapState.Closed;
	private MailboxFlagState _flagState = MailboxFlagState.Down;

	// Время, за которое солгард будет отправлен из закрытого ящика (т.е. за которое флажок опустится).
	private float _solguardSendPeriod = 2f;
	// Время Time.time в момент отправки солгарда.
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

		// Gear может отсутствовать, надо следить за этим в коде.
		if (gearGO != null)
			_gear = gearGO.GetComponent<Gear> ();

		_alexDoor = alexHouseDoorGO.GetComponent<AlexDoorScript> ();
		if (_alexDoor == null)
			throw new System.Exception ("AlexDoorScript not found.");

		// Собираем все коллайдеры, которые есть на этом объекте и на его потомках.
		_colliders = new List<Collider> (10);
		gameObject.GetComponentsInChildren<Collider> (_colliders);
	}

	private void AwakePostInit ()
	{
		// Определяем локальные стейты крышки и флажка по общему стейту почтового ящика.
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

	// Awake: Алекс отправляется к почтовому ящику за шестеренкой.
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
				// Шестеренка ждет внутри.
				if (_state == MailboxState.GearInsideClosed)
				{
					// Переключаем стейт в следующий.
					_state = MailboxState.GearInsideOpen;
					// Скрываем иконку взаимодействия.
					_myInteractive.doShowIcon = false;

					// Переключаем стейты крышки и флажка.
					DefineCapAndFlagStates (out _capState, out _flagState);
					_capTwoStated.state = ConvertCapStateToBool (_capState);
					_flagTwoStated.state = ConvertFlagStateToBool (_flagState);

					// Включаем интерактивность шестеренки, чтобы ее можно было подобрать.
					if (_gear == null)
						throw new System.Exception ("Gear not found.");
					_gear.enableInteractions = true;
					_gear.onStartBeingTaken = OnGearStartBeingTaken;
					_gear.onTaken = OnGearTaken;

					// Закрываем дверь Алекса, чтобы не запороть цикл.
					_alexDoor.preventInteractions = true;

					// Звук открывания крышки.
					openSoundGO.GetComponent<AudioSource> ().Play ();
				}
				// Шестеренка внутри, но ящик открыт.
				else if (_state == MailboxState.GearInsideOpen)
				{
					// Звук доставаемой шестеренки.
					//takeSoundGO.GetComponent<AudioSource> ().Play ();
					// ...
				}
				// Алекс забрал шестеренку, можно ее устанавливать.
				else if (_state == MailboxState.WaitForGearUpgrade)
				{
					_gear.onScrewed = OnGearScrewed;
					_gear.StartScrewing ();

					// На время прикручивания прячем иконку взаимодействия.
					_myInteractive.doShowIcon = false;

					// Переключаем стейт, чтобы не прикручивать шестеренку по второму разу.
					_state = MailboxState.WaitForSolguardShipment;

					// Звук прикручиваемой шестеренки.
					screwSoundGO.GetComponent<AudioSource> ().Play ();
				}
				// Шестеренка прикручивается или уже прикручена.
				else if (_state == MailboxState.WaitForSolguardShipment)
				{
					if (_gear.screwComplete)
					{
						// Переключаемся в режим отправки. Через какое-то время нужно будет перевести ящик в стейт EmptyClosed.
						_state = MailboxState.SendingSolguard;

						// Переключаем стейты крышки и флажка.
						DefineCapAndFlagStates (out _capState, out _flagState);
						_capTwoStated.state = ConvertCapStateToBool (_capState);
						_flagTwoStated.state = ConvertFlagStateToBool (_flagState);

						// Скрываем иконку взаимодействия.
						_myInteractive.doShowIcon = false;

						// Переключаем стейт истории в следующий (несмотря на то, что солгард еще в ящике).
						GlobalManager.instance.storyState = StoryState.SolguardIsSent;

						// Запоминаем момент отправки солгарда.
						_solguardSendStartTime = Time.time;

						// Открываем дверь в дом Алекса.
						_alexDoor.preventInteractions = false;

						// Звук закрываемой крышки.
						closeSoundGO.GetComponent<AudioSource> ().Play ();
					}
				}
			}
		}
	}

	private void UpdateOnSolguardIsSent ()
	{
		// Если отправка солгарда еще не завершена, проверяем, не пришло ли еще время ее завершить.
		if (_state == MailboxState.SendingSolguard && Time.time - _solguardSendStartTime >= _solguardSendPeriod)
		{
			_state = MailboxState.EmptyClosed;

			// Переключаем стейты крышки и флажка.
			DefineCapAndFlagStates (out _capState, out _flagState);
			_capTwoStated.state = ConvertCapStateToBool (_capState);
			_flagTwoStated.state = ConvertFlagStateToBool (_flagState);

			// Звук опускаемого флажка.
			flagSoundGO.GetComponent<AudioSource> ().Play ();
		}
	}

	private void OnGearStartBeingTaken ()
	{
		takeSoundGO.GetComponent<AudioSource> ().Play ();
	}

	// Если шестеренку окончательно достали :), переключаем стейт, чтобы можно было ее прикрутить.
	private void OnGearTaken ()
	{
		// Переключаем стейт.
		_state = MailboxState.WaitForGearUpgrade;

		// Переключаем стейты крышки и флажка.
		DefineCapAndFlagStates (out _capState, out _flagState);
		_capTwoStated.state = ConvertCapStateToBool (_capState);
		_flagTwoStated.state = ConvertFlagStateToBool (_flagState);

		// Показываем иконку взаимодействия.
		_myInteractive.doShowIcon = true;
		_myInteractive.interactionType = InteractionType.ScrewGear;
	}

	private void OnGearScrewed ()
	{
		//_state = MailboxState.WaitForSolguardShipment;

		// Показываем иконку "Отправить солгард".
		_myInteractive.interactionType = InteractionType.SendSolguard;
		_myInteractive.doShowIcon = true;
	}

	#endregion


	#region Utils

	/// <summary>
	/// Определяет стейты крышки и флажка по стейту почтового ящика.
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
