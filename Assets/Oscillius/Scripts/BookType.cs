using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BookType
{
	/// <summary>
	/// "�������" ��� ����� ����� Undefined. �������� �� ��������� ��� ����������� Readable-��������.
	/// </summary>
	Common,
	/// <summary>
	/// ����� ��� ��������� �������.
	/// </summary>
	InvisibleInkBook,
	/// <summary>
	/// ������������� ������� ����� ��������� ����������.
	/// </summary>
	LibraryMapInvisible,
	/// <summary>
	/// ����������� ������� ����� ��������� ����������.
	/// </summary>
	LibraryMapVisible,
	/// <summary>
	/// ����� � ������������ ���������� �������.
	/// </summary>
	LibraryBook,
	/// <summary>
	/// ����� ����� � ���, ��� ���������� ������ � ������� � �����.
	/// </summary>
	AlphaBook
}
