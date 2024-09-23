using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// НЕ ЗАБУДЬ НАЗНАЧИТЬ ОБЪЕКТ НА СЛОЙ INTERACTIVE!!!!!!!!!!
/// </summary>
public class Readable : MonoBehaviour
{
	[Tooltip ("Расстояние до камеры, на котором оказывается эта книга или заметка при чтении.")]
	public float readingDistance = .5f;
	public Vector3 readingDeltaAngles = Vector3.zero;
	[Tooltip ("Тип книги или заметки. Если Readable не относится к чему-то конкретному, то BookType.Common.")]
	public BookType bookType = BookType.Common;
	[Tooltip ("Объект с аудиосорсом, воспроизводящимся при взятии этой книги или заметки.")]
	public GameObject takeReadableSoundGO;
	[Tooltip ("Объект с аудиосорсом, воспроизводящимся, когда эта книга или заметка кладется обратно.")]
	public GameObject putReadableSoundGO;
	[Tooltip ("Коллайдер, указанный явным образом. Если не задан, то поиск коллайдера будет вестись в объекте, к которому добавлен скрипт.")]
	public BoxCollider colliderOverriden;

	private BoxCollider _myCollider;
	private Interactive _myInteractive;
	private bool _prevInteractionIsPossible = false;
	private CharController _charController;
	private Image _circleReticle;

	private Vector3 _normalPosition;
	private Quaternion _normalRotation;

	private float _positionStiffness = .4f;
	private float _rotationStiffness = 1f;
	// Время, по истечении которого в режиме Normal объект перестанет просчитывать свою трансформацию и примет исходное положение (_normalPosition, _normalRotation).
	private float _stiffnessTime = .7f;
	// Время, прошедшее с момент переключения из режима Reading в режим Normal.
	private float _curStiffnessTime = 0;

	private ReadableState _state = ReadableState.Normal;


	private void Awake ()
	{
		if (colliderOverriden == null)
		{
			_myCollider = GetComponent<BoxCollider> ();
			if (_myCollider == null)
			{
				throw new Exception ("BoxCollider not found.");
			} 
		}
		else
		{
			_myCollider = colliderOverriden;
		}

		_normalPosition = transform.position;
		_normalRotation = transform.rotation;

		_myInteractive = GetComponent<Interactive> ();
		if (_myInteractive == null)
		{
			throw new Exception ("Interactive component not found.");
		}

		GameObject circleReticleGO = GameObject.Find ("Circle Reticle");
		if (circleReticleGO != null)
		{
			_circleReticle = circleReticleGO.GetComponent<Image> ();
		}
	}

	private void Start ()
	{
		_charController = GlobalManager.instance.alexCamera.transform.root.GetComponent<CharController> ();
	}

	private void Update ()
	{
		if (_state == ReadableState.Normal)
			UpdateNormal ();
		else if (_state == ReadableState.Reading)
			UpdateReading ();
	}

	//private void LateUpdate ()
	private void FixedUpdate ()
	{
		// В режиме Reading всегда обновляем положение объекта. В режиме Normal - только до истечения _stiffnessTime секунд.
		if (_state != ReadableState.Normal || _curStiffnessTime > 0)
		{
			UpdateTransform (GlobalManager.instance.alexCamera, Time.fixedDeltaTime);
		}
	}

	private void UpdateNormal ()
	{
		_curStiffnessTime -= Time.deltaTime;

		GlobalManager globalManager = GlobalManager.instance;
		InteractionManager interactionManager = globalManager.interactionManager;

		if (interactionManager.intersectionFound && interactionManager.hitInfo.collider == _myCollider)
		{
			// При взаимодействии ("Читать") переключаем стейт, прячем иконку.
			if (Input.GetKeyDown (interactionManager.interactKeyCode))
			{
				_state = ReadableState.Reading;
				_myInteractive.doShowIcon = false;

				// Лочим камеру и запрещаем двигаться.
				if (_charController != null)
				{
					_charController.enableLook = false;
					_charController.enableMove = false;
				}
				// Прячем прицел.
				if (_circleReticle != null)
					_circleReticle.enabled = false;

				// Говорим глобальному менеджеру, что прочитали определенный тип книги (если эта книга относится к определенному типу).
				globalManager.OnBookHasBeenRead (bookType);

				// Воспроизводим звук поднимаемого листа бумаги, если он назначен.
				if (takeReadableSoundGO != null)
				{
					AudioSource takeReadableAudioSource = takeReadableSoundGO.GetComponent<AudioSource> ();
					if (takeReadableAudioSource != null)
					{
						AudioSource.PlayClipAtPoint (takeReadableAudioSource.clip, _normalPosition, takeReadableAudioSource.volume);
						//AudioSource.PlayClipAtPoint (takeReadableAudioSource.clip, gameObject.transform.position);
						//takeReadableAudioSource.Play ();
					}
				}
			}
		}
	}

	private void UpdateNormalOld (float deltaTime)
	{
		_curStiffnessTime -= deltaTime;

		GlobalManager globalManager = GlobalManager.instance;
		Camera alexCamera = globalManager.alexCamera;
		int interactiveLayer = LayerMask.NameToLayer (globalManager.interactiveLayerName);
		if (interactiveLayer < 0)
		{
			throw new Exception ("Interactive Layer not found by its name.");
		}
		int interactiveLayerMask = 1 << interactiveLayer;

		RaycastHit hitInfo;
		bool intersectionFound = Physics.Raycast (
			alexCamera.transform.position,
			alexCamera.transform.forward,
			out hitInfo,
			globalManager.maxInteractionDistance,
			interactiveLayerMask,
			QueryTriggerInteraction.Collide
		);

		// Если игрок целится в этот объект.
		if (intersectionFound && hitInfo.collider == _myCollider)
		{
			//if (!_prevInteractionIsPossible)
			//{
			//	globalManager.ShowInteractionIcon (InteractionType.Read);
			//}
			globalManager.ShowInteractionIcon (InteractionType.Read);

			if (Input.GetKeyDown (KeyCode.E))
			{
				_state = ReadableState.Reading;
			}
		}
		// Если игрок не целится.
		else
		{
			//if (_prevInteractionIsPossible)
			//{
			//	globalManager.HideInteractionIcon (InteractionType.Read);
			//}
			globalManager.HideInteractionIcon (InteractionType.Read);
		}

		_prevInteractionIsPossible = intersectionFound;

		//UpdateTransform (alexCamera, deltaTime);
	}

	private void UpdateReading ()
	{
		InteractionManager interactionManager = GlobalManager.instance.interactionManager;
		if (Input.GetKeyDown (interactionManager.interactKeyCode))
		{
			_state = ReadableState.Normal;
			_curStiffnessTime = _stiffnessTime;
			_myInteractive.doShowIcon = true;

			// Разрешаем осматриваться и двигаться.
			if (_charController != null)
			{
				_charController.enableLook = true;
				_charController.enableMove = true;
			}
			// Показываем прицел.
			if (_circleReticle != null)
				_circleReticle.enabled = true;

			// Воспроизводим звук отпускаемого листа бумаги, если он назначен.
			if (putReadableSoundGO != null)
			{
				AudioSource putReadableAudioSource = putReadableSoundGO.GetComponent<AudioSource> ();
				if (putReadableAudioSource != null)
				{
					AudioSource.PlayClipAtPoint (putReadableAudioSource.clip, _normalPosition, putReadableAudioSource.volume);
					//AudioSource.PlayClipAtPoint (putReadableAudioSource.clip, gameObject.transform.position);
					//putReadableAudioSource.Play ();
				}
			}
		}
	}

	private void UpdateReadingOld (float deltaTime)
	{
		// Во время чтения прячем иконку "читать" в любом случае.
		GlobalManager.instance.HideInteractionIcon (InteractionType.Read);

		if (Input.GetKeyDown (KeyCode.E))
		{
			_state = ReadableState.Normal;

			// В момент переключения из Reading в Normal сбрасываем stiffness-время.
			_curStiffnessTime = _stiffnessTime;
		}

		//UpdateTransform (GlobalManager.instance.alexCamera, deltaTime);
	}

	private void UpdateTransform (Camera alexCamera, float deltaTime)
	{
		Vector3 targetPosition = Vector3.zero;
		Quaternion targetRotation = Quaternion.identity;

		if (_state == ReadableState.Normal)
		{
			targetPosition = _normalPosition;
			targetRotation = _normalRotation;
		}
		else if (_state == ReadableState.Reading)
		{
			targetPosition = alexCamera.transform.position + alexCamera.transform.forward * readingDistance;

			targetRotation = Quaternion.LookRotation (alexCamera.transform.forward, alexCamera.transform.up);
			Quaternion deltaEuler = Quaternion.Euler (readingDeltaAngles);
			targetRotation = targetRotation * deltaEuler;
		}

		transform.position = Vector3.Lerp (transform.position, targetPosition, _positionStiffness);
		transform.rotation = Quaternion.Lerp (transform.rotation, targetRotation, _rotationStiffness);

		//transform.position = targetPosition;
		//transform.rotation = targetRotation;
	}


	// ====================================================
	// ==================== PROPERTIES ====================
	// ====================================================

	public ReadableState state { get { return _state; } }

}
