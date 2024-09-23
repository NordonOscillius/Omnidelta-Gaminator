using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeInLogic : MonoBehaviour
{

	private void Awake ()
	{
		GlobalManager.instance.screenFader.opacity = 1f;
		GlobalManager.instance.screenFader.speed = 1f;
	}

	private void Start ()
	{
		
	}

}
