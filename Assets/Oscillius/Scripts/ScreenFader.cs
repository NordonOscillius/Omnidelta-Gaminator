using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader
{
	private float _opacity = 0;
	private float _speed = 1f;
	private ScreenFaderState _state = ScreenFaderState.Inactive;

	public enum ScreenFaderState
	{
		Inactive,
		FadeToBlack,
		FadeIn
	}

	public delegate void Callback ();
	private List<Callback> _onCompleteCallbacks = new List<Callback> ();


	public ScreenFader ()
	{

	}

	public void SetFromBlackScreen ()
	{
		Image blackScreen = GlobalManager.instance.blackScreen;
		if (blackScreen == null)
		{
			Debug.Log ("Black Screen not found.");
			return;
		}
		_opacity = blackScreen.color.a;
	}

	public void ApplyToBlackScreen ()
	{
		Image blackScreen = GlobalManager.instance.blackScreen;
		if (blackScreen == null)
		{
			Debug.Log ("Black Screen not found.");
			return;
		}
		blackScreen.color = new Color (0, 0, 0, _opacity);
	}

	public void StartFadeToBlack (Callback onCompleteCallback = null)
	{
		if (onCompleteCallback != null)
			_onCompleteCallbacks.Add (onCompleteCallback);

		_state = ScreenFaderState.FadeToBlack;
	}

	public void StartFadeToBlack (float startOpacity, Callback onCompleteCallback = null)
	{
		_opacity = startOpacity;
		StartFadeToBlack (onCompleteCallback);
	}

	public void StartFadeIn (Callback onCompleteCallback = null)
	{
		if (onCompleteCallback != null)
			_onCompleteCallbacks.Add (onCompleteCallback);

		_state = ScreenFaderState.FadeIn;
	}

	public void StartFadeIn (float startOpacity, Callback onCompleteCallback = null)
	{
		_opacity = startOpacity;
		StartFadeIn (onCompleteCallback);
	}

	public void Update (float deltaTime)
	{
		if (_state == ScreenFaderState.FadeToBlack)
		{
			_opacity += _speed * deltaTime;
			if (_opacity >= 1)
			{
				_opacity = 1;
				_state = ScreenFaderState.Inactive;

				OnComplete ();
			}
		}
		else if (_state == ScreenFaderState.FadeIn)
		{
			_opacity -= _speed * deltaTime;
			if (_opacity <= 0)
			{
				_opacity = 0;
				_state = ScreenFaderState.Inactive;

				OnComplete ();
			}
		}

		// Применяем альфу к скрину.
		Image blackScreen = GlobalManager.instance.blackScreen;
		if (blackScreen == null)
		{
			Debug.Log ("Black Screen not found.");
			return;
		}
		blackScreen.color = new Color (0, 0, 0, _opacity);
	}

	private void OnComplete ()
	{
		int numCallbacks = _onCompleteCallbacks.Count;
		for (int i = 0; i < numCallbacks; i++)
		{
			_onCompleteCallbacks[i].Invoke ();
		}

		_onCompleteCallbacks.Clear ();
	}


	// ==================== PROPERTIES ====================

	public float opacity
	{
		get { return _opacity; }
		set { _opacity = value; }
	}

	public float speed
	{
		get { return _speed; }
		set { _speed = value; }
	}

}
