using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��������� ������������� ������, ������� ����� ���������� � ����� �� ���� ���������.
/// </summary>
public class TwoStated : MonoBehaviour
{
	[Tooltip ("������, ��������� �������� ��������� ������� ��� ������ ��� state = false.")]
	public GameObject falseTransformGO;
	[Tooltip ("������, ��������� �������� ��������� ������� ��� ������ ��� state = true.")]
	public GameObject trueTransformGO;
	[Tooltip ("�������� �������� - �������� �� ���� �� �������")]
	public float stiffness = .2f;
	[Tooltip ("�� ����� ������� ������� �������� ���������� �������������� � ������ ������ ������� ���������.")]
	public float animTime = 1.5f;
	public bool animatePosition = false;
	public bool animateRotation = true;

	// ��������� ������. True ������������ ��� ���������/���������/������� ���������, false - ��� ���������/����������/�������.
	private bool _state;
	// ������ ������� Time.time, � ������� �������� ��������.
	private float _animStartTime = 0f;
	// �������� �� ��������.
	private bool _animInProgress = false;

	// ����������� �����, ��� �������� �������� stiffness.
	private float _standardDeltaTime = 1 / 60f;


	private void Update ()
	{
		// ���� �������� �� ���������������, �������.
		if (!_animInProgress)
			return;

		Transform targetTF = _state ? trueTransformGO.transform : falseTransformGO.transform;

		// ���� �������� ���� �����������, ������ ������� ��� ������ � ��������.
		if (Time.time - _animStartTime >= animTime)
		{
			_animInProgress = false;

			if (animateRotation)
			{
				gameObject.transform.rotation = targetTF.transform.rotation; 
			}
			if (animatePosition)
			{
				gameObject.transform.position = targetTF.transform.position;
			}
		}
		// ������ �������� �������� �������������.
		else
		{
			float usedStiffness = 1 - Mathf.Pow (1 - stiffness, Time.deltaTime / _standardDeltaTime);
			//float usedStiffness = stiffness;
			if (animateRotation)
			{
				gameObject.transform.rotation = Quaternion.Lerp (gameObject.transform.rotation, targetTF.transform.rotation, usedStiffness); 
			}
			if (animatePosition)
			{
				gameObject.transform.position = Vector3.Lerp (gameObject.transform.position, targetTF.transform.position, usedStiffness);
			}
		}
	}

	// ���������� ��� ����� ���������.
	private void OnStateFlipped ()
	{
		_animStartTime = Time.time;
		_animInProgress = true;
	}

	/// <summary>
	/// ��������� ������� ������������� � ������, �� ��������� ���������� ��������.
	/// </summary>
	public void Snap ()
	{
		_animInProgress = false;

		Transform targetTF = _state ? trueTransformGO.transform : falseTransformGO.transform;
		if (animatePosition)
		{
			gameObject.transform.position = targetTF.transform.position;
		}
		if (animateRotation)
		{
			gameObject.transform.rotation = targetTF.transform.rotation;
		}
	}


	// ====================================================
	// ==================== PROPERTIES ====================
	// ====================================================

	public bool state
	{
		get { return _state; }
		set
		{
			if (_state == value)
				return;

			_state = value;
			OnStateFlipped ();
		}
	}

	/// <summary>
	/// �������� �� ��������.
	/// </summary>
	public bool animationInProgress { get { return _animInProgress; } }

}
