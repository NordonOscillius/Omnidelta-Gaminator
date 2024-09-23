using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharController : MonoBehaviour
{
	[Tooltip ("������, ������� ���������� ���������. ������ �������� �������� �������� ���������.")]
	public Camera fpsCamera;

	//private CapsuleCollider _capsuleCollider;
	//private Rigidbody _rigidBody;
	private CharacterController _unityController;

	// � ��������.
	private float _yaw = 0;
	// � ��������.
	private float _pitch = 0;
	//private bool _yawIsDirty = true;
	private Vector3 _cameraInitPosition;

	public float walkSpeed = 4f;
	public float runSpeed = 10f;
	public float retireSpeed = 3f;
	public float acceleration = 20f;
	public float turnCoef = .02f;

	public bool enableMove = true;
	public bool enableLook = true;

	private float _slopeLimit = 45f;
	private float _groundedNormalMinY = 0;

	private bool _isGrounded = false;

	private bool _leftKeyIsDown = false;
	private bool _rightKeyIsDown = false;
	private bool _forwardKeyIsDown = false;
	private bool _backwardKeyIsDown = false;
	private bool _runKeyIsDown = false;

	private Vector3 _curVelocity = Vector3.zero;
	private Vector3 _maxUpNormal = Vector3.zero;

	// �������� �������� ����, ��������������� ������� ����������.
	private static class Keys
	{
		public const string MoveForward = "Move Forward";
		public const string MoveBackward = "Move Backward";
		public const string MoveLeft = "Move Left";
		public const string MoveRight = "Move Right";
		public const string Run = "Run";
	}

	// �������� �������� ���� ��� ����.
	private static class MouseAxes
	{
		/// <summary>
		/// �������� ��� ��� �������� �����-������.
		/// </summary>
		public const string Yaw = "Yaw";
		/// <summary>
		/// �������� ��� ��� �������� �����-����.
		/// </summary>
		public const string Pitch = "Pitch";
	}

	private void Awake ()
	{
		//_capsuleCollider = gameObject.GetComponent<CapsuleCollider> ();
		//_rigidBody = gameObject.GetComponent<Rigidbody> ();
		_unityController = gameObject.GetComponent<CharacterController> ();

		//if (_capsuleCollider == null)
		//	throw new Exception ("CapsuleCollider �� ������.");
		//if (_rigidBody == null)
		//	throw new Exception ("Rigidbody �� ������.");
		if (fpsCamera == null)
			throw new Exception ("fpsCamera �� ���������.");
		if (_unityController == null)
			throw new Exception ("CharacterController �� ������.");

		_yaw = gameObject.transform.eulerAngles.y * Mathf.Deg2Rad;
		_pitch = fpsCamera.transform.localEulerAngles.x * Mathf.Deg2Rad;
		_cameraInitPosition = fpsCamera.transform.localPosition;
	}

	private void Start ()
	{
		this.slopeLimit = _slopeLimit;

		Cursor.lockState = CursorLockMode.Locked;
	}

	/*private void FixedUpdate ()
	{
		// ��������� � Rigidbody ������� �����-������.
		_rigidBody.MoveRotation (Quaternion.AngleAxis (_yaw * Mathf.Rad2Deg, Vector3.up));

		// ��������� ����������� � Rigidbody.

		bool isMovingSide = _leftKeyIsDown && !_rightKeyIsDown || !_leftKeyIsDown && _rightKeyIsDown;
		bool isMovingAlong = _forwardKeyIsDown && !_backwardKeyIsDown || !_forwardKeyIsDown && _backwardKeyIsDown;

		// ���� �������� ������ ��������.
		Vector3 moveDir = Vector3.zero;
		if (enableMove && (isMovingAlong || isMovingSide))
		{
			if (isMovingAlong)
			{
				moveDir = _forwardKeyIsDown ? transform.forward : -transform.forward;
			}
			if (isMovingSide)
			{
				moveDir += _rightKeyIsDown ? transform.right : -transform.right;
			}
			// ���� �������� �� ���������, ����������� ������ �����������.
			if (isMovingAlong && isMovingSide)
			{
				moveDir.Normalize ();
			}
		}

		float deltaTime = Time.fixedDeltaTime;
		float usedSpeed = _runKeyIsDown ? runSpeed : walkSpeed;

		_isGrounded = CheckIfGrounded ();
		if (_isGrounded)
		{
			_curVelocity.y = 0;
		}
		_curVelocity.x = moveDir.x * usedSpeed;
		_curVelocity.z = moveDir.z * usedSpeed;

		if (_isGrounded)
		{
			_curVelocity = Vector3.ProjectOnPlane (_curVelocity, _maxUpNormal);
		}
		else
		{
			_curVelocity.y += Physics.gravity.y * deltaTime;
		}

		_rigidBody.MovePosition (_rigidBody.transform.position + _curVelocity * deltaTime);
		_rigidBody.velocity = _curVelocity;
	}*/

	private void Update ()
	{
		// ��������� ��������� ������.
		_backwardKeyIsDown = Input.GetButton (Keys.MoveBackward);
		_forwardKeyIsDown = Input.GetButton (Keys.MoveForward);
		_leftKeyIsDown = Input.GetButton (Keys.MoveLeft);
		_rightKeyIsDown = Input.GetButton (Keys.MoveRight);
		_runKeyIsDown = Input.GetButton (Keys.Run);

		// ��������� ���� �������� Yaw � Pitch.
		if (enableLook)
		{
			//_yaw += Input.GetAxisRaw (MouseAxes.Yaw) * turnCoef;
			_yaw += Input.GetAxisRaw (MouseAxes.Yaw) * turnCoef * GlobalManager.instance.mouseSensitivity;
			if (_yaw < 0 || _yaw > Mathf.PI * 2)
			{
				_yaw = Mathf.Repeat (_yaw, Mathf.PI * 2);
			}

			//_pitch -= Input.GetAxisRaw (MouseAxes.Pitch) * turnCoef;
			_pitch -= Input.GetAxisRaw (MouseAxes.Pitch) * turnCoef * GlobalManager.instance.mouseSensitivity;
			if (_pitch < -Mathf.PI * .5)
				_pitch = -Mathf.PI * .5f;
			if (_pitch > Mathf.PI * .5)
				_pitch = Mathf.PI * .5f; 
		}

		// ������� �����-������.
		_unityController.transform.rotation = Quaternion.AngleAxis (_yaw * Mathf.Rad2Deg, Vector3.up);

		// ������� �����-����.
		fpsCamera.transform.localEulerAngles = new Vector3 (_pitch * Mathf.Rad2Deg, 0, 0);

		bool isMovingSide = _leftKeyIsDown && !_rightKeyIsDown || !_leftKeyIsDown && _rightKeyIsDown;
		bool isMovingAlong = _forwardKeyIsDown && !_backwardKeyIsDown || !_forwardKeyIsDown && _backwardKeyIsDown;

		// ���� �������� ������ ��������.
		Vector3 moveDir = Vector3.zero;
		if (enableMove && (isMovingAlong || isMovingSide))
		{
			if (isMovingAlong)
			{
				moveDir = _forwardKeyIsDown ? transform.forward : -transform.forward;
			}
			if (isMovingSide)
			{
				moveDir += _rightKeyIsDown ? transform.right : -transform.right;
			}
			// ���� �������� �� ���������, ����������� ������ �����������.
			if (isMovingAlong && isMovingSide)
			{
				moveDir.Normalize ();
			}
		}

		float deltaTime = Time.deltaTime;
		float usedSpeed = _runKeyIsDown ? runSpeed : walkSpeed;

		if (_unityController.isGrounded && _curVelocity.y < 0f)
		{
			_curVelocity.y = 0f;
		}
		_curVelocity.x = moveDir.x * usedSpeed;
		_curVelocity.z = moveDir.z * usedSpeed;

		// ��������� ���� �������.
		_curVelocity.y += Physics.gravity.y * deltaTime;

		_unityController.Move (_curVelocity * deltaTime);
	}

	/*private bool CheckIfGrounded ()
	{
		bool grounded = false;

		float effectiveHalfHeight = _capsuleCollider.height * .5f - _capsuleCollider.radius;
		Vector3 capsOffset = Vector3.up * effectiveHalfHeight;
		Vector3 capsuleBottom = transform.TransformPoint (_capsuleCollider.center - capsOffset);
		Vector3 capsuleTop = transform.TransformPoint (_capsuleCollider.center + capsOffset);

		int layerMask = ~0;
		Collider[] colliders = Physics.OverlapCapsule (capsuleTop, capsuleBottom, _capsuleCollider.radius, layerMask, QueryTriggerInteraction.Ignore);

		// ���������� "����� ������������" �������.
		_maxUpNormal = Vector3.zero;

		// ���������� �� ���� �����������, ������� ���� ������.
		int numColliders = colliders.Length;
		for (int i = 0; i < numColliders; i++)
		{
			Collider otherCollider = colliders[i];
			if (otherCollider.gameObject == gameObject)
				continue;

			Vector3 direction;
			float distance = 0;
			bool doPenetrate = Physics.ComputePenetration (
				_capsuleCollider, _capsuleCollider.transform.position, _capsuleCollider.transform.rotation,
				otherCollider, otherCollider.transform.position, otherCollider.transform.rotation,
				out direction, out distance
			);

			if (doPenetrate && direction.y >= _groundedNormalMinY)
			{
				grounded = true;

				// ������� "����� ������������ �������".
				if (_maxUpNormal.y < direction.y)
				{
					_maxUpNormal = direction;
				}
			}
		}

		return grounded;
	}*/


	// ==================== PROPERTIES ====================

	/// <summary>
	/// ������������ ����� �����������, �� ������� �������� �������� ������ (� ��������). ���� ������������� �������������� �����������, 90 - ������������.
	/// </summary>
	public float slopeLimit
	{
		get { return _slopeLimit; }
		set
		{
			_slopeLimit = value;
			if (_slopeLimit < 0)
				_slopeLimit = 0;
			if (_slopeLimit > 90)
				_slopeLimit = 90;

			_unityController.slopeLimit = value;
			//_groundedNormalMinY = Mathf.Cos (slopeLimit * Mathf.Deg2Rad);
		}
	}

	/// <summary>
	/// ���� �������� ������ ������������ ��� (� ��������).
	/// </summary>
	public float yaw
	{
		get { return _yaw; }
		set
		{
			if (value < 0 || value > Mathf.PI * 2)
			{
				value = Mathf.Repeat (value, Mathf.PI * 2);
			}
			_yaw = value;

			//_rigidBody.MoveRotation (Quaternion.AngleAxis (_yaw * Mathf.Rad2Deg, Vector3.up));
			_unityController.transform.rotation = Quaternion.AngleAxis (_yaw * Mathf.Rad2Deg, Vector3.up);
		}
	}

	/// <summary>
	/// ���� �������� ������ ������������ ��� (� ��������).
	/// </summary>
	public float yawDegrees
	{
		get { return _yaw * Mathf.Rad2Deg; }
		set { this.yaw = value * Mathf.Deg2Rad; }
	}

	/// <summary>
	/// ���� �������� ������ right-��� (� ��������).
	/// </summary>
	public float pitch
	{
		get { return _pitch; }
		set
		{
			if (value < -Mathf.PI * .5)
				value = -Mathf.PI * .5f;
			if (value > Mathf.PI * .5)
				value = Mathf.PI * .5f;
			_pitch = value;

			fpsCamera.transform.localEulerAngles = new Vector3 (_pitch * Mathf.Rad2Deg, 0, 0);
		}
	}

	public float pitchDegrees
	{
		get { return _pitch * Mathf.Rad2Deg; }
		set { this.pitch = value * Mathf.Deg2Rad; }
	}

}
