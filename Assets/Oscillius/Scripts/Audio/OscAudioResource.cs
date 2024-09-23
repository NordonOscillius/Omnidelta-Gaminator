using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OscAudioResource
{
	private OscAudioResourceEnum _type;
	private string _path;
	private AudioClip _audioClip;


	public OscAudioResource (OscAudioResourceEnum type, string path, AudioClip audioClip)
	{
		_type = type;
		_path = path;
		_audioClip = audioClip;
	}


	// ==================== PROPERTIES ====================

	public OscAudioResourceEnum type { get { return _type; } }

	public string path { get { return _path; } }

	public AudioClip audioClip { get { return _audioClip; } }

}
