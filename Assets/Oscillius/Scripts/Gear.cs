using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gear : MonoBehaviour
{
	public GameObject gearInsideTF;
	public GameObject gearScrewedTF;
	[Tooltip ("Объект, входящий в состав объекта игрока, в который будет прилетать шестеренка при забирании.")]
	public GameObject pickUpTargetTF;

	private TwoStated _myTwoStated;
	private MeshRenderer _meshRenderer;
	private Collider _myCollider;

	private GearState _state = GearState.InsideMailbox;
	// Если true, шестеренка будет реагировать на рейкасты.
	private bool _enableInteractions = false;
	// Если true, шестеренка полностью прикручена.
	private bool _screwComplete = false;

	public delegate void Callback ();
	// Вызывается в тот момент, когда шестеренку начинают доставать из ящика.
	public Callback onStartBeingTaken;
	// Вызывается после того, как шестеренку достали из ящика и она закончила свою анимацию.
	public Callback onTaken;
	// Вызывается после того, как шестеренку прикрутили на ящик и она закончила свою анимацию.
	public Callback onScrewed;


	public enum GearState
	{
		InsideMailbox,
		Taken,
		Screwed
	}


	#region Awake

	private void Awake ()
	{
		AwakePreInit ();

		switch (GlobalManager.instance.storyState)
		{
			case StoryState.FirstDays:
			case StoryState.FirstEncounter:
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
		_myTwoStated = GetComponent<TwoStated> ();
		if (_myTwoStated == null)
			throw new System.Exception ("TwoStated comp not found.");

		_meshRenderer = GetComponent<MeshRenderer> ();
		if (_meshRenderer == null)
			throw new System.Exception ("MeshRenderer comp not found.");

		_myCollider = GetComponent<Collider> ();
		if (_myCollider == null)
			throw new System.Exception ("Collider component not found.");
	}

	private void AwakePostInit ()
	{
		// Если шестеренка внутри ящика, то при доставании она будет двигаться из ящика к игроку.
		if (_state == GearState.InsideMailbox)
		{
			_myTwoStated.falseTransformGO = gearInsideTF;
			_myTwoStated.trueTransformGO = pickUpTargetTF;
		}
		// Если шестеренка уже прикручена, то она должна там и оставаться.
		else if (_state == GearState.Screwed)
		{
			_myTwoStated.falseTransformGO = gearScrewedTF;
			_myTwoStated.trueTransformGO = gearScrewedTF;
		}

		// Ставим шестеренку в первоначальное положение.
		_myTwoStated.state = false;
		_myTwoStated.Snap ();
	}

	// По умолчанию просто отключаем шестеренку.
	private void AwakeDefault ()
	{
		gameObject.SetActive (false);
		_enableInteractions = false;
	}

	// В эпизоде, когда Алекс выходит за шестеренкой, ее стейт устанавливается в Inside Mailbox.
	private void AwakeOnGoForGear ()
	{
		_state = GearState.InsideMailbox;
		_enableInteractions = false;
	}

	#endregion


	private void Update ()
	{
		// Если шестеренку уже достали и она уже долетела до нужного места, делаем ее невидимой. И вызываем коллбек.
		if (_state == GearState.Taken)
		{
			if (!_myTwoStated.animationInProgress && _meshRenderer.enabled)
			{
				_meshRenderer.enabled = false;
				if (onTaken != null)
				{
					onTaken.Invoke ();
					onTaken = null;
				}
			}
		}
		// Если шестеренка прикручивается или была прикручена.
		else if (_state == GearState.Screwed)
		{
			if (!_screwComplete)
			{
				if (!_myTwoStated.animationInProgress)
				{
					_screwComplete = true;
					if (onScrewed != null)
					{
						onScrewed.Invoke ();
						onScrewed = null;
					}
				}
			}
		}

		if (_enableInteractions)
		{
			InteractionManager interactionManager = GlobalManager.instance.interactionManager;
			if (interactionManager.intersectionFound &&
				interactionManager.hitInfo.collider == _myCollider &&
				Input.GetKeyDown (interactionManager.interactKeyCode)
			)
			{
				if (_state == GearState.InsideMailbox)
				{
					_state = GearState.Taken;
					_myTwoStated.falseTransformGO = gearInsideTF;
					_myTwoStated.trueTransformGO = pickUpTargetTF;
					_myTwoStated.state = true;

					_enableInteractions = false;

					// Воспроизводим звук доставаемой шестеренки.
					if (onStartBeingTaken != null)
					{
						onStartBeingTaken.Invoke ();
						onStartBeingTaken = null;
					}
				}
			}
		}
	}

	/// <summary>
	/// Вызывать из почтового ящика, когда нужно начать прикручивать шестеренку.
	/// </summary>
	public void StartScrewing ()
	{
		// Переключаем стейт.
		_state = GearState.Screwed;

		// Показываем шестеренку.
		_meshRenderer.enabled = true;

		// Ставим шестеренку в исходную позицию.
		_myTwoStated.falseTransformGO = pickUpTargetTF;
		_myTwoStated.trueTransformGO = gearScrewedTF;
		_myTwoStated.state = false;
		_myTwoStated.Snap ();

		// Запускаем анимацию.
		_myTwoStated.state = true;
	}


	// ====================================================
	// ==================== PROPERTIES ====================
	// ====================================================

	public bool enableInteractions
	{
		get { return _enableInteractions; }
		set { _enableInteractions = value; }
	}

	/// <summary>
	/// Если true, шестеренка полностью прикручена, и можно отправлять солгард.
	/// </summary>
	public bool screwComplete { get { return _screwComplete; } }

}
