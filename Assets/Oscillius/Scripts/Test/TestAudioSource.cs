using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAudioSource : MonoBehaviour
{
	private static TestAudioSource _instance;

	private GlobalManager _globalManager;
	public int numAmbientClips = 0;


	private void Awake ()
	{
		DontDestroyOnLoad (gameObject);

		//if (_instance == null)
		//{
		//	_instance = this;
		//}
		//else if (_instance != this)
		//{
		//	Destroy (gameObject);
		//}

		GameObject globalManagerGO = GameObject.Find ("Global Manager");
		if (globalManagerGO == null)
			throw new System.Exception ("Global Manager not found.");

		_globalManager = globalManagerGO.GetComponent<GlobalManager> ();
		if (_globalManager == null)
			throw new System.Exception ("Global Manager component not found.");
	}

	private void Update ()
	{
		numAmbientClips = _globalManager.audioManager.GetLayerByType (AudioLayerType.Ambient).numClips;
	}

}
