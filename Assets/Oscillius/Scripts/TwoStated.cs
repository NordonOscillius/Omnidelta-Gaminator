using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Поведение анимированной детали, которая может находиться в одном из двух состояний.
/// </summary>
public class TwoStated : MonoBehaviour
{
	[Tooltip ("Объект, положение которого стремится принять эта деталь при state = false.")]
	public GameObject falseTransformGO;
	[Tooltip ("Объект, положение которого стремится принять эта деталь при state = true.")]
	public GameObject trueTransformGO;
	[Tooltip ("Скорость анимации - значение от нуля до единицы")]
	public float stiffness = .2f;
	[Tooltip ("За какой отрезок времени анимация перестанет просчитываться и деталь займет целевое положение.")]
	public float animTime = 1.5f;
	public bool animatePosition = false;
	public bool animateRotation = true;

	// Состояние детали. True используется для открытого/поднятого/полного состояния, false - для закрытого/опущенного/пустого.
	private bool _state;
	// Момент времени Time.time, в которое началась анимация.
	private float _animStartTime = 0f;
	// Запущена ли анимация.
	private bool _animInProgress = false;

	// Стандартное время, для которого задается stiffness.
	private float _standardDeltaTime = 1 / 60f;


	private void Update ()
	{
		// Если анимация не воспроизводится, выходим.
		if (!_animInProgress)
			return;

		Transform targetTF = _state ? trueTransformGO.transform : falseTransformGO.transform;

		// Если анимации пора завершаться, просто снэппим наш объект к целевому.
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
		// Делаем анимацию линейной интерполяцией.
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

	// Вызывается при смене состояния.
	private void OnStateFlipped ()
	{
		_animStartTime = Time.time;
		_animInProgress = true;
	}

	/// <summary>
	/// Применяет целевую трансформацию к детали, не дожидаясь завершения анимации.
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
	/// Запущена ли анимация.
	/// </summary>
	public bool animationInProgress { get { return _animInProgress; } }

}
