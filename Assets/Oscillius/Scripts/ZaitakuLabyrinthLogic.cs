using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZaitakuLabyrinthLogic : MonoBehaviour
{
	// Кэшированная ссылка на объект, представляющий Start Point.
	private GameObject _startPointGO;


	private void Awake ()
	{
		CacheStartPointGO ();
	}

	private void Start ()
	{
		StartDefaultFadeIn ();
		PlaceCharacterAtStartPoint ();
		FadeOutAmbience ();
	}


	#region Utils

	// Кэширует объект Start Point.
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

	// Размещает персонажа в положении начальной точки, если она задана.
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

	private void FadeOutAmbience ()
	{
		AudioLayer ambientLayer = GlobalManager.instance.audioManager.GetLayerByType (AudioLayerType.Ambient);
		ambientLayer.outroSpeed = 2f;
		ambientLayer.FadeOut ();
	}

	private void StartDefaultFadeIn ()
	{
		// Делаем картинку черной и запускаем Fade In.
		ScreenFader screenFader = GlobalManager.instance.screenFader;
		screenFader.opacity = 1f;
		screenFader.speed = 1f;
		screenFader.StartFadeIn ();
	}

	#endregion

}
