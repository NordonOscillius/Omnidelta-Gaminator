using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���������, ������� ������ ���� �������� �� ������ ������������� ������. ��������� ��� ������ Interaction Manager.
/// </summary>
public class Interactive : MonoBehaviour
{
	/// <summary>
	/// ��� ��������������. ����� ��� ����������� ����, ����� ������ ������������ ��� ��������� ������� �� ������.
	/// </summary>
	public InteractionType interactionType = InteractionType.Open;

	/// <summary>
	/// ���������� �� ������ ��� ��������� ������� �� ������.
	/// </summary>
	public bool doShowIcon = true;

}
