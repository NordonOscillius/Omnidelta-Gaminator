using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZaitakuLabyrinthFalseLogic : MonoBehaviour
{

	private void Start ()
	{
		StartDefaultFadeIn ();
		FadeOutAmbience ();
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

}
