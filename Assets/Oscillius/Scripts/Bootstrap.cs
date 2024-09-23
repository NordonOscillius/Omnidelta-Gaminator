using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Bootstrap ����� ������ ��� �������� � ������������� ����������� ��������� � ������ ������� ��������, ������� ������ ���� �����������.
/// </summary>
public class Bootstrap : MonoBehaviour
{
	[Tooltip ("�������� �����, ������� ������ ���� ��������� ���������.")]
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
