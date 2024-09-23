using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Solguard : MonoBehaviour
{
	private Collider _collider;
	private Interactive _interactive;
	// Был ли этот цветок сорван?
	private bool _isGathered = false;

	//private InteractionType _interactionType = InteractionType.PickFlower;


	private void Awake ()
	{
		// Если этот цветок уже был сорван, уничтожаем его при загрузке.
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
		// На всякий случай.
		if (_isGathered)
			return;

		// Выходим во всех сценах, где солгард собрать невозможно.
		// Солгард можно собрать во всех сценах, начиная с "Похода за солгардом".
		StoryState storyState = GlobalManager.instance.storyState;
		if (storyState != StoryState.SolguardHike)
		{
			return;
		}

		// Если собрали двадцать штук, выключаем иконки на всех экземплярах солгарда.
		if (GlobalManager.instance.collectedSolguardNames.Count >= 20)
		{
			_interactive.doShowIcon = false;
		}

		if (GlobalManager.instance.interactionManager.intersectionFound)
		{
			RaycastHit hitInfo = GlobalManager.instance.interactionManager.hitInfo;
			if (hitInfo.collider == _collider)
			{
				// Если игрок нажимает кнопку действия, срываем цветок и прячем иконку.
				if (Input.GetKeyDown (GlobalManager.instance.interactionManager.interactKeyCode))
				{
					// Играем звук сорванного цветка.
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
