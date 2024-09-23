using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gear : MonoBehaviour
{
	public GameObject gearInsideTF;
	public GameObject gearScrewedTF;
	[Tooltip ("������, �������� � ������ ������� ������, � ������� ����� ��������� ���������� ��� ���������.")]
	public GameObject pickUpTargetTF;

	private TwoStated _myTwoStated;
	private MeshRenderer _meshRenderer;
	private Collider _myCollider;

	private GearState _state = GearState.InsideMailbox;
	// ���� true, ���������� ����� ����������� �� ��������.
	private bool _enableInteractions = false;
	// ���� true, ���������� ��������� ����������.
	private bool _screwComplete = false;

	public delegate void Callback ();
	// ���������� � ��� ������, ����� ���������� �������� ��������� �� �����.
	public Callback onStartBeingTaken;
	// ���������� ����� ����, ��� ���������� ������� �� ����� � ��� ��������� ���� ��������.
	public Callback onTaken;
	// ���������� ����� ����, ��� ���������� ���������� �� ���� � ��� ��������� ���� ��������.
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
		// ���� ���������� ������ �����, �� ��� ���������� ��� ����� ��������� �� ����� � ������.
		if (_state == GearState.InsideMailbox)
		{
			_myTwoStated.falseTransformGO = gearInsideTF;
			_myTwoStated.trueTransformGO = pickUpTargetTF;
		}
		// ���� ���������� ��� ����������, �� ��� ������ ��� � ����������.
		else if (_state == GearState.Screwed)
		{
			_myTwoStated.falseTransformGO = gearScrewedTF;
			_myTwoStated.trueTransformGO = gearScrewedTF;
		}

		// ������ ���������� � �������������� ���������.
		_myTwoStated.state = false;
		_myTwoStated.Snap ();
	}

	// �� ��������� ������ ��������� ����������.
	private void AwakeDefault ()
	{
		gameObject.SetActive (false);
		_enableInteractions = false;
	}

	// � �������, ����� ����� ������� �� �����������, �� ����� ��������������� � Inside Mailbox.
	private void AwakeOnGoForGear ()
	{
		_state = GearState.InsideMailbox;
		_enableInteractions = false;
	}

	#endregion


	private void Update ()
	{
		// ���� ���������� ��� ������� � ��� ��� �������� �� ������� �����, ������ �� ���������. � �������� �������.
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
		// ���� ���������� �������������� ��� ���� ����������.
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

					// ������������� ���� ����������� ����������.
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
	/// �������� �� ��������� �����, ����� ����� ������ ������������ ����������.
	/// </summary>
	public void StartScrewing ()
	{
		// ����������� �����.
		_state = GearState.Screwed;

		// ���������� ����������.
		_meshRenderer.enabled = true;

		// ������ ���������� � �������� �������.
		_myTwoStated.falseTransformGO = pickUpTargetTF;
		_myTwoStated.trueTransformGO = gearScrewedTF;
		_myTwoStated.state = false;
		_myTwoStated.Snap ();

		// ��������� ��������.
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
	/// ���� true, ���������� ��������� ����������, � ����� ���������� �������.
	/// </summary>
	public bool screwComplete { get { return _screwComplete; } }

}
