using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BookType
{
	/// <summary>
	/// "Обычный" тип чтива вроде Undefined. Значение по умолчанию для большинства Readable-объектов.
	/// </summary>
	Common,
	/// <summary>
	/// Книга про невидимые чернила.
	/// </summary>
	InvisibleInkBook,
	/// <summary>
	/// Непроявленный вариант карты Лабиринта Библиотеки.
	/// </summary>
	LibraryMapInvisible,
	/// <summary>
	/// Проявленный вариант карты Лабиринта Библиотеки.
	/// </summary>
	LibraryMapVisible,
	/// <summary>
	/// Книга о Разрозненной Библиотеке Зайтаку.
	/// </summary>
	LibraryBook,
	/// <summary>
	/// Книга Альфы о том, как уничтожить ячейку и попасть в Альфу.
	/// </summary>
	AlphaBook
}
