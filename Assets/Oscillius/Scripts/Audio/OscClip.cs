using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OscClip
{
	private OscAudioResource _resource;
	// ��������� �����.
	private AudioSource _audioSource;
	// ��� �����. ���������� �� ���� �� �������.
	private float _weight = 1;
	// ��������� ���������� _audioSource �� ������ �������� ����� ���-�����. ��� �������� ����� �������������� ��� ���������� ������ � �������� ���������.
	private float _loudness = 1;


	//public OscClip (AudioSource audioSource)
	//{
	//	_audioSource = audioSource;
	//	_initVolume = audioSource.volume;
	//}

	public OscClip (OscAudioResource resource, float loudness = 1, bool loop = true)
	{
		_resource = resource;
		_loudness = loudness;

		_audioSource = GlobalManager.instance.audioSourcesGO.AddComponent<AudioSource> ();
		_audioSource.clip = resource.audioClip;
		_audioSource.loop = loop;
		_audioSource.playOnAwake = false;
	}


	// ==================== PROPERTIES ====================

	public OscAudioResource resource { get { return _resource; } }

	public AudioSource audioSource { get { return _audioSource; } }

	public float weight
	{
		get { return _weight; }
		set
		{
			_weight = value;
			if (_weight < 0)
				_weight = 0;
			if (_weight > 1)
				_weight = 1;

			_audioSource.volume = _loudness * _weight;
		}
	}

	// ��������� ������������ �����. ������� �� ������ ������.
	public float loudness
	{
		get { return _loudness; }
		set
		{
			_loudness = value;
		}
	}

	public bool loop
	{
		get { return _audioSource.loop; }
		set { _audioSource.loop = value; }
	}

}
