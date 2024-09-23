using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �������� ����� - ��������� ����� ����������� ���������.
/// </summary>
public class AudioManager
{
	private GlobalManager _globalManager;
	// ������ ����������. ������ ���� ������ ������ ����� ������, ������ ����������� ������������� ���������������� AudioLayerType.
	private List<AudioLayer> _layers = new List<AudioLayer> ();
	// ������ hardcoded �������������. ����������� ��� �������� ��������������.
	private List<OscAudioResource> _resources = new List<OscAudioResource> ();


	public AudioManager (GlobalManager globalManager)
	{
		_globalManager = globalManager;

		// ������� ���� ��� ���� ����� AudioLayerType.
		Array enumValues = Enum.GetValues (typeof (AudioLayerType));
		int numValues = enumValues.Length;
		for (int i = 0; i < numValues; i++)
		{
			_layers.Add (new AudioLayer ((AudioLayerType) i));
		}

		// �������������� ��� ������������.
		InitResources ();
	}

	private void InitResources ()
	{
		// �������������� ��� ������������. ���������� �������� ������ ����������� � ��� �������, � ������� ����������� �������� ������������ OscAudioResourceType.
		InitResource (OscAudioResourceEnum.Ambience_Forest, "Audio/Ambience/Forest (a) Loop v0-0");
		InitResource (OscAudioResourceEnum.Music_Stars, "Audio/Music/Stars");
		InitResource (OscAudioResourceEnum.Music_Arrival_a, "Audio/Music/Arrival (b)");
		InitResource (OscAudioResourceEnum.Speech_Martin_Is_Missing, "Audio/Speech/E1");
		InitResource (OscAudioResourceEnum.Speech_Sun, "Audio/Speech/F + G v0-1");
		InitResource (OscAudioResourceEnum.Speech_Gear, "Audio/Speech/i-End");
		InitResource (OscAudioResourceEnum.Speech_Sorry_Alex, "Audio/Speech/K1");
		InitResource (OscAudioResourceEnum.Speech_When_Revenge, "Audio/Speech/K2");
		InitResource (OscAudioResourceEnum.Speech_Irony, "Audio/Speech/L to M");
		InitResource (OscAudioResourceEnum.Effects_Door_a, "Audio/Effects/Door (a)");
	}

	private void InitResource (OscAudioResourceEnum type, string path)
	{
		AudioClip audioClip = Resources.Load<AudioClip> (path);
		OscAudioResource resource = new OscAudioResource (type, path, audioClip);
		_resources.Add (resource);
	}

	public void Update (float deltaTime)
	{
		int numLayers = _layers.Count;
		for (int i = 0; i < numLayers; i++)
		{
			_layers[i].Update (deltaTime);
		}
	}

	public AudioLayer GetLayerByType (AudioLayerType layerType)
	{
		return _layers[(int) layerType];
	}

	public OscAudioResource GetResource (OscAudioResourceEnum type)
	{
		return _resources[(int) type];
	}

}
