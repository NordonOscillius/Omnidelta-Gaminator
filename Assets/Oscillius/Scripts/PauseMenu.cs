using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
	[Tooltip ("Объект, храняший Canvas с меню паузы.")]
	public GameObject pauseMenuCanvasGO;
	[Tooltip ("Объект, хранящий кнопку 'Выйти из игры.'")]
	public GameObject exitGameButtonGO;
	[Tooltip ("Объект, хранящий ТЕКСТ кнопки 'Качество'.")]
	public GameObject qualityTextGO;
	[Tooltip ("Объект, хранящий ТЕКСТ кнопки 'Разрешение'.")]
	public GameObject resolutionTextGO;
	[Tooltip ("Объект, хранящий слайдер 'Чувствительность мыши'.")]
	public GameObject sensitivitySliderGO;
	[Tooltip ("Объект, хранящий текст над слайдером 'Чувствительность мыши'.")]
	public GameObject sensitivityTextGO;
	[Tooltip ("Объект, хранящий слайдер 'Дальность прорисовки травы'.")]
	public GameObject grassDistanceSliderGO;
	[Tooltip ("Объект, хранящий текст над слайдером 'Дальность прорисовки травы'.")]
	public GameObject grassDistanceTextGO;
	[Tooltip ("Объект, хранящий слайдер 'Плотность травы'.")]
	public GameObject grassDensitySliderGO;
	[Tooltip ("Объект, хранящий текст над слайдером 'Плотность травы'.")]
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

	// Террейн, активный на данный момент. Устанавливается Logic-классами.
	private Terrain _terrain;


	private void Awake ()
	{
		// Отключаем меню паузы (если только в начале игры не выбираем настройки качества).
		//if (GlobalManager.instance.storyState != StoryState.QualitySelect)
		//{
		//	pauseMenuCanvasGO.SetActive (false); 
		//}

		// Лочим и прячем курсор.
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

		// Кэшируем _qualityText.
		_qualityText = qualityTextGO.GetComponent<Text> ();
		// Передаем название текущего Quality Level в текст кнопки.
		_qualityText.text = QualitySettings.names[QualitySettings.GetQualityLevel ()];

		// Кэшируем _resolutionText.
		_resolutionText = resolutionTextGO.GetComponent<Text> ();
		// Передаем название текущего разрешения в текст кнопки.
		_resolutionText.text = GetResolutionString (Screen.currentResolution);
		// Запоминаем все поддерживаемые разрешения и индекс текущего разрешения.
		_resolutions = new List<Resolution> (Screen.resolutions);
		_curResolutionIndex = _resolutions.IndexOf (Screen.currentResolution);

		// Ставим ползунок на слайдере в положение, соответствующее чувствительности мыши. Обновляем текст над слайдером.
		_sensitivitySlider = sensitivitySliderGO.GetComponent<Slider> ();
		//_sensitivitySlider.value = GlobalManager.instance.mouseSensitivity;
		_sensitivityText = sensitivityTextGO.GetComponent<Text> ();
		//_sensitivityText.text = GetSensitivityText (GlobalManager.instance.mouseSensitivity);

		// Ставим ползунок на слайдере в положение, соответствующее дальности прорисовки травы. Обновляем текст над слайдером.
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
		return "Чувствительность мыши: " + sensitivityValue.ToString ("F1");
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
		return "Дальность прорисовки травы: " + grassDistanceValue.ToString ("F0");
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
		return "Плотность травы: " + grassDensityValue.ToString ("F2");
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
