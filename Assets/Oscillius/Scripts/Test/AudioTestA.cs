using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTestA : MonoBehaviour
{
	private GlobalManager _globalManager;

	//public AudioSource testAudioSource;
	public OscAudioResourceEnum audioResourceEnum;


	private void Awake ()
	{
		//GameObject globalManagerGO = GameObject.Find ("Global Manager");
		//if (globalManagerGO == null)
		//	throw new System.Exception ("Global Manager not found.");

		//_globalManager = globalManagerGO.GetComponent<GlobalManager> ();
		//if (_globalManager == null)
		//	throw new System.Exception ("Global Manager component not found.");
	}

	private void Start ()
	{
		AudioLayer ambientLayer = GlobalManager.instance.audioManager.GetLayerByType (AudioLayerType.Ambient);
		ambientLayer.introSpeed = .5f;
		ambientLayer.outroSpeed = .5f;
		ambientLayer.IntroduceClip (audioResourceEnum);
	}

}
