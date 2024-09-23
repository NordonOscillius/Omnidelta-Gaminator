using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GaminatorLogic : MonoBehaviour
{
	[Tooltip ("Название сцены, которая будет загружена после клика.")]
	public string nextSceneName;

	private bool _isFadingToBlack = false;


	private void Start ()
	{
		ScreenFader screenFader = GlobalManager.instance.screenFader;
		screenFader.opacity = 1f;
		screenFader.speed = 2f;
		screenFader.StartFadeIn ();
	}

	private void Update ()
	{
		if (_isFadingToBlack)
			return;

		if (Input.GetMouseButtonDown (0) || Input.anyKeyDown)
		{
			_isFadingToBlack = true;

			ScreenFader screenFader = GlobalManager.instance.screenFader;
			screenFader.speed = 2f;
			screenFader.StartFadeToBlack (OnFadedOut);
		}
	}

	private void OnFadedOut ()
	{
		StartCoroutine (LoadNextScene ());
	}

	private IEnumerator LoadNextScene ()
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync (nextSceneName);

		while (!asyncLoad.isDone)
		{
			yield return null;
		}
	}

}
