using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
	[Tooltip ("������, �������� Canvas � ���� �����.")]
	public GameObject pauseMenuCanvasGO;
	[Tooltip ("������, �������� ������ '����� �� ����.'")]
	public GameObject exitGameButtonGO;
	[Tooltip ("������, �������� ����� ������ '��������'.")]
	public GameObject qualityTextGO;
	[Tooltip ("������, �������� ����� ������ '����������'.")]
	public GameObject resolutionTextGO;
	[Tooltip ("������, �������� ������� '���������������� ����'.")]
	public GameObject sensitivitySliderGO;
	[Tooltip ("������, �������� ����� ��� ��������� '���������������� ����'.")]
	public GameObject sensitivityTextGO;
	[Tooltip ("������, �������� ������� '��������� ���������� �����'.")]
	public GameObject grassDistanceSliderGO;
	[Tooltip ("������, �������� ����� ��� ��������� '��������� ���������� �����'.")]
	public GameObject grassDistanceTextGO;
	[Tooltip ("������, �������� ������� '��������� �����'.")]
	public GameObject grassDensitySliderGO;
	[Tooltip ("������, �������� ����� ��� ��������� '��������� �����'.")]
	public GameObject grassDensityTextGO;

	public static bool isPaused = false;

	private Text _qualityText;
	private Text _resolutionText;
	private List<Resolution> _resolutions;
	private int _curResolutionIndex = -1;
	private Slider _sensitivitySlider;
	private Text _sensitivityText;
	private Slider _grassDistanceSlider;
	private Text _grassDistanceText;
	private Slider _grassDensitySlider;
	private Text _grassDensityText;

	// �������, �������� �� ������ ������. ��������������� Logic-��������.
	private Terrain _terrain;


	private void Awake ()
	{
		// ��������� ���� ����� (���� ������ � ������ ���� �� �������� ��������� ��������).
		//if (GlobalManager.instance.storyState != StoryState.QualitySelect)
		//{
		//	pauseMenuCanvasGO.SetActive (false); 
		//}

		// ����� � ������ ������.
		if (GlobalManager.instance == null || GlobalManager.instance.storyState == StoryState.MainMenu)
		{
			return;
		}

		//if (GlobalManager.instance.storyState == StoryState.QualitySelect)
		//{
		//	Cursor.lockState = CursorLockMode.None;
		//	pauseMenuCanvasGO.SetActive (true);

		//	Debug.Log ("OK");
		//}
		//else
		//{
		//	Cursor.lockState = CursorLockMode.Locked;
		//}

		Cursor.lockState = CursorLockMode.Locked;

		// �������� _qualityText.
		_qualityText = qualityTextGO.GetComponent<Text> ();
		// �������� �������� �������� Quality Level � ����� ������.
		_qualityText.text = QualitySettings.names[QualitySettings.GetQualityLevel ()];

		// �������� _resolutionText.
		_resolutionText = resolutionTextGO.GetComponent<Text> ();
		// �������� �������� �������� ���������� � ����� ������.
		_resolutionText.text = GetResolutionString (Screen.currentResolution);
		// ���������� ��� �������������� ���������� � ������ �������� ����������.
		_resolutions = new List<Resolution> (Screen.resolutions);
		_curResolutionIndex = _resolutions.IndexOf (Screen.currentResolution);

		// ������ �������� �� �������� � ���������, ��������������� ���������������� ����. ��������� ����� ��� ���������.
		_sensitivitySlider = sensitivitySliderGO.GetComponent<Slider> ();
		//_sensitivitySlider.value = GlobalManager.instance.mouseSensitivity;
		_sensitivityText = sensitivityTextGO.GetComponent<Text> ();
		//_sensitivityText.text = GetSensitivityText (GlobalManager.instance.mouseSensitivity);

		// ������ �������� �� �������� � ���������, ��������������� ��������� ���������� �����. ��������� ����� ��� ���������.
		_grassDistanceSlider = grassDistanceSliderGO.GetComponent<Slider> ();
		//_grassDistanceSlider.value = GlobalManager.instance.grassDistance;
		_grassDistanceText = grassDistanceTextGO.GetComponent<Text> ();
		//_grassDistanceText.text = GetGrassDistanceText (GlobalManager.instance.grassDistance);

		_grassDensitySlider = grassDensitySliderGO.GetComponent<Slider> ();
		_grassDensityText = grassDensityTextGO.GetComponent<Text> ();
	}

	private void Start ()
	{
		if (GlobalManager.instance.storyState == StoryState.QualitySelect)
		{
			Cursor.lockState = CursorLockMode.None;
			pauseMenuCanvasGO.SetActive (true);
		}

		_sensitivitySlider.value = GlobalManager.instance.mouseSensitivity;
		_sensitivityText.text = GetSensitivityText (GlobalManager.instance.mouseSensitivity);

		_grassDistanceSlider.value = GlobalManager.instance.grassDistance;
		_grassDistanceText.text = GetGrassDistanceText (GlobalManager.instance.grassDistance);

		_grassDensitySlider.value = GlobalManager.instance.grassDensity;
		_grassDensityText.text = GetGrassDensityText (GlobalManager.instance.grassDensity);
	}

	private void Update ()
	{
		if (GlobalManager.instance == null ||
			GlobalManager.instance.storyState == StoryState.MainMenu ||
			GlobalManager.instance.storyState == StoryState.QualitySelect
		)
		{
			return;
		}

		//if (Input.GetKeyDown (KeyCode.O))
		if (Input.GetKeyDown (KeyCode.Escape))
		{
			isPaused = !isPaused;

			if (isPaused)
			{
				pauseMenuCanvasGO.SetActive (true);
				Time.timeScale = 0;
				Cursor.lockState = CursorLockMode.None;

				CharController charCtrl = GlobalManager.instance.alexCamera.transform.root.GetComponent<CharController> ();
				charCtrl.enableLook = false;
				charCtrl.enableMove = false;
			}
			else
			{
				pauseMenuCanvasGO.SetActive (false);
				Time.timeScale = 1;
				Cursor.lockState = CursorLockMode.Locked;

				CharController charCtrl = GlobalManager.instance.alexCamera.transform.root.GetComponent<CharController> ();
				charCtrl.enableLook = true;
				charCtrl.enableMove = true;
			}
		}
	}

	public void ExitGame ()
	{
		Application.Quit ();
	}

	public void OnQualityDownClicked ()
	{
		int curLevel = QualitySettings.GetQualityLevel ();
		if (curLevel > 0)
		{
			curLevel--;
			QualitySettings.SetQualityLevel (curLevel);
			_qualityText.text = QualitySettings.names[curLevel];
		}
	}

	public void OnQualityUpClicked ()
	{
		int curLevel = QualitySettings.GetQualityLevel ();
		string[] levelNames = QualitySettings.names;
		if (curLevel < levelNames.Length - 1)
		{
			curLevel++;
			QualitySettings.SetQualityLevel (curLevel);
			_qualityText.text = QualitySettings.names[curLevel];
		}
	}

	public void OnResolutionDownClicked ()
	{
		if (_curResolutionIndex > 0)
		{
			_curResolutionIndex--;
			Resolution curResolution = _resolutions[_curResolutionIndex];
			Screen.SetResolution (curResolution.width, curResolution.height, FullScreenMode.ExclusiveFullScreen);

			_resolutionText.text = GetResolutionString (curResolution);
		}
	}

	public void OnResolutionUpClicked ()
	{
		if (_curResolutionIndex < _resolutions.Count - 1)
		{
			_curResolutionIndex++;
			Resolution curResolution = _resolutions[_curResolutionIndex];
			Screen.SetResolution (curResolution.width, curResolution.height, FullScreenMode.ExclusiveFullScreen);

			_resolutionText.text = GetResolutionString (curResolution);
		}
	}

	private string GetResolutionString (Resolution resolution)
	{
		StringBuilder sb =
			new StringBuilder ()
			.Append (resolution.width)
			.Append (" x ")
			.Append (resolution.height)
		;
		return sb.ToString ();
	}

	public void OnSensitivitySliderValueChanged (float value)
	{
		GlobalManager.instance.mouseSensitivity = value;
		_sensitivityText.text = GetSensitivityText (value);
	}

	private string GetSensitivityText (float sensitivityValue)
	{
		return "���������������� ����: " + sensitivityValue.ToString ("F1");
	}

	public void OnGrassDistanceSliderValueChanged (float value)
	{
		GlobalManager.instance.grassDistance = value;
		_grassDistanceText.text = GetGrassDistanceText (value);

		if (_terrain != null)
			_terrain.detailObjectDistance = value;
	}

	private string GetGrassDistanceText (float grassDistanceValue)
	{
		return "��������� ���������� �����: " + grassDistanceValue.ToString ("F0");
	}

	public void OnGrassDensitySliderValueChanged (float value)
	{
		GlobalManager.instance.grassDensity = value;
		_grassDensityText.text = GetGrassDensityText (value);

		if (_terrain != null)
			_terrain.detailObjectDensity = value;
	}

	private string GetGrassDensityText (float grassDensityValue)
	{
		return "��������� �����: " + grassDensityValue.ToString ("F2");
	}


	// ====================================================
	// ==================== PROPERTIES ====================
	// ====================================================

	public Terrain terrain
	{
		get { return _terrain; }
		set { _terrain = value; }
	}

}
