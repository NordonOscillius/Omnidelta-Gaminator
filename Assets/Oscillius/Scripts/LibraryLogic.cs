using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LibraryLogic : MonoBehaviour
{
	private GameObject _startPointGO;


	private void Awake ()
	{
		
	}

	private void Start ()
	{
		StartDefaultFadeIn ();
	}


	#region Utils

	// �������� ������ Start Point.
	private void CacheStartPointGO ()
	{
		string usedStartPointName = GlobalManager.instance.playerStartPointName;
		if (usedStartPointName != null && usedStartPointName != "")
		{
			_startPointGO = GameObject.Find (usedStartPointName);
			if (_startPointGO == null)
				throw new System.Exception ("Start Point with name '" + usedStartPointName + "' not found.");
		}
	}

	// ��������� ��������� � ��������� ��������� �����, ���� ��� ������.
	private void PlaceCharacterAtStartPoint ()
	{
		if (_startPointGO != null)
		{
			CharController charController = GlobalManager.instance.alexCamera.transform.root.gameObject.GetComponent<CharController> ();

			charController.transform.position = _startPointGO.transform.position;
			Vector3 targetAngles = _startPointGO.transform.rotation.eulerAngles;
			charController.yawDegrees = targetAngles.y;
			charController.pitchDegrees = targetAngles.x;
		}
	}

	private void StartDefaultFadeIn ()
	{
		// ������ �������� ������ � ��������� Fade In.
		ScreenFader screenFader = GlobalManager.instance.screenFader;
		screenFader.opacity = 1f;
		screenFader.speed = 1f;
		screenFader.StartFadeIn ();
	}

	#endregion

}
