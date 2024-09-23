using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager
{
	private RaycastHit _hitInfo;
	private bool _intersectionFound = false;
	private bool _isEnabled = true;
	private KeyCode _interactKeyCode = KeyCode.E;


	public InteractionManager ()
	{

	}

	public void Update ()
	{
		// Если менеджер выключен, все взаимодействия будут игнорироваться.
		if (!_isEnabled)
		{
			_intersectionFound = false;
			return;
		}

		GlobalManager globalManager = GlobalManager.instance;

		Camera alexCamera = globalManager.alexCamera;
		if (alexCamera == null)
			return;

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

		_intersectionFound = intersectionFound;
		_hitInfo = hitInfo;

		// Показываем или прячем иконку.
		if (intersectionFound)
		{
			GameObject hitGameObject = hitInfo.transform.gameObject;
			if (hitGameObject != null)
			{
				Interactive interactive = hitGameObject.GetComponent<Interactive> ();

				// Если компонент Interactive не найден в hit-объекте, то ищем корневой объект и пытаемся найти Interactive на нем.
				if (interactive == null)
				{
					GameObject hitRootGameObject = hitGameObject.transform.root.gameObject;
					if (hitRootGameObject != null)
					{
						interactive = hitRootGameObject.GetComponent<Interactive> ();
					}
				}

				if (interactive != null)
				{
					if (interactive.doShowIcon)
					{
						globalManager.ShowInteractionIcon (interactive.interactionType);
					}
					else
					{
						globalManager.HideAllInteractionIcons ();
					}
				}
			}
		}
		else
		{
			globalManager.HideAllInteractionIcons ();
		}
	}


	// ====================================================
	// ==================== PROPERTIES ====================
	// ====================================================

	/// <summary>
	/// Информация о рейкасте, произошедшем на интерактивном слое при последнем обновлении менеджера. Рейкаст учитывает триггеры.
	/// </summary>
	public RaycastHit hitInfo
	{
		get { return _hitInfo; }
	}

	/// <summary>
	/// Было ли при последнем обновлении найдено пересечение рейкаст-луча с одним из коллайдеров, расположенных на интерактивном слое. Рейкаст учитывает триггеры.
	/// </summary>
	public bool intersectionFound
	{
		get { return _intersectionFound; }
	}

	/// <summary>
	/// Включен ли менеджер. Выключенный менеджер не выполняет рейкасты, и его intersectionFound установлено в false.
	/// </summary>
	public bool isEnabled
	{
		get { return _isEnabled; }
		set { _isEnabled = value; }
	}

	public KeyCode interactKeyCode
	{
		get { return _interactKeyCode; }
	}

}
