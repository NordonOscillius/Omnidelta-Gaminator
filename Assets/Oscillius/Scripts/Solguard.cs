using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Solguard : MonoBehaviour
{
	private Collider _collider;
	private Interactive _interactive;
	// ��� �� ���� ������ ������?
	private bool _isGathered = false;

	//private InteractionType _interactionType = InteractionType.PickFlower;


	private void Awake ()
	{
		// ���� ���� ������ ��� ��� ������, ���������� ��� ��� ��������.
		if (GlobalManager.instance.collectedSolguardNames.Contains (gameObject.name))
		{
			_isGathered = true;
			Destroy (gameObject);
			return;
		}

		_collider = GetComponent<Collider> ();
		if (_collider == null)
			throw new System.Exception ("Collider not found.");

		_interactive = GetComponent<Interactive> ();
		if (_interactive == null)
			throw new System.Exception ("Interactive component not found.");
	}

	private void Update ()
	{
		// �� ������ ������.
		if (_isGathered)
			return;

		// ������� �� ���� ������, ��� ������� ������� ����������.
		// ������� ����� ������� �� ���� ������, ������� � "������ �� ���������".
		StoryState storyState = GlobalManager.instance.storyState;
		if (storyState != StoryState.SolguardHike)
		{
			return;
		}

		// ���� ������� �������� ����, ��������� ������ �� ���� ����������� ��������.
		if (GlobalManager.instance.collectedSolguardNames.Count >= 20)
		{
			_interactive.doShowIcon = false;
		}

		if (GlobalManager.instance.interactionManager.intersectionFound)
		{
			RaycastHit hitInfo = GlobalManager.instance.interactionManager.hitInfo;
			if (hitInfo.collider == _collider)
			{
				// ���� ����� �������� ������ ��������, ������� ������ � ������ ������.
				if (Input.GetKeyDown (GlobalManager.instance.interactionManager.interactKeyCode))
				{
					// ������ ���� ���������� ������.
					AudioSource audioSource = GetComponent<AudioSource> ();
					AudioSource.PlayClipAtPoint (audioSource.clip, transform.position, audioSource.volume);

					//GlobalManager.instance.HideInteractionIcon (_interactionType);
					Destroy (gameObject);
					GlobalManager.instance.collectedSolguardNames.Add (gameObject.name);
					_isGathered = true;
				}
				//else
				//{
				//	GlobalManager.instance.ShowInteractionIcon (_interactionType);
				//}
			}
		}
		//else
		//{
		//	GlobalManager.instance.HideInteractionIcon (_interactionType);
		//}
	}

}
