using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Bootstrap нужен только для создания и инициализации глобального менеджера и других похожих объектов, которые должны быть синглтонами.
/// </summary>
public class Bootstrap : MonoBehaviour
{
	[Tooltip ("Название сцены, которая должна быть загружена следующей.")]
	public string startupSceneName;


	private void Awake ()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable ()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded (Scene scene, LoadSceneMode mode)
	{
		if (startupSceneName != null && startupSceneName != "")
		{
			StartCoroutine (LoadStartupScene ());
		}
	}

	private IEnumerator LoadStartupScene ()
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync (startupSceneName);

		while (!asyncLoad.isDone)
		{
			yield return null;
		}
	}

}
