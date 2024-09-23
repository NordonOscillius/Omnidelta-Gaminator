using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ����, ����������� ������ ��������������. ������ ���� ����������� ����������.
/// </summary>
public class AudioLayer
{
	private AudioLayerType _type;
	private List<OscClip> _clips = new List<OscClip> ();
	// ����, ��������� �������� ����� ������������� �� �������. ��������� ���� ��������� ������ ����� ����������� �� ����. ���� ���� ����������� � null, �������� ����� ��� �����.
	private OscClip _targetClip;
	// ��������, � ������� ����� ����������� ���������� ��������� �������� �����. �������� '2' ������������� �������� � ����������.
	private float _introSpeed = 1f;
	// ��������, � ������� ����� ����������� ��������� ��������� ���� ��������� ������.
	private float _outroSpeed = 1f;


	public AudioLayer (AudioLayerType type)
	{
		_type = type;
	}

	public void Update (float deltaTime)
	{
		int numClips = _clips.Count;
		for (int i = 0; i < numClips; i++)
		{
			OscClip curClip = _clips[i];

			// ��� �������� ����� ��������� � �������.
			if (curClip == _targetClip)
			{
				if (curClip.weight < 1)
				{
					curClip.weight += _introSpeed * deltaTime;
					// �������� �� out of range ������������ � ������� curClip.weight.
				}

				// ���� ���-���� �� ��� �������� (loop == false), ��� ���������� ������� ��� �� ����.
				if (!curClip.audioSource.loop && !curClip.audioSource.isPlaying)
				{
					MonoBehaviour.Destroy (curClip.audioSource);
					_clips.RemoveAt (i);
					i--;
					numClips--;
					_targetClip = null;
				}
			}
			// ���� ���� ��������� ������ ��������� � ����.
			// ��� ��������� ����� (� ������������� ����) ���� ��� ������ ����� ������� � ����������, ������� ����� ����� �������� �� ����������.
			else
			{
				curClip.weight -= _outroSpeed * deltaTime;

				// ���� ��� ����� ���������� ����� ����, ������������� ��� � ������� �� ����.
				if (Mathf.Approximately (curClip.weight, 0))
				{
					curClip.audioSource.Stop ();
					MonoBehaviour.Destroy (curClip.audioSource);

					_clips.RemoveAt (i);
					i--;
					numClips--;
				}
			}
		}
	}

	/// <summary>
	/// ������ � ���� ���� clip: ������ ��� ������� � ��������� �� ����, ���� ����������. ���� ���� ��� ��� �� ����, ��� ��������� � ����� ������� ������ ����� �� ��������� introSpeed, � ��������� ���� ��������� ������ - ����������� �� ��������� outroSpeed.
	/// </summary>
	/// <param name="clipSource"></param>
	//public void IntroduceClipOld (AudioSource clipSource)
	//{
	//	OscClip usedClip = GetClipWithSource (clipSource);
	//	if (usedClip == null)
	//	{
	//		usedClip = new OscClip (clipSource);
	//	}
	//	IntroduceClipPrivate (usedClip);
	//}

	public OscClip IntroduceClip (OscAudioResource resource, float loudness = -1)
	{
		OscClip usedClip = GetClipWithResourceType (resource.type);
		if (usedClip == null)
		{
			usedClip = loudness < 0 ? new OscClip (resource) : new OscClip (resource, loudness);
			usedClip.weight = 0;
			_clips.Add (usedClip);
		}
		else
		{
			// ���� ��������� ���������� ����� ����������, ������� ����� ������ ����� ��. ����� �� ������ ������.
			if (loudness >= 0)
				usedClip.loudness = loudness;
		}
		if (!usedClip.audioSource.isPlaying)
			usedClip.audioSource.Play ();

		_targetClip = usedClip;

		//IntroduceClipPrivate (usedClip);

		return usedClip;
	}

	// loudness < 0 ��� ����������� ��������� �����.
	public OscClip IntroduceClip (OscAudioResourceEnum resourceType, float loudness = -1)
	{
		OscAudioResource resource = GlobalManager.instance.audioManager.GetResource (resourceType);
		return IntroduceClip (resource, loudness);
	}

	public OscClip IntroduceClip (OscAudioResourceEnum resourceType, bool loop, float loudness = -1)
	{
		OscAudioResource resource = GlobalManager.instance.audioManager.GetResource (resourceType);
		OscClip clip = IntroduceClip (resource, loudness);
		clip.loop = loop;
		return clip;
	}

	/// <summary>
	/// ������ � ���� ���� clip: ������ ��� ������� � ��������� �� ����, ���� ����������. ���� ���� ��� ��� �� ����, ��� ��������� � ����� ������� ������ ����� �� ��������� introSpeed, � ��������� ���� ��������� ������ - ����������� �� ��������� outroSpeed.
	/// </summary>
	/// <param name="clip"></param>
	private void IntroduceClipPrivate (OscClip clip)
	{
		int index = _clips.IndexOf (clip);
		// ���� ����� ��� �� ���� �� ����.
		if (index < 0)
		{
			_clips.Add (clip);
			clip.weight = 0;

			if (!clip.audioSource.isPlaying)
				clip.audioSource.Play ();
		}
		// ���� ���� ��� �� ����, ���������, �� ���������� �� ��. ���� ����������, �������������.
		else
		{
			if (!clip.audioSource.isPlaying)
				clip.audioSource.Play ();
		}

		_targetClip = clip;
	}

	/// <summary>
	/// �������� ������ �� ������� ����. ��� ����� � ����� ������� ����� ��������.
	/// </summary>
	public void FadeOut ()
	{
		_targetClip = null;
	}

	// [Deprecated].
	public OscClip GetClipWithSource (AudioSource source)
	{
		int numClips = _clips.Count;
		for (int i = 0; i < numClips; i++)
		{
			OscClip curClip = _clips[i];
			if (curClip.audioSource == source)
			{
				return curClip;
			}
		}

		return null;
	}

	public OscClip GetClipWithResourceType (OscAudioResourceEnum resourceType)
	{
		int numClips = _clips.Count;
		for (int i = 0; i < numClips; i++)
		{
			OscClip curClip = _clips[i];
			if (curClip.resource.type == resourceType)
			{
				return curClip;
			}
		}
		return null;
	}


	// ==================== PROPERTIES ====================

	public AudioLayerType type { get { return _type; } }

	public float introSpeed
	{
		get { return _introSpeed; }
		set { _introSpeed = value; }
	}

	public float outroSpeed
	{
		get { return _outroSpeed; }
		set { _outroSpeed = value; }
	}

	public int numClips { get { return _clips.Count; } }

}