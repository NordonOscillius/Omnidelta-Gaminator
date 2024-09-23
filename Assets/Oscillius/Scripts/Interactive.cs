using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Компонент, который должен быть добавлен на каждый интерактивный объект. Необходим для работы Interaction Manager.
/// </summary>
public class Interactive : MonoBehaviour
{
	/// <summary>
	/// Тип взаимодействия. Нужен для определения того, какую иконку использовать при наведении курсора на объект.
	/// </summary>
	public InteractionType interactionType = InteractionType.Open;

	/// <summary>
	/// Показывать ли иконку при наведении курсора на объект.
	/// </summary>
	public bool doShowIcon = true;

}
