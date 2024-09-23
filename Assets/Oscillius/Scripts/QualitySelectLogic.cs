using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QualitySelectLogic : MonoBehaviour
{
	public string nextSceneName;


	private void Awake ()
	{
		GlobalManager.instance.storyState = StoryState.QualitySelect;
	}

	public void OnOkClicked ()
	{
		StartCoroutine (LoadMainMenuScene ());
	}

	private IEnumerator LoadMainMenuScene ()
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync (nextSceneName);
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
	}

}
